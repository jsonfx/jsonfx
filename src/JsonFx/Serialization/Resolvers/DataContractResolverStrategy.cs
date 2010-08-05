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

using JsonFx.CodeGen;

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
		#region Fields

		private const string DataContractAssemblyName = "System.Runtime.Serialization";
		private const string DataContractTypeName = "System.Runtime.Serialization.DataContractAttribute";
		private const string DataMemberTypeName = "System.Runtime.Serialization.DataMemberAttribute";
		private const string IgnoreDataMemberTypeName = "System.Runtime.Serialization.IgnoreDataMemberAttribute";

		private static readonly Type DataContractType;
		private static readonly Type DataMemberType;
		private static readonly Type IgnoreDataMemberType;

		private static readonly GetterDelegate DataContractNameGetter;
		private static readonly GetterDelegate DataContractNamespaceGetter;
		private static readonly GetterDelegate DataMemberNameGetter;

		#endregion Fields

		#region Init

		/// <summary>
		/// CCtor
		/// </summary>
		static DataContractResolverStrategy()
		{
			string[] assemblyName = typeof(Object).Assembly.FullName.Split(',');
			assemblyName[0] = DataContractAssemblyName;

			Assembly assembly = Assembly.Load(String.Join(",", assemblyName));

			DataContractType = assembly.GetType(DataContractTypeName);
			DataMemberType = assembly.GetType(DataMemberTypeName);
			IgnoreDataMemberType = assembly.GetType(IgnoreDataMemberTypeName);

			if (DataContractType != null)
			{
				PropertyInfo property = DataContractType.GetProperty("Name", BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
				DataContractNameGetter = DynamicMethodGenerator.GetPropertyGetter(property);
				property = DataContractType.GetProperty("Namespace", BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
				DataContractNamespaceGetter = DynamicMethodGenerator.GetPropertyGetter(property);
			}

			if (DataContractResolverStrategy.DataMemberType != null)
			{
				PropertyInfo property = DataMemberType.GetProperty("Name", BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
				DataMemberNameGetter = DynamicMethodGenerator.GetPropertyGetter(property);
			}
		}

		#endregion Init

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

			if (TypeCoercionUtility.HasAttribute(objType, DataContractResolverStrategy.DataContractType))
			{
				// use DataContract rules: member must be marked and not ignored
				return
					!TypeCoercionUtility.HasAttribute(member, DataContractResolverStrategy.DataMemberType) ||
					TypeCoercionUtility.HasAttribute(member, DataContractResolverStrategy.IgnoreDataMemberType);
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

			if (TypeCoercionUtility.HasAttribute(objType, DataContractResolverStrategy.DataContractType))
			{
				// use DataContract rules: member must be marked and not ignored
				return
					!TypeCoercionUtility.HasAttribute(member, DataContractResolverStrategy.DataMemberType) ||
					TypeCoercionUtility.HasAttribute(member, DataContractResolverStrategy.IgnoreDataMemberType);
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
		public override IEnumerable<DataName> GetName(MemberInfo member)
		{
			string localName, ns;
			Attribute typeAttr;
			if (member is Type)
			{
				typeAttr = TypeCoercionUtility.GetAttribute(member, DataContractResolverStrategy.DataContractType);
				if (typeAttr == null)
				{
					yield break;
				}

				localName = (string)DataContractResolverStrategy.DataContractNameGetter(typeAttr);
				ns = (string)DataContractResolverStrategy.DataContractNamespaceGetter(typeAttr);

				if (!String.IsNullOrEmpty(localName))
				{
					yield return new DataName(localName, null, ns);
				}
				yield break;
			}

			typeAttr = TypeCoercionUtility.GetAttribute(member.DeclaringType, DataContractResolverStrategy.DataContractType);
			if (typeAttr == null)
			{
				yield break;
			}

			ns = (string)DataContractResolverStrategy.DataContractNamespaceGetter(typeAttr);

			Attribute memberAttr = TypeCoercionUtility.GetAttribute(member, DataContractResolverStrategy.DataMemberType);
			if (memberAttr == null)
			{
				yield break;
			}

			localName = (string)DataContractResolverStrategy.DataMemberNameGetter(memberAttr);

			if (!String.IsNullOrEmpty(localName))
			{
				// members inherit DataContract namespaces
				yield return new DataName(localName, null, ns);
			}
		}

		#endregion Name Resolution Methods
	}
}
