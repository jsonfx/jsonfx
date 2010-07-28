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

namespace JsonFx.IO
{
	/// <summary>
	/// Supports forward-only iteration over an input sequence
	/// </summary>
	public interface IStream<T> : IDisposable
	{
		#region Properties

		/// <summary>
		/// Determines if the sequence has completed.
		/// </summary>
		bool IsCompleted
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating if is currently capturing a sequence
		/// </summary>
		bool IsChunking
		{
			get;
		}

		/// <summary>
		/// Gets the number of items currently chunked
		/// </summary>
		int ChunkSize
		{
			get;
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Returns but does not remove the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		T Peek();

		/// <summary>
		/// Returns and removes the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		T Pop();

		/// <summary>
		/// Begins chunking at the current index
		/// </summary>
		void BeginChunk();

		/// <summary>
		/// Ends chunking at the current index and returns the buffered sequence chunk
		/// </summary>
		/// <returns></returns>
		IEnumerable<T> EndChunk();

		#endregion Methods
	}
}
