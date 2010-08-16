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

using JsonFx.Common;
using JsonFx.Serialization.GraphCycles;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization
{
	public class CommonWalkerTests
	{
		#region Constants

		private const string TraitName = "Common";
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
					CommonGrammar.TokenArrayBegin("array"),
					CommonGrammar.TokenArrayEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenArrayBegin("array"),
					CommonGrammar.TokenArrayEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenArrayBegin("array"),
					CommonGrammar.TokenNull,
					CommonGrammar.TokenArrayEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenArrayBegin("array"),
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenTrue,
					CommonGrammar.TokenNull,
					CommonGrammar.TokenPrimitive('a'),
					CommonGrammar.TokenPrimitive('b'),
					CommonGrammar.TokenPrimitive('c'),
					CommonGrammar.TokenPrimitive(1),
					CommonGrammar.TokenPrimitive(2),
					CommonGrammar.TokenPrimitive(3),
					CommonGrammar.TokenArrayEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenArrayBegin("array"),
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenTrue,
					CommonGrammar.TokenNull,
					CommonGrammar.TokenArrayBegin("array"),
					CommonGrammar.TokenPrimitive('a'),
					CommonGrammar.TokenPrimitive('b'),
					CommonGrammar.TokenPrimitive('c'),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayBegin("array"),
					CommonGrammar.TokenPrimitive(1),
					CommonGrammar.TokenPrimitive(2),
					CommonGrammar.TokenPrimitive(3),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenArrayEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenObjectBegin("object"),
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenObjectBegin("object"),
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenObjectBegin("object"),
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenPrimitive(1),
					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenPrimitive(2),
					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenPrimitive(3),
					CommonGrammar.TokenProperty("Four"),
					CommonGrammar.TokenObjectBegin("object"),
					CommonGrammar.TokenProperty("A"),
					CommonGrammar.TokenPrimitive('a'),
					CommonGrammar.TokenProperty("B"),
					CommonGrammar.TokenPrimitive('b'),
					CommonGrammar.TokenProperty("C"),
					CommonGrammar.TokenPrimitive('c'),
					CommonGrammar.TokenObjectEnd,
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenObjectBegin("object"),
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenPrimitive(1),
					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenPrimitive(2),
					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenPrimitive(3),
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenFalse
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenTrue
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenPrimitive(Double.NaN)
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenPrimitive(Double.PositiveInfinity)
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenPrimitive(Double.NegativeInfinity)
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenPrimitive(Double.NaN)
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenPrimitive(Double.PositiveInfinity)
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenPrimitive(Double.NegativeInfinity)
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenObjectBegin("object"),
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenPrimitive(1),
					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenPrimitive(2),
					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenPrimitive(3),
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					CommonGrammar.TokenObjectBegin("object"),
					CommonGrammar.TokenProperty("foo"),
					CommonGrammar.TokenPrimitive(true),
					CommonGrammar.TokenProperty("array"),
					CommonGrammar.TokenArrayBegin("array"),
					CommonGrammar.TokenPrimitive(false),
					CommonGrammar.TokenPrimitive(1),
					CommonGrammar.TokenPrimitive(2),
					CommonGrammar.TokenPrimitive(Math.PI),
					CommonGrammar.TokenPrimitive(null),
					CommonGrammar.TokenPrimitive("hello world."),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
				CommonGrammar.TokenObjectBegin("DynamicExample"),
				CommonGrammar.TokenProperty("foo"),
				CommonGrammar.TokenPrimitive("hello world"),
				CommonGrammar.TokenProperty("number"),
				CommonGrammar.TokenPrimitive(42),
				CommonGrammar.TokenProperty("boolean"),
				CommonGrammar.TokenPrimitive(false),
				CommonGrammar.TokenProperty("null"),
				CommonGrammar.TokenPrimitive(null),
				CommonGrammar.TokenObjectEnd
			};

			var walker = new CommonWalker(new DataWriterSettings());
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
				CommonGrammar.TokenArrayBegin("array"),
				CommonGrammar.TokenPrimitive("JSON Test Pattern pass1"),
				CommonGrammar.TokenObjectBegin("object"),
				CommonGrammar.TokenProperty("object with 1 member"),
				CommonGrammar.TokenArrayBegin("array"),
				CommonGrammar.TokenPrimitive("array with 1 element"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectBegin("object"),
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenArrayBegin("array"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenPrimitive(-42),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectBegin("object"),
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
				CommonGrammar.TokenArrayBegin("array"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("object"),
				CommonGrammar.TokenObjectBegin("object"),
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
				CommonGrammar.TokenArrayBegin("array"),
				CommonGrammar.TokenPrimitive(1),
				CommonGrammar.TokenPrimitive(2),
				CommonGrammar.TokenPrimitive(3),
				CommonGrammar.TokenPrimitive(4),
				CommonGrammar.TokenPrimitive(5),
				CommonGrammar.TokenPrimitive(6),
				CommonGrammar.TokenPrimitive(7),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("compact"),
				CommonGrammar.TokenArrayBegin("array"),
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

			var walker = new CommonWalker(new DataWriterSettings());
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

			var walker = new CommonWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.Ignore
			});

			var expected = new[]
			{
				CommonGrammar.TokenObjectBegin("Person"),
				CommonGrammar.TokenProperty("Name"),
				CommonGrammar.TokenPrimitive("John, Jr."),

				CommonGrammar.TokenProperty("Father"),
				CommonGrammar.TokenObjectBegin("Person"),
				CommonGrammar.TokenProperty("Name"),
				CommonGrammar.TokenPrimitive("John, Sr."),
				CommonGrammar.TokenProperty("Father"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenProperty("Mother"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenProperty("Children"),
				CommonGrammar.TokenArrayBegin("array"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,

				CommonGrammar.TokenProperty("Mother"),
				CommonGrammar.TokenObjectBegin("Person"),
				CommonGrammar.TokenProperty("Name"),
				CommonGrammar.TokenPrimitive("Sally"),
				CommonGrammar.TokenProperty("Father"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenProperty("Mother"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenProperty("Children"),
				CommonGrammar.TokenArrayBegin("array"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,

				CommonGrammar.TokenProperty("Children"),
				CommonGrammar.TokenNull,

				CommonGrammar.TokenObjectEnd
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

			var walker = new CommonWalker(new DataWriterSettings
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

			var walker = new CommonWalker(new DataWriterSettings
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

			var walker = new CommonWalker(new DataWriterSettings
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

			var walker = new CommonWalker(new DataWriterSettings
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
					CommonGrammar.TokenNull
				};

			var walker = new CommonWalker(new DataWriterSettings());
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
					var walker = new CommonWalker(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
