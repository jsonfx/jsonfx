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
using System.Text;

namespace JsonFx.IO
{
	/// <summary>
	/// Supports a simple iteration over a string tracking line/column/position
	/// </summary>
	public class StringStream : IStream<char>, ITextStream
	{
		#region Constants

		public static readonly StringStream Null = new StringStream(null);

		#endregion Constants

		#region Fields

		private bool isCompleted;
		private bool isReady;
		private char current;

		private int column;
		private int line;
		private int index;

		private readonly string Buffer;
		private int start = -1;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="value"></param>
		public StringStream(string value)
		{
			this.Buffer = value ?? String.Empty;
			this.index = -1;
		}

		#endregion Init

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

		/// <summary>
		/// Gets the number of characters currently chunked
		/// </summary>
		public int ChunkSize
		{
			get
			{
				if (this.start < 0)
				{
					throw new InvalidOperationException("Not currently chunking.");
				}

				return (1+this.index-this.start);
			}
		}

		/// <summary>
		/// Gets a value indicating if the <see cref="StringStream"/> is currently chunking
		/// </summary>
		public bool IsChunking
		{
			get { return (this.start >= 0); }
		}

		/// <summary>
		/// Begins chunking at the current index
		/// </summary>
		public void BeginChunk()
		{
			this.start = this.index+1;
		}

		/// <summary>
		/// Ends chunking at the current index and returns the buffered text chunk
		/// </summary>
		/// <returns></returns>
		public string EndChunk()
		{
			if (this.start < 0)
			{
				throw new InvalidOperationException("Not currently chunking.");
			}

			// build chunk value
			string value = this.Buffer.Substring(this.start, (1+this.index-this.start));

			// reset chunk start
			this.start = -1;

			return value;
		}

		/// <summary>
		/// Ends chunking at the current index and returns the buffered text chunk
		/// </summary>
		/// <returns></returns>
		public void EndChunk(StringBuilder buffer)
		{
			if (this.start < 0)
			{
				throw new InvalidOperationException("Not currently chunking.");
			}

			// append chunk value
			buffer.Append(this.Buffer, this.start, (1+this.index-this.start));

			// reset chunk start
			this.start = -1;
		}

		#endregion ITextStream Members

		#region IStream<char> Members

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

			if (this.isCompleted)
			{
				return this.current;
			}

			// flag as needing to be iterated, but don't execute yet
			this.isReady = false;

			this.UpdateStats();

			return this.current;
		}

		#endregion IStream<char> Members

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

			// store the current item or null if complete
			int next = this.index+1;
			if (next < this.Buffer.Length)
			{
				this.isCompleted = false;
				this.current = this.Buffer[next];
			}
			else
			{
				this.isCompleted = true;
				this.current = default(char);
			}
		}

		/// <summary>
		/// Calculates index, line, and column statistics
		/// </summary>
		private void UpdateStats()
		{
			if (this.index < 0)
			{
				this.line = this.column = 1;
				this.index = 0;
			}
			else
			{
				// check for line endings
				switch (this.current)
				{
					case '\n':
					{
						if (this.Buffer[this.index] == '\r')
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

		#endregion Methods

		#region IDisposable Members

		/// <summary>
		/// Releases all resources used
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		#endregion IDisposable Members
	}
}
