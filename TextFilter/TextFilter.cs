namespace ktsu.TextFilter;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DotNet.Globbing;
using ktsu.FuzzySearch;

/// <summary>
/// Specifies the type of text filter to be used.
/// </summary>
public enum TextFilterType
{
	Glob,
	Regex,
	Fuzzy,
}

/// <summary>
/// Specifies the options for matching text filters.
/// </summary>
public enum TextFilterMatchOptions
{
	ByWholeString,
	ByWordAll,
	ByWordAny,
}

internal enum TextFilterTokenType
{
	Optional,
	Required,
	Excluded,
}

/// <summary>
/// Provides methods for filtering text based on different filter types and match options.
/// </summary>
public static class TextFilter
{
	private static HashSet<char> ExcludedTokenPrefixes { get; } = ['!', '-', '^'];
	private static HashSet<char> RequiredTokenPrefixes { get; } = ['+'];
	private static ConcurrentDictionary<string, Regex> RegexCache { get; } = [];
	private static ConcurrentDictionary<string, Glob> GlobCache { get; } = [];

	/// <summary>
	/// Gets a hint for the specified filter type.
	/// </summary>
	/// <param name="filterType">The type of the filter.</param>
	/// <returns>A hint string describing the filter pattern.</returns>
	public static string GetHint(TextFilterType filterType)
	{
		return filterType switch
		{
			TextFilterType.Glob => "glob pattern, 'optional1* opti?nal2 +required -excluded' etc, text must contain one of the optional tokens, all of the required tokens, and none of the excluded tokens",
			TextFilterType.Regex => "regex pattern, text must match the regex pattern",
			TextFilterType.Fuzzy => "fuzzy pattern, text is ranked by how well it matches the pattern",
			_ => throw new NotImplementedException($"{nameof(TextFilterType)}.{filterType} has not been implemented"),
		};
	}

	/// <summary>
	/// Filters the specified collection of strings based on the provided filter and filter type.
	/// </summary>
	/// <param name="strings">The collection of strings to filter.</param>
	/// <param name="filter">The filter pattern.</param>
	/// <param name="filterType">The type of the filter.</param>
	/// <param name="textFilterMatchOptions">The options for matching text filters.</param>
	/// <returns>A collection of strings that match the filter.</returns>
	/// <remarks>When using fuzzy matching, the strings are sorted by their match score.</remarks>
	public static IEnumerable<string> Filter(IEnumerable<string> strings, string filter, TextFilterType filterType = TextFilterType.Glob, TextFilterMatchOptions textFilterMatchOptions = TextFilterMatchOptions.ByWordAny)
	{
		ArgumentNullException.ThrowIfNull(strings);
		ArgumentNullException.ThrowIfNull(filter);

		return strings.Select(text =>
		{
			bool isMatch = IsMatch(text, filter, out int score, filterType, textFilterMatchOptions);
			return (text, isMatch, score);
		})
		.Where(t => t.isMatch)
		.OrderByDescending(t => t.score)
		.Select(t => t.text);
	}

	/// <summary>
	/// Ranks the specified collection of strings based on the provided fuzzy filter pattern.
	/// </summary>
	/// <param name="strings">The collection of strings to rank.</param>
	/// <param name="fuzzyFilter">The filter pattern.</param>
	/// <returns>The collection of strings sorted by their match score.</returns>
	/// <remarks>Uses fuzzy matching to rank the strings by their match score.</remarks>
	public static IEnumerable<string> Rank(IEnumerable<string> strings, string fuzzyFilter)
	{
		ArgumentNullException.ThrowIfNull(strings);
		ArgumentNullException.ThrowIfNull(fuzzyFilter);

		return strings.Select(text =>
		{
			bool isMatch = IsMatch(text, fuzzyFilter, out int score, TextFilterType.Fuzzy);
			return (text, isMatch, score);
		})
		.OrderByDescending(t => t.score)
		.Select(t => t.text);
	}

