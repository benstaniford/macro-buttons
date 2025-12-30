using MacroButtons.Models;
using Xunit;

namespace MacroButtons.Tests;

/// <summary>
/// Tests for ButtonItem model.
/// </summary>
public class ButtonItemTests
{
    [Fact]
    public void IsStaticTitle_WhenTitleIsString_ReturnsTrue()
    {
        var button = new ButtonItem
        {
            Title = "Click Me"
        };

        Assert.True(button.IsStaticTitle);
        Assert.False(button.IsDynamicTitle);
    }

    [Fact]
    public void IsDynamicTitle_WhenTitleIsTitleDefinition_ReturnsTrue()
    {
        var button = new ButtonItem
        {
            Title = new TitleDefinition()
        };

        Assert.True(button.IsDynamicTitle);
        Assert.False(button.IsStaticTitle);
    }

    [Fact]
    public void IsStaticTitle_WhenTitleIsNull_ReturnsFalse()
    {
        var button = new ButtonItem
        {
            Title = null
        };

        Assert.False(button.IsStaticTitle);
        Assert.False(button.IsDynamicTitle);
    }

    [Fact]
    public void HasAction_WhenActionIsSet_ReturnsTrue()
    {
        var button = new ButtonItem
        {
            Action = new ActionDefinition { Keypress = "^v" }
        };

        Assert.True(button.HasAction);
    }

    [Fact]
    public void HasAction_WhenActionIsNull_ReturnsFalse()
    {
        var button = new ButtonItem
        {
            Action = null
        };

        Assert.False(button.HasAction);
    }

    [Fact]
    public void HasSubmenu_WhenItemsIsNull_ReturnsFalse()
    {
        var button = new ButtonItem
        {
            Items = null
        };

        Assert.False(button.HasSubmenu);
    }

    [Fact]
    public void HasSubmenu_WhenItemsIsEmpty_ReturnsFalse()
    {
        var button = new ButtonItem
        {
            Items = new List<ButtonItem>()
        };

        Assert.False(button.HasSubmenu);
    }

    [Fact]
    public void HasSubmenu_WhenItemsHasElements_ReturnsTrue()
    {
        var button = new ButtonItem
        {
            Items = new List<ButtonItem>
            {
                new ButtonItem { Title = "Submenu Item 1" },
                new ButtonItem { Title = "Submenu Item 2" }
            }
        };

        Assert.True(button.HasSubmenu);
    }

    [Fact]
    public void HasActionAndSubmenu_WhenBothSet_ReturnsTrue()
    {
        var button = new ButtonItem
        {
            Action = new ActionDefinition { Keypress = "^v" },
            Items = new List<ButtonItem>
            {
                new ButtonItem { Title = "Submenu Item" }
            }
        };

        Assert.True(button.HasActionAndSubmenu);
    }

    [Fact]
    public void HasActionAndSubmenu_WhenOnlyActionSet_ReturnsFalse()
    {
        var button = new ButtonItem
        {
            Action = new ActionDefinition { Keypress = "^v" },
            Items = null
        };

        Assert.False(button.HasActionAndSubmenu);
    }

    [Fact]
    public void HasActionAndSubmenu_WhenOnlySubmenuSet_ReturnsFalse()
    {
        var button = new ButtonItem
        {
            Action = null,
            Items = new List<ButtonItem>
            {
                new ButtonItem { Title = "Submenu Item" }
            }
        };

        Assert.False(button.HasActionAndSubmenu);
    }

    [Fact]
    public void HasActionAndSubmenu_WhenNeitherSet_ReturnsFalse()
    {
        var button = new ButtonItem
        {
            Action = null,
            Items = null
        };

        Assert.False(button.HasActionAndSubmenu);
    }

    [Fact]
    public void GetStaticTitle_WhenTitleIsString_ReturnsString()
    {
        var button = new ButtonItem
        {
            Title = "Test Title"
        };

        var result = button.GetStaticTitle();

        Assert.Equal("Test Title", result);
    }

    [Fact]
    public void GetStaticTitle_WhenTitleIsNotString_ReturnsEmptyString()
    {
        var button = new ButtonItem
        {
            Title = new TitleDefinition()
        };

        var result = button.GetStaticTitle();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetStaticTitle_WhenTitleIsNull_ReturnsEmptyString()
    {
        var button = new ButtonItem
        {
            Title = null
        };

        var result = button.GetStaticTitle();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Theme_CanBeSet()
    {
        var button = new ButtonItem
        {
            Theme = "prominent"
        };

        Assert.Equal("prominent", button.Theme);
    }

    [Fact]
    public void Theme_CanBeNull()
    {
        var button = new ButtonItem
        {
            Theme = null
        };

        Assert.Null(button.Theme);
    }

    [Theory]
    [InlineData("default")]
    [InlineData("toggled")]
    [InlineData("prominent")]
    [InlineData("custom-theme")]
    public void Theme_AcceptsVariousThemeNames(string themeName)
    {
        var button = new ButtonItem
        {
            Theme = themeName
        };

        Assert.Equal(themeName, button.Theme);
    }

    [Fact]
    public void ButtonItem_CanBeFullyConfigured()
    {
        var button = new ButtonItem
        {
            Title = "My Button",
            Action = new ActionDefinition { Keypress = "^v" },
            Items = new List<ButtonItem>
            {
                new ButtonItem { Title = "Submenu Item" }
            },
            Theme = "prominent"
        };

        Assert.True(button.IsStaticTitle);
        Assert.True(button.HasAction);
        Assert.True(button.HasSubmenu);
        Assert.True(button.HasActionAndSubmenu);
        Assert.Equal("prominent", button.Theme);
        Assert.Equal("My Button", button.GetStaticTitle());
    }

    [Fact]
    public void ButtonItem_InfoOnlyButton_HasNoAction()
    {
        var button = new ButtonItem
        {
            Title = "Information Only",
            Action = null
        };

        Assert.True(button.IsStaticTitle);
        Assert.False(button.HasAction);
    }

    [Fact]
    public void ButtonItem_DynamicTitle_WithAction()
    {
        var button = new ButtonItem
        {
            Title = new TitleDefinition(),
            Action = new ActionDefinition { Exe = "notepad.exe" }
        };

        Assert.True(button.IsDynamicTitle);
        Assert.True(button.HasAction);
    }

    [Fact]
    public void ButtonItem_NestedSubmenu_Works()
    {
        var button = new ButtonItem
        {
            Title = "Parent",
            Items = new List<ButtonItem>
            {
                new ButtonItem
                {
                    Title = "Child",
                    Items = new List<ButtonItem>
                    {
                        new ButtonItem { Title = "Grandchild" }
                    }
                }
            }
        };

        Assert.True(button.HasSubmenu);
        Assert.Single(button.Items!);
        Assert.True(button.Items[0].HasSubmenu);
    }

    [Fact]
    public void ButtonItem_EmptyTitle_IsStaticTitle()
    {
        var button = new ButtonItem
        {
            Title = ""
        };

        Assert.True(button.IsStaticTitle);
        Assert.Equal("", button.GetStaticTitle());
    }

    [Fact]
    public void ButtonItem_WhitespaceTitle_IsStaticTitle()
    {
        var button = new ButtonItem
        {
            Title = "   "
        };

        Assert.True(button.IsStaticTitle);
        Assert.Equal("   ", button.GetStaticTitle());
    }
}
