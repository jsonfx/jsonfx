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
using System.IO;

namespace JsonFx.Serialization
{
	/// <summary>
	/// A common interface for data deserializers
	/// </summary>
	public interface IDataReader
	{
		#region Properties

		/// <summary>
		/// Gets the supported content type of the serialized data
		/// </summary>
		IEnumerable<string> ContentType
		{
			get;
		}

		/// <summary>
		/// Gets the settings used for deserialization
		/// </summary>
		DataReaderSettings Settings
		{
			get;
		}

		#endregion Properties

		#region Read Methods

		/// <summary>
		/// Deserializes a single object from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <param name="ignored">a value used to trigger Type inference for <typeparamref name="TResult"/> (e.g. for deserializing anonymous objects)</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		TResult Read<TResult>(TextReader input, TResult ignored);

		/// <summary>
		/// Deserializes a single object from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		TResult Read<TResult>(TextReader input);

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="input">the input reader</param>
		object Read(TextReader input);

		/// <summary>
		/// Deserializes a single object from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		object Read(TextReader input, Type targetType);

		/// <summary>
		/// Deserializes a single object from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <param name="ignored">a value used to trigger Type inference for <typeparamref name="TResult"/> (e.g. for deserializing anonymous objects)</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		TResult Read<TResult>(string input, TResult ignored);

		/// <summary>
		/// Deserializes a single object from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		TResult Read<TResult>(string input);

		/// <summary>
		/// Deserializes a single object from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		object Read(string input);

		/// <summary>
		/// Deserializes a single object from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		object Read(string input, Type targetType);

		#endregion Read Methods

		#region ReadMany Methods

		/// <summary>
		/// Deserializes a potentially endless sequence of objects from a stream source
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		/// <remarks>
		/// character stream => token stream => object stream
		/// </remarks>
		IEnumerable ReadMany(TextReader input);

		#endregion ReadMany Methods
	}
}
