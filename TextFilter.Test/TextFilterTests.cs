namespace TextFilter.Test;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ktsu.TextFilter;

[TestClass]
public class TextFilterTests
{
	[TestMethod]
	public void GetHint_Glob_ReturnsCorrectHint()
	{
		string hint = TextFilter.GetHint(TextFilterType.Glob);
		Assert.AreEqual("glob pattern, 'optional1* opti?nal2 +required -excluded' etc, text must contain one of the optional tokens, all of the required tokens, and none of the excluded tokens", hint);
	}

	[TestMethod]
	public void GetHint_Regex_ReturnsCorrectHint()
	{
		string hint = TextFilter.GetHint(TextFilterType.Regex);
		Assert.AreEqual("regex pattern, text must match the regex pattern", hint);
	}

	[TestMethod]
	public void GetHint_Fuzzy_ReturnsCorrectHint()
	{
		string hint = TextFilter.GetHint(TextFilterType.Fuzzy);
		Assert.AreEqual("fuzzy pattern, text is ranked by how well it matches the pattern", hint);
	}

	[TestMethod]
	public void Filter_Glob_ByWordAny_ReturnsCorrectResults()
	{
		var strings = new List<string> { "hello world", "hello", "world" };
		var result = TextFilter.Filter(strings, "hello*", TextFilterType.Glob, TextFilterMatchOptions.ByWordAny).ToList();
		CollectionAssert.AreEqual(new List<string> { "hello world", "hello" }, result);
	}

	[TestMethod]
	public void Filter_Regex_ByWordAny_ReturnsCorrectResults()
	{
		var strings = new List<string> { "hello world", "hello", "world" };
		var result = TextFilter.Filter(strings, "^hello", TextFilterType.Regex, TextFilterMatchOptions.ByWordAny).ToList();
		CollectionAssert.AreEqual(new List<string> { "hello world", "hello" }, result);
	}

	[TestMethod]
	public void Filter_Fuzzy_ByWordAny_ReturnsCorrectResults()
	{
		var strings = new List<string> { "hello world", "hello", "world" };
		var result = TextFilter.Filter(strings, "helo", TextFilterType.Fuzzy, TextFilterMatchOptions.ByWordAny).ToList();
		CollectionAssert.AreEqual(new List<string> { "hello", "hello world" }, result);
	}

