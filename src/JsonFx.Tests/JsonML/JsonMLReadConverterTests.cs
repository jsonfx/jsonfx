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

using JsonFx.Markup;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;
using JsonFx.Common;

namespace JsonFx.JsonML
{
	public class JsonMLReadConverterTests
	{
		#region Constants

		private const string TraitName = "JsonML";
		private const string TraitValue = "Deserialization";

		#endregion Constants

		#region Simple Single Element Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleOpenCloseTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("root")),
			        CommonGrammar.TokenArrayEnd
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleVoidTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementVoid(new DataName("root"))
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("root")),
			        CommonGrammar.TokenArrayEnd
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Single Element Tests

		#region Namespace Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DefaultNamespaceTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd(new DataName("root", String.Empty, "http://example.com/schema")),
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root", String.Empty, "http://example.com/schema")),
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacePrefixTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", "prefix", "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd(new DataName("root", "prefix", "http://example.com/schema")),
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root", "prefix", "http://example.com/schema")),
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacedChildTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenText("value"),
			        MarkupGrammar.TokenElementEnd(new DataName("child", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd(new DataName("foo"))
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("child", String.Empty, "http://example.com/schema")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue("value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildShareDefaultNamespace_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenText("value"),
			        MarkupGrammar.TokenElementEnd(new DataName("child", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenElementEnd(new DataName("foo", String.Empty, "http://example.org")),
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo", String.Empty, "http://example.org")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("child", String.Empty, "http://example.org")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue("value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildSharePrefixedNamespace_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", "bar", "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", "bar", "http://example.org")),
			        MarkupGrammar.TokenText("value"),
			        MarkupGrammar.TokenElementEnd(new DataName("child", "bar", "http://example.org")),
			        MarkupGrammar.TokenElementEnd(new DataName("foo", "bar", "http://example.org")),
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo", "bar", "http://example.org")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("child", "bar", "http://example.org")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue("value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildDifferentDefaultNamespaces_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://jsonfx.net")),
			        MarkupGrammar.TokenText("text value"),
			        MarkupGrammar.TokenElementEnd(new DataName("child", String.Empty, "http://jsonfx.net")),
			        MarkupGrammar.TokenElementEnd(new DataName("foo", String.Empty, "http://json.org"))
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo", String.Empty, "http://json.org")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("child", String.Empty, "http://jsonfx.net")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue("text value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DifferentPrefixSameNamespace_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementVoid(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenAttribute(new DataName("key", "blah", "http://example.org")),
			        MarkupGrammar.TokenText("value")
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo", String.Empty, "http://example.org")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("key", "blah", "http://example.org")),
					CommonGrammar.TokenValue("value"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NestedDefaultNamespaces_ReturnsSequence()
		{
			var input = new[]
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
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("outer", String.Empty, "http://example.org/outer")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("middle-1", String.Empty, "http://example.org/inner")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("inner", String.Empty, "http://example.org/inner")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue("this should be inner"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("middle-2", String.Empty, "http://example.org/outer")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue("this should be outer"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UndeclaredPrefixes_ReturnsDefault()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("one")),
			        MarkupGrammar.TokenElementBegin(new DataName("two")),
			        MarkupGrammar.TokenElementBegin(new DataName("three")),
			        MarkupGrammar.TokenElementEnd(new DataName("three")),
			        MarkupGrammar.TokenElementEnd(new DataName("two")),
			        MarkupGrammar.TokenElementEnd(new DataName("one"))
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("one")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("two")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("three")),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Simple Attribute Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttribute_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("attrName")),
			        MarkupGrammar.TokenText("attrValue"),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("attrName")),
					CommonGrammar.TokenValue("attrValue"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeHtmlStyle_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenText(String.Empty),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("noValue")),
					CommonGrammar.TokenValue(String.Empty),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeXmlStyle_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenText(String.Empty),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("noValue")),
					CommonGrammar.TokenValue(String.Empty),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeEmptyValue_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("emptyValue")),
			        MarkupGrammar.TokenText(String.Empty),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("emptyValue")),
					CommonGrammar.TokenValue(String.Empty),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AttributeWhitespaceQuotDelims_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("white")),
			        MarkupGrammar.TokenText(" extra whitespace around quote delims "),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("white")),
					CommonGrammar.TokenValue(" extra whitespace around quote delims "),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultipleAttributes_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("no-value")),
			        MarkupGrammar.TokenText(String.Empty),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenText(" this contains whitespace "),
			        MarkupGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        MarkupGrammar.TokenText("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        MarkupGrammar.TokenElementEnd(new DataName("root"))
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("no-value")),
					CommonGrammar.TokenValue(String.Empty),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenProperty(new DataName("whitespace")),
					CommonGrammar.TokenValue(" this contains whitespace "),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenProperty(new DataName("anyQuotedText")),
					CommonGrammar.TokenValue("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Attribute Tests

		#region Text Content Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityLt_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenText("<")
			    };
			var expected = new[]
				{
					CommonGrammar.TokenValue("<")
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithLeadingText_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenText("leading"),
			        MarkupGrammar.TokenText("&")
			    };
			var expected = new[]
				{
					CommonGrammar.TokenValue("leading&")
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithTrailingText_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenText("&"),
			        MarkupGrammar.TokenText("trailing")
			    };
			var expected = new[]
				{
					CommonGrammar.TokenValue("&trailing")
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedEntities_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenText(@"there should "),
			        MarkupGrammar.TokenText(@"<"),
			        MarkupGrammar.TokenText(@"b"),
			        MarkupGrammar.TokenText(@">"),
			        MarkupGrammar.TokenText(@"e decoded chars "),
			        MarkupGrammar.TokenText(@"&"),
			        MarkupGrammar.TokenText(@" inside this text")
			    };
			var expected = new[]
				{
			        CommonGrammar.TokenValue(@"there should <b>e decoded chars & inside this text")
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Text Content Tests

		#region Mixed Content Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlContent_ReturnsSequence()
		{
			var input = new[]
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
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("div")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("class")),
					CommonGrammar.TokenValue("content"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("p")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("style")),
					CommonGrammar.TokenValue("color:red"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("strong")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("Lorem ipsum"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(" dolor sit amet, "),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("i")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("consectetur"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(" adipiscing elit."),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlContentPrettyPrinted_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("div")),
			        MarkupGrammar.TokenAttribute(new DataName("class")),
			        MarkupGrammar.TokenText("content"),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenAttribute(new DataName("style")),
			        MarkupGrammar.TokenText("color:red"),
			        MarkupGrammar.TokenWhitespace("\r\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("strong")),
			        MarkupGrammar.TokenText("Lorem ipsum"),
			        MarkupGrammar.TokenElementEnd(new DataName("strong")),
			        MarkupGrammar.TokenText(" dolor sit amet, "),
			        MarkupGrammar.TokenElementBegin(new DataName("i")),
			        MarkupGrammar.TokenText("consectetur"),
			        MarkupGrammar.TokenElementEnd(new DataName("i")),
			        MarkupGrammar.TokenText(" adipiscing elit.\r\n\t"),
			        MarkupGrammar.TokenElementEnd(new DataName("p")),
			        MarkupGrammar.TokenWhitespace("\r\n"),
					MarkupGrammar.TokenElementEnd(new DataName("div")),
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("div")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("class")),
					CommonGrammar.TokenValue("content"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("p")),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("style")),
					CommonGrammar.TokenValue("color:red"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t\t"),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("strong")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("Lorem ipsum"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(" dolor sit amet, "),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("i")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("consectetur"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(" adipiscing elit.\r\n\t"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n"),
					CommonGrammar.TokenArrayEnd
				};

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Mixed Content Tests

		#region Error Recovery Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingTags_ReturnsSequenceAsIs()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed")),
			        MarkupGrammar.TokenElementBegin(new DataName("even")),
			        MarkupGrammar.TokenElementEnd(new DataName("odd")),
			        MarkupGrammar.TokenElementEnd(new DataName("ignored")),
			        MarkupGrammar.TokenElementEnd(new DataName("even"))
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("odd")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("auto-closed")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("even")),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingNamespacedTagsErrorRecovery_ReturnsSequenceAsIs()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd", "a", "http://example.com/odd/a")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        MarkupGrammar.TokenElementBegin(new DataName("even", "c", "http://example.com/even/c")),
			        MarkupGrammar.TokenElementEnd(new DataName("odd", "a", "http://example.com/odd/a")),
			        MarkupGrammar.TokenElementEnd(new DataName("ignored", "d", String.Empty)),
			        MarkupGrammar.TokenElementEnd(new DataName("even", "c", "http://example.com/even/c"))
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("odd", "a", "http://example.com/odd/a")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("even", "c", "http://example.com/even/c")),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnclosedTags_AutoCloses()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd", "a", "http://example.com/odd/a")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
					MarkupGrammar.TokenElementEnd(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        MarkupGrammar.TokenElementBegin(new DataName("even", "c", "http://example.com/even/c"))
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("odd", "a", "http://example.com/odd/a")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("even", "c", "http://example.com/even/c")),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Error Recovery Tests

		#region Unparsed Block Tests Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
			    };
			var expected = new[]
			    {
					 new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("?{0}?"), @"xml version=""1.0""")
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlComment_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
			    };
			var expected = new[]
			    {
					 new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("!--{0}--"), @" a quick note ")
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_TextValue_ReturnsTextValue()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenText(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };
			var expected = new[]
			    {
					CommonGrammar.TokenValue(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedTextValues_ReturnsTextValue()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenText(@"You can add a string to a number, but this stringifies the number:"),
			        MarkupGrammar.TokenElementEnd(new DataName("p")),
			        MarkupGrammar.TokenWhitespace("\r\n"),
			        MarkupGrammar.TokenElementBegin(new DataName("math")),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenText(@"x<y"),
			        MarkupGrammar.TokenElementEnd(new DataName("ms")),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenText(@"+"),
			        MarkupGrammar.TokenElementEnd(new DataName("mo")),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mn")),
			        MarkupGrammar.TokenText(@"3"),
			        MarkupGrammar.TokenElementEnd(new DataName("mn")),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenText(@"="),
			        MarkupGrammar.TokenElementEnd(new DataName("mo")),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenText(@"x<y3"),
			        MarkupGrammar.TokenElementEnd(new DataName("ms")),
			        MarkupGrammar.TokenWhitespace("\r\n"),
			        MarkupGrammar.TokenElementEnd(new DataName("math")),
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("p")),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(@"You can add a string to a number, but this stringifies the number:"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n"),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("math")),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("ms")),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(@"x<y"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("mo")),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(@"+"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("mn")),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(@"3"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("mo")),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(@"="),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("ms")),
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue(@"x<y3"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n"),
			        CommonGrammar.TokenArrayEnd
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDocTypeExternal_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!{0}",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };
			var expected = new[]
			    {
					new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("!{0}"),
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AspNetPageDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("%@{0}%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };
			var expected = new[]
			    {
					new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("%@{0}%"), @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_PhpHelloWorld_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("html")),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("head")),
			        MarkupGrammar.TokenWhitespace("\r\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("title")),
			        MarkupGrammar.TokenText("PHP Test"),
			        MarkupGrammar.TokenElementEnd(new DataName("title")),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementEnd(new DataName("head")),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("body")),
			        MarkupGrammar.TokenWhitespace("\r\n\t\t"),
			        MarkupGrammar.TokenUnparsed("?{0}?", @"php echo '<p>Hello World</p>'; "),
			        MarkupGrammar.TokenWhitespace("\r\n\t"),
			        MarkupGrammar.TokenElementEnd(new DataName("body")),
			        MarkupGrammar.TokenWhitespace("\r\n"),
			        MarkupGrammar.TokenElementEnd(new DataName("html")),
			    };
			var expected = new[]
			    {
					CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("html")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("head")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t\t"),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("title")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("PHP Test"),
			        CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("body")),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t\t"),
					CommonGrammar.TokenValueDelim,
			        new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("?{0}?"), @"php echo '<p>Hello World</p>'; "),
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
			        CommonGrammar.TokenValue("\r\n"),
			        CommonGrammar.TokenArrayEnd,
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_CodeCommentAroundMarkup_ReturnsSingleUnparsedBlock()
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
			var expected = new[]
			    {
					new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("%--{0}--%"),
@"
<html>
	<body style=""color:lime"">
		<!-- not much to say here -->
	</body>
</html>
")
			    };

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Unparsed Block Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			var input = new Token<MarkupTokenType>[0];
			var expected = new Token<CommonTokenType>[0];

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			var input = new Token<MarkupTokenType>[0];
			var expected = new Token<CommonTokenType>[0];

			var formatter = new JsonMLReader.JsonMLReadConverter();
			var actual = formatter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
