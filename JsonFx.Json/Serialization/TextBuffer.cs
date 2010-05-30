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

namespace JsonFx.Serialization
{
	/// <summary>
	/// Lighter implementation of StringBuilder
	/// </summary>
	internal class TextBuffer
	{
		#region Constants

		private const int DefaultBufferSize = 0x10;

		#endregion Constants

		#region Fields

		private char[] buffer = new char[DefaultBufferSize];
		private int capacity = DefaultBufferSize;
		private int length;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public TextBuffer()
			: this(DefaultBufferSize)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public TextBuffer(int bufferSize)
		{
			if (bufferSize < 1)
			{
				throw new ArgumentOutOfRangeException("bufferSize");
			}

			this.buffer = new char[bufferSize];
			this.capacity = bufferSize;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the length of the currently allocated string
		/// </summary>
		public int Length
		{
			get { return this.length; }
		}

		/// <summary>
		/// Gets the character at the specified index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public char this[int index]
		{
			get { return this.buffer[index]; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Appends the specified character to the buffer
		/// </summary>
		/// <param name="ch"></param>
		public void Append(char value)
		{
			this.EnsureCapacity(this.length+1);

			this.buffer[this.length] = value;
			this.length++;
		}

		/// <summary>
		/// Appends the specified character to the buffer
		/// </summary>
		/// <param name="ch"></param>
		public void Append(string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return;
			}

			this.EnsureCapacity(this.length+value.Length);

			value.CopyTo(0, this.buffer, this.length, value.Length);
			this.length += value.Length;
		}

		/// <summary>
		/// Resets the internal state keeping the internal buffer
		/// </summary>
		public void Clear()
		{
			this.length = 0;
		}

		/// <summary>
		/// Resets the internal state allocating a new buffer
		/// </summary>
		public void Clear(int capacity)
		{
			capacity = Math.Max(capacity, TextBuffer.DefaultBufferSize);

			this.length = 0;
			this.capacity = capacity;
			this.buffer = new char[capacity];
		}

		/// <summary>
		/// Ensures that enough capacity is allocated.
		/// </summary>
		/// <param name="size"></param>
		public void EnsureCapacity(int size)
		{
			if (this.capacity >= size)
			{
				return;
			}

			size = Math.Max(capacity * 2, size);

			char[] temp = new char[size];

			if (this.length > 0)
			{
				Buffer.BlockCopy(this.buffer, 0, temp, 0, this.length);
			}

			this.buffer = temp;
		}

		/// <summary>
		/// Gets the string represented by the buffer
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (this.length < 1)
			{
				return String.Empty;
			}

			return new String(this.buffer, 0, this.length);
		}

		#endregion Methods
	}
}
