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

using JsonFx.IO;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Common.Filters
{
	public class MSAjaxDateFilterTests
	{
		#region Constants

		private const string TraitName = "Filters";
		private const string TraitValue = "DateTime";

		#endregion Constants

		#region TryRead Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryRead_StandardTimeZone_ReadsAsUtc()
		{
			var input = new Stream<Token<CommonTokenType>>(new[]
				{
					CommonGrammar.TokenValue(@"\/Date(1204329599999)\/")
				});

			var expected = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc);

			DateTime actual;
			Assert.True(new MSAjaxDateFilter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryRead_DaylightSavingsTimeZone_ReadsAsUtc()
		{
			var input = new Stream<Token<CommonTokenType>>(new[]
				{
					CommonGrammar.TokenValue(@"\/Date(1278327077768)\/")
				});

			var expected = new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Utc);

			DateTime actual;
			Assert.True(new MSAjaxDateFilter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryRead_FutureTimeZone_ReadsAsUtc()
		{
			var input = new Stream<Token<CommonTokenType>>(new[]
				{
					CommonGrammar.TokenValue(@"\/Date(4102444799999)\/")
				});

			var expected = new DateTime(2099, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);

			DateTime actual;
			Assert.True(new MSAjaxDateFilter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryRead_DateTimeMinValueUtc_ReadsAsDateTimeMinValue()
		{
			var expected = DateTime.MinValue;

			var input = new Stream<Token<CommonTokenType>>(new[]
				{
					CommonGrammar.TokenValue(@"\/Date(-62135596800000)\/")
				});

			DateTime actual;
			Assert.True(new MSAjaxDateFilter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryRead_DateTimeMaxValueUtc_ReadsAsDateTimeMaxValue()
		{
			var input = new Stream<Token<CommonTokenType>>(new[]
				{
					CommonGrammar.TokenValue(@"\/Date(253402300800000)\/")
				});

			var expected = DateTime.MaxValue;

			DateTime actual;
			Assert.True(new MSAjaxDateFilter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		#endregion TryRead Tests

		#region TryWrite Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_UtcStandardTimeZone_DirectlyWritesAsUtc()
		{
			var input = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc);

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(1204329599999)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_LocalStandardTimeZone_ConvertsToUtcAndWritesAsUtc()
		{
			// Note: test only valid in Pacific Standard/Daylight Savings Time
			var input = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Local);

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(1204358399999)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_UnspecifiedStandardTimeZone_DirectlyWritesAsUtc()
		{
			var input = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Unspecified);

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(1204329599999)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_UtcDaylightSavingsTimeZone_DirectlyWritesAsUtc()
		{
			var input = new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Utc);

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(1278327077768)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_LocalDaylightSavingsTimeZone_ConvertsToUtcAndWritesAsUtc()
		{
			// Note: test only valid in Pacific Standard/Daylight Savings Time
			var input = new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Local);

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(1278352277768)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_UnspecifiedDaylightSavingsTimeZone_DirectlyWritesAsUtc()
		{
			var input = new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Unspecified);

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(1278327077768)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_FutureUtcTimeZone_DirectlyWritesAsUtc()
		{
			var input = new DateTime(2099, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(4102444799999)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_FutureLocalTimeZone_ConvertsToUtcAndWritesAsUtc()
		{
			// Note: test only valid in Pacific Standard/Daylight Savings Time
			var input = new DateTime(2099, 12, 31, 23, 59, 59, 999, DateTimeKind.Local);

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(4102473599999)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_FutureUnspecifiedTimeZone_DirectlyWritesAsUtc()
		{
			var input = new DateTime(2099, 12, 31, 23, 59, 59, 999, DateTimeKind.Unspecified);

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(4102444799999)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_DateTimeMinValue_DirectlyWritesAsUtc()
		{
			var input = DateTime.MinValue;

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(-62135596800000)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void TryWrite_DateTimeMaxValue_DirectlyWritesAsUtc()
		{
			var input = DateTime.MaxValue;

			var expected = new[]
				{
					CommonGrammar.TokenValue(@"\/Date(253402300800000)\/")
				};

			IEnumerable<Token<CommonTokenType>> actual;
			Assert.True(new MSAjaxDateFilter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		#endregion TryWrite Tests
	}
}
