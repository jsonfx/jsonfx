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
	internal abstract class PeekTextReader : TextReader
	{
		#region Properties

		/// <summary>
		/// Gets the total number of characters read from the input
		/// </summary>
		public abstract long Position
		{
			get;
		}

		#endregion Properties

		#region Methods

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
		public abstract int Peek(int index);

		/// <summary>
		/// Reads the next count characters without advancing the input position.
		/// </summary>
		/// <param name="count">the number of characters to read</param>
		/// <param name="value">the resulting string</param>
		/// <returns>the number of characters read or -1 if not enough characters are available</returns>
		public abstract int Peek(int count, out string value);

		/// <summary>
		/// Fills the buffer with the next character without advancing the input position.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns>the number of characters read or -1 if not enough characters are available</returns>
		public virtual int Peek(char[] buffer)
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
		public abstract int Peek(char[] buffer, int index, int count);

		/// <summary>
		/// Reads the next character from the input.
		/// </summary>
		/// <returns></returns>
		public override int Read()
		{
			// force an implementation
			throw new NotImplementedException();
		}

		/// <summary>
		/// Reads characters from the input and writes the data to buffer.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public virtual int Read(char[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			return this.Read(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Advances the character position by count characters.
		/// </summary>
		/// <param name="count"></param>
		public abstract void Flush(int count);

		/// <summary>
		/// Advances the character position by count characters.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="builder"></param>
		public abstract void Flush(int count, StringBuilder builder);

		/// <summary>
		/// Advances the character position by count characters.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="builder"></param>
		public abstract void Flush(int count, out string value);

		/// <summary>
		/// Advances the character position by 1 characters and peeks the next character.
		/// </summary>
		/// <returns>the next character to be read or -1 if no more characters are available</returns>
		public abstract int NextPeek();

		#endregion Methods
	}
}
