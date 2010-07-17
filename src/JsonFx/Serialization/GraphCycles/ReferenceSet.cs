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

namespace JsonFx.Serialization.GraphCycles
{
	/// <summary>
	/// Detects cycles by detecting duplicates in the a set of object references
	/// </summary>
	public class ReferenceSet : ICycleDetector
	{
		#region Constants

		private static readonly object[] EmptyArray = new object[0];
		private const int InitialSize = 4;
		private const int GrowthFactor = 2;

		#endregion Constants

		#region Fields

		private object[] array = ReferenceSet.EmptyArray;
		private int size;

		#endregion Fields

		#region Methods

		/// <summary>
		/// Adds a reference to the set
		/// </summary>
		/// <param name="item"></param>
		/// <returns>true if object already existed within set</returns>
		public bool Add(object item)
		{
			if (item == null || item is ValueType || item is string)
			{
				// ignore these as they are not indicative of graph cycles
				return false;
			}

			// check for duplicates
			for (int i=this.size-1; i >= 0; i--)
			{
				if (Object.ReferenceEquals(this.array[i], item))
				{
					return true;
				}
			}

			if (this.size >= this.array.Length)
			{
				object[] destArray = new object[(this.array.Length == 0) ? InitialSize : (GrowthFactor * this.array.Length)];
				if (this.size > 0)
				{
					Array.Copy(this.array, 0, destArray, 0, this.size);
				}
				this.array = destArray;
			}

			this.array[this.size++] = item;

			return false;
		}

		/// <summary>
		/// Removes a reference from the set
		/// </summary>
		/// <param name="item"></param>
		public void Remove(object item)
		{
			if (item == null || item is ValueType || item is string)
			{
				// ignore these as they are not indicative of graph cycles
				return;
			}

			// check for existance
			for (int i=this.size-1; i >= 0; i--)
			{
				if (Object.ReferenceEquals(this.array[i], item))
				{
					if (i+1 == this.size)
					{
						this.array[i] = null;
						this.size--;
					}
					else
					{
						// according to MSDN, this is safe: "If sourceArray and destinationArray overlap,
						// this method behaves as if the original values of sourceArray were preserved in
						// a temporary location before destinationArray is overwritten. "

						// shift buffer to zero aligned
						Array.Copy(this.array, i+1, this.array, i, this.size-i);
					}
					return;
				}
			}

			return;
		}

		#endregion Methods
	}
}
