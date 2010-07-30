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

using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Common
{
	public class CommonSubsequencerTests
	{
		#region Constants

		private const string TraitName = "LINQ";
		private const string TraitValue = "CommonSubsequencer";

		#endregion Constants

		#region IsArray Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_ArrayEmpty_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			Assert.True(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_ObjectEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Hello.")
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			Assert.False(input.IsPrimitive());
		}

		#endregion IsArray Tests

		#region IsObject Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_ObjectEmpty_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			Assert.True(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_ArrayEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Hello.")
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			Assert.False(input.IsPrimitive());
		}

		#endregion IsObject Tests

		#region IsPrimitive Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_ObjectEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_ArrayEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_FalsePrimitive_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			Assert.True(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_StringPrimitive_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Hello.")
			};

			Assert.True(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			Assert.False(input.IsPrimitive());
		}

		#endregion IsPrimitive Tests

		#region Array Item Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_MixedPrimitivesFilterAll_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new IEnumerable<Token<CommonTokenType>>[0];

			// select no items
			var actual = input.GetArrayItems(item => false).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_MixedPrimitivesFilterNone_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenFalse
				},
				new[]
				{
					CommonGrammar.TokenTrue
				},
				new[]
				{
					CommonGrammar.TokenNull
				},
				new[]
				{
					CommonGrammar.TokenValue("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenNull
				},
				new[]
				{
					CommonGrammar.TokenValue(42)
				},
			};

			// select all items
			var actual = input.GetArrayItems(item => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_MixedPrimitivesFilterNonNull_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenFalse
				},
				new[]
				{
					CommonGrammar.TokenTrue
				},
				new[]
				{
					CommonGrammar.TokenValue("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenValue(42)
				},
			};

			// select all items with a non-null value
			var actual = input.GetArrayItems(item => item.All(token => token.Value != null)).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_NestedArray_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenFalse
				},
				new[]
				{
					CommonGrammar.TokenValue("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenNull,
					CommonGrammar.TokenTrue,
					CommonGrammar.TokenValue("Hello!"),
					CommonGrammar.TokenValue(42),
					CommonGrammar.TokenArrayEnd,
				},
				new[]
				{
					CommonGrammar.TokenValue(42)
				},
			};

			// select all items
			var actual = input.GetArrayItems(item => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_NestedObject_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenFalse
				},
				new[]
				{
					CommonGrammar.TokenValue("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty("key1"),
					CommonGrammar.TokenNull,
					CommonGrammar.TokenProperty("key2"),
					CommonGrammar.TokenValue("Hello!"),
					CommonGrammar.TokenObjectEnd,
				},
				new[]
				{
					CommonGrammar.TokenValue(42)
				},
			};

			// select all items
			var actual = input.GetArrayItems(item => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItem_IndexFilter_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenValue("Pick me!"),
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenValue("Pick me!")
				},
				new[]
				{
					CommonGrammar.TokenValue(42)
				}
			};

			// select items with odd indexes
			var actual = input.GetArrayIndex(index => (index % 2 == 1)).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_FilteringByIndexAndValue_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenValue(Math.PI),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenFalse
				},
				new[]
				{
			        CommonGrammar.TokenValue(Math.PI),
				}
			};

			// select all even index primitive items
			var actual = input.GetArrayItems((tokens, index) => (index % 2 == 0) && tokens.FirstOrDefault().TokenType == CommonTokenType.Primitive).ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion Array Item Tests

		#region GetProperty Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void HasProperty_PickExisting_ReturnsTrue()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenValue(3),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			// test for a specific property
			var actual = input.HasProperty(name => name.LocalName == "three");

			Assert.True(actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void HasProperty_PickNonExisting_ReturnsFalse()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenValue(3),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			// test for a specific property
			var actual = input.HasProperty(name => name.LocalName == "five");

			Assert.False(actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetProperties_AllProperties_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenValue(3),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key1"), new[] { CommonGrammar.TokenNull } },
				{ new DataName("key2"), new[] { CommonGrammar.TokenValue("Hello!") } },
				{ new DataName("three"), new[] { CommonGrammar.TokenValue(3) } },
				{ new DataName("4"), new[] { CommonGrammar.TokenTrue } }
			};

			// select all properties
			var actual = input.GetProperties(name => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetProperties_OnlyOneProperty_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenValue(3),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] { CommonGrammar.TokenValue("Hello!") } }
			};

			// select all properties
			var actual = input.GetProperties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetProperties_NestedObjectPropertySkipped_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] { CommonGrammar.TokenValue("Hello!") } }
			};

			// select all properties
			var actual = input.GetProperties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetProperties_NestedObjectPropertyReturned_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] {
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty("key2"),
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenObjectEnd,
				} }
			};

			// select all properties
			var actual = input.GetProperties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetProperties_NestedArrayPropertySkipped_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] { CommonGrammar.TokenValue("Hello!") } }
			};

			// select all properties
			var actual = input.GetProperties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetProperties_NestedArrayPropertyReturned_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd,
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] {
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenValue(42),
					CommonGrammar.TokenArrayEnd,
				} }
			};

			// select all properties
			var actual = input.GetProperties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion GetProperty Tests

		#region Complex Graph Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetProperties_GraphComplex_ReturnsGraph()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("JSON Test Pattern pass1"),
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("object with 1 member"),
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("array with 1 element"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenValue(-42),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("integer"),
				CommonGrammar.TokenValue(1234567890),
				CommonGrammar.TokenProperty("real"),
				CommonGrammar.TokenValue(-9876.543210),
				CommonGrammar.TokenProperty("e"),
				CommonGrammar.TokenValue(0.123456789e-12),
				CommonGrammar.TokenProperty("E"),
				CommonGrammar.TokenValue(1.234567890E+34),
				CommonGrammar.TokenProperty(""),
				CommonGrammar.TokenValue(23456789012E66),
				CommonGrammar.TokenProperty("zero"),
				CommonGrammar.TokenValue(0),
				CommonGrammar.TokenProperty("one"),
				CommonGrammar.TokenValue(1),
				CommonGrammar.TokenProperty("space"),
				CommonGrammar.TokenValue(" "),
				CommonGrammar.TokenProperty("quote"),
				CommonGrammar.TokenValue("\""),
				CommonGrammar.TokenProperty("backslash"),
				CommonGrammar.TokenValue("\\"),
				CommonGrammar.TokenProperty("controls"),
				CommonGrammar.TokenValue("\b\f\n\r\t"),
				CommonGrammar.TokenProperty("slash"),
				CommonGrammar.TokenValue("/ & /"),
				CommonGrammar.TokenProperty("alpha"),
				CommonGrammar.TokenValue("abcdefghijklmnopqrstuvwyz"),
				CommonGrammar.TokenProperty("ALPHA"),
				CommonGrammar.TokenValue("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				CommonGrammar.TokenProperty("digit"),
				CommonGrammar.TokenValue("0123456789"),
				CommonGrammar.TokenProperty("0123456789"),
				CommonGrammar.TokenValue("digit"),
				CommonGrammar.TokenProperty("special"),
				CommonGrammar.TokenValue("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				CommonGrammar.TokenProperty("hex"),
				CommonGrammar.TokenValue("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				CommonGrammar.TokenProperty("true"),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenProperty("false"),
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenProperty("null"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenProperty("array"),
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("object"),
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenProperty("address"),
				CommonGrammar.TokenValue("50 St. James Street"),
				CommonGrammar.TokenProperty("url"),
				CommonGrammar.TokenValue("http://www.JSON.org/"),
				CommonGrammar.TokenProperty("comment"),
				CommonGrammar.TokenValue("// /* <!-- --"),
				CommonGrammar.TokenProperty("# -- --> */"),
				CommonGrammar.TokenValue(" "),
				CommonGrammar.TokenProperty(" s p a c e d "),
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue(1),
				CommonGrammar.TokenValue(2),
				CommonGrammar.TokenValue(3),
				CommonGrammar.TokenValue(4),
				CommonGrammar.TokenValue(5),
				CommonGrammar.TokenValue(6),
				CommonGrammar.TokenValue(7),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("compact"),
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue(1),
				CommonGrammar.TokenValue(2),
				CommonGrammar.TokenValue(3),
				CommonGrammar.TokenValue(4),
				CommonGrammar.TokenValue(5),
				CommonGrammar.TokenValue(6),
				CommonGrammar.TokenValue(7),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("jsontext"),
				CommonGrammar.TokenValue("{\"object with 1 member\":[\"array with 1 element\"]}"),
				CommonGrammar.TokenProperty("quotes"),
				CommonGrammar.TokenValue("&#34; \u0022 %22 0x22 034 &#x22;"),
				CommonGrammar.TokenProperty("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				CommonGrammar.TokenValue("A key can be any string"),
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValue(0.5),
				CommonGrammar.TokenValue(98.6),
				CommonGrammar.TokenValue(99.44),
				CommonGrammar.TokenValue(1066),
				CommonGrammar.TokenValue(10.0),
				CommonGrammar.TokenValue(1.0),
				CommonGrammar.TokenValue(0.1),
				CommonGrammar.TokenValue(1.0),
				CommonGrammar.TokenValue(2.0),
				CommonGrammar.TokenValue(2.0),
				CommonGrammar.TokenValue("rosebud"),
				CommonGrammar.TokenArrayEnd
			};

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("url"), new[] { CommonGrammar.TokenValue("http://www.JSON.org/") } },
				{ new DataName("compact"), new[] {
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenValue(1),
					CommonGrammar.TokenValue(2),
					CommonGrammar.TokenValue(3),
					CommonGrammar.TokenValue(4),
					CommonGrammar.TokenValue(5),
					CommonGrammar.TokenValue(6),
					CommonGrammar.TokenValue(7),
					CommonGrammar.TokenArrayEnd,
				} }
			};

			Assert.True(input.IsArray());
			Assert.False(input.IsObject());
			Assert.False(input.IsPrimitive());

			// cherry pick properties
			var actual = input
				.GetArrayIndex(index => index == 8).FirstOrDefault() // select the big object
				.GetProperties(name => name.LocalName == "url" || name.LocalName == "compact"); // select two properties

			Assert.Equal(expected, actual, false);
		}

		#endregion Complex Graph Tests
	}
}
