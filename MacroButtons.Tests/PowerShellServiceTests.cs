using MacroButtons.Services;
using Xunit;

namespace MacroButtons.Tests;

/// <summary>
/// Tests for PowerShellService.
/// </summary>
public class PowerShellServiceTests
{
    [Fact]
    public async Task ExecuteCommandAsync_WithSimpleCommand_ReturnsOutput()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "Write-Output 'Hello, World!'";

        // Act
        var result = await service.ExecuteCommandAsync(command, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Hello, World!", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithGetDate_ReturnsDateString()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "Get-Date -Format 'yyyy-MM-dd'";

        // Act
        var result = await service.ExecuteCommandAsync(command, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithCaptureOutputFalse_ReturnsEmpty()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "Write-Output 'Test'";

        // Act
        var result = await service.ExecuteCommandAsync(command, captureOutput: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithNullCommand_ThrowsArgumentException()
    {
        // Arrange
        var service = new PowerShellService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.ExecuteCommandAsync(null!, captureOutput: true));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithEmptyCommand_ThrowsArgumentException()
    {
        // Arrange
        var service = new PowerShellService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.ExecuteCommandAsync("", captureOutput: true));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithWhitespaceCommand_ThrowsArgumentException()
    {
        // Arrange
        var service = new PowerShellService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.ExecuteCommandAsync("   ", captureOutput: true));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithError_ReturnsErrorMessage()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "Get-NonExistentCmdlet";

        // Act
        var result = await service.ExecuteCommandAsync(command, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Error:", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithParameters_ExecutesWithParameters()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "param($Name, $Age) Write-Output \"$Name is $Age years old\"";
        var parameters = new Dictionary<string, object>
        {
            { "Name", "Alice" },
            { "Age", 30 }
        };

        // Act
        var result = await service.ExecuteCommandAsync(command, parameters, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Alice is 30 years old", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithNullParameters_ExecutesSuccessfully()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "Write-Output 'No parameters'";

        // Act
        var result = await service.ExecuteCommandAsync(command, parameters: null, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("No parameters", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithMultipleOutputLines_ReturnsAllLines()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "Write-Output 'Line 1'; Write-Output 'Line 2'; Write-Output 'Line 3'";

        // Act
        var result = await service.ExecuteCommandAsync(command, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Line 1", result);
        Assert.Contains("Line 2", result);
        Assert.Contains("Line 3", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithPipeline_ExecutesSuccessfully()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "1..5 | ForEach-Object { $_ * 2 }";

        // Act
        var result = await service.ExecuteCommandAsync(command, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("2", result);
        Assert.Contains("4", result);
        Assert.Contains("6", result);
        Assert.Contains("8", result);
        Assert.Contains("10", result);
    }

    [Fact]
    public async Task ExecuteScriptFileAsync_WithNonExistentFile_ReturnsError()
    {
        // Arrange
        var service = new PowerShellService();
        var scriptPath = "~/nonexistent-script-12345.ps1";

        // Act
        var result = await service.ExecuteScriptFileAsync(scriptPath, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Error: Script not found", result);
    }

    [Fact]
    public async Task ExecuteScriptFileAsync_WithNullPath_ThrowsArgumentException()
    {
        // Arrange
        var service = new PowerShellService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.ExecuteScriptFileAsync(null!, captureOutput: true));
    }

    [Fact]
    public async Task ExecuteScriptFileAsync_WithEmptyPath_ThrowsArgumentException()
    {
        // Arrange
        var service = new PowerShellService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.ExecuteScriptFileAsync("", captureOutput: true));
    }

    [Fact]
    public async Task ExecuteScriptFileAsync_WithValidScript_ExecutesSuccessfully()
    {
        // Arrange
        var service = new PowerShellService();
        var tempFile = Path.GetTempFileName();
        var scriptPath = Path.ChangeExtension(tempFile, ".ps1");
        File.Move(tempFile, scriptPath);

        try
        {
            await File.WriteAllTextAsync(scriptPath, "Write-Output 'Script executed'");

            // Act
            var result = await service.ExecuteScriptFileAsync(scriptPath, captureOutput: true);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Script executed", result);
        }
        finally
        {
            // Cleanup
            if (File.Exists(scriptPath))
                File.Delete(scriptPath);
        }
    }

    [Fact]
    public async Task ExecuteScriptFileAsync_WithParameters_PassesParametersCorrectly()
    {
        // Arrange
        var service = new PowerShellService();
        var tempFile = Path.GetTempFileName();
        var scriptPath = Path.ChangeExtension(tempFile, ".ps1");
        File.Move(tempFile, scriptPath);

        try
        {
            await File.WriteAllTextAsync(scriptPath,
                "param($Name, $Count)\nWrite-Output \"Hello $Name, count is $Count\"");

            var parameters = new Dictionary<string, object>
            {
                { "Name", "Bob" },
                { "Count", 42 }
            };

            // Act
            var result = await service.ExecuteScriptFileAsync(scriptPath, parameters, captureOutput: true);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Hello Bob, count is 42", result);
        }
        finally
        {
            // Cleanup
            if (File.Exists(scriptPath))
                File.Delete(scriptPath);
        }
    }

    [Fact]
    public async Task ExecuteScriptFileAsync_WithTildePath_ExpandsCorrectly()
    {
        // Arrange
        var service = new PowerShellService();
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var scriptPath = Path.Combine(userProfile, "test-script-temp-12345.ps1");

        try
        {
            await File.WriteAllTextAsync(scriptPath, "Write-Output 'Tilde expanded'");

            // Act - Use tilde notation
            var result = await service.ExecuteScriptFileAsync("~/test-script-temp-12345.ps1", captureOutput: true);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Tilde expanded", result);
        }
        finally
        {
            // Cleanup
            if (File.Exists(scriptPath))
                File.Delete(scriptPath);
        }
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithBooleanParameter_HandlesCorrectly()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "param([bool]$Flag) if ($Flag) { Write-Output 'True' } else { Write-Output 'False' }";
        var parameters = new Dictionary<string, object>
        {
            { "Flag", true }
        };

        // Act
        var result = await service.ExecuteCommandAsync(command, parameters, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("True", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithIntParameter_HandlesCorrectly()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "param([int]$Number) Write-Output ($Number * 2)";
        var parameters = new Dictionary<string, object>
        {
            { "Number", 21 }
        };

        // Act
        var result = await service.ExecuteCommandAsync(command, parameters, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("42", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_ConvertToJson_UsesStandardModules()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "@{Name='Test'; Value=123} | ConvertTo-Json";

        // Act
        var result = await service.ExecuteCommandAsync(command, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Name", result);
        Assert.Contains("Test", result);
        Assert.Contains("Value", result);
        Assert.Contains("123", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithSyntaxError_ReturnsError()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "Write-Output 'unclosed string";

        // Act
        var result = await service.ExecuteCommandAsync(command, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Error:", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithNoOutputAndNoError_ReturnsEmpty()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "$null"; // Command that produces no output

        // Act
        var result = await service.ExecuteCommandAsync(command, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task ExecuteScriptFileAsync_WithCaptureOutputFalse_ReturnsEmpty()
    {
        // Arrange
        var service = new PowerShellService();
        var tempFile = Path.GetTempFileName();
        var scriptPath = Path.ChangeExtension(tempFile, ".ps1");
        File.Move(tempFile, scriptPath);

        try
        {
            await File.WriteAllTextAsync(scriptPath, "Write-Output 'This should not be captured'");

            // Act
            var result = await service.ExecuteScriptFileAsync(scriptPath, captureOutput: false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result);
        }
        finally
        {
            // Cleanup
            if (File.Exists(scriptPath))
                File.Delete(scriptPath);
        }
    }

    [Fact]
    public async Task ExecuteCommandAsync_MultipleParameterTypes_HandlesCorrectly()
    {
        // Arrange
        var service = new PowerShellService();
        var command = "param($Name, [int]$Age, [bool]$Active) Write-Output \"$Name, $Age, $Active\"";
        var parameters = new Dictionary<string, object>
        {
            { "Name", "Charlie" },
            { "Age", 25 },
            { "Active", false }
        };

        // Act
        var result = await service.ExecuteCommandAsync(command, parameters, captureOutput: true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Charlie", result);
        Assert.Contains("25", result);
        Assert.Contains("False", result);
    }
}
