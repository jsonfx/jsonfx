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

using JsonFx.Model;
using JsonFx.Model.Filters;
using JsonFx.Serialization.Filters;
using JsonFx.Serialization.GraphCycles;
using JsonFx.Serialization.Resolvers;

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
		private GraphCycleType graphCycles = GraphCycleType.Ignore;
		private string tab = "\t";
		private int maxDepth;
		private string newLine = Environment.NewLine;
		private readonly ResolverCache ResolverCache;
		private readonly IEnumerable<IDataFilter<ModelTokenType>> ModelFilters;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public DataWriterSettings()
			: this(new PocoResolverStrategy(), new Iso8601DateFilter())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public DataWriterSettings(params IDataFilter<ModelTokenType>[] filters)
			: this(new PocoResolverStrategy(), filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public DataWriterSettings(IEnumerable<IDataFilter<ModelTokenType>> filters)
			: this(new PocoResolverStrategy(), filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="strategy"></param>
		public DataWriterSettings(IResolverStrategy strategy, params IDataFilter<ModelTokenType>[] filters)
			: this(new ResolverCache(strategy), filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="strategy"></param>
		public DataWriterSettings(IResolverStrategy strategy, IEnumerable<IDataFilter<ModelTokenType>> filters)
			: this(new ResolverCache(strategy), filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="resolverCache"></param>
		public DataWriterSettings(ResolverCache resolverCache, params IDataFilter<ModelTokenType>[] filters)
			: this(resolverCache, (IEnumerable<IDataFilter<ModelTokenType>>) filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="resolverCache"></param>
		public DataWriterSettings(ResolverCache resolverCache, IEnumerable<IDataFilter<ModelTokenType>> filters)
		{
			this.ResolverCache = resolverCache;
			this.ModelFilters = filters;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets what to do when graph cycles (repeated references) are encounted
		/// </summary>
		public GraphCycleType GraphCycles
		{
			get { return this.graphCycles; }
			set { this.graphCycles = value; }
		}

		/// <summary>
		/// Gets and sets the maximum nesting depth
		/// </summary>
		/// <remarks>
		/// Depth is a fast and easy safegaurd against detecting graph cycles but may produce false positives
		/// </remarks>
		public int MaxDepth
		{
			get { return this.maxDepth; }
			set { this.maxDepth = value; }
		}

		/// <summary>
		/// Gets and sets if output will be formatted for human reading.
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

		/// <summary>
		/// Gets the custom filters
		/// </summary>
		public IEnumerable<IDataFilter<ModelTokenType>> Filters
		{
			get { return this.ModelFilters; }
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
