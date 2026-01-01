using MacroButtons.Models;
using Xunit;

namespace MacroButtons.Tests;

/// <summary>
/// Tests for ActionDefinition model.
/// </summary>
public class ActionDefinitionTests
{
    [Fact]
    public void GetActionType_WhenNoActionSet_ReturnsNone()
    {
        var action = new ActionDefinition();

        var result = action.GetActionType();

        Assert.Equal(ActionType.None, result);
    }

    [Fact]
    public void GetActionType_WhenKeypressSet_ReturnsKeypress()
    {
        var action = new ActionDefinition
        {
            Keypress = "^v"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Keypress, result);
    }

    [Fact]
    public void GetActionType_WhenNavigateBackKeypress_ReturnsNavigateBack()
    {
        var action = new ActionDefinition
        {
            Keypress = "__NAVIGATE_BACK__"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.NavigateBack, result);
    }

    [Fact]
    public void GetActionType_WhenPythonSet_ReturnsPython()
    {
        var action = new ActionDefinition
        {
            Python = new List<string> { "-c", "print('hello')" }
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Python, result);
    }

    [Fact]
    public void GetActionType_WhenEmptyPythonList_ReturnsNone()
    {
        var action = new ActionDefinition
        {
            Python = new List<string>()
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.None, result);
    }

    [Fact]
    public void GetActionType_WhenExeSet_ReturnsExecutable()
    {
        var action = new ActionDefinition
        {
            Exe = "notepad.exe"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Executable, result);
    }

    [Fact]
    public void GetActionType_WhenBuiltinSet_ReturnsBuiltin()
    {
        var action = new ActionDefinition
        {
            Builtin = "clock()"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Builtin, result);
    }

    [Fact]
    public void GetActionType_WhenMultipleActionsSet_ReturnsFirstInPriority()
    {
        // Keypress has highest priority
        var action = new ActionDefinition
        {
            Keypress = "^v",
            Python = new List<string> { "-c", "print('test')" },
            Exe = "notepad.exe",
            Builtin = "quit()"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Keypress, result);
    }

    [Fact]
    public void GetActionType_WhenPythonAndExeSet_ReturnsPython()
    {
        // Python has priority over Exe
        var action = new ActionDefinition
        {
            Python = new List<string> { "-c", "print('test')" },
            Exe = "notepad.exe"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Python, result);
    }

    [Fact]
    public void GetActionType_WhenExeAndBuiltinSet_ReturnsExecutable()
    {
        // Exe has priority over Builtin
        var action = new ActionDefinition
        {
            Exe = "notepad.exe",
            Builtin = "quit()"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Executable, result);
    }

    [Fact]
    public void GetActionType_WhenEmptyKeypress_ReturnsNone()
    {
        var action = new ActionDefinition
        {
            Keypress = ""
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.None, result);
    }

    [Fact]
    public void GetActionType_WhenWhitespaceKeypress_ReturnsKeypress()
    {
        // The code doesn't trim whitespace, so "   " is treated as a valid keypress
        var action = new ActionDefinition
        {
            Keypress = "   "
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Keypress, result);
    }

    [Fact]
    public void GetActionType_WhenEmptyExe_ReturnsNone()
    {
        var action = new ActionDefinition
        {
            Exe = ""
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.None, result);
    }

    [Fact]
    public void GetActionType_WhenEmptyBuiltin_ReturnsNone()
    {
        var action = new ActionDefinition
        {
            Builtin = ""
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.None, result);
    }

    [Theory]
    [InlineData("^c")]
    [InlineData("!{F4}")]
    [InlineData("+{Enter}")]
    [InlineData("#r")]
    public void GetActionType_WithValidKeypressSyntax_ReturnsKeypress(string keypress)
    {
        var action = new ActionDefinition
        {
            Keypress = keypress
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Keypress, result);
    }

    [Theory]
    [InlineData("clock()")]
    [InlineData("quit()")]
    [InlineData("custom_command()")]
    public void GetActionType_WithBuiltinCommands_ReturnsBuiltin(string builtin)
    {
        var action = new ActionDefinition
        {
            Builtin = builtin
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Builtin, result);
    }

    // PowerShell Tests

    [Fact]
    public void GetActionType_WhenPowerShellSet_ReturnsPowerShell()
    {
        var action = new ActionDefinition
        {
            PowerShell = "Get-Date -Format 'HH:mm:ss'"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.PowerShell, result);
    }

