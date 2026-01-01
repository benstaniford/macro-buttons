using MacroButtons.Models;
using Xunit;

namespace MacroButtons.Tests;

/// <summary>
/// Tests for TitleDefinition model.
/// </summary>
public class TitleDefinitionTests
{
    [Fact]
    public void IsPython_WhenPythonIsSet_ReturnsTrue()
    {
        var title = new TitleDefinition
        {
            Python = new List<string> { "-c", "print('hello')" }
        };

        Assert.True(title.IsPython);
        Assert.False(title.IsExecutable);
        Assert.False(title.IsBuiltin);
    }

    [Fact]
    public void IsPython_WhenPythonIsEmpty_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            Python = new List<string>()
        };

        Assert.False(title.IsPython);
    }

    [Fact]
    public void IsPython_WhenPythonIsNull_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            Python = null
        };

        Assert.False(title.IsPython);
    }

    [Fact]
    public void IsExecutable_WhenExeIsSet_ReturnsTrue()
    {
        var title = new TitleDefinition
        {
            Exe = new List<string> { "notepad.exe" }
        };

        Assert.True(title.IsExecutable);
        Assert.False(title.IsPython);
        Assert.False(title.IsBuiltin);
    }

    [Fact]
    public void IsExecutable_WhenExeIsEmpty_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            Exe = new List<string>()
        };

        Assert.False(title.IsExecutable);
    }

    [Fact]
    public void IsExecutable_WhenExeIsNull_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            Exe = null
        };

        Assert.False(title.IsExecutable);
    }

    [Fact]
    public void IsBuiltin_WhenBuiltinIsSet_ReturnsTrue()
    {
        var title = new TitleDefinition
        {
            Builtin = "clock()"
        };

        Assert.True(title.IsBuiltin);
        Assert.False(title.IsPython);
        Assert.False(title.IsExecutable);
    }

    [Fact]
    public void IsBuiltin_WhenBuiltinIsEmpty_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            Builtin = ""
        };

        Assert.False(title.IsBuiltin);
    }

    [Fact]
    public void IsBuiltin_WhenBuiltinIsNull_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            Builtin = null
        };

        Assert.False(title.IsBuiltin);
    }

    [Theory]
    [InlineData("100ms", 100)]
    [InlineData("500ms", 500)]
    [InlineData("1000ms", 1000)]
    public void GetRefreshInterval_WithMilliseconds_ReturnsCorrectTimeSpan(string refresh, int expectedMs)
    {
        var title = new TitleDefinition
        {
            Refresh = refresh
        };

        var result = title.GetRefreshInterval();

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromMilliseconds(expectedMs), result.Value);
    }

    [Theory]
    [InlineData("1s", 1)]
    [InlineData("5s", 5)]
    [InlineData("30s", 30)]
    public void GetRefreshInterval_WithSeconds_ReturnsCorrectTimeSpan(string refresh, int expectedSeconds)
    {
        var title = new TitleDefinition
        {
            Refresh = refresh
        };

        var result = title.GetRefreshInterval();

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result.Value);
    }

    [Theory]
    [InlineData("1m", 1)]
    [InlineData("5m", 5)]
    public void GetRefreshInterval_WithMinutes_ReturnsCorrectTimeSpan(string refresh, int expectedMinutes)
    {
        var title = new TitleDefinition
        {
            Refresh = refresh
        };

        var result = title.GetRefreshInterval();

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromMinutes(expectedMinutes), result.Value);
    }

    [Theory]
    [InlineData("1h", 1)]
    [InlineData("2h", 2)]
    public void GetRefreshInterval_WithHours_ReturnsCorrectTimeSpan(string refresh, int expectedHours)
    {
        var title = new TitleDefinition
        {
            Refresh = refresh
        };

        var result = title.GetRefreshInterval();

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromHours(expectedHours), result.Value);
    }

    [Fact]
    public void GetRefreshInterval_WhenRefreshIsNull_ReturnsNull()
    {
        var title = new TitleDefinition
        {
            Refresh = null
        };

        var result = title.GetRefreshInterval();

        Assert.Null(result);
    }

    [Fact]
    public void GetRefreshInterval_WhenRefreshIsEmpty_ReturnsNull()
    {
        var title = new TitleDefinition
        {
            Refresh = ""
        };

        var result = title.GetRefreshInterval();

        Assert.Null(result);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("100")]
    [InlineData("abc")]
    [InlineData("100 ms")]
    [InlineData("ms100")]
    public void GetRefreshInterval_WithInvalidFormat_ReturnsNull(string refresh)
    {
        var title = new TitleDefinition
        {
            Refresh = refresh
        };

        var result = title.GetRefreshInterval();

        Assert.Null(result);
    }

    [Theory]
    [InlineData("1ms")]
    [InlineData("50ms")]
    [InlineData("99ms")]
    public void GetRefreshInterval_BelowMinimum_Returns100ms(string refresh)
    {
        var title = new TitleDefinition
        {
            Refresh = refresh
        };

        var result = title.GetRefreshInterval();

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromMilliseconds(100), result.Value);
    }

    [Fact]
    public void GetRefreshInterval_Exactly100ms_Returns100ms()
    {
        var title = new TitleDefinition
        {
            Refresh = "100ms"
        };

        var result = title.GetRefreshInterval();

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromMilliseconds(100), result.Value);
    }

    [Fact]
    public void GetRefreshInterval_Above100ms_ReturnsActualValue()
    {
        var title = new TitleDefinition
        {
            Refresh = "101ms"
        };

        var result = title.GetRefreshInterval();

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromMilliseconds(101), result.Value);
    }

    [Fact]
    public void Theme_CanBeSet()
    {
        var title = new TitleDefinition
        {
            Theme = "prominent"
        };

        Assert.Equal("prominent", title.Theme);
    }

    [Fact]
    public void Theme_CanBeNull()
    {
        var title = new TitleDefinition
        {
            Theme = null
        };

        Assert.Null(title.Theme);
    }

    [Theory]
    [InlineData("default")]
    [InlineData("toggled")]
    [InlineData("prominent")]
    public void Theme_AcceptsVariousThemes(string theme)
    {
        var title = new TitleDefinition
        {
            Theme = theme
        };

        Assert.Equal(theme, title.Theme);
    }

    [Fact]
    public void TitleDefinition_FullyConfigured_AllPropertiesSet()
    {
        var title = new TitleDefinition
        {
            Python = new List<string> { "-c", "print('test')" },
            Refresh = "1s",
            Theme = "prominent"
        };

        Assert.True(title.IsPython);
        Assert.Equal(TimeSpan.FromSeconds(1), title.GetRefreshInterval());
        Assert.Equal("prominent", title.Theme);
    }

    [Fact]
    public void TitleDefinition_MultipleTypesSet_OnlyFirstIsTrue()
    {
        var title = new TitleDefinition
        {
            Python = new List<string> { "-c", "print('test')" },
            Exe = new List<string> { "notepad.exe" },
            Builtin = "clock()"
        };

        // All can be true if all are set (not mutually exclusive in the model)
        Assert.True(title.IsPython);
        Assert.True(title.IsExecutable);
        Assert.True(title.IsBuiltin);
    }

    [Fact]
    public void GetRefreshInterval_WithWhitespace_ReturnsNull()
    {
        var title = new TitleDefinition
        {
            Refresh = "   "
        };

        var result = title.GetRefreshInterval();

        Assert.Null(result);
    }

    [Fact]
    public void GetRefreshInterval_ZeroValue_EnforcesMinimum()
    {
        var title = new TitleDefinition
        {
            Refresh = "0ms"
        };

        var result = title.GetRefreshInterval();

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromMilliseconds(100), result.Value);
    }

    // PowerShell Tests

    [Fact]
    public void IsPowerShell_WhenPowerShellIsSet_ReturnsTrue()
    {
        var title = new TitleDefinition
        {
            PowerShell = "Get-Date -Format 'HH:mm:ss'"
        };

        Assert.True(title.IsPowerShell);
        Assert.False(title.IsPowerShellScript);
        Assert.False(title.IsPython);
        Assert.False(title.IsExecutable);
        Assert.False(title.IsBuiltin);
    }

    [Fact]
    public void IsPowerShell_WhenPowerShellIsEmpty_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            PowerShell = ""
        };

        Assert.False(title.IsPowerShell);
    }

    [Fact]
    public void IsPowerShell_WhenPowerShellIsNull_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            PowerShell = null
        };

        Assert.False(title.IsPowerShell);
    }

    [Fact]
    public void IsPowerShellScript_WhenPowerShellScriptIsSet_ReturnsTrue()
    {
        var title = new TitleDefinition
        {
            PowerShellScript = "~/scripts/get-status.ps1"
        };

        Assert.True(title.IsPowerShellScript);
        Assert.False(title.IsPowerShell);
        Assert.False(title.IsPython);
        Assert.False(title.IsExecutable);
        Assert.False(title.IsBuiltin);
    }

    [Fact]
    public void IsPowerShellScript_WhenPowerShellScriptIsEmpty_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            PowerShellScript = ""
        };

        Assert.False(title.IsPowerShellScript);
    }

    [Fact]
    public void IsPowerShellScript_WhenPowerShellScriptIsNull_ReturnsFalse()
    {
        var title = new TitleDefinition
        {
            PowerShellScript = null
        };

        Assert.False(title.IsPowerShellScript);
    }

    [Fact]
    public void PowerShellParameters_CanBeSet()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Name", "Test" },
            { "Count", 5 },
            { "Enabled", true }
        };

        var title = new TitleDefinition
        {
            PowerShellScript = "~/scripts/test.ps1",
            PowerShellParameters = parameters
        };

        Assert.NotNull(title.PowerShellParameters);
        Assert.Equal(3, title.PowerShellParameters.Count);
        Assert.Equal("Test", title.PowerShellParameters["Name"]);
        Assert.Equal(5, title.PowerShellParameters["Count"]);
        Assert.Equal(true, title.PowerShellParameters["Enabled"]);
    }

    [Fact]
    public void PowerShellParameters_CanBeNull()
    {
        var title = new TitleDefinition
        {
            PowerShell = "Get-Date",
            PowerShellParameters = null
        };

        Assert.Null(title.PowerShellParameters);
    }

    [Fact]
    public void TitleDefinition_PowerShellWithParameters_AllPropertiesSet()
    {
        var parameters = new Dictionary<string, object> { { "Verbose", true } };

        var title = new TitleDefinition
        {
            PowerShell = "Get-Process",
            PowerShellParameters = parameters,
            Refresh = "2s",
            Theme = "prominent"
        };

        Assert.True(title.IsPowerShell);
        Assert.Equal(parameters, title.PowerShellParameters);
        Assert.Equal(TimeSpan.FromSeconds(2), title.GetRefreshInterval());
        Assert.Equal("prominent", title.Theme);
    }

    [Fact]
    public void TitleDefinition_PowerShellScriptWithParameters_AllPropertiesSet()
    {
        var parameters = new Dictionary<string, object>
        {
            { "DeviceIndex", 0 },
            { "Mute", false }
        };

        var title = new TitleDefinition
        {
            PowerShellScript = "~/scripts/microphone.ps1",
            PowerShellParameters = parameters,
            Refresh = "500ms"
        };

        Assert.True(title.IsPowerShellScript);
        Assert.Equal(parameters, title.PowerShellParameters);
        Assert.Equal(TimeSpan.FromMilliseconds(500), title.GetRefreshInterval());
    }

    [Fact]
    public void TitleDefinition_BothPowerShellTypesSet_BothAreTrue()
    {
        var title = new TitleDefinition
        {
            PowerShell = "Get-Date",
            PowerShellScript = "~/scripts/test.ps1"
        };

        // Both can be true if both are set (not mutually exclusive in the model)
        Assert.True(title.IsPowerShell);
        Assert.True(title.IsPowerShellScript);
    }
}
