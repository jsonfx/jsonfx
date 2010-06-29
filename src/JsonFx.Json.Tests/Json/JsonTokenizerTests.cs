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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenArrayEnd
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		public void GetTokens_ObjectEmpty_ReturnsEmptyObjectTokens()
		{
			const string input = "{}";
			var expected = new []
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("key"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("value"),
				JsonGrammar.TokenObjectEnd
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region String Tests

		[Fact]
		public void GetTokens_StringEmpty_ReturnsStringToken()
		{
			const string input = "\"\"";
			var expected = new []
			{
				JsonGrammar.TokenString(String.Empty)
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
				JsonGrammar.TokenString("A JSON payload should be an object or array, not a string.")
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
				JsonGrammar.TokenString("\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\"")
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
				JsonGrammar.TokenString("uu1u12u123\u12345")
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Illegal backslash escape: x15"),
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Illegal backslash escape: 17"),
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("single quote"),
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenString("unescaped ' single quote"),
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_StringUnescapedQuote_ReturnsStringToken()
		{
			const string input = @"'unescaped "" quote'";
			var expected = new []
			{
				JsonGrammar.TokenString("unescaped \" quote"),
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("\ttab\tcharacter\tin\tstring\t"),
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("\ttab\tcharacter\tin\tstring\t"),
				JsonGrammar.TokenArrayEnd
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
				JsonGrammar.TokenNumber(123456)
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
				JsonGrammar.TokenNumber(1.23456)
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
				JsonGrammar.TokenNumber(-0.123456)
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
				JsonGrammar.TokenNumber(.123456)
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
				JsonGrammar.TokenNumber(.123456)
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
				JsonGrammar.TokenNumber(-.123456)
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Numbers cannot have leading zeroes"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(13),
				JsonGrammar.TokenObjectEnd
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenLiteral("unquoted_key"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("keys must be quoted"),
				JsonGrammar.TokenObjectEnd
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenLiteral("$abcdefg0123456"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenObjectEnd
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenLiteral("_123456"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenObjectEnd
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
				JsonGrammar.TokenUndefined
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
				JsonGrammar.TokenNull
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
				JsonGrammar.TokenFalse
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
				JsonGrammar.TokenTrue
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
				JsonGrammar.TokenNaN
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
				JsonGrammar.TokenPositiveInfinity
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
				JsonGrammar.TokenPositiveInfinity
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
				JsonGrammar.TokenNegativeInfinity
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
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("foo"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("bar"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("value"),
				JsonGrammar.TokenObjectEnd
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

			var expected = new []
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
			var expected = new Token<JsonTokenType>[0];

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<JsonTokenType>[0];

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
			Assert.Equal(28, ex.Index);
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
			Assert.Equal(8, ex.Index);
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
			Assert.Equal(12, ex.Index);
		}

		#endregion Illegal Sequence Tests
	}
}
