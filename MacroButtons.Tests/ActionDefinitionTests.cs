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
    public void GetActionType_WhenWhitespaceKeypress_ReturnsNone()
    {
        var action = new ActionDefinition
        {
            Keypress = "   "
        };

        var result = action.GetActionType();

        Assert.Equal(ActionType.None, result);
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
}
