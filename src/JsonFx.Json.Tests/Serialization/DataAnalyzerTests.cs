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

using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization
{
	public class DataAnalyzerTests
	{
		#region Array Tests

		[Fact]
		public void Parse_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new []
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayEnd
			};

			var expected = new object[0];

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenNull,
				DataGrammar.TokenArrayEnd
			};

			var expected = new object[] { null };

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayMultiItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue(0),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenNull,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenFalse,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenTrue,
				DataGrammar.TokenArrayEnd
			};

			var expected = new object[]
			{
				0,
				null,
				false,
				true
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayNestedDeeply_ReturnsExpectedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("Not too deep"),
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd,
				DataGrammar.TokenArrayEnd
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

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze((input)).Cast<string[][][][][][][][][][][][][][][][][][][]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayUnclosed_ThrowsAnalyzerException()
		{
			// input from fail2.json in test suite at http://www.json.org/JSON_checker/
			var input = new []
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("Unclosed array")
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenNone, ex.Token);
		}

		[Fact]
		public void Parse_ArrayExtraComma_ThrowsAnalyzerException()
		{
			// input from fail4.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("extra comma"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenArrayEnd
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenArrayEnd, ex.Token);
		}

		[Fact]
		public void Parse_ArrayDoubleExtraComma_ThrowsAnalyzerException()
		{
			// input from fail5.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("double extra comma"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenArrayEnd
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenValueDelim, ex.Token);
		}

		[Fact]
		public void Parse_ArrayMissingValue_ThrowsAnalyzerException()
		{
			// input from fail6.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue("<-- missing value"),
				DataGrammar.TokenArrayEnd
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenValueDelim, ex.Token);
		}

		[Fact]
		public void Parse_ArrayColonInsteadOfComma_ThrowsAnalyzerException()
		{
			// input from fail22.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenProperty("Colon instead of comma"),
				DataGrammar.TokenFalse,
				DataGrammar.TokenArrayEnd
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenProperty("Colon instead of comma"), ex.Token);
		}

		[Fact]
		public void Parse_ArrayCloseMismatch_ThrowsAnalyzerException()
		{
			// input from fail33.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("mismatch"),
				DataGrammar.TokenObjectEnd
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenObjectEnd, ex.Token);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		public void Parse_ObjectEmpty_ReturnsEmptyObject()
		{
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>();

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		public void Parse_ObjectOneProperty_ReturnsSimpleObject()
		{
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("key"),
				DataGrammar.TokenValue("value"),
				DataGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>
				{
					{ "key", "value" }
				};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		public void Parse_ObjectNested_ReturnsNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("JSON Test Pattern pass3"),
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("The outermost value"),
				DataGrammar.TokenValue("must be an object or array."),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("In this test"),
				DataGrammar.TokenValue("It is an object."),
				DataGrammar.TokenObjectEnd,
				DataGrammar.TokenObjectEnd
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

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		public void Parse_ObjectExtraComma_ThrowsAnalyzerException()
		{
			// input from fail9.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("Extra comma"),
				DataGrammar.TokenTrue,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenObjectEnd
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenObjectEnd, ex.Token);
		}

		[Fact]
		public void Parse_ObjectMissingColon_ThrowsAnalyzerException()
		{
			// input from fail19.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenValue("Missing colon"),
				DataGrammar.TokenNull,
				DataGrammar.TokenObjectEnd
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenValue("Missing colon"), ex.Token);
		}

		[Fact]
		public void Parse_ObjectDoubleColon_ThrowsAnalyzerException()
		{
			// input from fail20.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("Double colon"),
				DataGrammar.TokenProperty(""),
				DataGrammar.TokenNull,
				DataGrammar.TokenObjectEnd
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataTokenType.PropertyKey, ex.Token.TokenType);
		}

		[Fact]
		public void Parse_ObjectUnterminated_ThrowsAnalyzerException()
		{
			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenValue("Comma instead of colon"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenNull,
				DataGrammar.TokenObjectEnd
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenValue("Comma instead of colon"), ex.Token);
		}

		[Fact]
		public void Parse_ObjectCommaInsteadOfClose_ThrowsAnalyzerException()
		{
			// input from fail32.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("Comma instead if closing brace"),
				DataGrammar.TokenTrue,
				DataGrammar.TokenValueDelim
			};

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

			AnalyzerException<DataTokenType> ex = Assert.Throws<AnalyzerException<DataTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(DataGrammar.TokenNone, ex.Token);
		}

		#endregion Object Tests

		#region Complex Graph Tests

		[Fact]
		public void Parse_GraphComplex_ReturnsGraph()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
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

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual, false);
		}

		#endregion Complex Graph Tests

		#region Input Edge Case Tests

		[Fact]
		public void Parse_EmptyInput_ReturnsNothing()
		{
			var input = Enumerable.Empty<Token<DataTokenType>>();

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());
			Assert.False(analyzer.Analyze<object>(input).Any());
		}

		[Fact]
		public void Parse_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<DataTokenType>>)null;

			var analyzer = new DataReader.DataAnalyzer(new DataReaderSettings());

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
					var analyzer = new DataReader.DataAnalyzer(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
