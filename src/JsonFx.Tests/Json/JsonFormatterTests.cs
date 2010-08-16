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

using JsonFx.Model;
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(0),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(0),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("Not too deep"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("Not too deep"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("key"),
				ModelGrammar.TokenPrimitive("value"),
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("key"),
				ModelGrammar.TokenPrimitive("value"),
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("JSON Test Pattern pass3"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("The outermost value"),
				ModelGrammar.TokenPrimitive("must be an object or array."),
				ModelGrammar.TokenProperty("In this test"),
				ModelGrammar.TokenPrimitive("It is an object."),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("JSON Test Pattern pass3"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("The outermost value"),
				ModelGrammar.TokenPrimitive("must be an object or array."),
				ModelGrammar.TokenProperty("In this test"),
				ModelGrammar.TokenPrimitive("It is an object."),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenPrimitive(String.Empty)
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
				ModelGrammar.TokenPrimitive("A JSON payload should be an object or array, not a string.")
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
				ModelGrammar.TokenPrimitive("\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\"")
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
				ModelGrammar.TokenPrimitive("unescaped ' single quote"),
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("\ttab\tcharacter\tin\tstring\t"),
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenPrimitive(123456)
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
				ModelGrammar.TokenPrimitive(1.23456)
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
				ModelGrammar.TokenPrimitive(-0.123456)
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
				ModelGrammar.TokenPrimitive(.123456)
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
				ModelGrammar.TokenPrimitive(.123456)
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
				ModelGrammar.TokenPrimitive(-.123456)
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
				ModelGrammar.TokenPrimitive(-123.456m)
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
				ModelGrammar.TokenPrimitive(-123.456f)
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
				ModelGrammar.TokenPrimitive(-34L)
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
				ModelGrammar.TokenPrimitive(Int64.MinValue)
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
				ModelGrammar.TokenPrimitive(Decimal.MaxValue)
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("JSON Test Pattern pass1"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("object with 1 member"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("array with 1 element"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenPrimitive(-42),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("integer"),
				ModelGrammar.TokenPrimitive(1234567890),
				ModelGrammar.TokenProperty("real"),
				ModelGrammar.TokenPrimitive(-9876.543210),
				ModelGrammar.TokenProperty("e"),
				ModelGrammar.TokenPrimitive(0.123456789e-12),
				ModelGrammar.TokenProperty("E"),
				ModelGrammar.TokenPrimitive(1.234567890E+34),
				ModelGrammar.TokenProperty(""),
				ModelGrammar.TokenPrimitive(23456789012E66),
				ModelGrammar.TokenProperty("zero"),
				ModelGrammar.TokenPrimitive(0),
				ModelGrammar.TokenProperty("one"),
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenProperty("space"),
				ModelGrammar.TokenPrimitive(" "),
				ModelGrammar.TokenProperty("quote"),
				ModelGrammar.TokenPrimitive("\""),
				ModelGrammar.TokenProperty("backslash"),
				ModelGrammar.TokenPrimitive("\\"),
				ModelGrammar.TokenProperty("controls"),
				ModelGrammar.TokenPrimitive("\b\f\n\r\t"),
				ModelGrammar.TokenProperty("slash"),
				ModelGrammar.TokenPrimitive("/ & /"),
				ModelGrammar.TokenProperty("alpha"),
				ModelGrammar.TokenPrimitive("abcdefghijklmnopqrstuvwyz"),
				ModelGrammar.TokenProperty("ALPHA"),
				ModelGrammar.TokenPrimitive("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				ModelGrammar.TokenProperty("digit"),
				ModelGrammar.TokenPrimitive("0123456789"),
				ModelGrammar.TokenProperty("0123456789"),
				ModelGrammar.TokenPrimitive("digit"),
				ModelGrammar.TokenProperty("special"),
				ModelGrammar.TokenPrimitive("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				ModelGrammar.TokenProperty("hex"),
				ModelGrammar.TokenPrimitive("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				ModelGrammar.TokenProperty("true"),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenProperty("false"),
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenProperty("null"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenProperty("array"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("object"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenProperty("address"),
				ModelGrammar.TokenPrimitive("50 St. James Street"),
				ModelGrammar.TokenProperty("url"),
				ModelGrammar.TokenPrimitive("http://www.JSON.org/"),
				ModelGrammar.TokenProperty("comment"),
				ModelGrammar.TokenPrimitive("// /* <!-- --"),
				ModelGrammar.TokenProperty("# -- --> */"),
				ModelGrammar.TokenPrimitive(" "),
				ModelGrammar.TokenProperty(" s p a c e d "),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenPrimitive(2),
				ModelGrammar.TokenPrimitive(3),
				ModelGrammar.TokenPrimitive(4),
				ModelGrammar.TokenPrimitive(5),
				ModelGrammar.TokenPrimitive(6),
				ModelGrammar.TokenPrimitive(7),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("compact"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenPrimitive(2),
				ModelGrammar.TokenPrimitive(3),
				ModelGrammar.TokenPrimitive(4),
				ModelGrammar.TokenPrimitive(5),
				ModelGrammar.TokenPrimitive(6),
				ModelGrammar.TokenPrimitive(7),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("jsontext"),
				ModelGrammar.TokenPrimitive("{\"object with 1 member\":[\"array with 1 element\"]}"),
				ModelGrammar.TokenProperty("quotes"),
				ModelGrammar.TokenPrimitive("&#34; \u0022 %22 0x22 034 &#x22;"),
				ModelGrammar.TokenProperty("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				ModelGrammar.TokenPrimitive("A key can be any string"),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenPrimitive(0.5),
				ModelGrammar.TokenPrimitive(98.6),
				ModelGrammar.TokenPrimitive(99.44),
				ModelGrammar.TokenPrimitive(1066),
				ModelGrammar.TokenPrimitive(10.0),
				ModelGrammar.TokenPrimitive(1.0),
				ModelGrammar.TokenPrimitive(0.1),
				ModelGrammar.TokenPrimitive(1.0),
				ModelGrammar.TokenPrimitive(2.0),
				ModelGrammar.TokenPrimitive(2.0),
				ModelGrammar.TokenPrimitive("rosebud"),
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("JSON Test Pattern pass1"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("object with 1 member"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("array with 1 element"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenPrimitive(-42),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("integer"),
				ModelGrammar.TokenPrimitive(1234567890),
				ModelGrammar.TokenProperty("real"),
				ModelGrammar.TokenPrimitive(-9876.543210),
				ModelGrammar.TokenProperty("e"),
				ModelGrammar.TokenPrimitive(0.123456789e-12),
				ModelGrammar.TokenProperty("E"),
				ModelGrammar.TokenPrimitive(1.234567890E+34),
				ModelGrammar.TokenProperty(""),
				ModelGrammar.TokenPrimitive(23456789012E66),
				ModelGrammar.TokenProperty("zero"),
				ModelGrammar.TokenPrimitive(0),
				ModelGrammar.TokenProperty("one"),
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenProperty("space"),
				ModelGrammar.TokenPrimitive(" "),
				ModelGrammar.TokenProperty("quote"),
				ModelGrammar.TokenPrimitive("\""),
				ModelGrammar.TokenProperty("backslash"),
				ModelGrammar.TokenPrimitive("\\"),
				ModelGrammar.TokenProperty("controls"),
				ModelGrammar.TokenPrimitive("\b\f\n\r\t"),
				ModelGrammar.TokenProperty("slash"),
				ModelGrammar.TokenPrimitive("/ & /"),
				ModelGrammar.TokenProperty("alpha"),
				ModelGrammar.TokenPrimitive("abcdefghijklmnopqrstuvwyz"),
				ModelGrammar.TokenProperty("ALPHA"),
				ModelGrammar.TokenPrimitive("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				ModelGrammar.TokenProperty("digit"),
				ModelGrammar.TokenPrimitive("0123456789"),
				ModelGrammar.TokenProperty("0123456789"),
				ModelGrammar.TokenPrimitive("digit"),
				ModelGrammar.TokenProperty("special"),
				ModelGrammar.TokenPrimitive("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				ModelGrammar.TokenProperty("hex"),
				ModelGrammar.TokenPrimitive("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				ModelGrammar.TokenProperty("true"),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenProperty("false"),
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenProperty("null"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenProperty("array"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("object"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenProperty("address"),
				ModelGrammar.TokenPrimitive("50 St. James Street"),
				ModelGrammar.TokenProperty("url"),
				ModelGrammar.TokenPrimitive("http://www.JSON.org/"),
				ModelGrammar.TokenProperty("comment"),
				ModelGrammar.TokenPrimitive("// /* <!-- --"),
				ModelGrammar.TokenProperty("# -- --> */"),
				ModelGrammar.TokenPrimitive(" "),
				ModelGrammar.TokenProperty(" s p a c e d "),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenPrimitive(2),
				ModelGrammar.TokenPrimitive(3),
				ModelGrammar.TokenPrimitive(4),
				ModelGrammar.TokenPrimitive(5),
				ModelGrammar.TokenPrimitive(6),
				ModelGrammar.TokenPrimitive(7),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("compact"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenPrimitive(2),
				ModelGrammar.TokenPrimitive(3),
				ModelGrammar.TokenPrimitive(4),
				ModelGrammar.TokenPrimitive(5),
				ModelGrammar.TokenPrimitive(6),
				ModelGrammar.TokenPrimitive(7),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("jsontext"),
				ModelGrammar.TokenPrimitive("{\"object with 1 member\":[\"array with 1 element\"]}"),
				ModelGrammar.TokenProperty("quotes"),
				ModelGrammar.TokenPrimitive("&#34; \u0022 %22 0x22 034 &#x22;"),
				ModelGrammar.TokenProperty("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				ModelGrammar.TokenPrimitive("A key can be any string"),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenPrimitive(0.5),
				ModelGrammar.TokenPrimitive(98.6),
				ModelGrammar.TokenPrimitive(99.44),
				ModelGrammar.TokenPrimitive(1066),
				ModelGrammar.TokenPrimitive(10.0),
				ModelGrammar.TokenPrimitive(1.0),
				ModelGrammar.TokenPrimitive(0.1),
				ModelGrammar.TokenPrimitive(1.0),
				ModelGrammar.TokenPrimitive(2.0),
				ModelGrammar.TokenPrimitive(2.0),
				ModelGrammar.TokenPrimitive("rosebud"),
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty(new DataName("key", null, "http://json.org")),
				ModelGrammar.TokenPrimitive("value"),
				ModelGrammar.TokenObjectEnd
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
			var input = Enumerable.Empty<Token<ModelTokenType>>();

			const string expected = "";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<ModelTokenType>>)null;

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
