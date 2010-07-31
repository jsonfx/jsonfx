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
using System.Linq.Expressions;

using JsonFx.Common;
using JsonFx.Serialization;

namespace JsonFx.Linq
{
	/// <summary>
	/// Searches for subsequences found by the expression and rehydrates into objects with the analyzer
	/// </summary>
	internal class QueryEngine : ExpressionVisitor
	{
		#region Fields

		private readonly ITokenAnalyzer<CommonTokenType> Analyzer;
		private readonly IEnumerable<Token<CommonTokenType>> Sequence;
		private Func<object> execute;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="analyzer"></param>
		/// <param name="sequence"></param>
		public QueryEngine(ITokenAnalyzer<CommonTokenType> analyzer, IEnumerable<Token<CommonTokenType>> sequence)
		{
			if (analyzer == null)
			{
				throw new ArgumentNullException("analyzer");
			}
			if (sequence == null)
			{
				throw new ArgumentNullException("sequence");
			}

			this.Analyzer = analyzer;
			this.Sequence = sequence;
		}

		#endregion Init

		#region IQueryEngine<CommonTokenType> Members

		public object Execute(Expression expression)
		{
			if (this.execute == null)
			{
				expression = this.Visit(expression);

				this.execute = Expression.Lambda<Func<object>>(expression, (IEnumerable<ParameterExpression>)null).Compile();
			}

			return this.execute();
		}

		#endregion IQueryEngine<CommonTokenType> Members

		#region Visit Methods

		protected override Expression VisitConstant(ConstantExpression constant)
		{
			if (!constant.Type.IsGenericType ||
				constant.Type.GetGenericTypeDefinition() != typeof(Query<>))
			{
				return base.VisitConstant(constant);
			}

			Type targetType = ((IQueryable)constant.Value).ElementType;
			IQueryable query = this.Analyzer.Analyze(this.Sequence, targetType).AsQueryable();

			// TODO: use a better way than reflection
			object result = typeof(Queryable).GetMethod("OfType").MakeGenericMethod(targetType).Invoke(null, new[] { query });

			// convert the query to the expected type
			return Expression.Constant(result);
		}

		#endregion Visit Methods
	}
}
