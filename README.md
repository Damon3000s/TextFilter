# ktsu.TextFilter

ktsu.TextFilter is a .NET library that provides methods for filtering text based on different filter types and match options. It supports glob patterns, regular expressions, and fuzzy matching to help you efficiently filter and search through collections of strings.

## Features

- **Glob Pattern Matching**: Filter text using glob patterns with optional, required, and excluded tokens.
- **Regular Expression Matching**: Filter text using regular expressions.
- **Fuzzy Matching**: Rank text based on how well it matches a fuzzy pattern.
- **Customizable Match Options**: Match by whole string, all words, or any word.

## Installation

To install TextFilter, add the following NuGet packages to your project:
```bash
dotnet add package ktsu.TextFilter
```

## Usage

### Matching Text

You can match a single string against a pattern using the `Match` method.

```csharp
using ktsu.TextFilter;

string text = "Hello, World!";
string pattern = "Hello*";

bool isMatch = TextFilter.Match(text, pattern);
```

### Filtering Text

You can filter a collection of strings using the `Filter` method.

```csharp
using ktsu.TextFilter;

string[] texts = new string[] { "Hello, World!", "Goodbye, World!" };
string pattern = "Hello*";

IEnumerable<string> matches = TextFilter.Filter(texts, pattern);
```

### Ranking Text

You can rank a collection of strings based on how well they match a fuzzy pattern using the `Rank` method.

```csharp
using ktsu.TextFilter;

string[] texts = new string[] { "Hello, World!", "Goodbye, World!" };
string pattern = "Hello";

IEnumerable<string> ranked = TextFilter.Rank(texts, pattern);
```

## Contributing

Contributions are welcome! For feature requests, bug reports, or questions, please open an issue on GitHub. If you would like to contribute code, please open a pull request with your changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [DotNet.Glob](https://github.com/dazinator/DotNet.Glob) for glob pattern matching.
- [ktsu.FuzzySearch](https://github.com/ktsu-io/ktsu.FuzzySearch) for fuzzy matching.

## Contact

For any questions or inquiries, please contact the project maintainers.
