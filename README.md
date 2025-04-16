# TextSearchLib

A powerful text search library for .NET that provides efficient file indexing and searching capabilities with real-time file system monitoring.

## Features

### TextSearchLib.Core
- 🔍 Fast word search across multiple files with configurable word slitting logic
- 📂 Real-time file system monitoring
- 📝 Support for both single files and directories
- 🔄 Automatic index updates on file changes
- 🚀 Suitable for concurrent search operations
- 📊 Verbose logging support for debugging
- 🔤 Case-sensitive search support


### TextSearchLib.DemoCli
A command-line interface demo application that showcases the library's capabilities:
- Interactive command prompt
- File and directory management
- Real-time search
- Verbose mode for debugging

## Getting Started

### Prerequisites
- .NET 6.0 or later
- Visual Studio 2022 or JetBrains Rider (recommended)

### Installation

```bash
cd TextSearchLib
dotnet build
```

### Usage

#### Using the Core Library

Basic usage:
```csharp
// Create a TextFinder instance with default settings
var textFinder = new TextFinder();

// Add files or directories
textFinder.AddFile("path/to/file.txt");
textFinder.AddDirectory("path/to/directory");

// Search for files containing a word
var results = textFinder.FindFilesContainingWord("wordToSearch");
```

With custom word splitting:
```csharp
var textFinder = new TextFinder(wordSplitter: text => text.Split('|'));
```

With case-sensitive search:
```csharp
// Create a TextFinder with case-sensitive search
var textFinder = new TextFinder(true);

// This will only match exact case
var results = textFinder.FindFilesContainingWord("CaseSensitive");
// Will not match: "casesensitive" or "CASESENSITIVE"
```

With logging:
```csharp
// Set up logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

var logger = loggerFactory.CreateLogger<TextFinder>();
var textFinder = new TextFinder(logger: logger);

// Now you'll see debug logs for file operations
textFinder.AddFile("path/to/file.txt");
```

Combining options:
```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

var logger = loggerFactory.CreateLogger<TextFinder>();
var textFinder = new TextFinder(
    wordSplitter: text => text.Split('|'),
    caseSensitive: true,
    logger: logger
);

// This will use all custom settings:
// - Custom word splitting
// - Case-sensitive search
// - Debug logging
textFinder.AddFile("path/to/file.txt");
var results = textFinder.FindFilesContainingWord("Case|Sensitive");
```

#### Using the Demo CLI

Run the demo application:
```bash
cd TextSearchLib.DemoCli
dotnet run
```

Available commands:
- `addfile <path>` - Add a single text file to the index
- `adddir <path>` - Add all text files from a directory (recursively)
- `find <word>` - Search for files containing a word
- `help` - Show available commands
- `exit` - Exit the program

Run with verbose logging:
```bash
dotnet run -- -v
```


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
