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

namespace JsonFx.IO
{
	/// <summary>
	/// Lazily executes an enumerator exposing the results as an <see cref="IList<T>"/>
	/// </summary>
	internal class SequenceBuffer<T> : IList<T>
	{
		#region Enumerator

		private sealed class Enumerator : IEnumerator<T>
		{
			#region Fields

			private readonly SequenceBuffer<T> Sequence;
			private int Index;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="sequence"></param>
			public Enumerator(SequenceBuffer<T> sequence)
			{
				this.Sequence = sequence;
				this.Index = -1;
			}

			#endregion Init

			#region IEnumerator<T> Members

			public T Current
			{
				get
				{
					if (this.Index < 0)
					{
						throw new InvalidOperationException("Enumerator not started");
					}
					if (this.Index >= this.Sequence.Cache.Count)
					{
						throw new InvalidOperationException("Enumerator has ended");
					}

					return this.Sequence[this.Index];
				}
			}

			#endregion IEnumerator<T> Members

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get { return this.Current; }
			}

			public bool MoveNext()
			{
				int next = this.Index+1;
				if (this.Sequence.TryAdvance(next))
				{
					this.Index = next;
					return true;
				}

				return false;
			}

			public void Reset()
			{
				this.Index = -1;
			}

			#endregion IEnumerator Members

			#region IDisposable Members

			void IDisposable.Dispose()
			{
			}

			#endregion IDisposable Members
		}

		#endregion Enumerator

		#region Fields

		private readonly List<T> Cache;
		private readonly IEnumerator<T> Iterator;
		private bool isComplete;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="sequence"></param>
		public SequenceBuffer(IEnumerable<T> sequence)
		{
			if (sequence == null)
			{
				sequence = new T[0];
			}

			this.Cache = new List<T>();
			this.Iterator = sequence.GetEnumerator();
		}

		#endregion Init

		#region IList<T> Members

		int IList<T>.IndexOf(T item)
		{
			int i = 0;
			foreach (var element in this)
			{
				if (Object.Equals(element, item))
				{
					return i;
				}

				i++;
			}

			return -1;
		}

		void IList<T>.Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		void IList<T>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public T this[int index]
		{
			get
			{
				this.TryAdvance(index);
				return this.Cache[index];
			}
		}

		T IList<T>.this[int index]
		{
			get
			{
				this.TryAdvance(index);
				return this.Cache[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		#endregion IList<T> Members

		#region ICollection<T> Members

		void ICollection<T>.Add(T item)
		{
			throw new NotSupportedException();
		}

		void ICollection<T>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<T>.Contains(T item)
		{
			foreach (var element in this)
			{
				if (Object.Equals(element, item))
				{
					return true;
				}
			}

			return false;
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			this.SeekToEnd();
			this.Cache.CopyTo(array, arrayIndex);
		}

		int ICollection<T>.Count
		{
			get
			{
				this.SeekToEnd();
				return this.Cache.Count;
			}
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException();
		}

		#endregion ICollection<T> Members

		#region IEnumerable<T> Members

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new SequenceBuffer<T>.Enumerator(this);
		}

		#endregion IEnumerable<T> Members

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new SequenceBuffer<T>.Enumerator(this);
		}

		#endregion IEnumerable Members

		#region Methods

		public bool TryAdvance(int index)
		{
			if (index < this.Cache.Count)
			{
				return true;
			}
			if (this.isComplete)
			{
				return false;
			}

			while ((index >= this.Cache.Count) &&
				this.Iterator.MoveNext())
			{
				this.Cache.Add(this.Iterator.Current);
			}

			this.isComplete = (index >= this.Cache.Count);

			return !this.isComplete;
		}

		private void SeekToEnd()
		{
			while (this.Iterator.MoveNext())
			{
				this.Cache.Add(this.Iterator.Current);
			}

			this.isComplete = true;
		}

		#endregion Methods
	}
}
