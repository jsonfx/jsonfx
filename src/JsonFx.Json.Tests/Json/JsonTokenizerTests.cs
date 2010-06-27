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

using JsonFx.Serialization;
using Xunit;

namespace JsonFx.Json
{
	public class JsonTokenizerTests
	{
		#region Simple Passing Array Sequences

		[Fact]
		public void GetTokens_ArrayEmpty_ReturnsEmptyArrayTokens()
		{
			const string input = "[]";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayOneItem_ReturnsArrayTokens()
		{
			const string input = "[null]";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayMultiItemWhitespace_ReturnsSimpleArrayTokens()
		{
			const string input = "[ 0, null, false ]";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenArrayEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		#endregion Simple Passing Array Sequences

		#region Simple Passing Object Sequences

		[Fact]
		public void GetTokens_ObjectEmpty_ReturnsEmptyObjectTokens()
		{
			const string input = "{}";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectOneProperty_ReturnsSimpleObjectTokens()
		{
			const string input = "{\"key\":\"value\"}";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("key"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("value"),
				JsonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		#endregion Simple Passing Object Sequences

		#region Simple Passing String Sequences

		[Fact]
		public void GetTokens_StringEmpty_ReturnsStringToken()
		{
			const string input = "\"\"";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenString(String.Empty)
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		#endregion Simple Passing String Sequences

		#region Simple Passing Number Sequences

		[Fact]
		public void GetTokens_NumberInteger_ReturnsNumberToken()
		{
			const string input = "123456";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenNumber(123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberFloat_ReturnsNumberToken()
		{
			const string input = "1.23456";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenNumber(1.23456)
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberNegFloat_ReturnsNumberToken()
		{
			const string input = "-0.123456";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenNumber(-0.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberNoLeadingDigitFloat_ReturnsNumberToken()
		{
			const string input = ".123456";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenNumber(.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberPosNoLeadingDigitFloat_ReturnsNumberToken()
		{
			const string input = "+.123456";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenNumber(.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NumberNegNoLeadingDigitFloat_ReturnsNumberToken()
		{
			const string input = "-.123456";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenNumber(-.123456)
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		#endregion Simple Passing Number Sequences

		#region Simple Passing Literal Sequences

		[Fact]
		public void GetTokens_LiteralNonQuotedKey_NumberObjectWithLiteralKey()
		{
			const string input = @"{ key : null }";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenLiteral("key"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_LiteralNonQuotedKeyDollarSign_NumberObjectWithLiteralKey()
		{
			const string input = @"{ $abcdefg0123456 : false }";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenLiteral("$abcdefg0123456"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_LiteralNonQuotedKeyNumber_NumberObjectWithLiteralKey()
		{
			const string input = @"{ _123456 : true }";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenLiteral("_123456"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenObjectEnd
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		#endregion Simple Passing Literal Sequences

		#region Simple Passing Keyword Sequences

		[Fact]
		public void GetTokens_KeywordNull_ReturnsNullToken()
		{
			const string input = @"null";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenNull
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordUndefined_ReturnsUndefinedToken()
		{
			const string input = @"undefined";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenUndefined
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordFalse_ReturnsFalseToken()
		{
			const string input = @"false";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenFalse
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordTrue_ReturnsTrueToken()
		{
			const string input = @"true";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenTrue
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordNan_ReturnsNanToken()
		{
			const string input = @"NaN";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenNaN
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordInfinity_ReturnsPositiveInfinityToken()
		{
			const string input = @"Infinity";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenPositiveInfinity
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordPosInfinity_ReturnsPositiveInfinityToken()
		{
			const string input = @"+Infinity";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenPositiveInfinity
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_KeywordNegInfinity_ReturnsNegativeInfinityToken()
		{
			const string input = @"-Infinity";
			var expected = new List<Token<JsonTokenType>>
			{
				JsonGrammar.TokenNegativeInfinity
			};

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		#endregion Simple Passing Keyword Sequences

		#region Complex Passing Graph Sequences

		[Fact]
		public void GetTokens_GraphComplex_ReturnsGraphTokenStream()
		{
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

			var expected = new List<Token<JsonTokenType>>
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

			var tokenizer = new JsonReader.JsonTokenizer(new DataReaderSettings());
			var actual = new List<Token<JsonTokenType>>(tokenizer.GetTokens(input));

			Assert.Equal(expected, actual);
		}

		#endregion Complex Passing Graph Sequences
	}
}
