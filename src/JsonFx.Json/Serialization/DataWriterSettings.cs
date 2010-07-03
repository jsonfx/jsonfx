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

namespace JsonFx.Serialization
{
	/// <summary>
	/// Controls the serialization settings for IDataWriter
	/// </summary>
	public sealed class DataWriterSettings :
		IResolverCacheContainer
	{
		#region Fields

		private bool prettyPrint;
		private string tab = "\t";
		private int maxDepth = 100;
		private string newLine = Environment.NewLine;
		private readonly ResolverCache ResolverCache;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public DataWriterSettings()
			: this(new DataNameResolverStrategy())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="strategy"></param>
		public DataWriterSettings(IResolverStrategy strategy)
			: this(new ResolverCache(strategy))
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="resolverCache"></param>
		public DataWriterSettings(ResolverCache resolverCache)
		{
			this.ResolverCache = resolverCache;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the maximum nesting depth
		/// </summary>
		/// <remarks>
		/// Depth is fast and easy safegaurd against detecting graph cycles.
		/// </remarks>
		public int MaxDepth
		{
			get { return this.maxDepth; }
			set { this.maxDepth = value; }
		}

		/// <summary>
		/// Gets and sets if JSON will be formatted for human reading.
		/// </summary>
		public bool PrettyPrint
		{
			get { return this.prettyPrint; }
			set { this.prettyPrint = value; }
		}

		/// <summary>
		/// Gets and sets the string to use for indentation
		/// </summary>
		public string Tab
		{
			get { return this.tab; }
			set { this.tab = value; }
		}

		/// <summary>
		/// Gets and sets the line terminator string
		/// </summary>
		public string NewLine
		{
			get { return this.newLine; }
			set { this.newLine = value; }
		}

		/// <summary>
		/// Gets manager of name resolution for IDataReader
		/// </summary>
		public ResolverCache Resolver
		{
			get { return this.ResolverCache; }
		}

		#endregion Properties

		#region IResolverCacheContainer Members

		ResolverCache IResolverCacheContainer.ResolverCache
		{
			get { return this.ResolverCache; }
		}

		#endregion IResolverCacheContainer Members
	}
}
