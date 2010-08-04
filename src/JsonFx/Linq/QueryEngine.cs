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
using System.Reflection;

using JsonFx.Common;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;

using TokenSequence=System.Collections.Generic.IEnumerable<JsonFx.Serialization.Token<JsonFx.Common.CommonTokenType>>;

namespace JsonFx.Linq
{
	/// <summary>
	/// Searches for subsequences found by the expression and rehydrates into objects with the analyzer
	/// </summary>
	internal class QueryEngine : ExpressionVisitor<QueryEngine.QueryContext>
	{
		#region QueryContext

		public class QueryContext
		{
			internal IEnumerable<TokenSequence> Input { get; set; }

			internal Type InputType { get; set; }

			internal Type OutputType { get; set; }

			internal bool Transforming { get; set; }

			internal IDictionary<string, ParameterExpression> Parameters { get; set; }
		}

		#endregion QueryContext

		#region Fields

		private static readonly MethodInfo MemberAccess;

		private readonly DataReaderSettings Settings;
		private readonly ITokenAnalyzer<CommonTokenType> Analyzer;
		private readonly IEnumerable<TokenSequence> Source;

		private Func<object> execute;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		static QueryEngine()
		{
			// look this one up since doesn't change

			QueryEngine.MemberAccess = typeof(CommonSubsequencer).GetMethod(
				"Property",
				BindingFlags.Public|BindingFlags.Static,
				null,
				new[] { typeof(TokenSequence), typeof(DataName) },
				null);
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="analyzer"></param>
		/// <param name="sequence"></param>
		public QueryEngine(ITokenAnalyzer<CommonTokenType> analyzer, IEnumerable<TokenSequence> values)
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
			this.Settings = analyzer.Settings;
			this.Source = values;
		}

		#endregion Init

		#region IQueryEngine<CommonTokenType> Members

		public object Execute(Expression expression)
		{
			if (this.execute == null)
			{
				//var walker = new ExpressionWalker(new JsonFx.Json.JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true }));
				//System.Diagnostics.Trace.WriteLine("Query Expression:");
				//System.Diagnostics.Trace.WriteLine(walker.GetQueryText(expression));

				Expression translated = this.Visit(expression, new QueryContext { Input = this.Source });
				if (expression != null &&
					translated != null &&
					expression.Type != translated.Type)
				{
					Type targetType = expression.Type;
					bool asSingle = true;

					Type queryableType = targetType.IsGenericType ? targetType.GetGenericTypeDefinition() : null;
					if (queryableType == typeof(IQueryable<>))
					{
						asSingle = false;
						targetType = targetType.GetGenericArguments()[0];
					}

					// should just need to be analyzed
					translated = this.CallAnalyze(targetType, translated, asSingle);
				}

				//System.Diagnostics.Trace.WriteLine("Translated Expression");
				//System.Diagnostics.Trace.WriteLine(walker.GetQueryText(translated));

				this.execute = Expression.Lambda<Func<object>>(translated).Compile();
			}

			return this.execute();
		}

		#endregion IQueryEngine<CommonTokenType> Members

		#region Visit Methods

		protected override Expression VisitConstant(ConstantExpression constant, QueryContext context)
		{
			Type type = constant.Type;

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(JsonFx.Linq.Query<>))
			{
				context.InputType = type.GetGenericArguments()[0];

				// seed of transforming the expression
				// all other changes will ripple from this point
				return Expression.Constant(context.Input.AsQueryable());
			}

			return base.VisitConstant(constant, context);
		}

		protected override Expression VisitMethodCall(MethodCallExpression m, QueryContext context)
		{
			if (m.Method.DeclaringType == typeof(Queryable) || m.Method.DeclaringType == typeof(Enumerable))
			{
				// TODO: determine invalid situations to transform

				switch (m.Method.Name)
				{
					case "Where":
					{
						Type[] methodArgs = m.Method.GetGenericArguments();
						var nextContext = new QueryContext
						{
							Input = context.Input,
							InputType = methodArgs[0],
							Transforming = true
						};

						Expression source = this.Visit(m.Arguments[0], nextContext);

						Expression predicate = this.Visit(m.Arguments[1], nextContext);

						return Expression.Call(typeof(Queryable), "Where", new [] { typeof(TokenSequence) }, source, predicate);
					}
					case "Select":
					{
						Type[] methodArgs = m.Method.GetGenericArguments();
						var nextContext = new QueryContext
						{
							Input = context.Input,
							InputType = methodArgs[0],
							OutputType = methodArgs[1],
							Transforming = true
						};

						Expression source = this.Visit(m.Arguments[0], nextContext);

						Expression selector = this.Visit(m.Arguments[1], nextContext);

						return Expression.Call(typeof(Queryable), "Select", new[] { typeof(TokenSequence), nextContext.OutputType }, source, selector);
					}
					default:
					{
						IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments, context);
						if (args == m.Arguments)
						{
							// no change
							return m;
						}

						Expression[] argArray = args.ToArray();
						Expression source = argArray[0];
						if (source != null &&
							source.Type != m.Type)
						{
							Type targetType = m.Type;
							Type queryableType = targetType.IsGenericType ? targetType.GetGenericTypeDefinition() : null;
							if (queryableType == typeof(IQueryable<>))
							{
								targetType = targetType.GetGenericArguments()[0];
							}

							// should just need to be analyzed
							argArray[0] = this.CallAnalyze(targetType, source, false);
						}

						return Expression.Call(m.Method, argArray);
					}
				}
			}

