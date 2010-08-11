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
		private const string TraitValue = "Formatter";

		#endregion Constants

		#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginUnnamed,
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
				CommonGrammar.TokenArrayBeginUnnamed,
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
				CommonGrammar.TokenArrayBeginUnnamed,
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
				CommonGrammar.TokenArrayBeginUnnamed,
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive(0),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenFalse,
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive(0),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenFalse,
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("Not too deep"),
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("Not too deep"),
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
				CommonGrammar.TokenObjectBeginUnnamed,
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
				CommonGrammar.TokenObjectBeginUnnamed,
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
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenPrimitive("value"),
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
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenPrimitive("value"),
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
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("JSON Test Pattern pass3"),
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("The outermost value"),
				CommonGrammar.TokenPrimitive("must be an object or array."),
				CommonGrammar.TokenProperty("In this test"),
				CommonGrammar.TokenPrimitive("It is an object."),
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
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("JSON Test Pattern pass3"),
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("The outermost value"),
				CommonGrammar.TokenPrimitive("must be an object or array."),
				CommonGrammar.TokenProperty("In this test"),
				CommonGrammar.TokenPrimitive("It is an object."),
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
				CommonGrammar.TokenPrimitive(String.Empty)
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
				CommonGrammar.TokenPrimitive("A JSON payload should be an object or array, not a string.")
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
				CommonGrammar.TokenPrimitive("\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\"")
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
				CommonGrammar.TokenPrimitive("unescaped ' single quote"),
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("\ttab\tcharacter\tin\tstring\t"),
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
				CommonGrammar.TokenPrimitive(123456)
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
				CommonGrammar.TokenPrimitive(1.23456)
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
				CommonGrammar.TokenPrimitive(-0.123456)
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
				CommonGrammar.TokenPrimitive(.123456)
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
				CommonGrammar.TokenPrimitive(.123456)
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
				CommonGrammar.TokenPrimitive(-.123456)
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
				CommonGrammar.TokenPrimitive(-123.456m)
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
				CommonGrammar.TokenPrimitive(-123.456f)
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
				CommonGrammar.TokenPrimitive(-34L)
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
				CommonGrammar.TokenPrimitive(Int64.MinValue)
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
				CommonGrammar.TokenPrimitive(Decimal.MaxValue)
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("JSON Test Pattern pass1"),
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("object with 1 member"),
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("array with 1 element"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenPrimitive(-42),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectBeginUnnamed,
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("object"),
				CommonGrammar.TokenObjectBeginUnnamed,
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive(1),
				CommonGrammar.TokenPrimitive(2),
				CommonGrammar.TokenPrimitive(3),
				CommonGrammar.TokenPrimitive(4),
				CommonGrammar.TokenPrimitive(5),
				CommonGrammar.TokenPrimitive(6),
				CommonGrammar.TokenPrimitive(7),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("compact"),
				CommonGrammar.TokenArrayBeginUnnamed,
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

			const string expected = @"[""JSON Test Pattern pass1"",{""object with 1 member"":[""array with 1 element""]},{},[],-42,true,false,null,{""integer"":1234567890,""real"":-9876.54321,""e"":1.23456789E-13,""E"":1.23456789E+34,"""":2.3456789012E+76,""zero"":0,""one"":1,""space"":"" "",""quote"":""\"""",""backslash"":""\\"",""controls"":""\b\f\n\r\t"",""slash"":""/ & /"",""alpha"":""abcdefghijklmnopqrstuvwyz"",""ALPHA"":""ABCDEFGHIJKLMNOPQRSTUVWYZ"",""digit"":""0123456789"",""0123456789"":""digit"",""special"":""`1~!@#$%^&*()_+-={':[,]}|;.</>?"",""hex"":""\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A"",""true"":true,""false"":false,""null"":null,""array"":[],""object"":{},""address"":""50 St. James Street"",""url"":""http://www.JSON.org/"",""comment"":""// /* <!-- --"",""# -- --> */"":"" "","" s p a c e d "":[1,2,3,4,5,6,7],""compact"":[1,2,3,4,5,6,7],""jsontext"":""{\""object with 1 member\"":[\""array with 1 element\""]}"",""quotes"":""&#34; \"" %22 0x22 034 &#x22;"",""/\\\""\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"":""A key can be any string""},0.5,98.6,99.44,1066,10,1,0.1,1,2,2,""rosebud""]";

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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("JSON Test Pattern pass1"),
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("object with 1 member"),
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("array with 1 element"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenPrimitive(-42),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectBeginUnnamed,
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("object"),
				CommonGrammar.TokenObjectBeginUnnamed,
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
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive(1),
				CommonGrammar.TokenPrimitive(2),
				CommonGrammar.TokenPrimitive(3),
				CommonGrammar.TokenPrimitive(4),
				CommonGrammar.TokenPrimitive(5),
				CommonGrammar.TokenPrimitive(6),
				CommonGrammar.TokenPrimitive(7),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenProperty("compact"),
				CommonGrammar.TokenArrayBeginUnnamed,
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
		""special"" : ""`1~!@#$%^&*()_+-={':[,]}|;.</>?"",
		""hex"" : ""\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A"",
		""true"" : true,
		""false"" : false,
		""null"" : null,
		""array"" : [],
		""object"" : {},
		""address"" : ""50 St. James Street"",
		""url"" : ""http://www.JSON.org/"",
		""comment"" : ""// /* <!-- --"",
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
		""/\\\""\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"" : ""A key can be any string""
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
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty(new DataName("key", null, "http://json.org")),
				CommonGrammar.TokenPrimitive("value"),
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
