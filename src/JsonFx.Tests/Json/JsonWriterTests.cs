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

using JsonFx.Json.Resolvers;
using JsonFx.Model.Filters;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;
using JsonFx.Xml.Resolvers;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json
{
	public class JsonWriterTests
	{
		#region Constants

		private const string TraitName = "JSON";
		private const string TraitValue = "Writer";

		#endregion Constants

		#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_StringArrayDefaults_SerializesArray()
		{
			var input = new string[]
				{
					"aaa",
					"bbb"
				};
			var expected = @"[""aaa"",""bbb""]";

			var actual = new JsonWriter().Write(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_OnlyDefaults_SerializesIso8601Dates()
		{
			var input = new object[]
				{
					"Normal string before",
					new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc),
					new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Unspecified),
					"Normal string after"
				};
			var expected = @"[""Normal string before"",""2008-02-29T23:59:59.999Z"",""2010-07-05T10:51:17.768"",""Normal string after""]";

			var actual = new JsonWriter().Write(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_RecognizesMultipleDateTimeFiltersIsoFirst_SerializesIso8601Dates()
		{
			var input = new object[]
				{
					"Normal string before",
					new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc),
					new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Utc),
					"Normal string after"
				};
			var expected = @"[""Normal string before"",""2008-02-29T23:59:59.999Z"",""2010-07-05T10:51:17.768Z"",""Normal string after""]";

			var writer = new JsonWriter(
				new DataWriterSettings(new Iso8601DateFilter(), new MSAjaxDateFilter()));

			var actual = writer.Write(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_RecognizesMultipleDateTimeFiltersAjaxFirst_SerializesMSAjaxDates()
		{
			var input = new object[]
				{
					"Normal string before",
					new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc),
					new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Utc),
					"Normal string after"
				};
			var expected = @"[""Normal string before"",""/Date(1204329599999)/"",""/Date(1278327077768)/"",""Normal string after""]";

			var writer = new JsonWriter(
				new DataWriterSettings(new MSAjaxDateFilter(), new Iso8601DateFilter()));

			var actual = writer.Write(input);

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Enum Tests

		public class Foo
		{
			public Bar Baz { get; set; }

			public override bool Equals(object obj)
			{
				Foo that = obj as Foo;
				if (that == null)
				{
					return false;
				}

				return this.Baz.Equals(that.Baz);
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}

		public enum Bar
		{
			First,
			Second,
			Third
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_ObjectContainingEnum_SerializesObject()
		{
			var input = new Foo { Baz = Bar.First };
			var expected = "{\"Baz\":\"First\"}";

			var actual = new JsonWriter().Write(input);

			Assert.Equal(expected, actual);
		}

		#endregion Enum Tests

		#region Resolver Tests

		public class CustomNamedObject
		{
			[JsonName("upperPropertyName")]
			public string UpperPropertyName { get; set; }

			[JsonName("arbitraryOther")]
			public string OtherPropertyName { get; set; }
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultipleConventions_ReturnsData()
		{
			var input = new CustomNamedObject
			{
				UpperPropertyName = "Foo.",
				OtherPropertyName = "Bar."
			};

			var expected = "{\"upperPropertyName\":\"Foo.\",\"arbitraryOther\":\"Bar.\"}";

			var resolver = new CombinedResolverStrategy(
				new JsonResolverStrategy(),
				new DataContractResolverStrategy(),
				new XmlResolverStrategy(),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.PascalCase),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.CamelCase),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-"),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Uppercase, "_"));

			var actual = new JsonWriter(new DataWriterSettings(resolver)).Write(input);

			Assert.Equal(expected, actual, false);
		}

		#endregion Resolver Tests
	}
}
