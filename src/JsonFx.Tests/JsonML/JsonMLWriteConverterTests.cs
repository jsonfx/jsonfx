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

using JsonFx.Common;
using JsonFx.Markup;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.JsonML
{
	public class JsonMLWriteConverterTests
	{
		#region Constants

		private const string TraitName = "JsonML";
		private const string TraitValue = "Serialization";

		#endregion Constants

		#region Simple Single Element Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleOpenCloseTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("root")),
			        CommonGrammar.TokenArrayEnd
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleVoidTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("root")),
			        CommonGrammar.TokenArrayEnd
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementVoid(new DataName("root"))
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

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
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root", String.Empty, "http://example.com/schema")),
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacePrefixTag_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root", "prefix", "http://example.com/schema")),
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", "prefix", "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacedChildTag_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo")),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("child", String.Empty, "http://example.com/schema")),
					CommonGrammar.TokenValue("value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildShareDefaultNamespace_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo", String.Empty, "http://example.org")),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("child", String.Empty, "http://example.org")),
					CommonGrammar.TokenValue("value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildSharePrefixedNamespace_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo", "bar", "http://example.org")),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("child", "bar", "http://example.org")),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", "bar", "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", "bar", "http://example.org")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildDifferentDefaultNamespaces_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo", String.Empty, "http://json.org")),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("child", String.Empty, "http://jsonfx.net")),
					CommonGrammar.TokenValue("text value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://jsonfx.net")),
			        MarkupGrammar.TokenValue("text value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DifferentPrefixSameNamespace_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("foo", String.Empty, "http://example.org")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("key", "blah", "http://example.org")),
					CommonGrammar.TokenValue("value"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementVoid(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenAttribute(new DataName("key", "blah", "http://example.org")),
			        MarkupGrammar.TokenValue("value")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NestedDefaultNamespaces_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("outer", String.Empty, "http://example.org/outer")),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("middle-1", String.Empty, "http://example.org/inner")),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("inner", String.Empty, "http://example.org/inner")),
					CommonGrammar.TokenValue("this should be inner"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("middle-2", String.Empty, "http://example.org/outer")),
					CommonGrammar.TokenValue("this should be outer"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
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
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UndeclaredPrefixes_ReturnsDefault()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("one")),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("two")),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("three")),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("one")),
			        MarkupGrammar.TokenElementBegin(new DataName("two")),
			        MarkupGrammar.TokenElementBegin(new DataName("three")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

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
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("attrName")),
					CommonGrammar.TokenValue("attrValue"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("attrName")),
			        MarkupGrammar.TokenValue("attrValue"),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeHtmlStyle_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("noValue")),
					CommonGrammar.TokenValue(String.Empty),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeXmlStyle_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("noValue")),
					CommonGrammar.TokenValue(String.Empty),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeEmptyValue_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("emptyValue")),
					CommonGrammar.TokenValue(String.Empty),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("emptyValue")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AttributeWhitespaceQuotDelims_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("white")),
					CommonGrammar.TokenValue(" extra whitespace around quote delims "),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("white")),
			        MarkupGrammar.TokenValue(" extra whitespace around quote delims "),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultipleAttributes_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("root")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("no-value")),
					CommonGrammar.TokenValue(String.Empty),
					CommonGrammar.TokenProperty(new DataName("whitespace")),
					CommonGrammar.TokenValue(" this contains whitespace "),
					CommonGrammar.TokenProperty(new DataName("anyQuotedText")),
					CommonGrammar.TokenValue("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("no-value")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenValue(" this contains whitespace "),
			        MarkupGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        MarkupGrammar.TokenValue("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

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
					CommonGrammar.TokenValue("<")
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("<")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithLeadingText_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenValue("leading&")
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("leading&")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithTrailingText_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenValue("&trailing")
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("&trailing")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedEntities_ReturnsSequence()
		{
			var input = new[]
				{
			        CommonGrammar.TokenValue(@"there should <b>e decoded chars & inside this text")
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue(@"there should <b>e decoded chars & inside this text")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

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
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("div")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("class")),
					CommonGrammar.TokenValue("content"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("p")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("style")),
					CommonGrammar.TokenValue("color:red"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("strong")),
			        CommonGrammar.TokenValue("Lorem ipsum"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue(" dolor sit amet, "),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("i")),
			        CommonGrammar.TokenValue("consectetur"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue(" adipiscing elit."),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
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

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlContentPrettyPrinted_ReturnsSequence()
		{
			var input = new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("div")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("class")),
					CommonGrammar.TokenValue("content"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenValue("\r\n\t"),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("p")),
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty(new DataName("style")),
					CommonGrammar.TokenValue("color:red"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenValue("\r\n\t\t"),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("strong")),
			        CommonGrammar.TokenValue("Lorem ipsum"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue(" dolor sit amet, "),
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(new DataName("i")),
			        CommonGrammar.TokenValue("consectetur"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue(" adipiscing elit.\r\n\t"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n"),
					CommonGrammar.TokenArrayEnd
				};
			var expected = new[]
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

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

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
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("odd")),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("auto-closed")),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("even")),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed")),
			        MarkupGrammar.TokenElementBegin(new DataName("even")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingNamespacedTagsErrorRecovery_ReturnsSequenceAsIs()
		{
			var input = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("odd", "a", "http://example.com/odd/a")),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("even", "c", "http://example.com/even/c")),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd", "a", "http://example.com/odd/a")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        MarkupGrammar.TokenElementBegin(new DataName("even", "c", "http://example.com/even/c")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnclosedTags_AutoCloses()
		{
			var input = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("odd", "a", "http://example.com/odd/a")),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("even", "c", "http://example.com/even/c")),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd", "a", "http://example.com/odd/a")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
					MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementBegin(new DataName("even", "c", "http://example.com/even/c")),
					MarkupGrammar.TokenElementEnd,
					MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

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
					 new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("?{0}?"), @"xml version=""1.0""")
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlComment_ReturnsUnparsed()
		{
			var input = new[]
			    {
					 new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("!--{0}--"), @" a quick note ")
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_TextValue_ReturnsTextValue()
		{
			var input = new[]
			    {
					CommonGrammar.TokenValue(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedTextValues_ReturnsTextValue()
		{
			var input = new[]
			    {
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("p")),
			        CommonGrammar.TokenValue(@"You can add a string to a number, but this stringifies the number:"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n"),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("math")),
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("ms")),
			        CommonGrammar.TokenValue(@"x<y"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("mo")),
			        CommonGrammar.TokenValue(@"+"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("mn")),
			        CommonGrammar.TokenValue(@"3"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("mo")),
			        CommonGrammar.TokenValue(@"="),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("ms")),
			        CommonGrammar.TokenValue(@"x<y3"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n"),
			        CommonGrammar.TokenArrayEnd
			    };
			var expected = new[]
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

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDocTypeExternal_ReturnsUnparsed()
		{
			var input = new[]
			    {
					new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("!{0}"),
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!{0}",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AspNetPageDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
					new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("%@{0}%"), @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("%@{0}%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_PhpHelloWorld_ReturnsSequence()
		{
			var input = new[]
			    {
					CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("html")),
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("head")),
			        CommonGrammar.TokenValue("\r\n\t\t"),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("title")),
			        CommonGrammar.TokenValue("PHP Test"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayBeginNoName,
			        CommonGrammar.TokenValue(new DataName("body")),
			        CommonGrammar.TokenValue("\r\n\t\t"),
			        new Token<CommonTokenType>(CommonTokenType.Primitive, new DataName("?{0}?"), @"php echo '<p>Hello World</p>'; "),
			        CommonGrammar.TokenValue("\r\n\t"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenValue("\r\n"),
			        CommonGrammar.TokenArrayEnd,
			    };
			var expected = new[]
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

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_CodeCommentAroundMarkup_ReturnsSingleUnparsedBlock()
		{
			var input = new[]
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
			var expected = new[]
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

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Unparsed Block Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			var input = new Token<CommonTokenType>[0];
			var expected = new Token<MarkupTokenType>[0];

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			var input = new Token<CommonTokenType>[0];
			var expected = new Token<MarkupTokenType>[0];

			var converter = new JsonMLWriter.JsonMLWriteConverter();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
