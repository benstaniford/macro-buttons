using System.IO;
using System.Reflection;
using MacroButtons.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MacroButtons.Services;

/// <summary>
/// Service for loading and managing the application configuration.
/// </summary>
public class ConfigurationService
{
    private readonly ProfileService _profileService;

    public ConfigurationService(ProfileService profileService)
    {
        _profileService = profileService;

        // Perform one-time migration if needed
        PerformMigrationIfNeeded();
    }

    /// <summary>
    /// Migrates from old single-config format (~/.macrobuttons.json) to new multi-profile format.
    /// </summary>
    private void PerformMigrationIfNeeded()
    {
        var oldPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".macrobuttons.json");

        if (File.Exists(oldPath))
        {
            try
            {
                // Read old config
                var json = File.ReadAllText(oldPath);
                var config = DeserializeConfiguration(json);

                if (config != null)
                {
                    // Save to new location as "default" profile
                    SaveConfiguration(config, "default");

                    // Set as current profile
                    _profileService.SetCurrentProfileName("default");

                    // Rename old file as backup (don't delete - user might want it)
                    File.Move(oldPath, oldPath + ".backup");
                }
            }
            catch
            {
                // If migration fails, just continue - the app will create a default profile
            }
        }
    }

    /// <summary>
    /// Loads the configuration for a specific profile.
    /// Creates a default configuration if the profile file doesn't exist.
    /// </summary>
    public MacroButtonConfig LoadConfiguration(string profileName)
    {
        var configPath = _profileService.GetProfilePath(profileName);

        if (!File.Exists(configPath))
        {
            CreateDefaultConfiguration(profileName);
        }

        var json = File.ReadAllText(configPath);
        var config = DeserializeConfiguration(json);
        return config ?? CreateFallbackConfiguration(profileName);
    }

    /// <summary>
    /// Saves a configuration to a specific profile.
    /// </summary>
    public void SaveConfiguration(MacroButtonConfig config, string profileName)
    {
        // Update profile name in config
        config.Global.ProfileName = profileName;

        // Serialize to JSON
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);

        // Write to profile file
        var path = _profileService.GetProfilePath(profileName);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Gets the full path to a profile's configuration file.
    /// </summary>
    public string GetConfigPath(string profileName) => _profileService.GetProfilePath(profileName);

    /// <summary>
    /// Creates the default configuration file for a specific profile.
    /// </summary>
    private void CreateDefaultConfiguration(string profileName)
    {
        try
        {
            // Try to load from embedded resource first
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MacroButtons.Resources.DefaultConfig.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var defaultJson = reader.ReadToEnd();

                // Deserialize, update profile name, and save
                var config = DeserializeConfiguration(defaultJson);
                if (config != null)
                {
                    SaveConfiguration(config, profileName);
                }
                else
                {
                    // If deserialization failed, create fallback
                    var fallbackConfig = CreateFallbackConfiguration(profileName);
                    SaveConfiguration(fallbackConfig, profileName);
                }
            }
            else
            {
                // Fallback: create inline default config
                var defaultConfig = CreateFallbackConfiguration(profileName);
                SaveConfiguration(defaultConfig, profileName);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create default configuration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes the configuration JSON with special handling for the Title field.
    /// Title can be either a string or a TitleDefinition object.
    /// </summary>
    private MacroButtonConfig? DeserializeConfiguration(string json)
    {
        try
        {
            var jObject = JObject.Parse(json);
            var config = new MacroButtonConfig();

            // Deserialize theme
            if (jObject["theme"] != null)
            {
                config.Theme = jObject["theme"]!.ToObject<ThemeConfig>() ?? new ThemeConfig();
            }

            // Deserialize global
            if (jObject["global"] != null)
            {
                config.Global = jObject["global"]!.ToObject<GlobalConfig>() ?? new GlobalConfig();
            }

            // Deserialize items with special handling for Title field and recursive Items
            if (jObject["items"] is JArray itemsArray)
            {
                foreach (var item in itemsArray)
                {
                    var buttonItem = DeserializeButtonItem(item);
                    config.Items.Add(buttonItem);
                }
            }

            return config;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse configuration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Recursively deserializes a ButtonItem from a JToken.
    /// Handles polymorphic Title field and nested Items array.
    /// </summary>
    private ButtonItem DeserializeButtonItem(JToken itemToken)
    {
        var buttonItem = new ButtonItem();

        // Handle title (can be string or object)
        var titleToken = itemToken["title"];
        if (titleToken != null)
        {
            if (titleToken.Type == JTokenType.String)
            {
                buttonItem.Title = titleToken.Value<string>();
            }
            else if (titleToken.Type == JTokenType.Object)
            {
                buttonItem.Title = titleToken.ToObject<TitleDefinition>();
            }
        }

        // Handle action (can be null or object)
        var actionToken = itemToken["action"];
        if (actionToken != null && actionToken.Type != JTokenType.Null)
        {
            buttonItem.Action = actionToken.ToObject<ActionDefinition>();
        }

        // Handle nested items (recursive deserialization)
        var nestedItemsToken = itemToken["items"];
        if (nestedItemsToken != null && nestedItemsToken.Type == JTokenType.Array)
        {
            buttonItem.Items = new List<ButtonItem>();
            foreach (var nestedItem in (JArray)nestedItemsToken)
            {
                // RECURSION: Deserialize nested ButtonItems
                var nestedButtonItem = DeserializeButtonItem(nestedItem);
                buttonItem.Items.Add(nestedButtonItem);
            }
        }

        return buttonItem;
    }

    /// <summary>
    /// Creates a fallback configuration when the file can't be loaded.
    /// </summary>
    private MacroButtonConfig CreateFallbackConfiguration(string profileName)
    {
        // Detect the smallest monitor for default configuration
        var monitorService = new MonitorService();
        var smallestMonitorIndex = monitorService.GetSmallestMonitorIndex();

        return new MacroButtonConfig
        {
            Items = new List<ButtonItem>
            {
                new ButtonItem
                {
                    Title = "Paste",
                    Action = new ActionDefinition { Keypress = "^v" }
                },
                new ButtonItem
                {
                    Title = "Save",
                    Action = new ActionDefinition { Keypress = "^s" }
                },
                new ButtonItem
                {
                    Title = "Hello World",
                    Action = null
                },
                new ButtonItem
                {
                    Title = "Calculator",
                    Action = new ActionDefinition { Exe = "calc.exe" }
                }
            },
            Theme = new ThemeConfig
            {
                Foreground = "darkgreen",
                Background = "black"
            },
            Global = new GlobalConfig
            {
                Refresh = "30s",
                MonitorIndex = smallestMonitorIndex,
                ProfileName = profileName,
                SendKeys = new SendKeysConfig
                {
                    Delay = "10ms",
                    Duration = "30ms"
                }
            }
        };
    }
}
