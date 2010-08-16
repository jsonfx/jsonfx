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
using System.Linq;
using System.Linq.Expressions;

using JsonFx.Model;
using JsonFx.Serialization;

using TokenSequence=System.Collections.Generic.IEnumerable<JsonFx.Serialization.Token<JsonFx.Model.ModelTokenType>>;

namespace JsonFx.Linq
{
	internal class QueryProvider : BaseQueryProvider
	{
		#region Fields

		private readonly QueryEngine Engine;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="analyzer"></param>
		/// <param name="sequences"></param>
		public QueryProvider(ITokenAnalyzer<ModelTokenType> analyzer, IQueryable<TokenSequence> sequences)
		{
			if (analyzer == null)
			{
				throw new ArgumentNullException("analyzer");
			}
			if (sequences == null)
			{
				throw new ArgumentNullException("sequences");
			}

			this.Engine = new QueryEngine(analyzer, sequences);
		}

		#endregion Init

		#region QueryProvider Methods

		public override object Execute(Expression expression)
		{
			// create this once and store it so multiple executions are cached
			Expression translated = this.Engine.Translate(expression);

			if (translated.Type.IsValueType)
			{
				// must obey boxing rules or variance breaks
				translated = Expression.Convert(translated, typeof(object));
			}

			Func<object> compiled = Expression.Lambda<Func<object>>(translated).Compile();

			return compiled();
		}

		public override string GetQueryText(Expression expression)
		{
			if (expression == null)
			{
				return String.Empty;
			}

			return new ExpressionWalker(new Json.JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true })).GetQueryText(expression);
		}

		#endregion QueryProvider Methods
	}
}
