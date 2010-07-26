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

using JsonFx.Markup;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Html
{
	public class HtmlFormatterTests
	{
		#region Constants

		private const string TraitName = "HTML";
		private const string TraitValue = "Serialization";

		#endregion Constants

		#region Simple Single Element Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleOpenCloseTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenElementEnd
			    };
			const string expected = @"<root></root>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Simple Single Element Tests

		#region Namespace Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_DefaultNamespaceTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<root xmlns=""http://example.com/schema""></root>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamespacePrefixTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", "prefix", "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<prefix:root xmlns:prefix=""http://example.com/schema""></prefix:root>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamespacedChildTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };
			const string expected = @"<foo><child xmlns=""http://example.com/schema"">value</child></foo>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ParentAndChildShareDefaultNamespace_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<foo xmlns=""http://example.org""><child>value</child></foo>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ParentAndChildSharePrefixedNamespace_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", "bar", "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", "bar", "http://example.org")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<bar:foo xmlns:bar=""http://example.org""><bar:child>value</bar:child></bar:foo>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ParentAndChildDifferentDefaultNamespaces_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://jsonfx.net")),
			        MarkupGrammar.TokenValue("text value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<foo xmlns=""http://json.org""><child xmlns=""http://jsonfx.net"">text value</child></foo>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_DifferentPrefixSameNamespace_ReturnsMarkup()
		{
			// Not sure if this is correct: http://stackoverflow.com/questions/3312390
			// "The namespace name for an unprefixed attribute name always has no value"
			// "The attribute value in a default namespace declaration MAY be empty.
			// This has the same effect, within the scope of the declaration, of there being no default namespace."
			// http://www.w3.org/TR/xml-names/#defaulting

			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenAttribute(new DataName("key", "blah", "http://example.org")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<foo xmlns=""http://example.org"" key=""value""></foo>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NestedDefaultNamespaces_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("outer", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenElementBegin(new DataName("middle-1", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenElementBegin(new DataName("inner", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenValue("this should be inner"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementBegin(new DataName("middle-2", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenValue("this should be outer"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<outer xmlns=""http://example.org/outer""><middle-1 xmlns=""http://example.org/inner""><inner>this should be inner</inner></middle-1><middle-2>this should be outer</middle-2></outer>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Simple Attribute Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleAttribute_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("attrName")),
			        MarkupGrammar.TokenValue("attrValue"),
			        MarkupGrammar.TokenElementEnd
			    };
			const string expected = @"<root attrName=""attrValue""></root>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleEmptyAttributeXhtmlStyle_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };
			const string expected = @"<root noValue=""noValue""></root>";

			var formatter = new HtmlFormatter(new DataWriterSettings()) { EmptyAttributes=HtmlFormatter.EmptyAttributeType.Xhtml };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleEmptyAttributeXmlStyle_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };
			const string expected = @"<root noValue=""""></root>";

			var formatter = new HtmlFormatter(new DataWriterSettings()) { EmptyAttributes=HtmlFormatter.EmptyAttributeType.Xml };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleEmptyAttributeHtmlStyle_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };
			const string expected = @"<root noValue></root>";

			var formatter = new HtmlFormatter(new DataWriterSettings()) { EmptyAttributes=HtmlFormatter.EmptyAttributeType.Html };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleAttributeWhitespace_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenValue(" this contains whitespace "),
			        MarkupGrammar.TokenElementEnd
			    };
			const string expected = @"<root whitespace="" this contains whitespace ""></root>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_MultipleAttributes_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementVoid(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenValue(" this contains whitespace "),
			        MarkupGrammar.TokenAttribute(new DataName("no-value")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        MarkupGrammar.TokenValue("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			    };
			const string expected = @"<root whitespace="" this contains whitespace "" no-value anyQuotedText=""/\"+"\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A"+@"&#x8;&#xC;&#xA;&#xD;&#x9;`1~!@#$%^&amp;*()_+-=[]{}|;:',./&lt;&gt;?"" />";

			var formatter = new HtmlFormatter(new DataWriterSettings()) { CanonicalForm = false };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_MultipleAttributesCanonicalForm_ReturnsCanonicalMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementVoid(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenValue(" this contains whitespace "),
			        MarkupGrammar.TokenAttribute(new DataName("no-value")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        MarkupGrammar.TokenValue("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?")
			    };
			const string expected = @"<root anyQuotedText=""/\"+"\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A"+@"&#x8;&#xC;&#xA;&#xD;&#x9;`1~!@#$%^&amp;*()_+-=[]{}|;:&apos;,./&lt;>?"" no-value whitespace="" this contains whitespace ""></root>";

			var formatter = new HtmlFormatter(new DataWriterSettings()) { CanonicalForm = true };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Simple Attribute Tests

		#region Text Content Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlEntityLt_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue("<")
			    };
			const string expected = @"&lt;";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlEntityB_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue("B")
			    };
			const string expected = @"B";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlEntityHex_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue("\uABCD")
			    };
			const string expected = "\uABCD";

			var formatter = new HtmlFormatter(new DataWriterSettings()) { EncodeNonAscii=false };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlEntityHexEncodeNonAscii_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue("\uABCD")
			    };
			const string expected = @"&#xABCD;";

			var formatter = new HtmlFormatter(new DataWriterSettings()) { EncodeNonAscii=true };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_HtmlEntityEuro_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue("€")
			    };
			const string expected = "€";

			var formatter = new HtmlFormatter(new DataWriterSettings()) { EncodeNonAscii=false };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_HtmlEntityEuroEncodeNonAscii_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue("€")
			    };
			const string expected = @"&#x20AC;";

			var formatter = new HtmlFormatter(new DataWriterSettings()) { EncodeNonAscii=true };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_EntityWithLeadingText_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue("leading"),
			        MarkupGrammar.TokenValue("&")
			    };
			const string expected = @"leading&amp;";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_EntityWithTrailingText_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue("&"),
			        MarkupGrammar.TokenValue("trailing")
			    };
			const string expected = @"&amp;trailing";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_MixedEntities_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue(@"there should <b>e decoded chars & inside this text")
			    };
			const string expected = @"there should &lt;b&gt;e decoded chars &amp; inside this text";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Text Content Tests

		#region Mixed Content Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_HtmlContent_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("div")),
			        MarkupGrammar.TokenAttribute(new DataName("class")),
			        MarkupGrammar.TokenValue("content"),
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenAttribute(new DataName("style")),
			        MarkupGrammar.TokenValue("color:red"),
			        MarkupGrammar.TokenElementBegin(new DataName("strong")),
			        MarkupGrammar.TokenValue("Lorem ipsum"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue(" dolor sit amet, "),
			        MarkupGrammar.TokenElementBegin(new DataName("i")),
			        MarkupGrammar.TokenValue("consectetur"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue(" adipiscing elit."),
			        MarkupGrammar.TokenElementEnd,
					MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<div class=""content""><p style=""color:red""><strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.</p></div>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_HtmlContentPrettyPrinted_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("div")),
			        MarkupGrammar.TokenAttribute(new DataName("class")),
			        MarkupGrammar.TokenValue("content"),
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenAttribute(new DataName("style")),
			        MarkupGrammar.TokenValue("color:red"),
			        MarkupGrammar.TokenValue("\r\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("strong")),
			        MarkupGrammar.TokenValue("Lorem ipsum"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue(" dolor sit amet, "),
			        MarkupGrammar.TokenElementBegin(new DataName("i")),
			        MarkupGrammar.TokenValue("consectetur"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue(" adipiscing elit.\r\n\t"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n"),
					MarkupGrammar.TokenElementEnd,
			    };
			const string expected =
@"<div class=""content"">
	<p style=""color:red"">
		<strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.
	</p>
</div>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Mixed Content Tests

		#region Error Recovery Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_UnclosedOpenTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root"))
			    };
			const string expected = @"<root>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_UnopenedCloseTag_ReturnsMarkup()
		{
			var input = new []
				{
					MarkupGrammar.TokenElementEnd
				};
			const string expected = @"";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_OverlappingTags_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed")),
			        MarkupGrammar.TokenElementBegin(new DataName("even")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };
			const string expected = @"<odd><auto-closed><even></even></auto-closed></odd>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_OverlappingTagsAutoBalanced_ReturnsMarkupAsIs()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed")),
			        MarkupGrammar.TokenElementBegin(new DataName("even")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };
			const string expected = @"<odd><auto-closed><even></even></auto-closed></odd>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_OverlappingNamespacedTagsErrorRecovery_ReturnsMarkupAsIs()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd", "a", "http://example.com/odd/a")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        MarkupGrammar.TokenElementBegin(new DataName("even", "c", "http://example.com/even/c")),
					MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<a:odd xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns:c=""http://example.com/even/c""></c:even></b:auto-closed></a:odd>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_OverlappingNamespacedTagsErrorRecovery_ReturnsAutoBalanced()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd", "a", "http://example.com/odd/a")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        MarkupGrammar.TokenElementBegin(new DataName("even", "c", "http://example.com/even/c")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };
			const string expected = @"<a:odd xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns:c=""http://example.com/even/c""></c:even></b:auto-closed></a:odd>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Error Recovery Tests

		#region Unparsed Block Tests Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
			    };
			const string expected = @"<?xml version=""1.0""?>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlComment_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
			    };
			const string expected = @"<!-- a quick note -->";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_MarkupLikeText_ReturnsTextValue()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenValue(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };
			const string expected = @"value&gt;""0"" &amp;&amp; value&lt;""10"" ?""valid"":""error""";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_MathML_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenValue(@"You can add a string to a number, but this stringifies the number:"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n"),
			        MarkupGrammar.TokenElementBegin(new DataName("math")),
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenValue(@"x<y"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenValue(@"+"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mn")),
			        MarkupGrammar.TokenValue(@"3"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenValue(@"="),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenValue(@"x<y3"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n"),
			        MarkupGrammar.TokenElementEnd,
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

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlDocTypeExternal_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!{0}",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };
			const string expected =
@"<!DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		//[Fact(Skip="Embedded DOCTYPE not supported")]
		public void Format_XmlDocTypeLocal_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!{0}",
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

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_AspNetPageDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("%@{0}%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };
			const string expected = @"<%@ Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" %>";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_PhpHelloWorld_ReturnsMarkup()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("html")),
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("head")),
			        MarkupGrammar.TokenValue("\r\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("title")),
			        MarkupGrammar.TokenValue("PHP Test"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("body")),
			        MarkupGrammar.TokenValue("\r\n\t\t"),
			        MarkupGrammar.TokenUnparsed("?{0}?", @"php echo '<p>Hello World</p>'; "),
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n"),
			        MarkupGrammar.TokenElementEnd,
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

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_CodeCommentAroundMarkup_ReturnsSingleUnparsedBlock()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("%--{0}--%",
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

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Unparsed Block Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_EmptyInput_ReturnsEmptySequence()
		{
			var input = new Token<MarkupTokenType>[0];
			const string expected = "";

			var formatter = new HtmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
