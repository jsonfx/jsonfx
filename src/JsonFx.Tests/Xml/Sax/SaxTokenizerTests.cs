#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;
using System.Linq;

using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Xml.Sax
{
	public class SaxTokenizerTests
	{
		#region Simple Single Element Tests

		[Fact]
		public void GetTokens_SingleOpenCloseTag_ReturnsSequence()
		{
			const string input = @"<root></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleVoidTag_ReturnsSequence()
		{
			const string input = @"<root />";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Single Element Tests

		#region Namespace Tests

		[Fact]
		public void GetTokens_DefaultNamespaceTag_ReturnsSequence()
		{
			const string input = @"<root xmlns=""http://example.com/schema""></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        SaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        SaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        SaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NamespacePrefixTag_ReturnsSequence()
		{
			const string input = @"<prefix:root xmlns:prefix=""http://example.com/schema""></prefix:root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenPrefixBegin("prefix", "http://example.com/schema"),
			        SaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        SaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        SaxGrammar.TokenPrefixEnd("prefix", "http://example.com/schema"),
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NamespacedChildTag_ReturnsSequence()
		{
			const string input = @"<foo><child xmlns=""http://example.com/schema"">value</child></foo>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("foo")),
			        SaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        SaxGrammar.TokenElementBegin(new DataName("child", "http://example.com/schema")),
			        SaxGrammar.TokenText("value"),
			        SaxGrammar.TokenElementEnd(new DataName("child", "http://example.com/schema")),
			        SaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			        SaxGrammar.TokenElementEnd(new DataName("foo"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ParentAndChildShareDefaultNamespace_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://example.org""><child>value</child></foo>";
			var expected = new[]
			    {
			        SaxGrammar.TokenPrefixBegin("", "http://example.org"),
			        SaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        SaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        SaxGrammar.TokenText("value"),
			        SaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        SaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        SaxGrammar.TokenPrefixEnd("", "http://example.org")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ParentAndChildSharePrefixedNamespace_ReturnsSequence()
		{
			const string input = @"<bar:foo xmlns:bar=""http://example.org""><bar:child>value</bar:child></bar:foo>";
			var expected = new[]
			    {
			        SaxGrammar.TokenPrefixBegin("bar", "http://example.org"),
			        SaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        SaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        SaxGrammar.TokenText("value"),
			        SaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        SaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        SaxGrammar.TokenPrefixEnd("bar", "http://example.org")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ParentAndChildDifferentDefaultNamespaces_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://json.org""><child xmlns=""http://jsonfx.net"">text value</child></foo>";
			var expected = new[]
			    {
			        SaxGrammar.TokenPrefixBegin("", "http://json.org"),
			        SaxGrammar.TokenElementBegin(new DataName("foo", "http://json.org")),
			        SaxGrammar.TokenPrefixBegin("", "http://jsonfx.net"),
			        SaxGrammar.TokenElementBegin(new DataName("child", "http://jsonfx.net")),
			        SaxGrammar.TokenText("text value"),
			        SaxGrammar.TokenElementEnd(new DataName("child", "http://jsonfx.net")),
			        SaxGrammar.TokenPrefixEnd("", "http://jsonfx.net"),
			        SaxGrammar.TokenElementEnd(new DataName("foo", "http://json.org")),
			        SaxGrammar.TokenPrefixEnd("", "http://json.org")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_DifferentPrefixSameNamespace_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://example.org"" xmlns:blah=""http://example.org"" blah:key=""value"" />";
			var expected = new[]
			    {
			        SaxGrammar.TokenPrefixBegin("", "http://example.org"),
			        SaxGrammar.TokenPrefixBegin("blah", "http://example.org"),
			        SaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        SaxGrammar.TokenAttribute(new DataName("key", "http://example.org"), "value"),
			        SaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        SaxGrammar.TokenPrefixEnd("", "http://example.org"),
			        SaxGrammar.TokenPrefixEnd("blah", "http://example.org")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NestedDefaultNamespaces_ReturnsSequence()
		{
			const string input = @"<outer xmlns=""http://example.org/outer""><middle-1 xmlns=""http://example.org/inner""><inner>this should be inner</inner></middle-1><middle-2>this should be outer</middle-2></outer>";

			var expected = new[]
			    {
			        SaxGrammar.TokenPrefixBegin("", "http://example.org/outer"),
			        SaxGrammar.TokenElementBegin(new DataName("outer", "http://example.org/outer")),
			        SaxGrammar.TokenPrefixBegin("", "http://example.org/inner"),
			        SaxGrammar.TokenElementBegin(new DataName("middle-1", "http://example.org/inner")),
			        SaxGrammar.TokenElementBegin(new DataName("inner", "http://example.org/inner")),
			        SaxGrammar.TokenText("this should be inner"),
			        SaxGrammar.TokenElementEnd(new DataName("inner", "http://example.org/inner")),
			        SaxGrammar.TokenElementEnd(new DataName("middle-1", "http://example.org/inner")),
			        SaxGrammar.TokenPrefixEnd("", "http://example.org/inner"),
			        SaxGrammar.TokenElementBegin(new DataName("middle-2", "http://example.org/outer")),
			        SaxGrammar.TokenText("this should be outer"),
			        SaxGrammar.TokenElementEnd(new DataName("middle-2", "http://example.org/outer")),
			        SaxGrammar.TokenElementEnd(new DataName("outer", "http://example.org/outer")),
			        SaxGrammar.TokenPrefixEnd("", "http://example.org/outer")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Simple Attribute Tests

		[Fact]
		public void GetTokens_SingleTagSingleAttribute_ReturnsSequence()
		{
			const string input = @"<root attrName=""attrValue""></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("attrName"), "attrValue"),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeNoValue_ReturnsSequence()
		{
			const string input = @"<root noValue></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("noValue"), String.Empty),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeEmptyValue_ReturnsSequence()
		{
			const string input = @"<root emptyValue=""""></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("emptyValue"), String.Empty),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagWhitespaceAttributeQuotDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  "" extra whitespace around quote delims "" ></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("white"), " extra whitespace around quote delims "),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagWhitespaceAttributeAposDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  ' extra whitespace around apostrophe delims ' ></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("white"), " extra whitespace around apostrophe delims "),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeWhitespace_ReturnsSequence()
		{
			const string input = @"<root whitespace="" this contains whitespace ""></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("whitespace"), " this contains whitespace "),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeSingleQuoted_ReturnsSequence()
		{
			const string input = @"<root singleQuoted='apostrophe'></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("singleQuoted"), "apostrophe"),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeSingleQuotedWhitespace_ReturnsSequence()
		{
			const string input = @"<root singleQuoted_whitespace=' apostrophe with whitespace '></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("singleQuoted_whitespace"), " apostrophe with whitespace "),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagMultipleAttributes_ReturnsSequence()
		{
			const string input = @"<root no-value whitespace="" this contains whitespace "" anyQuotedText=""/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?""></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("no-value"), String.Empty),
			        SaxGrammar.TokenAttribute(new DataName("whitespace"), " this contains whitespace "),
			        SaxGrammar.TokenAttribute(new DataName("anyQuotedText"), @"/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Attribute Tests

		#region Text Content Tests

		[Fact]
		public void GetTokens_XmlEntityLt_ReturnsSequence()
		{
			const string input = @"&lt;";
			var expected = new[]
			    {
			        SaxGrammar.TokenText("<")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityB_ReturnsSequence()
		{
			const string input = @"&#66;";
			var expected = new[]
			    {
			        SaxGrammar.TokenText("B")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityHexLowerX_ReturnsSequence()
		{
			const string input = @"&#x37;";
			var expected = new[]
			    {
			        SaxGrammar.TokenText("7")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityHexUpperX_ReturnsSequence()
		{
			const string input = @"&#X38;";
			var expected = new[]
			    {
			        SaxGrammar.TokenText("8")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityHexUpperCase_ReturnsSequence()
		{
			const string input = @"&#xABCD;";
			var expected = new[]
			    {
			        SaxGrammar.TokenText("\uABCD")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityHexLowerCase_ReturnsSequence()
		{
			const string input = @"&#xabcd;";
			var expected = new[]
			    {
			        SaxGrammar.TokenText("\uabcd")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_HtmlEntityEuro_ReturnsSequence()
		{
			const string input = @"&euro;";
			var expected = new[]
			    {
			        SaxGrammar.TokenText("€")
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_MixedEntities_ReturnsSequence()
		{
			const string input = @"there should &lt;b&gt;e decoded chars &amp; inside this text";
			var expected = new[]
			    {
			        SaxGrammar.TokenText(@"there should "),
			        SaxGrammar.TokenText(@"<"),
			        SaxGrammar.TokenText(@"b"),
			        SaxGrammar.TokenText(@">"),
			        SaxGrammar.TokenText(@"e decoded chars "),
			        SaxGrammar.TokenText(@"&"),
			        SaxGrammar.TokenText(@" inside this text"),
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Text Content Tests

		#region Mixed Content Tests

		[Fact]
		public void GetTokens_HtmlContent_ReturnsSequence()
		{
			const string input = @"<div class=""content""><p style=""color:red""><strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.</p></div>";

			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("div")),
			        SaxGrammar.TokenAttribute(new DataName("class"), "content"),
			        SaxGrammar.TokenElementBegin(new DataName("p")),
			        SaxGrammar.TokenAttribute(new DataName("style"), "color:red"),
			        SaxGrammar.TokenElementBegin(new DataName("strong")),
			        SaxGrammar.TokenText("Lorem ipsum"),
			        SaxGrammar.TokenElementEnd(new DataName("strong")),
			        SaxGrammar.TokenText(" dolor sit amet, "),
			        SaxGrammar.TokenElementBegin(new DataName("i")),
			        SaxGrammar.TokenText("consectetur"),
			        SaxGrammar.TokenElementEnd(new DataName("i")),
			        SaxGrammar.TokenText(" adipiscing elit."),
			        SaxGrammar.TokenElementEnd(new DataName("p")),
					SaxGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_HtmlContentPrettyPrinted_ReturnsSequence()
		{
			const string input =
@"<div class=""content"">
	<p style=""color:red"">
		<strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.
	</p>
</div>";

			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("div")),
			        SaxGrammar.TokenAttribute(new DataName("class"), "content"),
			        SaxGrammar.TokenWhitespace("\r\n\t"),
			        SaxGrammar.TokenElementBegin(new DataName("p")),
			        SaxGrammar.TokenAttribute(new DataName("style"), "color:red"),
			        SaxGrammar.TokenWhitespace("\r\n\t\t"),
			        SaxGrammar.TokenElementBegin(new DataName("strong")),
			        SaxGrammar.TokenText("Lorem ipsum"),
			        SaxGrammar.TokenElementEnd(new DataName("strong")),
			        SaxGrammar.TokenText(" dolor sit amet, "),
			        SaxGrammar.TokenElementBegin(new DataName("i")),
			        SaxGrammar.TokenText("consectetur"),
			        SaxGrammar.TokenElementEnd(new DataName("i")),
			        SaxGrammar.TokenText(" adipiscing elit.\r\n\t"),
			        SaxGrammar.TokenElementEnd(new DataName("p")),
			        SaxGrammar.TokenWhitespace("\r\n"),
					SaxGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Mixed Content Tests

		#region Error Recovery Tests

		[Fact]
		public void GetTokens_UnclosedOpenTag_ReturnsSequence()
		{
			const string input = @"<root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// TODO: determine how this should be handled
			Assert.Equal(expected, actual);
		}

		#endregion Error Recovery Tests

		#region Input Edge Case Tests

		[Fact]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			const string input = null;
			var expected = new Token<SaxTokenType>[0];

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<SaxTokenType>[0];

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
