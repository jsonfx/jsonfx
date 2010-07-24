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
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json
{
	public class JsonFormatterTests
	{
		#region Constants

		private const string TraitName = "JSON";
		private const string TraitValue = "Serialization";

		#endregion Constants

		#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			const string expected = "[]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayEmptyPrettyPrint_ReturnsPrettyPrintedEmptyArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			const string expected = "[]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd
			};

			const string expected = "[null]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayOneItemPrettyPrint_ReturnsExpectedPrettyPrintedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd
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
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayMultiItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue(0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenArrayEnd
			};

			const string expected = "[0,null,false,true]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayMultiItemPrettyPrint_ReturnsExpectedPrettyPrintedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue(0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenArrayEnd
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
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayNestedDeeply_ReturnsExpectedArray()
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

			const string expected = @"[[[[[[[[[[[[[[[[[[[""Not too deep""]]]]]]]]]]]]]]]]]]]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayNestedDeeplyPrettyPrint_ReturnsExpectedPrettyPrintedArray()
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
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectEmpty_ReturnsEmptyObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"{}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectEmptyPrettyPrint_ReturnsPrettyPrintedEmptyObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"{}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectOneProperty_ReturnsSimpleObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"{""key"":""value""}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectOnePropertyPrettyPrint_ReturnsPrettyPrintedSimpleObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
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
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectNested_ReturnsNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("JSON Test Pattern pass3"),
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("The outermost value"),
				CommonGrammar.TokenValue("must be an object or array."),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("In this test"),
				CommonGrammar.TokenValue("It is an object."),
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"{""JSON Test Pattern pass3"":{""The outermost value"":""must be an object or array."",""In this test"":""It is an object.""}}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectNestedPrettyPrint_ReturnsPrettyPrintedNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("JSON Test Pattern pass3"),
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("The outermost value"),
				CommonGrammar.TokenValue("must be an object or array."),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("In this test"),
				CommonGrammar.TokenValue("It is an object."),
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectEnd
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
		[Trait(TraitName, TraitValue)]
		public void Format_StringTokenEmpty_ReturnsEmptyString()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(String.Empty)
			};

			const string expected = "\"\"";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_StringTokenSimple_ReturnsString()
		{
			// input from fail1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenValue("A JSON payload should be an object or array, not a string.")
			};

			const string expected = @"""A JSON payload should be an object or array, not a string.""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_StringTokenEscapedChars_ReturnsString()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\"")
			};

			const string expected = @"""\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A\""""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_StringTokenUnescapedSingleQuote_ReturnsString()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("unescaped ' single quote"),
			};

			const string expected = @"""unescaped ' single quote""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_StringTokenTabChar_ReturnsString()
		{
			// input from fail25.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("\ttab\tcharacter\tin\tstring\t"),
				CommonGrammar.TokenArrayEnd
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
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenInteger_ReturnsNumber()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(123456)
			};

			const string expected = "123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenDouble_ReturnsNumber()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(1.23456)
			};

			const string expected = "1.23456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenNegDouble_ReturnsNumber()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(-0.123456)
			};

			const string expected = "-0.123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenNoLeadingDigitDouble_ReturnsNumber()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(.123456)
			};

			const string expected = "0.123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenPosNoLeadingDigitDouble_ReturnsNumber()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(.123456)
			};

			const string expected = "0.123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenNegNoLeadingDigitDouble_ReturnsNumber()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(-.123456)
			};

			const string expected = "-0.123456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenNegDecimal_ReturnsNumber()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(-123.456m)
			};

			const string expected = "-123.456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenNegFloat_ReturnsNumber()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(-123.456f)
			};

			const string expected = "-123.456";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenNegLong_ReturnsNumber()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(-34L)
			};

			const string expected = "-34";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenOverflowLong_ReturnsString()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(Int64.MinValue)
			};

			const string expected = @"""-9223372036854775808""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NumberTokenOverflowDecimal_ReturnsString()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(Decimal.MaxValue)
			};

			const string expected = @"""79228162514264337593543950335""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Number Tests

		#region Complex Graph Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_GraphComplex_ReturnsGraph()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("JSON Test Pattern pass1"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("object with 1 member"),
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("array with 1 element"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenArrayBeginNoName,
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
				CommonGrammar.TokenObjectBeginNoName,
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
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("object"),
				CommonGrammar.TokenObjectBeginNoName,
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
				CommonGrammar.TokenArrayBeginNoName,
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
				CommonGrammar.TokenArrayBeginNoName,
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

			const string expected = @"[""JSON Test Pattern pass1"",{""object with 1 member"":[""array with 1 element""]},{},[],-42,true,false,null,{""integer"":1234567890,""real"":-9876.54321,""e"":1.23456789e-13,""E"":1.23456789e+34,"""":2.3456789012e+76,""zero"":0,""one"":1,""space"":"" "",""quote"":""\"""",""backslash"":""\\"",""controls"":""\b\f\n\r\t"",""slash"":""/ & /"",""alpha"":""abcdefghijklmnopqrstuvwyz"",""ALPHA"":""ABCDEFGHIJKLMNOPQRSTUVWYZ"",""digit"":""0123456789"",""0123456789"":""digit"",""special"":""`1~!@#$%^&*()_+-={':[,]}|;.\u003C/>?"",""hex"":""\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A"",""true"":true,""false"":false,""null"":null,""array"":[],""object"":{},""address"":""50 St. James Street"",""url"":""http://www.JSON.org/"",""comment"":""// /* \u003C!-- --"",""# -- --> */"":"" "","" s p a c e d "":[1,2,3,4,5,6,7],""compact"":[1,2,3,4,5,6,7],""jsontext"":""{\""object with 1 member\"":[\""array with 1 element\""]}"",""quotes"":""&#34; \"" %22 0x22 034 &#x22;"",""/\\\""\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./\u003C>?"":""A key can be any string""},0.5,98.6,99.44,1066,10,1,0.1,1,2,2,""rosebud""]";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_GraphComplexPrettyPrint_ReturnsPrettyPrintedGraph()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("JSON Test Pattern pass1"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("object with 1 member"),
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("array with 1 element"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenArrayBeginNoName,
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
				CommonGrammar.TokenObjectBeginNoName,
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
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("object"),
				CommonGrammar.TokenObjectBeginNoName,
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
				CommonGrammar.TokenArrayBeginNoName,
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
				CommonGrammar.TokenArrayBeginNoName,
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

		#region Namespace Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ObjectOneNamespacedProperty_CorrectlyIgnoresNamespace()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty(new DataName("key", null, "http://json.org")),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"{""key"":""value""}";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_EmptyInput_ReturnsEmptyString()
		{
			var input = Enumerable.Empty<Token<CommonTokenType>>();

			const string expected = "";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<CommonTokenType>>)null;

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
		[Trait(TraitName, TraitValue)]
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
