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
using System.Reflection;

using JsonFx.Serialization;

namespace JsonFx.Linq
{
	/// <summary>
	/// Boiler-plate <see cref="IQueryProvider"/> implementation
	/// </summary>
	internal abstract class BaseQueryProvider : IQueryProvider, IQueryTextProvider
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		protected BaseQueryProvider()
		{
		}

		#endregion Init

		#region IQueryProvider Members

		IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
		{
			return new Query<S>(this, expression);
		}

		IQueryable IQueryProvider.CreateQuery(Expression expression)
		{
			Type elementType = TypeCoercionUtility.GetElementType(expression.Type) ?? expression.Type;

			try
			{
				// TODO: replace with DynamicMethodGenerator.GetTypeFactory?
				return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });
			}
			catch (TargetInvocationException ex)
			{
				// unwrap inner exception
				throw ex.InnerException;
			}
		}

		S IQueryProvider.Execute<S>(Expression expression)
		{
			return (S)(this.Execute(expression) ?? default(S));
		}

		object IQueryProvider.Execute(Expression expression)
		{
			return this.Execute(expression);
		}

		public abstract object Execute(Expression expression);

		#endregion IQueryProvider Members

		#region IQueryTextProvider Members

		public abstract string GetQueryText(Expression expression);

		#endregion IQueryTextProvider Members
	}
}
