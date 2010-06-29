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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("key"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("value"),
				JsonGrammar.TokenObjectEnd
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("key"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("value"),
				JsonGrammar.TokenObjectEnd
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
				JsonGrammar.TokenString(String.Empty)
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
				JsonGrammar.TokenString("A JSON payload should be an object or array, not a string.")
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
				JsonGrammar.TokenString("\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\"")
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
				JsonGrammar.TokenString("unescaped ' single quote"),
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("\ttab\tcharacter\tin\tstring\t"),
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenNumber(123456)
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
				JsonGrammar.TokenNumber(1.23456)
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
				JsonGrammar.TokenNumber(-0.123456)
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
				JsonGrammar.TokenNumber(.123456)
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
				JsonGrammar.TokenNumber(.123456)
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
				JsonGrammar.TokenNumber(-.123456)
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
				JsonGrammar.TokenNumber(-123.456m)
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
				JsonGrammar.TokenNumber(-123.456f)
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
				JsonGrammar.TokenNumber(-34L)
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
				JsonGrammar.TokenNumber(Int64.MinValue)
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
				JsonGrammar.TokenNumber(Decimal.MaxValue)
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("JSON Test Pattern pass1"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("object with 1 member"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("array with 1 element"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(-42),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("integer"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1234567890),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("real"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(-9876.543210),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("e"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(0.123456789e-12),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("E"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1.234567890E+34),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString(""),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(23456789012E66),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("zero"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("one"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("space"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString(" "),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("quote"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\""),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("backslash"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\\"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("controls"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\b\f\n\r\t"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("slash"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("/ & /"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("alpha"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("abcdefghijklmnopqrstuvwyz"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("ALPHA"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("digit"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("0123456789"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("0123456789"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("digit"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("special"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("hex"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("true"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("false"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("null"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("array"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("object"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("address"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("50 St. James Street"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("url"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("http://www.JSON.org/"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("comment"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("// /* <!-- --"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("# -- --> */"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString(" "),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString(" s p a c e d "),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(3),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(4),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(7),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("compact"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(3),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(4),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(7),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("jsontext"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("{\"object with 1 member\":[\"array with 1 element\"]}"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("quotes"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("&#34; \u0022 %22 0x22 034 &#x22;"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("A key can be any string"),
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(0.5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(98.6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(99.44),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1066),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(10.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(0.1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("rosebud"),
				JsonGrammar.TokenArrayEnd
			};

			const string expected = @"[""JSON Test Pattern pass1"",{""object with 1 member"":[""array with 1 element""]},{},[],-42,true,false,null,{""integer"":1234567890,""real"":-9876.54321,""e"":1.23456789E-13,""E"":1.23456789E+34,"""":2.3456789012E+76,""zero"":0,""one"":1,""space"":"" "",""quote"":""\"""",""backslash"":""\\"",""controls"":""\b\f\n\r\t"",""slash"":""/ & /"",""alpha"":""abcdefghijklmnopqrstuvwyz"",""ALPHA"":""ABCDEFGHIJKLMNOPQRSTUVWYZ"",""digit"":""0123456789"",""0123456789"":""digit"",""special"":""`1~!@#$%^&*()_+-={':[,]}|;.\u003C/>?"",""hex"":""\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A"",""true"":true,""false"":false,""null"":null,""array"":[],""object"":{},""address"":""50 St. James Street"",""url"":""http://www.JSON.org/"",""comment"":""// /* \u003C!-- --"",""# -- --> */"":"" "","" s p a c e d "":[1,2,3,4,5,6,7],""compact"":[1,2,3,4,5,6,7],""jsontext"":""{\""object with 1 member\"":[\""array with 1 element\""]}"",""quotes"":""&#34; \"" %22 0x22 034 &#x22;"",""/\\\""\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./\u003C>?"":""A key can be any string""},0.5,98.6,99.44,1066,10,1,0.1,1,2,2,""rosebud""]";

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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("JSON Test Pattern pass1"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("object with 1 member"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("array with 1 element"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(-42),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("integer"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1234567890),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("real"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(-9876.543210),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("e"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(0.123456789e-12),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("E"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1.234567890E+34),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString(""),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(23456789012E66),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("zero"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("one"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("space"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString(" "),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("quote"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\""),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("backslash"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\\"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("controls"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\b\f\n\r\t"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("slash"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("/ & /"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("alpha"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("abcdefghijklmnopqrstuvwyz"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("ALPHA"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("digit"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("0123456789"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("0123456789"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("digit"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("special"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("hex"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("true"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("false"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("null"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("array"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("object"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("address"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("50 St. James Street"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("url"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("http://www.JSON.org/"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("comment"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("// /* <!-- --"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("# -- --> */"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString(" "),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString(" s p a c e d "),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(3),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(4),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(7),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("compact"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(3),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(4),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(7),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("jsontext"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("{\"object with 1 member\":[\"array with 1 element\"]}"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("quotes"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("&#34; \u0022 %22 0x22 034 &#x22;"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("A key can be any string"),
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(0.5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(98.6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(99.44),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1066),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(10.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(0.1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("rosebud"),
				JsonGrammar.TokenArrayEnd
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
		""e"" : 1.23456789E-13,
		""E"" : 1.23456789E+34,
		"""" : 2.3456789012E+76,
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
			var input = Enumerable.Empty<Token<JsonTokenType>>();

			const string expected = "";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<JsonTokenType>>)null;

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
		public void Format_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var formatter = new JsonWriter.JsonFormatter(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		[Fact]
		public void Format_MissingArrayOpen_ThrowsSerializationException()
		{
			var input = new[]
			{
				JsonGrammar.TokenArrayEnd
			};

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());

			SerializationException ex = Assert.Throws<SerializationException>(
				delegate
				{
					var actual = formatter.Format(input);
				});

			//Assert.Equal(, ex);
		}

		[Fact]
		public void Format_ExtraArrayClose_ThrowsSerializationException()
		{
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd
			};

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());

			SerializationException ex = Assert.Throws<SerializationException>(
				delegate
				{
					var actual = formatter.Format(input);
				});

			//Assert.Equal(, ex);
		}

		[Fact]
		public void Format_MissingObjectOpen_ThrowsSerializationException()
		{
			var input = new[]
			{
				JsonGrammar.TokenObjectEnd
			};

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());

			SerializationException ex = Assert.Throws<SerializationException>(
				delegate
				{
					var actual = formatter.Format(input);
				});

			//Assert.Equal(, ex);
		}

		[Fact]
		public void Format_ExtraObjectClose_ThrowsSerializationException()
		{
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenObjectEnd
			};

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());

			SerializationException ex = Assert.Throws<SerializationException>(
				delegate
				{
					var actual = formatter.Format(input);
				});

			//Assert.Equal(, ex);
		}

		[Fact]
		public void Format_MaxDepthExceeded_ThrowsSerializationException()
		{
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenObjectEnd
			};

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { MaxDepth = 5 });

			SerializationException ex = Assert.Throws<SerializationException>(
				delegate
				{
					var actual = formatter.Format(input);
				});

			//Assert.Equal(, ex);
		}

		#endregion Input Edge Case Tests
	}
}
