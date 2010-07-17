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
using System.Text;

using JsonFx.Json;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Bson
{
	public class BsonConverterTests
	{
		#region Round Trip Tests

		[Fact]
		public void ConvertJson2Bson_HelloWorld_RoundTripsJsonToBsonAndBack()
		{
			// input from example at http://bsonspec.org/#/specification
			var inputText =
@"{
	""hello"" : ""world""
}";

			var expectedBinary = Encoding.UTF8.GetBytes(
				"\x16\x00\x00\x00\x02hello\x00"+
				"\x06\x00\x00\x00world\x00\x00");

			var expectedText = @"{""hello"":""world""}";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputText);

			var bsonFormatter = new BsonWriter.BsonFormatter();
			var actualBinary = bsonFormatter.Format(tokens1);

			Assert.Equal(expectedBinary, actualBinary);

			var bsonTokenizer = new BsonReader.BsonTokenizer();
			var tokens2 = bsonTokenizer.GetTokens(actualBinary);

			var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = false });
			var actualText = jsonFormatter.Format(tokens2);

			Assert.Equal(expectedText, actualText);
		}

		[Fact]
		public void ConvertJson2Bson_ArrayAsProperty_RoundTripsJsonToBsonAndBack()
		{
			// input from example at http://bsonspec.org/#/specification
			var inputText =
@"{
	""BSON"" : [
		""awesome"",
		5.05,
		1986
	]
}";

			var expectedBinary = new byte[]
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

			var expectedText = @"{""BSON"":[""awesome"",5.05,1986]}";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputText);

			var bsonFormatter = new BsonWriter.BsonFormatter();
			var actualBinary = bsonFormatter.Format(tokens1);

			Assert.Equal(expectedBinary, actualBinary);

			var bsonTokenizer = new BsonReader.BsonTokenizer();
			var tokens2 = bsonTokenizer.GetTokens(actualBinary);

			var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = false });
			var actualText = jsonFormatter.Format(tokens2);

			Assert.Equal(expectedText, actualText);
		}

		[Fact]
		public void ConvertJson2Bson_BooleanValue_RoundTripsJsonToBsonAndBack()
		{
			// input from example at http://codebetter.com/blogs/karlseguin/archive/2010/03/05/bson-serialization.aspx
			var inputText = @"{valid:true}";

			var expectedBinary = new byte[]
				{
					13, 0, 0, 0, 8, 118, 97, 108, 105, 100, 0, 1, 0
				};

			var expectedText =
@"{
	""valid"" : true
}";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputText);

			var bsonFormatter = new BsonWriter.BsonFormatter();
			var actualBinary = bsonFormatter.Format(tokens1);

			Assert.Equal(expectedBinary, actualBinary);

			var bsonTokenizer = new BsonReader.BsonTokenizer();
			var tokens2 = bsonTokenizer.GetTokens(actualBinary);

			var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = true });
			var actualText = jsonFormatter.Format(tokens2);

			Assert.Equal(expectedText, actualText);
		}

		#endregion Round Trip Tests
	}
}
