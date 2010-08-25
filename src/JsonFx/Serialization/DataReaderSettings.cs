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
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Controls deserialization settings for IDataReader
	/// </summary>
	public sealed class DataReaderSettings :
		IResolverCacheContainer
	{
		#region Fields

		private bool allowNullValueTypes = true;
		private bool allowTrailingContent = true;
		private readonly ResolverCache ResolverCache;
		private readonly IEnumerable<IDataFilter<ModelTokenType>> ModelFilters;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public DataReaderSettings()
			: this(new PocoResolverStrategy(), new Iso8601DateFilter())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public DataReaderSettings(params IDataFilter<ModelTokenType>[] filters)
			: this(new PocoResolverStrategy(), filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public DataReaderSettings(IEnumerable<IDataFilter<ModelTokenType>> filters)
			: this(new PocoResolverStrategy(), filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="strategy"></param>
		public DataReaderSettings(IResolverStrategy strategy, params IDataFilter<ModelTokenType>[] filters)
			: this(new ResolverCache(strategy), filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="strategy"></param>
		public DataReaderSettings(IResolverStrategy strategy, IEnumerable<IDataFilter<ModelTokenType>> filters)
			: this(new ResolverCache(strategy), filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="resolverCache"></param>
		public DataReaderSettings(ResolverCache resolverCache, params IDataFilter<ModelTokenType>[] filters)
			: this(resolverCache, (IEnumerable<IDataFilter<ModelTokenType>>) filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="resolverCache"></param>
		public DataReaderSettings(ResolverCache resolverCache, IEnumerable<IDataFilter<ModelTokenType>> filters)
		{
			this.ResolverCache = resolverCache;
			this.ModelFilters = filters;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets if ValueTypes can accept values of null
		/// </summary>
		/// <remarks>
		/// If this is true and a ValueType T is assigned the value of null,
		/// it will receive the value of default(T).
		/// Setting this to false, throws an exception if null is
		/// specified for a ValueType member.
		/// </remarks>
		public bool AllowNullValueTypes
		{
			get { return this.allowNullValueTypes; }
			set { this.allowNullValueTypes = value; }
		}

		/// <summary>
		/// Gets and sets if should verify that stream is empty after deserialzing each object
		/// </summary>
		/// <remarks>
		/// Setting to true allows reading a JSON stream inside other structures (e.g. JavaScript)
		/// </remarks>
		public bool AllowTrailingContent
		{
			get { return this.allowTrailingContent; }
			set { this.allowTrailingContent = value; }
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
