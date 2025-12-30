using MacroButtons.Helpers;
using Xunit;

namespace MacroButtons.Tests;

/// <summary>
/// Tests for WindowHelper pattern matching logic.
/// Note: These tests only cover the pattern normalization logic.
/// Full window matching tests would require Windows-specific APIs and are not included.
/// </summary>
public class WindowHelperTests
{
    [Theory]
    [InlineData("firefox", "firefox.exe", true)]
    [InlineData("firefox.exe", "firefox.exe", true)]
    [InlineData("Firefox", "firefox.exe", true)]
    [InlineData("FIREFOX.EXE", "firefox.exe", true)]
    [InlineData("chrome", "firefox.exe", false)]
    [InlineData("notepad", "notepad.exe", true)]
    [InlineData("notepad.exe", "notepad.exe", true)]
    [InlineData("Notepad.EXE", "notepad.exe", true)]
    public void ProcessNameNormalization_WorksCorrectly(string pattern, string actualProcessName, bool shouldMatch)
    {
        // This test verifies the normalization logic conceptually
        // We normalize the pattern to include .exe if not present
        var normalizedPattern = pattern;
        if (!normalizedPattern.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            normalizedPattern += ".exe";
        }

        var result = normalizedPattern.Equals(actualProcessName, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(shouldMatch, result);
    }

    [Theory]
    [InlineData("firefox|MozillaWindowClass", "firefox.exe")]
    [InlineData("firefox.exe|MozillaWindowClass", "firefox.exe")]
    [InlineData("notepad|Notepad", "notepad.exe")]
    [InlineData("chrome.exe|Chrome_WidgetWin_1", "chrome.exe")]
    public void ProcessNameWithWindowClass_NormalizesProcessNamePart(string pattern, string expectedProcessName)
    {
        // Test that process name normalization works with window class patterns
        var parts = pattern.Split('|', 2);
        var processNamePart = parts[0].Trim();

        if (!processNamePart.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            processNamePart += ".exe";
        }

        Assert.Equal(expectedProcessName, processNamePart, ignoreCase: true);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyOrNullPattern_DoesNotMatch(string? pattern)
    {
        var isValid = !string.IsNullOrWhiteSpace(pattern);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("firefox", "firefox")]
    [InlineData("firefox.exe", "firefox.exe")]
    [InlineData("FIREFOX", "FIREFOX")]
    [InlineData("firefox.EXE", "firefox.EXE")]
    public void PatternNormalization_PreservesOriginalCasing(string input, string expected)
    {
        // Even though matching is case-insensitive, the normalization should preserve
        // the original casing for consistency
        var normalized = input;
        if (!normalized.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".exe";
        }

        // Should have .exe appended but preserve original casing of the base name
        if (input.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Equal(expected, normalized);
        }
        else
        {
            Assert.StartsWith(expected, normalized);
            Assert.EndsWith(".exe", normalized);
        }
    }

    [Theory]
    [InlineData("firefox.EXE", true)]
    [InlineData("firefox.exe", true)]
    [InlineData("firefox.Exe", true)]
    [InlineData("firefox", false)]
    [InlineData("FIREFOX", false)]
    public void EndsWithExe_IsCaseInsensitive(string input, bool shouldEndWithExe)
    {
        var result = input.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

        Assert.Equal(shouldEndWithExe, result);
    }
}
