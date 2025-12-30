using MacroButtons.Models;
using Xunit;

namespace MacroButtons.Tests;

/// <summary>
/// Tests for GlobalConfig model.
/// </summary>
public class GlobalConfigTests
{
    [Theory]
    [InlineData("1s", 1)]
    [InlineData("30s", 30)]
    [InlineData("60s", 60)]
    [InlineData("120s", 120)]
    public void GetRefreshInterval_WithSeconds_ReturnsCorrectTimeSpan(string refresh, int expectedSeconds)
    {
        var config = new GlobalConfig { Refresh = refresh };

        var result = config.GetRefreshInterval();

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result);
    }

    [Theory]
    [InlineData("1m", 1)]
    [InlineData("5m", 5)]
    [InlineData("30m", 30)]
    [InlineData("60m", 60)]
    public void GetRefreshInterval_WithMinutes_ReturnsCorrectTimeSpan(string refresh, int expectedMinutes)
    {
        var config = new GlobalConfig { Refresh = refresh };

        var result = config.GetRefreshInterval();

        Assert.Equal(TimeSpan.FromMinutes(expectedMinutes), result);
    }

    [Theory]
    [InlineData("1h", 1)]
    [InlineData("2h", 2)]
    [InlineData("24h", 24)]
    public void GetRefreshInterval_WithHours_ReturnsCorrectTimeSpan(string refresh, int expectedHours)
    {
        var config = new GlobalConfig { Refresh = refresh };

        var result = config.GetRefreshInterval();

        Assert.Equal(TimeSpan.FromHours(expectedHours), result);
    }

    [Fact]
    public void GetRefreshInterval_WithDefaultValue_Returns30Seconds()
    {
        var config = new GlobalConfig(); // Uses default "30s"

        var result = config.GetRefreshInterval();

        Assert.Equal(TimeSpan.FromSeconds(30), result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("10")]
    [InlineData("10x")]
    [InlineData("abc123")]
    [InlineData("10 s")]
    [InlineData("s10")]
    public void GetRefreshInterval_WithInvalidFormat_ReturnsDefault30Seconds(string refresh)
    {
        var config = new GlobalConfig { Refresh = refresh };

        var result = config.GetRefreshInterval();

        Assert.Equal(TimeSpan.FromSeconds(30), result);
    }

    [Fact]
    public void GetRefreshInterval_WithNullRefresh_ReturnsDefault()
    {
        var config = new GlobalConfig { Refresh = null! };

        var result = config.GetRefreshInterval();

        Assert.Equal(TimeSpan.FromSeconds(30), result);
    }

    [Theory]
    [InlineData("0s")]
    [InlineData("0m")]
    [InlineData("0h")]
    public void GetRefreshInterval_WithZeroValue_ReturnsZero(string refresh)
    {
        var config = new GlobalConfig { Refresh = refresh };

        var result = config.GetRefreshInterval();

        Assert.Equal(TimeSpan.Zero, result);
    }

    [Theory]
    [InlineData("999s")]
    [InlineData("999m")]
    [InlineData("999h")]
    public void GetRefreshInterval_WithLargeValues_ParsesCorrectly(string refresh)
    {
        var config = new GlobalConfig { Refresh = refresh };

        var result = config.GetRefreshInterval();

        Assert.True(result.TotalSeconds > 0);
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var config = new GlobalConfig();

        Assert.Equal("30s", config.Refresh);
        Assert.NotNull(config.SendKeys);
        Assert.Equal("default", config.ProfileName);
        Assert.Null(config.ActiveWindow);
    }

    [Fact]
    public void SendKeys_IsInitializedByDefault()
    {
        var config = new GlobalConfig();

        Assert.NotNull(config.SendKeys);
        Assert.IsType<SendKeysConfig>(config.SendKeys);
    }

    [Fact]
    public void ProfileName_CanBeSet()
    {
        var config = new GlobalConfig
        {
            ProfileName = "gaming"
        };

        Assert.Equal("gaming", config.ProfileName);
    }

    [Fact]
    public void ActiveWindow_CanBeNull()
    {
        var config = new GlobalConfig
        {
            ActiveWindow = null
        };

        Assert.Null(config.ActiveWindow);
    }

    [Fact]
    public void ActiveWindow_CanBeSet()
    {
        var config = new GlobalConfig
        {
            ActiveWindow = "notepad.exe"
        };

        Assert.Equal("notepad.exe", config.ActiveWindow);
    }

    [Theory]
    [InlineData("notepad.exe")]
    [InlineData("chrome.exe|Chrome_WidgetWin_1")]
    [InlineData("game.exe|UnityWndClass")]
    public void ActiveWindow_AcceptsVariousFormats(string activeWindow)
    {
        var config = new GlobalConfig
        {
            ActiveWindow = activeWindow
        };

        Assert.Equal(activeWindow, config.ActiveWindow);
    }
}
