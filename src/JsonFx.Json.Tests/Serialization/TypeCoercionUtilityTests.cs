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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using JsonFx.Common;
using JsonFx.Json;
using JsonFx.Json.Resolvers;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization
{
	public class TypeCoercionUtilityTests
	{
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

		#region List Tests

		[Fact]
		public void CoerceType_CoerceToBaseType_NoCoercionNeeded()
		{
			dynamic input = new ExpandoObject();
			input.One = 1;
			input.Two = 2;
			input.Three = 3;
			input.Four = 4;

			var settings = new DataReaderSettings();
			var actual = new TypeCoercionUtility(settings, settings.AllowNullValueTypes).CoerceType(typeof(IDictionary<string, object>), input);

			Assert.Equal(input, actual);
		}

		[Fact]
		public void CoerceType_ArrayToList_CoercesViaListCtor()
		{
			var input = new string[]
				{
					"One",
					"Two",
					"Three",
					"Four"
				};

			var expected = new List<string>
				{
					"One",
					"Two",
					"Three",
					"Four"
				};

			var settings = new DataReaderSettings();
			var actual = new TypeCoercionUtility(settings, settings.AllowNullValueTypes).CoerceType(typeof(List<string>), input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void CoerceType_ExpandoToGenericDictionary_CoercesViaGenericDictionaryCopy()
		{
			dynamic input = new ExpandoObject();
			input.One = 1;
			input.Two = 2;
			input.Three = 3;
			input.Four = 4;

			var expected = new Dictionary<string, object>
				{
					{ "One", 1 },
					{ "Two", 2 },
					{ "Three", 3 },
					{ "Four", 4 }
				};

			var settings = new DataReaderSettings();
			var actual = new TypeCoercionUtility(settings, settings.AllowNullValueTypes).CoerceType(typeof(Dictionary<string, object>), input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void CoerceType_DictionaryToExpando_CoercesViaDictionaryCopy()
		{
			var input = new Hashtable
				{
					{ "One", 1 },
					{ "Two", 2 },
					{ "Three", 3 },
					{ "Four", 4 }
				};

			dynamic expected = new ExpandoObject();
			expected.One = 1;
			expected.Two = 2;
			expected.Three = 3;
			expected.Four = 4;

			var settings = new DataReaderSettings();
			var actual = new TypeCoercionUtility(settings, settings.AllowNullValueTypes).CoerceType(expected.GetType(), input);

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Enum Tests

		// TODO: these are actually testing type coercion and resolver strategy, need to isolate and improve testability

		[Fact]
		public void JsonAnalyzerParse_EnumFromString_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Two")
			};

			var expected = ExampleEnum.Two;

			var analyzer = new CommonAnalyzer(new DataReaderSettings(new JsonResolverStrategy()));
			var actual = analyzer.Analyze<ExampleEnum>(input).Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void JsonAnalyzerParse_EnumFromJsonName_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("yellow")
			};

			var expected = ExampleEnum.Two;

			var analyzer = new CommonAnalyzer(new DataReaderSettings(new JsonResolverStrategy()));
			var actual = analyzer.Analyze<ExampleEnum>(input).Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void JsonAnalyzerParse_EnumFromNumber_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(3)
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
		public void JsonFormatterFormat_EnumPocoToString_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(ExampleEnum.Zero)
			};

			var expected = @"""Zero""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void JsonFormatterFormat_EnumEmptyStringJsonNameToString_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(ExampleEnum.One)
			};

			var expected = @"""One""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void JsonFormatterFormat_EnumWithJsonName_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(ExampleEnum.Two)
			};

			var expected = @"""yellow""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void JsonFormatterFormat_EnumNullJsonNameToString_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue(ExampleEnum.Three)
			};

			var expected = @"""Three""";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void JsonFormatterFormat_EnumFromNumber_ReturnsEnum()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue((int)ExampleEnum.Three)
			};

			var expected = "3";

			var formatter = new JsonWriter.JsonFormatter(new DataWriterSettings(new JsonResolverStrategy()));
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Enum Tests
	}
}
