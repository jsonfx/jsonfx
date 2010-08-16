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
using System.Linq;

using JsonFx.Linq;
using JsonFx.Serialization;

using TokenSequence=System.Collections.Generic.IEnumerable<JsonFx.Serialization.Token<JsonFx.Model.ModelTokenType>>;

namespace JsonFx.Model
{
	/// <summary>
	/// Represents a query across a data source
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Query<T> : JsonFx.Linq.Query<T>
	{
		#region Fields

		private readonly ITokenAnalyzer<ModelTokenType> Analyzer;
		private readonly IEnumerable<TokenSequence> Sequences;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="analyzer"></param>
		/// <param name="sequence"></param>
		internal Query(ITokenAnalyzer<ModelTokenType> analyzer, TokenSequence sequence)
			: this(analyzer, sequence.SplitValues())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="analyzer"></param>
		/// <param name="sequences"></param>
		internal Query(ITokenAnalyzer<ModelTokenType> analyzer, IEnumerable<TokenSequence> sequences)
			: base(new QueryProvider(analyzer, sequences.AsQueryable()))
		{
			this.Analyzer = analyzer;
			this.Sequences = sequences;
		}

		#endregion Init

		#region Query Methods

		/// <summary>
		/// Extends the query to all descendants
		/// </summary>
		public Query<T> Descendants()
		{
			return new Query<T>(this.Analyzer, this.Sequences.SelectMany(sequence => sequence.Descendants()));
		}

		/// <summary>
		/// Extends the query to all descendants
		/// </summary>
		public Query<T> DescendantsAndSelf()
		{
			return new Query<T>(this.Analyzer, this.Sequences.SelectMany(sequence => sequence.DescendantsAndSelf()));
		}

		/// <summary>
		/// Gets all items of the array
		/// </summary>
		public Query<T> ArrayItems()
		{
			return new Query<T>(this.Analyzer, this.Sequences.SelectMany(sequence => sequence.ArrayItems()));
		}

		/// <summary>
		/// Gets the items of the array with indexes satisfying the <paramref name="predicate"/>
		/// </summary>
		/// <param name="predicate"></param>
		public Query<T> ArrayItems(Func<int, bool> predicate)
		{
			return new Query<T>(this.Analyzer, this.Sequences.SelectMany(sequence => sequence.ArrayItems(predicate)));
		}

		/// <summary>
		/// Filters to only objects with a particular property name defined
		/// </summary>
		/// <param name="predicate"></param>
		public Query<T> WhereHasProperty(Func<DataName, bool> predicate)
		{
			return new Query<T>(this.Analyzer, this.Sequences.Where(sequence => sequence.HasProperty(predicate)));
		}

		/// <summary>
		/// Filters to a lookup of only the properties which match the predicate
		/// </summary>
		/// <param name="predicate"></param>
		public ILookup<DataName, Query<T>> Properties(Func<DataName, bool> predicate)
		{
			return this.Sequences.SelectMany(sequence => sequence.Properties(predicate)).ToLookup(pair => pair.Key, pair => new Query<T>(this.Analyzer, pair.Value));
		}

		/// <summary>
		/// Filters to only arrays
		/// </summary>
		/// <param name="predicate"></param>
		public Query<T> WhereIsArray()
		{
			return new Query<T>(this.Analyzer, this.Sequences.Where(sequence => sequence.IsArray()));
		}

		/// <summary>
		/// Filters to only objects
		/// </summary>
		/// <param name="predicate"></param>
		public Query<T> WhereIsObject()
		{
			return new Query<T>(this.Analyzer, this.Sequences.Where(sequence => sequence.IsObject()));
		}

		/// <summary>
		/// Filters to only simple values
		/// </summary>
		/// <param name="predicate"></param>
		public Query<T> WhereIsPrimitive()
		{
			return new Query<T>(this.Analyzer, this.Sequences.Where(sequence => sequence.IsPrimitive()));
		}

		#endregion Query Methods
	}
}
