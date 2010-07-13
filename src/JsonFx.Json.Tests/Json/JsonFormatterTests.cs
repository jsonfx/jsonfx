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
	public class JsonFormatterTests
	{
		#region Array Tests

		[Fact]
		public void Format_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayEnd
			};

			const string expected = "[]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayEmptyPrettyPrint_ReturnsPrettyPrintedEmptyArray()
		{
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayEnd
			};

			const string expected = "[]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenNull,
				DataGrammar.TokenArrayEnd
			};

			const string expected = "[null]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayOneItemPrettyPrint_ReturnsExpectedPrettyPrintedArray()
		{
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenNull,
				DataGrammar.TokenArrayEnd
			};

			const string expected =
@"[
	null
]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayMultiItem_ReturnsExpectedArray()
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

			const string expected = "[0,null,false,true]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayMultiItemPrettyPrint_ReturnsExpectedPrettyPrintedArray()
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

			const string expected =
@"[
	0,
	null,
	false,
	true
]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayNestedDeeply_ReturnsExpectedArray()
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

			const string expected = @"[[[[[[[[[[[[[[[[[[[""Not too deep""]]]]]]]]]]]]]]]]]]]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayNestedDeeplyPrettyPrint_ReturnsExpectedPrettyPrintedArray()
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

			const string expected =
@"[
	[
		[
			[
				[
					[
						[
							[
								[
									[
										[
											[
												[
													[
														[
															[
																[
																	[
																		[
																			""Not too deep""
																		]
																	]
																]
															]
														]
													]
												]
											]
										]
									]
								]
							]
						]
					]
				]
			]
		]
	]
]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		public void Format_ObjectEmpty_ReturnsEmptyObject()
		{
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenObjectEnd
			};

			const string expected = @"{}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectEmptyPrettyPrint_ReturnsPrettyPrintedEmptyObject()
		{
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenObjectEnd
			};

			const string expected = @"{}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectOneProperty_ReturnsSimpleObject()
		{
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("key"),
				DataGrammar.TokenValue("value"),
				DataGrammar.TokenObjectEnd
			};

			const string expected = @"{""key"":""value""}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectOnePropertyPrettyPrint_ReturnsPrettyPrintedSimpleObject()
		{
			var input = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("key"),
				DataGrammar.TokenValue("value"),
				DataGrammar.TokenObjectEnd
			};

			const string expected =
@"{
	""key"" : ""value""
}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectNested_ReturnsNestedObject()
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

			const string expected = @"{""JSON Test Pattern pass3"":{""The outermost value"":""must be an object or array."",""In this test"":""It is an object.""}}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectNestedPrettyPrint_ReturnsPrettyPrintedNestedObject()
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

			const string expected =
