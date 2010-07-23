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
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
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
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        XmlGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        XmlGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			    };
			const string expected = @"<root xmlns=""http://example.com/schema""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamespacePrefixTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenPrefixBegin("prefix", "http://example.com/schema"),
			        XmlGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        XmlGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        XmlGrammar.TokenPrefixEnd("prefix", "http://example.com/schema"),
			    };
			const string expected = @"<prefix:root xmlns:prefix=""http://example.com/schema""></prefix:root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamespacedChildTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("foo")),
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        XmlGrammar.TokenElementBegin(new DataName("child", "http://example.com/schema")),
			        XmlGrammar.TokenText("value"),
			        XmlGrammar.TokenElementEnd(new DataName("child", "http://example.com/schema")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			        XmlGrammar.TokenElementEnd(new DataName("foo"))
			    };
			const string expected = @"<foo><child xmlns=""http://example.com/schema"">value</child></foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ParentAndChildShareDefaultNamespace_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenPrefixBegin("", "http://example.org"),
			        XmlGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        XmlGrammar.TokenText("value"),
			        XmlGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        XmlGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.org")
			    };
			const string expected = @"<foo xmlns=""http://example.org""><child>value</child></foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ParentAndChildSharePrefixedNamespace_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenPrefixBegin("bar", "http://example.org"),
			        XmlGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        XmlGrammar.TokenText("value"),
			        XmlGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        XmlGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenPrefixEnd("bar", "http://example.org")
			    };
			const string expected = @"<bar:foo xmlns:bar=""http://example.org""><bar:child>value</bar:child></bar:foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ParentAndChildDifferentDefaultNamespaces_ReturnsMarkup()
		{
			var input = new[]
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
			const string expected = @"<foo xmlns=""http://json.org""><child xmlns=""http://jsonfx.net"">text value</child></foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
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
			        XmlGrammar.TokenPrefixBegin("", "http://example.org"),
			        XmlGrammar.TokenPrefixBegin("blah", "http://example.org"),
			        XmlGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenAttribute(new DataName("key", "http://example.org")),
			        XmlGrammar.TokenText("value"),
			        XmlGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.org"),
			        XmlGrammar.TokenPrefixEnd("blah", "http://example.org")
			    };
			const string expected = @"<foo xmlns=""http://example.org"" xmlns:blah=""http://example.org"" key=""value""></foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NestedDefaultNamespaces_ReturnsMarkup()
		{
			var input = new[]
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
			const string expected = @"<outer xmlns=""http://example.org/outer""><middle-1 xmlns=""http://example.org/inner""><inner>this should be inner</inner></middle-1><middle-2>this should be outer</middle-2></outer>";

			var formatter = new StaxFormatter(new DataWriterSettings());
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
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("attrName")),
			        XmlGrammar.TokenText("attrValue"),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root attrName=""attrValue""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleEmptyAttributeXmlStyle_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("noValue")),
			        XmlGrammar.TokenText(String.Empty),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root noValue=""""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleEmptyAttributeHtmlStyle_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("noValue")),
			        XmlGrammar.TokenText(String.Empty),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root noValue></root>";

			var formatter = new StaxFormatter(new DataWriterSettings()) { Html5EmptyAttributes=true };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleAttributeEmptyValue_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("emptyValue")),
			        XmlGrammar.TokenText(String.Empty),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root emptyValue=""""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_SingleAttributeWhitespace_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("whitespace")),
			        XmlGrammar.TokenText(" this contains whitespace "),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root whitespace="" this contains whitespace ""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_MultipleAttributes_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("root")),
			        XmlGrammar.TokenAttribute(new DataName("no-value")),
			        XmlGrammar.TokenText(String.Empty),
			        XmlGrammar.TokenAttribute(new DataName("whitespace")),
			        XmlGrammar.TokenText(" this contains whitespace "),
			        XmlGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        XmlGrammar.TokenText("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        XmlGrammar.TokenElementEnd(new DataName("root"))
			    };
			const string expected = @"<root anyQuotedText=""/\"+"\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A"+@"&#x8;&#xC;&#xA;&#xD;&#x9;`1~!@#$%^&amp;*()_+-=[]{}|;:',./&lt;&gt;?"" no-value="""" whitespace="" this contains whitespace ""></root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
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
			        XmlGrammar.TokenText("<")
			    };
			const string expected = @"&lt;";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlEntityB_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenText("B")
			    };
			const string expected = @"B";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlEntityHex_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenText("\uABCD")
			    };
			const string expected = "\uABCD";

			var formatter = new StaxFormatter(new DataWriterSettings()) { EncodeNonAscii=false };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlEntityHexEncodeNonAscii_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenText("\uABCD")
			    };
			const string expected = @"&#xABCD;";

			var formatter = new StaxFormatter(new DataWriterSettings()) { EncodeNonAscii=true };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_HtmlEntityEuro_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenText("€")
			    };
			const string expected = "€";

			var formatter = new StaxFormatter(new DataWriterSettings()) { EncodeNonAscii=false };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_HtmlEntityEuroEncodeNonAscii_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenText("€")
			    };
			const string expected = @"&#x20AC;";

			var formatter = new StaxFormatter(new DataWriterSettings()) { EncodeNonAscii=true };
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_EntityWithLeadingText_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenText("leading"),
			        XmlGrammar.TokenText("&")
			    };
			const string expected = @"leading&amp;";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_EntityWithTrailingText_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenText("&"),
			        XmlGrammar.TokenText("trailing")
			    };
			const string expected = @"&amp;trailing";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_MixedEntities_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenText(@"there should <b>e decoded chars & inside this text")
			    };
			const string expected = @"there should &lt;b&gt;e decoded chars &amp; inside this text";

			var formatter = new StaxFormatter(new DataWriterSettings());
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
			const string expected = @"<div class=""content""><p style=""color:red""><strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.</p></div>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_HtmlContentPrettyPrinted_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("div")),
			        XmlGrammar.TokenAttribute(new DataName("class")),
			        XmlGrammar.TokenText("content"),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("p")),
			        XmlGrammar.TokenAttribute(new DataName("style")),
			        XmlGrammar.TokenText("color:red"),
			        XmlGrammar.TokenWhitespace("\r\n\t\t"),
			        XmlGrammar.TokenElementBegin(new DataName("strong")),
			        XmlGrammar.TokenText("Lorem ipsum"),
			        XmlGrammar.TokenElementEnd(new DataName("strong")),
			        XmlGrammar.TokenText(" dolor sit amet, "),
			        XmlGrammar.TokenElementBegin(new DataName("i")),
			        XmlGrammar.TokenText("consectetur"),
			        XmlGrammar.TokenElementEnd(new DataName("i")),
			        XmlGrammar.TokenText(" adipiscing elit.\r\n\t"),
			        XmlGrammar.TokenElementEnd(new DataName("p")),
			        XmlGrammar.TokenWhitespace("\r\n"),
					XmlGrammar.TokenElementEnd(new DataName("div")),
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
		[Trait(TraitName, TraitValue)]
		public void Format_UnclosedOpenTag_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("root"))
			    };
			const string expected = @"<root>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_UnopenedCloseTag_ReturnsMarkup()
		{
			var input = new []
				{
					XmlGrammar.TokenElementEnd(new DataName("foo"))
				};
			const string expected = @"</foo>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_OverlappingTags_ReturnsMarkupAsIs()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("odd")),
			        XmlGrammar.TokenElementBegin(new DataName("auto-closed")),
			        XmlGrammar.TokenElementBegin(new DataName("even")),
			        XmlGrammar.TokenElementEnd(new DataName("odd")),
			        XmlGrammar.TokenElementEnd(new DataName("ignored")),
			        XmlGrammar.TokenElementEnd(new DataName("even"))
			    };
			const string expected = @"<odd><auto-closed><even></odd></ignored></even>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_OverlappingTagsAutoBalanced_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("odd")),
			        XmlGrammar.TokenElementBegin(new DataName("auto-closed")),
			        XmlGrammar.TokenElementBegin(new DataName("even")),
			        XmlGrammar.TokenElementEnd(new DataName("even")),
			        XmlGrammar.TokenElementEnd(new DataName("auto-closed")),
			        XmlGrammar.TokenElementEnd(new DataName("odd"))
			    };
			const string expected = @"<odd><auto-closed><even></even></auto-closed></odd>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_OverlappingNamespacedTagsErrorRecovery_ReturnsMarkupAsIs()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/odd"),
			        XmlGrammar.TokenPrefixBegin("a", "http://example.com/odd/a"),
			        XmlGrammar.TokenElementBegin(new DataName("odd", "http://example.com/odd/a")),
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/auto-closed"),
			        XmlGrammar.TokenPrefixBegin("b", "http://example.com/auto-closed/b"),
			        XmlGrammar.TokenElementBegin(new DataName("auto-closed", "http://example.com/auto-closed/b")),
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/even"),
			        XmlGrammar.TokenPrefixBegin("c", "http://example.com/even/c"),
			        XmlGrammar.TokenElementBegin(new DataName("even", "http://example.com/even/c")),
			        XmlGrammar.TokenElementEnd(new DataName("odd", "http://example.com/odd/a")),
			        // NOTE: skips prefix ending for odd because can't know odd declared them
			        XmlGrammar.TokenElementEnd(new DataName("ignored")),
			        XmlGrammar.TokenElementEnd(new DataName("even", "http://example.com/even/c")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.com/even"),
			        XmlGrammar.TokenPrefixEnd("c", "http://example.com/even/c")
			    };
			const string expected = @"<a:odd xmlns=""http://example.com/odd"" xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns=""http://example.com/auto-closed"" xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns=""http://example.com/even"" xmlns:c=""http://example.com/even/c""></a:odd></ignored></c:even>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_OverlappingNamespacedTagsErrorRecoveryAutoBalanced_ReturnsMarkupAsIs()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/odd"),
			        XmlGrammar.TokenPrefixBegin("a", "http://example.com/odd/a"),
			        XmlGrammar.TokenElementBegin(new DataName("odd", "http://example.com/odd/a")),
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/auto-closed"),
			        XmlGrammar.TokenPrefixBegin("b", "http://example.com/auto-closed/b"),
			        XmlGrammar.TokenElementBegin(new DataName("auto-closed", "http://example.com/auto-closed/b")),
			        XmlGrammar.TokenPrefixBegin("", "http://example.com/even"),
			        XmlGrammar.TokenPrefixBegin("c", "http://example.com/even/c"),
			        XmlGrammar.TokenElementBegin(new DataName("even", "http://example.com/even/c")),
					XmlGrammar.TokenElementEnd(new DataName("even", "http://example.com/even/c")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.com/even"),
			        XmlGrammar.TokenPrefixEnd("c", "http://example.com/even/c"),
			        XmlGrammar.TokenElementEnd(new DataName("auto-closed", "http://example.com/auto-closed/b")),
			        XmlGrammar.TokenPrefixEnd("", "http://example.com/auto-closed"),
			        XmlGrammar.TokenPrefixEnd("b", "http://example.com/auto-closed/b"),
			        XmlGrammar.TokenElementEnd(new DataName("odd", "http://example.com/odd/a")),
					XmlGrammar.TokenPrefixEnd("", "http://example.com/odd"),
			        XmlGrammar.TokenPrefixEnd("a", "http://example.com/odd/a"),
			    };
			const string expected = @"<a:odd xmlns=""http://example.com/odd"" xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns=""http://example.com/auto-closed"" xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns=""http://example.com/even"" xmlns:c=""http://example.com/even/c""></c:even></b:auto-closed></a:odd>";

			var formatter = new StaxFormatter(new DataWriterSettings());
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
			        XmlGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
			    };
			const string expected = @"<?xml version=""1.0""?>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_XmlComment_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
			    };
			const string expected = @"<!-- a quick note -->";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_MarkupLikeText_ReturnsTextValue()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenText(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };
			const string expected = @"value&gt;""0"" &amp;&amp; value&lt;""10"" ?""valid"":""error""";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_MathML_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("p")),
			        XmlGrammar.TokenText(@"You can add a string to a number, but this stringifies the number:"),
			        XmlGrammar.TokenElementEnd(new DataName("p")),
			        XmlGrammar.TokenWhitespace("\r\n"),
			        XmlGrammar.TokenElementBegin(new DataName("math")),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("ms")),
			        XmlGrammar.TokenText(@"x<y"),
			        XmlGrammar.TokenElementEnd(new DataName("ms")),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("mo")),
			        XmlGrammar.TokenText(@"+"),
			        XmlGrammar.TokenElementEnd(new DataName("mo")),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("mn")),
			        XmlGrammar.TokenText(@"3"),
			        XmlGrammar.TokenElementEnd(new DataName("mn")),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("mo")),
			        XmlGrammar.TokenText(@"="),
			        XmlGrammar.TokenElementEnd(new DataName("mo")),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("ms")),
			        XmlGrammar.TokenText(@"x<y3"),
			        XmlGrammar.TokenElementEnd(new DataName("ms")),
			        XmlGrammar.TokenWhitespace("\r\n"),
			        XmlGrammar.TokenElementEnd(new DataName("math")),
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
		[Trait(TraitName, TraitValue)]
		public void Format_XmlDocTypeExternal_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenUnparsed("!{0}",
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
			        XmlGrammar.TokenUnparsed("!{0}",
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
		[Trait(TraitName, TraitValue)]
		public void Format_AspNetPageDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenUnparsed("%@{0}%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };
			const string expected = @"<%@ Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" %>";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_PhpHelloWorld_ReturnsMarkup()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenElementBegin(new DataName("html")),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("head")),
			        XmlGrammar.TokenWhitespace("\r\n\t\t"),
			        XmlGrammar.TokenElementBegin(new DataName("title")),
			        XmlGrammar.TokenText("PHP Test"),
			        XmlGrammar.TokenElementEnd(new DataName("title")),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementEnd(new DataName("head")),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementBegin(new DataName("body")),
			        XmlGrammar.TokenWhitespace("\r\n\t\t"),
			        XmlGrammar.TokenUnparsed("?{0}?", @"php echo '<p>Hello World</p>'; "),
			        XmlGrammar.TokenWhitespace("\r\n\t"),
			        XmlGrammar.TokenElementEnd(new DataName("body")),
			        XmlGrammar.TokenWhitespace("\r\n"),
			        XmlGrammar.TokenElementEnd(new DataName("html")),
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
		[Trait(TraitName, TraitValue)]
		public void Format_CodeCommentAroundMarkup_ReturnsSingleUnparsedBlock()
		{
			var input = new[]
			    {
			        XmlGrammar.TokenUnparsed("%--{0}--%",
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
		[Trait(TraitName, TraitValue)]
		public void Format_EmptyInput_ReturnsEmptySequence()
		{
			var input = new Token<XmlTokenType>[0];
			const string expected = "";

			var formatter = new StaxFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
