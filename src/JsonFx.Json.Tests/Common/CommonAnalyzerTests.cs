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
		#region Array Tests

		[Fact]
		public void Parse_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new []
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayEnd
			};

			var expected = new object[0];

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd
			};

			var expected = new object[] { null };

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayMultiItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValue(0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenValueDelim,
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
		public void Parse_ArrayNestedDeeply_ReturnsExpectedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenArrayBegin,
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
		public void Parse_ArrayUnclosed_ThrowsAnalyzerException()
		{
			// input from fail2.json in test suite at http://www.json.org/JSON_checker/
			var input = new []
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValue("Unclosed array")
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenNone, ex.Token);
		}

		[Fact]
		public void Parse_ArrayExtraComma_ThrowsAnalyzerException()
		{
			// input from fail4.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValue("extra comma"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenArrayEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenArrayEnd, ex.Token);
		}

		[Fact]
		public void Parse_ArrayDoubleExtraComma_ThrowsAnalyzerException()
		{
			// input from fail5.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValue("double extra comma"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenArrayEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenValueDelim, ex.Token);
		}

		[Fact]
		public void Parse_ArrayMissingValue_ThrowsAnalyzerException()
		{
			// input from fail6.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue("<-- missing value"),
				CommonGrammar.TokenArrayEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenValueDelim, ex.Token);
		}

		[Fact]
		public void Parse_ArrayColonInsteadOfComma_ThrowsAnalyzerException()
		{
			// input from fail22.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenProperty("Colon instead of comma"),
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenArrayEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenProperty("Colon instead of comma"), ex.Token);
		}

		[Fact]
		public void Parse_ArrayCloseMismatch_ThrowsAnalyzerException()
		{
			// input from fail33.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin,
				CommonGrammar.TokenValue("mismatch"),
				CommonGrammar.TokenObjectEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
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
		public void Parse_ObjectEmpty_ReturnsEmptyObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>();

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		public void Parse_ObjectOneProperty_ReturnsSimpleObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin,
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
		public void Parse_ObjectNested_ReturnsNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("JSON Test Pattern pass3"),
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("The outermost value"),
				CommonGrammar.TokenValue("must be an object or array."),
				CommonGrammar.TokenValueDelim,
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
		public void Parse_ObjectExtraComma_ThrowsAnalyzerException()
		{
			// input from fail9.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("Extra comma"),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenObjectEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenObjectEnd, ex.Token);
		}

		[Fact]
		public void Parse_ObjectMissingColon_ThrowsAnalyzerException()
		{
			// input from fail19.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenValue("Missing colon"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenValue("Missing colon"), ex.Token);
		}

		[Fact]
		public void Parse_ObjectDoubleColon_ThrowsAnalyzerException()
		{
			// input from fail20.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("Double colon"),
				CommonGrammar.TokenProperty(""),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonTokenType.Property, ex.Token.TokenType);
		}

		[Fact]
		public void Parse_ObjectUnterminated_ThrowsAnalyzerException()
		{
			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenValue("Comma instead of colon"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectEnd
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(CommonGrammar.TokenValue("Comma instead of colon"), ex.Token);
		}

		[Fact]
		public void Parse_ObjectCommaInsteadOfClose_ThrowsAnalyzerException()
		{
			// input from fail32.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin,
				CommonGrammar.TokenProperty("Comma instead if closing brace"),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenValueDelim
			};

			var analyzer = new CommonAnalyzer(new DataReaderSettings());

			AnalyzerException<CommonTokenType> ex = Assert.Throws<AnalyzerException<CommonTokenType>>(
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
		public void Parse_GraphComplex_ReturnsGraph()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
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
		public void Parse_EmptyInput_ReturnsNothing()
		{
			var input = Enumerable.Empty<Token<CommonTokenType>>();

			var analyzer = new CommonAnalyzer(new DataReaderSettings());
			Assert.False(analyzer.Analyze<object>(input).Any());
		}

		[Fact]
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
