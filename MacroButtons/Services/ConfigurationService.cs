using System.IO;
using System.Reflection;
using MacroButtons.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MacroButtons.Services;

/// <summary>
/// Service for loading and managing the application configuration.
/// </summary>
public class ConfigurationService
{
    private readonly ProfileService _profileService;

    // Case-insensitive JSON settings for deserialization
    private static readonly JsonSerializerSettings CaseInsensitiveSettings = new JsonSerializerSettings
    {
        ContractResolver = new CaseInsensitiveContractResolver(),
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        Formatting = Formatting.Indented
    };

    // Custom contract resolver for case-insensitive property matching
    private class CaseInsensitiveContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLowerInvariant();
        }
    }

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

        // Serialize to JSON with clean, minimal output
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,  // Skip null properties
            DefaultValueHandling = DefaultValueHandling.Ignore,  // Skip default values
            ContractResolver = new CleanJsonContractResolver()  // Skip computed properties
        };

        var json = JsonConvert.SerializeObject(config, settings);

        // Write to profile file
        var path = _profileService.GetProfilePath(profileName);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Custom contract resolver that skips computed (read-only) properties.
    /// </summary>
    private class CleanJsonContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            // Skip read-only properties (computed properties)
            if (property.Writable == false)
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }
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
            // Detect the smallest monitor for default configuration
            var monitorService = new MonitorService();
            var smallestMonitorIndex = monitorService.GetSmallestMonitorIndex();

            // Try to load from embedded resource first
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MacroButtons.Resources.DefaultConfig.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var defaultJson = reader.ReadToEnd();

                // Deserialize, update profile name and monitor index, and save
                var config = DeserializeConfiguration(defaultJson);
                if (config != null)
                {
                    config.Global.MonitorIndex = smallestMonitorIndex;
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

            // Deserialize theme (case-insensitive)
            var themeToken = jObject.Properties()
                .FirstOrDefault(p => p.Name.Equals("theme", StringComparison.OrdinalIgnoreCase))?.Value;
            if (themeToken != null)
            {
                config.Theme = themeToken.ToObject<ThemeConfig>(JsonSerializer.Create(CaseInsensitiveSettings))
                    ?? new ThemeConfig();
            }

            // Deserialize global (case-insensitive)
            var globalToken = jObject.Properties()
                .FirstOrDefault(p => p.Name.Equals("global", StringComparison.OrdinalIgnoreCase))?.Value;
            if (globalToken != null)
            {
                config.Global = globalToken.ToObject<GlobalConfig>(JsonSerializer.Create(CaseInsensitiveSettings))
                    ?? new GlobalConfig();
            }

            // Deserialize items (case-insensitive) with special handling for Title field and recursive Items
            var itemsToken = jObject.Properties()
                .FirstOrDefault(p => p.Name.Equals("items", StringComparison.OrdinalIgnoreCase))?.Value;
            if (itemsToken is JArray itemsArray)
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
        var serializer = JsonSerializer.Create(CaseInsensitiveSettings);

        // Handle title (can be string or object) - case-insensitive
        var titleToken = (itemToken as JObject)?.Properties()
            .FirstOrDefault(p => p.Name.Equals("title", StringComparison.OrdinalIgnoreCase))?.Value;
        if (titleToken != null)
        {
            if (titleToken.Type == JTokenType.String)
            {
                buttonItem.Title = titleToken.Value<string>();
            }
            else if (titleToken.Type == JTokenType.Object)
            {
                buttonItem.Title = titleToken.ToObject<TitleDefinition>(serializer);
            }
        }

        // Handle action (can be null or object) - case-insensitive
        var actionToken = (itemToken as JObject)?.Properties()
            .FirstOrDefault(p => p.Name.Equals("action", StringComparison.OrdinalIgnoreCase))?.Value;
        if (actionToken != null && actionToken.Type != JTokenType.Null)
        {
            buttonItem.Action = actionToken.ToObject<ActionDefinition>(serializer);
        }

        // Handle nested items (recursive deserialization) - case-insensitive
        var nestedItemsToken = (itemToken as JObject)?.Properties()
            .FirstOrDefault(p => p.Name.Equals("items", StringComparison.OrdinalIgnoreCase))?.Value;
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
                    Action = new ActionDefinition { Exe = "%SystemRoot%/system32/calc.exe" }
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