@"{
	""JSON Test Pattern pass3"" : {
		""The outermost value"" : ""must be an object or array."",
		""In this test"" : ""It is an object.""
	}
}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region String Tests

		[Fact]
		public void Format_StringTokenEmpty_ReturnsEmptyString()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(String.Empty)
			};

			const string expected = "\"\"";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_StringTokenSimple_ReturnsString()
		{
			// input from fail1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenValue("A JSON payload should be an object or array, not a string.")
			};

			const string expected = @"""A JSON payload should be an object or array, not a string.""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_StringTokenEscapedChars_ReturnsString()
		{
			var input = new[]
			{
				DataGrammar.TokenValue("\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\"")
			};

			const string expected = @"""\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A\""""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_StringTokenUnescapedSingleQuote_ReturnsString()
		{
			var input = new[]
			{
				DataGrammar.TokenValue("unescaped ' single quote"),
			};

			const string expected = @"""unescaped ' single quote""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_StringTokenTabChar_ReturnsString()
		{
			// input from fail25.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("\ttab\tcharacter\tin\tstring\t"),
				DataGrammar.TokenArrayEnd
			};

			const string expected = @"[""\ttab\tcharacter\tin\tstring\t""]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		#endregion String Tests

		#region Number Tests

		[Fact]
		public void Format_NumberTokenInteger_ReturnsNumber()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(123456)
			};

			const string expected = "123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenDouble_ReturnsNumber()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(1.23456)
			};

			const string expected = "1.23456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenNegDouble_ReturnsNumber()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(-0.123456)
			};

			const string expected = "-0.123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenNoLeadingDigitDouble_ReturnsNumber()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(.123456)
			};

			const string expected = "0.123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenPosNoLeadingDigitDouble_ReturnsNumber()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(.123456)
			};

			const string expected = "0.123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenNegNoLeadingDigitDouble_ReturnsNumber()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(-.123456)
			};

			const string expected = "-0.123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenNegDecimal_ReturnsNumber()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(-123.456m)
			};

			const string expected = "-123.456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenNegFloat_ReturnsNumber()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(-123.456f)
			};

			const string expected = "-123.456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenNegLong_ReturnsNumber()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(-34L)
			};

			const string expected = "-34";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenOverflowLong_ReturnsString()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(Int64.MinValue)
			};

			const string expected = @"""-9223372036854775808""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NumberTokenOverflowDecimal_ReturnsString()
		{
			var input = new[]
			{
				DataGrammar.TokenValue(Decimal.MaxValue)
			};

			const string expected = @"""79228162514264337593543950335""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Number Tests

		#region Complex Graph Tests

		[Fact]
		public void Format_GraphComplex_ReturnsGraph()
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

			const string expected = @"[""JSON Test Pattern pass1"",{""object with 1 member"":[""array with 1 element""]},{},[],-42,true,false,null,{""integer"":1234567890,""real"":-9876.54321,""e"":1.23456789e-13,""E"":1.23456789e+34,"""":2.3456789012e+76,""zero"":0,""one"":1,""space"":"" "",""quote"":""\"""",""backslash"":""\\"",""controls"":""\b\f\n\r\t"",""slash"":""/ & /"",""alpha"":""abcdefghijklmnopqrstuvwyz"",""ALPHA"":""ABCDEFGHIJKLMNOPQRSTUVWYZ"",""digit"":""0123456789"",""0123456789"":""digit"",""special"":""`1~!@#$%^&*()_+-={':[,]}|;.\u003C/>?"",""hex"":""\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A"",""true"":true,""false"":false,""null"":null,""array"":[],""object"":{},""address"":""50 St. James Street"",""url"":""http://www.JSON.org/"",""comment"":""// /* \u003C!-- --"",""# -- --> */"":"" "","" s p a c e d "":[1,2,3,4,5,6,7],""compact"":[1,2,3,4,5,6,7],""jsontext"":""{\""object with 1 member\"":[\""array with 1 element\""]}"",""quotes"":""&#34; \"" %22 0x22 034 &#x22;"",""/\\\""\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./\u003C>?"":""A key can be any string""},0.5,98.6,99.44,1066,10,1,0.1,1,2,2,""rosebud""]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_GraphComplexPrettyPrint_ReturnsPrettyPrintedGraph()
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

			const string expected =
@"[
	""JSON Test Pattern pass1"",
	{
		""object with 1 member"" : [
			""array with 1 element""
		]
	},
	{},
	[],
	-42,
	true,
	false,
	null,
	{
		""integer"" : 1234567890,
		""real"" : -9876.54321,
		""e"" : 1.23456789e-13,
		""E"" : 1.23456789e+34,
		"""" : 2.3456789012e+76,
		""zero"" : 0,
		""one"" : 1,
		""space"" : "" "",
		""quote"" : ""\"""",
		""backslash"" : ""\\"",
		""controls"" : ""\b\f\n\r\t"",
		""slash"" : ""/ & /"",
		""alpha"" : ""abcdefghijklmnopqrstuvwyz"",
		""ALPHA"" : ""ABCDEFGHIJKLMNOPQRSTUVWYZ"",
		""digit"" : ""0123456789"",
		""0123456789"" : ""digit"",
		""special"" : ""`1~!@#$%^&*()_+-={':[,]}|;.\u003C/>?"",
		""hex"" : ""\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A"",
		""true"" : true,
		""false"" : false,
		""null"" : null,
		""array"" : [],
		""object"" : {},
		""address"" : ""50 St. James Street"",
		""url"" : ""http://www.JSON.org/"",
		""comment"" : ""// /* \u003C!-- --"",
		""# -- --> */"" : "" "",
		"" s p a c e d "" : [
			1,
			2,
			3,
			4,
			5,
			6,
			7
		],
		""compact"" : [
			1,
			2,
			3,
			4,
			5,
			6,
			7
		],
		""jsontext"" : ""{\""object with 1 member\"":[\""array with 1 element\""]}"",
		""quotes"" : ""&#34; \"" %22 0x22 034 &#x22;"",
		""/\\\""\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./\u003C>?"" : ""A key can be any string""
	},
	0.5,
	98.6,
	99.44,
	1066,
	10,
	1,
	0.1,
	1,
	2,
	2,
	""rosebud""
]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Complex Graph Tests

		#region Input Edge Case Tests

		[Fact]
		public void Format_EmptyInput_ReturnsEmptyString()
		{
			var input = Enumerable.Empty<Token<DataTokenType>>();

			const string expected = "";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<DataTokenType>>)null;

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());

			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var actual = formatter.Format(input);
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
					var formatter = new JsonWriter.JsonFormatter(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
