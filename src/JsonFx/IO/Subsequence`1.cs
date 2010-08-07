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
	/// Wrapper for an (unchanging) <see cref="IList<T>"/> defining a subsequence
	/// </summary>
	internal class Subsequence<T> : IList<T>
	{
		#region Enumerator

		private sealed class Enumerator : IEnumerator<T>
		{
			#region Fields

			private readonly IList<T> Items;
			private readonly int Start;
			private readonly int End;
			private int Index;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="sequence"></param>
			/// <param name="start"></param>
			/// <param name="count"></param>
			public Enumerator(IList<T> sequence, int start, int length)
			{
				this.Items = sequence;
				this.Index = start-1;
				this.Start = start;
				this.End = start+length;
			}

			#endregion Init

			#region IEnumerator<T> Members

			public T Current
			{
				get
				{
					if (this.Index < this.Start)
					{
						throw new InvalidOperationException("Enumerator not started");
					}
					if (this.Index >= this.End)
					{
						throw new InvalidOperationException("Enumerator has ended");
					}

					return this.Items[this.Index];
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
				if (this.Index < this.End)
				{
					this.Index++;
					return (this.Index < this.End);
				}

				return false;
			}

			public void Reset()
			{
				this.Index = this.Start-1;
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

		private readonly IList<T> Items;
		private readonly int Start;
		private readonly int Size;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="sequence"></param>
		public Subsequence(IList<T> sequence, int start, int length)
		{
			if (sequence == null)
			{
				throw new ArgumentNullException("sequence");
			}
			if (start < 0 || start >= sequence.Count)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			if (length < 0 || length > start+sequence.Count)
			{
				throw new ArgumentOutOfRangeException("length");
			}

			Subsequence<T> subsequence = sequence as Subsequence<T>;
			if (subsequence != null)
			{
				// optimize by using original internal list
				this.Items = subsequence.Items;
				this.Start = subsequence.Start + start;
				this.Size = length;
				return;
			}

			this.Items = sequence;
			this.Start= start;
			this.Size = length;
		}

		#endregion Init

		#region IList<T> Members

		public int IndexOf(T item)
		{
#if !SILVERLIGHT
			T[] array = this.Items as T[];
			if (array != null)
			{
				return Array.FindIndex(
					array,
					this.Start,
					this.Size,
					n => Object.Equals(n, item));
			}

			List<T> list = this.Items as List<T>;
			if (list != null)
			{
				return list.FindIndex(
					this.Start,
					this.Size,
					n => Object.Equals(n, item));
			}
#endif

			int endIndex = this.Start + this.Size;
			for (int i = this.Start; i < endIndex; i++)
			{
				if (Object.Equals(this.Items[i], item))
				{
					return i;
				}
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
			get { return this.Items[this.Start+index]; }
		}

		T IList<T>.this[int index]
		{
			get { return this.Items[this.Start+index]; }
			set { throw new NotSupportedException(); }
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

		public bool Contains(T item)
		{
			return (this.IndexOf(item) >= 0);
		}

		public void CopyTo(T[] dest, int arrayIndex)
		{
			if (dest == null)
			{
				throw new ArgumentNullException("dest");
			}

			if (this.Size + arrayIndex > dest.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}

			T[] array = this.Items as T[];
			if (array != null)
			{
				Array.Copy(
					array,
					this.Start,
					dest,
					arrayIndex,
					this.Size);
				return;
			}

			List<T> list = this.Items as List<T>;
			if (list != null)
			{
				list.CopyTo(
					this.Start,
					dest,
					arrayIndex,
					this.Size);
				return;
			}

			for (int i=arrayIndex, j=this.Start, end=arrayIndex+this.Size; i<end; i++, j++)
			{
				dest[i] = this.Items[j];
			}
		}

		public int Count
		{
			get { return this.Size; }
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

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this.Items, this.Start, this.Size);
		}

		#endregion IEnumerable<T> Members

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this.Items, this.Start, this.Size);
		}

		#endregion IEnumerable Members
	}
}
