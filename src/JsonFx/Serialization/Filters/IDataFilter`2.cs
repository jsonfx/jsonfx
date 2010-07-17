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
using System.Collections.Generic;

using JsonFx.IO;

namespace JsonFx.Serialization.Filters
{
	/// <summary>
	/// Allows a mechanism for manipulating serialization
	/// </summary>
	/// <typeparam name="TTokenType">Defines the type of token stream this filter understands</typeparam>
	public interface IDataFilter<TTokenType>
	{
		/// <summary>
		/// Allows a class to act as a factory for a type via input Token&lt;T&gt; sequence
		/// </summary>
		/// <param name="tokens">input tokens</param>
		/// <param name="value"></param>
		/// <returns>true if value was generated</returns>
		bool TryRead(DataReaderSettings settings, IStream<Token<TTokenType>> tokens, out object value);

		/// <summary>
		/// Allows a class to serialize a type as Token&lt;T&gt; sequence
		/// </summary>
		/// <param name="value"></param>
		/// <param name="tokens"></param>
		/// <returns>true if value was consumed</returns>
		bool TryWrite(DataWriterSettings settings, object value, out IEnumerable<Token<TTokenType>> tokens);
	}

	/// <summary>
	/// Allows a mechanism for manipulating serialization
	/// </summary>
	/// <typeparam name="TTokenType">Defines the type of token stream this filter understands</typeparam>
	/// <typeparam name="TResult">Defines the type this filter reads/writes</typeparam>
	public interface IDataFilter<TTokenType, TResult> : IDataFilter<TTokenType>
	{
		/// <summary>
		/// Allows a class to act as a factory for a type via input Token&lt;T&gt; sequence
		/// </summary>
		/// <param name="tokens">input tokens</param>
		/// <param name="value"></param>
		/// <returns>true if value was generated</returns>
		bool TryRead(DataReaderSettings settings, IStream<Token<TTokenType>> tokens, out TResult value);

		/// <summary>
		/// Allows a class to serialize a type as Token&lt;T&gt; sequence
		/// </summary>
		/// <param name="value"></param>
		/// <param name="tokens"></param>
		/// <returns>true if value was consumed</returns>
		bool TryWrite(DataWriterSettings settings, TResult value, out IEnumerable<Token<TTokenType>> tokens);
	}
}
