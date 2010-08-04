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
using System.Linq;

using JsonFx.Serialization;

namespace JsonFx.Linq
{
	/// <summary>
	/// A common interface for querying data readers
	/// </summary>
	public interface IQueryableReader : IDataReader
	{
		#region Query Methods

		/// <summary>
		/// Begins a query of the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <param name="ignored">a value used to trigger Type inference for <typeparamref name="TResult"/> (e.g. for deserializing anonymous objects)</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		IQueryable<TResult> Query<TResult>(TextReader input, TResult ignored);

		/// <summary>
		/// Begins a query of the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		IQueryable<TResult> Query<TResult>(TextReader input);

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="input">the input reader</param>
		IQueryable Query(TextReader input);

		/// <summary>
		/// Begins a query of the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		IQueryable Query(TextReader input, Type targetType);

		/// <summary>
		/// Begins a query of the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <param name="ignored">a value used to trigger Type inference for <typeparamref name="TResult"/> (e.g. for deserializing anonymous objects)</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		IQueryable<TResult> Query<TResult>(string input, TResult ignored);

		/// <summary>
		/// Begins a query of the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		IQueryable<TResult> Query<TResult>(string input);

		/// <summary>
		/// Begins a query of the given input
		/// </summary>
		/// <param name="input">the input text</param>
		IQueryable Query(string input);

		/// <summary>
		/// Begins a query of the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		IQueryable Query(string input, Type targetType);

		#endregion Query Methods
	}
}
