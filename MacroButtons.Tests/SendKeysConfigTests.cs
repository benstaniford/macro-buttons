using MacroButtons.Models;
using Xunit;

namespace MacroButtons.Tests;

/// <summary>
/// Tests for SendKeysConfig model.
/// </summary>
public class SendKeysConfigTests
{
    [Theory]
    [InlineData("10ms", 10)]
    [InlineData("30ms", 30)]
    [InlineData("50ms", 50)]
    [InlineData("100ms", 100)]
    [InlineData("1000ms", 1000)]
    public void GetDelay_WithMilliseconds_ReturnsCorrectTimeSpan(string delay, int expectedMilliseconds)
    {
        var config = new SendKeysConfig { Delay = delay };

        var result = config.GetDelay();

        Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), result);
    }

    [Theory]
    [InlineData("1s", 1)]
    [InlineData("2s", 2)]
    [InlineData("5s", 5)]
    public void GetDelay_WithSeconds_ReturnsCorrectTimeSpan(string delay, int expectedSeconds)
    {
        var config = new SendKeysConfig { Delay = delay };

        var result = config.GetDelay();

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result);
    }

    [Fact]
    public void GetDelay_WithDefaultValue_Returns30Milliseconds()
    {
        var config = new SendKeysConfig(); // Uses default "30ms"

        var result = config.GetDelay();

        Assert.Equal(TimeSpan.FromMilliseconds(30), result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("10")]
    [InlineData("10x")]
    [InlineData("abc")]
    [InlineData("10 ms")]
    [InlineData("ms10")]
    public void GetDelay_WithInvalidFormat_ReturnsDefault30Milliseconds(string delay)
    {
        var config = new SendKeysConfig { Delay = delay };

        var result = config.GetDelay();

        Assert.Equal(TimeSpan.FromMilliseconds(30), result);
    }

    [Theory]
    [InlineData("10ms", 10)]
    [InlineData("30ms", 30)]
    [InlineData("50ms", 50)]
    [InlineData("100ms", 100)]
    public void GetDuration_WithMilliseconds_ReturnsCorrectTimeSpan(string duration, int expectedMilliseconds)
    {
        var config = new SendKeysConfig { Duration = duration };

        var result = config.GetDuration();

        Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), result);
    }

    [Theory]
    [InlineData("1s", 1)]
    [InlineData("2s", 2)]
    public void GetDuration_WithSeconds_ReturnsCorrectTimeSpan(string duration, int expectedSeconds)
    {
        var config = new SendKeysConfig { Duration = duration };

        var result = config.GetDuration();

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result);
    }

    [Fact]
    public void GetDuration_WithDefaultValue_Returns30Milliseconds()
    {
        var config = new SendKeysConfig(); // Uses default "30ms"

        var result = config.GetDuration();

        Assert.Equal(TimeSpan.FromMilliseconds(30), result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("10")]
    [InlineData("abc")]
    public void GetDuration_WithInvalidFormat_ReturnsDefault30Milliseconds(string duration)
    {
        var config = new SendKeysConfig { Duration = duration };

        var result = config.GetDuration();

        Assert.Equal(TimeSpan.FromMilliseconds(30), result);
    }

    [Fact]
    public void GetDelay_WithNullValue_ThrowsException()
    {
        var config = new SendKeysConfig { Delay = null! };

        Assert.Throws<ArgumentNullException>(() => config.GetDelay());
    }

    [Fact]
    public void GetDuration_WithNullValue_ThrowsException()
    {
        var config = new SendKeysConfig { Duration = null! };

        Assert.Throws<ArgumentNullException>(() => config.GetDuration());
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var config = new SendKeysConfig();

        Assert.Equal("30ms", config.Delay);
        Assert.Equal("30ms", config.Duration);
    }

    [Theory]
    [InlineData("0ms")]
    [InlineData("0s")]
    public void GetDelay_WithZeroValue_ReturnsZero(string delay)
    {
        var config = new SendKeysConfig { Delay = delay };

        var result = config.GetDelay();

        Assert.Equal(TimeSpan.Zero, result);
    }

    [Theory]
    [InlineData("0ms")]
    [InlineData("0s")]
    public void GetDuration_WithZeroValue_ReturnsZero(string duration)
    {
        var config = new SendKeysConfig { Duration = duration };

        var result = config.GetDuration();

        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public void GetDelay_AndGetDuration_CanHaveDifferentValues()
    {
        var config = new SendKeysConfig
        {
            Delay = "50ms",
            Duration = "100ms"
        };

        var delay = config.GetDelay();
        var duration = config.GetDuration();

        Assert.Equal(TimeSpan.FromMilliseconds(50), delay);
        Assert.Equal(TimeSpan.FromMilliseconds(100), duration);
    }

    [Theory]
    [InlineData("1ms")]
    [InlineData("5ms")]
    [InlineData("1000ms")]
    [InlineData("5000ms")]
    public void GetDelay_WithVariousMillisecondValues_ParsesCorrectly(string delay)
    {
        var config = new SendKeysConfig { Delay = delay };

        var result = config.GetDelay();

        Assert.True(result.TotalMilliseconds > 0);
    }

    [Theory]
    [InlineData("1ms")]
    [InlineData("5ms")]
    [InlineData("1000ms")]
    public void GetDuration_WithVariousMillisecondValues_ParsesCorrectly(string duration)
    {
        var config = new SendKeysConfig { Duration = duration };

        var result = config.GetDuration();

        Assert.True(result.TotalMilliseconds > 0);
    }

    [Fact]
    public void SendKeysConfig_CanBeInstantiatedWithCustomValues()
    {
        var config = new SendKeysConfig
        {
            Delay = "100ms",
            Duration = "50ms"
        };

        Assert.Equal("100ms", config.Delay);
        Assert.Equal("50ms", config.Duration);
        Assert.Equal(TimeSpan.FromMilliseconds(100), config.GetDelay());
        Assert.Equal(TimeSpan.FromMilliseconds(50), config.GetDuration());
    }
}
