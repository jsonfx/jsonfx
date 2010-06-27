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
using System.IO;
using System.Text;

using Xunit;

namespace JsonFx.Serialization
{
	public class TextReaderScannerTests
	{
		#region Start State Tests

		[Fact]
		public void Current_MoveNextNotCalled_ReturnsNullChar()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			Assert.Equal('\0', scanner.Current);
		}

		[Fact]
		public void Index_MoveNextNotCalled_ReturnsNegOne()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			Assert.Equal(-1, scanner.Index);
		}

		[Fact]
		public void Line_MoveNextNotCalled_ReturnsNegOne()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			Assert.Equal(0, scanner.Line);
		}

		[Fact]
		public void Column_MoveNextNotCalled_ReturnsNegOne()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			Assert.Equal(0, scanner.Column);
		}

		[Fact]
		public void IsEnd_MoveNextNotCalled_ReturnsFalse()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			Assert.Equal(false, scanner.IsEnd);
		}

		#endregion Start State Tests

		#region MoveNext Tests

		[Fact]
		public void MoveNext_NullReader_ProducesEmptySequence()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			var buffer = new StringBuilder();
			while (scanner.MoveNext())
			{
				buffer.Append(scanner.Current);
			}

			Assert.Equal(String.Empty, buffer.ToString());
		}

		[Fact]
		public void MoveNext_EmptySequence_ProducesEmptySequence()
		{
			const string input = "";

			var scanner = new TextReaderScanner(new StringReader(input));

			var buffer = new StringBuilder();
			while (scanner.MoveNext())
			{
				buffer.Append(scanner.Current);
			}

			Assert.Equal(input, buffer.ToString());
		}

		[Fact]
		public void MoveNext_OneCharSequence_ProducesSameSequence()
		{
			const string input = "_";

			var scanner = new TextReaderScanner(new StringReader(input));

			var buffer = new StringBuilder();
			while (scanner.MoveNext())
			{
				buffer.Append(scanner.Current);
			}

			Assert.Equal(input, buffer.ToString());
		}

		[Fact]
		public void MoveNext_LongSequence_ProducesSameSequence()
		{
			const string input = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var scanner = new TextReaderScanner(new StringReader(input));

			var buffer = new StringBuilder();
			while (scanner.MoveNext())
			{
				buffer.Append(scanner.Current);
			}

			Assert.Equal(input, buffer.ToString());
		}

		[Fact]
		public void MoveNext_UnicodeSequence_ProducesSameSequence()
		{
			const string input = "私が日本語を話すことはありません。";

			var scanner = new TextReaderScanner(new StringReader(input));

			var buffer = new StringBuilder();
			while (scanner.MoveNext())
			{
				buffer.Append(scanner.Current);
			}

			Assert.Equal(input, buffer.ToString());
		}

		#endregion MoveNext Tests

		#region IsEnd Tests

		[Fact]
		public void IsEnd_LongString_ReturnsFalseUntilEnd()
		{
			const string input = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var scanner = new TextReaderScanner(new StringReader(input));

			while (scanner.MoveNext())
			{
				Assert.Equal(false, scanner.IsEnd);
			}

			Assert.Equal(true, scanner.IsEnd);
		}

		#endregion IsEnd Tests

		#region Line, Column, Index Tests

		[Fact]
		public void Line_MultilineString_CountsCorrectNumberOfLines()
		{
			const string input = @"Line one
Line two
Line three
Line Four";

			var scanner = new TextReaderScanner(new StringReader(input));

			while (scanner.MoveNext());

			Assert.Equal(4, scanner.Line);
		}

		[Fact]
		public void Column_MultilineString_CountsCorrectNumberOfColumns()
		{
			const string input = @"Line one
Line two
Line three
Line Four";

			var scanner = new TextReaderScanner(new StringReader(input));

			while (scanner.MoveNext());

			Assert.Equal(9, scanner.Column);
		}

		[Fact]
		public void Index_MultilineString_CountsCorrectNumberOfChars()
		{
			const string input = @"Line one
Line two
Line three
Line Four";

			var scanner = new TextReaderScanner(new StringReader(input));

			long i;
			for (i=0; scanner.MoveNext(); i++)
			{
				Assert.Equal(i, scanner.Index);
			}

			Assert.Equal(i-1, scanner.Index);
		}

		#endregion Line, Column, Index Tests
	}
}
