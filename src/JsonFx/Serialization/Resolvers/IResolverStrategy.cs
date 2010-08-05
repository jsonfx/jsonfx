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
	/// Gets a delegate which determines if the property or field should not be serialized based upon its value.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public delegate bool ValueIgnoredDelegate(object instance, object memberValue);

	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter
	/// </summary>
	/// <remarks>
	/// Provides an extensibility point to control member naming and visibility at a very granular level.
	/// </remarks>
	public interface IResolverStrategy
	{
		#region Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="isImmutableType"></param>
		/// <returns></returns>
		bool IsPropertyIgnored(PropertyInfo member, bool isImmutableType);

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		bool IsFieldIgnored(FieldInfo member);

		/// <summary>
		/// Determines if the property or field should not be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="target"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		ValueIgnoredDelegate GetValueIgnoredCallback(MemberInfo member);

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		IEnumerable<DataName> GetName(MemberInfo member);

		/// <summary>
		/// Allows a strategy to perform a custom sort order to outputted members
		/// </summary>
		/// <param name="members"></param>
		/// <returns></returns>
		/// <remarks>
		/// A common usage is to ensure that Attributes sort first
		/// </remarks>
		IEnumerable<MemberMap> SortMembers(IEnumerable<MemberMap> members);

		#endregion Methods
	}
}
