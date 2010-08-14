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
	public class JsonMLInTransformerTests
	{
		#region Constants

		private const string TraitName = "JsonML";
		private const string TraitValue = "InTransformer";

		#endregion Constants

		#region Simple Single Element Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleOpenCloseTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("root"),
			        CommonGrammar.TokenArrayEnd
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("root"),
			        CommonGrammar.TokenArrayEnd
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
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
			        MarkupGrammar.TokenElementBegin(new DataName("root", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd,
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("root"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns"),
					CommonGrammar.TokenPrimitive("http://example.com/schema"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacePrefixTag_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", "prefix", "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd,
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("prefix:root"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns:prefix"),
					CommonGrammar.TokenPrimitive("http://example.com/schema"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("foo"),
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("child"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns"),
					CommonGrammar.TokenPrimitive("http://example.com/schema"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenPrimitive("value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("foo"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns"),
					CommonGrammar.TokenPrimitive("http://example.org"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("child"),
					CommonGrammar.TokenPrimitive("value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("bar:foo"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns:bar"),
					CommonGrammar.TokenPrimitive("http://example.org"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("bar:child"),
					CommonGrammar.TokenPrimitive("value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive("text value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("foo"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns"),
					CommonGrammar.TokenPrimitive("http://json.org"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("child"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns"),
					CommonGrammar.TokenPrimitive("http://jsonfx.net"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenPrimitive("text value"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive("value")
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("foo"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("blah:key"),
					CommonGrammar.TokenPrimitive("value"),
					CommonGrammar.TokenProperty("xmlns"),
					CommonGrammar.TokenPrimitive("http://example.org"),
					CommonGrammar.TokenProperty("xmlns:blah"),
					CommonGrammar.TokenPrimitive("http://example.org"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive("this should be inner"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementBegin(new DataName("middle-2", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenPrimitive("this should be outer"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("outer"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns"),
					CommonGrammar.TokenPrimitive("http://example.org/outer"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("middle-1"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns"),
					CommonGrammar.TokenPrimitive("http://example.org/inner"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("inner"),
					CommonGrammar.TokenPrimitive("this should be inner"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("middle-2"),
					CommonGrammar.TokenPrimitive("this should be outer"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("one"),
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("two"),
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("three"),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
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
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("attrName")),
			        MarkupGrammar.TokenPrimitive("attrValue"),
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("root"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("attrName")),
					CommonGrammar.TokenPrimitive("attrValue"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("root"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("noValue")),
					CommonGrammar.TokenPrimitive(String.Empty),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("root"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("noValue")),
					CommonGrammar.TokenPrimitive(String.Empty),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("root"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("emptyValue")),
					CommonGrammar.TokenPrimitive(String.Empty),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive(" extra whitespace around quote delims "),
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("root"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("white")),
					CommonGrammar.TokenPrimitive(" extra whitespace around quote delims "),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenPrimitive(String.Empty),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenPrimitive(" this contains whitespace "),
			        MarkupGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        MarkupGrammar.TokenPrimitive("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("root"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("no-value")),
					CommonGrammar.TokenPrimitive(String.Empty),
					CommonGrammar.TokenProperty(new DataName("whitespace")),
					CommonGrammar.TokenPrimitive(" this contains whitespace "),
					CommonGrammar.TokenProperty(new DataName("anyQuotedText")),
					CommonGrammar.TokenPrimitive("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
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
			        MarkupGrammar.TokenPrimitive("<")
			    };
			var expected = new[]
				{
					CommonGrammar.TokenPrimitive("<")
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithLeadingText_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenPrimitive("leading"),
			        MarkupGrammar.TokenPrimitive("&")
			    };
			var expected = new[]
				{
					CommonGrammar.TokenPrimitive("leading&")
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithTrailingText_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenPrimitive("&"),
			        MarkupGrammar.TokenPrimitive("trailing")
			    };
			var expected = new[]
				{
					CommonGrammar.TokenPrimitive("&trailing")
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedEntities_ReturnsSequence()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenPrimitive(@"there should "),
			        MarkupGrammar.TokenPrimitive(@"<"),
			        MarkupGrammar.TokenPrimitive(@"b"),
			        MarkupGrammar.TokenPrimitive(@">"),
			        MarkupGrammar.TokenPrimitive(@"e decoded chars "),
			        MarkupGrammar.TokenPrimitive(@"&"),
			        MarkupGrammar.TokenPrimitive(@" inside this text")
			    };
			var expected = new[]
				{
			        CommonGrammar.TokenPrimitive(@"there should <b>e decoded chars & inside this text")
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
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
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("div"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("class")),
					CommonGrammar.TokenPrimitive("content"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("p"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("style")),
					CommonGrammar.TokenPrimitive("color:red"),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("strong"),
			        CommonGrammar.TokenPrimitive("Lorem ipsum"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive(" dolor sit amet, "),
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("i"),
			        CommonGrammar.TokenPrimitive("consectetur"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive(" adipiscing elit."),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			var expected = new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("div"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("class")),
					CommonGrammar.TokenPrimitive("content"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenPrimitive("\r\n\t"),
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("p"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty(new DataName("style")),
					CommonGrammar.TokenPrimitive("color:red"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenPrimitive("\r\n\t\t"),
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("strong"),
			        CommonGrammar.TokenPrimitive("Lorem ipsum"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive(" dolor sit amet, "),
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive("i"),
			        CommonGrammar.TokenPrimitive("consectetur"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive(" adipiscing elit.\r\n\t"),
					CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n"),
					CommonGrammar.TokenArrayEnd
				};

			var converter = new JsonMLReader.JsonMLInTransformer { PreserveWhitespace=true };
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
			        MarkupGrammar.TokenElementBegin(new DataName("odd")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed")),
			        MarkupGrammar.TokenElementBegin(new DataName("even")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("odd"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("auto-closed"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("even"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("a:odd"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns:a"),
					CommonGrammar.TokenPrimitive("http://example.com/odd/a"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("b:auto-closed"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns:b"),
					CommonGrammar.TokenPrimitive("http://example.com/auto-closed/b"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("c:even"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns:c"),
					CommonGrammar.TokenPrimitive("http://example.com/even/c"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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
					MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementBegin(new DataName("even", "c", "http://example.com/even/c"))
			    };
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("a:odd"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns:a"),
					CommonGrammar.TokenPrimitive("http://example.com/odd/a"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("b:auto-closed"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns:b"),
					CommonGrammar.TokenPrimitive("http://example.com/auto-closed/b"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("c:even"),
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("xmlns:c"),
					CommonGrammar.TokenPrimitive("http://example.com/even/c"),
					CommonGrammar.TokenObjectEnd,
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenArrayEnd,
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
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
					MarkupGrammar.TokenUnparsed("?", "?", @"xml version=""1.0""")
			    };
			var expected = new[]
			    {
					CommonGrammar.TokenPrimitive(new UnparsedBlock("?", "?", @"xml version=""1.0"""))
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlComment_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!--", "--", @" a quick note ")
			    };
			var expected = new[]
			    {
					CommonGrammar.TokenPrimitive(new UnparsedBlock("!--", "--", @" a quick note "))
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_TextValue_ReturnsTextValue()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenPrimitive(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };
			var expected = new[]
			    {
					CommonGrammar.TokenPrimitive(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedTextValues_ReturnsTextValue()
		{
			var input = new[]
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
			var expected = new[]
			    {
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("p"),
			        CommonGrammar.TokenPrimitive(@"You can add a string to a number, but this stringifies the number:"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("math"),
			        CommonGrammar.TokenPrimitive("\r\n\t"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("ms"),
			        CommonGrammar.TokenPrimitive(@"x<y"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n\t"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("mo"),
			        CommonGrammar.TokenPrimitive(@"+"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n\t"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("mn"),
			        CommonGrammar.TokenPrimitive(@"3"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n\t"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("mo"),
			        CommonGrammar.TokenPrimitive(@"="),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n\t"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("ms"),
			        CommonGrammar.TokenPrimitive(@"x<y3"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n"),
			        CommonGrammar.TokenArrayEnd
			    };

			var converter = new JsonMLReader.JsonMLInTransformer { PreserveWhitespace=true };
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDocTypeExternal_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!", "",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };
			var expected = new[]
			    {
					CommonGrammar.TokenPrimitive(new UnparsedBlock("!", "",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"""))
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AspNetPageDeclaration_ReturnsUnparsed()
		{
			var input = new[]
			    {
			        MarkupGrammar.TokenUnparsed("%@", "%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };
			var expected = new[]
			    {
					CommonGrammar.TokenPrimitive(new UnparsedBlock("%@", "%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" "))
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_PhpHelloWorld_ReturnsSequence()
		{
			var input = new[]
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
			var expected = new[]
			    {
					CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("html"),
			        CommonGrammar.TokenPrimitive("\r\n\t"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("head"),
			        CommonGrammar.TokenPrimitive("\r\n\t\t"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("title"),
			        CommonGrammar.TokenPrimitive("PHP Test"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n\t"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n\t"),
			        CommonGrammar.TokenArrayBeginUnnamed,
			        CommonGrammar.TokenPrimitive("body"),
			        CommonGrammar.TokenPrimitive("\r\n\t\t"),
					CommonGrammar.TokenPrimitive(new UnparsedBlock("?", "?", @"php echo '<p>Hello World</p>'; ")),
			        CommonGrammar.TokenPrimitive("\r\n\t"),
			        CommonGrammar.TokenArrayEnd,
			        CommonGrammar.TokenPrimitive("\r\n"),
			        CommonGrammar.TokenArrayEnd,
			    };

			var converter = new JsonMLReader.JsonMLInTransformer { PreserveWhitespace=true };
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_CodeCommentAroundMarkup_ReturnsSingleUnparsedBlock()
		{
			var input = new[]
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
			var expected = new[]
			    {
					CommonGrammar.TokenPrimitive(new UnparsedBlock("%--", "--%",
@"
<html>
	<body style=""color:lime"">
		<!-- not much to say here -->
	</body>
</html>
"))
			    };

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

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

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			var input = new Token<MarkupTokenType>[0];
			var expected = new Token<CommonTokenType>[0];

			var converter = new JsonMLReader.JsonMLInTransformer();
			var actual = converter.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
