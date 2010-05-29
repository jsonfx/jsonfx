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

namespace JsonFx.IO
{
	/// <summary>
	/// PeekReader implementation which holds entire string in memory for faster performance.
	/// </summary>
	internal class StringPeekReader : PeekReader
	{
		#region Constants

		public new static readonly StringPeekReader Null = new StringPeekReader(String.Empty);

		#endregion Constants

		#region Fields

		private readonly string Buffer;
		private readonly int Length;
		private int position;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="value"></param>
		public StringPeekReader(string value)
		{
			this.Buffer = value ?? String.Empty;
			this.Length = this.Buffer.Length;
		}

		#endregion Init

		#region Properties

		public override long Position
		{
			get { return this.position; }
		}

		#endregion Properties

		#region PeekReader Methods

		public override int Peek(int index)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", "index cannot be negative");
			}

			if (this.position + index >= this.Length)
			{
				return -1;
			}

			return this.Buffer[this.position + index];
		}

		public override int Peek(int count, out string value)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "count cannot be negative");
			}

			count = Math.Min(count, this.Length-this.position);

			value = this.Buffer.Substring(this.position, count);
			return count;
		}

		public override int Peek(char[] buffer, int index, int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "count cannot be negative");
			}

			count = Math.Min(count, this.Length-this.position);
			this.Buffer.CopyTo(this.position, buffer, index, count);

			return count;
		}

		/// <summary>
		/// Reads the next character from the input and advances the character position by one character.
		/// </summary>
		/// <returns>the next character to be read or -1 if no more characters are available</returns>
		public override int Read()
		{
			int ch = this.Peek();

			this.FlushInternal(1);

			return ch;
		}

		/// <summary>
		/// Reads a maximum of count characters from the input and writes the data to buffer, beginning at index.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public override int Read(char[] buffer, int index, int count)
		{
			count = this.Peek(buffer, index, count);

			this.FlushInternal(count);

			return count;
		}

		/// <summary>
		/// Reads a line of characters from the input and returns the data as a string.
		/// </summary>
		/// <returns></returns>
		public override string ReadLine()
		{
			if (this.Length - this.position < 1)
			{
				return null;
			}

			StringBuilder builder = new StringBuilder();

			for (int i=this.position; i<this.Length; i++)
			{
				// check each character for line ending
				char ch = this.Buffer[i];
				switch (ch)
				{
					case '\r':
					case '\n':
					{
						// append final segment
						int lineLength = i-this.position;
						this.Flush(lineLength, builder);

						if ((ch == '\r') && (this.Peek() == '\n'))
						{
							// treat CRLF as single char
							this.FlushInternal(2);
						}
						else
						{
							// consume line ending
							this.FlushInternal(1);
						}
						return builder.ToString();
					}
				}
			}

			// append buffered segment and flush
			builder.Append(this.Buffer, this.position, this.Length);
			this.FlushInternal(this.Length);

			return builder.ToString();
		}

		public override void Flush(int count)
		{
			this.EnsureFlush(count);

			this.FlushInternal(count);
		}

		public override void Flush(int count, StringBuilder builder)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}

			this.EnsureFlush(count);

			builder.Append(this.Buffer, this.position, count);

			this.FlushInternal(count);
		}

		public override void Flush(int count, out string value)
		{
			this.EnsureFlush(count);

			value = this.Buffer.Substring(this.position, count);

			this.FlushInternal(count);
		}

		public override int FlushPeek()
		{
			if (this.Length - this.position < 1)
			{
				throw new ArgumentOutOfRangeException("count", "Attempted to flush beyond end of input.");
			}

			this.FlushInternal(1);

			if (this.Length - this.position < 1)
			{
				return -1;
			}

			return this.Buffer[this.position];
		}

		#endregion PeekReader Methods

		#region Methods

		private void EnsureFlush(int count)
		{
			if (this.Length < this.position + count)
			{
				throw new ArgumentOutOfRangeException("count", "Attempted to flush beyond end of input");
			}
		}

		private void FlushInternal(int count)
		{
			this.position += count;
		}

		#endregion Methods
	}
}
