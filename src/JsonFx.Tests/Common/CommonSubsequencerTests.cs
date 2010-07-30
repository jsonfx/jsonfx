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

			var expected = true;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
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

			var expected = false;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Hello.")
			};

			var expected = false;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			var expected = false;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			var expected = false;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
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

			var expected = true;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
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

			var expected = false;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			var expected = false;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Hello.")
			};

			var expected = false;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			var expected = false;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
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

			var expected = false;
			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
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

			var expected = false;

			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_FalsePrimitive_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			var expected = true;
			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_StringPrimitive_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Hello.")
			};

			var expected = true;
			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			var expected = false;
			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
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
		public void GetProperties_AllProperties_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key1"), new[] { CommonGrammar.TokenNull } },
				{ new DataName("key2"), new[] { CommonGrammar.TokenValue("Hello!") } }
			};

			// select all properties
			var actual = input.GetProperties(name => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion GetProperty Tests
	}
}
