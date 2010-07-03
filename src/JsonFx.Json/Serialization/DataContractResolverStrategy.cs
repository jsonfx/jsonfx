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
using System.Runtime.Serialization;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter using DataContract attributes
	/// </summary>
	public class DataContractResolverStrategy : IResolverStrategy
	{
		#region Name Resolution Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="isAnonymousType"></param>
		/// <returns></returns>
		public virtual bool IsPropertyIgnored(PropertyInfo member, bool isAnonymousType)
		{
			if ((TypeCoercionUtility.GetAttribute<DataContractAttribute>(member.DeclaringType) == null) &&
				(TypeCoercionUtility.GetAttribute<DataMemberAttribute>(member) == null))
			{
				return true;
			}

			if (!member.CanRead || (!member.CanWrite && !isAnonymousType))
			{
				return true;
			}

			return TypeCoercionUtility.HasAttribute<IgnoreDataMemberAttribute>(member);
		}

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public virtual bool IsFieldIgnored(FieldInfo member)
		{
			if ((TypeCoercionUtility.GetAttribute<DataContractAttribute>(member.DeclaringType) == null) &&
				(TypeCoercionUtility.GetAttribute<DataMemberAttribute>(member) == null))
			{
				return true;
			}

			if (!member.IsPublic || (member.IsStatic != member.DeclaringType.IsEnum) || member.IsInitOnly)
			{
				return true;
			}

			return TypeCoercionUtility.HasAttribute<IgnoreDataMemberAttribute>(member);
		}

		/// <summary>
		/// Gets a delegate which determines if the property or field should not be serialized based upon its value.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public virtual ValueIgnoredDelegate GetValueIgnored(MemberInfo member)
		{
			return null;
		}

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public virtual string GetName(MemberInfo member)
		{
			DataMemberAttribute attribute = TypeCoercionUtility.GetAttribute<DataMemberAttribute>(member);

			return (attribute != null) ? attribute.Name : null;
		}

		#endregion Name Resolution Methods
	}
}
