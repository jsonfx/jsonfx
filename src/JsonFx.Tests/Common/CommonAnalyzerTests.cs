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
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization
{
	public class CommonAnalyzerTests
	{
		#region Constants

		private const string TraitName = "Common";
		private const string TraitValue = "Analyzer";

		#endregion Constants

		#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new []
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			var expected = new object[0];

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd
			};

			var expected = new object[] { null };

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ArrayMultiItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue(0),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenArrayEnd
			};

			var expected = new object[]
			{
				0,
				null,
				false,
				true
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ArrayNestedDeeply_ReturnsExpectedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
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

			var expected = new []
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
																						"Not too deep"
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

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze((input)).Cast<string[][][][][][][][][][][][][][][][][][][]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ArrayUnclosed_ThrowsAnalyzerException()
		{
			// input from fail2.json in test suite at http://www.json.org/JSON_checker/
			var input = new []
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("Unclosed array")
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			TokenException<CommonTokenType> ex = Assert.Throws<TokenException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenNone, ex.Token);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_PropertyInsideArray_ThrowsAnalyzerException()
		{
			// input from fail22.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenProperty("Colon instead of comma"),
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenArrayEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			TokenException<CommonTokenType> ex = Assert.Throws<TokenException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenProperty("Colon instead of comma"), ex.Token);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ArrayCloseMismatch_ThrowsAnalyzerException()
		{
			// input from fail33.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("mismatch"),
				CommonGrammar.TokenObjectEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			TokenException<CommonTokenType> ex = Assert.Throws<TokenException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenObjectEnd, ex.Token);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ObjectEmpty_ReturnsEmptyObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>();

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ObjectOneProperty_ReturnsSimpleObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>
				{
					{ "key", "value" }
				};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ObjectNested_ReturnsNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
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

			var expected = new Dictionary<string, object>
				{
					{
						"JSON Test Pattern pass3",
						new Dictionary<string, object>
						{
							{ "The outermost value", "must be an object or array." },
							{ "In this test", "It is an object." }
						}
					}
				};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ObjectMissingColon_ThrowsAnalyzerException()
		{
			// input from fail19.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenValue("Missing colon"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			TokenException<CommonTokenType> ex = Assert.Throws<TokenException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenValue("Missing colon"), ex.Token);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ObjectDoubleColon_ThrowsAnalyzerException()
		{
			// input from fail20.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("Double colon"),
				CommonGrammar.TokenProperty(""),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			TokenException<CommonTokenType> ex = Assert.Throws<TokenException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonTokenType.Property, ex.Token.TokenType);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ValueInsteadOfProperty_ThrowsAnalyzerException()
		{
			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenValue("Comma instead of colon"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			TokenException<CommonTokenType> ex = Assert.Throws<TokenException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenValue("Comma instead of colon"), ex.Token);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_ObjectUnterminated_ThrowsAnalyzerException()
		{
			// input from fail32.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("Comma instead if closing brace"),
				CommonGrammar.TokenTrue
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			TokenException<CommonTokenType> ex = Assert.Throws<TokenException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenNone, ex.Token);
		}

		#endregion Object Tests

		#region Complex Graph Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_GraphComplex_ReturnsGraph()
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

			var expected = new object[] {
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

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual, false);
		}

		#endregion Complex Graph Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_EmptyInput_ReturnsNothing()
		{
			var input = Enumerable.Empty<Token<CommonTokenType>>();

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			Assert.False(analyzer.Analyze<object>(input).Any());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Parse_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<CommonTokenType>>)null;

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected param
			Assert.Equal("tokens", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Ctor_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var analyzer = new CommonAnalyzer(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