	/// <summary>
	/// Determines whether the specified text matches the filter pattern.
	/// </summary>
	/// <param name="text">The text to match.</param>
	/// <param name="filter">The filter pattern.</param>
	/// <param name="score">The score of the match (used with fuzzy matching).</param>
	/// <param name="filterType">The type of the filter.</param>
	/// <param name="textFilterMatchOptions">The options for matching text filters.</param>
	/// <returns><c>true</c> if the text matches the filter pattern; otherwise, <c>false</c>.</returns>
	public static bool IsMatch(string text, string filter, out int score, TextFilterType filterType = TextFilterType.Glob, TextFilterMatchOptions textFilterMatchOptions = TextFilterMatchOptions.ByWordAny)
	{
		ArgumentNullException.ThrowIfNull(text);
		ArgumentNullException.ThrowIfNull(filter);

		score = int.MinValue;
		return filterType switch
		{
			TextFilterType.Glob => DoesMatchGlob(text, filter, textFilterMatchOptions),
			TextFilterType.Regex => DoesMatchRegex(text, filter, textFilterMatchOptions),
			TextFilterType.Fuzzy => Fuzzy.Contains(text, filter, out score),
			_ => throw new NotImplementedException($"{nameof(TextFilterType)}.{filterType} has not been implemented"),
		};
	}

	/// <summary>
	/// Determines whether the specified text matches the filter pattern.
	/// </summary>
	/// <param name="text">The text to match.</param>
	/// <param name="filter">The filter pattern.</param>
	/// <param name="filterType">The type of the filter.</param>
	/// <param name="textFilterMatchOptions">The options for matching text filters.</param>
	/// <returns><c>true</c> if the text matches the filter pattern; otherwise, <c>false</c>.</returns>
	public static bool IsMatch(string text, string filter, TextFilterType filterType = TextFilterType.Glob, TextFilterMatchOptions textFilterMatchOptions = TextFilterMatchOptions.ByWordAny)
		=> IsMatch(text, filter, out _, filterType, textFilterMatchOptions);

