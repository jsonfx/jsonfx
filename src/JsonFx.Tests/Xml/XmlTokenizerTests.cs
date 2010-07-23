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

namespace JsonFx.Xml
{
	public class XmlTokenizerTests
	{
		#region Constants

		private const string TraitName = "XML";
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
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        XmlGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        XmlGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenPrefixBegin("prefix", "http://example.com/schema"),
			        XmlGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        XmlGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        XmlGrammar.TokenPrefixEnd("prefix", "http://example.com/schema"),
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("foo")),
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        XmlGrammar.TokenElementBegin(new DataName("child", "http://example.com/schema")),
			        XmlGrammar.TokenText("value"),
			        XmlGrammar.TokenElementEnd(new DataName("child", "http://example.com/schema")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			        XmlGrammar.TokenElementEnd(new DataName("foo"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenPrefixBegin("", "http://example.org"),
			        XmlGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        XmlGrammar.TokenText("value"),
			        XmlGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        XmlGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.org")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenPrefixBegin("bar", "http://example.org"),
			        XmlGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        XmlGrammar.TokenText("value"),
			        XmlGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        XmlGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenPrefixEnd("bar", "http://example.org")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenPrefixBegin("", "http://json.org"),
			        XmlGrammar.TokenElementBegin(new DataName("foo", "http://json.org")),
			        XmlGrammar.TokenPrefixBegin("", "http://jsonfx.net"),
			        XmlGrammar.TokenElementBegin(new DataName("child", "http://jsonfx.net")),
			        XmlGrammar.TokenText("text value"),
			        XmlGrammar.TokenElementEnd(new DataName("child", "http://jsonfx.net")),
			        XmlGrammar.TokenPrefixEnd("", "http://jsonfx.net"),
			        XmlGrammar.TokenElementEnd(new DataName("foo", "http://json.org")),
			        XmlGrammar.TokenPrefixEnd("", "http://json.org")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenPrefixBegin("", "http://example.org"),
			        XmlGrammar.TokenPrefixBegin("blah", "http://example.org"),
			        XmlGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenAttribute(new DataName("key", "http://example.org")),
			        XmlGrammar.TokenText("value"),
			        XmlGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.org"),
			        XmlGrammar.TokenPrefixEnd("blah", "http://example.org")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenPrefixBegin("", "http://example.org/outer"),
			        XmlGrammar.TokenElementBegin(new DataName("outer", "http://example.org/outer")),
			        XmlGrammar.TokenPrefixBegin("", "http://example.org/inner"),
			        XmlGrammar.TokenElementBegin(new DataName("middle-1", "http://example.org/inner")),
			        XmlGrammar.TokenElementBegin(new DataName("inner", "http://example.org/inner")),
			        XmlGrammar.TokenText("this should be inner"),
			        XmlGrammar.TokenElementEnd(new DataName("inner", "http://example.org/inner")),
			        XmlGrammar.TokenElementEnd(new DataName("middle-1", "http://example.org/inner")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.org/inner"),
			        XmlGrammar.TokenElementBegin(new DataName("middle-2", "http://example.org/outer")),
			        XmlGrammar.TokenText("this should be outer"),
			        XmlGrammar.TokenElementEnd(new DataName("middle-2", "http://example.org/outer")),
			        XmlGrammar.TokenElementEnd(new DataName("outer", "http://example.org/outer")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.org/outer")
			    };

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UndeclaredPrefixes_ThrowsDeserializationException()
		{
			const string input = @"<a:one><b:two><c:three></d:three></e:two></f:one>";
			var expected = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("one")),
			        XmlGrammar.TokenElementBegin(new DataName("two")),
			        XmlGrammar.TokenElementBegin(new DataName("three")),
			        XmlGrammar.TokenElementEnd(new DataName("three")),
			        XmlGrammar.TokenElementEnd(new DataName("two")),
			        XmlGrammar.TokenElementEnd(new DataName("one"))
			    };

			var tokenizer = new XmlTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(2, ex.Index);
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
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("attrName")),
			        XmlGrammar.TokenText("attrValue"),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("noValue")),
			        XmlGrammar.TokenText(String.Empty),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("emptyValue")),
			        XmlGrammar.TokenText(String.Empty),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("white")),
			        XmlGrammar.TokenText(" extra whitespace around quote delims "),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("whitespace")),
			        XmlGrammar.TokenText(" this contains whitespace "),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultipleAttributes_ReturnsSequence()
		{
			const string input = @"<root no-value="""" whitespace="" this contains whitespace "" anyQuotedText="""+"/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\n\r\t`1~!@#$%^&amp;*()_+-=[]{}|;:',./&lt;&gt;?"+@"""></root>";
			var expected = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        XmlGrammar.TokenText("/\\\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A   `1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        XmlGrammar.TokenAttribute(new DataName("no-value")),
			        XmlGrammar.TokenText(String.Empty),
			        XmlGrammar.TokenAttribute(new DataName("whitespace")),
			        XmlGrammar.TokenText(" this contains whitespace "),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenText("<")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenText("B")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenText("7")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenText("\uABCD")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenText("\uabcd")
			    };

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlEntityEuro_ReturnsSequence()
		{
			const string input = @"&#x20AC;";
			var expected = new[]
			    {
			        XmlGrammar.TokenText("€")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenText("leading&")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenText("&trailing")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenText(@"there should <b>e decoded chars & inside this text")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("div")),
			        XmlGrammar.TokenAttribute(new DataName("class")),
			        XmlGrammar.TokenText("content"),
			        XmlGrammar.TokenElementBegin(new DataName("p")),
			        XmlGrammar.TokenAttribute(new DataName("style")),
			        XmlGrammar.TokenText("color:red"),
			        XmlGrammar.TokenElementBegin(new DataName("strong")),
			        XmlGrammar.TokenText("Lorem ipsum"),
			        XmlGrammar.TokenElementEnd(new DataName("strong")),
			        XmlGrammar.TokenText(" dolor sit amet, "),
			        XmlGrammar.TokenElementBegin(new DataName("i")),
			        XmlGrammar.TokenText("consectetur"),
			        XmlGrammar.TokenElementEnd(new DataName("i")),
			        XmlGrammar.TokenText(" adipiscing elit."),
			        XmlGrammar.TokenElementEnd(new DataName("p")),
					XmlGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("div")),
			        XmlGrammar.TokenAttribute(new DataName("class")),
			        XmlGrammar.TokenText("content"),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("p")),
			        XmlGrammar.TokenAttribute(new DataName("style")),
			        XmlGrammar.TokenText("color:red"),
			        XmlGrammar.TokenWhitespace("\n\t\t"),
			        XmlGrammar.TokenElementBegin(new DataName("strong")),
			        XmlGrammar.TokenText("Lorem ipsum"),
			        XmlGrammar.TokenElementEnd(new DataName("strong")),
			        XmlGrammar.TokenText(" dolor sit amet, "),
			        XmlGrammar.TokenElementBegin(new DataName("i")),
			        XmlGrammar.TokenText("consectetur"),
			        XmlGrammar.TokenElementEnd(new DataName("i")),
			        XmlGrammar.TokenText(" adipiscing elit.\n\t"),
			        XmlGrammar.TokenElementEnd(new DataName("p")),
			        XmlGrammar.TokenWhitespace("\n"),
					XmlGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Mixed Content Tests

		#region Unparsed Block Tests Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDeclaration_ReturnsUnparsed()
		{
			const string input = @"<?xml version=""1.0""?>";
			var expected = new[]
			    {
			        XmlGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenText(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("p")),
			        XmlGrammar.TokenText(@"You can add a string to a number, but this stringifies the number:"),
			        XmlGrammar.TokenElementEnd(new DataName("p")),
			        XmlGrammar.TokenWhitespace("\n"),
			        XmlGrammar.TokenElementBegin(new DataName("math")),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("ms")),
			        XmlGrammar.TokenText(@"x<y"),
			        XmlGrammar.TokenElementEnd(new DataName("ms")),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("mo")),
			        XmlGrammar.TokenText(@"+"),
			        XmlGrammar.TokenElementEnd(new DataName("mo")),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("mn")),
			        XmlGrammar.TokenText(@"3"),
			        XmlGrammar.TokenElementEnd(new DataName("mn")),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("mo")),
			        XmlGrammar.TokenText(@"="),
			        XmlGrammar.TokenElementEnd(new DataName("mo")),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("ms")),
			        XmlGrammar.TokenText(@"x<y3"),
			        XmlGrammar.TokenElementEnd(new DataName("ms")),
			        XmlGrammar.TokenWhitespace("\n"),
			        XmlGrammar.TokenElementEnd(new DataName("math")),
			    };

			var tokenizer = new XmlTokenizer();
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
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">
<root />";

			var tokenizer = new XmlTokenizer();
			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(0, ex.Index);
		}

		//[Fact(Skip="Embedded DOCTYPE not supported")]
		public void GetTokens_XmlDocTypeLocal_ReturnsUnparsed()
		{
			const string input =
@"<!DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]>
<root />";
			var expected = new[]
			    {
			        XmlGrammar.TokenUnparsed("!{0}",
@"DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]"),
					XmlGrammar.TokenElementBegin(new DataName("root")),
					XmlGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        XmlGrammar.TokenElementBegin(new DataName("html")),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("head")),
			        XmlGrammar.TokenWhitespace("\n\t\t"),
			        XmlGrammar.TokenElementBegin(new DataName("title")),
			        XmlGrammar.TokenText("PHP Test"),
			        XmlGrammar.TokenElementEnd(new DataName("title")),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementEnd(new DataName("head")),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("body")),
			        XmlGrammar.TokenWhitespace("\n\t\t"),
			        XmlGrammar.TokenUnparsed("?{0}?", @"php echo '<p>Hello World</p>'; "),
			        XmlGrammar.TokenWhitespace("\n\t"),
			        XmlGrammar.TokenElementEnd(new DataName("body")),
			        XmlGrammar.TokenWhitespace("\n"),
			        XmlGrammar.TokenElementEnd(new DataName("html")),
			    };

			var tokenizer = new XmlTokenizer();
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
			var expected = new Token<XmlTokenType>[0];

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<XmlTokenType>[0];

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
