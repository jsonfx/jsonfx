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
using System.IO;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Supports a simple iteration over a TextReader tracking line/column/position
	/// </summary>
	public class TextReaderScanner : ITextScanner
	{
		#region Constants

		public static readonly TextReaderScanner Null = new TextReaderScanner(TextReader.Null);

		#endregion Constants

		#region Fields

		private readonly TextReader Reader;
		private char current;
		private bool isInited;
		private bool isEnd;
		private int column;
		private int line;
		private long index;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="reader"></param>
		public TextReaderScanner(TextReader reader)
		{
			this.Reader = reader;
			this.index = -1L;
		}

		#endregion Fields

		#region ITextScanner Members

		/// <summary>
		/// Gets the total number of characters read from the input
		/// </summary>
		public int Column
		{
			get { return this.column; }
		}

		/// <summary>
		/// Gets the total number of lines read from the input
		/// </summary>
		public int Line
		{
			get { return this.line; }
		}

		/// <summary>
		/// Gets the current position within the input
		/// </summary>
		public long Index
		{
			get { return this.index; }
		}

		/// <summary>
		/// Gets if at the end of the input
		/// </summary>
		public bool IsEnd
		{
			get { return this.isEnd; }
		}

		#endregion ITextScanner Members

		#region IEnumerator<char> Members

		/// <summary>
		/// Gets the current character
		/// </summary>
		public char Current
		{
			get { return this.current; }
		}

		#endregion IEnumerator<char> Members

		#region IEnumerator Members

		/// <summary>
		/// Gets the current character
		/// </summary>
		object IEnumerator.Current
		{
			get { return this.Current; }
		}

		/// <summary>
		/// Advances the position of the underlying input
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element;
		/// false if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			if (this.isEnd)
			{
				// no more chars in sequence
				return false;
			}

			if (this.isInited)
			{
				// consume current peek char
				this.Reader.Read();
			}
			else
			{
				// flag as initialized
				this.isInited = true;
			}

			int ch = this.Reader.Peek();
			if (ch < 0)
			{
				this.isEnd = true;
				this.current = '\0';
				return false;
			}

			if (this.index < 0)
			{
				this.line = this.column = 1;
				this.index = 0;
			}
			else
			{
				// check for line endings
				switch (ch)
				{
					case '\n':
					{
						if (this.current == '\r')
						{
							// consider CRLF to be one line ending
							break;
						}
						// fall through
						goto case '\r';
					}
					case '\r':
					{
						this.line++;
						this.column = 0;
						break;
					}
					default:
					{
						this.column++;
						break;
					}
				}
				this.index++;
			}
			this.current = (char)ch;

			return true;
		}

		/// <summary>
		/// Not supported
		/// </summary>
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		#endregion IEnumerator Members

		#region IDisposable Members

		/// <summary>
		/// Releases all resources used by the underlying 
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Reader.Dispose();
			}
		}

		#endregion IDisposable Members
	}
}
