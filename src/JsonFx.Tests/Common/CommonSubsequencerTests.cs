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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayEnd
			};

			Assert.True(input.IsArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_ObjectEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenObjectEnd
			};

			Assert.False(input.IsArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive("Hello.")
			};

			Assert.False(input.IsArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			Assert.False(input.IsArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			Assert.False(input.IsArray());
		}

		#endregion IsArray Tests

		#region IsObject Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_ObjectEmpty_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenObjectEnd
			};

			Assert.True(input.IsObject());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_ArrayEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayEnd
			};

			Assert.False(input.IsObject());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			Assert.False(input.IsObject());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive("Hello.")
			};

			Assert.False(input.IsObject());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			Assert.False(input.IsObject());
		}

		#endregion IsObject Tests

		#region IsPrimitive Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_ObjectEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginUnnamed,
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
				CommonGrammar.TokenArrayBeginUnnamed,
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
				CommonGrammar.TokenPrimitive("Hello.")
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
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenPrimitive(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new IEnumerable<Token<CommonTokenType>>[0];

			// select no items
			var actual = input.GetArrayItems(item => false).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_MixedPrimitivesNoFilter_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenPrimitive(42),
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
					CommonGrammar.TokenPrimitive("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenNull
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(42)
				},
			};

			// select all items
			var actual = input.GetArrayItems().ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_MixedPrimitivesFilterNonNull_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenPrimitive(42),
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
					CommonGrammar.TokenPrimitive("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(42)
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
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenPrimitive(42),
		        CommonGrammar.TokenArrayEnd,
		        CommonGrammar.TokenPrimitive(42),
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
					CommonGrammar.TokenPrimitive("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenNull,
					CommonGrammar.TokenTrue,
					CommonGrammar.TokenPrimitive("Hello!"),
					CommonGrammar.TokenPrimitive(42),
					CommonGrammar.TokenArrayEnd,
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(42)
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
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenPrimitive(42),
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
					CommonGrammar.TokenPrimitive("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("key1"),
					CommonGrammar.TokenNull,
					CommonGrammar.TokenProperty("key2"),
					CommonGrammar.TokenPrimitive("Hello!"),
					CommonGrammar.TokenObjectEnd,
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(42)
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
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenPrimitive("Pick me!"),
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenPrimitive(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenPrimitive("Pick me!")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(42)
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
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenPrimitive(42),
		        CommonGrammar.TokenPrimitive(Math.PI),
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
			        CommonGrammar.TokenPrimitive(Math.PI),
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
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenPrimitive(3),
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
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenPrimitive(3),
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
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenPrimitive(3),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key1"), new[] { CommonGrammar.TokenNull } },
				{ new DataName("key2"), new[] { CommonGrammar.TokenPrimitive("Hello!") } },
				{ new DataName("three"), new[] { CommonGrammar.TokenPrimitive(3) } },
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
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenPrimitive(3),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] { CommonGrammar.TokenPrimitive("Hello!") } }
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
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] { CommonGrammar.TokenPrimitive("Hello!") } }
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
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] {
					CommonGrammar.TokenObjectBeginUnnamed,
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
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenPrimitive(42),
		        CommonGrammar.TokenArrayEnd,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] { CommonGrammar.TokenPrimitive("Hello!") } }
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
		        CommonGrammar.TokenObjectBeginUnnamed,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenArrayBeginUnnamed,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenPrimitive(42),
		        CommonGrammar.TokenArrayEnd,
		        CommonGrammar.TokenProperty("three"),
		        CommonGrammar.TokenPrimitive("Hello!"),
		        CommonGrammar.TokenProperty("4"),
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("key2"), new[] {
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenPrimitive(42),
					CommonGrammar.TokenArrayEnd,
				} }
			};

			// select all properties
			var actual = input.GetProperties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion GetProperty Tests

		#region Descendants Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetDescendantsAndSelf_NestedObjects_ReturnsAllSubsequencesAndSelf()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenNull,

					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenArrayBeginUnnamed,
						CommonGrammar.TokenTrue,
						CommonGrammar.TokenPrimitive("2-B"),
						CommonGrammar.TokenPrimitive(23),
					CommonGrammar.TokenArrayEnd,

					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenObjectBeginUnnamed,
						CommonGrammar.TokenProperty("A"),
						CommonGrammar.TokenPrimitive("3-A"),
						CommonGrammar.TokenProperty("B"),
						CommonGrammar.TokenPrimitive(32),
						CommonGrammar.TokenProperty("C"),
						CommonGrammar.TokenPrimitive("3-C"),
					CommonGrammar.TokenObjectEnd,

					CommonGrammar.TokenProperty("Four"),
					CommonGrammar.TokenPrimitive(4),
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenObjectBeginUnnamed,
						CommonGrammar.TokenProperty("One"),
						CommonGrammar.TokenNull,

						CommonGrammar.TokenProperty("Two"),
						CommonGrammar.TokenArrayBeginUnnamed,
							CommonGrammar.TokenTrue,
							CommonGrammar.TokenPrimitive("2-B"),
							CommonGrammar.TokenPrimitive(23),
						CommonGrammar.TokenArrayEnd,

						CommonGrammar.TokenProperty("Three"),
						CommonGrammar.TokenObjectBeginUnnamed,
							CommonGrammar.TokenProperty("A"),
							CommonGrammar.TokenPrimitive("3-A"),
							CommonGrammar.TokenProperty("B"),
							CommonGrammar.TokenPrimitive(32),
							CommonGrammar.TokenProperty("C"),
							CommonGrammar.TokenPrimitive("3-C"),
						CommonGrammar.TokenObjectEnd,

						CommonGrammar.TokenProperty("Four"),
						CommonGrammar.TokenPrimitive(4),
					CommonGrammar.TokenObjectEnd
				},
				new[]
				{
					CommonGrammar.TokenNull
				},
				new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
						CommonGrammar.TokenTrue,
						CommonGrammar.TokenPrimitive("2-B"),
						CommonGrammar.TokenPrimitive(23),
					CommonGrammar.TokenArrayEnd
				},
				new[]
				{
					CommonGrammar.TokenTrue
				},
				new[]
				{
					CommonGrammar.TokenPrimitive("2-B")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(23)
				},
				new[]
				{
					CommonGrammar.TokenObjectBeginUnnamed,
						CommonGrammar.TokenProperty("A"),
						CommonGrammar.TokenPrimitive("3-A"),
						CommonGrammar.TokenProperty("B"),
						CommonGrammar.TokenPrimitive(32),
						CommonGrammar.TokenProperty("C"),
						CommonGrammar.TokenPrimitive("3-C"),
					CommonGrammar.TokenObjectEnd
				},
				new[]
				{
					CommonGrammar.TokenPrimitive("3-A")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(32)
				},
				new[]
				{
					CommonGrammar.TokenPrimitive("3-C")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(4),
				}
			};

			// select all descendants and self
			var actual = input.GetDescendantsAndSelf().ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetDescendants_NestedObjects_ReturnsAllSubsequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenNull,

					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenArrayBeginUnnamed,
						CommonGrammar.TokenTrue,
						CommonGrammar.TokenPrimitive("2-B"),
						CommonGrammar.TokenPrimitive(23),
					CommonGrammar.TokenArrayEnd,

					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenObjectBeginUnnamed,
						CommonGrammar.TokenProperty("A"),
						CommonGrammar.TokenPrimitive("3-A"),
						CommonGrammar.TokenProperty("B"),
						CommonGrammar.TokenPrimitive(32),
						CommonGrammar.TokenProperty("C"),
						CommonGrammar.TokenPrimitive("3-C"),
					CommonGrammar.TokenObjectEnd,

					CommonGrammar.TokenProperty("Four"),
					CommonGrammar.TokenPrimitive(4),
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenNull
				},
				new[]
				{
					CommonGrammar.TokenArrayBeginUnnamed,
						CommonGrammar.TokenTrue,
						CommonGrammar.TokenPrimitive("2-B"),
						CommonGrammar.TokenPrimitive(23),
					CommonGrammar.TokenArrayEnd
				},
				new[]
				{
					CommonGrammar.TokenTrue
				},
				new[]
				{
					CommonGrammar.TokenPrimitive("2-B")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(23)
				},
				new[]
				{
					CommonGrammar.TokenObjectBeginUnnamed,
						CommonGrammar.TokenProperty("A"),
						CommonGrammar.TokenPrimitive("3-A"),
						CommonGrammar.TokenProperty("B"),
						CommonGrammar.TokenPrimitive(32),
						CommonGrammar.TokenProperty("C"),
						CommonGrammar.TokenPrimitive("3-C"),
					CommonGrammar.TokenObjectEnd
				},
				new[]
				{
					CommonGrammar.TokenPrimitive("3-A")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(32)
				},
				new[]
				{
					CommonGrammar.TokenPrimitive("3-C")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(4),
				}
			};

			// select all descendants
			var actual = input.GetDescendants().ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetDescendants_NestedObjectsFindDescendantsWithProperty_ReturnsMatchingSubsequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenNull,

					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenArrayBeginUnnamed,
						CommonGrammar.TokenTrue,
						CommonGrammar.TokenPrimitive("2-B"),
						CommonGrammar.TokenPrimitive(23),
					CommonGrammar.TokenArrayEnd,

					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenObjectBeginUnnamed,
						CommonGrammar.TokenProperty("A"),
						CommonGrammar.TokenPrimitive("3-A"),
						CommonGrammar.TokenProperty("B"),
						CommonGrammar.TokenPrimitive(32),
						CommonGrammar.TokenProperty("C"),
						CommonGrammar.TokenPrimitive("3-C"),
					CommonGrammar.TokenObjectEnd,

					CommonGrammar.TokenProperty("Four"),
					CommonGrammar.TokenPrimitive(4),
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenObjectBeginUnnamed,
						CommonGrammar.TokenProperty("A"),
						CommonGrammar.TokenPrimitive("3-A"),
						CommonGrammar.TokenProperty("B"),
						CommonGrammar.TokenPrimitive(32),
						CommonGrammar.TokenProperty("C"),
						CommonGrammar.TokenPrimitive("3-C"),
					CommonGrammar.TokenObjectEnd
				}
			};

			// select all descendants with property named "B"
			var actual = input.GetDescendants().Where(child => child.HasProperty(name => name.LocalName == "B")).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetDescendantsAndSelf_NestedObjectsReturnsAllPrimitives_ReturnsMatchingSubsequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenObjectBeginUnnamed,
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenNull,

					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenArrayBeginUnnamed,
						CommonGrammar.TokenTrue,
						CommonGrammar.TokenPrimitive("2-B"),
						CommonGrammar.TokenPrimitive(23),
					CommonGrammar.TokenArrayEnd,

					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenObjectBeginUnnamed,
						CommonGrammar.TokenProperty("A"),
						CommonGrammar.TokenPrimitive("3-A"),
						CommonGrammar.TokenProperty("B"),
						CommonGrammar.TokenPrimitive(32),
						CommonGrammar.TokenProperty("C"),
						CommonGrammar.TokenPrimitive("3-C"),
					CommonGrammar.TokenObjectEnd,

					CommonGrammar.TokenProperty("Four"),
					CommonGrammar.TokenPrimitive(4),
		        CommonGrammar.TokenObjectEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenNull
				},
				new[]
				{
					CommonGrammar.TokenTrue
				},
				new[]
				{
					CommonGrammar.TokenPrimitive("2-B")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(23)
				},
				new[]
				{
					CommonGrammar.TokenPrimitive("3-A")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(32)
				},
				new[]
				{
					CommonGrammar.TokenPrimitive("3-C")
				},
				new[]
				{
					CommonGrammar.TokenPrimitive(4),
				}
			};

			// select all primitive values
			var actual = input.GetDescendantsAndSelf().Where(child => child.IsPrimitive()).ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion Descendants Tests

		#region Complex Graph Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetProperties_GraphComplex_ReturnsGraph()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("JSON Test Pattern pass1"),
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("object with 1 member"),
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("array with 1 element"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenPrimitive(-42),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("integer"),
				CommonGrammar.TokenPrimitive(1234567890),
				CommonGrammar.TokenProperty("real"),
				CommonGrammar.TokenPrimitive(-9876.543210),
				CommonGrammar.TokenProperty("e"),
				CommonGrammar.TokenPrimitive(0.123456789e-12),
				CommonGrammar.TokenProperty("E"),
				CommonGrammar.TokenPrimitive(1.234567890E+34),
				CommonGrammar.TokenProperty(""),
				CommonGrammar.TokenPrimitive(23456789012E66),
				CommonGrammar.TokenProperty("zero"),
				CommonGrammar.TokenPrimitive(0),
				CommonGrammar.TokenProperty("one"),
				CommonGrammar.TokenPrimitive(1),
				CommonGrammar.TokenProperty("space"),
				CommonGrammar.TokenPrimitive(" "),
				CommonGrammar.TokenProperty("quote"),
				CommonGrammar.TokenPrimitive("\""),
				CommonGrammar.TokenProperty("backslash"),
				CommonGrammar.TokenPrimitive("\\"),
				CommonGrammar.TokenProperty("controls"),
				CommonGrammar.TokenPrimitive("\b\f\n\r\t"),
				CommonGrammar.TokenProperty("slash"),
				CommonGrammar.TokenPrimitive("/ & /"),
				CommonGrammar.TokenProperty("alpha"),
				CommonGrammar.TokenPrimitive("abcdefghijklmnopqrstuvwyz"),
				CommonGrammar.TokenProperty("ALPHA"),
				CommonGrammar.TokenPrimitive("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				CommonGrammar.TokenProperty("digit"),
				CommonGrammar.TokenPrimitive("0123456789"),
				CommonGrammar.TokenProperty("0123456789"),
				CommonGrammar.TokenPrimitive("digit"),
				CommonGrammar.TokenProperty("special"),
				CommonGrammar.TokenPrimitive("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				CommonGrammar.TokenProperty("hex"),
				CommonGrammar.TokenPrimitive("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				CommonGrammar.TokenProperty("true"),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenProperty("false"),
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenProperty("null"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenProperty("array"),
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("object"),
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenProperty("address"),
				CommonGrammar.TokenPrimitive("50 St. James Street"),
				CommonGrammar.TokenProperty("url"),
				CommonGrammar.TokenPrimitive("http://www.JSON.org/"),
				CommonGrammar.TokenProperty("comment"),
				CommonGrammar.TokenPrimitive("// /* <!-- --"),
				CommonGrammar.TokenProperty("# -- --> */"),
				CommonGrammar.TokenPrimitive(" "),
				CommonGrammar.TokenProperty(" s p a c e d "),
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive(1),
				CommonGrammar.TokenPrimitive(2),
				CommonGrammar.TokenPrimitive(3),
				CommonGrammar.TokenPrimitive(4),
				CommonGrammar.TokenPrimitive(5),
				CommonGrammar.TokenPrimitive(6),
				CommonGrammar.TokenPrimitive(7),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("compact"),
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive(1),
				CommonGrammar.TokenPrimitive(2),
				CommonGrammar.TokenPrimitive(3),
				CommonGrammar.TokenPrimitive(4),
				CommonGrammar.TokenPrimitive(5),
				CommonGrammar.TokenPrimitive(6),
				CommonGrammar.TokenPrimitive(7),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("jsontext"),
				CommonGrammar.TokenPrimitive("{\"object with 1 member\":[\"array with 1 element\"]}"),
				CommonGrammar.TokenProperty("quotes"),
				CommonGrammar.TokenPrimitive("&#34; \u0022 %22 0x22 034 &#x22;"),
				CommonGrammar.TokenProperty("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				CommonGrammar.TokenPrimitive("A key can be any string"),
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenPrimitive(0.5),
				CommonGrammar.TokenPrimitive(98.6),
				CommonGrammar.TokenPrimitive(99.44),
				CommonGrammar.TokenPrimitive(1066),
				CommonGrammar.TokenPrimitive(10.0),
				CommonGrammar.TokenPrimitive(1.0),
				CommonGrammar.TokenPrimitive(0.1),
				CommonGrammar.TokenPrimitive(1.0),
				CommonGrammar.TokenPrimitive(2.0),
				CommonGrammar.TokenPrimitive(2.0),
				CommonGrammar.TokenPrimitive("rosebud"),
				CommonGrammar.TokenArrayEnd
			};

			var expected = new Dictionary<DataName, IEnumerable<Token<CommonTokenType>>>
			{
				{ new DataName("url"), new[] { CommonGrammar.TokenPrimitive("http://www.JSON.org/") } },
				{ new DataName("compact"), new[] {
					CommonGrammar.TokenArrayBeginUnnamed,
					CommonGrammar.TokenPrimitive(1),
					CommonGrammar.TokenPrimitive(2),
					CommonGrammar.TokenPrimitive(3),
					CommonGrammar.TokenPrimitive(4),
					CommonGrammar.TokenPrimitive(5),
					CommonGrammar.TokenPrimitive(6),
					CommonGrammar.TokenPrimitive(7),
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
