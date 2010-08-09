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
using System.Linq;
using System.Linq.Expressions;

namespace JsonFx.Linq
{
	public class Query<T> :
		IQueryable<T>,
		IQueryable,
		IEnumerable<T>,
		IEnumerable,
		IOrderedQueryable<T>,
		IOrderedQueryable
	{
		#region Fields

		private readonly IQueryProvider Provider;
		private readonly Expression Expression;
		private object result;
		private bool hasResult;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="provider"></param>
		public Query(IQueryProvider provider)
			: this(provider, null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="expression"></param>
		public Query(IQueryProvider provider, Expression expression)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			if (expression != null &&
				!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
			{
				throw new ArgumentException("expression");
			}

			this.Provider = provider;
			this.Expression = (expression != null) ? expression : Expression.Constant(this);
		}

		#endregion Init

		#region IQueryable Properties

		/// <summary>
		/// Gets the query expression
		/// </summary>
		Expression IQueryable.Expression
		{
			get { return this.Expression; }
		}

		/// <summary>
		/// Gets the query return type
		/// </summary>
		Type IQueryable.ElementType
		{
			get { return typeof(T); }
		}

		/// <summary>
		/// Gets the underlying provider
		/// </summary>
		IQueryProvider IQueryable.Provider
		{
			get { return this.Provider; }
		}

		#endregion IQueryable Properties

		#region IEnumerable Methods

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			if (!this.hasResult)
			{
				this.result = this.Provider.Execute<IEnumerable<T>>(this.Expression);
				this.hasResult = true;
			}

			return ((IEnumerable<T>)this.result).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (!this.hasResult)
			{
				this.result = this.Provider.Execute(this.Expression);
				this.hasResult = true;
			}

			return ((IEnumerable)this.result).GetEnumerator();
		}

		#endregion IEnumerable Methods

		#region Object Overrides

		/// <summary>
		/// Returns a string representation of the query
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			IQueryTextProvider queryText = this.Provider as IQueryTextProvider;
			if (queryText == null)
			{
				return String.Empty;
			}

			return queryText.GetQueryText(this.Expression);
		}

		#endregion Object Overrides
	}
}
