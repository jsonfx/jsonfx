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

using JsonFx.Json;
using JsonFx.Json.Resolvers;
using JsonFx.Serialization;
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

		#region Test Types

		class ComplexType
		{
			#region Properties

			public int integer { get; set; }

			public double real { get; set; }

			public double e { get; set; }

			public double E { get; set; }

			[JsonName("")]
			public double _ { get; set; }

			public int zero { get; set; }

			public int one { get; set; }

			public string space { get; set; }

			public string quote { get; set; }

			public string backslash { get; set; }

			public string controls { get; set; }

			public string slash { get; set; }

			public string alpha { get; set; }

			public string ALPHA { get; set; }

			public string digit { get; set; }

			[JsonName("0123456789")]
			public string _0123456789 { get; set; }

			public string special { get; set; }

			public string hex { get; set; }

			public bool @true { get; set; }

			public bool @false { get; set; }

			public object @null { get; set; }

			public object[] array { get; set; }

			public object @object { get; set; }

			public string address { get; set; }

			public Uri url { get; set; }

			public string comment { get; set; }

			[JsonName("# -- --> */")]
			public string Comments { get; set; }

			[JsonName(" s p a c e d ")]
			public int[] spaced { get; set; }

			public int[] compact { get; set; }

			public string jsontext { get; set; }

			public string quotes { get; set; }

			[JsonName("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?")]
			public string A_key_can_be_any_string { get; set; }

			#endregion Properties

			#region Object Overrides

			public override bool Equals(object obj)
			{
				ComplexType that = obj as ComplexType;
				if (that == null)
				{
					return false;
				}

				return
					(this.integer == that.integer) &&
					(this.real == that.real ) &&
					(this.e == that.e ) &&
					(this.E == that.E ) &&
					(this._ == that._ ) &&
					(this.zero == that.zero ) &&
					(this.one == that.one ) &&
					(this.space == that.space ) &&
					(this.quote == that.quote ) &&
					(this.backslash == that.backslash ) &&
					(this.controls == that.controls ) &&
					(this.slash == that.slash ) &&
					(this.alpha == that.alpha ) &&
					(this.ALPHA == that.ALPHA ) &&
					(this.digit == that.digit ) &&
					(this._0123456789 == that._0123456789 ) &&
					(this.special == that.special ) &&
					(this.hex == that.hex ) &&
					(this.@true == that.@true ) &&
					(this.@false == that.@false ) &&
					(this.@null == that.@null ) &&
					ComplexType.ArraysEqual(this.array, that.array) &&
					ComplexType.DictionaryEqual((IDictionary<string, object>)this.@object, (IDictionary<string, object>)that.@object) &&
					(this.address == that.address ) &&
					(this.url == that.url ) &&
					(this.comment == that.comment ) &&
					(this.Comments == that.Comments ) &&
					ComplexType.ArraysEqual(this.spaced, that.spaced) &&
					ComplexType.ArraysEqual(this.compact, that.compact) &&
					(this.jsontext == that.jsontext ) &&
					(this.quotes == that.quotes ) &&
					(this.A_key_can_be_any_string == that.A_key_can_be_any_string );
			}

			private static bool ArraysEqual(Array x, Array y)
			{
				if (x == null)
				{
					return (y == null);
				}
				else if (y == null)
				{
					return false;
				}

				if (x.Length != y.Length)
				{
					return false;
				}

				for (int i=0, length=x.Length; i<length; i++)
				{
					if (!Object.Equals(x.GetValue(i), y.GetValue(i)))
					{
						return false;
					}
				}

				return true;
			}

			private static bool DictionaryEqual(IDictionary<string, object> x, IDictionary<string, object> y)
			{
				if (x == null)
				{
					return (y == null);
				}
				else if (y == null)
				{
					return false;
				}

				if (x.Count != y.Count)
				{
					return false;
				}

				foreach (var key in x.Keys)
				{
					if (!Object.Equals(x[key], y[key]))
					{
						return false;
					}
				}

				return true;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion Object Overrides
		}

		#endregion Test Types

		#region Anonymous Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void QueryArrayItems_MatchingPropertyToArray_ReturnsSingleObject()
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
				from obj in source.ArrayItems()
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
		public void Query_MatchingPropertyFirstOrDefault_ReturnsSingleObject()
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
		public void QueryArrayItems_MatchingPropertyToArray_ReturnsArray()
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

			var query = source.ArrayItems().Where(obj => obj.key == "value");

			var actual = query.ToArray();

			Assert.Equal(expected, actual, true);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Query_PropertyNoMatchFirstOrDefault_ReturnsNull()
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

		#region Nested Object Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void QueryDescendants_MatchingPropertyToArray_ReturnsArray()
		{
			var input = @"
{
	""name"" : ""Root Object"",
	""matchThis"" : ""find me."",
	""children"" : [
		{
			""name"" : ""Skipped Child"",
			""matchThis"": ""skip me."",
			""children"" : [
				{
					""name"" : ""Skipped Grandchild 1"",
					""matchThis"": ""skip me.""
				},
				{
					""name"" : ""Matching Grandchild 1"",
					""matchThis"": ""find me.""
				}
			]
		},
		{
			""name"" : ""Matching Child"",
			""matchThis"": ""find me."",
			""children"" : [
				{
					""name"" : ""Matching Grandchild 2"",
					""matchThis"": ""find me.""
				},
				{
					""name"" : ""Skipped Grandchild 2"",
					""matchThis"": ""skip me.""
				}
			]
		}
	]
}";

			// returns in document order
			var expected = new[]
				{
					new
					{
						Name = "Matching Grandchild 1"
					},
					new
					{
						Name = "Matching Child"
					},
					new
					{
						Name = "Matching Grandchild 2"
					}
				};

			// define an anonymous object which contains only the fields we want to retrieve
			var template = new { name=String.Empty, matchThis=String.Empty };

			var source = new JsonReader().Query(input, template);

			var query =
				from descendant in source.Descendants()
				where (descendant.matchThis == "find me.")
				select new
				{
					Name = descendant.name
				};

			var actual = query.ToArray();

			Assert.Equal(expected, actual, true);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void QueryDescendantsAndSelf_MatchingPropertyToArray_ReturnsArray()
		{
			var input = @"
{
	""name"" : ""Root Object"",
	""matchThis"" : ""find me."",
	""children"" : [
		{
			""name"" : ""Skipped Child"",
			""matchThis"": ""skip me."",
			""children"" : [
				{
					""name"" : ""Skipped Grandchild 1"",
					""matchThis"": ""skip me.""
				},
				{
					""name"" : ""Matching Grandchild 1"",
					""matchThis"": ""find me.""
				}
			]
		},
		{
			""name"" : ""Matching Child"",
			""matchThis"": ""find me."",
			""children"" : [
				{
					""name"" : ""Matching Grandchild 2"",
					""matchThis"": ""find me.""
				},
				{
					""name"" : ""Skipped Grandchild 2"",
					""matchThis"": ""skip me.""
				}
			]
		}
	]
}";

			// returns in document order
			var expected = new[]
				{
					new
					{
						Name = "Root Object"
					},
					new
					{
						Name = "Matching Grandchild 1"
					},
					new
					{
						Name = "Matching Child"
					},
					new
					{
						Name = "Matching Grandchild 2"
					}
				};

			// define an anonymous object which contains only the fields we want to retrieve
			var template = new { name=String.Empty, matchThis=String.Empty };

			var source = new JsonReader().Query(input, template);

			var query =
				from descendant in source.DescendantsAndSelf()
				where (descendant.matchThis == "find me.")
				select new
				{
					Name = descendant.name
				};

			var actual = query.ToArray();

			Assert.Equal(expected, actual, true);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void QueryDescendantsAndSelf_ExecuteTwice_ReturnsArrayTwice()
		{
			var input = @"
{
	""name"" : ""Root Object"",
	""matchThis"" : ""find me."",
	""children"" : [
		{
			""name"" : ""Skipped Child"",
			""matchThis"": ""skip me."",
			""children"" : [
				{
					""name"" : ""Skipped Grandchild 1"",
					""matchThis"": ""skip me.""
				},
				{
					""name"" : ""Matching Grandchild 1"",
					""matchThis"": ""find me.""
				}
			]
		},
		{
			""name"" : ""Matching Child"",
			""matchThis"": ""find me."",
			""children"" : [
				{
					""name"" : ""Matching Grandchild 2"",
					""matchThis"": ""find me.""
				},
				{
					""name"" : ""Skipped Grandchild 2"",
					""matchThis"": ""skip me.""
				}
			]
		}
	]
}";

			// returns in document order
			var expected = new[]
				{
					new
					{
						Name = "Root Object"
					},
					new
					{
						Name = "Matching Grandchild 1"
					},
					new
					{
						Name = "Matching Child"
					},
					new
					{
						Name = "Matching Grandchild 2"
					}
				};

			// define an anonymous object which contains only the fields we want to retrieve
			var template = new { name=String.Empty, matchThis=String.Empty };

			var source = new JsonReader().Query(input, template);

			var query =
				from descendant in source.DescendantsAndSelf()
				where (descendant.matchThis == "find me.")
				select new
				{
					Name = descendant.name
				};

			var actual = query.ToArray();

			Assert.Equal(expected, actual, true);

			var actual2 = query.ToArray();

			Assert.Equal(expected, actual2, true);
		}

		#endregion Nested Object Tests

		#region Complex Graph Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void QueryDescendants_MatchingPropertyValue_ReturnsStronglyTypedObject()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			const string input = @"[
    ""JSON Test Pattern pass1"",
    {""object with 1 member"":[""array with 1 element""]},
    {},
    [],
    -42,
    true,
    false,
    null,
    {
        ""integer"": 1234567890,
        ""real"": -9876.543210,
        ""e"": 0.123456789e-12,
        ""E"": 1.234567890E+34,
        """":  23456789012E66,
        ""zero"": 0,
        ""one"": 1,
        ""space"": "" "",
        ""quote"": ""\"""",
        ""backslash"": ""\\"",
        ""controls"": ""\b\f\n\r\t"",
        ""slash"": ""/ & \/"",
        ""alpha"": ""abcdefghijklmnopqrstuvwyz"",
        ""ALPHA"": ""ABCDEFGHIJKLMNOPQRSTUVWYZ"",
        ""digit"": ""0123456789"",
        ""0123456789"": ""digit"",
        ""special"": ""`1~!@#$%^&*()_+-={':[,]}|;.</>?"",
        ""hex"": ""\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"",
        ""true"": true,
        ""false"": false,
        ""null"": null,
        ""array"":[  ],
        ""object"":{  },
        ""address"": ""50 St. James Street"",
        ""url"": ""http://www.JSON.org/"",
        ""comment"": ""// /* <!-- --"",
        ""# -- --> */"": "" "",
        "" s p a c e d "" :[1,2 , 3

,

4 , 5        ,          6           ,7        ],""compact"":[1,2,3,4,5,6,7],
        ""jsontext"": ""{\""object with 1 member\"":[\""array with 1 element\""]}"",
        ""quotes"": ""&#34; \u0022 %22 0x22 034 &#x22;"",
        ""\/\\\""\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?""
: ""A key can be any string""
    },
    0.5 ,98.6
,
99.44
,

1066,
1e1,
0.1e1,
1e-1,
1e00,2e+00,2e-00
,""rosebud""]";

			var expected = new ComplexType
			{
				integer = 1234567890,
				real = -9876.543210,
				e = 0.123456789e-12,
				E = 1.234567890E+34,
				_ = 23456789012E66,
				zero = 0,
				one = 1,
				space = " ",
				quote = "\"",
				backslash = "\\",
				controls = "\b\f\n\r\t",
				slash = "/ & /",
				alpha = "abcdefghijklmnopqrstuvwyz",
				ALPHA = "ABCDEFGHIJKLMNOPQRSTUVWYZ",
				digit = "0123456789",
				_0123456789 = "digit",
				special = "`1~!@#$%^&*()_+-={':[,]}|;.</>?",
				hex = "\u0123\u4567\u89AB\uCDEF\uabcd\uef4A",
				@true = true,
				@false = false,
				@null = null,
				array = new object[0],
				@object = new Dictionary<string, object>(),
				address = "50 St. James Street",
				url = new Uri("http://www.JSON.org/"),
				comment = "// /* <!-- --",
				Comments = " ",
				spaced = new [] { 1,2,3,4,5,6,7 },
				compact = new [] { 1,2,3,4,5,6,7 },
				jsontext = "{\"object with 1 member\":[\"array with 1 element\"]}",
				quotes = "&#34; \u0022 %22 0x22 034 &#x22;",
				A_key_can_be_any_string = "A key can be any string"
			};

			var reader = new JsonReader(new DataReaderSettings(new JsonResolverStrategy()));
			var source = reader.Query<ComplexType>(input);

			var query =
				from foo in source.Descendants()
				where foo.url == new Uri("http://www.JSON.org/")
				select foo;

			var actual = query.FirstOrDefault();

			Assert.Equal(expected, actual, false);
		}


		#endregion Complex Graph Tests
	}
}
