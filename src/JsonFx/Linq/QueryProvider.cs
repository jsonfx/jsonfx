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
using System.Linq.Expressions;

using JsonFx.Common;
using JsonFx.Serialization;

using TokenSequence=System.Collections.Generic.IEnumerable<JsonFx.Serialization.Token<JsonFx.Common.CommonTokenType>>;

namespace JsonFx.Linq
{
	public class QueryProvider : BaseQueryProvider
	{
		#region Fields

		private readonly ITokenAnalyzer<CommonTokenType> Analyzer;
		private readonly IEnumerable<TokenSequence> Values;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="analyzer"></param>
		/// <param name="sequence"></param>
		public QueryProvider(ITokenAnalyzer<CommonTokenType> analyzer, IEnumerable<TokenSequence> values)
		{
			if (analyzer == null)
			{
				throw new ArgumentNullException("analyzer");
			}
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}

			this.Analyzer = analyzer;
			this.Values = values;
		}

		#endregion Init

		#region QueryProvider Methods

		public override object Execute(Expression expression)
		{
			return new QueryEngine(this.Analyzer, this.Values).Execute(expression);
		}

		public override string GetQueryText(Expression expression)
		{
			if (expression == null)
			{
				return String.Empty;
			}

			return new ExpressionWalker(new Json.JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true }, false)).GetQueryText(expression);
		}

		#endregion QueryProvider Methods
	}
}
