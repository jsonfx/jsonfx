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

using JsonFx.Common;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Common
{
	public class CommonSubsequencerTests
	{
		#region Constants

		private const string TraitName = "LINQ";
		private const string TraitValue = "CommonSubsequencer";

		#endregion Constants

		#region IsArray Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_ArrayEmpty_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			var expected = true;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_ObjectEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			var expected = false;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Hello.")
			};

			var expected = false;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			var expected = false;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsArray_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			var expected = false;
			var actual = input.IsArray();

			Assert.Equal(expected, actual);
		}

		#endregion IsArray Tests

		#region IsObject Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_ObjectEmpty_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			var expected = true;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_ArrayEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			var expected = false;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_FalsePrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			var expected = false;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_StringPrimitive_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Hello.")
			};

			var expected = false;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsObject_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			var expected = false;
			var actual = input.IsObject();

			Assert.Equal(expected, actual);
		}

		#endregion IsObject Tests

		#region IsPrimitive Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_ObjectEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBeginNoName,
				CommonGrammar.TokenObjectEnd
			};

			var expected = false;
			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_ArrayEmpty_ReturnsFalse()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBeginNoName,
				CommonGrammar.TokenArrayEnd
			};

			var expected = false;

			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_FalsePrimitive_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenFalse
			};

			var expected = true;
			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_StringPrimitive_ReturnsTrue()
		{
			var input = new[]
			{
				CommonGrammar.TokenValue("Hello.")
			};

			var expected = true;
			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsPrimitive_EmptySequence_ReturnsFalse()
		{
			var input = new Token<CommonTokenType>[0];

			var expected = false;
			var actual = input.IsPrimitive();

			Assert.Equal(expected, actual);
		}

		#endregion IsPrimitive Tests

		//#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_MixedPrimitivesFilterAll_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new IEnumerable<Token<CommonTokenType>>[0];

			// select all items with a non-null value
			var actual = input.GetArrayItems(item => false).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_MixedPrimitivesFilterNone_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenFalse
				},
				new[]
				{
					CommonGrammar.TokenTrue
				},
				new[]
				{
					CommonGrammar.TokenNull
				},
				new[]
				{
					CommonGrammar.TokenValue("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenNull
				},
				new[]
				{
					CommonGrammar.TokenValue(42)
				},
			};

			// select all items with a non-null value
			var actual = input.GetArrayItems(item => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_MixedPrimitivesFilterNonNull_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenFalse
				},
				new[]
				{
					CommonGrammar.TokenTrue
				},
				new[]
				{
					CommonGrammar.TokenValue("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenValue(42)
				},
			};

			// select all items with a non-null value
			var actual = input.GetArrayItems(item => item.All(token => token.Value != null)).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_NestedArrayFilterNonNull_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenTrue,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenFalse
				},
				new[]
				{
					CommonGrammar.TokenValue("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenArrayBeginNoName,
					CommonGrammar.TokenFalse,
					CommonGrammar.TokenNull,
					CommonGrammar.TokenTrue,
					CommonGrammar.TokenValue("Hello!"),
					CommonGrammar.TokenValue(42),
					CommonGrammar.TokenArrayEnd,
				},
				new[]
				{
					CommonGrammar.TokenValue(42)
				},
			};

			// select all items with a non-null value
			var actual = input.GetArrayItems(item => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetArrayItems_NestedObjectFilterNonNull_ReturnsSplitSequences()
		{
			var input = new[]
		    {
		        CommonGrammar.TokenArrayBeginNoName,
		        CommonGrammar.TokenFalse,
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenObjectBeginNoName,
		        CommonGrammar.TokenProperty("key1"),
		        CommonGrammar.TokenNull,
		        CommonGrammar.TokenProperty("key2"),
		        CommonGrammar.TokenValue("Hello!"),
		        CommonGrammar.TokenObjectEnd,
		        CommonGrammar.TokenValue(42),
		        CommonGrammar.TokenArrayEnd
		    };

			var expected = new[]
			{
				new[]
				{
					CommonGrammar.TokenFalse
				},
				new[]
				{
					CommonGrammar.TokenValue("Hello!")
				},
				new[]
				{
					CommonGrammar.TokenObjectBeginNoName,
					CommonGrammar.TokenProperty("key1"),
					CommonGrammar.TokenNull,
					CommonGrammar.TokenProperty("key2"),
					CommonGrammar.TokenValue("Hello!"),
					CommonGrammar.TokenObjectEnd,
				},
				new[]
				{
					CommonGrammar.TokenValue(42)
				},
			};

			// select all items with a non-null value
			var actual = input.GetArrayItems(item => true).ToArray();

			Assert.Equal(expected, actual, false);
		}

		////[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Parse_ArrayMultiItem_ReturnsExpectedArray()
		//{
		//    var input = new[]
		//    {
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenValue(0),
		//        CommonGrammar.TokenNull,
		//        CommonGrammar.TokenFalse,
		//        CommonGrammar.TokenTrue,
		//        CommonGrammar.TokenArrayEnd
		//    };

		//    var expected = new object[]
		//    {
		//        0,
		//        null,
		//        false,
		//        true
		//    };

		//    var query = new Query<object>(new CommonSubsequencer(new CommonAnalyzer(new DataReaderSettings()), input));
		//    var actual = query.Cast<object[]>().Single();

		//    Assert.Equal(expected, actual);
		//}

		////[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Parse_ArrayNestedDeeply_ReturnsExpectedArray()
		//{
		//    // input from pass2.json in test suite at http://www.json.org/JSON_checker/
		//    var input = new[]
		//    {
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenValue("Not too deep"),
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenArrayEnd
		//    };

		//    var expected = new []
		//    {
		//        new []
		//        {
		//            new []
		//            {
		//                new []
		//                {
		//                    new []
		//                    {
		//                        new []
		//                        {
		//                            new []
		//                            {
		//                                new []
		//                                {
		//                                    new []
		//                                    {
		//                                        new []
		//                                        {
		//                                            new []
		//                                            {
		//                                                new []
		//                                                {
		//                                                    new []
		//                                                    {
		//                                                        new []
		//                                                        {
		//                                                            new []
		//                                                            {
		//                                                                new []
		//                                                                {
		//                                                                    new []
		//                                                                    {
		//                                                                        new []
		//                                                                        {
		//                                                                            new []
		//                                                                            {
		//                                                                                "Not too deep"
		//                                                                            }
		//                                                                        }
		//                                                                    }
		//                                                                }
		//                                                            }
		//                                                        }
		//                                                    }
		//                                                }
		//                                            }
		//                                        }
		//                                    }
		//                                }
		//                            }
		//                        }
		//                    }
		//                }
		//            }
		//        }
		//    };

		//    var query = new Query<object>(new CommonSubsequencer(new CommonAnalyzer(new DataReaderSettings()), input));
		//    var actual = query.Cast<string[][][][][][][][][][][][][][][][][][][]>().Single();

		//    Assert.Equal(expected, actual);
		//}

		//#endregion Array Tests

		//#region Object Tests

		////[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Parse_ObjectEmpty_ReturnsEmptyObject()
		//{
		//    var input = new[]
		//    {
		//        CommonGrammar.TokenObjectBeginNoName,
		//        CommonGrammar.TokenObjectEnd
		//    };

		//    var expected = new Dictionary<string, object>();

		//    var query = new Query<object>(new CommonSubsequencer(new CommonAnalyzer(new DataReaderSettings()), input));
		//    var actual = query.Cast<IDictionary<string, object>>().Single();

		//    Assert.Equal(expected, actual, false);
		//}

		//[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Parse_ObjectOneProperty_ReturnsSimpleObject()
		//{
		//    var input = new[]
		//    {
		//        CommonGrammar.TokenObjectBeginNoName,
		//        CommonGrammar.TokenProperty("key"),
		//        CommonGrammar.TokenValue("value"),
		//        CommonGrammar.TokenObjectEnd
		//    };

		//    var expected = new
		//        {
		//            key = "value"
		//        };

		//    var query = Query.Find(input, new { key=String.Empty })
		//        .Where(obj => obj.key == "value");

		//    var actual = query.First();

		//    Assert.Equal(expected, actual, false);
		//}

		////[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Parse_ObjectNested_ReturnsNestedObject()
		//{
		//    // input from pass3.json in test suite at http://www.json.org/JSON_checker/
		//    var input = new[]
		//    {
		//        CommonGrammar.TokenObjectBeginNoName,
		//        CommonGrammar.TokenProperty("JSON Test Pattern pass3"),
		//        CommonGrammar.TokenObjectBeginNoName,
		//        CommonGrammar.TokenProperty("The outermost value"),
		//        CommonGrammar.TokenValue("must be an object or array."),
		//        CommonGrammar.TokenProperty("In this test"),
		//        CommonGrammar.TokenValue("It is an object."),
		//        CommonGrammar.TokenObjectEnd,
		//        CommonGrammar.TokenObjectEnd
		//    };

		//    var expected = new Dictionary<string, object>
		//        {
		//            {
		//                "JSON Test Pattern pass3",
		//                new Dictionary<string, object>
		//                {
		//                    { "The outermost value", "must be an object or array." },
		//                    { "In this test", "It is an object." }
		//                }
		//            }
		//        };

		//    var query = new Query<object>(new CommonSubsequencer(new CommonAnalyzer(new DataReaderSettings()), input));
		//    var actual = query.Cast<IDictionary<string, object>>().Single();

		//    Assert.Equal(expected, actual, false);
		//}

		//#endregion Object Tests

		//#region Complex Graph Tests

		////[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Parse_GraphComplex_ReturnsGraph()
		//{
		//    // input from pass1.json in test suite at http://www.json.org/JSON_checker/
		//    var input = new[]
		//    {
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenValue("JSON Test Pattern pass1"),
		//        CommonGrammar.TokenObjectBeginNoName,
		//        CommonGrammar.TokenProperty("object with 1 member"),
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenValue("array with 1 element"),
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenObjectEnd,
		//        CommonGrammar.TokenObjectBeginNoName,
		//        CommonGrammar.TokenObjectEnd,
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenValue(-42),
		//        CommonGrammar.TokenTrue,
		//        CommonGrammar.TokenFalse,
		//        CommonGrammar.TokenNull,
		//        CommonGrammar.TokenObjectBeginNoName,
		//        CommonGrammar.TokenProperty("integer"),
		//        CommonGrammar.TokenValue(1234567890),
		//        CommonGrammar.TokenProperty("real"),
		//        CommonGrammar.TokenValue(-9876.543210),
		//        CommonGrammar.TokenProperty("e"),
		//        CommonGrammar.TokenValue(0.123456789e-12),
		//        CommonGrammar.TokenProperty("E"),
		//        CommonGrammar.TokenValue(1.234567890E+34),
		//        CommonGrammar.TokenProperty(""),
		//        CommonGrammar.TokenValue(23456789012E66),
		//        CommonGrammar.TokenProperty("zero"),
		//        CommonGrammar.TokenValue(0),
		//        CommonGrammar.TokenProperty("one"),
		//        CommonGrammar.TokenValue(1),
		//        CommonGrammar.TokenProperty("space"),
		//        CommonGrammar.TokenValue(" "),
		//        CommonGrammar.TokenProperty("quote"),
		//        CommonGrammar.TokenValue("\""),
		//        CommonGrammar.TokenProperty("backslash"),
		//        CommonGrammar.TokenValue("\\"),
		//        CommonGrammar.TokenProperty("controls"),
		//        CommonGrammar.TokenValue("\b\f\n\r\t"),
		//        CommonGrammar.TokenProperty("slash"),
		//        CommonGrammar.TokenValue("/ & /"),
		//        CommonGrammar.TokenProperty("alpha"),
		//        CommonGrammar.TokenValue("abcdefghijklmnopqrstuvwyz"),
		//        CommonGrammar.TokenProperty("ALPHA"),
		//        CommonGrammar.TokenValue("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
		//        CommonGrammar.TokenProperty("digit"),
		//        CommonGrammar.TokenValue("0123456789"),
		//        CommonGrammar.TokenProperty("0123456789"),
		//        CommonGrammar.TokenValue("digit"),
		//        CommonGrammar.TokenProperty("special"),
		//        CommonGrammar.TokenValue("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
		//        CommonGrammar.TokenProperty("hex"),
		//        CommonGrammar.TokenValue("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
		//        CommonGrammar.TokenProperty("true"),
		//        CommonGrammar.TokenTrue,
		//        CommonGrammar.TokenProperty("false"),
		//        CommonGrammar.TokenFalse,
		//        CommonGrammar.TokenProperty("null"),
		//        CommonGrammar.TokenNull,
		//        CommonGrammar.TokenProperty("array"),
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenProperty("object"),
		//        CommonGrammar.TokenObjectBeginNoName,
		//        CommonGrammar.TokenObjectEnd,
		//        CommonGrammar.TokenProperty("address"),
		//        CommonGrammar.TokenValue("50 St. James Street"),
		//        CommonGrammar.TokenProperty("url"),
		//        CommonGrammar.TokenValue("http://www.JSON.org/"),
		//        CommonGrammar.TokenProperty("comment"),
		//        CommonGrammar.TokenValue("// /* <!-- --"),
		//        CommonGrammar.TokenProperty("# -- --> */"),
		//        CommonGrammar.TokenValue(" "),
		//        CommonGrammar.TokenProperty(" s p a c e d "),
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenValue(1),
		//        CommonGrammar.TokenValue(2),
		//        CommonGrammar.TokenValue(3),
		//        CommonGrammar.TokenValue(4),
		//        CommonGrammar.TokenValue(5),
		//        CommonGrammar.TokenValue(6),
		//        CommonGrammar.TokenValue(7),
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenProperty("compact"),
		//        CommonGrammar.TokenArrayBeginNoName,
		//        CommonGrammar.TokenValue(1),
		//        CommonGrammar.TokenValue(2),
		//        CommonGrammar.TokenValue(3),
		//        CommonGrammar.TokenValue(4),
		//        CommonGrammar.TokenValue(5),
		//        CommonGrammar.TokenValue(6),
		//        CommonGrammar.TokenValue(7),
		//        CommonGrammar.TokenArrayEnd,
		//        CommonGrammar.TokenProperty("jsontext"),
		//        CommonGrammar.TokenValue("{\"object with 1 member\":[\"array with 1 element\"]}"),
		//        CommonGrammar.TokenProperty("quotes"),
		//        CommonGrammar.TokenValue("&#34; \u0022 %22 0x22 034 &#x22;"),
		//        CommonGrammar.TokenProperty("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
		//        CommonGrammar.TokenValue("A key can be any string"),
		//        CommonGrammar.TokenObjectEnd,
		//        CommonGrammar.TokenValue(0.5),
		//        CommonGrammar.TokenValue(98.6),
		//        CommonGrammar.TokenValue(99.44),
		//        CommonGrammar.TokenValue(1066),
		//        CommonGrammar.TokenValue(10.0),
		//        CommonGrammar.TokenValue(1.0),
		//        CommonGrammar.TokenValue(0.1),
		//        CommonGrammar.TokenValue(1.0),
		//        CommonGrammar.TokenValue(2.0),
		//        CommonGrammar.TokenValue(2.0),
		//        CommonGrammar.TokenValue("rosebud"),
		//        CommonGrammar.TokenArrayEnd
		//    };

		//    var expected = new object[] {
		//        "JSON Test Pattern pass1",
		//        new Dictionary<string, object>
		//        {
		//            { "object with 1 member", new[] { "array with 1 element" } },
		//        },
		//        new Dictionary<string, object>(),
		//        new object[0],
		//        -42,
		//        true,
		//        false,
		//        null,
		//        new Dictionary<string, object> {
		//            { "integer", 1234567890 },
		//            { "real", -9876.543210 },
		//            { "e", 0.123456789e-12 },
		//            { "E", 1.234567890E+34 },
		//            { "", 23456789012E66 },
		//            { "zero", 0 },
		//            { "one", 1 },
		//            { "space", " " },
		//            { "quote", "\"" },
		//            { "backslash", "\\" },
		//            { "controls", "\b\f\n\r\t" },
		//            { "slash", "/ & /" },
		//            { "alpha", "abcdefghijklmnopqrstuvwyz" },
		//            { "ALPHA", "ABCDEFGHIJKLMNOPQRSTUVWYZ" },
		//            { "digit", "0123456789" },
		//            { "0123456789", "digit" },
		//            { "special", "`1~!@#$%^&*()_+-={':[,]}|;.</>?" },
		//            { "hex", "\u0123\u4567\u89AB\uCDEF\uabcd\uef4A" },
		//            { "true", true },
		//            { "false", false },
		//            { "null", null },
		//            { "array", new object[0] },
		//            { "object", new Dictionary<string, object>() },
		//            { "address", "50 St. James Street" },
		//            { "url", "http://www.JSON.org/" },
		//            { "comment", "// /* <!-- --" },
		//            { "# -- --> */", " " },
		//            { " s p a c e d ", new [] { 1,2,3,4,5,6,7 } },
		//            { "compact", new [] { 1,2,3,4,5,6,7 } },
		//            { "jsontext", "{\"object with 1 member\":[\"array with 1 element\"]}" },
		//            { "quotes", "&#34; \u0022 %22 0x22 034 &#x22;" },
		//            { "/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?", "A key can be any string" }
		//        },
		//        0.5,
		//        98.6,
		//        99.44,
		//        1066,
		//        1e1,
		//        0.1e1,
		//        1e-1,
		//        1e00,
		//        2e+00,
		//        2e-00,
		//        "rosebud"
		//    };

		//    var query = new Query<object>(new CommonSubsequencer(new CommonAnalyzer(new DataReaderSettings()), input));
		//    var actual = query.Cast<object[]>().Single();

		//    Assert.Equal(expected, actual, false);
		//}

		//#endregion Complex Graph Tests

		//#region Input Edge Case Tests

		////[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Parse_EmptyInput_ReturnsNothing()
		//{
		//    var input = Enumerable.Empty<Token<CommonTokenType>>();

		//    var query = new Query<object>(new CommonSubsequencer(new CommonAnalyzer(new DataReaderSettings()), input));
		//    Assert.False(query.Any());
		//}

		////[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Parse_NullInput_ThrowsArgumentNullException()
		//{
		//    var input = (IEnumerable<Token<CommonTokenType>>)null;

		//    ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
		//        delegate
		//        {
		//            var query = new Query<object>(new CommonSubsequencer(new CommonAnalyzer(new DataReaderSettings()), input));
		//        });

		//    // verify exception is coming from expected param
		//    Assert.Equal("sequence", ex.ParamName);
		//}

		////[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Parse_NullAnalyzer_ThrowsArgumentNullException()
		//{
		//    var input = new[]
		//        {
		//            CommonGrammar.TokenArrayBeginNoName,
		//            CommonGrammar.TokenArrayEnd
		//        };

		//    ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
		//        delegate
		//        {
		//            var query = new Query<object>(new CommonSubsequencer(null, input));
		//        });

		//    // verify exception is coming from expected param
		//    Assert.Equal("analyzer", ex.ParamName);
		//}

		////[Fact]
		//[Trait(TraitName, TraitValue)]
		//public void Ctor_NullSettings_ThrowsArgumentNullException()
		//{
		//    ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
		//        delegate
		//        {
		//            var analyzer = new CommonAnalyzer(null);
		//        });

		//    // verify exception is coming from expected param
		//    Assert.Equal("settings", ex.ParamName);
		//}

		//#endregion Input Edge Case Tests
	}
}
