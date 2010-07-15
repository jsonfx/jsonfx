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

namespace JsonFx.Xml
{
	public class XmlFormatterTests
	{
		#region Array Tests

		[Fact]
		public void Format_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayEnd
			};

			const string expected = @"<Array></Array>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayEmptyPrettyPrint_ReturnsPrettyPrintedEmptyArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayEnd
			};

			const string expected = @"<Array></Array>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd
			};

			const string expected = @"<Array><Object /></Array>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayOneItemPrettyPrint_ReturnsExpectedPrettyPrintedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenNull,
				CommonGrammar.TokenArrayEnd
			};

			const string expected =
@"<Array>
	<Object />
</Array>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayMultiItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenValue(0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenArrayEnd
			};

			// TODO: update when primitives are correctly converting
			const string expected = @"<Array><Int32>0</Int32><Object /><Boolean>False</Boolean><Boolean>True</Boolean></Array>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayMultiItemPrettyPrint_ReturnsExpectedPrettyPrintedArray()
		{
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenValue(0),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenNull,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenFalse,
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenTrue,
				CommonGrammar.TokenArrayEnd
			};

			// TODO: update when primitives are correctly converting
			const string expected =
@"<Array>
	<Int32>0</Int32>
	<Object />
	<Boolean>False</Boolean>
	<Boolean>True</Boolean>
</Array>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayNestedDeeply_ReturnsExpectedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenValue("Not too deep"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd
			};

			const string expected = @"<Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><Array><String>Not too deep</String></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array></Array>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ArrayNestedDeeplyPrettyPrint_ReturnsExpectedPrettyPrintedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenArrayBegin(),
				CommonGrammar.TokenValue("Not too deep"),
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd,
				CommonGrammar.TokenArrayEnd
			};

			const string expected =
@"<Array>
	<Array>
		<Array>
			<Array>
				<Array>
					<Array>
						<Array>
							<Array>
								<Array>
									<Array>
										<Array>
											<Array>
												<Array>
													<Array>
														<Array>
															<Array>
																<Array>
																	<Array>
																		<Array>
																			<String>Not too deep</String>
																		</Array>
																	</Array>
																</Array>
															</Array>
														</Array>
													</Array>
												</Array>
											</Array>
										</Array>
									</Array>
								</Array>
							</Array>
						</Array>
					</Array>
				</Array>
			</Array>
		</Array>
	</Array>
</Array>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint=true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		public void Format_ObjectEmpty_RendersEmptyObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin(),
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"<Object></Object>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectEmptyPrettyPrint_RendersPrettyPrintedEmptyObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin(),
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"<Object></Object>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint = true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectOneProperty_RendersSimpleObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin(),
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"<Object><key>value</key></Object>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectOnePropertyPrettyPrint_RendersPrettyPrintedSimpleObject()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin("Yada"),
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			const string expected =
@"<Yada>
	<key>value</key>
</Yada>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint = true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectNested_RendersNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin(),
				CommonGrammar.TokenProperty("JSON Test Pattern pass3"),
				CommonGrammar.TokenObjectBegin(),
				CommonGrammar.TokenProperty("The outermost value"),
				CommonGrammar.TokenValue("must be an object or array."),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("In this test"),
				CommonGrammar.TokenValue("It is an object."),
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectEnd
			};

			// TODO: encode DataName values
			const string expected = @"<Object><JSON Test Pattern pass3><The outermost value>must be an object or array.</The outermost value><In this test>It is an object.</In this test></JSON Test Pattern pass3></Object>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_ObjectNestedPrettyPrint_RendersPrettyPrintedNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin("Root"),
				CommonGrammar.TokenProperty("JSON Test Pattern pass3"),
				CommonGrammar.TokenObjectBegin(),
				CommonGrammar.TokenProperty("The outermost value"),
				CommonGrammar.TokenValue("must be an object or array."),
				CommonGrammar.TokenValueDelim,
				CommonGrammar.TokenProperty("In this test"),
				CommonGrammar.TokenValue("It is an object."),
				CommonGrammar.TokenObjectEnd,
				CommonGrammar.TokenObjectEnd
			};

			// TODO: encode DataName values
			const string expected =
@"<Root>
	<JSON Test Pattern pass3>
		<The outermost value>must be an object or array.</The outermost value>
		<In this test>It is an object.</In this test>
	</JSON Test Pattern pass3>
</Root>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint = true });
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region Namespace Tests

		[Fact(Skip="XML Namespaces are not yet implemented")]
		public void Format_NamespacedObjectOneProperty_CorrectlyIgnoresNamespace()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin("foo"),
				CommonGrammar.TokenProperty("key", "http://json.org"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"<foo><a:key xmlns:a=""http://json.org"">value</a:key></foo>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact(Skip="XML Namespaces are not yet implemented")]
		public void Format_ObjectOneNamespacedProperty_CorrectlyIgnoresNamespace()
		{
			var input = new[]
			{
				CommonGrammar.TokenObjectBegin("foo", "http://json.org"),
				CommonGrammar.TokenProperty("key"),
				CommonGrammar.TokenValue("value"),
				CommonGrammar.TokenObjectEnd
			};

			const string expected = @"<a:foo xmlns:a=""http://json.org""><key>value</key></a:foo>";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Input Edge Case Tests

		[Fact]
		public void Format_EmptyInput_RendersEmptyString()
		{
			var input = Enumerable.Empty<Token<CommonTokenType>>();

			const string expected = "";

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());
			var actual = formatter.Format(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Format_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<CommonTokenType>>)null;

			var formatter = new XmlWriter.XmlFormatter(new DataWriterSettings());

			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var actual = formatter.Format(input);
				});

			// verify exception is coming from expected param
			Assert.Equal("tokens", ex.ParamName);
		}

		[Fact]
		public void Ctor_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var formatter = new XmlWriter.XmlFormatter(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
