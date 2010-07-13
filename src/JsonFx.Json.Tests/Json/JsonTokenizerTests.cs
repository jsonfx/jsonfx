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

using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json
{
	public class JsonTokenizerTests
	{
		#region Array Tests

		[Fact]
		public void GetTokens_ArrayEmpty_ReturnsEmptyArrayTokens()
		{
			const string input = "[]";
			var expected = new []
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayOneItem_ReturnsArrayTokens()
		{
			const string input = "[null]";
			var expected = new []
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenNull,
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayMultiItemWhitespace_ReturnsSimpleArrayTokens()
		{
			const string input = "[ 0, null,  false,true ]";
			var expected = new []
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayNestedDeeply_ReturnsNestedArrayTokens()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[[[[[[[[[[[[[[[[[[[""Not too deep""]]]]]]]]]]]]]]]]]]]";
			var expected = new []
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayExtraComma_ProducesInvalidSequence()
		{
			// NOTE: analyzer should flag this as an error as is grammar error, not tokenization error

			// input from fail4.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""extra comma"",]";
			var expected = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("extra comma"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayDoubleExtraComma_ProducesInvalidSequence()
		{
			// NOTE: analyzer should flag this as an error as is grammar error, not tokenization error

			// input from fail5.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""double extra comma"",,]";
			var expected = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("double extra comma"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayMissingValue_ProducesInvalidSequence()
		{
			// NOTE: analyzer should flag this as an error as is grammar error, not tokenization error

			// input from fail6.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[   , ""<-- missing value""]";
			var expected = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenValue("<-- missing value"),
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayColonInsteadOfComma_ProducesInvalidSequence()
		{
			// NOTE: analyzer should flag this as an error as is grammar error, not tokenization error

			// input from fail22.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""Colon instead of comma"": false]";
			var expected = new[]
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenProperty("Colon instead of comma"),
				DataGrammar.TokenFalse,
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayBadValue_ThrowsAnalyzerException()
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
		public void GetTokens_ObjectEmpty_ReturnsEmptyObjectTokens()
		{
			const string input = "{}";
			var expected = new []
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectOneProperty_ReturnsSimpleObjectTokens()
		{
			const string input = @"{""key"":""value""}";
			var expected = new []
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("key"),
				DataGrammar.TokenValue("value"),
				DataGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectCommaInsteadOfColon_ProducesInvalidSequence()
		{
			// NOTE: analyzer should flag this as an error as is grammar error, not tokenization error

			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = @"{""Comma instead of colon"", null}";

			var expected = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenValue("Comma instead of colon"),
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenNull,
				DataGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectUnterminated_ProducesInvalidSequence()
		{
			// NOTE: analyzer should flag this as an error as is grammar error, not tokenization error

			// input from fail32.json in test suite at http://www.json.org/JSON_checker/
			var input = @"{""Comma instead of closing brace"": true,";

			var expected = new[]
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("Comma instead of closing brace"),
				DataGrammar.TokenTrue,
				DataGrammar.TokenValueDelim
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region String Tests

		[Fact]
		public void GetTokens_StringEmpty_ReturnsEmptyStringToken()
		{
			const string input = "\"\"";
			var expected = new []
			{
				DataGrammar.TokenValue(String.Empty)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringSimple_ReturnsStringToken()
		{
			// input from fail1.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"""A JSON payload should be an object or array, not a string.""";
			var expected = new []
			{
				DataGrammar.TokenValue("A JSON payload should be an object or array, not a string.")
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringEscapedChars_ReturnsStringToken()
		{
			const string input = @"""\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\""""";
			var expected = new []
			{
				DataGrammar.TokenValue("\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\"")
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringImproperlyEscapedChars_ReturnsStringTokenWithSimpleChars()
		{
			const string input = @"""\u\u1\u12\u123\u12345""";
			var expected = new []
			{
				DataGrammar.TokenValue("uu1u12u123\u12345")
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringUnrecognizedEscapeLetter_EscapesToSimpleChar()
		{
			// input from fail15.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""Illegal backslash escape: \x15""]";
			var expected = new []
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("Illegal backslash escape: x15"),
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringUnrecognizedEscapeNull_CharIgnored()
		{
			// input from fail17.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""Illegal backslash escape: \017""]";
			var expected = new []
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("Illegal backslash escape: 17"),
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringSingleQuote_ReturnsStringToken()
		{
			// input from fail24.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"['single quote']";
			var expected = new []
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("single quote"),
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringUnescapedSingleQuote_ReturnsStringToken()
		{
			const string input = @"""unescaped ' single quote""";
			var expected = new []
			{
				DataGrammar.TokenValue("unescaped ' single quote"),
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringUnescapedDoubleQuote_ReturnsStringToken()
		{
			const string input = @"'unescaped "" quote'";
			var expected = new []
			{
				DataGrammar.TokenValue("unescaped \" quote"),
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringTabChar_ReturnsStringToken()
		{
			// input from fail25.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""	tab	character	in	string	""]";
			var expected = new []
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("\ttab\tcharacter\tin\tstring\t"),
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringEscapedTabChar_ReturnsStringToken()
		{
			// input from fail26.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[""\	tab\	character\	in\	string\	""]";
			var expected = new []
			{
				DataGrammar.TokenArrayBegin,
				DataGrammar.TokenValue("\ttab\tcharacter\tin\tstring\t"),
				DataGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
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
		public void GetTokens_NumberInteger_ReturnsNumberToken()
		{
			const string input = "123456";
			var expected = new []
			{
				DataGrammar.TokenValue(123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberFloat_ReturnsNumberToken()
		{
			const string input = "1.23456";
			var expected = new []
			{
				DataGrammar.TokenValue(1.23456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberNegFloat_ReturnsNumberToken()
		{
			const string input = "-0.123456";
			var expected = new []
			{
				DataGrammar.TokenValue(-0.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberNoLeadingDigitFloat_ReturnsNumberToken()
		{
			const string input = ".123456";
			var expected = new []
			{
				DataGrammar.TokenValue(.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberPosNoLeadingDigitFloat_ReturnsNumberToken()
		{
			const string input = "+.123456";
			var expected = new []
			{
				DataGrammar.TokenValue(.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberNegNoLeadingDigitFloat_ReturnsNumberToken()
		{
			const string input = "-.123456";
			var expected = new []
			{
				DataGrammar.TokenValue(-.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberIntegerLeadingZero_ReturnsObjectTokensWithNumberValue()
		{
			// input from fail13.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{""Numbers cannot have leading zeroes"": 013}";
			var expected = new []
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("Numbers cannot have leading zeroes"),
				DataGrammar.TokenValue(13),
				DataGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
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
		public void GetTokens_LiteralNonQuotedKey_ReturnsObjectTokensWithLiteralKey()
		{
			// input from fail3.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{unquoted_key: ""keys must be quoted""}";
			var expected = new []
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("unquoted_key"),
				DataGrammar.TokenValue("keys must be quoted"),
				DataGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_LiteralNonQuotedKeyDollarSign_ReturnsObjectTokensWithLiteralKey()
		{
			const string input = @"{ $abcdefg0123456 : false }";
			var expected = new []
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("$abcdefg0123456"),
				DataGrammar.TokenFalse,
				DataGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_LiteralNonQuotedKeyNumber_ReturnsObjectTokensWithLiteralKey()
		{
			const string input = @"{ _123456 : true }";
			var expected = new []
			{
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("_123456"),
				DataGrammar.TokenTrue,
				DataGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Literal Tests

		#region Keyword Tests

		[Fact]
		public void GetTokens_KeywordUndefined_ReturnsUndefinedToken()
		{
			const string input = @"undefined";
			var expected = new []
			{
				DataGrammar.TokenNull
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordNull_ReturnsNullToken()
		{
			const string input = @"null";
			var expected = new []
			{
				DataGrammar.TokenNull
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordFalse_ReturnsFalseToken()
		{
			const string input = @"false";
			var expected = new []
			{
				DataGrammar.TokenFalse
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordTrue_ReturnsTrueToken()
		{
			const string input = @"true";
			var expected = new []
			{
				DataGrammar.TokenTrue
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordNan_ReturnsNanToken()
		{
			const string input = @"NaN";
			var expected = new []
			{
				DataGrammar.TokenNaN
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordInfinity_ReturnsPositiveInfinityToken()
		{
			const string input = @"Infinity";
			var expected = new []
			{
				DataGrammar.TokenPositiveInfinity
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordPosInfinity_ReturnsPositiveInfinityToken()
		{
			const string input = @"+Infinity";
			var expected = new []
			{
				DataGrammar.TokenPositiveInfinity
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordNegInfinity_ReturnsNegativeInfinityToken()
		{
			const string input = @"-Infinity";
			var expected = new []
			{
				DataGrammar.TokenNegativeInfinity
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		#endregion Keyword Tests

		#region Complex Graph Tests

		[Fact]
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
				DataGrammar.TokenObjectBegin,
				DataGrammar.TokenProperty("foo"),
				DataGrammar.TokenNull,
				DataGrammar.TokenValueDelim,
				DataGrammar.TokenProperty("bar"),
				DataGrammar.TokenValue("value"),
				DataGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Complex Graph Tests

		#region Input Edge Case Tests

		[Fact]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			const string input = null;
			var expected = new Token<DataTokenType>[0];

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<DataTokenType>[0];

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests

		#region Illegal Sequence Tests

		[Fact]
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
			Assert.Equal(26, ex.Index);
		}

		[Fact]
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
