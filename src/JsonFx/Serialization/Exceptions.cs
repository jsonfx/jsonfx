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
	/// Indicates an error occurred during serialization
	/// </summary>
	public class SerializationException : InvalidOperationException
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public SerializationException() : base() { }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		public SerializationException(string message) : base(message) { }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public SerializationException(string message, Exception innerException) : base(message, innerException) { }

#if !SILVERLIGHT
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public SerializationException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
		#endregion Init
	}

	/// <summary>
	/// Indicates an error occurred during deserialization
	/// </summary>
	public class DeserializationException : SerializationException
	{
		#region Fields

		private readonly int column = -1;
		private readonly int line = -1;
		private readonly long index = -1L;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="index"></param>
		public DeserializationException(string message, long index)
			: this(message, index, -1, -1)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="index"></param>
		/// <param name="line"></param>
		/// <param name="column"></param>
		public DeserializationException(string message, long index, int line, int column)
			: base(message)
		{
			this.column = column;
			this.line = line;
			this.index = index;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="index"></param>
		/// <param name="innerException"></param>
		public DeserializationException(string message, long index, Exception innerException)
			: this(message, index, -1, -1, innerException)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="index"></param>
		/// <param name="innerException"></param>
		public DeserializationException(string message, long index, int line, int column, Exception innerException)
			: base(message, innerException)
		{
			this.column = column;
			this.line = line;
			this.index = index;
		}

#if !SILVERLIGHT
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public DeserializationException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the character column in the stream where the error occurred
		/// </summary>
		public int Column
		{
			get { return this.column; }
		}

		/// <summary>
		/// Gets the character position in the stream where the error occurred
		/// </summary>
		public long Index
		{
			get { return this.index; }
		}

		/// <summary>
		/// Gets the character line in the stream where the error occurred
		/// </summary>
		public int Line
		{
			get { return this.line; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Helper method which converts the index into Line and Column numbers
		/// </summary>
		/// <param name="source"></param>
		/// <param name="line"></param>
		/// <param name="col"></param>
		public void GetLineAndColumn(string source, out int line, out int col)
		{
			if (source == null)
			{
				throw new ArgumentNullException();
			}

			col = 1;
			line = 1;

			bool foundLF = false;
			int i = Math.Min((int)this.index, source.Length);
			for (; i>0; i--)
			{
				if (!foundLF)
				{
					col++;
				}
				if (source[i-1] == '\n')
				{
					line++;
					foundLF = true;
				}
			}
		}

		#endregion Methods
	}

	/// <summary>
	/// Indicates an error occurred during token consumption
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TokenException<T> : SerializationException
	{
		#region Fields

		private readonly Token<T> token;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public TokenException(Token<T> token)
			: base()
		{
			this.token = token;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		public TokenException(Token<T> token, string message)
			: base(message)
		{
			this.token = token;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public TokenException(Token<T> token, string message, Exception innerException)
			: base(message, innerException)
		{
			this.token = token;
		}

#if !SILVERLIGHT
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public TokenException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the token in the sequence where the error occurred
		/// </summary>
		public Token<T> Token
		{
			get { return this.token; }
		}

		#endregion Properties
	}

	/// <summary>
	/// Indicates an error occurred during type coercion
	/// </summary>
	public class TypeCoercionException : ArgumentException
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public TypeCoercionException() : base() { }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		public TypeCoercionException(string message) : base(message) { }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public TypeCoercionException(string message, Exception innerException) : base(message, innerException) { }

#if !SILVERLIGHT
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public TypeCoercionException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
		#endregion Init
	}
}
