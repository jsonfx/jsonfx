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

using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Xml.Stax
{
	public class StaxFormatterTests
	{
		#region Simple Single Element Tests

		[Fact]
		public void Format_SingleOpenCloseTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Simple Single Element Tests

		#region Namespace Tests

		[Fact]
		public void Format_DefaultNamespaceTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			    };
			const string expected = @"<root xmlns=""http://example.com/schema""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NamespacePrefixTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("prefix", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("prefix", "http://example.com/schema"),
			    };
			const string expected = @"<prefix:root xmlns:prefix=""http://example.com/schema""></prefix:root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NamespacedChildTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("foo")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.com/schema")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			        StaxGrammar.TokenElementEnd(new DataName("foo"))
			    };
			const string expected = @"<foo><child xmlns=""http://example.com/schema"">value</child></foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ParentAndChildShareDefaultNamespace_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org")
			    };
			const string expected = @"<foo xmlns=""http://example.org""><child>value</child></foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ParentAndChildSharePrefixedNamespace_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("bar", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("bar", "http://example.org")
			    };
			const string expected = @"<bar:foo xmlns:bar=""http://example.org""><bar:child>value</bar:child></bar:foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ParentAndChildDifferentDefaultNamespaces_ReturnsMarkup()
		{
			var input = new[]
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
			const string expected = @"<foo xmlns=""http://json.org""><child xmlns=""http://jsonfx.net"">text value</child></foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_DifferentPrefixSameNamespace_ReturnsMarkup()
		{
			// Not sure if this is correct: http://stackoverflow.com/questions/3312390
			// "The namespace name for an unprefixed attribute name always has no value"
			// "The attribute value in a default namespace declaration MAY be empty.
			// This has the same effect, within the scope of the declaration, of there being no default namespace."
			// http://www.w3.org/TR/xml-names/#defaulting

			var input = new[]
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
			const string expected = @"<foo xmlns=""http://example.org"" xmlns:blah=""http://example.org"" key=""value""></foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NestedDefaultNamespaces_ReturnsMarkup()
		{
			var input = new[]
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
			const string expected = @"<outer xmlns=""http://example.org/outer""><middle-1 xmlns=""http://example.org/inner""><inner>this should be inner</inner></middle-1><middle-2>this should be outer</middle-2></outer>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Simple Attribute Tests

		[Fact]
		public void Format_SingleAttribute_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("attrName")),
			        StaxGrammar.TokenText("attrValue"),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root attrName=""attrValue""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_SingleEmptyAttributeXmlStyle_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("noValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root noValue=""""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_SingleEmptyAttributeHtmlStyle_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("noValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root noValue></root>";

			var formatter = new StaxFormatter(new DataWriterSettings()) { Html5EmptyAttributes=true };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_SingleAttributeEmptyValue_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("emptyValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root emptyValue=""""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_SingleAttributeWhitespace_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("whitespace")),
			        StaxGrammar.TokenText(" this contains whitespace "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root whitespace="" this contains whitespace ""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_MultipleAttributes_ReturnsMarkup()
		{
			var input = new[]
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
			const string expected = @"<root anyQuotedText=""/\"+"\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A"+@"&#x8;&#xC;&#xA;&#xD;&#x9;`1~!@#$%^&amp;*()_+-=[]{}|;:',./&lt;&gt;?"" no-value="""" whitespace="" this contains whitespace ""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Simple Attribute Tests

		#region Text Content Tests

		[Fact]
		public void Format_XmlEntityLt_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText("<")
			    };
			const string expected = @"&lt;";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_XmlEntityB_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText("B")
			    };
			const string expected = @"B";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_XmlEntityHex_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText("\uABCD")
			    };
			const string expected = "\uABCD";

			var formatter = new StaxFormatter(new DataWriterSettings()) { EncodeNonAscii=false };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_XmlEntityHexEncodeNonAscii_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText("\uABCD")
			    };
			const string expected = @"&#xABCD;";

			var formatter = new StaxFormatter(new DataWriterSettings()) { EncodeNonAscii=true };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_HtmlEntityEuro_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText("€")
			    };
			const string expected = "€";

			var formatter = new StaxFormatter(new DataWriterSettings()) { EncodeNonAscii=false };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_HtmlEntityEuroEncodeNonAscii_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText("€")
			    };
			const string expected = @"&#x20AC;";

			var formatter = new StaxFormatter(new DataWriterSettings()) { EncodeNonAscii=true };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_EntityWithLeadingText_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText("leading"),
			        StaxGrammar.TokenText("&")
			    };
			const string expected = @"leading&amp;";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_EntityWithTrailingText_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText("&"),
			        StaxGrammar.TokenText("trailing")
			    };
			const string expected = @"&amp;trailing";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_MixedEntities_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText(@"there should <b>e decoded chars & inside this text")
			    };
			const string expected = @"there should &lt;b&gt;e decoded chars &amp; inside this text";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Text Content Tests

		#region Mixed Content Tests

		[Fact]
		public void Format_HtmlContent_ReturnsMarkup()
		{
			var input = new[]
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
			const string expected = @"<div class=""content""><p style=""color:red""><strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.</p></div>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_HtmlContentPrettyPrinted_ReturnsMarkup()
		{
			var input = new[]
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
			const string expected =
@"<div class=""content"">
	<p style=""color:red"">
		<strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.
	</p>
</div>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Mixed Content Tests

		#region Error Recovery Tests

		[Fact]
		public void Format_UnclosedOpenTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root"))
			    };
			const string expected = @"<root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_UnopenedCloseTag_ReturnsMarkup()
		{
			var input = new []
				{
					StaxGrammar.TokenElementEnd(new DataName("foo"))
				};
			const string expected = @"</foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_OverlappingTags_ReturnsMarkupAsIs()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("odd")),
			        StaxGrammar.TokenElementBegin(new DataName("auto-closed")),
			        StaxGrammar.TokenElementBegin(new DataName("even")),
			        StaxGrammar.TokenElementEnd(new DataName("odd")),
			        StaxGrammar.TokenElementEnd(new DataName("ignored")),
			        StaxGrammar.TokenElementEnd(new DataName("even"))
			    };
			const string expected = @"<odd><auto-closed><even></odd></ignored></even>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_OverlappingTagsAutoBalanced_ReturnsMarkup()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("odd")),
			        StaxGrammar.TokenElementBegin(new DataName("auto-closed")),
			        StaxGrammar.TokenElementBegin(new DataName("even")),
			        StaxGrammar.TokenElementEnd(new DataName("even")),
			        StaxGrammar.TokenElementEnd(new DataName("auto-closed")),
			        StaxGrammar.TokenElementEnd(new DataName("odd"))
			    };
			const string expected = @"<odd><auto-closed><even></even></auto-closed></odd>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_OverlappingNamespacedTagsErrorRecovery_ReturnsMarkupAsIs()
		{
			var input = new[]
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
			const string expected = @"<a:odd xmlns=""http://example.com/odd"" xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns=""http://example.com/auto-closed"" xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns=""http://example.com/even"" xmlns:c=""http://example.com/even/c""></a:odd></ignored></c:even>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_OverlappingNamespacedTagsErrorRecoveryAutoBalanced_ReturnsMarkupAsIs()
		{
			var input = new[]
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
			const string expected = @"<a:odd xmlns=""http://example.com/odd"" xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns=""http://example.com/auto-closed"" xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns=""http://example.com/even"" xmlns:c=""http://example.com/even/c""></c:even></b:auto-closed></a:odd>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Error Recovery Tests

		#region Unparsed Block Tests Tests

		[Fact]
		public void Format_XmlDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
			    };
			const string expected = @"<?xml version=""1.0""?>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_XmlComment_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
			    };
			const string expected = @"<!-- a quick note -->";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_MarkupLikeText_ReturnsTextValue()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenText(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };
			const string expected = @"value&gt;""0"" &amp;&amp; value&lt;""10"" ?""valid"":""error""";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_MathML_ReturnsMarkup()
		{
			var input = new[]
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
			const string expected =
@"<p>You can add a string to a number, but this stringifies the number:</p>
<math>
	<ms>x&lt;y</ms>
	<mo>+</mo>
	<mn>3</mn>
	<mo>=</mo>
	<ms>x&lt;y3</ms>
</math>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_XmlDocTypeExternal_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenUnparsed("!{0}",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };
			const string expected =
@"<!DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		//[Fact(Skip="Embedded DOCTYPE not supported")]
		public void Format_XmlDocTypeLocal_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenUnparsed("!{0}",
@"DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]")
			    };
			const string expected =
@"<!DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_AspNetPageDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        StaxGrammar.TokenUnparsed("%@{0}%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };
			const string expected = @"<%@ Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" %>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_PhpHelloWorld_ReturnsMarkup()
		{
			var input = new[]
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
			const string expected =
@"<html>
	<head>
		<title>PHP Test</title>
	</head>
	<body>
		<?php echo '<p>Hello World</p>'; ?>
	</body>
</html>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_CodeCommentAroundMarkup_ReturnsSingleUnparsedBlock()
		{
			var input = new[]
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
			const string expected =
@"<%--
<html>
	<body style=""color:lime"">
		<!-- not much to say here -->
	</body>
</html>
--%>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Unparsed Block Tests

		#region Input Edge Case Tests

		[Fact]
		public void Format_EmptyInput_ReturnsEmptySequence()
		{
			var input = new Token<StaxTokenType>[0];
			const string expected = "";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
