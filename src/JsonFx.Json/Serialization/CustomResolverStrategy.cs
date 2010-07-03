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
using System.ComponentModel;
using System.Reflection;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter by using a decorator pattern around any other strategy
	/// </summary>
	/// <remarks>
	/// Provides an extensibility point to control member naming and visibility at a very granular level.
	/// </remarks>
	public sealed class CustomResolverStrategy : IResolverStrategy
	{
		#region Fields

		private readonly IResolverStrategy InnerStrategy;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="strategy"></param>
		public CustomResolverStrategy(IResolverStrategy strategy)
		{
			this.InnerStrategy = strategy;
		}

		#endregion Init

		#region Properties

		public delegate bool PropertyIgnoredDelegate(PropertyInfo propertyInfo, bool isAnonymous);

		public PropertyIgnoredDelegate IsPropertyIgnoredCustom
		{
			get;
			set;
		}

		public delegate bool FieldIgnoredDelegate(FieldInfo fieldInfo);

		public FieldIgnoredDelegate IsFieldIgnoredCustom
		{
			get;
			set;
		}

		public delegate bool ValueIgnoredDelegate(MemberInfo memberInfo, object target, object value);

		public ValueIgnoredDelegate IsValueIgnoredCustom
		{
			get;
			set;
		}

		public delegate string GetNameDelegate(MemberInfo memberInfo);

		public GetNameDelegate GetNameCustom
		{
			get;
			set;
		}

		#endregion Properties

		#region Name Resolution Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="isAnonymousType"></param>
		/// <returns></returns>
		public bool IsPropertyIgnored(PropertyInfo member, bool isAnonymousType)
		{
			if (this.IsPropertyIgnoredCustom != null)
			{
				return this.IsPropertyIgnoredCustom(member, isAnonymousType);
			}

			return this.InnerStrategy.IsPropertyIgnored(member, isAnonymousType);
		}

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public bool IsFieldIgnored(FieldInfo member)
		{
			if (this.IsFieldIgnoredCustom != null)
			{
				return this.IsFieldIgnoredCustom(member);
			}

			return this.InnerStrategy.IsFieldIgnored(member);
		}

		/// <summary>
		/// Determines if the property or field should not be serialized based upon its value.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="target"></param>
		/// <param name="value"></param>
		/// <returns>if has a value equivalent to the DefaultValueAttribute</returns>
		public bool IsValueIgnored(MemberInfo member, object target, object value)
		{
			if (this.IsValueIgnoredCustom != null)
			{
				return this.IsValueIgnoredCustom(member, target, value);
			}

			return this.InnerStrategy.IsValueIgnored(member, target, value);
		}

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public string GetName(MemberInfo member)
		{
			if (this.GetNameCustom != null)
			{
				return this.GetNameCustom(member);
			}

			return this.InnerStrategy.GetName(member);
		}

		#endregion Name Resolution Methods
	}
}
