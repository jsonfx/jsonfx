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
using System.Reflection;

namespace JsonFx.Serialization.Resolvers
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter by combining an ordered sequence of any other strategies
	/// </summary>
	/// <remarks>
	/// Each strategy is invoked in order, the first to respond wins.
	/// </remarks>
	public sealed class CombinedResolverStrategy : IResolverStrategy
	{
		#region Fields

		private readonly IEnumerable<IResolverStrategy> InnerStrategies;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="strategies">ordered sequence of strategies</param>
		public CombinedResolverStrategy(params IResolverStrategy[] strategies)
			: this((IEnumerable<IResolverStrategy>)strategies)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="strategies">ordered sequence of strategies</param>
		public CombinedResolverStrategy(IEnumerable<IResolverStrategy> strategies)
		{
			if (strategies == null)
			{
				throw new ArgumentNullException("strategies");
			}

			foreach (IResolverStrategy strategy in strategies)
			{
				if (strategies == null)
				{
					throw new ArgumentNullException("strategies");
				}
			}

			this.InnerStrategies = strategies;
		}

		#endregion Init

		#region Name Resolution Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="isImmutableType"></param>
		/// <returns>true if any strategy specifies should be ignored</returns>
		public bool IsPropertyIgnored(PropertyInfo member, bool isImmutableType)
		{
			foreach (IResolverStrategy strategy in this.InnerStrategies)
			{
				if (strategy.IsPropertyIgnored(member, isImmutableType))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns>true if any strategy specifies should be ignored</returns>
		public bool IsFieldIgnored(FieldInfo member)
		{
			foreach (IResolverStrategy strategy in this.InnerStrategies)
			{
				if (strategy.IsFieldIgnored(member))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets a delegate which determines if the property or field should not be serialized based upon its value.
		/// </summary>
		/// <param name="member"></param>
		/// <returns>true if any strategy specifies this should be ignored</returns>
		public ValueIgnoredDelegate GetValueIgnoredCallback(MemberInfo member)
		{
			List<ValueIgnoredDelegate> ignoredDelegates = new List<ValueIgnoredDelegate>();

			foreach (IResolverStrategy strategy in this.InnerStrategies)
			{
				ValueIgnoredDelegate ignoredDelegate = strategy.GetValueIgnoredCallback(member);
				if (ignoredDelegate != null)
				{
					ignoredDelegates.Add(ignoredDelegate);
				}
			}

			if (ignoredDelegates.Count < 1)
			{
				return null;
			}

			if (ignoredDelegates.Count == 1)
			{
				return ignoredDelegates[0];
			}

			return delegate(object instance, object memberValue)
			{
				foreach (ValueIgnoredDelegate ignoredDelegate in ignoredDelegates)
				{
					if (ignoredDelegate(instance, memberValue))
					{
						return true;
					}
				}

				return false;
			};
		}

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		/// <returns>custom name if any strategy specifies one, otherwise null</returns>
		public IEnumerable<DataName> GetName(MemberInfo member)
		{
			foreach (IResolverStrategy strategy in this.InnerStrategies)
			{
				foreach (DataName name in strategy.GetName(member))
				{
					if (!name.IsEmpty)
					{
						yield return name;
					}
				}
			}
		}

		/// <summary>
		/// Allows a strategy to perform a custom sort order to outputted members
		/// </summary>
		/// <param name="members"></param>
		/// <returns></returns>
		/// <remarks>
		/// A common usage is to ensure that Attributes sort first
		/// </remarks>
		public IEnumerable<MemberMap> SortMembers(IEnumerable<MemberMap> members)
		{
			foreach (IResolverStrategy strategy in this.InnerStrategies)
			{
				members = strategy.SortMembers(members) ?? members;
			}

			return members;
		}

		#endregion Name Resolution Methods
	}
}
