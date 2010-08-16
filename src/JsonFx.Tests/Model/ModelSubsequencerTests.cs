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

namespace JsonFx.Model
{
	public class ModelSubsequencerTests
	{
		#region Constants

		private const string TraitName = "LINQ";
		private const string TraitValue = "ModelSubsequencer";

		#endregion Constants

		#region IsArray Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_ArrayEmpty_ReturnsTrue()
		{
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd
			};

			Assert.True(input.IsArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_ObjectEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd
			};

			Assert.False(input.IsArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				ModelGrammar.TokenPrimitive("Hello.")
			};

			Assert.False(input.IsArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				ModelGrammar.TokenFalse
			};

			Assert.False(input.IsArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_EmptySequence_ReturnsFalse()
		{
			var input = new Token<ModelTokenType>[0];

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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd
			};

			Assert.True(input.IsObject());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_ArrayEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd
			};

			Assert.False(input.IsObject());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				ModelGrammar.TokenFalse
			};

			Assert.False(input.IsObject());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				ModelGrammar.TokenPrimitive("Hello.")
			};

			Assert.False(input.IsObject());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_EmptySequence_ReturnsFalse()
		{
			var input = new Token<ModelTokenType>[0];

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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_ArrayEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd
			};

			Assert.False(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_FalsePrimitive_ReturnsTrue()
		{
			var input = new[]
			{
				ModelGrammar.TokenFalse
			};

			Assert.True(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_StringPrimitive_ReturnsTrue()
		{
			var input = new[]
			{
				ModelGrammar.TokenPrimitive("Hello.")
			};

			Assert.True(input.IsPrimitive());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_EmptySequence_ReturnsFalse()
		{
			var input = new Token<ModelTokenType>[0];

			Assert.False(input.IsPrimitive());
		}

		#endregion IsPrimitive Tests

		#region ArrayItems Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ArrayItems_MixedPrimitivesFilterAll_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenArrayEnd
		    };

			var expected = new IEnumerable<Token<ModelTokenType>>[0];

			// select no items
			var actual = input.ArrayItems(index => false).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ArrayItems_MixedPrimitivesNoFilter_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenFalse
				},
				new[]
				{
					ModelGrammar.TokenTrue
				},
				new[]
				{
					ModelGrammar.TokenNull
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("Hello!")
				},
				new[]
				{
					ModelGrammar.TokenNull
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(42)
				},
			};

			// select all items
			var actual = input.ArrayItems().ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ArrayItems_MixedPrimitivesFilterNonNull_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenFalse
				},
				new[]
				{
					ModelGrammar.TokenTrue
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("Hello!")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(42)
				},
			};

			// select all items with a non-null value
			var actual = input.ArrayItems().Where(item => item.All(token => token.Value != null)).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ArrayItems_NestedArray_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenArrayEnd,
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenFalse
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("Hello!")
				},
				new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenFalse,
					ModelGrammar.TokenNull,
					ModelGrammar.TokenTrue,
					ModelGrammar.TokenPrimitive("Hello!"),
					ModelGrammar.TokenPrimitive(42),
					ModelGrammar.TokenArrayEnd,
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(42)
				},
			};

			// select all items
			var actual = input.ArrayItems(index => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ArrayItems_NestedObject_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenObjectEnd,
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenFalse
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("Hello!")
				},
				new[]
				{
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty("key1"),
					ModelGrammar.TokenNull,
					ModelGrammar.TokenProperty("key2"),
					ModelGrammar.TokenPrimitive("Hello!"),
					ModelGrammar.TokenObjectEnd,
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(42)
				},
			};

			// select all items
			var actual = input.ArrayItems(index => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ArrayItems_IndexFilter_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenPrimitive("Pick me!"),
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenObjectEnd,
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenPrimitive("Pick me!")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(42)
				}
			};

			// select items with odd indexes
			var actual = input.ArrayItems(index => (index % 2 == 1)).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ArrayItems_FilteringByIndexAndValue_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenObjectEnd,
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenPrimitive(Math.PI),
		        ModelGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenFalse
				},
				new[]
				{
			        ModelGrammar.TokenPrimitive(Math.PI),
				}
			};

			// select all even index primitive items
			var actual = input.ArrayItems(index => (index % 2 == 0)).Where(item => item.IsPrimitive()).ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion ArrayItems Tests

		#region HasProperty Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void HasProperty_PickExisting_ReturnsTrue()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenProperty("three"),
		        ModelGrammar.TokenPrimitive(3),
		        ModelGrammar.TokenProperty("4"),
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenObjectEnd
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
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenProperty("three"),
		        ModelGrammar.TokenPrimitive(3),
		        ModelGrammar.TokenProperty("4"),
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenObjectEnd
		    };

			// test for a specific property
			var actual = input.HasProperty(name => name.LocalName == "five");

			Assert.False(actual);
		}

		#endregion HasProperty Tests

		#region ObjectProperties Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ObjectProperties_AllProperties_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenProperty("three"),
		        ModelGrammar.TokenPrimitive(3),
		        ModelGrammar.TokenProperty("4"),
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<ModelTokenType>>>
			{
				{ new DataName("key1"), new[] { ModelGrammar.TokenNull } },
				{ new DataName("key2"), new[] { ModelGrammar.TokenPrimitive("Hello!") } },
				{ new DataName("three"), new[] { ModelGrammar.TokenPrimitive(3) } },
				{ new DataName("4"), new[] { ModelGrammar.TokenTrue } }
			};

			// select all properties
			var actual = input.Properties(name => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ObjectProperties_OnlyOneProperty_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenProperty("three"),
		        ModelGrammar.TokenPrimitive(3),
		        ModelGrammar.TokenProperty("4"),
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<ModelTokenType>>>
			{
				{ new DataName("key2"), new[] { ModelGrammar.TokenPrimitive("Hello!") } }
			};

			// select all properties
			var actual = input.Properties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ObjectProperties_NestedObjectPropertySkipped_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("three"),
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenObjectEnd,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenProperty("4"),
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<ModelTokenType>>>
			{
				{ new DataName("key2"), new[] { ModelGrammar.TokenPrimitive("Hello!") } }
			};

			// select all properties
			var actual = input.Properties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ObjectProperties_NestedObjectPropertyReturned_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenObjectEnd,
		        ModelGrammar.TokenProperty("three"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenProperty("4"),
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<ModelTokenType>>>
			{
				{ new DataName("key2"), new[] {
					ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty("key2"),
					ModelGrammar.TokenFalse,
					ModelGrammar.TokenObjectEnd,
				} }
			};

			// select all properties
			var actual = input.Properties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ObjectProperties_NestedArrayPropertySkipped_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("three"),
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenArrayEnd,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenProperty("4"),
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<ModelTokenType>>>
			{
				{ new DataName("key2"), new[] { ModelGrammar.TokenPrimitive("Hello!") } }
			};

			// select all properties
			var actual = input.Properties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ObjectProperties_NestedArrayPropertyReturned_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
		        ModelGrammar.TokenProperty("key1"),
		        ModelGrammar.TokenNull,
		        ModelGrammar.TokenProperty("key2"),
		        ModelGrammar.TokenArrayBeginUnnamed,
		        ModelGrammar.TokenFalse,
		        ModelGrammar.TokenPrimitive(42),
		        ModelGrammar.TokenArrayEnd,
		        ModelGrammar.TokenProperty("three"),
		        ModelGrammar.TokenPrimitive("Hello!"),
		        ModelGrammar.TokenProperty("4"),
		        ModelGrammar.TokenTrue,
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new Dictionary<DataName, IEnumerable<Token<ModelTokenType>>>
			{
				{ new DataName("key2"), new[] {
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenFalse,
					ModelGrammar.TokenPrimitive(42),
					ModelGrammar.TokenArrayEnd,
				} }
			};

			// select all properties
			var actual = input.Properties(name => name.LocalName == "key2").ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion ObjectProperties Tests

		#region Descendants Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void DescendantsAndSelf_NestedObjects_ReturnsAllSubsequencesAndSelf()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty("One"),
					ModelGrammar.TokenNull,

					ModelGrammar.TokenProperty("Two"),
					ModelGrammar.TokenArrayBeginUnnamed,
						ModelGrammar.TokenTrue,
						ModelGrammar.TokenPrimitive("2-B"),
						ModelGrammar.TokenPrimitive(23),
					ModelGrammar.TokenArrayEnd,

					ModelGrammar.TokenProperty("Three"),
					ModelGrammar.TokenObjectBeginUnnamed,
						ModelGrammar.TokenProperty("A"),
						ModelGrammar.TokenPrimitive("3-A"),
						ModelGrammar.TokenProperty("B"),
						ModelGrammar.TokenPrimitive(32),
						ModelGrammar.TokenProperty("C"),
						ModelGrammar.TokenPrimitive("3-C"),
					ModelGrammar.TokenObjectEnd,

					ModelGrammar.TokenProperty("Four"),
					ModelGrammar.TokenPrimitive(4),
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenObjectBeginUnnamed,
						ModelGrammar.TokenProperty("One"),
						ModelGrammar.TokenNull,

						ModelGrammar.TokenProperty("Two"),
						ModelGrammar.TokenArrayBeginUnnamed,
							ModelGrammar.TokenTrue,
							ModelGrammar.TokenPrimitive("2-B"),
							ModelGrammar.TokenPrimitive(23),
						ModelGrammar.TokenArrayEnd,

						ModelGrammar.TokenProperty("Three"),
						ModelGrammar.TokenObjectBeginUnnamed,
							ModelGrammar.TokenProperty("A"),
							ModelGrammar.TokenPrimitive("3-A"),
							ModelGrammar.TokenProperty("B"),
							ModelGrammar.TokenPrimitive(32),
							ModelGrammar.TokenProperty("C"),
							ModelGrammar.TokenPrimitive("3-C"),
						ModelGrammar.TokenObjectEnd,

						ModelGrammar.TokenProperty("Four"),
						ModelGrammar.TokenPrimitive(4),
					ModelGrammar.TokenObjectEnd
				},
				new[]
				{
					ModelGrammar.TokenNull
				},
				new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
						ModelGrammar.TokenTrue,
						ModelGrammar.TokenPrimitive("2-B"),
						ModelGrammar.TokenPrimitive(23),
					ModelGrammar.TokenArrayEnd
				},
				new[]
				{
					ModelGrammar.TokenTrue
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("2-B")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(23)
				},
				new[]
				{
					ModelGrammar.TokenObjectBeginUnnamed,
						ModelGrammar.TokenProperty("A"),
						ModelGrammar.TokenPrimitive("3-A"),
						ModelGrammar.TokenProperty("B"),
						ModelGrammar.TokenPrimitive(32),
						ModelGrammar.TokenProperty("C"),
						ModelGrammar.TokenPrimitive("3-C"),
					ModelGrammar.TokenObjectEnd
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("3-A")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(32)
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("3-C")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(4),
				}
			};

			// select all descendants and self
			var actual = input.DescendantsAndSelf().ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Descendants_NestedObjects_ReturnsAllSubsequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty("One"),
					ModelGrammar.TokenNull,

					ModelGrammar.TokenProperty("Two"),
					ModelGrammar.TokenArrayBeginUnnamed,
						ModelGrammar.TokenTrue,
						ModelGrammar.TokenPrimitive("2-B"),
						ModelGrammar.TokenPrimitive(23),
					ModelGrammar.TokenArrayEnd,

					ModelGrammar.TokenProperty("Three"),
					ModelGrammar.TokenObjectBeginUnnamed,
						ModelGrammar.TokenProperty("A"),
						ModelGrammar.TokenPrimitive("3-A"),
						ModelGrammar.TokenProperty("B"),
						ModelGrammar.TokenPrimitive(32),
						ModelGrammar.TokenProperty("C"),
						ModelGrammar.TokenPrimitive("3-C"),
					ModelGrammar.TokenObjectEnd,

					ModelGrammar.TokenProperty("Four"),
					ModelGrammar.TokenPrimitive(4),
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenNull
				},
				new[]
				{
					ModelGrammar.TokenArrayBeginUnnamed,
						ModelGrammar.TokenTrue,
						ModelGrammar.TokenPrimitive("2-B"),
						ModelGrammar.TokenPrimitive(23),
					ModelGrammar.TokenArrayEnd
				},
				new[]
				{
					ModelGrammar.TokenTrue
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("2-B")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(23)
				},
				new[]
				{
					ModelGrammar.TokenObjectBeginUnnamed,
						ModelGrammar.TokenProperty("A"),
						ModelGrammar.TokenPrimitive("3-A"),
						ModelGrammar.TokenProperty("B"),
						ModelGrammar.TokenPrimitive(32),
						ModelGrammar.TokenProperty("C"),
						ModelGrammar.TokenPrimitive("3-C"),
					ModelGrammar.TokenObjectEnd
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("3-A")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(32)
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("3-C")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(4),
				}
			};

			// select all descendants
			var actual = input.Descendants().ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Descendants_NestedObjectsFindDescendantsWithProperty_ReturnsMatchingSubsequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty("One"),
					ModelGrammar.TokenNull,

					ModelGrammar.TokenProperty("Two"),
					ModelGrammar.TokenArrayBeginUnnamed,
						ModelGrammar.TokenTrue,
						ModelGrammar.TokenPrimitive("2-B"),
						ModelGrammar.TokenPrimitive(23),
					ModelGrammar.TokenArrayEnd,

					ModelGrammar.TokenProperty("Three"),
					ModelGrammar.TokenObjectBeginUnnamed,
						ModelGrammar.TokenProperty("A"),
						ModelGrammar.TokenPrimitive("3-A"),
						ModelGrammar.TokenProperty("B"),
						ModelGrammar.TokenPrimitive(32),
						ModelGrammar.TokenProperty("C"),
						ModelGrammar.TokenPrimitive("3-C"),
					ModelGrammar.TokenObjectEnd,

					ModelGrammar.TokenProperty("Four"),
					ModelGrammar.TokenPrimitive(4),
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenObjectBeginUnnamed,
						ModelGrammar.TokenProperty("A"),
						ModelGrammar.TokenPrimitive("3-A"),
						ModelGrammar.TokenProperty("B"),
						ModelGrammar.TokenPrimitive(32),
						ModelGrammar.TokenProperty("C"),
						ModelGrammar.TokenPrimitive("3-C"),
					ModelGrammar.TokenObjectEnd
				}
			};

			// select all descendants with property named "B"
			var actual = input.Descendants().Where(child => child.HasProperty(name => name.LocalName == "B")).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void DescendantsAndSelf_NestedObjectsReturnsAllPrimitives_ReturnsMatchingSubsequences()
		{
			var input = new[]
		    {
		        ModelGrammar.TokenObjectBeginUnnamed,
					ModelGrammar.TokenProperty("One"),
					ModelGrammar.TokenNull,

					ModelGrammar.TokenProperty("Two"),
					ModelGrammar.TokenArrayBeginUnnamed,
						ModelGrammar.TokenTrue,
						ModelGrammar.TokenPrimitive("2-B"),
						ModelGrammar.TokenPrimitive(23),
					ModelGrammar.TokenArrayEnd,

					ModelGrammar.TokenProperty("Three"),
					ModelGrammar.TokenObjectBeginUnnamed,
						ModelGrammar.TokenProperty("A"),
						ModelGrammar.TokenPrimitive("3-A"),
						ModelGrammar.TokenProperty("B"),
						ModelGrammar.TokenPrimitive(32),
						ModelGrammar.TokenProperty("C"),
						ModelGrammar.TokenPrimitive("3-C"),
					ModelGrammar.TokenObjectEnd,

					ModelGrammar.TokenProperty("Four"),
					ModelGrammar.TokenPrimitive(4),
		        ModelGrammar.TokenObjectEnd
		    };

			var expected = new[]
			{
				new[]
				{
					ModelGrammar.TokenNull
				},
				new[]
				{
					ModelGrammar.TokenTrue
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("2-B")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(23)
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("3-A")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(32)
				},
				new[]
				{
					ModelGrammar.TokenPrimitive("3-C")
				},
				new[]
				{
					ModelGrammar.TokenPrimitive(4),
				}
			};

			// select all primitive values
			var actual = input.DescendantsAndSelf().Where(child => child.IsPrimitive()).ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion Descendants Tests

		#region Complex Graph Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Properties_GraphComplex_ReturnsGraph()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("JSON Test Pattern pass1"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("object with 1 member"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("array with 1 element"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenPrimitive(-42),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("integer"),
				ModelGrammar.TokenPrimitive(1234567890),
				ModelGrammar.TokenProperty("real"),
				ModelGrammar.TokenPrimitive(-9876.543210),
				ModelGrammar.TokenProperty("e"),
				ModelGrammar.TokenPrimitive(0.123456789e-12),
				ModelGrammar.TokenProperty("E"),
				ModelGrammar.TokenPrimitive(1.234567890E+34),
				ModelGrammar.TokenProperty(""),
				ModelGrammar.TokenPrimitive(23456789012E66),
				ModelGrammar.TokenProperty("zero"),
				ModelGrammar.TokenPrimitive(0),
				ModelGrammar.TokenProperty("one"),
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenProperty("space"),
				ModelGrammar.TokenPrimitive(" "),
				ModelGrammar.TokenProperty("quote"),
				ModelGrammar.TokenPrimitive("\""),
				ModelGrammar.TokenProperty("backslash"),
				ModelGrammar.TokenPrimitive("\\"),
				ModelGrammar.TokenProperty("controls"),
				ModelGrammar.TokenPrimitive("\b\f\n\r\t"),
				ModelGrammar.TokenProperty("slash"),
				ModelGrammar.TokenPrimitive("/ & /"),
				ModelGrammar.TokenProperty("alpha"),
				ModelGrammar.TokenPrimitive("abcdefghijklmnopqrstuvwyz"),
				ModelGrammar.TokenProperty("ALPHA"),
				ModelGrammar.TokenPrimitive("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				ModelGrammar.TokenProperty("digit"),
				ModelGrammar.TokenPrimitive("0123456789"),
				ModelGrammar.TokenProperty("0123456789"),
				ModelGrammar.TokenPrimitive("digit"),
				ModelGrammar.TokenProperty("special"),
				ModelGrammar.TokenPrimitive("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				ModelGrammar.TokenProperty("hex"),
				ModelGrammar.TokenPrimitive("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				ModelGrammar.TokenProperty("true"),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenProperty("false"),
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenProperty("null"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenProperty("array"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("object"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenProperty("address"),
				ModelGrammar.TokenPrimitive("50 St. James Street"),
				ModelGrammar.TokenProperty("url"),
				ModelGrammar.TokenPrimitive("http://www.JSON.org/"),
				ModelGrammar.TokenProperty("comment"),
				ModelGrammar.TokenPrimitive("// /* <!-- --"),
				ModelGrammar.TokenProperty("# -- --> */"),
				ModelGrammar.TokenPrimitive(" "),
				ModelGrammar.TokenProperty(" s p a c e d "),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenPrimitive(2),
				ModelGrammar.TokenPrimitive(3),
				ModelGrammar.TokenPrimitive(4),
				ModelGrammar.TokenPrimitive(5),
				ModelGrammar.TokenPrimitive(6),
				ModelGrammar.TokenPrimitive(7),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("compact"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenPrimitive(2),
				ModelGrammar.TokenPrimitive(3),
				ModelGrammar.TokenPrimitive(4),
				ModelGrammar.TokenPrimitive(5),
				ModelGrammar.TokenPrimitive(6),
				ModelGrammar.TokenPrimitive(7),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("jsontext"),
				ModelGrammar.TokenPrimitive("{\"object with 1 member\":[\"array with 1 element\"]}"),
				ModelGrammar.TokenProperty("quotes"),
				ModelGrammar.TokenPrimitive("&#34; \u0022 %22 0x22 034 &#x22;"),
				ModelGrammar.TokenProperty("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				ModelGrammar.TokenPrimitive("A key can be any string"),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenPrimitive(0.5),
				ModelGrammar.TokenPrimitive(98.6),
				ModelGrammar.TokenPrimitive(99.44),
				ModelGrammar.TokenPrimitive(1066),
				ModelGrammar.TokenPrimitive(10.0),
				ModelGrammar.TokenPrimitive(1.0),
				ModelGrammar.TokenPrimitive(0.1),
				ModelGrammar.TokenPrimitive(1.0),
				ModelGrammar.TokenPrimitive(2.0),
				ModelGrammar.TokenPrimitive(2.0),
				ModelGrammar.TokenPrimitive("rosebud"),
				ModelGrammar.TokenArrayEnd
			};

			var expected = new Dictionary<DataName, IEnumerable<Token<ModelTokenType>>>
			{
				{ new DataName("url"), new[] { ModelGrammar.TokenPrimitive("http://www.JSON.org/") } },
				{ new DataName("compact"), new[] {
					ModelGrammar.TokenArrayBeginUnnamed,
					ModelGrammar.TokenPrimitive(1),
					ModelGrammar.TokenPrimitive(2),
					ModelGrammar.TokenPrimitive(3),
					ModelGrammar.TokenPrimitive(4),
					ModelGrammar.TokenPrimitive(5),
					ModelGrammar.TokenPrimitive(6),
					ModelGrammar.TokenPrimitive(7),
					ModelGrammar.TokenArrayEnd,
				} }
			};

			Assert.True(input.IsArray());
			Assert.False(input.IsObject());
			Assert.False(input.IsPrimitive());

			// cherry pick properties
			var actual = input
				.ArrayItems(index => index == 8).FirstOrDefault() // select the big object
				.Properties(name => name.LocalName == "url" || name.LocalName == "compact"); // select two properties

			Assert.Equal(expected, actual, false);
		}

		#endregion Complex Graph Tests
	}
}
