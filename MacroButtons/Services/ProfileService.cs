using System.IO;
using System.Text.RegularExpressions;
using MacroButtons.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MacroButtons.Services;

/// <summary>
/// Service for managing configuration profiles.
/// </summary>
public class ProfileService
{
    private readonly string _profileDirectory;
    private readonly string _currentProfileMarkerFile;

    public ProfileService()
    {
        _profileDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".macrobuttons");
        _currentProfileMarkerFile = Path.Combine(_profileDirectory, ".current");

        EnsureProfileDirectoryExists();
    }

    /// <summary>
    /// Ensures the profile directory exists.
    /// </summary>
    private void EnsureProfileDirectoryExists()
    {
        if (!Directory.Exists(_profileDirectory))
        {
            Directory.CreateDirectory(_profileDirectory);
        }
    }

    /// <summary>
    /// Gets the profile directory path.
    /// </summary>
    public string GetProfileDirectory() => _profileDirectory;

    /// <summary>
    /// Gets the full path for a specific profile.
    /// </summary>
    public string GetProfilePath(string profileName)
    {
        return Path.Combine(_profileDirectory, $"{profileName}.json");
    }

    /// <summary>
    /// Lists all available profiles by scanning for *.json files in the profile directory.
    /// </summary>
    public List<string> ListProfiles()
    {
        EnsureProfileDirectoryExists();

        var jsonFiles = Directory.GetFiles(_profileDirectory, "*.json");
        var profiles = jsonFiles
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrEmpty(name))
            .OrderBy(name => name)
            .ToList();

        return profiles;
    }

    /// <summary>
    /// Checks if a profile exists.
    /// </summary>
    public bool ProfileExists(string profileName)
    {
        var profilePath = GetProfilePath(profileName);
        return File.Exists(profilePath);
    }

    /// <summary>
    /// Gets the current profile name from the .current marker file.
    /// Returns "default" if the file doesn't exist or is invalid.
    /// </summary>
    public string GetCurrentProfileName()
    {
        try
        {
            if (File.Exists(_currentProfileMarkerFile))
            {
                var currentProfile = File.ReadAllText(_currentProfileMarkerFile).Trim();
                if (!string.IsNullOrWhiteSpace(currentProfile) && ProfileExists(currentProfile))
                {
                    return currentProfile;
                }
            }
        }
        catch
        {
            // Ignore errors, fall back to default
        }

        // Fall back to "default" profile
        return "default";
    }

    /// <summary>
    /// Sets the current profile name by writing to the .current marker file.
    /// </summary>
    public void SetCurrentProfileName(string profileName)
    {
        EnsureProfileDirectoryExists();
        File.WriteAllText(_currentProfileMarkerFile, profileName);
    }

    /// <summary>
    /// Creates a new profile.
    /// </summary>
    /// <param name="profileName">Name of the profile to create</param>
    /// <param name="sourceProfileName">Optional source profile name to copy from. If null, uses default config.</param>
    public void CreateProfile(string profileName, string? sourceProfileName = null)
    {
        var profilePath = GetProfilePath(profileName);

        if (File.Exists(profilePath))
        {
            throw new InvalidOperationException($"Profile '{profileName}' already exists.");
        }

        if (!string.IsNullOrEmpty(sourceProfileName))
        {
            // Copy from existing profile file
            var sourcePath = GetProfilePath(sourceProfileName);
            if (!File.Exists(sourcePath))
            {
                throw new InvalidOperationException($"Source profile '{sourceProfileName}' does not exist.");
            }

            // Read the source file
            var json = File.ReadAllText(sourcePath);

            // Parse and update the profile name
            var jObject = JObject.Parse(json);

            // Find and update the profilename field (case-insensitive)
            var globalToken = jObject.Properties()
                .FirstOrDefault(p => p.Name.Equals("global", StringComparison.OrdinalIgnoreCase))?.Value as JObject;

            if (globalToken != null)
            {
                var profileNameProp = globalToken.Properties()
                    .FirstOrDefault(p => p.Name.Equals("profilename", StringComparison.OrdinalIgnoreCase));

                if (profileNameProp != null)
                {
                    profileNameProp.Value = profileName;
                }
                else
                {
                    // Add profileName if it doesn't exist
                    globalToken["profileName"] = profileName;
                }
            }

            // Write to new file (preserves formatting from source)
            File.WriteAllText(profilePath, jObject.ToString(Formatting.Indented));
        }
        else
        {
            // Create default profile - ConfigurationService will handle this
            // Just create an empty marker file that will be populated on first load
            File.WriteAllText(profilePath, "{}");
        }
    }

    /// <summary>
    /// Renames a profile by renaming the file and updating the ProfileName field in the JSON.
    /// </summary>
    public void RenameProfile(string oldName, string newName)
    {
        if (oldName == newName)
        {
            return; // No change needed
        }

        var oldPath = GetProfilePath(oldName);
        var newPath = GetProfilePath(newName);

        if (!File.Exists(oldPath))
        {
            throw new InvalidOperationException($"Profile '{oldName}' does not exist.");
        }

        if (File.Exists(newPath))
        {
            throw new InvalidOperationException($"Profile '{newName}' already exists.");
        }

        // Read the file
        var json = File.ReadAllText(oldPath);

        // Parse and update the profile name
        var jObject = JObject.Parse(json);

        // Find and update the profilename field (case-insensitive)
        var globalToken = jObject.Properties()
            .FirstOrDefault(p => p.Name.Equals("global", StringComparison.OrdinalIgnoreCase))?.Value as JObject;

        if (globalToken != null)
        {
            var profileNameProp = globalToken.Properties()
                .FirstOrDefault(p => p.Name.Equals("profilename", StringComparison.OrdinalIgnoreCase));

            if (profileNameProp != null)
            {
                profileNameProp.Value = newName;
            }
            else
            {
                // Add profileName if it doesn't exist
                globalToken["profileName"] = newName;
            }
        }

        // Write to new file (preserves formatting from source)
        File.WriteAllText(newPath, jObject.ToString(Formatting.Indented));

        // Delete old file
        File.Delete(oldPath);

        // Update .current file if this was the current profile
        var currentProfile = GetCurrentProfileName();
        if (currentProfile == oldName)
        {
            SetCurrentProfileName(newName);
        }
    }

    /// <summary>
    /// Deletes a profile.
    /// </summary>
    public void DeleteProfile(string profileName)
    {
        var profilePath = GetProfilePath(profileName);

        if (!File.Exists(profilePath))
        {
            throw new InvalidOperationException($"Profile '{profileName}' does not exist.");
        }

        // Prevent deleting the last profile
        var profiles = ListProfiles();
        if (profiles.Count <= 1)
        {
            throw new InvalidOperationException("Cannot delete the last profile.");
        }

        // Delete the file
        File.Delete(profilePath);

        // If this was the current profile, switch to another one
        var currentProfile = GetCurrentProfileName();
        if (currentProfile == profileName)
        {
            var nextProfile = profiles.FirstOrDefault(p => p != profileName);
            if (nextProfile != null)
            {
                SetCurrentProfileName(nextProfile);
            }
        }
    }

    /// <summary>
    /// Validates a profile name.
    /// </summary>
    /// <param name="profileName">Profile name to validate</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool ValidateProfileName(string profileName, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(profileName))
        {
            errorMessage = "Profile name cannot be empty.";
            return false;
        }

        if (profileName.Length > 100)
        {
            errorMessage = "Profile name cannot exceed 100 characters.";
            return false;
        }

        // Check for reserved names
        if (profileName == "." || profileName == "..")
        {
            errorMessage = "Profile name cannot be '.' or '..'.";
            return false;
        }

        // Check for illegal file system characters
        var illegalChars = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
        if (profileName.IndexOfAny(illegalChars) >= 0)
        {
            errorMessage = $"Profile name cannot contain the following characters: \\ / : * ? \" < > |";
            return false;
        }

        return true;
    }
}
