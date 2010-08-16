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
using System.Dynamic;
using System.Linq;

using JsonFx.Model;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization
{
	public class ModelAnalyzerTests
	{
		#region Constants

		private const string TraitName = "Common Model";
		private const string TraitValue = "Analyzer";

		#endregion Constants

		#region Test Types

		public class Person
		{
			public string Name { get; set; }
			public Person Father { get; set; }
			public Person Mother { get; set; }
			public Person[] Children { get; set; }
		}

		public class DynamicExample : DynamicObject
		{
			internal readonly Dictionary<string, object> Values = new Dictionary<string, object>();

			public override IEnumerable<string> GetDynamicMemberNames()
			{
				return Values.Keys;
			}

			public override bool TryDeleteMember(DeleteMemberBinder binder)
			{
				return this.Values.Remove(binder.Name);
			}

			public override bool TryGetMember(GetMemberBinder binder, out object result)
			{
				return this.Values.TryGetValue(binder.Name, out result);
			}

			public override bool TrySetMember(SetMemberBinder binder, object value)
			{
				this.Values[binder.Name] = value;
				return true;
			}
		}

		#endregion Test Types

		#region Array Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new []
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd
			};

			var expected = new object[0];

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenArrayEnd
			};

			var expected = new object[] { null };

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ArrayMultiItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(0),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenArrayEnd
			};

			var expected = new object[]
			{
				0,
				null,
				false,
				true
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ArrayNestedDeeply_ReturnsExpectedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("Not too deep"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenArrayEnd
			};

			var expected = new []
			{
				new []
				{
					new []
					{
						new []
						{
							new []
							{
								new []
								{
									new []
									{
										new []
										{
											new []
											{
												new []
												{
													new []
													{
														new []
														{
															new []
															{
																new []
																{
																	new []
																	{
																		new []
																		{
																			new []
																			{
																				new []
																				{
																					new []
																					{
																						"Not too deep"
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze((input)).Cast<string[][][][][][][][][][][][][][][][][][][]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ArrayUnclosed_ThrowsAnalyzerException()
		{
			// input from fail2.json in test suite at http://www.json.org/JSON_checker/
			var input = new []
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("Unclosed array")
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());

			TokenException<ModelTokenType> ex = Assert.Throws<TokenException<ModelTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(ModelGrammar.TokenNone, ex.Token);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_PropertyInsideArray_ThrowsAnalyzerException()
		{
			// input from fail22.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenProperty("Colon instead of comma"),
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenArrayEnd
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());

			TokenException<ModelTokenType> ex = Assert.Throws<TokenException<ModelTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(ModelGrammar.TokenProperty("Colon instead of comma"), ex.Token);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ArrayCloseMismatch_ThrowsAnalyzerException()
		{
			// input from fail33.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("mismatch"),
				ModelGrammar.TokenObjectEnd
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());

			TokenException<ModelTokenType> ex = Assert.Throws<TokenException<ModelTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(ModelGrammar.TokenObjectEnd, ex.Token);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ObjectEmpty_ReturnsEmptyObject()
		{
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>();

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ObjectOneProperty_ReturnsSimpleObject()
		{
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("key"),
				ModelGrammar.TokenPrimitive("value"),
				ModelGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>
				{
					{ "key", "value" }
				};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ObjectNested_ReturnsNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("JSON Test Pattern pass3"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("The outermost value"),
				ModelGrammar.TokenPrimitive("must be an object or array."),
				ModelGrammar.TokenProperty("In this test"),
				ModelGrammar.TokenPrimitive("It is an object."),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>
				{
					{
						"JSON Test Pattern pass3",
						new Dictionary<string, object>
						{
							{ "The outermost value", "must be an object or array." },
							{ "In this test", "It is an object." }
						}
					}
				};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ObjectMissingColon_ThrowsAnalyzerException()
		{
			// input from fail19.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenPrimitive("Missing colon"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenObjectEnd
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());

			TokenException<ModelTokenType> ex = Assert.Throws<TokenException<ModelTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(ModelGrammar.TokenPrimitive("Missing colon"), ex.Token);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ObjectDoubleColon_ThrowsAnalyzerException()
		{
			// input from fail20.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("Double colon"),
				ModelGrammar.TokenProperty(""),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenObjectEnd
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());

			TokenException<ModelTokenType> ex = Assert.Throws<TokenException<ModelTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(ModelTokenType.Property, ex.Token.TokenType);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ValueInsteadOfProperty_ThrowsAnalyzerException()
		{
			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenPrimitive("Comma instead of colon"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenObjectEnd
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());

			TokenException<ModelTokenType> ex = Assert.Throws<TokenException<ModelTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(ModelGrammar.TokenPrimitive("Comma instead of colon"), ex.Token);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_ObjectUnterminated_ThrowsAnalyzerException()
		{
			// input from fail32.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("Comma instead if closing brace"),
				ModelGrammar.TokenTrue
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());

			TokenException<ModelTokenType> ex = Assert.Throws<TokenException<ModelTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(ModelGrammar.TokenNone, ex.Token);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_AnonymousObject_ReturnsAnonymousObject()
		{
			// NOTE: order is important to ensure type equivalence

			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("AString"),
				ModelGrammar.TokenPrimitive("Hello world!"),
				ModelGrammar.TokenProperty("AnInt32"),
				ModelGrammar.TokenPrimitive(42),
				ModelGrammar.TokenProperty("AnAnonymous"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("AnotherString"),
				ModelGrammar.TokenPrimitive("Foo."),
				ModelGrammar.TokenProperty("AnInt64"),
				ModelGrammar.TokenPrimitive( ((long)Int32.MaxValue) * 2L ),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenProperty("ADouble"),
				ModelGrammar.TokenPrimitive(Math.PI),
				ModelGrammar.TokenObjectEnd
			};

			var expected = new
			{
				AString = "Hello world!",
				AnInt32 = 42,
				AnAnonymous = new
				{
					AnotherString = "Foo.",
					AnInt64 = ((long)Int32.MaxValue) * 2L
				},
				ADouble = Math.PI
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input, expected).Single();

			Assert.Equal(expected, actual, false);
		}

		#endregion Object Tests

		#region Dynamic Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_DynamicExample_ReturnsDynamicObject()
		{
			var input = new[]
			{
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("foo"),
				ModelGrammar.TokenPrimitive("hello world"),
				ModelGrammar.TokenProperty("number"),
				ModelGrammar.TokenPrimitive(42),
				ModelGrammar.TokenProperty("boolean"),
				ModelGrammar.TokenPrimitive(false),
				ModelGrammar.TokenProperty("null"),
				ModelGrammar.TokenPrimitive(null),
				ModelGrammar.TokenObjectEnd
			};

			dynamic expected = new DynamicExample();
			expected.foo = "hello world";
			expected.number = 42;
			expected.boolean = false;
			expected.@null = null;

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze<DynamicExample>(input).Single();

			Assert.Equal(expected.Values, actual.Values, false);
		}

		#endregion Dynamic Tests

		#region Complex Graph Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_GraphComplex_ReturnsGraph()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("JSON Test Pattern pass1"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("object with 1 member"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive("array with 1 element"),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenPrimitive(-42),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenNull,
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenProperty("integer"),
				ModelGrammar.TokenPrimitive(1234567890),
				ModelGrammar.TokenProperty("real"),
				ModelGrammar.TokenPrimitive(-9876.543210),
				ModelGrammar.TokenProperty("e"),
				ModelGrammar.TokenPrimitive(0.123456789e-12),
				ModelGrammar.TokenProperty("E"),
				ModelGrammar.TokenPrimitive(1.234567890E+34),
				ModelGrammar.TokenProperty(""),
				ModelGrammar.TokenPrimitive(23456789012E66),
				ModelGrammar.TokenProperty("zero"),
				ModelGrammar.TokenPrimitive(0),
				ModelGrammar.TokenProperty("one"),
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenProperty("space"),
				ModelGrammar.TokenPrimitive(" "),
				ModelGrammar.TokenProperty("quote"),
				ModelGrammar.TokenPrimitive("\""),
				ModelGrammar.TokenProperty("backslash"),
				ModelGrammar.TokenPrimitive("\\"),
				ModelGrammar.TokenProperty("controls"),
				ModelGrammar.TokenPrimitive("\b\f\n\r\t"),
				ModelGrammar.TokenProperty("slash"),
				ModelGrammar.TokenPrimitive("/ & /"),
				ModelGrammar.TokenProperty("alpha"),
				ModelGrammar.TokenPrimitive("abcdefghijklmnopqrstuvwyz"),
				ModelGrammar.TokenProperty("ALPHA"),
				ModelGrammar.TokenPrimitive("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				ModelGrammar.TokenProperty("digit"),
				ModelGrammar.TokenPrimitive("0123456789"),
				ModelGrammar.TokenProperty("0123456789"),
				ModelGrammar.TokenPrimitive("digit"),
				ModelGrammar.TokenProperty("special"),
				ModelGrammar.TokenPrimitive("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				ModelGrammar.TokenProperty("hex"),
				ModelGrammar.TokenPrimitive("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				ModelGrammar.TokenProperty("true"),
				ModelGrammar.TokenTrue,
				ModelGrammar.TokenProperty("false"),
				ModelGrammar.TokenFalse,
				ModelGrammar.TokenProperty("null"),
				ModelGrammar.TokenNull,
				ModelGrammar.TokenProperty("array"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("object"),
				ModelGrammar.TokenObjectBeginUnnamed,
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenProperty("address"),
				ModelGrammar.TokenPrimitive("50 St. James Street"),
				ModelGrammar.TokenProperty("url"),
				ModelGrammar.TokenPrimitive("http://www.JSON.org/"),
				ModelGrammar.TokenProperty("comment"),
				ModelGrammar.TokenPrimitive("// /* <!-- --"),
				ModelGrammar.TokenProperty("# -- --> */"),
				ModelGrammar.TokenPrimitive(" "),
				ModelGrammar.TokenProperty(" s p a c e d "),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenPrimitive(2),
				ModelGrammar.TokenPrimitive(3),
				ModelGrammar.TokenPrimitive(4),
				ModelGrammar.TokenPrimitive(5),
				ModelGrammar.TokenPrimitive(6),
				ModelGrammar.TokenPrimitive(7),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("compact"),
				ModelGrammar.TokenArrayBeginUnnamed,
				ModelGrammar.TokenPrimitive(1),
				ModelGrammar.TokenPrimitive(2),
				ModelGrammar.TokenPrimitive(3),
				ModelGrammar.TokenPrimitive(4),
				ModelGrammar.TokenPrimitive(5),
				ModelGrammar.TokenPrimitive(6),
				ModelGrammar.TokenPrimitive(7),
				ModelGrammar.TokenArrayEnd,
				ModelGrammar.TokenProperty("jsontext"),
				ModelGrammar.TokenPrimitive("{\"object with 1 member\":[\"array with 1 element\"]}"),
				ModelGrammar.TokenProperty("quotes"),
				ModelGrammar.TokenPrimitive("&#34; \u0022 %22 0x22 034 &#x22;"),
				ModelGrammar.TokenProperty("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				ModelGrammar.TokenPrimitive("A key can be any string"),
				ModelGrammar.TokenObjectEnd,
				ModelGrammar.TokenPrimitive(0.5),
				ModelGrammar.TokenPrimitive(98.6),
				ModelGrammar.TokenPrimitive(99.44),
				ModelGrammar.TokenPrimitive(1066),
				ModelGrammar.TokenPrimitive(10.0),
				ModelGrammar.TokenPrimitive(1.0),
				ModelGrammar.TokenPrimitive(0.1),
				ModelGrammar.TokenPrimitive(1.0),
				ModelGrammar.TokenPrimitive(2.0),
				ModelGrammar.TokenPrimitive(2.0),
				ModelGrammar.TokenPrimitive("rosebud"),
				ModelGrammar.TokenArrayEnd
			};

			var expected = new object[] {
				"JSON Test Pattern pass1",
				new Dictionary<string, object>
				{
					{ "object with 1 member", new[] { "array with 1 element" } },
				},
				new Dictionary<string, object>(),
				new object[0],
				-42,
				true,
				false,
				null,
				new Dictionary<string, object> {
					{ "integer", 1234567890 },
					{ "real", -9876.543210 },
					{ "e", 0.123456789e-12 },
					{ "E", 1.234567890E+34 },
					{ "", 23456789012E66 },
					{ "zero", 0 },
					{ "one", 1 },
					{ "space", " " },
					{ "quote", "\"" },
					{ "backslash", "\\" },
					{ "controls", "\b\f\n\r\t" },
					{ "slash", "/ & /" },
					{ "alpha", "abcdefghijklmnopqrstuvwyz" },
					{ "ALPHA", "ABCDEFGHIJKLMNOPQRSTUVWYZ" },
					{ "digit", "0123456789" },
					{ "0123456789", "digit" },
					{ "special", "`1~!@#$%^&*()_+-={':[,]}|;.</>?" },
					{ "hex", "\u0123\u4567\u89AB\uCDEF\uabcd\uef4A" },
					{ "true", true },
					{ "false", false },
					{ "null", null },
					{ "array", new object[0] },
					{ "object", new Dictionary<string, object>() },
					{ "address", "50 St. James Street" },
					{ "url", "http://www.JSON.org/" },
					{ "comment", "// /* <!-- --" },
					{ "# -- --> */", " " },
					{ " s p a c e d ", new [] { 1,2,3,4,5,6,7 } },
					{ "compact", new [] { 1,2,3,4,5,6,7 } },
					{ "jsontext", "{\"object with 1 member\":[\"array with 1 element\"]}" },
					{ "quotes", "&#34; \u0022 %22 0x22 034 &#x22;" },
					{ "/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?", "A key can be any string" }
				},
				0.5,
				98.6,
				99.44,
				1066,
				1e1,
				0.1e1,
				1e-1,
				1e00,
				2e+00,
				2e-00,
				"rosebud"
			};

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual, false);
		}

		#endregion Complex Graph Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_EmptyInput_ReturnsNothing()
		{
			var input = Enumerable.Empty<Token<ModelTokenType>>();

			var analyzer = new ModelAnalyzer(new DataReaderSettings());
			Assert.False(analyzer.Analyze<object>(input).Any());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Analyze_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<ModelTokenType>>)null;

			var analyzer = new ModelAnalyzer(new DataReaderSettings());

			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected param
			Assert.Equal("tokens", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Ctor_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var analyzer = new ModelAnalyzer(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
