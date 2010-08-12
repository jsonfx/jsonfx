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
using System.IO;
using System.Text;

namespace JsonFx.Serialization
{
	/// <summary>
	/// A common interface for data serializers
	/// </summary>
	public interface IDataWriter
	{
		#region Properties

		/// <summary>
		/// Gets the content encoding for the serialized data
		/// </summary>
		Encoding ContentEncoding
		{
			get;
		}

		/// <summary>
		/// Gets the supported content types for the serialized data
		/// </summary>
		IEnumerable<string> ContentType
		{
			get;
		}

		/// <summary>
		/// Gets the supported file extensions for the serialized data
		/// </summary>
		IEnumerable<string> FileExtension
		{
			get;
		}

		/// <summary>
		/// Gets the settings used for serialization
		/// </summary>
		DataWriterSettings Settings
		{
			get;
		}

		#endregion Properties

		#region Write Methods

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="output">the output writer</param>
		/// <param name="data">the data to be serialized</param>
		void Write(object data, TextWriter output);

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="data">the data to be serialized</param>
		/// <returns>the serialized data</returns>
		string Write(object data);

		#endregion Write Methods
	}
}
