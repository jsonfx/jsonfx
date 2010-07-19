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
using System.IO;
using System.Linq;
using System.Text;

using JsonFx.Common;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Bson
{
	public class BsonTokenizerTests
	{
		#region Array Tests

		[Fact]
		public void GetTokens_ArrayRoot_ReturnsDocument()
		{
			// BSON doesn't provide a way to know if the root element is an array

			var input = new byte[]
			{
				0x26, 0x00, 0x00, 0x00,
					0x02, (byte)'0', 0, 0x08, 0x00, 0x00, 0x00, (byte)'a', (byte)'w', (byte)'e', (byte)'s', (byte)'o', (byte)'m', (byte)'e', 0x00,
					0x01, (byte)'1', 0, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x14, 0x40,
					0x10, (byte)'2', 0, 0xC2, 0x07, 0x00, 0x00,
				0x00
			};

			var expected = new[]
		    {
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("0"),
				CommonGrammar.TokenValue("awesome"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("1"),
				CommonGrammar.TokenValue(5.05),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("2"),
				CommonGrammar.TokenValue(1986),
				CommonGrammar.TokenObjectEnd
		    };

			var tokenizer = new BsonReader.BsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		public void GetTokens_HelloWorld_ReturnsDocument()
		{
			// input from example at http://bsonspec.org/#/specification
			var input = Encoding.UTF8.GetBytes(
				"\x16\x00\x00\x00\x02hello\x00"+
				"\x06\x00\x00\x00world\x00\x00");

			var expected = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("hello"),
				CommonGrammar.TokenValue("world"),
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new BsonReader.BsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayAsProperty_ReturnsDocument()
		{
			// input from example at http://bsonspec.org/#/specification

			// Encoding doesn't seem to like control chars
			//var input = Encoding.UTF8.GetBytes(
			//    "1\x00\x00\x00\x04BSON\x00&\x00"+
			//    "\x00\x00\x020\x00\x08\x00\x00"+
			//    "\x00awesome\x00\x011\x00333333"+
			//    "\x14@\x102\x00\xc2\x07\x00\x00"+
			//    "\x00\x00");

			var input = new byte[]
			{
				0x31, 0x00, 0x00, 0x00, 
				0x04, (byte)'B', (byte)'S', (byte)'O', (byte)'N', 0x00,
				0x26, 0x00, 0x00, 0x00,
					0x02, (byte)'0', 0, 0x08, 0x00, 0x00, 0x00, (byte)'a', (byte)'w', (byte)'e', (byte)'s', (byte)'o', (byte)'m', (byte)'e', 0x00,
					0x01, (byte)'1', 0, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x14, 0x40,
					0x10, (byte)'2', 0, 0xC2, 0x07, 0x00, 0x00,
				0x00,
				0x00
			};

		    var expected = new[]
		    {
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("BSON"),
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenValue("awesome"),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(5.05),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenValue(1986),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenObjectEnd
		    };

		    var tokenizer = new BsonReader.BsonTokenizer();
		    var actual = tokenizer.GetTokens(input).ToArray();

		    Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_BooleanValue_ReturnsDocument()
		{
			// input from example at http://codebetter.com/blogs/karlseguin/archive/2010/03/05/bson-serialization.aspx
			var input = new byte[]
				{
					13, 0, 0, 0, 8, 118, 97, 108, 105, 100, 0, 1, 0
				};

			var expected = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenProperty("valid"),
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenObjectEnd
			};

			var tokenizer = new BsonReader.BsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region Input Edge Case Tests

		[Fact]
		public void GetTokens_NullByteArray_ThrowsArgumentNullException()
		{
			var input = (byte[])null;
			var expected = new Token<CommonTokenType>[0];

			var tokenizer = new BsonReader.BsonTokenizer();

			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal("bytes", ex.ParamName);
		}

		[Fact]
		public void GetTokens_NullStream_ThrowsArgumentNullException()
		{
			var input = (Stream)null;
			var expected = new Token<CommonTokenType>[0];

			var tokenizer = new BsonReader.BsonTokenizer();

			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal("stream", ex.ParamName);
		}

		[Fact]
		public void GetTokens_EmptyByteArray_ReturnsEmptySequence()
		{
			var input = new byte[0];
			var expected = new Token<CommonTokenType>[0];

			var tokenizer = new BsonReader.BsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyStream_ReturnsEmptySequence()
		{
			var input = Stream.Null;
			var expected = new Token<CommonTokenType>[0];

			var tokenizer = new BsonReader.BsonTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
