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
	/// Supports forward-only iteration over an input sequence of <typeparamref name="T"/>
	/// </summary>
	public abstract class Stream<T> : IStream<T>
	{
		#region Constants

		public static readonly Stream<T> Null = new ListStream<T>(null);

		#endregion Constants

		#region Factory Method

		public static IStream<T> Create(IEnumerable<T> sequence)
		{
			return Stream<T>.Create(sequence, false);
		}

		/// <summary>
		/// Factory method for generic streams
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="buffered"></param>
		/// <returns></returns>
		public static IStream<T> Create(IEnumerable<T> sequence, bool buffered)
		{
			IList<T> list = sequence as IList<T>;
			if (list != null)
			{
				return new ListStream<T>(list);
			}
			else if (buffered)
			{
				list = new SequenceBuffer<T>(sequence);
				return new ListStream<T>(list);
			}

			return new EnumerableStream<T>(sequence);
		}

		#endregion Factory Method

		#region IStream<T> Properties

		/// <summary>
		/// Determines if the input sequence has reached the end
		/// </summary>
		public abstract bool IsCompleted
		{
			get;
		}

		#endregion IStream<T> Properties

		#region IStream<T> Methods

		/// <summary>
		/// Returns but does not remove the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public abstract T Peek();

		/// <summary>
		/// Returns and removes the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public abstract T Pop();

		#endregion IStream<T> Methods

		#region Chunking Members

		public abstract bool IsChunking
		{
			get;
		}

		public abstract int ChunkSize
		{
			get;
		}

		public abstract void BeginChunk();

		public abstract IEnumerable<T> EndChunk();

		#endregion Chunking Members

		#region IDisposable Members

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected abstract void Dispose(bool disposing);

		#endregion IDisposable Members
	}
}
