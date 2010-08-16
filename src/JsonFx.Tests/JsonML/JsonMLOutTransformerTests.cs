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
using JsonFx.Model;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.JsonML
{
	public class JsonMLOutTransformerTests
	{
		#region Constants

		private const string TraitName = "JsonML";
		private const string TraitValue = "OutTransformer";

		#endregion Constants

		#region Simple Single Element Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleOpenCloseTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("root")),
			        ModelGrammar.TokenArrayEnd
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleVoidTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("root")),
			        ModelGrammar.TokenArrayEnd
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
					MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleVoidTagWithAttributes_ReturnsSequence()
		{
			var input = new[]
			    {
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("root")),
			        ModelGrammar.TokenObjectBeginUnnamed,
			        ModelGrammar.TokenProperty(new DataName("key")),
			        ModelGrammar.TokenPrimitive("value"),
			        ModelGrammar.TokenObjectEnd,
			        ModelGrammar.TokenArrayEnd
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("key")),
			        MarkupGrammar.TokenPrimitive("value"),
					MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
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
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("root", String.Empty, "http://example.com/schema")),
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", String.Empty, "http://example.com/schema")),
					MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacePrefixTag_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("root", "prefix", "http://example.com/schema")),
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", "prefix", "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacedChildTag_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("foo")),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("child", String.Empty, "http://example.com/schema")),
					ModelGrammar.TokenPrimitive("value"),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenPrimitive("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildShareDefaultNamespace_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("foo", String.Empty, "http://example.org")),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("child", String.Empty, "http://example.org")),
					ModelGrammar.TokenPrimitive("value"),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenPrimitive("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildSharePrefixedNamespace_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("foo", "bar", "http://example.org")),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("child", "bar", "http://example.org")),
			        ModelGrammar.TokenPrimitive("value"),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", "bar", "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", "bar", "http://example.org")),
			        MarkupGrammar.TokenPrimitive("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildDifferentDefaultNamespaces_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("foo", String.Empty, "http://json.org")),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("child", String.Empty, "http://jsonfx.net")),
					ModelGrammar.TokenPrimitive("text value"),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://jsonfx.net")),
			        MarkupGrammar.TokenPrimitive("text value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DifferentPrefixSameNamespace_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("foo", String.Empty, "http://example.org")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("key", "blah", "http://example.org")),
					ModelGrammar.TokenPrimitive("value"),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenAttribute(new DataName("key", "blah", "http://example.org")),
			        MarkupGrammar.TokenPrimitive("value"),
					MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NestedDefaultNamespaces_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("outer", String.Empty, "http://example.org/outer")),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("middle-1", String.Empty, "http://example.org/inner")),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("inner", String.Empty, "http://example.org/inner")),
					ModelGrammar.TokenPrimitive("this should be inner"),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("middle-2", String.Empty, "http://example.org/outer")),
					ModelGrammar.TokenPrimitive("this should be outer"),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("outer", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenElementBegin(new DataName("middle-1", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenElementBegin(new DataName("inner", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenPrimitive("this should be inner"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementBegin(new DataName("middle-2", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenPrimitive("this should be outer"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UndeclaredPrefixes_ReturnsDefault()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("one")),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("two")),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("three")),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd
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

			var converter = new JsonMLWriter.JsonMLOutTransformer();
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
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("root")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("attrName")),
					ModelGrammar.TokenPrimitive("attrValue"),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("attrName")),
			        MarkupGrammar.TokenPrimitive("attrValue"),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeHtmlStyle_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("root")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("noValue")),
					ModelGrammar.TokenPrimitive(String.Empty),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenPrimitive(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeXmlStyle_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("root")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("noValue")),
					ModelGrammar.TokenPrimitive(String.Empty),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenPrimitive(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeEmptyValue_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("root")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("emptyValue")),
					ModelGrammar.TokenPrimitive(String.Empty),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("emptyValue")),
			        MarkupGrammar.TokenPrimitive(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AttributeWhitespaceQuotDelims_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("root")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("white")),
					ModelGrammar.TokenPrimitive(" extra whitespace around quote delims "),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("white")),
			        MarkupGrammar.TokenPrimitive(" extra whitespace around quote delims "),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultipleAttributes_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("root")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("no-value")),
					ModelGrammar.TokenPrimitive(String.Empty),
					ModelGrammar.TokenProperty(new DataName("whitespace")),
					ModelGrammar.TokenPrimitive(" this contains whitespace "),
					ModelGrammar.TokenProperty(new DataName("anyQuotedText")),
					ModelGrammar.TokenPrimitive("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("no-value")),
			        MarkupGrammar.TokenPrimitive(String.Empty),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenPrimitive(" this contains whitespace "),
			        MarkupGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        MarkupGrammar.TokenPrimitive("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        MarkupGrammar.TokenElementEnd
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
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
					ModelGrammar.TokenPrimitive("<")
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenPrimitive("<")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithLeadingText_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenPrimitive("leading&")
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenPrimitive("leading&")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithTrailingText_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenPrimitive("&trailing")
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenPrimitive("&trailing")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedEntities_ReturnsSequence()
		{
			var input = new[]
				{
			        ModelGrammar.TokenPrimitive(@"there should <b>e decoded chars & inside this text")
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenPrimitive(@"there should <b>e decoded chars & inside this text")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
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
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("div")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("class")),
					ModelGrammar.TokenPrimitive("content"),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("p")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("style")),
					ModelGrammar.TokenPrimitive("color:red"),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("strong")),
			        ModelGrammar.TokenPrimitive("Lorem ipsum"),
					ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive(" dolor sit amet, "),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("i")),
			        ModelGrammar.TokenPrimitive("consectetur"),
					ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive(" adipiscing elit."),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("div")),
			        MarkupGrammar.TokenAttribute(new DataName("class")),
			        MarkupGrammar.TokenPrimitive("content"),
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenAttribute(new DataName("style")),
			        MarkupGrammar.TokenPrimitive("color:red"),
			        MarkupGrammar.TokenElementBegin(new DataName("strong")),
			        MarkupGrammar.TokenPrimitive("Lorem ipsum"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive(" dolor sit amet, "),
			        MarkupGrammar.TokenElementBegin(new DataName("i")),
			        MarkupGrammar.TokenPrimitive("consectetur"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive(" adipiscing elit."),
			        MarkupGrammar.TokenElementEnd,
					MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlContentPrettyPrinted_ReturnsSequence()
		{
			var input = new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("div")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("class")),
					ModelGrammar.TokenPrimitive("content"),
					ModelGrammar.TokenObjectEnd,
			        ModelGrammar.TokenPrimitive("\r\n\t"),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("p")),
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty(new DataName("style")),
					ModelGrammar.TokenPrimitive("color:red"),
					ModelGrammar.TokenObjectEnd,
			        ModelGrammar.TokenPrimitive("\r\n\t\t"),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("strong")),
			        ModelGrammar.TokenPrimitive("Lorem ipsum"),
					ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive(" dolor sit amet, "),
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(new DataName("i")),
			        ModelGrammar.TokenPrimitive("consectetur"),
					ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive(" adipiscing elit.\r\n\t"),
					ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n"),
					ModelGrammar.TokenArrayEnd
				};
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("div")),
			        MarkupGrammar.TokenAttribute(new DataName("class")),
			        MarkupGrammar.TokenPrimitive("content"),
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenAttribute(new DataName("style")),
			        MarkupGrammar.TokenPrimitive("color:red"),
			        MarkupGrammar.TokenPrimitive("\r\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("strong")),
			        MarkupGrammar.TokenPrimitive("Lorem ipsum"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive(" dolor sit amet, "),
			        MarkupGrammar.TokenElementBegin(new DataName("i")),
			        MarkupGrammar.TokenPrimitive("consectetur"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive(" adipiscing elit.\r\n\t"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n"),
					MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
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
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("odd")),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("auto-closed")),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("even")),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenArrayEnd,
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

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingNamespacedTagsErrorRecovery_ReturnsSequenceAsIs()
		{
			var input = new[]
			    {
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("odd", "a", "http://example.com/odd/a")),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("even", "c", "http://example.com/even/c")),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenArrayEnd,
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

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnclosedTags_AutoCloses()
		{
			var input = new[]
			    {
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("odd", "a", "http://example.com/odd/a")),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("even", "c", "http://example.com/even/c")),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenArrayEnd,
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

			var converter = new JsonMLWriter.JsonMLOutTransformer();
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
					 ModelGrammar.TokenPrimitive(new UnparsedBlock("?", "?", @"xml version=""1.0"""))
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("?", "?", @"xml version=""1.0""")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlComment_ReturnsUnparsed()
		{
			var input = new[]
			    {
					 ModelGrammar.TokenPrimitive(new UnparsedBlock("!--", "--", @" a quick note "))
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!--", "--", @" a quick note ")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_TextValue_ReturnsTextValue()
		{
			var input = new[]
			    {
					ModelGrammar.TokenPrimitive(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenPrimitive(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedTextValues_ReturnsTextValue()
		{
			var input = new[]
			    {
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("p")),
			        ModelGrammar.TokenPrimitive(@"You can add a string to a number, but this stringifies the number:"),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n"),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("math")),
			        ModelGrammar.TokenPrimitive("\r\n\t"),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("ms")),
			        ModelGrammar.TokenPrimitive(@"x<y"),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n\t"),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("mo")),
			        ModelGrammar.TokenPrimitive(@"+"),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n\t"),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("mn")),
			        ModelGrammar.TokenPrimitive(@"3"),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n\t"),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("mo")),
			        ModelGrammar.TokenPrimitive(@"="),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n\t"),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("ms")),
			        ModelGrammar.TokenPrimitive(@"x<y3"),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n"),
			        ModelGrammar.TokenArrayEnd
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenPrimitive(@"You can add a string to a number, but this stringifies the number:"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n"),
			        MarkupGrammar.TokenElementBegin(new DataName("math")),
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenPrimitive(@"x<y"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenPrimitive(@"+"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mn")),
			        MarkupGrammar.TokenPrimitive(@"3"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenPrimitive(@"="),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenPrimitive(@"x<y3"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n"),
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDocTypeExternal_ReturnsUnparsed()
		{
			var input = new[]
			    {
					ModelGrammar.TokenPrimitive(new UnparsedBlock("!", "",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"""))
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!", "",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AspNetPageDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
					ModelGrammar.TokenPrimitive(new UnparsedBlock("%@", "%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" "))
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("%@", "%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_PhpHelloWorld_ReturnsSequence()
		{
			var input = new[]
			    {
					ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("html")),
			        ModelGrammar.TokenPrimitive("\r\n\t"),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("head")),
			        ModelGrammar.TokenPrimitive("\r\n\t\t"),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("title")),
			        ModelGrammar.TokenPrimitive("PHP Test"),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n\t"),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n\t"),
			        ModelGrammar.TokenArrayBeginUnnamed,
			        ModelGrammar.TokenPrimitive(new DataName("body")),
			        ModelGrammar.TokenPrimitive("\r\n\t\t"),
					ModelGrammar.TokenPrimitive(new UnparsedBlock("?", "?", @"php echo '<p>Hello World</p>'; ")),
			        ModelGrammar.TokenPrimitive("\r\n\t"),
			        ModelGrammar.TokenArrayEnd,
			        ModelGrammar.TokenPrimitive("\r\n"),
			        ModelGrammar.TokenArrayEnd,
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("html")),
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("head")),
			        MarkupGrammar.TokenPrimitive("\r\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("title")),
			        MarkupGrammar.TokenPrimitive("PHP Test"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("body")),
			        MarkupGrammar.TokenPrimitive("\r\n\t\t"),
			        MarkupGrammar.TokenUnparsed("?", "?", @"php echo '<p>Hello World</p>'; "),
			        MarkupGrammar.TokenPrimitive("\r\n\t"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenPrimitive("\r\n"),
			        MarkupGrammar.TokenElementEnd,
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_CodeCommentAroundMarkup_ReturnsSingleUnparsedBlock()
		{
			var input = new[]
			    {
					ModelGrammar.TokenPrimitive(new UnparsedBlock("%--", "--%",
@"
<html>
	<body style=""color:lime"">
		<!-- not much to say here -->
	</body>
</html>
"))
			    };
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("%--", "--%",
@"
<html>
	<body style=""color:lime"">
		<!-- not much to say here -->
	</body>
</html>
")
			    };

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Unparsed Block Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			var input = new Token<ModelTokenType>[0];
			var expected = new Token<MarkupTokenType>[0];

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			var input = new Token<ModelTokenType>[0];
			var expected = new Token<MarkupTokenType>[0];

			var converter = new JsonMLWriter.JsonMLOutTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
