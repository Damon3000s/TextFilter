# ktsu.TextFilter

> A .NET library for filtering text using glob patterns, regular expressions, and fuzzy matching

[![License](https://img.shields.io/github/license/ktsu-dev/TextFilter)](https://github.com/ktsu-dev/TextFilter/blob/main/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/v/ktsu.TextFilter.svg)](https://www.nuget.org/packages/ktsu.TextFilter/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ktsu.TextFilter.svg)](https://www.nuget.org/packages/ktsu.TextFilter/)
[![Build Status](https://github.com/ktsu-dev/TextFilter/workflows/build/badge.svg)](https://github.com/ktsu-dev/TextFilter/actions)
[![GitHub Stars](https://img.shields.io/github/stars/ktsu-dev/TextFilter?style=social)](https://github.com/ktsu-dev/TextFilter/stargazers)

## Introduction

ktsu.TextFilter is a .NET library that provides methods for filtering text based on different filter types and match options. It supports glob patterns, regular expressions, and fuzzy matching to help you efficiently filter and search through collections of strings.

## Features

- **Glob Pattern Matching**: Filter text using glob patterns with optional, required, and excluded tokens.
- **Regular Expression Matching**: Filter text using regular expressions.
- **Fuzzy Matching**: Rank text based on how well it matches a fuzzy pattern.
- **Customizable Match Options**: Match by whole string, all words, or any word.

## Installation

### Package Manager Console

```powershell
Install-Package ktsu.TextFilter
```

### .NET CLI

```bash
dotnet add package ktsu.TextFilter
```

### Package Reference

```xml
<PackageReference Include="ktsu.TextFilter" Version="x.y.z" />
```

## Usage Examples

### Basic Example

```csharp
using ktsu.TextFilter;

string text = "Hello, World!";
string pattern = "Hello*";

bool isMatch = TextFilter.Match(text, pattern);
```

### Filtering Collections

```csharp
using ktsu.TextFilter;

string[] texts = new string[] { "Hello, World!", "Goodbye, World!" };
string pattern = "Hello*";

IEnumerable<string> matches = TextFilter.Filter(texts, pattern);
```

### Ranking Results

```csharp
using ktsu.TextFilter;

string[] texts = new string[] { "Hello, World!", "Goodbye, World!" };
string pattern = "Hello";

IEnumerable<string> ranked = TextFilter.Rank(texts, pattern);
```

## Advanced Usage

### Match Options

You can customize how matching is performed using the `MatchOptions` enum:

```csharp
using ktsu.TextFilter;

string text = "Hello beautiful world";
string pattern = "hello world";

// Match by any word in the pattern
bool anyWordMatch = TextFilter.Match(text, pattern, MatchOptions.AnyWord);

// Match by all words in the pattern
bool allWordsMatch = TextFilter.Match(text, pattern, MatchOptions.AllWords);

// Match the entire string
bool wholeStringMatch = TextFilter.Match(text, pattern, MatchOptions.WholeString);
```

### Filter Types

TextFilter supports different filter types:

```csharp
using ktsu.TextFilter;

string text = "Hello, World!";
string globPattern = "Hello*";
string regexPattern = "^Hello";

// Glob pattern matching
bool globMatch = TextFilter.Match(text, globPattern, filterType: FilterType.Glob);

// Regular expression matching
bool regexMatch = TextFilter.Match(text, regexPattern, filterType: FilterType.Regex);

// Fuzzy matching
bool fuzzyMatch = TextFilter.Match(text, "Helo", filterType: FilterType.Fuzzy);
```

## API Reference

### `TextFilter` Class

The primary class for text filtering operations.

#### Methods

| Name | Return Type | Description |
|------|-------------|-------------|
| `Match(string text, string pattern, MatchOptions options = MatchOptions.WholeString, FilterType filterType = FilterType.Glob)` | `bool` | Tests if the input text matches the specified pattern |
| `Filter(IEnumerable<string> texts, string pattern, MatchOptions options = MatchOptions.WholeString, FilterType filterType = FilterType.Glob)` | `IEnumerable<string>` | Returns all texts that match the pattern |
| `Rank(IEnumerable<string> texts, string pattern, MatchOptions options = MatchOptions.WholeString)` | `IEnumerable<string>` | Returns texts ranked by how well they match the pattern |

### Enums

#### `MatchOptions`

| Value | Description |
|-------|-------------|
| `WholeString` | Match the entire string against the pattern |
| `AllWords` | Match all words in the pattern against the string |
| `AnyWord` | Match any word in the pattern against the string |

#### `FilterType`

| Value | Description |
|-------|-------------|
| `Glob` | Use glob pattern matching |
| `Regex` | Use regular expression matching |
| `Fuzzy` | Use fuzzy matching |

## Contributing

Contributions are welcome! For feature requests, bug reports, or questions, please open an issue on GitHub. If you would like to contribute code, please open a pull request with your changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details.

## Acknowledgements

- [DotNet.Glob](https://github.com/dazinator/DotNet.Glob) for glob pattern matching.
- [ktsu.FuzzySearch](https://github.com/ktsu-io/ktsu.FuzzySearch) for fuzzy matching.
