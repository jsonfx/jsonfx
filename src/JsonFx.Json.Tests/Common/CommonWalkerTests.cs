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
using JsonFx.Serialization.GraphCycles;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization
{
	public class CommonWalkerTests
	{
		#region Test Types

		public class Person
		{
			public string Name { get; set; }
			public Person Father { get; set; }
			public Person Mother { get; set; }
			public Person[] Children { get; set; }
		}

		#endregion Test Types

		#region Array Tests

		[Fact]
		public void GetTokens_ArrayEmpty_ReturnsEmptyArrayTokens()
		{
			var input = new object[0];

			var expected = new[]
				{
					CommonGrammar.TokenArrayBegin,
					CommonGrammar.TokenArrayEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ListEmpty_ReturnsEmptyArrayTokens()
		{
			var input = new List<object>(0);

			var expected = new[]
				{
					CommonGrammar.TokenArrayBegin,
					CommonGrammar.TokenArrayEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArraySingleItem_ReturnsSingleItemArrayTokens()
		{
			var input = new object[]
				{
					null
				};

			var expected = new[]
				{
					CommonGrammar.TokenArrayBegin,
					CommonGrammar.TokenNull,
					CommonGrammar.TokenArrayEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
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
					CommonGrammar.TokenArrayBegin,
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenTrue,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenNull,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue('a'),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue('b'),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue('c'),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue(1),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue(2),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue(3),
					CommonGrammar.TokenArrayEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
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
					CommonGrammar.TokenArrayBegin,
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenTrue,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenNull,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBegin,
					CommonGrammar.TokenValue('a'),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue('b'),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue('c'),
					CommonGrammar.TokenArrayEnd,
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenArrayBegin,
					CommonGrammar.TokenValue(1),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue(2),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenValue(3),
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
		public void GetTokens_EmptyObject_ReturnsEmptyObjectTokens()
		{
			var input = new object();

			var expected = new[]
				{
					CommonGrammar.TokenObjectBegin,
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyDictionary_ReturnsEmptyObjectTokens()
		{
			var input = new Dictionary<string,object>(0);

			var expected = new[]
				{
					CommonGrammar.TokenObjectBegin,
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectAnonymous_ReturnsObjectTokens()
		{
			var input = new
			{
				One = 1,
				Two = 2,
				Three = 3
			};

			var expected = new[]
				{
					CommonGrammar.TokenObjectBegin,
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenValue(1),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenValue(2),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenValue(3),
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectDynamic_ReturnsObjectTokens()
		{
			dynamic input = new System.Dynamic.ExpandoObject();
			input.One = 1;
			input.Two = 2;
			input.Three = 3;

			var expected = new[]
				{
					CommonGrammar.TokenObjectBegin,
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenValue(1),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenValue(2),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenValue(3),
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens((object)input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
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
					CommonGrammar.TokenObjectBegin,
					CommonGrammar.TokenProperty("One"),
					CommonGrammar.TokenValue(1),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenProperty("Two"),
					CommonGrammar.TokenValue(2),
					CommonGrammar.TokenValueDelim,
					CommonGrammar.TokenProperty("Three"),
					CommonGrammar.TokenValue(3),
					CommonGrammar.TokenObjectEnd
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region Boolean Tests

		[Fact]
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
		public void GetTokens_DoubleNaN_ReturnsNaNToken()
		{
			var input = Double.NaN;

			var expected = new[]
				{
					CommonGrammar.TokenValue(Double.NaN)
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_DoublePosInfinity_ReturnsPosInfinityToken()
		{
			var input = Double.PositiveInfinity;

			var expected = new[]
				{
					CommonGrammar.TokenValue(Double.PositiveInfinity)
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_DoubleNegInfinity_ReturnsNegInfinityToken()
		{
			var input = Double.NegativeInfinity;

			var expected = new[]
				{
					CommonGrammar.TokenValue(Double.NegativeInfinity)
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleNaN_ReturnsNaNToken()
		{
			var input = Single.NaN;

			var expected = new[]
				{
					CommonGrammar.TokenValue(Double.NaN)
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SinglePosInfinity_ReturnsPosInfinityToken()
		{
			var input = Single.PositiveInfinity;

			var expected = new[]
				{
					CommonGrammar.TokenValue(Double.PositiveInfinity)
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleNegInfinity_ReturnsNegInfinityToken()
		{
			var input = Single.NegativeInfinity;

			var expected = new[]
				{
					CommonGrammar.TokenValue(Double.NegativeInfinity)
				};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Number Tests

		#region Complex Graph Tests

		[Fact]
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
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValue("JSON Test Pattern pass1"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("object with 1 member"),
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValue("array with 1 element"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(-42),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("integer"),
				CommonGrammar.TokenValue(1234567890),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("real"),
				CommonGrammar.TokenValue(-9876.543210),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("e"),
				CommonGrammar.TokenValue(0.123456789e-12),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("E"),
				CommonGrammar.TokenValue(1.234567890E+34),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty(""),
				CommonGrammar.TokenValue(23456789012E66),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("zero"),
				CommonGrammar.TokenValue(0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("one"),
				CommonGrammar.TokenValue(1),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("space"),
				CommonGrammar.TokenValue(" "),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("quote"),
				CommonGrammar.TokenValue("\""),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("backslash"),
				CommonGrammar.TokenValue("\\"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("controls"),
				CommonGrammar.TokenValue("\b\f\n\r\t"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("slash"),
				CommonGrammar.TokenValue("/ & /"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("alpha"),
				CommonGrammar.TokenValue("abcdefghijklmnopqrstuvwyz"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("ALPHA"),
				CommonGrammar.TokenValue("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("digit"),
				CommonGrammar.TokenValue("0123456789"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("0123456789"),
				CommonGrammar.TokenValue("digit"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("special"),
				CommonGrammar.TokenValue("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("hex"),
				CommonGrammar.TokenValue("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("true"),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("false"),
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("null"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("array"),
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("object"),
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("address"),
				CommonGrammar.TokenValue("50 St. James Street"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("url"),
				CommonGrammar.TokenValue("http://www.JSON.org/"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("comment"),
				CommonGrammar.TokenValue("// /* <!-- --"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("# -- --> */"),
				CommonGrammar.TokenValue(" "),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty(" s p a c e d "),
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValue(1),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(2),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(3),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(4),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(5),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(6),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(7),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("compact"),
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValue(1),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(2),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(3),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(4),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(5),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(6),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(7),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("jsontext"),
				CommonGrammar.TokenValue("{\"object with 1 member\":[\"array with 1 element\"]}"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("quotes"),
				CommonGrammar.TokenValue("&#34; \u0022 %22 0x22 034 &#x22;"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				CommonGrammar.TokenValue("A key can be any string"),
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(0.5),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(98.6),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(99.44),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(1066),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(10.0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(1.0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(0.1),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(1.0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(2.0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(2.0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue("rosebud"),
				CommonGrammar.TokenArrayEnd
			};

			var walker = new CommonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Complex Graph Tests

		#region Graph Cycles Tests

		[Fact]
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
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("Name"),
				CommonGrammar.TokenValue("John, Jr."),
				CommonGrammar.TokenValueDelim,

				CommonGrammar.TokenProperty("Father"),
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("Name"),
				CommonGrammar.TokenValue("John, Sr."),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("Father"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("Mother"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("Children"),
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,

				CommonGrammar.TokenProperty("Mother"),
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("Name"),
				CommonGrammar.TokenValue("Sally"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("Father"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("Mother"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("Children"),
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,

				CommonGrammar.TokenProperty("Children"),
				CommonGrammar.TokenNull,

				CommonGrammar.TokenObjectEnd
			};

			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
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
