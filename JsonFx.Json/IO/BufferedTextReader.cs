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
	internal class BufferedTextReader : PeekTextReader
	{
		#region Constants

		public new static readonly BufferedTextReader Null = new BufferedTextReader(TextReader.Null, 1);
		private const int DefaultBufferSize = 1024;
		private const int MinBufferSize = 128;

		#endregion Constants

		#region Fields

		private readonly TextReader Reader;
		private char[] buffer;
		private int start;
		private int count;
		private long position;
		private bool isDisposed;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="reader"></param>
		public BufferedTextReader(TextReader reader)
			: this(reader, DefaultBufferSize)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="bufferSize"></param>
		public BufferedTextReader(TextReader reader, int bufferSize)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			if (bufferSize < 1)
			{
				throw new ArgumentOutOfRangeException("bufferSize", "bufferSize must be a positive number.");
			}
#if !DEBUG
			if (bufferSize < BufferedTextReader.MinBufferSize)
			{
				bufferSize = BufferedTextReader.MinBufferSize;
			}
#endif

			this.Reader = reader;
			this.buffer = new char[bufferSize];
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the total number of characters read from the input
		/// </summary>
		public override long Position
		{
			get { return this.position; }
		}

		#endregion Properties

		#region TextReader Methods

		/// <summary>
		/// Reads the next character without advancing the input position.
		/// </summary>
		public override int Peek()
		{
			return this.Peek(0);
		}

		/// <summary>
		/// Reads the next character without advancing the input position.
		/// </summary>
		/// <param name="index">the position to look ahead</param>
		/// <returns>the next character to be read or -1 if no more characters are available</returns>
		public override int Peek(int index)
		{
			this.EnsureBuffer(index+1);
			if (this.count < index+1)
			{
				return -1;
			}

			return this.buffer[this.start+index];
		}

		/// <summary>
		/// Reads the next count characters without advancing the input position.
		/// </summary>
		/// <param name="count">the number of characters to read</param>
		/// <param name="value">the resulting string</param>
		/// <returns>the number of characters read or -1 if not enough characters are available</returns>
		public override int Peek(int count, out string value)
		{
			this.EnsureBuffer(count);
			if (this.count < 1)
			{
				value = null;
				return -1;
			}

			if (this.count < count)
			{
				count = this.count;
			}

			if (count > 0)
			{
				value = new String(this.buffer, this.start, count);
			}
			else
			{
				value = String.Empty;
			}
			return value.Length;
		}

		/// <summary>
		/// Fills the buffer with the next character without advancing the input position.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns>the number of characters read or -1 if not enough characters are available</returns>
		public override int Peek(char[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			return this.Peek(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Fills the buffer with the next character without advancing the input position.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns>the number of characters read or -1 if not enough characters are available</returns>
		public override int Peek(char[] buffer, int index, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if ((buffer.Length - index) < count)
			{
				throw new ArgumentException("Invalid buffer offset or length");
			}

			this.EnsureBuffer(count);
			if (this.count < 1)
			{
				return -1;
			}

			int copyCount = Math.Min(this.count, count);

			Array.Copy(this.buffer, this.start, buffer, index, copyCount);

			return copyCount;
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
		/// Reads characters from the input and writes the data to buffer.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public override int Read(char[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			return this.Read(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Reads a maximum of count characters from the input and writes the data to buffer, beginning at index.
		/// </summary>
		/// <param name="destBuffer"></param>
		/// <param name="destIndex"></param>
		/// <param name="destCount"></param>
		/// <returns></returns>
		public override int Read(char[] buffer, int index, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if ((buffer.Length - index) < count)
			{
				throw new ArgumentException("Invalid buffer offset or length");
			}

			int total = 0;
			while (count > 0)
			{
				// fill buffer
				this.EnsureBuffer(Math.Min(count, this.buffer.Length));
				if (this.count < 1)
				{
					// end of input
					break;
				}

				// determine how many are left
				int max = Math.Min(count, this.count);

				// copy into user buffer
				Array.Copy(this.buffer, this.start, buffer, index, max);

				// adjust read counts
				total += max;
				index += max;
				count -= max;

				// adjust buffer counts
				this.position += max;
				this.start += max;
				this.count -= max;
			}
			return total;
		}

		/// <summary>
		/// Reads a line of characters from the input and returns the data as a string.
		/// </summary>
		/// <returns></returns>
		public override string ReadLine()
		{
			// fill buffer, also check disposed state
			this.EnsureBuffer();

			if (this.count < 1)
			{
				return null;
			}

			StringBuilder builder = new StringBuilder(this.buffer.Length);
			while (this.count > 0)
			{
				for (int i=this.start; i<this.count; i++)
				{
					// check each character for line ending
					char ch = this.buffer[i];
					switch (ch)
					{
						case '\r':
						case '\n':
						{
							// append final segment
							int lineLength = i-this.start;
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
				builder.Append(this.buffer, this.start, this.count);
				this.position += this.count;
				this.start += this.count;
				this.count = 0;

				// refill buffer
				this.EnsureBuffer();
			}

			return builder.ToString();
		}

		/// <summary>
		/// Releases all resources used by the BufferedTextReader object.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (this.isDisposed)
			{
				return;
			}

			this.buffer = null;
			this.count = 0;
			this.start = 0;
			this.position = 0L;
			this.isDisposed = true;

			this.Reader.Dispose();

			base.Dispose(disposing);
		}

		#endregion TextReader Methods

		#region Buffer Methods

		/// <summary>
		/// Advances the character position by 1 characters and peeks the next character.
		/// </summary>
		/// <returns>the next character to be read or -1 if no more characters are available</returns>
		public override int NextPeek()
		{
			this.EnsureBuffer(2);
			if (this.count < 1)
			{
				throw new ArgumentOutOfRangeException("count", "Attempted to flush beyond end of input.");
			}

			this.FlushInternal(1);

			if (this.count < 1)
			{
				return -1;
			}

			return this.buffer[this.start];
		}

		/// <summary>
		/// Advances the character position by count characters.
		/// </summary>
		/// <param name="count"></param>
		public override void Flush(int count)
		{
			this.EnsureFlush(count);
			this.FlushInternal(count);
		}

		/// <summary>
		/// Advances the character position by count characters.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="builder"></param>
		public override void Flush(int count, StringBuilder builder)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}

			this.EnsureFlush(count);

			// append flushed value
			builder.Append(this.buffer, this.start, count);

			this.FlushInternal(count);
		}

		/// <summary>
		/// Advances the character position by count characters.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="builder"></param>
		public override void Flush(int count, out string value)
		{
			this.EnsureBuffer(count);
			if (this.count < count)
			{
				throw new ArgumentOutOfRangeException("count", "Attempted to flush beyond end of input.");
			}

			// extract flushed value as string
			value = new String(this.buffer, this.start, count);

			this.FlushInternal(count);
		}

		private void EnsureFlush(int count)
		{
			this.EnsureBuffer(count);
			if (this.count < count)
			{
				throw new ArgumentOutOfRangeException("count", "Attempted to flush beyond end of input.");
			}
		}

		private void FlushInternal(int count)
		{
			this.position += count;
			this.start += count;
			this.count -= count;
		}

		/// <summary>
		/// Ensures that buffer is not disposed and fully populated
		/// </summary>
		private void EnsureBuffer()
		{
			this.EnsureBuffer(this.isDisposed ? 0 : this.buffer.Length);
		}

		/// <summary>
		/// Ensures that buffer is not disposed, large enough for destCount and fully populated
		/// </summary>
		/// <param name="destCount"></param>
		private void EnsureBuffer(int bufferSize)
		{
			if (this.isDisposed)
			{
				throw new ObjectDisposedException("Underlying TextReader has been disposed.");
			}

			if (bufferSize < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", "bufferSize cannot be negative");
			}

			if (bufferSize <= this.count)
			{
				// already contains enough data
				return;
			} 

			if (bufferSize > this.buffer.Length)
			{
				// allocate a larger buffer
				char[] temp = new char[bufferSize];
				if (this.count > 0)
				{
					// shift and copy any existing chars
					Array.Copy(this.buffer, this.start, temp, 0, this.count);
				}
				this.buffer = temp;
			}
			else if (this.start > 0 && this.count > 0)
			{
				// shift buffer to zero aligned
				Array.Copy(this.buffer, this.start, this.buffer, 0, this.count);
			}

			// start is always zero here
			this.start = 0;

			// populate the unused portion of the buffer
			this.count += this.Reader.Read(this.buffer, this.count, this.buffer.Length-this.count);
		}

		#endregion Buffer Methods
	}
}