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

using JsonFx.Json;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Linq
{
	public class QueryEngineTests
	{
		#region Constants

		private const string TraitName = "LINQ";
		private const string TraitValue = "QueryEngine";

		#endregion Constants

		#region Anonymous Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Find_MatchingPropertyToArray_ReturnsSingleObject()
		{
			var input = @"
[
	{
		""key"": ""value""
	},
	{
		""key"": ""other-value""
	}
]";

			var expected =
				new []
				{
					new
					{
						Other = "otherValue",
						Key = "value"
					}
				};

			var source = new JsonReader().Query(input, new { key=String.Empty });

			var query =
				from obj in source
				where obj.key == "value"
				select new
				{
					Other = "otherValue",
					Key = obj.key
				};

			var actual = query.ToArray();

			Assert.Equal(expected, actual, true);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Find_MatchingPropertyFirstOrDefault_ReturnsSingleObject()
		{
			var input = @"
{
	""key"": ""value""
}";

			var expected =
				new
				{
					Other = "otherValue",
					Key = "value"
				};

			var source = new JsonReader().Query(input, new { key=String.Empty });

			var query =
				from obj in source
				where obj.key == "value"
				select new
				{
					Other = "otherValue",
					Key = obj.key
				};

			var actual = query.FirstOrDefault();

			Assert.Equal(expected, actual, true);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Find_MatchingPropertyToArray_ReturnsArray()
		{
			var input = @"
[
	{
		""key"": ""value""
	},
	{
		""key"": ""other-value""
	}
]";

			var expected = new[]
				{
					new
					{
						key = "value"
					}
				};

			var source = new JsonReader().Query(input, new { key=String.Empty });

			var query = source.Where(obj => obj.key == "value");

			var actual = query.ToArray();

			Assert.Equal(expected, actual, true);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Find_PropertyNoMatchFirstOrDefault_ReturnsNull()
		{
			var input = @"
[
	{
		""key"": ""value""
	},
	{
		""key"": ""other-value""
	}
]";

			var expected = (object)null;

			var source = new JsonReader().Query(input, new { key=String.Empty });

			var query = source.Where(obj => obj.key == "not-a-key");

			var actual = query.FirstOrDefault();

			Assert.Equal(expected, actual, false);
		}

		#endregion Anonymous Tests

		#region Complex Graph Tests



		#endregion Complex Graph Tests
	}
}
