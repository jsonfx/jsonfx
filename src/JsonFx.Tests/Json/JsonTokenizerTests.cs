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
using System.Linq;

using JsonFx.Common;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json
{
	public class JsonTokenizerTests
	{
		#region Constants

		private const string TraitName = "JSON";
		private const string TraitValue = "Tokenizer";

		#endregion Constants

		#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayEmpty_ReturnsEmptyArrayTokens()
		{
			const string input = "[]";
			var expected = new []
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayOneItem_ReturnsArrayTokens()
		{
			const string input = "[null]";
			var expected = new []
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayMultiItemWhitespace_ReturnsSimpleArrayTokens()
		{
			const string input = "[ 0, null,  false,true ]";
			var expected = new []
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive(0),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayNestedDeeply_ReturnsNestedArrayTokens()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[[[[[[[[[[[[[[[[[[[""Not too deep""]]]]]]]]]]]]]]]]]]]";
			var expected = new []
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayExtraComma_ThrowsDeserializationException()
		{
			// input from fail4.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""extra comma"",]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(15, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayDoubleExtraComma_ThrowsDeserializationException()
		{
			// input from fail5.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""double extra comma"",,]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(22, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayMissingValue_ThrowsDeserializationException()
		{
			// input from fail6.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[   , ""<-- missing value""]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(4, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayColonInsteadOfComma_ProducesInvalidSequence()
		{
			// NOTE: analyzer must flag this as an error as is grammar error, not tokenization error

			// input from fail22.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""Colon instead of comma"": false]";
			var expected = new[]
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenProperty("Colon instead of comma"),
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ArrayBadValue_ThrowsDeserializationException()
		{
			// input from fail23.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""Bad value"", truth]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
					Console.WriteLine(actual);
				});

			// verify exception is coming from expected index
			Assert.Equal(14, ex.Index);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ObjectEmpty_ReturnsEmptyObjectTokens()
		{
			const string input = "{}";
			var expected = new []
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ObjectOneProperty_ReturnsSimpleObjectTokens()
		{
			const string input = @"{""key"":""value""}";
			var expected = new []
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenPrimitive("value"),
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ObjectNested_ReturnsNestedObjectTokens()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{
    ""JSON Test Pattern pass3"": {
        ""The outermost value"": ""must be an object or array."",
        ""In this test"": ""It is an object.""
    }
}
";

			var expected = new []
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ObjectCommaInsteadOfColon_ProducesInvalidSequence()
		{
			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = @"{""Comma instead of colon"", null}";
			var expected = new[]
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenPrimitive("Comma instead of colon"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ObjectUnterminated_ProducesInvalidSequence()
		{
			// NOTE: analyzer must flag this as an error as is grammar error, not tokenization error

			// input from fail32.json in test suite at http://www.json.org/JSON_checker/
			var input = @"{""Comma instead of closing brace"": true,";

			var expected = new[]
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("Comma instead of closing brace"),
				CommonGrammar.TokenTrue
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region String Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringEmpty_ReturnsEmptyStringToken()
		{
			const string input = "\"\"";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(String.Empty)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringSimple_ReturnsStringToken()
		{
			// input from fail1.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"""A JSON payload should be an object or array, not a string.""";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive("A JSON payload should be an object or array, not a string.")
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringEscapedChars_ReturnsStringToken()
		{
			const string input = @"""\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\""""";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive("\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\"")
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringImproperlyEscapedChars_ReturnsStringTokenWithSimpleChars()
		{
			const string input = @"""\u\u1\u12\u123\u12345""";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive("uu1u12u123\u12345")
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringUnrecognizedEscapeLetter_EscapesToSimpleChar()
		{
			// input from fail15.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""Illegal backslash escape: \x15""]";
			var expected = new []
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("Illegal backslash escape: x15"),
				CommonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringUnrecognizedEscapeNull_CharIgnored()
		{
			// input from fail17.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""Illegal backslash escape: \017""]";
			var expected = new []
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("Illegal backslash escape: 17"),
				CommonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringSingleQuote_ReturnsStringToken()
		{
			// input from fail24.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"['single quote']";
			var expected = new []
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("single quote"),
				CommonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringUnescapedSingleQuote_ReturnsStringToken()
		{
			const string input = @"""unescaped ' single quote""";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive("unescaped ' single quote"),
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringUnescapedDoubleQuote_ReturnsStringToken()
		{
			const string input = @"'unescaped "" quote'";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive("unescaped \" quote"),
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringTabChar_ReturnsStringToken()
		{
			// input from fail25.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""	tab	character	in	string	""]";
			var expected = new []
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("\ttab\tcharacter\tin\tstring\t"),
				CommonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringEscapedTabChar_ReturnsStringToken()
		{
			// input from fail26.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""\	tab\	character\	in\	string\	""]";
			var expected = new []
			{
				CommonGrammar.TokenArrayBeginUnnamed,
				CommonGrammar.TokenPrimitive("\ttab\tcharacter\tin\tstring\t"),
				CommonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringUnquoted_ThrowsDeserializationException()
		{
			// input from fail16.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[\naked]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(1, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringLineBreak_ThrowsDeserializationException()
		{
			// input from fail27.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""line
break""]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(1, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringEscapedLineBreak_ThrowsDeserializationException()
		{
			// input from fail28.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""line\
break""]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(1, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_StringUnterminated_ThrowsDeserializationException()
		{
			const string input = @"""unterminated";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(0, ex.Index);
		}

		#endregion String Tests

		#region Number Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberInteger_ReturnsNumberToken()
		{
			const string input = "123456";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberFloat_ReturnsNumberToken()
		{
			const string input = "1.23456";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(1.23456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberNegFloat_ReturnsNumberToken()
		{
			const string input = "-0.123456";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(-0.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberNoLeadingDigitFloat_ReturnsNumberToken()
		{
			const string input = ".123456";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberPosNoLeadingDigitFloat_ReturnsNumberToken()
		{
			const string input = "+.123456";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberNegNoLeadingDigitFloat_ReturnsNumberToken()
		{
			const string input = "-.123456";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(-.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberIntegerLeadingZero_ReturnsObjectTokensWithNumberValue()
		{
			// input from fail13.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{""Numbers cannot have leading zeroes"": 013}";
			var expected = new []
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("Numbers cannot have leading zeroes"),
				CommonGrammar.TokenPrimitive(13),
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberHexValue_ThrowsDeserializationException()
		{
			// input from fail14.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{""Numbers cannot be hex"": 0x14}";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(26, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberFloatMissingExp_ThrowsDeserializationException()
		{
			// input from fail29.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[0e]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(1, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberFloatMissingExpDigits_ThrowsDeserializationException()
		{
			// input from fail30.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[0e+]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(1, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberFloatExtraExpSign_ThrowsDeserializationException()
		{
			// input from fail31.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[0e+-1]";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(1, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberUnfinishedFloat_ThrowsDeserializationException()
		{
			const string input = @"123.";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(0, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NumberFloatMissingFractional_ThrowsDeserializationException()
		{
			const string input = @"123.e5";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(0, ex.Index);
		}

		#endregion Number Tests

		#region Literal Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_LiteralNonQuotedKey_ReturnsObjectTokensWithLiteralKey()
		{
			// input from fail3.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{unquoted_key: ""keys must be quoted""}";
			var expected = new []
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("unquoted_key"),
				CommonGrammar.TokenPrimitive("keys must be quoted"),
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_LiteralNonQuotedKeyDollarSign_ReturnsObjectTokensWithLiteralKey()
		{
			const string input = @"{ $abcdefg0123456 : false }";
			var expected = new []
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("$abcdefg0123456"),
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_LiteralNonQuotedKeyNumber_ReturnsObjectTokensWithLiteralKey()
		{
			const string input = @"{ _123456 : true }";
			var expected = new []
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("_123456"),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Literal Tests

		#region Keyword Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_KeywordUndefined_ReturnsUndefinedToken()
		{
			const string input = @"undefined";
			var expected = new []
			{
				CommonGrammar.TokenNull
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_KeywordNull_ReturnsNullToken()
		{
			const string input = @"null";
			var expected = new []
			{
				CommonGrammar.TokenNull
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_KeywordFalse_ReturnsFalseToken()
		{
			const string input = @"false";
			var expected = new []
			{
				CommonGrammar.TokenFalse
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_KeywordTrue_ReturnsTrueToken()
		{
			const string input = @"true";
			var expected = new []
			{
				CommonGrammar.TokenTrue
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_KeywordNan_ReturnsNanToken()
		{
			const string input = @"NaN";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(Double.NaN)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_KeywordInfinity_ReturnsPositiveInfinityToken()
		{
			const string input = @"Infinity";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(Double.PositiveInfinity)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_KeywordPosInfinity_ReturnsPositiveInfinityToken()
		{
			const string input = @"+Infinity";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(Double.PositiveInfinity)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_KeywordNegInfinity_ReturnsNegativeInfinityToken()
		{
			const string input = @"-Infinity";
			var expected = new []
			{
				CommonGrammar.TokenPrimitive(Double.NegativeInfinity)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		#endregion Keyword Tests

		#region Complex Graph Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_GraphWhitespace_ReturnsGraphTokens()
		{
			const string input = @"
	{
""foo""

  :   	null   ,

		""bar""     :
""value""
  } 
 
";
			var expected = new []
			{
				CommonGrammar.TokenObjectBeginUnnamed,
				CommonGrammar.TokenProperty("foo"),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenProperty("bar"),
				CommonGrammar.TokenPrimitive("value"),
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_GraphComplex_ReturnsGraphTokens()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[
    ""JSON Test Pattern pass1"",
    {""object with 1 member"":[""array with 1 element""]},
    {},
    [],
    -42,
    true,
    false,
    null,
    {
        ""integer"": 1234567890,
        ""real"": -9876.543210,
        ""e"": 0.123456789e-12,
        ""E"": 1.234567890E+34,
        """":  23456789012E66,
        ""zero"": 0,
        ""one"": 1,
        ""space"": "" "",
        ""quote"": ""\"""",
        ""backslash"": ""\\"",
        ""controls"": ""\b\f\n\r\t"",
        ""slash"": ""/ & \/"",
        ""alpha"": ""abcdefghijklmnopqrstuvwyz"",
        ""ALPHA"": ""ABCDEFGHIJKLMNOPQRSTUVWYZ"",
        ""digit"": ""0123456789"",
        ""0123456789"": ""digit"",
        ""special"": ""`1~!@#$%^&*()_+-={':[,]}|;.</>?"",
        ""hex"": ""\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"",
        ""true"": true,
        ""false"": false,
        ""null"": null,
        ""array"":[  ],
        ""object"":{  },
        ""address"": ""50 St. James Street"",
        ""url"": ""http://www.JSON.org/"",
        ""comment"": ""// /* <!-- --"",
        ""# -- --> */"": "" "",
        "" s p a c e d "" :[1,2 , 3

,

4 , 5        ,          6           ,7        ],""compact"":[1,2,3,4,5,6,7],
        ""jsontext"": ""{\""object with 1 member\"":[\""array with 1 element\""]}"",
        ""quotes"": ""&#34; \u0022 %22 0x22 034 &#x22;"",
        ""\/\\\""\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?""
: ""A key can be any string""
    },
    0.5 ,98.6
,
99.44
,

1066,
1e1,
0.1e1,
1e-1,
1e00,2e+00,2e-00
,""rosebud""]";

			var expected = new[]
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Complex Graph Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			const string input = null;
			var expected = new Token<CommonTokenType>[0];

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<CommonTokenType>[0];

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests

		#region Illegal Sequence Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ObjectExtraComma_ThrowsDeserializationException()
		{
			// input from fail9.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{""Extra comma"": true,}";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(21, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_IllegalExpression_ThrowsDeserializationException()
		{
			// input from fail11.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{""Illegal expression"": 1 + 2}";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(24, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_IllegalInvocation_ThrowsDeserializationException()
		{
			// input from fail12.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{""Illegal invocation"": alert()}";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(23, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_IllegalStatement_ThrowsDeserializationException()
		{
			const string input = @"var foo = true;";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(0, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_IllegalFunction_ThrowsDeserializationException()
		{
			const string input = @"new function() { }";

			var tokenizer = new JsonReader.JsonTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			// verify exception is coming from expected index
			Assert.Equal(0, ex.Index);
		}

		#endregion Illegal Sequence Tests
	}
}
