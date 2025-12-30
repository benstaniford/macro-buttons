# MacroButtons.Tests

Unit test project for MacroButtons application.

## Overview

This project contains comprehensive unit tests for the MacroButtons application, focusing on components that can be tested without requiring a Windows environment or extensive refactoring.

## Test Coverage

### AutoHotkeyParserTests (60+ tests)
Comprehensive tests for the AutoHotkey syntax parser including:
- Single character parsing (letters, digits, special characters)
- Modifier keys (Ctrl, Shift, Alt, Win)
- Multiple modifiers
- Special keys (Enter, Tab, F1-F12, arrows, etc.)
- Complex keystroke sequences
- Edge cases and error handling

### ActionDefinitionTests (20+ tests)
Tests for action type detection:
- Different action types (Keypress, Python, Executable, Builtin, NavigateBack)
- Priority resolution when multiple actions are set
- Empty and null value handling

### GlobalConfigTests (20+ tests)
Tests for global configuration parsing:
- Refresh interval parsing (seconds, minutes, hours)
- Invalid format handling
- Default values
- Edge cases (zero, large values, null)

### SendKeysConfigTests (25+ tests)
Tests for keystroke timing configuration:
- Delay and duration parsing (milliseconds, seconds)
- Invalid format handling
- Default values
- Independent delay and duration settings

### ButtonItemTests (25+ tests)
Tests for button item model:
- Static vs dynamic titles
- Action and submenu detection
- Theme support
- Nested submenus
- Edge cases

### TitleDefinitionTests (30+ tests)
Tests for dynamic title definitions:
- Python, Executable, and Builtin command detection
- Refresh interval parsing with minimum 100ms enforcement
- Theme support
- Edge cases and validation

## Running Tests

### On Windows
```bash
# Run all tests
dotnet test MacroButtons.Tests/MacroButtons.Tests.csproj

# Run with detailed output
dotnet test MacroButtons.Tests/MacroButtons.Tests.csproj --logger "console;verbosity=detailed"

# Run specific test class
dotnet test MacroButtons.Tests/MacroButtons.Tests.csproj --filter "FullyQualifiedName~AutoHotkeyParserTests"
```

### On Linux (limited support)
Some tests will run on Linux, but components that depend on Windows-specific features (WPF, InputSimulator, etc.) may not work.

```bash
dotnet test MacroButtons.Tests/MacroButtons.Tests.csproj
```

## Test Framework

- **xUnit**: Primary test framework
- **Target Framework**: .NET 8.0-windows

## What's NOT Tested (Without Refactoring)

The following components are not tested in this project as they would require significant refactoring to make them testable:

- **KeystrokeService**: Depends on InputSimulatorPlus and Win32 APIs
- **MonitorService**: Depends on System.Windows.Forms.Screen
- **WindowHelper**: Uses Win32 APIs directly
- **CommandExecutionService**: Process execution (could be tested with mocks)
- **ConfigurationService**: File I/O operations (could be tested with mocks)
- **ViewModels**: WPF-specific dependencies (DispatcherTimer, ICommand)
- **Services with side effects**: Profile management, settings persistence

## Future Improvements

To increase test coverage without breaking existing functionality:

1. **Dependency Injection**: Introduce interfaces for services to enable mocking
2. **Separate Logic from UI**: Extract business logic from ViewModels
3. **Abstract File I/O**: Create file system abstraction for ConfigurationService
4. **Mock Win32 APIs**: Create wrappers around Win32 calls for testing

## Contributing

When adding new tests:
- Follow existing naming conventions (e.g., `MethodName_Scenario_ExpectedBehavior`)
- Use `[Theory]` with `[InlineData]` for parameterized tests
- Add descriptive comments for complex test scenarios
- Ensure tests are independent and can run in any order
