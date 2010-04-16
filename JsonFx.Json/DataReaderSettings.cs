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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace JsonFx.Json
{
	/// <summary>
	/// Controls deserialization settings for IDataReader
	/// </summary>
	public class DataReaderSettings
	{
		#region Fields

		private bool allowNullValueTypes = true;
		private readonly Dictionary<Type, Dictionary<string, MemberInfo>> MemberMapCache = new Dictionary<Type, Dictionary<string, MemberInfo>>();

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets and sets if ValueTypes can accept values of null
		/// </summary>
		/// <remarks>
		/// Only affects deserialization, if a ValueType T is assigned the
		/// value of null, it will receive the value of default(T).
		/// Setting this to false, throws an exception if null is
		/// specified for a ValueType member.
		/// </remarks>
		public bool AllowNullValueTypes
		{
			get { return this.allowNullValueTypes; }
			set { this.allowNullValueTypes = value; }
		}

		#endregion Properties

		#region Name Resolution Methods

		/// <summary>
		/// Determines if memberInfo is not to be serialized.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		internal bool IsIgnored(MemberInfo memberInfo)
		{
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if ((propertyInfo != null) && this.IsPropertyIgnored(propertyInfo))
			{
				return true;
			}

			FieldInfo fieldInfo = memberInfo as FieldInfo;
			if ((fieldInfo != null) && this.IsFieldIgnored(fieldInfo))
			{
				return true;
			}

			return this.IsCustomIgnored(memberInfo);
		}

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be read/write properties</remarks>
		protected virtual bool IsPropertyIgnored(PropertyInfo info)
		{
			return (!info.CanRead || !info.CanWrite);
		}

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be public field</remarks>
		protected virtual bool IsFieldIgnored(FieldInfo info)
		{
			return !info.IsPublic;
		}

		/// <summary>
		/// Gets a value indicating if the member is to be serialized.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks>default implementation checks for JsonIgnoreAttribute</remarks>
		protected virtual bool IsCustomIgnored(MemberInfo info)
		{
			// TODO: extend here for JsonIgnoreAttribute, XmlIgnoreAttribute, DataContractAttribute, JsonSpecifiedPropertyAttribute
			return (DataReaderSettings.GetAttribute<JsonIgnoreAttribute>(info) != null);
		}

		/// <summary>
		/// Gets the serialized name for the Enum value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		internal string GetName(Enum value)
		{
			return this.GetName(this.GetMemberInfo(value));
		}

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected internal virtual string GetName(MemberInfo value)
		{
			JsonNameAttribute attribute = DataReaderSettings.GetAttribute<JsonNameAttribute>(value);

			// TODO: extend here for JsonNameAttribute, XmlNameAttribute, DataContractAttribute
			return (attribute == null) ? attribute.Name : null;
		}

		#endregion Name Resolution Methods

		#region Reflection Methods

		internal Dictionary<string, MemberInfo> GetMemberMap(Type objectType)
		{
			// do not incurr the cost of member map for dictionaries
			if (typeof(IDictionary).IsAssignableFrom(objectType))
			{
				return null;
			}

			if (this.MemberMapCache.ContainsKey(objectType))
			{
				// map was stored in cache
				return this.MemberMapCache[objectType];
			}

			// create a new map
			Dictionary<string, MemberInfo> memberMap = new Dictionary<string, MemberInfo>();

			// load properties into property map
			PropertyInfo[] properties = objectType.GetProperties();
			foreach (PropertyInfo info in properties)
			{
				if (this.IsIgnored(info))
				{
					continue;
				}

				string name = this.GetName(info);
				if (String.IsNullOrEmpty(name))
				{
					memberMap[info.Name] = info;
				}
				else
				{
					memberMap[name] = info;
				}
			}

			// load public fields into property map
			FieldInfo[] fields = objectType.GetFields();
			foreach (FieldInfo info in fields)
			{
				if (this.IsIgnored(info))
				{
					continue;
				}

				string name = this.GetName(info);
				if (String.IsNullOrEmpty(name))
				{
					memberMap[info.Name] = info;
				}
				else
				{
					memberMap[name] = info;
				}
			}

			// store in cache for future usage
			this.MemberMapCache[objectType] = memberMap;

			return memberMap;
		}

		/// <summary>
		/// Gets the Enum field for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private MemberInfo GetMemberInfo(Enum value)
		{
			Type type = value.GetType();

			string name = Enum.GetName(type, value);
			if (String.IsNullOrEmpty(name))
			{
				return null;
			}

			return type.GetField(name);
		}

		/// <summary>
		/// Gets the attribute T for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <typeparam name="T">Attribute Type</typeparam>
		/// <returns>attribte</returns>
		protected static T GetAttribute<T>(MemberInfo memberInfo)
			where T : Attribute
		{
			if (memberInfo == null || !Attribute.IsDefined(memberInfo, typeof(T)))
			{
				return default(T);
			}
			return (T)Attribute.GetCustomAttribute(memberInfo, typeof(T));
		}

		#endregion Reflection Methods
	}
}
