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
	/// Controls name resolution for IDataReader / IDataWriter using plain old CLR object (POCO) names
	/// </summary>
	public class PocoResolverStrategy : IResolverStrategy
	{
		#region Name Resolution Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="isImmutableType"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be read/write properties, or immutable</remarks>
		public virtual bool IsPropertyIgnored(PropertyInfo member, bool isImmutableType)
		{
			// must be public read/write (or anonymous object)
			MethodInfo getter = member.CanRead ? member.GetGetMethod() : null;
			MethodInfo setter = member.CanWrite ? member.GetSetMethod() : null;

			return
				(getter == null || !getter.IsPublic) ||
				(!isImmutableType && (setter == null || !setter.IsPublic));
		}

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be public, non-readonly field</remarks>
		public virtual bool IsFieldIgnored(FieldInfo member)
		{
			// must be public read/write
			return (!member.IsPublic || member.IsInitOnly);
		}

		/// <summary>
		/// Gets a delegate which determines if the property or field should not be serialized based upon its value.
		/// </summary>
		/// <param name="member"></param>
		/// <returns>if has a value equivalent to the DefaultValueAttribute</returns>
		/// <remarks>
		/// This is useful when default values need not be serialized.
		/// </remarks>
		public virtual ValueIgnoredDelegate GetValueIgnoredCallback(MemberInfo member)
		{
			return null;
		}

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public virtual IEnumerable<DataName> GetName(MemberInfo member)
		{
			return null;
		}

		/// <summary>
		/// Allows a strategy to perform a custom sort order to outputted members
		/// </summary>
		/// <param name="members"></param>
		/// <returns></returns>
		/// <remarks>
		/// A common usage is to ensure that Attributes sort first
		/// </remarks>
		public virtual IEnumerable<MemberMap> SortMembers(IEnumerable<MemberMap> members)
		{
			return members;
		}

		#endregion Name Resolution Methods
	}
}
