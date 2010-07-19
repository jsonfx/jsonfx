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
	/// StringBuilder-like implementation built like List&lt;char&gt;
	/// </summary>
	internal class CharBuffer
	{
		#region Constants

		private static readonly char[] EmptyBuffer = new char[0];
		private const int DefaultCapacity = 0x20;

		#endregion Constants

		#region Fields

		private char[] buffer;
		private int size;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public CharBuffer()
		{
			this.buffer = CharBuffer.EmptyBuffer;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="capacity"></param>
		public CharBuffer(int capacity)
		{
			this.buffer = new char[capacity];
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the number of characters in the buffer
		/// </summary>
		public int Length
		{
			get { return this.size; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Resets the buffer to an empty state
		/// </summary>
		public void Clear()
		{
			this.size = 0;
		}

		/// <summary>
		/// Appends a single char to the buffer
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public CharBuffer Append(char value)
		{
			if (this.size <= this.buffer.Length)
			{
				this.EnsureCapacity(this.size + 1);
			}

			this.buffer[this.size++] = value;

			return this;
		}

		/// <summary>
		/// Appends a string value to the buffer
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public CharBuffer Append(string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return this;
			}

			int length = value.Length;
			if (this.size <= this.buffer.Length)
			{
				this.EnsureCapacity(this.size + length);
			}

			for (int i=0; i<length; i++)
			{
				this.buffer[this.size++] = value[i];
			}

			return this;
		}

		/// <summary>
		/// Copies the buffer value into a <see cref="StringBuilder"/>
		/// </summary>
		/// <param name="buffer"></param>
		public void CopyTo(StringBuilder buffer)
		{
			if (this.size < 1)
			{
				return;
			}

			buffer.Append(this.buffer, 0, this.size);
		}

		private void EnsureCapacity(int min)
		{
			int length = this.buffer.Length;
			if (length >= min)
			{
				return;
			}

			int capacity = Math.Max(Math.Max(CharBuffer.DefaultCapacity, (length * 2)), min);

			char[] temp = new char[capacity];
			if (this.size > 0)
			{
				Array.Copy(this.buffer, 0, temp, 0, this.size);
			}
			this.buffer = temp;
		}

		#endregion Methods

		#region Object Overrides

		public override string ToString()
		{
			return new String(this.buffer, 0, this.size);
		}

		#endregion Object Overrides
	}
}
