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
using System.Reflection;

using JsonFx.Model;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;

using TokenSequence=System.Collections.Generic.IEnumerable<JsonFx.Serialization.Token<JsonFx.Model.ModelTokenType>>;

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
			internal IQueryable<TokenSequence> Input { get; set; }

			internal Type InputType { get; set; }

			internal IDictionary<string, ParameterExpression> Parameters { get; set; }
		}

		#endregion QueryContext

		#region Fields

		private static readonly MethodInfo MemberAccess;

		private readonly ResolverCache Resolver;
		private readonly ITokenAnalyzer<ModelTokenType> Analyzer;
		private readonly IQueryable<TokenSequence> Source;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		static QueryEngine()
		{
			// look this one up since doesn't change

			QueryEngine.MemberAccess = typeof(ModelSubsequencer).GetMethod(
				"Property",
				BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy,
				null,
				new[] { typeof(TokenSequence), typeof(DataName) },
				null);
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="analyzer"></param>
		/// <param name="sequence"></param>
		public QueryEngine(ITokenAnalyzer<ModelTokenType> analyzer, IQueryable<TokenSequence> input)
		{
			if (analyzer == null)
			{
				throw new ArgumentNullException("analyzer");
			}
			if (input == null)
			{
				throw new ArgumentNullException("values");
			}

			this.Analyzer = analyzer;
			this.Resolver = analyzer.Settings.Resolver;
			this.Source = input;
		}

		#endregion Init

		#region IQueryEngine<ModelTokenType> Members

		public Expression Translate(Expression expression)
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

				if (targetType.IsGenericType &&
					(typeof(IQueryable).IsAssignableFrom(targetType)) ||
					(typeof(IEnumerable).IsAssignableFrom(targetType)))
				{
					asSingle = false;
					targetType = targetType.GetGenericArguments()[0];
				}

				// should just need to be analyzed
				translated = this.CallAnalyze(targetType, translated, asSingle);
			}

			//System.Diagnostics.Trace.WriteLine("Translated Expression");
			//System.Diagnostics.Trace.WriteLine(walker.GetQueryText(translated));

			return translated;
		}

		#endregion IQueryEngine<ModelTokenType> Members

		#region Visit Methods

		protected override Expression VisitConstant(ConstantExpression constant, QueryContext context)
		{
			Type type = constant.Type;
			Type[] genericArgs = type.IsGenericType ? type.GetGenericArguments() : null;
			Type queryType = typeof(JsonFx.Linq.Query<>);

			if (genericArgs != null &&
				genericArgs.Length == 1 &&
				queryType.MakeGenericType(genericArgs).IsAssignableFrom(type))
			{
				context.InputType = genericArgs[0];

				// seed of transforming the expression
				// all other changes will ripple from this point
				return Expression.Constant(context.Input);
			}

			return base.VisitConstant(constant, context);
		}

		protected override Expression VisitMethodCall(MethodCallExpression m, QueryContext context)
		{
			if (m.Method.DeclaringType != typeof(Queryable) && m.Method.DeclaringType != typeof(Enumerable))
			{
				// only intercept Queryable & Enumerable calls
				return base.VisitMethodCall(m, context);
			}

			Type[] methodArgs = m.Method.IsGenericMethod ? m.Method.GetGenericArguments() : Type.EmptyTypes;
			var nextContext = new QueryContext
			{
				Input = context.Input,
				InputType = (methodArgs.Length > 0) ? methodArgs[0] : null
			};

			Expression[] args = this.VisitExpressionList(m.Arguments, nextContext).ToArray();

			Expression source = args[0];
			if (source != null &&
						source.Type != m.Arguments[0].Type)
			{
				Type sourceType = source.Type;
				if (sourceType.IsGenericType && typeof(IQueryable).IsAssignableFrom(sourceType))
				{
					sourceType = sourceType.GetGenericArguments()[0];
				}

				methodArgs[0] = sourceType;
			}

			return Expression.Call(m.Method.DeclaringType, m.Method.Name, methodArgs, args);
		}

		protected override Expression VisitLambda(LambdaExpression lambda, QueryContext context)
		{
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

			MemberMap map = this.Resolver.LoadMemberMap(member);

			ParameterExpression p = (ParameterExpression)m.Expression;

			// expression is loosely translated to:
			//		this.Analyzer.Analyze<targetType>(ModelSubsequencer.Property(value, map.DataName).FirstOrDefault());

			var memberAccess = Expression.Call(QueryEngine.MemberAccess, context.Parameters[p.Name], Expression.Constant(map.DataName));

			return this.CallAnalyze(targetType, memberAccess, true);
		}

		private Expression CallAnalyze(Type targetType, Expression sequence, bool asSingle)
		{
			Expression analyze;

			if (sequence.Type == typeof(IEnumerable<TokenSequence>))
			{
				sequence = Expression.Call(typeof(Queryable), "AsQueryable", new[] { typeof(TokenSequence) }, sequence);
			}

			if (sequence.Type == typeof(IQueryable<TokenSequence>) ||
				sequence.Type == typeof(IOrderedQueryable<TokenSequence>))
			{
				sequence = Expression.Call(typeof(Queryable), "DefaultIfEmpty", new[] { typeof(TokenSequence) }, sequence, Expression.Call(typeof(Enumerable), "Empty", new[] { typeof(Token<ModelTokenType>) }));

				// lambda to analyze each sequence stream
				ParameterExpression p = Expression.Parameter(typeof(TokenSequence), "sequence");
				analyze = Expression.Lambda(Expression.Call(Expression.Constant(this.Analyzer), "Analyze", new[] { targetType }, p), p);

				// combine sequence of analyzed results
				analyze = Expression.Call(typeof(Queryable), "SelectMany", new[] { typeof(TokenSequence), targetType }, sequence, analyze);
			}
			else
			{
				sequence = Expression.Coalesce(sequence, Expression.Call(typeof(Enumerable), "Empty", new[] { typeof(Token<ModelTokenType>) }));

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
