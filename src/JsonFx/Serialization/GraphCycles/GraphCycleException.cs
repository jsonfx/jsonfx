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
	/// Indicates a graph cycle was detected during serialization
	/// </summary>
	public class GraphCycleException : SerializationException
	{
		#region Fields

		private readonly GraphCycleType GraphCycleType;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public GraphCycleException(GraphCycleType cycleType) : base()
		{
			this.GraphCycleType = cycleType;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		public GraphCycleException(GraphCycleType cycleType, string message) : base(message)
		{
			this.GraphCycleType = cycleType;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public GraphCycleException(GraphCycleType cycleType, string message, Exception innerException)
			: base(message, innerException)
		{
			this.GraphCycleType = cycleType;
		}

#if !SILVERLIGHT
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public GraphCycleException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the type of cycle which caused the error
		/// </summary>
		public GraphCycleType CycleType
		{
			get { return this.GraphCycleType; }
		}

		#endregion Properties
	}
}
