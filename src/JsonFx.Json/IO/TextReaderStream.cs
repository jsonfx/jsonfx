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

namespace JsonFx.IO
{
	/// <summary>
	/// Supports a simple iteration over a TextReader tracking line/column/position
	/// </summary>
	public class TextReaderStream : ITextStream
	{
		#region Constants

		public static readonly TextReaderStream Null = new TextReaderStream(TextReader.Null);

		#endregion Constants

		#region Fields

		private readonly TextReader Reader;
		private char current;
		private char prev;
		private bool isCompleted;
		private bool isReady;
		private int column;
		private int line;
		private long index;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="reader"></param>
		public TextReaderStream(TextReader reader)
		{
			this.Reader = reader;
			this.index = -1L;
		}

		#endregion Fields

		#region ITextStream Members

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

		#endregion ITextStream Members

		#region IStream<T> Properties

		/// <summary>
		/// Determines if the input sequence has reached the end
		/// </summary>
		public virtual bool IsCompleted
		{
			get
			{
				this.EnsureReady();

				return this.isCompleted;
			}
		}

		#endregion IStream<T> Properties

		#region IStream<T> Methods

		/// <summary>
		/// Returns but does not remove the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public virtual char Peek()
		{
			this.EnsureReady();

			// return the current item or null if complete
			return this.current;
		}

		/// <summary>
		/// Returns and removes the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public virtual char Pop()
		{
			this.EnsureReady();

			if (!this.isCompleted)
			{
				// pop the previously peeked cahr
				this.Reader.Read();

				// flag as needing to be iterated, but don't execute yet
				this.isReady = false;
			}

			this.UpdateStats(this.prev, this.current);

			// store for next iteration
			return (this.prev = this.current);
		}

		#endregion IStream<T> Methods

		#region Methods

		/// <summary>
		/// Deferred execution of iterator
		/// </summary>
		private void EnsureReady()
		{
			// only execute when requested
			if (this.isReady)
			{
				return;
			}
			this.isReady = true;

			// lazy execution of reader
			int value = this.Reader.Peek();
			this.isCompleted = (value < 0);

			// store the current item or null if complete
			if (this.isCompleted)
			{
				this.current = default(char);
			}
			else
			{
				this.current = (char)value;
			}
		}

		#endregion Methods

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
