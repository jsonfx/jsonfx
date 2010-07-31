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

using JsonFx.Common;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json.Resolvers
{
	public class JsonResolverStrategyTests
	{
		#region Constants

		private const string TraitName = "Resolvers";
		private const string TraitValue = "JsonResolverStrategy";

		#endregion Constants

		#region Test Types

		private enum ExampleEnum
		{
			Zero = 0,

			[JsonName("")]
			One = 1,

			[JsonName("yellow")]
			Two = 2,

			[JsonName(null)]
			Three = 3
		}

		#endregion Test Types

		#region Enum Tests

		// TODO: these are actually testing type coercion and resolver strategy, need to isolate and improve testability

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void JsonAnalyzerParse_EnumFromString_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive("Two")
			};

			var expected = ExampleEnum.Two;

			var analyzer = new CommonAnalyzer(new DataReaderSettings(new JsonResolverStrategy()));
			var actual = analyzer.Analyze<ExampleEnum>(input).Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void JsonAnalyzerParse_EnumFromJsonName_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive("yellow")
			};

			var expected = ExampleEnum.Two;

			var analyzer = new CommonAnalyzer(new DataReaderSettings(new JsonResolverStrategy()));
			var actual = analyzer.Analyze<ExampleEnum>(input).Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void JsonAnalyzerParse_EnumFromNumber_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive(3)
			};

			var expected = ExampleEnum.Three;

			var analyzer = new CommonAnalyzer(new DataReaderSettings(new JsonResolverStrategy()));
			var actual = analyzer.Analyze<ExampleEnum>(input).Single();

			Assert.Equal(expected, actual);
		}

		#endregion Enum Tests

		#region Enum Tests

		// TODO: these are actually testing type coercion and resolver strategy, need to isolate and improve testability

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void JsonFormatterFormat_EnumPocoToString_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive(ExampleEnum.Zero)
			};

			var expected = @"""Zero""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void JsonFormatterFormat_EnumEmptyStringJsonNameToString_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive(ExampleEnum.One)
			};

			var expected = @"""One""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void JsonFormatterFormat_EnumWithJsonName_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive(ExampleEnum.Two)
			};

			var expected = @"""yellow""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void JsonFormatterFormat_EnumNullJsonNameToString_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive(ExampleEnum.Three)
			};

			var expected = @"""Three""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void JsonFormatterFormat_EnumFromNumber_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenPrimitive((int)ExampleEnum.Three)
			};

			var expected = "3";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Enum Tests
	}
}
