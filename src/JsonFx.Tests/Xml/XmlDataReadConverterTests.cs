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

using JsonFx.Common;
using JsonFx.Markup;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Xml
{
	public class XmlDataReadConverterTests
	{
		#region Constants

		private const string TraitName = "XML";
		private const string TraitValue = "Deserialization";

		#endregion Constants

		#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayEmpty_ReturnsEmptyArray()
		{
			var expected = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayOneItem_ReturnsExpectedArray()
		{
			var expected = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementVoid(new DataName("object")),
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayMultiItem_ReturnsExpectedArray()
		{
			var expected = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue(0),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenArrayEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("int")),
				MarkupGrammar.TokenValue("0"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementVoid(new DataName("object")),
				MarkupGrammar.TokenElementBegin(new DataName("boolean")),
				MarkupGrammar.TokenValue("false"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("boolean")),
				MarkupGrammar.TokenValue("true"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayNestedDeeply_ReturnsExpectedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var expected = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("Not too deep"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),
				MarkupGrammar.TokenElementBegin(new DataName("array")),

				MarkupGrammar.TokenElementBegin(new DataName("string")),
				MarkupGrammar.TokenValue("Not too deep"),
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

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectEmpty_RendersEmptyObject()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("object")),
				MarkupGrammar.TokenElementEnd
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectOneProperty_RendersSimpleObject()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("object")),
				MarkupGrammar.TokenElementBegin(new DataName("key")),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamedObjectOneProperty_RendersSimpleObject()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin("Yada"),
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("Yada")),
				MarkupGrammar.TokenElementBegin(new DataName("key")),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectNested_RendersNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var expected = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("JSON Test Pattern pass3"),
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("The outermost value"),
				CommonGrammar.TokenValue("must be an object or array."),
				CommonGrammar.TokenProperty("In this test"),
				CommonGrammar.TokenValue("It is an object."),
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("object")),
				MarkupGrammar.TokenElementBegin(new DataName("JSON_x0020_Test_x0020_Pattern_x0020_pass3")),
				MarkupGrammar.TokenElementBegin(new DataName("The_x0020_outermost_x0020_value")),
				MarkupGrammar.TokenValue("must be an object or array."),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementBegin(new DataName("In_x0020_this_x0020_test")),
				MarkupGrammar.TokenValue("It is an object."),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region Namespace Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamespacedObjectOneProperty_CorrectlyEmitsNamespace()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin("foo"),
				CommonGrammar.TokenProperty(new DataName("key", String.Empty, "http://json.org")),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("foo")),
				MarkupGrammar.TokenElementBegin(new DataName("key", String.Empty, "http://json.org")),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectAndPropertyShareNamespace_CorrectlyEmitsNamespace()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin(new DataName("foo", String.Empty, "http://json.org")),
				CommonGrammar.TokenProperty(new DataName("key", String.Empty, "http://json.org")),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
				MarkupGrammar.TokenElementBegin(new DataName("key", String.Empty, "http://json.org")),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamespacedObjectNonNamespacedProperty_CorrectlyEmitsNamespace()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin(new DataName("foo", String.Empty, "http://json.org")),
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
				MarkupGrammar.TokenElementBegin(new DataName("key")),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NamespacedObjectOneDifferentNamespaceProperty_CorrectlyEmitsNamespaces()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin(new DataName("foo", String.Empty, "http://json.org")),
				CommonGrammar.TokenProperty(new DataName("key", String.Empty, "http://jsonfx.net")),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
				MarkupGrammar.TokenElementBegin(new DataName("key", String.Empty, "http://jsonfx.net")),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectAndAttributeShareNamespace_CorrectlyEmitsNamespace()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin(new DataName("foo", String.Empty, "http://json.org")),
				CommonGrammar.TokenProperty(new DataName("key", String.Empty, "http://json.org", true)),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
				MarkupGrammar.TokenAttribute(new DataName("key", String.Empty, "http://json.org", true)),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectAndAttributeDifferentNamespaces_CorrectlyEmitsNamespaces()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin(new DataName("foo", String.Empty, "http://json.org")),
				CommonGrammar.TokenProperty(new DataName("key", String.Empty, "http://jsonfx.net", true)),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
				MarkupGrammar.TokenAttribute(new DataName("key", "q1", "http://jsonfx.net", true)),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NestedObjectsSkippingNamespaces_CorrectlyEmitsNamespaces()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin(new DataName("foo1", String.Empty, "http://json.org")),
				CommonGrammar.TokenProperty(new DataName("key1", String.Empty, "http://jsonfx.net")),

				CommonGrammar.TokenObjectBegin(new DataName("foo2", String.Empty, "http://json.org")),
				CommonGrammar.TokenProperty(new DataName("key2", String.Empty, "http://jsonfx.net")),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd,

				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("foo1", String.Empty, "http://json.org")),
				MarkupGrammar.TokenElementBegin(new DataName("key1", String.Empty, "http://jsonfx.net")),
				MarkupGrammar.TokenElementBegin(new DataName("key2", String.Empty, "http://jsonfx.net")),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NestedObjectsAlternatingNamespaces_CorrectlyEmitsNamespaces()
		{
			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin(new DataName("foo1", String.Empty, "http://json.org")),
				CommonGrammar.TokenProperty(new DataName("key1", String.Empty, "http://jsonfx.net")),

				CommonGrammar.TokenObjectBegin(new DataName("foo2", String.Empty, "http://jsonfx.net")),
				CommonGrammar.TokenProperty(new DataName("key2", String.Empty, "http://json.org")),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd,

				CommonGrammar.TokenObjectEnd
			};

			var input = new[]
			{
				MarkupGrammar.TokenElementBegin(new DataName("foo1", String.Empty, "http://json.org")),
				MarkupGrammar.TokenElementBegin(new DataName("key1", String.Empty, "http://jsonfx.net")),
				MarkupGrammar.TokenElementBegin(new DataName("key2", String.Empty, "http://json.org")),
				MarkupGrammar.TokenValue("value"),
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
				MarkupGrammar.TokenElementEnd,
			};

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_EmptyInput_RendersEmptyString()
		{
			var expected = Enumerable.Empty<Token<CommonTokenType>>();

			var input = Enumerable.Empty<Token<MarkupTokenType>>();

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());
			var actual = transformer.Transform(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<MarkupTokenType>>)null;

			var transformer = new XmlReader.XmlDataReadConverter(new DataReaderSettings());

			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var actual = transformer.Transform(input).ToArray();
				});

			// verify exception is coming from expected param
			Assert.Equal("input", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Ctor_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var transformer = new XmlReader.XmlDataReadConverter(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
