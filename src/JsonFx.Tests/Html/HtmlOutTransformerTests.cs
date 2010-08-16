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
using System.Collections.Generic;
using System.Linq;

using JsonFx.Markup;
using JsonFx.Model;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Html
{
	public class HtmlOutTransformerTests
	{
		#region Constants

		private const string TraitName = "HTML";
		private const string TraitValue = "OutTransformer";

		#endregion Constants

		#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd
			};

			var expected = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("div")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenArrayEnd
			};

			var expected = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("div")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenPrimitive(null),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayMultiItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(0),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenArrayEnd
			};

			var expected = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("div")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenPrimitive(0),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenPrimitive(null),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenPrimitive(false),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenPrimitive(true),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayNestedDeeply_ReturnsExpectedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("Not too deep"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd
			};

			var expected = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("div")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenElementBegin(new DataName("ol")),
				MarkupGrammar.TokenElementBegin(new DataName("li")),
				MarkupGrammar.TokenPrimitive("Not too deep"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectEmpty_RendersEmptyObject()
		{
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd
			};

			var expected = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("div")),
				MarkupGrammar.TokenElementBegin(new DataName("dl")),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectOneProperty_RendersSimpleObject()
		{
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("key"),
				ModelGrammar.TokenPrimitive("value"),
				ModelGrammar.TokenObjectEnd
			};

			var expected = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("div")),
				MarkupGrammar.TokenElementBegin(new DataName("dl")),
				MarkupGrammar.TokenElementBegin(new DataName("dt")),
				MarkupGrammar.TokenPrimitive(new DataName("key")),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("dd")),
				MarkupGrammar.TokenPrimitive("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamedObjectOneProperty_RendersSimpleObject()
		{
			var input = new[]
			{
				ModelGrammar.TokenObjectBegin("Yada"),
				ModelGrammar.TokenProperty("key"),
				ModelGrammar.TokenPrimitive("value"),
				ModelGrammar.TokenObjectEnd
			};

			var expected = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("div")),
				MarkupGrammar.TokenElementBegin(new DataName("dl")),
				MarkupGrammar.TokenAttribute(new DataName("title")),
				MarkupGrammar.TokenPrimitive(new DataName("Yada")),
				MarkupGrammar.TokenElementBegin(new DataName("dt")),
				MarkupGrammar.TokenPrimitive(new DataName("key")),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("dd")),
				MarkupGrammar.TokenPrimitive("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectNested_RendersNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("JSON Test Pattern pass3"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("The outermost value"),
				ModelGrammar.TokenPrimitive("must be an object or array."),
				ModelGrammar.TokenProperty("In this test"),
				ModelGrammar.TokenPrimitive("It is an object."),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenObjectEnd
			};

			var expected = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("div")),
				MarkupGrammar.TokenElementBegin(new DataName("dl")),
				MarkupGrammar.TokenElementBegin(new DataName("dt")),
				MarkupGrammar.TokenPrimitive(new DataName("JSON Test Pattern pass3")),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("dd")),
				MarkupGrammar.TokenElementBegin(new DataName("dl")),
				MarkupGrammar.TokenElementBegin(new DataName("dt")),
				MarkupGrammar.TokenPrimitive(new DataName("The outermost value")),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("dd")),
				MarkupGrammar.TokenPrimitive("must be an object or array."),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("dt")),
				MarkupGrammar.TokenPrimitive(new DataName("In this test")),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("dd")),
				MarkupGrammar.TokenPrimitive("It is an object."),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region Namespace Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamespacedObjectOneProperty_CorrectlyEmitsNamespace()
		{
			var input = new[]
			{
				ModelGrammar.TokenObjectBegin("foo"),
				ModelGrammar.TokenProperty(new DataName("key", String.Empty, "http://json.org")),
				ModelGrammar.TokenPrimitive("value"),
				ModelGrammar.TokenObjectEnd
			};

			var expected = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("div")),
				MarkupGrammar.TokenElementBegin(new DataName("dl")),
				MarkupGrammar.TokenAttribute(new DataName("title")),
				MarkupGrammar.TokenPrimitive(new DataName("foo")),
				MarkupGrammar.TokenElementBegin(new DataName("dt")),
				MarkupGrammar.TokenPrimitive(new DataName("key", String.Empty, "http://json.org")),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("dd")),
				MarkupGrammar.TokenPrimitive("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_EmptyInput_RendersEmptyString()
		{
			var input = Enumerable.Empty<Token<ModelTokenType>>();

			var expected = Enumerable.Empty<Token<MarkupTokenType>>();

			var transformer = new HtmlOutTransformer();
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<ModelTokenType>>)null;

			var transformer = new HtmlOutTransformer();

			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var actual = transformer.Transform(input).ToArray();
				});

			// verify exception is coming from expected param
			Assert.Equal("input", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