			return base.VisitMethodCall(m, context);
		}

		protected override Expression VisitLambda(LambdaExpression lambda, QueryContext context)
		{
			if (!context.Transforming)
			{
				base.VisitLambda(lambda, context);
			}

			int length = lambda.Parameters.Count;

			IDictionary<string, ParameterExpression> oldParams = context.Parameters;
			try
			{
				context.Parameters = new Dictionary<string, ParameterExpression>(length);
				for (int i=0; i<length; i++)
				{
					if (lambda.Parameters[i].Type == context.InputType)
					{
						context.Parameters[lambda.Parameters[i].Name] = Expression.Parameter(typeof(TokenSequence), lambda.Parameters[i].Name);
					}
					else
					{
						context.Parameters[lambda.Parameters[i].Name] = lambda.Parameters[i];
					}
				}

				Type[] exprTypeArgs = lambda.Type.GetGenericArguments();
				length = exprTypeArgs.Length;

				for (int i=0; i<length; i++)
				{
					if (exprTypeArgs[i] == context.InputType)
					{
						exprTypeArgs[i] = typeof(TokenSequence);
					}
				}

				Expression body = this.Visit(lambda.Body, context);

				return Expression.Lambda(lambda.Type.GetGenericTypeDefinition().MakeGenericType(exprTypeArgs), body, context.Parameters.Values);
			}
			finally
			{
				context.Parameters = oldParams;
			}
		}

		protected override Expression VisitMemberAccess(MemberExpression m, QueryContext context)
		{
			if (!context.Transforming)
			{
				return base.VisitMemberAccess(m, context);
			}

			MemberInfo member = m.Member;
			Type targetType;
			if (member is PropertyInfo)
			{
				targetType = ((PropertyInfo)member).PropertyType;
			}
			else if (member is FieldInfo)
			{
				targetType = ((FieldInfo)member).FieldType;
			}
			else
			{
				targetType = null;
			}

			MemberMap map = this.Settings.Resolver.LoadMemberMap(member);

			ParameterExpression p = (ParameterExpression)m.Expression;

			// expression gets translated to:
			//		this.Analyzer.Analyze<targetType>(CommonSubsequencer.Property(value, map.DataName).FirstOrDefault();
	
			var memberAccess = Expression.Call(QueryEngine.MemberAccess, context.Parameters[p.Name], Expression.Constant(map.DataName));

			return this.CallAnalyze(targetType, memberAccess, true);
		}

		private Expression CallAnalyze(Type targetType, Expression sequence, bool asSingle)
		{
			Expression analyze;

			if (sequence.Type == typeof(IQueryable<TokenSequence>))
			{
				// lambda to analyze each sequence stream
				ParameterExpression p = Expression.Parameter(typeof(TokenSequence), "sequence");
				analyze = Expression.Lambda(Expression.Call(Expression.Constant(this.Analyzer), "Analyze", new[] { targetType }, p), p);

				// combine sequence of analyzed results
				analyze = Expression.Call(typeof(Queryable), "SelectMany", new[] { typeof(TokenSequence), targetType }, sequence, analyze);
			}
			else
			{
				// analyze sequence stream
				analyze = Expression.Call(Expression.Constant(this.Analyzer), "Analyze", new[] { targetType }, sequence);
			}

			if (asSingle)
			{
				// reduce to only first value
				analyze = Expression.Call(typeof(Enumerable), "FirstOrDefault", new[] { targetType }, analyze);
			}

			return analyze;
		}

		#endregion Visit Methods
	}
}
