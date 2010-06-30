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
using System.Reflection;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter
	/// </summary>
	/// <remarks>
	/// Provides an extensibility point to control member naming at a very granular level.
	/// </remarks>
	public interface IDataNameResolver
	{
		#region Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="isAnonymousType"></param>
		/// <returns></returns>
		bool IsPropertyIgnored(PropertyInfo member, bool isAnonymousType);

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		bool IsFieldIgnored(FieldInfo member);

		/// <summary>
		/// Gets a value indicating if the member is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		bool IsIgnored(MemberInfo member);

		/// <summary>
		/// Determines if the property or field should not be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="target"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool IsValueIgnored(MemberInfo member, object target, out object value);

		/// <summary>
		/// Determines if the member value matches the DefaultValue attribute
		/// </summary>
		/// <param name="member"></param>
		/// <param name="value"></param>
		/// <returns>if has a value equivalent to the DefaultValueAttribute</returns>
		bool IsDefaultValue(MemberInfo member, object value);

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		string GetName(MemberInfo member);

		/// <summary>
		/// Gets the serialized name for the Enum value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		string GetName(Enum value);

		#endregion Methods
	}
}
