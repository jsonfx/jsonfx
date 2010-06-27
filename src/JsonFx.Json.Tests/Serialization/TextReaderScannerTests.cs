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

			Assert.Equal(scanner.Current, '\0');
		}

		[Fact]
		public void Index_MoveNextNotCalled_ReturnsNegOne()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			Assert.Equal(scanner.Index, -1);
		}

		[Fact]
		public void Line_MoveNextNotCalled_ReturnsNegOne()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			Assert.Equal(scanner.Line, 0);
		}

		[Fact]
		public void Column_MoveNextNotCalled_ReturnsNegOne()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			Assert.Equal(scanner.Column, 0);
		}

		[Fact]
		public void IsEnd_MoveNextNotCalled_ReturnsFalse()
		{
			var scanner = new TextReaderScanner(TextReader.Null);

			Assert.Equal(scanner.IsEnd, false);
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
			const string value = "";

			var scanner = new TextReaderScanner(new StringReader(value));

			var buffer = new StringBuilder();
			while (scanner.MoveNext())
			{
				buffer.Append(scanner.Current);
			}

			Assert.Equal(value, buffer.ToString());
		}

		[Fact]
		public void MoveNext_OneCharSequence_ProducesSameSequence()
		{
			const string value = "_";

			var scanner = new TextReaderScanner(new StringReader(value));

			var buffer = new StringBuilder();
			while (scanner.MoveNext())
			{
				buffer.Append(scanner.Current);
			}

			Assert.Equal(value, buffer.ToString());
		}

		[Fact]
		public void MoveNext_LongSequence_ProducesSameSequence()
		{
			const string value = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var scanner = new TextReaderScanner(new StringReader(value));

			var buffer = new StringBuilder();
			while (scanner.MoveNext())
			{
				buffer.Append(scanner.Current);
			}

			Assert.Equal(value, buffer.ToString());
		}

		[Fact]
		public void MoveNext_UnicodeSequence_ProducesSameSequence()
		{
			const string value = "私が日本語を話すことはありません。";

			var scanner = new TextReaderScanner(new StringReader(value));

			var buffer = new StringBuilder();
			while (scanner.MoveNext())
			{
				buffer.Append(scanner.Current);
			}

			Assert.Equal(value, buffer.ToString());
		}

		#endregion MoveNext Tests

		#region IsEnd Tests

		[Fact]
		public void IsEnd_LongString_ReturnsFalseUntilEnd()
		{
			const string value = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var scanner = new TextReaderScanner(new StringReader(value));

			while (scanner.MoveNext())
			{
				Assert.Equal(scanner.IsEnd, false);
			}

			Assert.Equal(scanner.IsEnd, true);
		}

		#endregion IsEnd Tests
	}
}
