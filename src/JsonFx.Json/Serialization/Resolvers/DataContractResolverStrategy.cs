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

namespace JsonFx.Serialization.Resolvers
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter using DataContract attributes
	/// </summary>
	/// <remarks>
	/// http://msdn.microsoft.com/en-us/library/kd1dc9w5.aspx
	/// </remarks>
	public class DataContractResolverStrategy : PocoResolverStrategy
	{
		#region Name Resolution Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="isImmutableType"></param>
		/// <returns></returns>
		public override bool IsPropertyIgnored(PropertyInfo member, bool isImmutableType)
		{
			Type objType = member.ReflectedType ?? member.DeclaringType;

			if (TypeCoercionUtility.HasAttribute<DataContractAttribute>(objType))
			{
				// use DataContract rules: member must be marked and not ignored
				return
					!TypeCoercionUtility.HasAttribute<DataMemberAttribute>(member) ||
					TypeCoercionUtility.HasAttribute<IgnoreDataMemberAttribute>(member);
			}

			// use POCO rules: must be public read/write (or anonymous object)
			return base.IsPropertyIgnored(member, isImmutableType);
		}

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public override bool IsFieldIgnored(FieldInfo member)
		{
			Type objType = member.ReflectedType ?? member.DeclaringType;

			if (TypeCoercionUtility.HasAttribute<DataContractAttribute>(objType))
			{
				// use DataContract rules: member must be marked and not ignored
				return
					!TypeCoercionUtility.HasAttribute<DataMemberAttribute>(member) ||
					TypeCoercionUtility.HasAttribute<IgnoreDataMemberAttribute>(member);
			}

			// use POCO rules: must be public read/write
			return base.IsFieldIgnored(member);
		}

		/// <summary>
		/// Gets a delegate which determines if the property or field should not be serialized based upon its value.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public override ValueIgnoredDelegate GetValueIgnoredCallback(MemberInfo member)
		{
			return null;
		}

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public override DataName GetName(MemberInfo member)
		{
			if (member is Type)
			{
				DataContractAttribute typeAttr = TypeCoercionUtility.GetAttribute<DataContractAttribute>(member);

				return (typeAttr != null && !String.IsNullOrEmpty(typeAttr.Name)) ? new DataName(typeAttr.Name, typeAttr.Namespace) : null;
			}

			DataMemberAttribute memberAttr = TypeCoercionUtility.GetAttribute<DataMemberAttribute>(member);

			return (memberAttr != null && !String.IsNullOrEmpty(memberAttr.Name)) ? new DataName(memberAttr.Name) : null;
		}

		#endregion Name Resolution Methods
	}
}