	[TestMethod]
	public void IsMatch_Glob_ByWordAny_ReturnsTrue()
	{
		bool result = TextFilter.IsMatch("hello world", "hello*", TextFilterType.Glob, TextFilterMatchOptions.ByWordAny);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsMatch_Regex_ByWordAny_ReturnsTrue()
	{
		bool result = TextFilter.IsMatch("hello world", "^hello", TextFilterType.Regex, TextFilterMatchOptions.ByWordAny);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsMatch_Fuzzy_ByWordAny_ReturnsTrue()
	{
		bool result = TextFilter.IsMatch("hello world", "helo", TextFilterType.Fuzzy, TextFilterMatchOptions.ByWordAny);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void DoesMatchGlob_ByWordAny_ReturnsTrue()
	{
		bool result = TextFilter.DoesMatchGlob("hello world", "hello*", TextFilterMatchOptions.ByWordAny);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void DoesMatchRegex_ByWordAny_ReturnsTrue()
	{
		bool result = TextFilter.DoesMatchRegex("hello world", "^hello", TextFilterMatchOptions.ByWordAny);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void AnyTokenMatchesGlobFilter_ReturnsTrue()
	{
		var textTokens = new HashSet<string> { "hello", "world" };
		bool result = TextFilter.AnyTokenMatchesGlobFilter("hello*", textTokens);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void AllTokensMatchGlobFilter_ReturnsFalse()
	{
		var textTokens = new HashSet<string> { "hello", "world" };
		bool result = TextFilter.AllTokensMatchGlobFilter("hello*", textTokens);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void ExtractTextTokens_ByWholeString_ReturnsCorrectTokens()
	{
		var result = TextFilter.ExtractTextTokens("hello world", TextFilterMatchOptions.ByWholeString);
		CollectionAssert.AreEqual(new List<string> { "hello world" }, result.ToList());
	}

	[TestMethod]
	public void ExtractTextTokens_ByWordAll_ReturnsCorrectTokens()
	{
		var result = TextFilter.ExtractTextTokens("hello world", TextFilterMatchOptions.ByWordAll);
		CollectionAssert.AreEqual(new List<string> { "hello", "world" }, result.ToList());
	}

	[TestMethod]
	public void ExtractTextTokens_ByWordAny_ReturnsCorrectTokens()
	{
		var result = TextFilter.ExtractTextTokens("hello world", TextFilterMatchOptions.ByWordAny);
		CollectionAssert.AreEqual(new List<string> { "hello", "world" }, result.ToList());
	}

	[TestMethod]
	public void ExtractGlobFilterTokens_ReturnsCorrectTokens()
	{
		var result = TextFilter.ExtractGlobFilterTokens("hello* +required -excluded");
		CollectionAssert.AreEqual(new List<string> { "hello*" }, result[TextFilterTokenType.Optional].ToList());
		CollectionAssert.AreEqual(new List<string> { "required" }, result[TextFilterTokenType.Required].ToList());
		CollectionAssert.AreEqual(new List<string> { "excluded" }, result[TextFilterTokenType.Excluded].ToList());
	}

	[TestMethod]
	public void DoesMatchGlob_WithExcludedToken_ReturnsFalse()
	{
		bool result = TextFilter.DoesMatchGlob("hello world", "hello* -world", TextFilterMatchOptions.ByWordAny);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void DoesMatchGlob_WithRequiredToken_ReturnsTrue()
	{
		bool result = TextFilter.DoesMatchGlob("hello world", "hello* +world", TextFilterMatchOptions.ByWordAny);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void DoesMatchGlob_WithOptionalToken_ReturnsTrue()
	{
		bool result = TextFilter.DoesMatchGlob("hello world", "hello* world", TextFilterMatchOptions.ByWordAny);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void DoesMatchRegex_WithMultipleTokens_ReturnsTrue()
	{
		bool result = TextFilter.DoesMatchRegex("hello world", "^hello|world$", TextFilterMatchOptions.ByWordAny);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void DoesMatchRegex_WithNoMatch_ReturnsFalse()
	{
		bool result = TextFilter.DoesMatchRegex("hello world", "^test", TextFilterMatchOptions.ByWordAny);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Filter_EmptyStrings_ReturnsEmpty()
	{
		var strings = new List<string>();
		var result = TextFilter.Filter(strings, "hello*", TextFilterType.Glob, TextFilterMatchOptions.ByWordAny).ToList();
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void Filter_NullStrings_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.Filter(null!, "hello*", TextFilterType.Glob, TextFilterMatchOptions.ByWordAny).ToList());
	}

	[TestMethod]
	public void Filter_EmptyFilter_ReturnsAllStrings()
	{
		var strings = new List<string> { "hello world", "hello", "world" };
		var result = TextFilter.Filter(strings, "", TextFilterType.Glob, TextFilterMatchOptions.ByWordAny).ToList();
		CollectionAssert.AreEqual(strings, result);
	}

	[TestMethod]
	public void Filter_NullFilter_ThrowsArgumentNullException()
	{
		var strings = new List<string> { "hello world", "hello", "world" };
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.Filter(strings, null!, TextFilterType.Glob, TextFilterMatchOptions.ByWordAny).ToList());
	}

	[TestMethod]
	public void Filter_LargeDataset_Performance()
	{
		var strings = Enumerable.Range(0, 100000).Select(i => "string" + i).ToList();
		var result = TextFilter.Filter(strings, "string*", TextFilterType.Glob, TextFilterMatchOptions.ByWordAny).ToList();
		Assert.AreEqual(100000, result.Count);
	}

	[TestMethod]
	public void Filter_ConcurrentAccess()
	{
		var strings = new List<string> { "hello world", "hello", "world" };
		var tasks = new List<Task>();

		for (int i = 0; i < 100; i++)
		{
			tasks.Add(Task.Run(() =>
			{
				var result = TextFilter.Filter(strings, "hello*", TextFilterType.Glob, TextFilterMatchOptions.ByWordAny).ToList();
				CollectionAssert.AreEqual(new List<string> { "hello world", "hello" }, result);
			}));
		}

		Task.WaitAll([.. tasks]);
	}

	[TestMethod]
	public void IsMatch_NullText_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.IsMatch(null!, "hello*", TextFilterType.Glob, TextFilterMatchOptions.ByWordAny));
	}

	[TestMethod]
	public void IsMatch_NullFilter_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.IsMatch("hello world", null!, TextFilterType.Glob, TextFilterMatchOptions.ByWordAny));
	}

	[TestMethod]
	public void AnyTokenMatchesGlobFilter_NullFilterToken_ThrowsArgumentNullException()
	{
		var textTokens = new HashSet<string> { "hello", "world" };
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.AnyTokenMatchesGlobFilter(null!, textTokens));
	}

	[TestMethod]
	public void AnyTokenMatchesGlobFilter_NullTextTokens_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.AnyTokenMatchesGlobFilter("hello*", null!));
	}

	[TestMethod]
	public void AllTokensMatchGlobFilter_NullFilterToken_ThrowsArgumentNullException()
	{
		var textTokens = new HashSet<string> { "hello", "world" };
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.AllTokensMatchGlobFilter(null!, textTokens));
	}

	[TestMethod]
	public void AllTokensMatchGlobFilter_NullTextTokens_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.AllTokensMatchGlobFilter("hello*", null!));
	}

	[TestMethod]
	public void DoesMatchGlob_NullText_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.DoesMatchGlob(null!, "hello*", TextFilterMatchOptions.ByWordAny));
	}

	[TestMethod]
	public void DoesMatchGlob_NullFilter_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.DoesMatchGlob("hello world", null!, TextFilterMatchOptions.ByWordAny));
	}

	[TestMethod]
	public void DoesMatchRegex_NullText_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.DoesMatchRegex(null!, "^hello", TextFilterMatchOptions.ByWordAny));
	}

	[TestMethod]
	public void DoesMatchRegex_NullFilter_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.DoesMatchRegex("hello world", null!, TextFilterMatchOptions.ByWordAny));
	}

	[TestMethod]
	public void Rank_Fuzzy_ReturnsCorrectRanking()
	{
		var strings = new List<string> { "hello world", "hello", "world" };
		var result = TextFilter.Rank(strings, "helo").ToList();
		CollectionAssert.AreEqual(new List<string> { "hello", "hello world", "world" }, result);
	}

	[TestMethod]
	public void Rank_EmptyStrings_ReturnsEmpty()
	{
		var strings = new List<string>();
		var result = TextFilter.Rank(strings, "helo").ToList();
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void Rank_NullStrings_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.Rank(null!, "helo").ToList());
	}

	[TestMethod]
	public void Rank_NullFilter_ThrowsArgumentNullException()
	{
		var strings = new List<string> { "hello world", "hello", "world" };
		Assert.ThrowsException<ArgumentNullException>(() => TextFilter.Rank(strings, null!).ToList());
	}
}
