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
	/// Supports a simple iteration over a list with ability to capture a subsequence
	/// </summary>
	internal class ListStream<T> : Stream<T>
	{
		#region Fields

		private bool isCompleted;
		private bool isReady;
		private T current;

		private readonly IList<T> Buffer;
		private int index = -1;
		private int start = -1;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="value"></param>
		public ListStream(IList<T> value)
		{
			this.Buffer = value ?? new T[0];
		}

		#endregion Init

		#region Chunking Members

		/// <summary>
		/// Gets the number of characters currently chunked
		/// </summary>
		public override int ChunkSize
		{
			get
			{
				if (this.start < 0)
				{
					throw new InvalidOperationException("Not currently chunking.");
				}

				return (1+this.index-this.start);
			}
		}

		/// <summary>
		/// Gets a value indicating if the <see cref="ListStream<T>"/> is currently chunking
		/// </summary>
		public override bool IsChunking
		{
			get { return (this.start >= 0); }
		}

		/// <summary>
		/// Begins chunking at the current index
		/// </summary>
		public override void BeginChunk()
		{
			this.start = this.index+1;
		}

		/// <summary>
		/// Ends chunking at the current index and returns the buffered chunk
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<T> EndChunk()
		{
			if (this.start < 0)
			{
				throw new InvalidOperationException("Not currently chunking.");
			}

			// build chunk value
			IEnumerable<T> value = new Subsequence<T>(this.Buffer, this.start, (1+this.index-this.start));

			// reset chunk start
			this.start = -1;

			return value;
		}

		#endregion Chunking Members

		#region IStream<T> Members

		/// <summary>
		/// Determines if the input sequence has reached the end
		/// </summary>
		public override bool IsCompleted
		{
			get
			{
				this.EnsureReady();

				return this.isCompleted;
			}
		}

		/// <summary>
		/// Returns but does not remove the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public override T Peek()
		{
			this.EnsureReady();

			// return the current item or null if complete
			return this.current;
		}

		/// <summary>
		/// Returns and removes the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public override T Pop()
		{
			this.EnsureReady();

			if (this.isCompleted)
			{
				return this.current;
			}

			// flag as needing to be iterated, but don't execute yet
			this.isReady = false;
			this.index++;

			return this.current;
		}

		#endregion IStream<T> Members

		#region Methods

		/// <summary>
		/// Deferred execution of iterator
		/// </summary>
		private void EnsureReady()
		{
			// only execute when requested
			if (this.isReady || this.isCompleted)
			{
				return;
			}
			this.isReady = true;

			// store the current item or null if complete
			int next = this.index+1;

			SequenceBuffer<T> sBuffer = this.Buffer as SequenceBuffer<T>;
			if (sBuffer != null)
			{
				// avoid using SequenceBuffer<T>.Count as will need to enumerate entire sequence
				if (sBuffer.TryAdvance(next))
				{
					this.current = this.Buffer[next];
				}
				else
				{
					this.isCompleted = true;
					this.current = default(T);
				}
			}
			else
			{
				if (next < this.Buffer.Count)
				{
					this.current = this.Buffer[next];
				}
				else
				{
					this.isCompleted = true;
					this.current = default(T);
				}
			}
		}

		#endregion Methods

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
		}

		#endregion IDisposable Members
	}
}
