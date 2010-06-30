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
using System.Linq;

using JsonFx.Serialization;
using Xunit;
using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json
{
	public class JsonGeneratorTests
	{
		#region Array Tests

		[Fact]
		public void GetTokens_ArrayEmpty_ReturnsEmptyArrayTokens()
		{
			var input = new object[0];

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenArrayEnd
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ListEmpty_ReturnsEmptyArrayTokens()
		{
			var input = new List<object>(0);

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenArrayEnd
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArraySingleItem_ReturnsSingleItemArrayTokens()
		{
			var input = new object[]
				{
					null
				};

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenArrayEnd
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayMultiItem_ReturnsArrayTokens()
		{
			var input = new object[]
				{
					false,
					true,
					null,
					'a',
					'b',
					'c',
					1,
					2,
					3
				};

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenFalse,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenTrue,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('a'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('b'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('c'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(1),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(2),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(3),
					JsonGrammar.TokenArrayEnd
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayNested_ReturnsNestedArrayTokens()
		{
			var input = new object[]
				{
					false,
					true,
					null,
					new []
					{
						'a',
						'b',
						'c'
					},
					new []
					{
						1,
						2,
						3
					}
				};

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenFalse,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenTrue,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenString('a'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('b'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('c'),
					JsonGrammar.TokenArrayEnd,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenNumber(1),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(2),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(3),
					JsonGrammar.TokenArrayEnd,
					JsonGrammar.TokenArrayEnd
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		public void GetTokens_EmptyObject_ReturnsEmptyObjectTokens()
		{
			var input = new object();

			var expected = new[]
				{
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenObjectEnd
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyDictionary_ReturnsEmptyObjectTokens()
		{
			var input = new Dictionary<string,object>(0);

			var expected = new[]
				{
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenObjectEnd
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region Boolean Tests

		[Fact]
		public void GetTokens_False_ReturnsFalseToken()
		{
			var input = false;

			var expected = new[]
				{
					JsonGrammar.TokenFalse
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_True_ReturnsTrueToken()
		{
			var input = true;

			var expected = new[]
				{
					JsonGrammar.TokenTrue
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Boolean Tests

		#region Number Case Tests

		[Fact]
		public void GetTokens_NaN_ReturnsNaNToken()
		{
			var input = Double.NaN;

			var expected = new[]
				{
					JsonGrammar.TokenNaN
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_PosInfinity_ReturnsPosInfinityToken()
		{
			var input = Double.PositiveInfinity;

			var expected = new[]
				{
					JsonGrammar.TokenPositiveInfinity
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NegInfinity_ReturnsNegInfinityToken()
		{
			var input = Double.NegativeInfinity;

			var expected = new[]
				{
					JsonGrammar.TokenNegativeInfinity
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Number Tests

		#region Input Edge Case Tests

		[Fact]
		public void GetTokens_Null_ReturnsNullToken()
		{
			var input = (object)null;

			var expected = new[]
				{
					JsonGrammar.TokenNull
				};

			var generator = new JsonWriter.JsonGenerator(new DataWriterSettings());
			var actual = generator.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Ctor_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var generator = new JsonWriter.JsonGenerator(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
