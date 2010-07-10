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
	public class StringStream : Stream<char>, ITextStream
	{
		#region Constants

		public static readonly StringStream Null = new StringStream(null);

		#endregion Constants

		#region Fields

		private int column;
		private int line;
		private long index;
		private char prev;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="value"></param>
		public StringStream(string value)
			: base(value ?? String.Empty)
		{
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

		#endregion ITextScanner Members

		#region IStream<char> Members

		/// <summary>
		/// Returns and removes the character at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public override char Pop()
		{
			char value = base.Pop();

			this.UpdateStats(this.prev, value);

			// store for next iteration
			return (this.prev = value);
		}

		#endregion IStream<char> Members

		#region Statistics Methods

		/// <summary>
		/// Calculates index, line, and column statistics
		/// </summary>
		/// <param name="prev"></param>
		/// <param name="value"></param>
		private void UpdateStats(char prev, char value)
		{
			if (this.index < 0)
			{
				this.line = this.column = 1;
				this.index = 0;
			}
			else
			{
				// check for line endings
				switch (value)
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
		}

		#endregion Statistics Methods
	}
}
