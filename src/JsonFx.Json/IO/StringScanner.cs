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

namespace JsonFx.IO
{
	/// <summary>
	/// Supports a simple iteration over a string tracking line/column/position
	/// </summary>
	public class StringScanner : ITextScanner
	{
		#region Constants

		public static readonly StringScanner Null = new StringScanner(String.Empty);

		#endregion Constants

		#region Fields

		private readonly CharEnumerator enumerator;
		private bool isEnd;
		private char current;
		private int column;
		private int line;
		private long index;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="value"></param>
		public StringScanner(string value)
		{
			this.enumerator = (value ?? String.Empty).GetEnumerator();
			this.index = -1L;
		}

		#endregion Init

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

		#region IDisposable Members

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IDisposable)this.enumerator).Dispose();
			}
		}

		#endregion IDisposable Members

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

			char prev = this.current;

			if (!this.enumerator.MoveNext())
			{
				this.isEnd = true;
				this.current = '\0';
				return false;
			}

			if (this.index < 0)
			{
				this.current = this.enumerator.Current;
				this.line = this.column = 1;
				this.index = 0;
			}
			else
			{
				// check for line endings
				switch (this.current = this.enumerator.Current)
				{
					case '\n':
					{
						if (prev == '\r')
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

			return true;
		}

		/// <summary>
		/// Resets the enumerator to the beginning of the string
		/// </summary>
		public void Reset()
		{
			this.enumerator.Reset();

			this.current = '\0';
			this.isEnd = false;
			this.column = 0;
			this.line = 0;
			this.index = -1L;
		}

		#endregion IEnumerator Members
	}
}