    [Fact]
    public void GetActionType_WhenEmptyPowerShell_ReturnsNone()
    {
        var action = new ActionDefinition
        {
            PowerShell = ""
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.None, result);
    }

    [Fact]
    public void GetActionType_WhenPowerShellScriptSet_ReturnsPowerShellScript()
    {
        var action = new ActionDefinition
        {
            PowerShellScript = "~/scripts/toggle-mic.ps1"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.PowerShellScript, result);
    }

    [Fact]
    public void GetActionType_WhenEmptyPowerShellScript_ReturnsNone()
    {
        var action = new ActionDefinition
        {
            PowerShellScript = ""
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.None, result);
    }

    [Fact]
    public void GetActionType_PowerShellPriorityOverBuiltin()
    {
        // PowerShell has priority over Builtin
        var action = new ActionDefinition
        {
            PowerShell = "Get-Date",
            Builtin = "clock()"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.PowerShell, result);
    }

    [Fact]
    public void GetActionType_PowerShellScriptPriorityOverBuiltin()
    {
        // PowerShellScript has priority over Builtin
        var action = new ActionDefinition
        {
            PowerShellScript = "~/script.ps1",
            Builtin = "quit()"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.PowerShellScript, result);
    }

    [Fact]
    public void GetActionType_ExePriorityOverPowerShell()
    {
        // Exe has priority over PowerShell
        var action = new ActionDefinition
        {
            Exe = "notepad.exe",
            PowerShell = "Get-Date"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Executable, result);
    }

    [Fact]
    public void GetActionType_ExePriorityOverPowerShellScript()
    {
        // Exe has priority over PowerShellScript
        var action = new ActionDefinition
        {
            Exe = "calc.exe",
            PowerShellScript = "~/script.ps1"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Executable, result);
    }

    [Fact]
    public void GetActionType_PythonPriorityOverPowerShell()
    {
        // Python has priority over PowerShell
        var action = new ActionDefinition
        {
            Python = new List<string> { "-c", "print('test')" },
            PowerShell = "Get-Date"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Python, result);
    }

    [Fact]
    public void GetActionType_FullPriorityChain_ReturnsKeypress()
    {
        // Test full priority chain: Keypress > Python > Exe > PowerShell > PowerShellScript > Builtin
        var action = new ActionDefinition
        {
            Keypress = "^v",
            Python = new List<string> { "-c", "print('test')" },
            Exe = "notepad.exe",
            PowerShell = "Get-Date",
            PowerShellScript = "~/script.ps1",
            Builtin = "quit()"
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.Keypress, result);
    }

    [Fact]
    public void PowerShellParameters_CanBeSet()
    {
        var parameters = new Dictionary<string, object>
        {
            { "DeviceIndex", 0 },
            { "Mute", true }
        };

        var action = new ActionDefinition
        {
            PowerShellScript = "~/scripts/toggle-mic.ps1",
            PowerShellParameters = parameters
        };

        Assert.NotNull(action.PowerShellParameters);
        Assert.Equal(2, action.PowerShellParameters.Count);
        Assert.Equal(0, action.PowerShellParameters["DeviceIndex"]);
        Assert.Equal(true, action.PowerShellParameters["Mute"]);
    }

    [Fact]
    public void PowerShellParameters_CanBeNull()
    {
        var action = new ActionDefinition
        {
            PowerShell = "Get-Process",
            PowerShellParameters = null
        };

        Assert.Null(action.PowerShellParameters);
    }

    [Theory]
    [InlineData("Get-Date")]
    [InlineData("Get-Process | Select-Object -First 5")]
    [InlineData("Write-Output 'Hello, World!'")]
    public void GetActionType_WithValidPowerShellCommands_ReturnsPowerShell(string command)
    {
        var action = new ActionDefinition
        {
            PowerShell = command
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.PowerShell, result);
    }

    [Theory]
    [InlineData("~/scripts/test.ps1")]
    [InlineData("C:\\Scripts\\MyScript.ps1")]
    [InlineData("%USERPROFILE%\\scripts\\script.ps1")]
    public void GetActionType_WithValidPowerShellScriptPaths_ReturnsPowerShellScript(string scriptPath)
    {
        var action = new ActionDefinition
        {
            PowerShellScript = scriptPath
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.PowerShellScript, result);
    }
}
