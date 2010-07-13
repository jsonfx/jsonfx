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

using JsonFx.Serialization.GraphCycles;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization
{
	public class JsonWalkerTests
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
					DataGrammar.TokenArrayBegin,
					DataGrammar.TokenArrayEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ListEmpty_ReturnsEmptyArrayTokens()
		{
			var input = new List<object>(0);

			var expected = new[]
				{
					DataGrammar.TokenArrayBegin,
					DataGrammar.TokenArrayEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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
					DataGrammar.TokenArrayBegin,
					DataGrammar.TokenNull,
					DataGrammar.TokenArrayEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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
					DataGrammar.TokenArrayBegin,
					DataGrammar.TokenFalse,
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenTrue,
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenNull,
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue('a'),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue('b'),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue('c'),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue(1),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue(2),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue(3),
					DataGrammar.TokenArrayEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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
					DataGrammar.TokenArrayBegin,
					DataGrammar.TokenFalse,
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenTrue,
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenNull,
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenArrayBegin,
					DataGrammar.TokenValue('a'),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue('b'),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue('c'),
					DataGrammar.TokenArrayEnd,
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenArrayBegin,
					DataGrammar.TokenValue(1),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue(2),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenValue(3),
					DataGrammar.TokenArrayEnd,
					DataGrammar.TokenArrayEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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
					DataGrammar.TokenObjectBegin,
					DataGrammar.TokenObjectEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyDictionary_ReturnsEmptyObjectTokens()
		{
			var input = new Dictionary<string,object>(0);

			var expected = new[]
				{
					DataGrammar.TokenObjectBegin,
					DataGrammar.TokenObjectEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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
					DataGrammar.TokenObjectBegin,
					DataGrammar.TokenProperty("One"),
					DataGrammar.TokenValue(1),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenProperty("Two"),
					DataGrammar.TokenValue(2),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenProperty("Three"),
					DataGrammar.TokenValue(3),
					DataGrammar.TokenObjectEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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
					DataGrammar.TokenObjectBegin,
					DataGrammar.TokenProperty("One"),
					DataGrammar.TokenValue(1),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenProperty("Two"),
					DataGrammar.TokenValue(2),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenProperty("Three"),
					DataGrammar.TokenValue(3),
					DataGrammar.TokenObjectEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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
					DataGrammar.TokenObjectBegin,
					DataGrammar.TokenProperty("One"),
					DataGrammar.TokenValue(1),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenProperty("Two"),
					DataGrammar.TokenValue(2),
					DataGrammar.TokenValueDelim,
					DataGrammar.TokenProperty("Three"),
					DataGrammar.TokenValue(3),
					DataGrammar.TokenObjectEnd
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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
					DataGrammar.TokenFalse
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_True_ReturnsTrueToken()
		{
			var input = true;

			var expected = new[]
				{
					DataGrammar.TokenTrue
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Boolean Tests

		#region Number Case Tests

		[Fact]
		public void GetTokens_NaN_ReturnsNaNToken()
		{
			var input = Double.NaN;

			var expected = new[]
				{
					DataGrammar.TokenNaN
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_PosInfinity_ReturnsPosInfinityToken()
		{
			var input = Double.PositiveInfinity;

			var expected = new[]
				{
					DataGrammar.TokenPositiveInfinity
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NegInfinity_ReturnsNegInfinityToken()
		{
			var input = Double.NegativeInfinity;

			var expected = new[]
				{
					DataGrammar.TokenNegativeInfinity
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("JSON Test Pattern pass1"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("object with 1 member"),
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("array with 1 element"),
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenObjectEnd,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenObjectEnd,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(-42),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenTrue,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenFalse,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenNull,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("integer"),
				DataGrammar.TokenValue(1234567890),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("real"),
				DataGrammar.TokenValue(-9876.543210),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("e"),
				DataGrammar.TokenValue(0.123456789e-12),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("E"),
				DataGrammar.TokenValue(1.234567890E+34),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty(""),
				DataGrammar.TokenValue(23456789012E66),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("zero"),
				DataGrammar.TokenValue(0),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("one"),
				DataGrammar.TokenValue(1),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("space"),
				DataGrammar.TokenValue(" "),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("quote"),
				DataGrammar.TokenValue("\""),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("backslash"),
				DataGrammar.TokenValue("\\"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("controls"),
				DataGrammar.TokenValue("\b\f\n\r\t"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("slash"),
				DataGrammar.TokenValue("/ & /"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("alpha"),
				DataGrammar.TokenValue("abcdefghijklmnopqrstuvwyz"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("ALPHA"),
				DataGrammar.TokenValue("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("digit"),
				DataGrammar.TokenValue("0123456789"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("0123456789"),
				DataGrammar.TokenValue("digit"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("special"),
				DataGrammar.TokenValue("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("hex"),
				DataGrammar.TokenValue("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("true"),
				DataGrammar.TokenTrue,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("false"),
				DataGrammar.TokenFalse,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("null"),
				DataGrammar.TokenNull,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("array"),
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("object"),
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenObjectEnd,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("address"),
				DataGrammar.TokenValue("50 St. James Street"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("url"),
				DataGrammar.TokenValue("http://www.JSON.org/"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("comment"),
				DataGrammar.TokenValue("// /* <!-- --"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("# -- --> */"),
				DataGrammar.TokenValue(" "),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty(" s p a c e d "),
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue(1),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(2),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(3),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(4),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(5),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(6),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(7),
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("compact"),
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue(1),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(2),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(3),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(4),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(5),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(6),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(7),
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("jsontext"),
				DataGrammar.TokenValue("{\"object with 1 member\":[\"array with 1 element\"]}"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("quotes"),
				DataGrammar.TokenValue("&#34; \u0022 %22 0x22 034 &#x22;"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				DataGrammar.TokenValue("A key can be any string"),
				DataGrammar.TokenObjectEnd,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(0.5),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(98.6),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(99.44),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(1066),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(10.0),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(1.0),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(0.1),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(1.0),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(2.0),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue(2.0),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue("rosebud"),
				DataGrammar.TokenArrayEnd
			};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
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

			var walker = new DataWriter.DataWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.Ignore
			});

			var expected = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("Name"),
				DataGrammar.TokenValue("John, Jr."),
				DataGrammar.TokenValueDelim,

				DataGrammar.TokenProperty("Father"),
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("Name"),
				DataGrammar.TokenValue("John, Sr."),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("Father"),
				DataGrammar.TokenNull,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("Mother"),
				DataGrammar.TokenNull,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("Children"),
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenNull,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenObjectEnd,
				DataGrammar.TokenValueDelim,

				DataGrammar.TokenProperty("Mother"),
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("Name"),
				DataGrammar.TokenValue("Sally"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("Father"),
				DataGrammar.TokenNull,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("Mother"),
				DataGrammar.TokenNull,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("Children"),
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenNull,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenObjectEnd,
				DataGrammar.TokenValueDelim,

				DataGrammar.TokenProperty("Children"),
				DataGrammar.TokenNull,

				DataGrammar.TokenObjectEnd
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

			var walker = new DataWriter.DataWalker(new DataWriterSettings
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

			var walker = new DataWriter.DataWalker(new DataWriterSettings
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

			var walker = new DataWriter.DataWalker(new DataWriterSettings
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

			var walker = new DataWriter.DataWalker(new DataWriterSettings
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
					DataGrammar.TokenNull
				};

			var walker = new DataWriter.DataWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Ctor_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var walker = new DataWriter.DataWalker(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
