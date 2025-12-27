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
    private readonly string _configPath;

    public ConfigurationService()
    {
        _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".macrobuttons.json");
    }

    /// <summary>
    /// Loads the configuration from the user's profile directory.
    /// Creates a default configuration if the file doesn't exist.
    /// </summary>
    public MacroButtonConfig LoadConfiguration()
    {
        if (!File.Exists(_configPath))
        {
            CreateDefaultConfiguration();
        }

        var json = File.ReadAllText(_configPath);
        var config = DeserializeConfiguration(json);
        return config ?? CreateFallbackConfiguration();
    }

    /// <summary>
    /// Creates the default configuration file in the user's profile directory.
    /// </summary>
    private void CreateDefaultConfiguration()
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
                File.WriteAllText(_configPath, defaultJson);
            }
            else
            {
                // Fallback: create inline default config
                var defaultConfig = CreateFallbackConfiguration();
                var json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
                File.WriteAllText(_configPath, json);
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

            // Deserialize items with special handling for Title field
            if (jObject["items"] is JArray itemsArray)
            {
                foreach (var item in itemsArray)
                {
                    var buttonItem = new ButtonItem();

                    // Handle title (can be string or object)
                    var titleToken = item["title"];
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
                    var actionToken = item["action"];
                    if (actionToken != null && actionToken.Type != JTokenType.Null)
                    {
                        buttonItem.Action = actionToken.ToObject<ActionDefinition>();
                    }

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
    /// Creates a fallback configuration when the file can't be loaded.
    /// </summary>
    private MacroButtonConfig CreateFallbackConfiguration()
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
                SendKeys = new SendKeysConfig
                {
                    Delay = "10ms",
                    Duration = "30ms"
                }
            }
        };
    }
}
