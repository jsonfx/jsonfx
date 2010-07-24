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
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("root"), MarkupTagType.VoidTag)
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
			        MarkupGrammar.TokenElementBegin(new DataName("root", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd(new DataName("root", String.Empty, "http://example.com/schema")),
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
			        MarkupGrammar.TokenElementBegin(new DataName("root", "prefix", "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd(new DataName("root", "prefix", "http://example.com/schema"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("foo")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenText("value"),
			        MarkupGrammar.TokenElementEnd(new DataName("child", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd(new DataName("foo"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenText("value"),
			        MarkupGrammar.TokenElementEnd(new DataName("child", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenElementEnd(new DataName("foo", String.Empty, "http://example.org"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("foo", "bar", "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", "bar", "http://example.org")),
			        MarkupGrammar.TokenText("value"),
			        MarkupGrammar.TokenElementEnd(new DataName("child", "bar", "http://example.org")),
			        MarkupGrammar.TokenElementEnd(new DataName("foo", "bar", "http://example.org"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://jsonfx.net")),
			        MarkupGrammar.TokenText("text value"),
			        MarkupGrammar.TokenElementEnd(new DataName("child", String.Empty, "http://jsonfx.net")),
			        MarkupGrammar.TokenElementEnd(new DataName("foo", String.Empty, "http://json.org"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://example.org"), MarkupTagType.VoidTag),
			        MarkupGrammar.TokenAttribute(new DataName("key", "blah", "http://example.org")),
			        MarkupGrammar.TokenText("value")
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
			        MarkupGrammar.TokenElementBegin(new DataName("outer", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenElementBegin(new DataName("middle-1", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenElementBegin(new DataName("inner", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenText("this should be inner"),
			        MarkupGrammar.TokenElementEnd(new DataName("inner", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenElementEnd(new DataName("middle-1", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenElementBegin(new DataName("middle-2", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenText("this should be outer"),
			        MarkupGrammar.TokenElementEnd(new DataName("middle-2", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenElementEnd(new DataName("outer", String.Empty, "http://example.org/outer"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("one")),
			        MarkupGrammar.TokenElementBegin(new DataName("two")),
			        MarkupGrammar.TokenElementBegin(new DataName("three")),
			        MarkupGrammar.TokenElementEnd(new DataName("three")),
			        MarkupGrammar.TokenElementEnd(new DataName("two")),
			        MarkupGrammar.TokenElementEnd(new DataName("one"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("attrName")),
			        MarkupGrammar.TokenText("attrValue"),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenText(String.Empty),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("emptyValue")),
			        MarkupGrammar.TokenText(String.Empty),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("white")),
			        MarkupGrammar.TokenText(" extra whitespace around quote delims "),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenText(" this contains whitespace "),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        MarkupGrammar.TokenText("/\\\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A   `1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        MarkupGrammar.TokenAttribute(new DataName("no-value")),
			        MarkupGrammar.TokenText(String.Empty),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenText(" this contains whitespace "),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
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
			        MarkupGrammar.TokenText("<")
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
			        MarkupGrammar.TokenText("B")
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
			        MarkupGrammar.TokenText("7")
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
			        MarkupGrammar.TokenText("\uABCD")
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
			        MarkupGrammar.TokenText("\uabcd")
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
			        MarkupGrammar.TokenText("€")
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
			        MarkupGrammar.TokenText("leading&")
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
			        MarkupGrammar.TokenText("&trailing")
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
			        MarkupGrammar.TokenText(@"there should <b>e decoded chars & inside this text")
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
			        MarkupGrammar.TokenElementBegin(new DataName("div")),
			        MarkupGrammar.TokenAttribute(new DataName("class")),
			        MarkupGrammar.TokenText("content"),
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenAttribute(new DataName("style")),
			        MarkupGrammar.TokenText("color:red"),
			        MarkupGrammar.TokenElementBegin(new DataName("strong")),
			        MarkupGrammar.TokenText("Lorem ipsum"),
			        MarkupGrammar.TokenElementEnd(new DataName("strong")),
			        MarkupGrammar.TokenText(" dolor sit amet, "),
			        MarkupGrammar.TokenElementBegin(new DataName("i")),
			        MarkupGrammar.TokenText("consectetur"),
			        MarkupGrammar.TokenElementEnd(new DataName("i")),
			        MarkupGrammar.TokenText(" adipiscing elit."),
			        MarkupGrammar.TokenElementEnd(new DataName("p")),
					MarkupGrammar.TokenElementEnd(new DataName("div")),
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
			        MarkupGrammar.TokenElementBegin(new DataName("div")),
			        MarkupGrammar.TokenAttribute(new DataName("class")),
			        MarkupGrammar.TokenText("content"),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenAttribute(new DataName("style")),
			        MarkupGrammar.TokenText("color:red"),
			        MarkupGrammar.TokenWhitespace("\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("strong")),
			        MarkupGrammar.TokenText("Lorem ipsum"),
			        MarkupGrammar.TokenElementEnd(new DataName("strong")),
			        MarkupGrammar.TokenText(" dolor sit amet, "),
			        MarkupGrammar.TokenElementBegin(new DataName("i")),
			        MarkupGrammar.TokenText("consectetur"),
			        MarkupGrammar.TokenElementEnd(new DataName("i")),
			        MarkupGrammar.TokenText(" adipiscing elit.\n\t"),
			        MarkupGrammar.TokenElementEnd(new DataName("p")),
			        MarkupGrammar.TokenWhitespace("\n"),
					MarkupGrammar.TokenElementEnd(new DataName("div")),
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
			        MarkupGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
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
			        MarkupGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
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
			        MarkupGrammar.TokenText(@"value>""0"" && value<""10"" ?""valid"":""error""")
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
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenText(@"You can add a string to a number, but this stringifies the number:"),
			        MarkupGrammar.TokenElementEnd(new DataName("p")),
			        MarkupGrammar.TokenWhitespace("\n"),
			        MarkupGrammar.TokenElementBegin(new DataName("math")),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenText(@"x<y"),
			        MarkupGrammar.TokenElementEnd(new DataName("ms")),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenText(@"+"),
			        MarkupGrammar.TokenElementEnd(new DataName("mo")),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mn")),
			        MarkupGrammar.TokenText(@"3"),
			        MarkupGrammar.TokenElementEnd(new DataName("mn")),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenText(@"="),
			        MarkupGrammar.TokenElementEnd(new DataName("mo")),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenText(@"x<y3"),
			        MarkupGrammar.TokenElementEnd(new DataName("ms")),
			        MarkupGrammar.TokenWhitespace("\n"),
			        MarkupGrammar.TokenElementEnd(new DataName("math")),
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
			        MarkupGrammar.TokenUnparsed("!{0}",
@"DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]"),
					MarkupGrammar.TokenElementBegin(new DataName("root")),
					MarkupGrammar.TokenElementEnd(new DataName("root"))
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
			        MarkupGrammar.TokenElementBegin(new DataName("html")),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("head")),
			        MarkupGrammar.TokenWhitespace("\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("title")),
			        MarkupGrammar.TokenText("PHP Test"),
			        MarkupGrammar.TokenElementEnd(new DataName("title")),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementEnd(new DataName("head")),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("body")),
			        MarkupGrammar.TokenWhitespace("\n\t\t"),
			        MarkupGrammar.TokenUnparsed("?{0}?", @"php echo '<p>Hello World</p>'; "),
			        MarkupGrammar.TokenWhitespace("\n\t"),
			        MarkupGrammar.TokenElementEnd(new DataName("body")),
			        MarkupGrammar.TokenWhitespace("\n"),
			        MarkupGrammar.TokenElementEnd(new DataName("html")),
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
			var expected = new Token<MarkupTokenType>[0];

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<MarkupTokenType>[0];

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
