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
using System.Dynamic;
using System.Linq;

using JsonFx.Model;
using JsonFx.Serialization.GraphCycles;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization
{
	public class ModelWalkerTests
	{
		#region Constants

		private const string TraitName = "Common Model";
		private const string TraitValue = "Walker";

		#endregion Constants

		#region Test Types

		public class Person
		{
			public string Name { get; set; }
			public Person Father { get; set; }
			public Person Mother { get; set; }
			public Person[] Children { get; set; }
		}

		public class DynamicExample : DynamicObject
		{
			internal readonly Dictionary<string, object> Values = new Dictionary<string, object>();

			public override IEnumerable<string> GetDynamicMemberNames()
			{
				return Values.Keys;
			}

			public override bool TryDeleteMember(DeleteMemberBinder binder)
			{
				return this.Values.Remove(binder.Name);
			}

			public override bool TryGetMember(GetMemberBinder binder, out object result)
			{
				return this.Values.TryGetValue(binder.Name, out result);
			}

			public override bool TrySetMember(SetMemberBinder binder, object value)
			{
				this.Values[binder.Name] = value;
				return true;
			}
		}

		#endregion Test Types

		#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayEmpty_ReturnsEmptyArrayTokens()
		{
			var input = new object[0];

			var expected = new[]
				{
					ModelGrammar.TokenArrayBegin("array"),
					ModelGrammar.TokenArrayEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ListEmpty_ReturnsEmptyArrayTokens()
		{
			var input = new List<object>(0);

			var expected = new[]
				{
					ModelGrammar.TokenArrayBegin("array"),
					ModelGrammar.TokenArrayEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArraySingleItem_ReturnsSingleItemArrayTokens()
		{
			var input = new object[]
				{
					null
				};

			var expected = new[]
				{
					ModelGrammar.TokenArrayBegin("array"),
					ModelGrammar.TokenNull,
					ModelGrammar.TokenArrayEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayMultiItem_ReturnsArrayTokens()
		{
			var input = new object[]
				{
					false,
					true,
					null,
					'a',
					'b',
					'c',
					1,
					2,
					3
				};

			var expected = new[]
				{
					ModelGrammar.TokenArrayBegin("array"),
					ModelGrammar.TokenFalse,
					ModelGrammar.TokenTrue,
					ModelGrammar.TokenNull,
					ModelGrammar.TokenPrimitive('a'),
					ModelGrammar.TokenPrimitive('b'),
					ModelGrammar.TokenPrimitive('c'),
					ModelGrammar.TokenPrimitive(1),
					ModelGrammar.TokenPrimitive(2),
					ModelGrammar.TokenPrimitive(3),
					ModelGrammar.TokenArrayEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayNested_ReturnsNestedArrayTokens()
		{
			var input = new object[]
				{
					false,
					true,
					null,
					new []
					{
						'a',
						'b',
						'c'
					},
					new []
					{
						1,
						2,
						3
					}
				};

			var expected = new[]
				{
					ModelGrammar.TokenArrayBegin("array"),
					ModelGrammar.TokenFalse,
					ModelGrammar.TokenTrue,
					ModelGrammar.TokenNull,
					ModelGrammar.TokenArrayBegin("array"),
					ModelGrammar.TokenPrimitive('a'),
					ModelGrammar.TokenPrimitive('b'),
					ModelGrammar.TokenPrimitive('c'),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayBegin("array"),
					ModelGrammar.TokenPrimitive(1),
					ModelGrammar.TokenPrimitive(2),
					ModelGrammar.TokenPrimitive(3),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenArrayEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyObject_ReturnsEmptyObjectTokens()
		{
			var input = new object();

			var expected = new[]
				{
					ModelGrammar.TokenObjectBegin("object"),
					ModelGrammar.TokenObjectEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyDictionary_ReturnsEmptyObjectTokens()
		{
			var input = new Dictionary<string, object>(0);

			var expected = new[]
				{
					ModelGrammar.TokenObjectBegin("object"),
					ModelGrammar.TokenObjectEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ObjectAnonymous_ReturnsObjectTokens()
		{
			var input = new
			{
				One = 1,
				Two = 2,
				Three = 3,
				Four = new
				{
					A = 'a',
					B = 'b',
					C = 'c'
				}
			};

			var expected = new[]
				{
					ModelGrammar.TokenObjectBegin("object"),
					ModelGrammar.TokenProperty("One"),
					ModelGrammar.TokenPrimitive(1),
					ModelGrammar.TokenProperty("Two"),
					ModelGrammar.TokenPrimitive(2),
					ModelGrammar.TokenProperty("Three"),
					ModelGrammar.TokenPrimitive(3),
					ModelGrammar.TokenProperty("Four"),
					ModelGrammar.TokenObjectBegin("object"),
					ModelGrammar.TokenProperty("A"),
					ModelGrammar.TokenPrimitive('a'),
					ModelGrammar.TokenProperty("B"),
					ModelGrammar.TokenPrimitive('b'),
					ModelGrammar.TokenProperty("C"),
					ModelGrammar.TokenPrimitive('c'),
					ModelGrammar.TokenObjectEnd,
					ModelGrammar.TokenObjectEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ObjectDictionary_ReturnsObjectTokens()
		{
			var input = new Dictionary<string, object>
			{
				{ "One", 1 },
				{ "Two", 2 },
				{ "Three", 3 }
			};

			var expected = new[]
				{
					ModelGrammar.TokenObjectBegin("object"),
					ModelGrammar.TokenProperty("One"),
					ModelGrammar.TokenPrimitive(1),
					ModelGrammar.TokenProperty("Two"),
					ModelGrammar.TokenPrimitive(2),
					ModelGrammar.TokenProperty("Three"),
					ModelGrammar.TokenPrimitive(3),
					ModelGrammar.TokenObjectEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region Boolean Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_False_ReturnsFalseToken()
		{
			var input = false;

			var expected = new[]
				{
					ModelGrammar.TokenFalse
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_True_ReturnsTrueToken()
		{
			var input = true;

			var expected = new[]
				{
					ModelGrammar.TokenTrue
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Boolean Tests

		#region Number Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DoubleNaN_ReturnsNaNToken()
		{
			var input = Double.NaN;

			var expected = new[]
				{
					ModelGrammar.TokenPrimitive(Double.NaN)
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DoublePosInfinity_ReturnsPosInfinityToken()
		{
			var input = Double.PositiveInfinity;

			var expected = new[]
				{
					ModelGrammar.TokenPrimitive(Double.PositiveInfinity)
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DoubleNegInfinity_ReturnsNegInfinityToken()
		{
			var input = Double.NegativeInfinity;

			var expected = new[]
				{
					ModelGrammar.TokenPrimitive(Double.NegativeInfinity)
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleNaN_ReturnsNaNToken()
		{
			var input = Single.NaN;

			var expected = new[]
				{
					ModelGrammar.TokenPrimitive(Double.NaN)
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SinglePosInfinity_ReturnsPosInfinityToken()
		{
			var input = Single.PositiveInfinity;

			var expected = new[]
				{
					ModelGrammar.TokenPrimitive(Double.PositiveInfinity)
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleNegInfinity_ReturnsNegInfinityToken()
		{
			var input = Single.NegativeInfinity;

			var expected = new[]
				{
					ModelGrammar.TokenPrimitive(Double.NegativeInfinity)
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Number Tests

		#region Dynamic Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ExpandoObject_ReturnsObjectTokens()
		{
			dynamic input = new ExpandoObject();
			input.One = 1;
			input.Two = 2;
			input.Three = 3;

			var expected = new[]
				{
					ModelGrammar.TokenObjectBegin("object"),
					ModelGrammar.TokenProperty("One"),
					ModelGrammar.TokenPrimitive(1),
					ModelGrammar.TokenProperty("Two"),
					ModelGrammar.TokenPrimitive(2),
					ModelGrammar.TokenProperty("Three"),
					ModelGrammar.TokenPrimitive(3),
					ModelGrammar.TokenObjectEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens((object)input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ExpandoObjectNested_ReturnsObjectTokens()
		{
			dynamic input = new ExpandoObject();

			input.foo = true;
			input.array = new object[]
			{
				false, 1, 2, Math.PI, null, "hello world."
			};

			var expected = new[]
				{
					ModelGrammar.TokenObjectBegin("object"),
					ModelGrammar.TokenProperty("foo"),
					ModelGrammar.TokenPrimitive(true),
					ModelGrammar.TokenProperty("array"),
					ModelGrammar.TokenArrayBegin("array"),
					ModelGrammar.TokenPrimitive(false),
					ModelGrammar.TokenPrimitive(1),
					ModelGrammar.TokenPrimitive(2),
					ModelGrammar.TokenPrimitive(Math.PI),
					ModelGrammar.TokenPrimitive(null),
					ModelGrammar.TokenPrimitive("hello world."),
					ModelGrammar.TokenArrayEnd,
					ModelGrammar.TokenObjectEnd
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DynamicExample_ReturnsDynamicObject()
		{
			dynamic input = new DynamicExample();
			input.foo = "hello world";
			input.number = 42;
			input.boolean = false;
			input.@null = null;

			var expected = new[]
			{
				ModelGrammar.TokenObjectBegin("DynamicExample"),
				ModelGrammar.TokenProperty("foo"),
				ModelGrammar.TokenPrimitive("hello world"),
				ModelGrammar.TokenProperty("number"),
				ModelGrammar.TokenPrimitive(42),
				ModelGrammar.TokenProperty("boolean"),
				ModelGrammar.TokenPrimitive(false),
				ModelGrammar.TokenProperty("null"),
				ModelGrammar.TokenPrimitive(null),
				ModelGrammar.TokenObjectEnd
			};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion Dynamic Tests

		#region Complex Graph Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_GraphComplex_ReturnsObjectTokens()
		{
			var input = new object[] {
				"JSON Test Pattern pass1",
				new Dictionary<string, object>
				{
					{ "object with 1 member", new[] { "array with 1 element" } },
				},
				new Dictionary<string, object>(),
				new object[0],
				-42,
				true,
				false,
				null,
				new Dictionary<string, object> {
					{ "integer", 1234567890 },
					{ "real", -9876.543210 },
					{ "e", 0.123456789e-12 },
					{ "E", 1.234567890E+34 },
					{ "", 23456789012E66 },
					{ "zero", 0 },
					{ "one", 1 },
					{ "space", " " },
					{ "quote", "\"" },
					{ "backslash", "\\" },
					{ "controls", "\b\f\n\r\t" },
					{ "slash", "/ & /" },
					{ "alpha", "abcdefghijklmnopqrstuvwyz" },
					{ "ALPHA", "ABCDEFGHIJKLMNOPQRSTUVWYZ" },
					{ "digit", "0123456789" },
					{ "0123456789", "digit" },
					{ "special", "`1~!@#$%^&*()_+-={':[,]}|;.</>?" },
					{ "hex", "\u0123\u4567\u89AB\uCDEF\uabcd\uef4A" },
					{ "true", true },
					{ "false", false },
					{ "null", null },
					{ "array", new object[0] },
					{ "object", new Dictionary<string, object>() },
					{ "address", "50 St. James Street" },
					{ "url", "http://www.JSON.org/" },
					{ "comment", "// /* <!-- --" },
					{ "# -- --> */", " " },
					{ " s p a c e d ", new [] { 1,2,3,4,5,6,7 } },
					{ "compact", new [] { 1,2,3,4,5,6,7 } },
					{ "jsontext", "{\"object with 1 member\":[\"array with 1 element\"]}" },
					{ "quotes", "&#34; \u0022 %22 0x22 034 &#x22;" },
					{ "/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?", "A key can be any string" }
				},
				0.5,
				98.6,
				99.44,
				1066,
				1e1,
				0.1e1,
				1e-1,
				1e00,
				2e+00,
				2e-00,
				"rosebud"
			};

			var expected = new[]
			{
				ModelGrammar.TokenArrayBegin("array"),
				ModelGrammar.TokenPrimitive("JSON Test Pattern pass1"),
				ModelGrammar.TokenObjectBegin("object"),
				ModelGrammar.TokenProperty("object with 1 member"),
				ModelGrammar.TokenArrayBegin("array"),
				ModelGrammar.TokenPrimitive("array with 1 element"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenObjectBegin("object"),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenArrayBegin("array"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenPrimitive(-42),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenObjectBegin("object"),
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
				ModelGrammar.TokenArrayBegin("array"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("object"),
				ModelGrammar.TokenObjectBegin("object"),
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
				ModelGrammar.TokenArrayBegin("array"),
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenPrimitive(2),
				ModelGrammar.TokenPrimitive(3),
				ModelGrammar.TokenPrimitive(4),
				ModelGrammar.TokenPrimitive(5),
				ModelGrammar.TokenPrimitive(6),
				ModelGrammar.TokenPrimitive(7),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("compact"),
				ModelGrammar.TokenArrayBegin("array"),
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

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Complex Graph Tests

		#region Graph Cycles Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_GraphCycleTypeIgnore_ReplacesCycleStartWithNull()
		{
			var input = new Person
			{
				Name = "John, Jr.",
				Father = new Person
				{
					Name = "John, Sr."
				},
				Mother = new Person
				{
					Name = "Sally"
				}
			};

			// create multiple cycles
			input.Father.Children = input.Mother.Children = new Person[]
			{
				input
			};

			var walker = new ModelWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.Ignore
			});

			var expected = new[]
			{
				ModelGrammar.TokenObjectBegin("Person"),
				ModelGrammar.TokenProperty("Name"),
				ModelGrammar.TokenPrimitive("John, Jr."),

				ModelGrammar.TokenProperty("Father"),
				ModelGrammar.TokenObjectBegin("Person"),
				ModelGrammar.TokenProperty("Name"),
				ModelGrammar.TokenPrimitive("John, Sr."),
				ModelGrammar.TokenProperty("Father"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenProperty("Mother"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenProperty("Children"),
				ModelGrammar.TokenArrayBegin("array"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenObjectEnd,

				ModelGrammar.TokenProperty("Mother"),
				ModelGrammar.TokenObjectBegin("Person"),
				ModelGrammar.TokenProperty("Name"),
				ModelGrammar.TokenPrimitive("Sally"),
				ModelGrammar.TokenProperty("Father"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenProperty("Mother"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenProperty("Children"),
				ModelGrammar.TokenArrayBegin("array"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenObjectEnd,

				ModelGrammar.TokenProperty("Children"),
				ModelGrammar.TokenNull,

				ModelGrammar.TokenObjectEnd
			};

			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_GraphCycleTypeReferences_ThrowsGraphCycleException()
		{
			var input = new Person
			{
				Name = "John, Jr.",
				Father = new Person
				{
					Name = "John, Sr."
				},
				Mother = new Person
				{
					Name = "Sally"
				}
			};

			// create multiple cycles
			input.Father.Children = input.Mother.Children = new Person[]
			{
				input
			};

			var walker = new ModelWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.Reference
			});

			GraphCycleException ex = Assert.Throws<GraphCycleException>(
				delegate
				{
					walker.GetTokens(input).ToArray();
				});

			Assert.Equal(GraphCycleType.Reference, ex.CycleType);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_GraphCycleTypeMaxDepth_ThrowsGraphCycleException()
		{
			var input = new Person
			{
				Name = "John, Jr.",
				Father = new Person
				{
					Name = "John, Sr."
				},
				Mother = new Person
				{
					Name = "Sally"
				}
			};

			// create multiple cycles
			input.Father.Children = input.Mother.Children = new Person[]
			{
				input
			};

			var walker = new ModelWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.MaxDepth,
				MaxDepth = 25
			});

			GraphCycleException ex = Assert.Throws<GraphCycleException>(
				delegate
				{
					walker.GetTokens(input).ToArray();
				});

			Assert.Equal(GraphCycleType.MaxDepth, ex.CycleType);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_GraphCycleTypeMaxDepthNoMaxDepth_ThrowsArgumentException()
		{
			var input = new Person
			{
				Name = "John, Jr.",
				Father = new Person
				{
					Name = "John, Sr."
				},
				Mother = new Person
				{
					Name = "Sally"
				}
			};

			// create multiple cycles
			input.Father.Children = input.Mother.Children = new Person[]
			{
				input
			};

			var walker = new ModelWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.MaxDepth,
				MaxDepth = 0
			});

			ArgumentException ex = Assert.Throws<ArgumentException>(
				delegate
				{
					walker.GetTokens(input).ToArray();
				});

			Assert.Equal("maxDepth", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_GraphCycleTypeMaxDepthFalsePositive_ThrowsGraphCycleException()
		{
			// input from fail18.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				new []
				{
					new []
					{
						new []
						{
							new []
							{
								new []
								{
									new []
									{
										new []
										{
											new []
											{
												new []
												{
													new []
													{
														new []
														{
															new []
															{
																new []
																{
																	new []
																	{
																		new []
																		{
																			new []
																			{
																				new []
																				{
																					new []
																					{
																						new []
																						{
																							"Too deep"
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			};

			var walker = new ModelWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.MaxDepth,
				MaxDepth = 19
			});

			GraphCycleException ex = Assert.Throws<GraphCycleException>(
				delegate
				{
					walker.GetTokens(input).ToArray();
				});

			Assert.Equal(GraphCycleType.MaxDepth, ex.CycleType);
		}

		#endregion Graph Cycles Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_Null_ReturnsNullToken()
		{
			var input = (object)null;

			var expected = new[]
				{
					ModelGrammar.TokenNull
				};

			var walker = new ModelWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Ctor_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var walker = new ModelWalker(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
