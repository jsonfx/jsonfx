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
	/// Detects graph cycles by tracking graph depth
	/// </summary>
	public class DepthCounter : ICycleDetector
	{
		#region Fields

		private readonly int MaxDepth;
		private int depth = 0;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="maxDepth"></param>
		public DepthCounter(int maxDepth)
		{
			if (maxDepth < 1)
			{
				throw new ArgumentException("MaxDepth must be a positive value", "maxDepth");
			}
			this.MaxDepth = maxDepth;
		}

		#endregion Init

		#region Methods

		/// <summary>
		/// Increments the depth
		/// </summary>
		/// <param name="item"></param>
		/// <returns>true if MaxDepth has not been exceeded</returns>
		public bool Add(object item)
		{
			this.depth++;

			return (this.depth >= this.MaxDepth);
		}

		/// <summary>
		/// Increments the depth
		/// </summary>
		/// <param name="item"></param>
		public void Remove(object item)
		{
			this.depth--;
		}

		#endregion Methods
	}
}
