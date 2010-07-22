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
using JsonFx.Serialization.Resolvers;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Xml.Stax
{
	public class StaxTokenizerTests
	{
		#region Constants

		private const string TraitName = "HTML";
		private const string TraitValue = "Deserialization";

		#endregion Constants

		#region Simple Single Element Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleOpenCloseTag_ReturnsSequence()
		{
			const string input = @"<root></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleVoidTag_ReturnsSequence()
		{
			const string input = @"<root />";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Single Element Tests

		#region Namespace Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DefaultNamespaceTag_ReturnsSequence()
		{
			const string input = @"<root xmlns=""http://example.com/schema""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacePrefixTag_ReturnsSequence()
		{
			const string input = @"<prefix:root xmlns:prefix=""http://example.com/schema""></prefix:root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("prefix", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("prefix", "http://example.com/schema"),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacedChildTag_ReturnsSequence()
		{
			const string input = @"<foo><child xmlns=""http://example.com/schema"">value</child></foo>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("foo")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.com/schema")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			        StaxGrammar.TokenElementEnd(new DataName("foo"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildShareDefaultNamespace_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://example.org""><child>value</child></foo>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildSharePrefixedNamespace_ReturnsSequence()
		{
			const string input = @"<bar:foo xmlns:bar=""http://example.org""><bar:child>value</bar:child></bar:foo>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("bar", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("bar", "http://example.org")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildDifferentDefaultNamespaces_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://json.org""><child xmlns=""http://jsonfx.net"">text value</child></foo>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://json.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://json.org")),
			        StaxGrammar.TokenPrefixBegin("", "http://jsonfx.net"),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://jsonfx.net")),
			        StaxGrammar.TokenText("text value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://jsonfx.net")),
			        StaxGrammar.TokenPrefixEnd("", "http://jsonfx.net"),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://json.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://json.org")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DifferentPrefixSameNamespace_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://example.org"" xmlns:blah=""http://example.org"" blah:key=""value"" />";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.org"),
			        StaxGrammar.TokenPrefixBegin("blah", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenAttribute(new DataName("key", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org"),
			        StaxGrammar.TokenPrefixEnd("blah", "http://example.org")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NestedDefaultNamespaces_ReturnsSequence()
		{
			const string input = @"<outer xmlns=""http://example.org/outer""><middle-1 xmlns=""http://example.org/inner""><inner>this should be inner</inner></middle-1><middle-2>this should be outer</middle-2></outer>";

			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.org/outer"),
			        StaxGrammar.TokenElementBegin(new DataName("outer", "http://example.org/outer")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.org/inner"),
			        StaxGrammar.TokenElementBegin(new DataName("middle-1", "http://example.org/inner")),
			        StaxGrammar.TokenElementBegin(new DataName("inner", "http://example.org/inner")),
			        StaxGrammar.TokenText("this should be inner"),
			        StaxGrammar.TokenElementEnd(new DataName("inner", "http://example.org/inner")),
			        StaxGrammar.TokenElementEnd(new DataName("middle-1", "http://example.org/inner")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org/inner"),
			        StaxGrammar.TokenElementBegin(new DataName("middle-2", "http://example.org/outer")),
			        StaxGrammar.TokenText("this should be outer"),
			        StaxGrammar.TokenElementEnd(new DataName("middle-2", "http://example.org/outer")),
			        StaxGrammar.TokenElementEnd(new DataName("outer", "http://example.org/outer")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org/outer")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UndeclaredPrefixesErrorRecovery_ReturnsAsDefault()
		{
			const string input = @"<a:one><b:two><c:three></d:three></e:two></f:one>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("one")),
			        StaxGrammar.TokenElementBegin(new DataName("two")),
			        StaxGrammar.TokenElementBegin(new DataName("three")),
			        StaxGrammar.TokenElementEnd(new DataName("three")),
			        StaxGrammar.TokenElementEnd(new DataName("two")),
			        StaxGrammar.TokenElementEnd(new DataName("one"))
			    };

			var tokenizer = new StaxTokenizer { ErrorRecovery=false };

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(6, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UndeclaredPrefixes_ReturnsAsDefault()
		{
			const string input = @"<a:one><b:two><c:three></d:three></e:two></f:one>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("one")),
			        StaxGrammar.TokenElementBegin(new DataName("two")),
			        StaxGrammar.TokenElementBegin(new DataName("three")),
			        StaxGrammar.TokenElementEnd(new DataName("three")),
			        StaxGrammar.TokenElementEnd(new DataName("two")),
			        StaxGrammar.TokenElementEnd(new DataName("one"))
			    };

			var tokenizer = new StaxTokenizer { ErrorRecovery=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Simple Attribute Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttribute_ReturnsSequence()
		{
			const string input = @"<root attrName=""attrValue""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("attrName")),
			        StaxGrammar.TokenText("attrValue"),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeHtmlStyle_ReturnsSequence()
		{
			const string input = @"<root noValue></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("noValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeXmlStyle_ReturnsSequence()
		{
			const string input = @"<root noValue=""""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("noValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeEmptyValue_ReturnsSequence()
		{
			const string input = @"<root emptyValue=""""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("emptyValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AttributeWhitespaceQuotDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  "" extra whitespace around quote delims "" ></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("white")),
			        StaxGrammar.TokenText(" extra whitespace around quote delims "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_WhitespaceAttributeAposDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  ' extra whitespace around apostrophe delims ' ></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("white")),
			        StaxGrammar.TokenText(" extra whitespace around apostrophe delims "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeWhitespace_ReturnsSequence()
		{
			const string input = @"<root whitespace="" this contains whitespace ""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("whitespace")),
			        StaxGrammar.TokenText(" this contains whitespace "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeSingleQuoted_ReturnsSequence()
		{
			const string input = @"<root singleQuoted='apostrophe'></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("singleQuoted")),
			        StaxGrammar.TokenText("apostrophe"),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeSingleQuotedWhitespace_ReturnsSequence()
		{
			const string input = @"<root singleQuoted_whitespace=' apostrophe with whitespace '></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("singleQuoted_whitespace")),
			        StaxGrammar.TokenText(" apostrophe with whitespace "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultipleAttributes_ReturnsSequence()
		{
			const string input = @"<root no-value whitespace="" this contains whitespace "" anyQuotedText="""+"/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"+@"""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("no-value")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenAttribute(new DataName("whitespace")),
			        StaxGrammar.TokenText(" this contains whitespace "),
			        StaxGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        StaxGrammar.TokenText("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Attribute Tests

		#region Text Content Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityLt_ReturnsSequence()
		{
			const string input = @"&lt;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("<")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityB_ReturnsSequence()
		{
			const string input = @"&#66;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("B")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityHexLowerX_ReturnsSequence()
		{
			const string input = @"&#x37;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("7")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityHexUpperX_ReturnsSequence()
		{
			const string input = @"&#X38;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("8")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityHexUpperCase_ReturnsSequence()
		{
			const string input = @"&#xABCD;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("\uABCD")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityHexLowerCase_ReturnsSequence()
		{
			const string input = @"&#xabcd;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("\uabcd")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlEntityEuro_ReturnsSequence()
		{
			const string input = @"&euro;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("€")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithLeadingText_ReturnsSequence()
		{
			const string input = @"leading&amp;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("leading"),
			        StaxGrammar.TokenText("&")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithTrailingText_ReturnsSequence()
		{
			const string input = @"&amp;trailing";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("&"),
			        StaxGrammar.TokenText("trailing")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedEntities_ReturnsSequence()
		{
			const string input = @"there should &lt;b&gt;e decoded chars &amp; inside this text";
			var expected = new[]
			    {
			        StaxGrammar.TokenText(@"there should "),
			        StaxGrammar.TokenText(@"<"),
			        StaxGrammar.TokenText(@"b"),
			        StaxGrammar.TokenText(@">"),
			        StaxGrammar.TokenText(@"e decoded chars "),
			        StaxGrammar.TokenText(@"&"),
			        StaxGrammar.TokenText(@" inside this text")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedEntitiesMalformed_ReturnsSequence()
		{
			const string input = @"there should &#xnot &Xltb&#gte decoded chars & inside this text";
			var expected = new[]
			    {
			        StaxGrammar.TokenText(@"there should "),
			        StaxGrammar.TokenText(@"&#x"),
			        StaxGrammar.TokenText(@"not "),
			        StaxGrammar.TokenText(@"&Xltb"),
			        StaxGrammar.TokenText(@"&#"),
			        StaxGrammar.TokenText(@"gte decoded chars "),
			        StaxGrammar.TokenText(@"&"),
			        StaxGrammar.TokenText(@" inside this text")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Text Content Tests

		#region Mixed Content Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlContent_ReturnsSequence()
		{
			const string input = @"<div class=""content""><p style=""color:red""><strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.</p></div>";

			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("div")),
			        StaxGrammar.TokenAttribute(new DataName("class")),
			        StaxGrammar.TokenText("content"),
			        StaxGrammar.TokenElementBegin(new DataName("p")),
			        StaxGrammar.TokenAttribute(new DataName("style")),
			        StaxGrammar.TokenText("color:red"),
			        StaxGrammar.TokenElementBegin(new DataName("strong")),
			        StaxGrammar.TokenText("Lorem ipsum"),
			        StaxGrammar.TokenElementEnd(new DataName("strong")),
			        StaxGrammar.TokenText(" dolor sit amet, "),
			        StaxGrammar.TokenElementBegin(new DataName("i")),
			        StaxGrammar.TokenText("consectetur"),
			        StaxGrammar.TokenElementEnd(new DataName("i")),
			        StaxGrammar.TokenText(" adipiscing elit."),
			        StaxGrammar.TokenElementEnd(new DataName("p")),
					StaxGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
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
			        StaxGrammar.TokenElementBegin(new DataName("div")),
			        StaxGrammar.TokenAttribute(new DataName("class")),
			        StaxGrammar.TokenText("content"),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("p")),
			        StaxGrammar.TokenAttribute(new DataName("style")),
			        StaxGrammar.TokenText("color:red"),
			        StaxGrammar.TokenWhitespace("\r\n\t\t"),
			        StaxGrammar.TokenElementBegin(new DataName("strong")),
			        StaxGrammar.TokenText("Lorem ipsum"),
			        StaxGrammar.TokenElementEnd(new DataName("strong")),
			        StaxGrammar.TokenText(" dolor sit amet, "),
			        StaxGrammar.TokenElementBegin(new DataName("i")),
			        StaxGrammar.TokenText("consectetur"),
			        StaxGrammar.TokenElementEnd(new DataName("i")),
			        StaxGrammar.TokenText(" adipiscing elit.\r\n\t"),
			        StaxGrammar.TokenElementEnd(new DataName("p")),
			        StaxGrammar.TokenWhitespace("\r\n"),
					StaxGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Mixed Content Tests

		#region Error Recovery Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnclosedOpenTag_ReturnsSequence()
		{
			const string input = @"<root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer { ErrorRecovery=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnclosedOpenTagAutoBalance_ReturnsSequence()
		{
			const string input = @"<root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer { ErrorRecovery=true, AutoBalanceTags=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnopenedCloseTag_ReturnsSequence()
		{
			const string input = @"</foo>";
			var expected = new []
				{
					StaxGrammar.TokenElementEnd(new DataName("foo"))
				};

			var tokenizer = new StaxTokenizer { ErrorRecovery=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnopenedCloseTagAutoBalance_ReturnsSequence()
		{
			const string input = @"</foo>";
			var expected = new Token<StaxTokenType>[0];

			var tokenizer = new StaxTokenizer { ErrorRecovery=true, AutoBalanceTags=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingTagsNoRecovery_ThrowsDeserializationException()
		{
			const string input = @"<odd><auto-closed><even></odd></ignored></even>";

			var tokenizer = new StaxTokenizer { ErrorRecovery=false };

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(29, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingTags_ReturnsSequenceAsIs()
		{
			const string input = @"<odd><auto-closed><even></odd></ignored></even>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("odd")),
			        StaxGrammar.TokenElementBegin(new DataName("auto-closed")),
			        StaxGrammar.TokenElementBegin(new DataName("even")),
			        StaxGrammar.TokenElementEnd(new DataName("odd")),
			        StaxGrammar.TokenElementEnd(new DataName("ignored")),
			        StaxGrammar.TokenElementEnd(new DataName("even"))
			    };

			var tokenizer = new StaxTokenizer { ErrorRecovery=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingTagsAutoBalancing_ReturnsSequenceRebalanced()
		{
			const string input = @"<odd><auto-closed><even></odd></ignored></even>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("odd")),
			        StaxGrammar.TokenElementBegin(new DataName("auto-closed")),
			        StaxGrammar.TokenElementBegin(new DataName("even")),
			        StaxGrammar.TokenElementEnd(new DataName("even")),
			        StaxGrammar.TokenElementEnd(new DataName("auto-closed")),
			        StaxGrammar.TokenElementEnd(new DataName("odd"))
			    };

			var tokenizer = new StaxTokenizer { ErrorRecovery=true, AutoBalanceTags=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingNamespacedTagsErrorRecovery_ReturnsSequenceAsIs()
		{
			const string input = @"<a:odd xmlns=""http://example.com/odd"" xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns=""http://example.com/auto-closed"" xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns=""http://example.com/even"" xmlns:c=""http://example.com/even/c""></a:odd></d:ignored></c:even>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/odd"),
			        StaxGrammar.TokenPrefixBegin("a", "http://example.com/odd/a"),
			        StaxGrammar.TokenElementBegin(new DataName("odd", "http://example.com/odd/a")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/auto-closed"),
			        StaxGrammar.TokenPrefixBegin("b", "http://example.com/auto-closed/b"),
			        StaxGrammar.TokenElementBegin(new DataName("auto-closed", "http://example.com/auto-closed/b")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/even"),
			        StaxGrammar.TokenPrefixBegin("c", "http://example.com/even/c"),
			        StaxGrammar.TokenElementBegin(new DataName("even", "http://example.com/even/c")),
			        StaxGrammar.TokenElementEnd(new DataName("odd", "http://example.com/odd/a")),
			        // NOTE: skips prefix ending for odd because can't know odd declared them
			        StaxGrammar.TokenElementEnd(new DataName("ignored")),
			        StaxGrammar.TokenElementEnd(new DataName("even", "http://example.com/even/c")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/even"),
			        StaxGrammar.TokenPrefixEnd("c", "http://example.com/even/c")
			    };

			var tokenizer = new StaxTokenizer { ErrorRecovery=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingNamespacedTagsErrorRecoveryAutoBalancing_ReturnsSequenceAsIs()
		{
			const string input = @"<a:odd xmlns=""http://example.com/odd"" xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns=""http://example.com/auto-closed"" xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns=""http://example.com/even"" xmlns:c=""http://example.com/even/c""></a:odd></d:ignored></c:even>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/odd"),
			        StaxGrammar.TokenPrefixBegin("a", "http://example.com/odd/a"),
			        StaxGrammar.TokenElementBegin(new DataName("odd", "http://example.com/odd/a")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/auto-closed"),
			        StaxGrammar.TokenPrefixBegin("b", "http://example.com/auto-closed/b"),
			        StaxGrammar.TokenElementBegin(new DataName("auto-closed", "http://example.com/auto-closed/b")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/even"),
			        StaxGrammar.TokenPrefixBegin("c", "http://example.com/even/c"),
			        StaxGrammar.TokenElementBegin(new DataName("even", "http://example.com/even/c")),
					StaxGrammar.TokenElementEnd(new DataName("even", "http://example.com/even/c")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/even"),
			        StaxGrammar.TokenPrefixEnd("c", "http://example.com/even/c"),
			        StaxGrammar.TokenElementEnd(new DataName("auto-closed", "http://example.com/auto-closed/b")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/auto-closed"),
			        StaxGrammar.TokenPrefixEnd("b", "http://example.com/auto-closed/b"),
			        StaxGrammar.TokenElementEnd(new DataName("odd", "http://example.com/odd/a")),
					StaxGrammar.TokenPrefixEnd("", "http://example.com/odd"),
			        StaxGrammar.TokenPrefixEnd("a", "http://example.com/odd/a"),
			    };

			var tokenizer = new StaxTokenizer { ErrorRecovery=true, AutoBalanceTags=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Error Recovery Tests

		#region Unparsed Block Tests Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDeclaration_ReturnsUnparsed()
		{
			const string input = @"<?xml version=""1.0""?>";
			var expected = new[]
			    {
			        StaxGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlComment_ReturnsUnparsed()
		{
			const string input = @"<!-- a quick note -->";
			var expected = new[]
			    {
			        StaxGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlCData_ReturnsTextValue()
		{
			const string input = @"<![CDATA[value>""0"" && value<""10"" ?""valid"":""error""]]>";
			var expected = new[]
			    {
			        StaxGrammar.TokenText(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlCDataMixed_ReturnsTextValue()
		{
			const string input =
@"<p>You can add a string to a number, but this stringifies the number:</p>
<math>
	<ms><![CDATA[x<y]]></ms>
	<mo>+</mo>
	<mn>3</mn>
	<mo>=</mo>
	<ms><![CDATA[x<y3]]></ms>
</math>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("p")),
			        StaxGrammar.TokenText(@"You can add a string to a number, but this stringifies the number:"),
			        StaxGrammar.TokenElementEnd(new DataName("p")),
			        StaxGrammar.TokenWhitespace("\r\n"),
			        StaxGrammar.TokenElementBegin(new DataName("math")),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("ms")),
			        StaxGrammar.TokenText(@"x<y"),
			        StaxGrammar.TokenElementEnd(new DataName("ms")),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("mo")),
			        StaxGrammar.TokenText(@"+"),
			        StaxGrammar.TokenElementEnd(new DataName("mo")),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("mn")),
			        StaxGrammar.TokenText(@"3"),
			        StaxGrammar.TokenElementEnd(new DataName("mn")),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("mo")),
			        StaxGrammar.TokenText(@"="),
			        StaxGrammar.TokenElementEnd(new DataName("mo")),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("ms")),
			        StaxGrammar.TokenText(@"x<y3"),
			        StaxGrammar.TokenElementEnd(new DataName("ms")),
			        StaxGrammar.TokenWhitespace("\r\n"),
			        StaxGrammar.TokenElementEnd(new DataName("math")),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDocTypeExternal_ReturnsUnparsed()
		{
			const string input =
@"<!DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">";

			var expected = new[]
			    {
			        StaxGrammar.TokenUnparsed("!{0}",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		//[Fact(Skip="Embedded DOCTYPE not supported")]
		public void GetTokens_XmlDocTypeLocal_ReturnsUnparsed()
		{
			const string input =
@"<!DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]>";
			var expected = new[]
			    {
			        StaxGrammar.TokenUnparsed("!{0}",
@"DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AspNetPageDeclaration_ReturnsUnparsed()
		{
			const string input = @"<%@ Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" %>";

			var expected = new[]
			    {
			        StaxGrammar.TokenUnparsed("%@{0}%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_PhpHelloWorld_ReturnsSequence()
		{
			const string input =
@"<html>
	<head>
		<title>PHP Test</title>
	</head>
	<body>
		<?php echo '<p>Hello World</p>'; ?>
	</body>
</html>";

			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("html")),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("head")),
			        StaxGrammar.TokenWhitespace("\r\n\t\t"),
			        StaxGrammar.TokenElementBegin(new DataName("title")),
			        StaxGrammar.TokenText("PHP Test"),
			        StaxGrammar.TokenElementEnd(new DataName("title")),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementEnd(new DataName("head")),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("body")),
			        StaxGrammar.TokenWhitespace("\r\n\t\t"),
			        StaxGrammar.TokenUnparsed("?{0}?", @"php echo '<p>Hello World</p>'; "),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementEnd(new DataName("body")),
			        StaxGrammar.TokenWhitespace("\r\n"),
			        StaxGrammar.TokenElementEnd(new DataName("html")),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_CodeCommentAroundMarkup_ReturnsSingleUnparsedBlock()
		{
			const string input =
@"<%--
<html>
	<body style=""color:lime"">
		<!-- not much to say here -->
	</body>
</html>
--%>";

			var expected = new[]
			    {
			        StaxGrammar.TokenUnparsed("%--{0}--%",
@"
<html>
	<body style=""color:lime"">
		<!-- not much to say here -->
	</body>
</html>
")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Unparsed Block Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			const string input = null;
			var expected = new Token<StaxTokenType>[0];

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<StaxTokenType>[0];

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
