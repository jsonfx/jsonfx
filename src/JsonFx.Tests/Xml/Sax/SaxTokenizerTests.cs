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

using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Xml.Sax
{
	public class SaxTokenizerTests
	{
		#region Simple Single Element Tests

		[Fact]
		public void GetTokens_SingleOpenCloseTag_ReturnsSequence()
		{
			const string input = @"<root></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleVoidTag_ReturnsSequence()
		{
			const string input = @"<root />";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Single Element Tests

		#region Simple Attribute Tests

		[Fact]
		public void GetTokens_SingleTagSingleAttribute_ReturnsSequence()
		{
			const string input = @"<root attrName=""attrValue""></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("attrName"), "attrValue"),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeNoValue_ReturnsSequence()
		{
			const string input = @"<root noValue></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("noValue"), String.Empty),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeEmptyValue_ReturnsSequence()
		{
			const string input = @"<root emptyValue=""""></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("emptyValue"), String.Empty),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagWhitespaceAttributeQuotDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  "" extra whitespace around quote delims "" ></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("white"), " extra whitespace around quote delims "),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagWhitespaceAttributeAposDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  ' extra whitespace around apostrophe delims ' ></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("white"), " extra whitespace around apostrophe delims "),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeWhitespace_ReturnsSequence()
		{
			const string input = @"<root whitespace="" this contains whitespace ""></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("whitespace"), " this contains whitespace "),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeSingleQuoted_ReturnsSequence()
		{
			const string input = @"<root singleQuoted='apostrophe'></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("singleQuoted"), "apostrophe"),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeSingleQuotedWhitespace_ReturnsSequence()
		{
			const string input = @"<root singleQuoted_whitespace=' apostrophe with whitespace '></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("singleQuoted_whitespace"), " apostrophe with whitespace "),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagMultipleAttributes_ReturnsSequence()
		{
			const string input = @"<root no-value whitespace="" this contains whitespace "" anyQuotedText=""/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?""></root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root")),
			        SaxGrammar.TokenAttribute(new DataName("no-value"), String.Empty),
			        SaxGrammar.TokenAttribute(new DataName("whitespace"), " this contains whitespace "),
			        SaxGrammar.TokenAttribute(new DataName("anyQuotedText"), @"/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        SaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Attribute Tests

		#region Error Recovery Tests

		[Fact]
		public void GetTokens_UnclosedOpenTag_ReturnsSequence()
		{
			const string input = @"<root>";
			var expected = new[]
			    {
			        SaxGrammar.TokenElementBegin(new DataName("root"))
			    };

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// TODO: determine how this should be handled
			Assert.Equal(expected, actual);
		}

		#endregion Error Recovery Tests

		#region Input Edge Case Tests

		[Fact]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			const string input = null;
			var expected = new Token<SaxTokenType>[0];

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<SaxTokenType>[0];

			var tokenizer = new SaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
