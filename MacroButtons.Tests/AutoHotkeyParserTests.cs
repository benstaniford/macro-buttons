using MacroButtons.Helpers;
using WindowsInput.Native;
using Xunit;

namespace MacroButtons.Tests;

/// <summary>
/// Tests for AutoHotkey syntax parser.
/// </summary>
public class AutoHotkeyParserTests
{
    private readonly AutoHotkeyParser _parser;

    public AutoHotkeyParserTests()
    {
        _parser = new AutoHotkeyParser();
    }

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyList()
    {
        var result = _parser.Parse("");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_NullString_ReturnsEmptyList()
    {
        var result = _parser.Parse(null!);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("a", VirtualKeyCode.VK_A)]
    [InlineData("b", VirtualKeyCode.VK_B)]
    [InlineData("z", VirtualKeyCode.VK_Z)]
    [InlineData("A", VirtualKeyCode.VK_A)]
    [InlineData("Z", VirtualKeyCode.VK_Z)]
    public void Parse_SingleLetter_ReturnsCorrectKeyPress(string input, VirtualKeyCode expectedKey)
    {
        var result = _parser.Parse(input);

        Assert.Single(result);
        Assert.Equal(KeyActionType.KeyPress, result[0].Type);
        Assert.Equal(expectedKey, result[0].VirtualKeyCode);
    }

    [Theory]
    [InlineData("0", VirtualKeyCode.VK_0)]
    [InlineData("1", VirtualKeyCode.VK_1)]
    [InlineData("9", VirtualKeyCode.VK_9)]
    public void Parse_SingleDigit_ReturnsCorrectKeyPress(string input, VirtualKeyCode expectedKey)
    {
        var result = _parser.Parse(input);

        Assert.Single(result);
        Assert.Equal(KeyActionType.KeyPress, result[0].Type);
        Assert.Equal(expectedKey, result[0].VirtualKeyCode);
    }

    [Fact]
    public void Parse_CtrlModifier_ReturnsCorrectSequence()
    {
        var result = _parser.Parse("^v");

        Assert.Equal(3, result.Count);
        Assert.Equal(KeyActionType.ModifierDown, result[0].Type);
        Assert.Equal(VirtualKeyCode.CONTROL, result[0].VirtualKeyCode);
        Assert.Equal(KeyActionType.KeyPress, result[1].Type);
        Assert.Equal(VirtualKeyCode.VK_V, result[1].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierUp, result[2].Type);
        Assert.Equal(VirtualKeyCode.CONTROL, result[2].VirtualKeyCode);
    }

    [Fact]
    public void Parse_ShiftModifier_ReturnsCorrectSequence()
    {
        var result = _parser.Parse("+a");

        Assert.Equal(3, result.Count);
        Assert.Equal(KeyActionType.ModifierDown, result[0].Type);
        Assert.Equal(VirtualKeyCode.SHIFT, result[0].VirtualKeyCode);
        Assert.Equal(KeyActionType.KeyPress, result[1].Type);
        Assert.Equal(VirtualKeyCode.VK_A, result[1].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierUp, result[2].Type);
        Assert.Equal(VirtualKeyCode.SHIFT, result[2].VirtualKeyCode);
    }

    [Fact]
    public void Parse_AltModifier_ReturnsCorrectSequence()
    {
        var result = _parser.Parse("!f4");

        Assert.Equal(5, result.Count); // Alt down, F press, F release, 4 press, Alt up
        // Note: The parser processes each character separately
        // So "!f4" is: Alt+F, then 4 (not Alt+F4 together)
        Assert.Equal(KeyActionType.ModifierDown, result[0].Type);
        Assert.Equal(VirtualKeyCode.MENU, result[0].VirtualKeyCode);
    }

    [Fact]
    public void Parse_WinModifier_ReturnsCorrectSequence()
    {
        var result = _parser.Parse("#r");

        Assert.Equal(3, result.Count);
        Assert.Equal(KeyActionType.ModifierDown, result[0].Type);
        Assert.Equal(VirtualKeyCode.LWIN, result[0].VirtualKeyCode);
        Assert.Equal(KeyActionType.KeyPress, result[1].Type);
        Assert.Equal(VirtualKeyCode.VK_R, result[1].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierUp, result[2].Type);
        Assert.Equal(VirtualKeyCode.LWIN, result[2].VirtualKeyCode);
    }

    [Fact]
    public void Parse_MultipleModifiers_ReturnsCorrectSequence()
    {
        var result = _parser.Parse("^+s"); // Ctrl+Shift+S

        Assert.Equal(5, result.Count);
        Assert.Equal(KeyActionType.ModifierDown, result[0].Type);
        Assert.Equal(VirtualKeyCode.CONTROL, result[0].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierDown, result[1].Type);
        Assert.Equal(VirtualKeyCode.SHIFT, result[1].VirtualKeyCode);
        Assert.Equal(KeyActionType.KeyPress, result[2].Type);
        Assert.Equal(VirtualKeyCode.VK_S, result[2].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierUp, result[3].Type);
        Assert.Equal(VirtualKeyCode.SHIFT, result[3].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierUp, result[4].Type);
        Assert.Equal(VirtualKeyCode.CONTROL, result[4].VirtualKeyCode);
    }

    [Theory]
    [InlineData("{Enter}", VirtualKeyCode.RETURN)]
    [InlineData("{Return}", VirtualKeyCode.RETURN)]
    [InlineData("{Tab}", VirtualKeyCode.TAB)]
    [InlineData("{Space}", VirtualKeyCode.SPACE)]
    [InlineData("{Backspace}", VirtualKeyCode.BACK)]
    [InlineData("{BS}", VirtualKeyCode.BACK)]
    [InlineData("{Delete}", VirtualKeyCode.DELETE)]
    [InlineData("{Del}", VirtualKeyCode.DELETE)]
    [InlineData("{Insert}", VirtualKeyCode.INSERT)]
    [InlineData("{Ins}", VirtualKeyCode.INSERT)]
    public void Parse_SpecialKeys_ReturnsCorrectKeyPress(string input, VirtualKeyCode expectedKey)
    {
        var result = _parser.Parse(input);

        Assert.Single(result);
        Assert.Equal(KeyActionType.KeyPress, result[0].Type);
        Assert.Equal(expectedKey, result[0].VirtualKeyCode);
    }

    [Theory]
    [InlineData("{Home}", VirtualKeyCode.HOME)]
    [InlineData("{End}", VirtualKeyCode.END)]
    [InlineData("{PageUp}", VirtualKeyCode.PRIOR)]
    [InlineData("{PgUp}", VirtualKeyCode.PRIOR)]
    [InlineData("{PageDown}", VirtualKeyCode.NEXT)]
    [InlineData("{PgDn}", VirtualKeyCode.NEXT)]
    public void Parse_NavigationKeys_ReturnsCorrectKeyPress(string input, VirtualKeyCode expectedKey)
    {
        var result = _parser.Parse(input);

        Assert.Single(result);
        Assert.Equal(KeyActionType.KeyPress, result[0].Type);
        Assert.Equal(expectedKey, result[0].VirtualKeyCode);
    }

    [Theory]
    [InlineData("{Up}", VirtualKeyCode.UP)]
    [InlineData("{Down}", VirtualKeyCode.DOWN)]
    [InlineData("{Left}", VirtualKeyCode.LEFT)]
    [InlineData("{Right}", VirtualKeyCode.RIGHT)]
    public void Parse_ArrowKeys_ReturnsCorrectKeyPress(string input, VirtualKeyCode expectedKey)
    {
        var result = _parser.Parse(input);

        Assert.Single(result);
        Assert.Equal(KeyActionType.KeyPress, result[0].Type);
        Assert.Equal(expectedKey, result[0].VirtualKeyCode);
    }

    [Theory]
    [InlineData("{F1}", VirtualKeyCode.F1)]
    [InlineData("{F2}", VirtualKeyCode.F2)]
    [InlineData("{F5}", VirtualKeyCode.F5)]
    [InlineData("{F10}", VirtualKeyCode.F10)]
    [InlineData("{F12}", VirtualKeyCode.F12)]
    public void Parse_FunctionKeys_ReturnsCorrectKeyPress(string input, VirtualKeyCode expectedKey)
    {
        var result = _parser.Parse(input);

        Assert.Single(result);
        Assert.Equal(KeyActionType.KeyPress, result[0].Type);
        Assert.Equal(expectedKey, result[0].VirtualKeyCode);
    }

    [Theory]
    [InlineData("{Esc}", VirtualKeyCode.ESCAPE)]
    [InlineData("{Escape}", VirtualKeyCode.ESCAPE)]
    [InlineData("{PrintScreen}", VirtualKeyCode.SNAPSHOT)]
    [InlineData("{PrtSc}", VirtualKeyCode.SNAPSHOT)]
    [InlineData("{Pause}", VirtualKeyCode.PAUSE)]
    [InlineData("{CapsLock}", VirtualKeyCode.CAPITAL)]
    [InlineData("{NumLock}", VirtualKeyCode.NUMLOCK)]
    [InlineData("{ScrollLock}", VirtualKeyCode.SCROLL)]
    public void Parse_SystemKeys_ReturnsCorrectKeyPress(string input, VirtualKeyCode expectedKey)
    {
        var result = _parser.Parse(input);

        Assert.Single(result);
        Assert.Equal(KeyActionType.KeyPress, result[0].Type);
        Assert.Equal(expectedKey, result[0].VirtualKeyCode);
    }

    [Fact]
    public void Parse_SpecialKeyWithModifier_ReturnsCorrectSequence()
    {
        var result = _parser.Parse("+{Enter}"); // Shift+Enter

        Assert.Equal(3, result.Count);
        Assert.Equal(KeyActionType.ModifierDown, result[0].Type);
        Assert.Equal(VirtualKeyCode.SHIFT, result[0].VirtualKeyCode);
        Assert.Equal(KeyActionType.KeyPress, result[1].Type);
        Assert.Equal(VirtualKeyCode.RETURN, result[1].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierUp, result[2].Type);
        Assert.Equal(VirtualKeyCode.SHIFT, result[2].VirtualKeyCode);
    }

    [Fact]
    public void Parse_AltF4_ReturnsCorrectSequence()
    {
        var result = _parser.Parse("!{F4}"); // Alt+F4

        Assert.Equal(3, result.Count);
        Assert.Equal(KeyActionType.ModifierDown, result[0].Type);
        Assert.Equal(VirtualKeyCode.MENU, result[0].VirtualKeyCode);
        Assert.Equal(KeyActionType.KeyPress, result[1].Type);
        Assert.Equal(VirtualKeyCode.F4, result[1].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierUp, result[2].Type);
        Assert.Equal(VirtualKeyCode.MENU, result[2].VirtualKeyCode);
    }

    [Theory]
    [InlineData(" ", VirtualKeyCode.SPACE)]
    [InlineData(",", VirtualKeyCode.OEM_COMMA)]
    [InlineData(".", VirtualKeyCode.OEM_PERIOD)]
    [InlineData("/", VirtualKeyCode.OEM_2)]
    [InlineData(";", VirtualKeyCode.OEM_1)]
    [InlineData("'", VirtualKeyCode.OEM_7)]
    [InlineData("[", VirtualKeyCode.OEM_4)]
    [InlineData("]", VirtualKeyCode.OEM_6)]
    [InlineData("\\", VirtualKeyCode.OEM_5)]
    [InlineData("-", VirtualKeyCode.OEM_MINUS)]
    [InlineData("=", VirtualKeyCode.OEM_PLUS)]
    [InlineData("`", VirtualKeyCode.OEM_3)]
    public void Parse_SpecialCharacters_ReturnsCorrectKeyPress(string input, VirtualKeyCode expectedKey)
    {
        var result = _parser.Parse(input);

        Assert.Single(result);
        Assert.Equal(KeyActionType.KeyPress, result[0].Type);
        Assert.Equal(expectedKey, result[0].VirtualKeyCode);
    }

    [Fact]
    public void Parse_MultipleKeys_ReturnsSequentialActions()
    {
        var result = _parser.Parse("abc");

        Assert.Equal(3, result.Count);
        Assert.All(result, action => Assert.Equal(KeyActionType.KeyPress, action.Type));
        Assert.Equal(VirtualKeyCode.VK_A, result[0].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.VK_B, result[1].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.VK_C, result[2].VirtualKeyCode);
    }

    [Fact]
    public void Parse_MalformedBrace_SkipsInvalidPart()
    {
        var result = _parser.Parse("{InvalidKey");

        // Malformed brace should be skipped
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_UnknownSpecialKey_ReturnsEmpty()
    {
        var result = _parser.Parse("{UnknownKey}");

        // Unknown special key should return no actions
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_ComplexSequence_ReturnsCorrectActions()
    {
        var result = _parser.Parse("^c{Tab}^v"); // Ctrl+C, Tab, Ctrl+V

        // Should have: Ctrl down, C press, Ctrl up, Tab press, Ctrl down, V press, Ctrl up
        Assert.Equal(7, result.Count);

        // Verify Ctrl+C
        Assert.Equal(KeyActionType.ModifierDown, result[0].Type);
        Assert.Equal(VirtualKeyCode.CONTROL, result[0].VirtualKeyCode);
        Assert.Equal(KeyActionType.KeyPress, result[1].Type);
        Assert.Equal(VirtualKeyCode.VK_C, result[1].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierUp, result[2].Type);
        Assert.Equal(VirtualKeyCode.CONTROL, result[2].VirtualKeyCode);

        // Verify Tab
        Assert.Equal(KeyActionType.KeyPress, result[3].Type);
        Assert.Equal(VirtualKeyCode.TAB, result[3].VirtualKeyCode);

        // Verify Ctrl+V
        Assert.Equal(KeyActionType.ModifierDown, result[4].Type);
        Assert.Equal(VirtualKeyCode.CONTROL, result[4].VirtualKeyCode);
        Assert.Equal(KeyActionType.KeyPress, result[5].Type);
        Assert.Equal(VirtualKeyCode.VK_V, result[5].VirtualKeyCode);
        Assert.Equal(KeyActionType.ModifierUp, result[6].Type);
        Assert.Equal(VirtualKeyCode.CONTROL, result[6].VirtualKeyCode);
    }

    [Fact]
    public void Parse_CaseInsensitiveSpecialKeys_WorksCorrectly()
    {
        var resultLower = _parser.Parse("{enter}");
        var resultUpper = _parser.Parse("{ENTER}");
        var resultMixed = _parser.Parse("{EnTeR}");

        Assert.Single(resultLower);
        Assert.Single(resultUpper);
        Assert.Single(resultMixed);

        Assert.Equal(VirtualKeyCode.RETURN, resultLower[0].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.RETURN, resultUpper[0].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.RETURN, resultMixed[0].VirtualKeyCode);
    }

    [Fact]
    public void Parse_ModifierReleaseOrder_IsReversed()
    {
        var result = _parser.Parse("^!+s"); // Ctrl+Alt+Shift+S

        // Should press modifiers in order: Ctrl, Alt, Shift
        // Then release in reverse: Shift, Alt, Ctrl
        Assert.Equal(7, result.Count);

        Assert.Equal(VirtualKeyCode.CONTROL, result[0].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.MENU, result[1].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.SHIFT, result[2].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.VK_S, result[3].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.SHIFT, result[4].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.MENU, result[5].VirtualKeyCode);
        Assert.Equal(VirtualKeyCode.CONTROL, result[6].VirtualKeyCode);
    }
}
