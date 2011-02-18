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

using JsonFx.Model;
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(0),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenProperty("Colon instead of comma"),
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("key"),
				ModelGrammar.TokenPrimitive("value"),
				ModelGrammar.TokenObjectEnd
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ObjectCommaInsteadOfColon_ProducesInvalidSequence()
		{
			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = @"{""Comma instead of colon"", null}";
			var expected = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenPrimitive("Comma instead of colon"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("Comma instead of closing brace"),
				ModelGrammar.TokenTrue
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
				ModelGrammar.TokenPrimitive(String.Empty)
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
				ModelGrammar.TokenPrimitive("A JSON payload should be an object or array, not a string.")
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
				ModelGrammar.TokenPrimitive("\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\"")
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
				ModelGrammar.TokenPrimitive("uu1u12u123\u12345")
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("Illegal backslash escape: x15"),
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("Illegal backslash escape: 17"),
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("single quote"),
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenPrimitive("unescaped ' single quote"),
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
				ModelGrammar.TokenPrimitive("unescaped \" quote"),
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("\ttab\tcharacter\tin\tstring\t"),
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("\ttab\tcharacter\tin\tstring\t"),
				ModelGrammar.TokenArrayEnd
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
				ModelGrammar.TokenPrimitive(123456)
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
				ModelGrammar.TokenPrimitive(1.23456)
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
				ModelGrammar.TokenPrimitive(-0.123456)
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
				ModelGrammar.TokenPrimitive(.123456)
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
				ModelGrammar.TokenPrimitive(.123456)
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
				ModelGrammar.TokenPrimitive(-.123456)
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("Numbers cannot have leading zeroes"),
				ModelGrammar.TokenPrimitive(13),
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("unquoted_key"),
				ModelGrammar.TokenPrimitive("keys must be quoted"),
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("$abcdefg0123456"),
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("_123456"),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenObjectEnd
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
				ModelGrammar.TokenNull
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
				ModelGrammar.TokenNull
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
				ModelGrammar.TokenFalse
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
				ModelGrammar.TokenTrue
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
				ModelGrammar.TokenPrimitive(Double.NaN)
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
				ModelGrammar.TokenPrimitive(Double.PositiveInfinity)
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
				ModelGrammar.TokenPrimitive(Double.PositiveInfinity)
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
				ModelGrammar.TokenPrimitive(Double.NegativeInfinity)
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// this is not allowed according to strict JSON, but we're following Postel's Law
			Assert.Equal(expected, actual);
		}

		#endregion Keyword Tests

		#region Comment Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IgnoreMutipleSingleLineComments()
		{
			var input = "//comment1\r\n//comment2\r\n//comment3\r\n{ \"Bars\": [{\"Baz\": \"Test\"}]}";
			var expected = new []
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("Bars"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("Baz"),
				ModelGrammar.TokenPrimitive("Test"),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IgnoreMutilineComments()
		{
			var input = "/*comment1\r\ncomment2\r\ncomment3*/\r\n{ \"Bars\": [{\"Baz\": \"Test\"}]}";
			var expected = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("Bars"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("Baz"),
				ModelGrammar.TokenPrimitive("Test"),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Comment Tests

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
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("foo"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenProperty("bar"),
				ModelGrammar.TokenPrimitive("value"),
				ModelGrammar.TokenObjectEnd
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
			var expected = new Token<ModelTokenType>[0];

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<ModelTokenType>[0];

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

		#region Multiple Pass Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultiplePassesOverOutput_ReturnsNestedObjectTokensTwice()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"{
    ""JSON Test Pattern pass3"": {
        ""The outermost value"": ""must be an object or array."",
        ""In this test"": ""It is an object.""
    }
}
";

			var expected = new[]
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

			var tokenizer = new JsonReader.JsonTokenizer();
			var actual = tokenizer.GetTokens(input);

			Assert.Equal(expected, actual.ToArray(), false);

			Assert.Equal(expected, actual.ToArray(), false);
		}

		#endregion Multiple Pass Tests
	}
}
