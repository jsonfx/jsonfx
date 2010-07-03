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

using JsonFx.Serialization;
using Xunit;
using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json
{
	public class JsonReaderTests
	{
		#region Array Tests

		[Fact(Skip="JsonReader doesn't check stream after parsing")]
		public void Deserialize_ArrayCommaAfterClose_ThrowsDeserializationException()
		{
			// input from fail7.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""Comma after the close""],";

			var reader = new JsonReader(new DataReaderSettings());

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = reader.Deserialize(input);
				});

			// verify exception is coming from expected position
			Assert.Equal(35L, ex.Index);
		}

		[Fact(Skip="JsonReader doesn't check stream after parsing")]
		public void Deserialize_ArrayExtraClose_ThrowsDeserializationException()
		{
			// input from fail8.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""Extra close""]]";

			var reader = new JsonReader(new DataReaderSettings());

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = reader.Deserialize(input);
				});

			// verify exception is coming from expected position
			Assert.Equal(15L, ex.Index);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact(Skip="JsonReader doesn't check stream after parsing")]
		public void Deserialize_ObjectExtraValueAfterClose_ThrowsParseException()
		{
			// input from fail10.json in test suite at http://www.json.org/JSON_checker/
			var input = @"{""Extra value after close"": true} ""misplaced quoted value""";

			var reader = new JsonReader(new DataReaderSettings());

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = reader.Deserialize(input);
				});

			// verify exception is coming from expected position
			Assert.Equal(34L, ex.Index);
		}

		#endregion Object Tests
	}
}
