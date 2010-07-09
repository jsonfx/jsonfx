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
	public class Stream<T> : IDisposable
	{
		#region Fields

		private readonly IEnumerable<T> Sequence;
		private readonly IEnumerator<T> Enumerator;
		private bool isReady;
		private bool isComplete;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="sequence"></param>
		public Stream(IEnumerable<T> sequence)
		{
			if (sequence == null)
			{
				throw new ArgumentNullException("sequence");
			}

			this.Sequence = sequence;
			this.Enumerator = sequence.GetEnumerator();
		}

		#endregion Fields

		#region Properties

		public bool IsComplete
		{
			get
			{
				this.EnsureReady();

				return this.isComplete;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Returns but does not remove the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public T Peek()
		{
			this.EnsureReady();

			// return the current item or null if complete
			return this.isComplete ? default(T) : this.Enumerator.Current;
		}

		/// <summary>
		/// Returns and removes the top item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public T Pop()
		{
			this.EnsureReady();

			if (this.isComplete)
			{
				// return null if complete
				return default(T);
			}

			// flag as needing to be iterated, but don't execute yet
			this.isReady = this.isComplete;

			// return the current item
			return this.Enumerator.Current;
		}

		/// <summary>
		/// Deferred execution of iterator
		/// </summary>
		private void EnsureReady()
		{
			// only execute when requested
			if (this.isReady)
			{
				return;
			}

			// lazy execution of MoveNext
			this.isComplete = !this.Enumerator.MoveNext();
			this.isReady = true;
		}

		#endregion Methods

		#region IDisposable Members

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IDisposable)this.Enumerator).Dispose();
			}
		}

		#endregion IDisposable Members
	}
}
