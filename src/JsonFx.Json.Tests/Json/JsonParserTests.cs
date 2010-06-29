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

namespace JsonFx.Json
{
	public class JsonParserTests
	{
		#region Test Types

		private enum ExampleEnum
		{
			[DataName("zero")]
			Zero = 0,

			[DataName("one")]
			One = 1,

			[DataName("two")]
			Two = 2,

			[DataName("three")]
			Three = 3
		}

		#endregion Test Types

		#region Array Tests

		[Fact]
		public void GetTokens_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new []
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd
			};

			var expected = new object[0];

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenArrayEnd
			};

			var expected = new object[] { null };

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayMultiItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenArrayEnd
			};

			var expected = new object[]
			{
				0,
				null,
				false,
				true
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayNestedDeeply_ReturnsExpectedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Not too deep"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd
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

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse((input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayUnclosed_ThrowsArgumentException()
		{
			// input from fail2.json in test suite at http://www.json.org/JSON_checker/
			var input = new []
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Unclosed array")
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenNone, ex.Token);
		}

		[Fact]
		public void GetTokens_ArrayExtraComma_ThrowsArgumentException()
		{
			// input from fail4.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("extra comma"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenArrayEnd, ex.Token);
		}

		[Fact]
		public void GetTokens_ArrayDoubleExtraComma_ThrowsArgumentException()
		{
			// input from fail5.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("double extra comma"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenValueDelim, ex.Token);
		}

		[Fact]
		public void GetTokens_ArrayMissingValue_ThrowsArgumentException()
		{
			// input from fail6.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("<-- missing value"),
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenValueDelim, ex.Token);
		}

		[Fact(Skip="JsonParser doesn't currently check after stream")]
		public void GetTokens_ArrayCommaAfterClose_ThrowsArgumentException()
		{
			// input from fail7.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Comma after the close"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenValueDelim, ex.Token);
		}

		[Fact(Skip="JsonParser doesn't currently check after stream")]
		public void GetTokens_ArrayExtraClose_ThrowsArgumentException()
		{
			// input from fail8.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Extra close"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenArrayEnd, ex.Token);
		}

		[Fact(Skip="JsonParser doesn't currently check depth")]
		public void GetTokens_ArrayNestedTooDeeply_ThrowsArgumentException()
		{
			// input from fail18.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Too deep"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenArrayBegin, ex.Token);
		}

		[Fact]
		public void GetTokens_ArrayColonInsteadOfComma_ThrowsArgumentException()
		{
			// input from fail22.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Colon instead of comma"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenPairDelim, ex.Token);
		}

		[Fact]
		public void GetTokens_ArrayBadValue_ThrowsArgumentException()
		{
			// input from fail23.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Bad value"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenLiteral("truth"),
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenLiteral("truth"), ex.Token);
		}

		[Fact]
		public void GetTokens_ArrayCloseMismatch_ThrowsArgumentException()
		{
			// input from fail33.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("mismatch"),
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenObjectEnd, ex.Token);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		public void GetTokens_ObjectEmpty_ReturnsEmptyObject()
		{
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>();

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectOneProperty_ReturnsSimpleObject()
		{
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("key"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("value"),
				JsonGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>
				{
					{ "key", "value" }
				};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectNested_ReturnsNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("JSON Test Pattern pass3"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("The outermost value"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("must be an object or array."),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("In this test"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("It is an object."),
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenObjectEnd
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

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectExtraComma_ThrowsArgumentException()
		{
			// input from fail9.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Extra comma"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenObjectEnd, ex.Token);
		}

		[Fact(Skip="JsonParser doesn't currently check after stream")]
		public void GetTokens_ObjectExtraValueAfterClose_ThrowsArgumentException()
		{
			// input from fail10.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Extra value after close"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenString("misplaced quoted value")
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenString("misplaced quoted value"), ex.Token);
		}

		[Fact]
		public void GetTokens_ObjectMissingColon_ThrowsArgumentException()
		{
			// input from fail19.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Missing colon"),
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenNull, ex.Token);
		}

		[Fact]
		public void GetTokens_ObjectDoubleColon_ThrowsArgumentException()
		{
			// input from fail20.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Double colon"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenPairDelim, ex.Token);
		}

		[Fact]
		public void GetTokens_ObjectCommaInsteadOfColon_ThrowsArgumentException()
		{
			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Comma instead of colon"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenValueDelim, ex.Token);
		}

		[Fact]
		public void GetTokens_ObjectCommaInsteadOfClose_ThrowsArgumentException()
		{
			// input from fail32.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Comma instead if closing brace"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			ParseException<JsonTokenType> ex = Assert.Throws<ParseException<JsonTokenType>>(
				delegate
				{
					var actual = parser.Parse(input);
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenNone, ex.Token);
		}

		#endregion Object Tests

		#region Enum Tests

		// TODO: these are actually testing type coercion, need to isolate type coercion to improve testability

		[Fact]
		public void GetTokens_EnumFromString_ReturnsEnum()
		{
			var input = new[]
			{
				JsonGrammar.TokenString("Two")
			};

			var expected = ExampleEnum.Two;

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse<ExampleEnum>(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EnumFromJsonName_ReturnsEnum()
		{
			var input = new[]
			{
				JsonGrammar.TokenString("two")
			};

			var expected = ExampleEnum.Two;

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse<ExampleEnum>(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EnumFromNumber_ReturnsEnum()
		{
			var input = new[]
			{
				JsonGrammar.TokenNumber(3)
			};

			var expected = ExampleEnum.Three;

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse<ExampleEnum>(input);

			Assert.Equal(expected, actual);
		}

		#endregion Enum Tests
	}
}