	internal static HashSet<string> ExtractTextTokens(string text, TextFilterMatchOptions textFilterMatchOptions)
	{
		return textFilterMatchOptions switch
		{
			TextFilterMatchOptions.ByWholeString => [text],
			TextFilterMatchOptions.ByWordAll => [.. text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)],
			TextFilterMatchOptions.ByWordAny => [.. text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)],
			_ => throw new NotImplementedException($"{nameof(TextFilterMatchOptions)}.{textFilterMatchOptions} has not been implemented"),
		};
	}

	internal static Dictionary<TextFilterTokenType, HashSet<string>> ExtractGlobFilterTokens(string filter)
	{
		string[] filterTokens = filter.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		return filterTokens.GroupBy(t =>
		{
			char prefix = t.First();

			return ExcludedTokenPrefixes.Contains(prefix)
				? TextFilterTokenType.Excluded
				: RequiredTokenPrefixes.Contains(prefix)
				? TextFilterTokenType.Required
				: TextFilterTokenType.Optional;
		})
		.ToDictionary(g => g.Key, g =>
		{
			bool removePrefix = g.Key is TextFilterTokenType.Required or TextFilterTokenType.Excluded;
			return (removePrefix ? g.Select(t => t[1..]) : g).ToHashSet();
		});
	}

	/// <summary>
	/// Determines whether the specified text matches the glob filter pattern.
	/// </summary>
	/// <param name="text">The text to match.</param>
	/// <param name="filter">The glob filter pattern.</param>
	/// <param name="textFilterMatchOptions">The options for matching text filters.</param>
	/// <returns><c>true</c> if the text matches the glob filter pattern; otherwise, <c>false</c>.</returns>
	public static bool DoesMatchGlob(string text, string filter, TextFilterMatchOptions textFilterMatchOptions)
	{
		ArgumentNullException.ThrowIfNull(text);
		ArgumentNullException.ThrowIfNull(filter);

		var filterTokens = ExtractGlobFilterTokens(filter);
		var textTokens = ExtractTextTokens(text, textFilterMatchOptions);

		if (!filterTokens.TryGetValue(TextFilterTokenType.Excluded, out var excludedTokens))
		{
			excludedTokens = [];
		}

		if (!filterTokens.TryGetValue(TextFilterTokenType.Required, out var requiredTokens))
		{
			requiredTokens = [];
		}

		if (!filterTokens.TryGetValue(TextFilterTokenType.Optional, out var optionalTokens))
		{
			optionalTokens = [];
		}

		bool anyExcludedMatches = excludedTokens.Any(filterToken =>
		{
			return AnyTokenMatchesGlobFilter(filterToken, textTokens);
		});

		if (anyExcludedMatches)
		{
			return false; // text contains an excluded token
		}

		Func<IEnumerable<string>, Func<string, bool>, bool> optionalMatchFunc = textFilterMatchOptions is TextFilterMatchOptions.ByWordAny
			? Enumerable.Any
			: Enumerable.All;

		bool anyOptionalMatches = optionalMatchFunc(optionalTokens, filterToken => AnyTokenMatchesGlobFilter(filterToken, textTokens));

		if (optionalTokens.Count != 0 && !anyOptionalMatches)
		{
			return false; // optional tokens were set but text does not contain any optional tokens
		}

		Func<string, HashSet<string>, bool> requiredMatchFunc = textFilterMatchOptions is TextFilterMatchOptions.ByWordAny
			? AnyTokenMatchesGlobFilter
			: AllTokensMatchGlobFilter;

		bool allRequiredMatches = requiredTokens.All(filterToken => requiredMatchFunc(filterToken, textTokens));

		if (!allRequiredMatches)
		{
			return false; // text does not contain all required tokens
		}

		return true;
	}

	/// <summary>
	/// Determines whether any token in the text matches the specified glob filter token.
	/// </summary>
	/// <param name="filterToken">The glob filter token.</param>
	/// <param name="textTokens">The set of text tokens to match against.</param>
	/// <returns><c>true</c> if any token matches the glob filter token; otherwise, <c>false</c>.</returns>
	public static bool AnyTokenMatchesGlobFilter(string filterToken, HashSet<string> textTokens)
	{
		ArgumentNullException.ThrowIfNull(filterToken);
		ArgumentNullException.ThrowIfNull(textTokens);

		if (!GlobCache.TryGetValue(filterToken, out var glob))
		{
			glob = Glob.Parse(filterToken);
			GlobCache.TryAdd(filterToken, glob);
		}

		return textTokens.Any(glob.IsMatch);
	}

	/// <summary>
	/// Determines whether all tokens in the text match the specified glob filter token.
	/// </summary>
	/// <param name="filterToken">The glob filter token.</param>
	/// <param name="textTokens">The set of text tokens to match against.</param>
	/// <returns><c>true</c> if all tokens match the glob filter token; otherwise, <c>false</c>.</returns>
	public static bool AllTokensMatchGlobFilter(string filterToken, HashSet<string> textTokens)
	{
		ArgumentNullException.ThrowIfNull(filterToken);
		ArgumentNullException.ThrowIfNull(textTokens);

		if (!GlobCache.TryGetValue(filterToken, out var glob))
		{
			glob = Glob.Parse(filterToken);
			GlobCache.TryAdd(filterToken, glob);
		}

		return textTokens.All(glob.IsMatch);
	}

	/// <summary>
	/// Determines whether the specified text matches the regex filter pattern.
	/// </summary>
	/// <param name="text">The text to match.</param>
	/// <param name="filter">The regex filter pattern.</param>
	/// <param name="textFilterMatchOptions">The options for matching text filters.</param>
	/// <returns><c>true</c> if the text matches the regex filter pattern; otherwise, <c>false</c>.</returns>
	public static bool DoesMatchRegex(string text, string filter, TextFilterMatchOptions textFilterMatchOptions)
	{
		ArgumentNullException.ThrowIfNull(text);
		ArgumentNullException.ThrowIfNull(filter);

		var textTokens = ExtractTextTokens(text, textFilterMatchOptions);
		if (!RegexCache.TryGetValue(filter, out var regex))
		{
			regex = new Regex(filter, RegexOptions.Compiled);
			RegexCache.TryAdd(filter, regex);
		}

		Func<IEnumerable<string>, Func<string, bool>, bool> matchFunc = textFilterMatchOptions is TextFilterMatchOptions.ByWordAny
			? Enumerable.Any
			: Enumerable.All;

		return matchFunc(textTokens, textToken => regex.IsMatch(textToken));
	}
}
