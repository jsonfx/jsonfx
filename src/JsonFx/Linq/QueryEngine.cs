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
		private static readonly MethodInfo WhereTokenSequence;
		private static readonly MethodInfo SelectTokenSequence;
		private static readonly MethodInfo FirstOrDefaultGeneric;
		private static readonly MethodInfo AnalyzeMethod;

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
			QueryEngine.MemberAccess = typeof(CommonSubsequencer).GetMethod(
				"Property",
				BindingFlags.Public|BindingFlags.Static,
				null,
				new[] { typeof(TokenSequence), typeof(DataName) },
				null);

			MethodInfo[] methods = typeof(Queryable).GetMethods(BindingFlags.Public|BindingFlags.Static);

			// TODO: figure out how to grab generic versions directly
			// I think there are Expression.Call overrides for this

			foreach (MethodInfo method in methods)
			{
				if (!method.IsGenericMethod)
				{
					continue;
				}

				ParameterInfo[] paramTypes = method.GetParameters();
				if (paramTypes.Length != 2)
				{
					continue;
				}

				Type exprType = paramTypes[1].ParameterType;
				if (!exprType.IsGenericType ||
					!exprType.GetGenericArguments()[0].IsGenericType ||
					exprType.GetGenericArguments()[0].GetGenericTypeDefinition() != typeof(Func<,>))
				{
					continue;
				}

				switch (method.Name)
				{
					case "Where":
					{
						QueryEngine.WhereTokenSequence = method.MakeGenericMethod(typeof(TokenSequence));

						if (QueryEngine.SelectTokenSequence == null)
						{
							// keep searching
							continue;
						}
						break;
					}
					case "Select":
					{
						QueryEngine.SelectTokenSequence = method;

						if (QueryEngine.WhereTokenSequence == null)
						{
							// keep searching
							continue;
						}
						break;
					}
					default:
					{
						continue;
					}
				}

				// exit loop
				break;
			}

			methods = typeof(Enumerable).GetMethods(BindingFlags.Public|BindingFlags.Static);

			foreach (MethodInfo method in methods)
			{
				if (method.IsGenericMethod &&
					method.Name == "FirstOrDefault" &&
					method.GetParameters().Length == 1)
				{
					QueryEngine.FirstOrDefaultGeneric = method;
					break;
				}
			}

			methods = typeof(ITokenAnalyzer<CommonTokenType>).GetMethods(BindingFlags.Public|BindingFlags.Instance);

			foreach (MethodInfo method in methods)
			{
				if (method.IsGenericMethod &&
					method.Name == "Analyze" &&
					method.GetParameters().Length == 1)
				{
					QueryEngine.AnalyzeMethod = method;
					break;
				}
			}
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
				expression = this.Visit(expression, new QueryContext { Input = this.Source });

				//System.Diagnostics.Trace.WriteLine("Translated Expression");
				//System.Diagnostics.Trace.WriteLine(new ExpressionWalker(new JsonFx.Json.JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint=true })).GetQueryText(expression));

				this.execute = Expression.Lambda<Func<object>>(expression).Compile();
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

						return Expression.Call(QueryEngine.WhereTokenSequence, source, predicate);
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

						return Expression.Call(QueryEngine.SelectTokenSequence.MakeGenericMethod(typeof(TokenSequence), nextContext.OutputType), source, selector);
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

			return this.CallAnalyze(memberAccess, targetType, true);
		}

		private Expression CallAnalyze(Expression sequence, Type targetType, bool asSingle)
		{
			var analyze = Expression.Call(Expression.Constant(this.Analyzer), QueryEngine.AnalyzeMethod.MakeGenericMethod(targetType), sequence);

			if (asSingle)
			{
				analyze = Expression.Call(QueryEngine.FirstOrDefaultGeneric.MakeGenericMethod(targetType), analyze);
			}

			return analyze;
		}

		#endregion Visit Methods
	}
}
