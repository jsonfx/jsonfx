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
using System.Collections.Generic;

using JsonFx.Serialization;
using Xunit;

namespace JsonFx.Json
{
	public class JsonParserTests
	{
		#region Test Types

		private enum ExampleEnum
		{
			[DataName("zero")]
			Zero = 0,

			[DataName("one")]
			One = 1,

			[DataName("two")]
			Two = 2,

			[DataName("three")]
			Three = 3
		}

		#endregion Test Types

		#region Array Tests

		[Fact]
		public void GetTokens_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new []
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd
			};

			var expected = new object[0];

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse((input), null);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayUnclosed_ThrowsArgumentException()
		{
			// input from fail2.json in test suite at http://www.json.org/JSON_checker/
			var input = new []
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Unclosed array")
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ArrayExtraComma_ThrowsArgumentException()
		{
			// input from fail4.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("extra comma"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ArrayDoubleExtraComma_ThrowsArgumentException()
		{
			// input from fail5.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("double extra comma"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ArrayMissingValue_ThrowsArgumentException()
		{
			// input from fail6.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("<-- missing value"),
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact(Skip="JsonParser doesn't currently check after stream")]
		public void GetTokens_ArrayCommaAfterClose_ThrowsArgumentException()
		{
			// input from fail7.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Comma after the close"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact(Skip="JsonParser doesn't currently check after stream")]
		public void GetTokens_ArrayExtraClose_ThrowsArgumentException()
		{
			// input from fail8.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Extra close"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ObjectExtraComma_ThrowsArgumentException()
		{
			// input from fail9.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Extra comma"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact(Skip="JsonParser doesn't currently check after stream")]
		public void GetTokens_ObjectExtraValueAfterClose_ThrowsArgumentException()
		{
			// input from fail10.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Extra value after close"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenString("misplaced quoted value")
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact(Skip="JsonParser doesn't currently check depth")]
		public void GetTokens_ArraysNestedTooDep_ThrowsArgumentException()
		{
			// input from fail18.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
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
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Too deep"),
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
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ObjectMissingColon_ThrowsArgumentException()
		{
			// input from fail19.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Missing colon"),
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ObjectDoubleColon_ThrowsArgumentException()
		{
			// input from fail20.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Double colon"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ObjectCommaInsteadOfColon_ThrowsArgumentException()
		{
			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Comma instead of colon"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ArrayColonInsteadOfComma_ThrowsArgumentException()
		{
			// input from fail22.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Colon instead of comma"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ArrayBadValue_ThrowsArgumentException()
		{
			// input from fail23.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Bad value"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenLiteral("truth"),
				JsonGrammar.TokenArrayEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ObjectCommaInsteadOfClose_ThrowsArgumentException()
		{
			// input from fail32.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Comma instead if closing brace"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		[Fact]
		public void GetTokens_ArrayCloseMismatch_ThrowsArgumentException()
		{
			// input from fail33.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("mismatch"),
				JsonGrammar.TokenObjectEnd
			};

			var parser = new JsonReader.JsonParser(new DataReaderSettings());

			Assert.Throws<ArgumentException>(
				delegate
				{
					var actual = parser.Parse(input, null);
				});
		}

		#endregion Array Tests

		#region Enum Tests

		[Fact]
		public void GetTokens_EnumFromString_ReturnsEnum()
		{
			var input = new[]
			{
				JsonGrammar.TokenString("Two")
			};

			var expected = ExampleEnum.Two;

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse((input), typeof(ExampleEnum));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EnumFromJsonName_ReturnsEnum()
		{
			var input = new[]
			{
				JsonGrammar.TokenString("two")
			};

			var expected = ExampleEnum.Two;

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse((input), typeof(ExampleEnum));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EnumFromNumber_ReturnsEnum()
		{
			var input = new[]
			{
				JsonGrammar.TokenNumber(3)
			};

			var expected = ExampleEnum.Three;

			var parser = new JsonReader.JsonParser(new DataReaderSettings());
			var actual = parser.Parse((input), typeof(ExampleEnum));

			Assert.Equal(expected, actual);
		}

		#endregion Enum Tests
	}
}
