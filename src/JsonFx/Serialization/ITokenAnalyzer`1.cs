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
using System.Collections.Generic;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Consumes a sequence of tokens to produce a sequence of objects, optionally coerced to a given type
	/// </summary>
	/// <typeparam name="T">token type</typeparam>
	public interface ITokenAnalyzer<T>
	{
		#region Properties

		DataReaderSettings Settings
		{
			get;
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Parses the token sequence
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		IEnumerable Analyze(IEnumerable<Token<T>> tokens);

		/// <summary>
		/// Parses the token sequence, optionally coercing the result to Type targetType
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="targetType">optional type for coercion (null if not specified)</param>
		/// <returns></returns>
		IEnumerable Analyze(IEnumerable<Token<T>> tokens, Type targetType);

		/// <summary>
		/// Parses the token sequence, coercing the result to Type TResult
		/// </summary>
		/// <typeparam name="TResult">optional type for coercion (null if not specified)</typeparam>
		/// <param name="tokens"></param>
		/// <returns></returns>
		IEnumerable<TResult> Analyze<TResult>(IEnumerable<Token<T>> tokens);

		/// <summary>
		/// Parses the token stream coercing the result to TResult (type inferred from <paramref name="ignored"/>)
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="tokens"></param>
		/// <param name="ignored">an example value used solely for Type inference</param>
		/// <returns></returns>
		IEnumerable<TResult> Analyze<TResult>(IEnumerable<Token<T>> tokens, TResult ignored);

		#endregion Methods
	}
}
