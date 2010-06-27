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
		public void GetTokens_EmptyArray_EmptyArrayTokens()
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
		public void GetTokens_OneItemArray_ArrayTokens()
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
		public void GetTokens_MultiItemArrayWhitespace_SimpleArrayTokens()
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
		public void GetTokens_EmptyObject_EmptyObjectTokens()
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
		public void GetTokens_OnePropertyObject_SimpleObjectTokens()
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
		public void GetTokens_EmptyString_StringToken()
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
		public void GetTokens_Integer_NumberToken()
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
		public void GetTokens_Float_NumberToken()
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
		public void GetTokens_NegFloat_NumberToken()
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
		public void GetTokens_NoLeadingDigitFloat_NumberToken()
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
		public void GetTokens_NegNoLeadingDigitFloat_NumberToken()
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
	}
}
