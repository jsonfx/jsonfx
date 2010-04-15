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

		internal bool IsIgnored(object value)
		{
			if ((value is PropertyInfo) && this.IsPropertyIgnored((PropertyInfo)value))
			{
				return true;
			}

			if ((value is FieldInfo) && this.IsFieldIgnored((FieldInfo)value))
			{
				return true;
			}

			return this.IsCustomIgnored(value);
		}

		protected virtual bool IsPropertyIgnored(PropertyInfo info)
		{
			return (!info.CanRead || !info.CanWrite);
		}

		protected virtual bool IsFieldIgnored(FieldInfo info)
		{
			return !info.IsPublic;
		}

		protected virtual bool IsCustomIgnored(object value)
		{
			// TODO: extend here for JsonIgnoreAttribute, XmlIgnoreAttribute, DataContractAttribute
			return (DataReaderSettings.GetAttribute<JsonIgnoreAttribute>(value) != null);
		}

		protected internal virtual string GetName(object value)
		{
			JsonNameAttribute attribute = DataReaderSettings.GetAttribute<JsonNameAttribute>(value);

			// TODO: extend here for JsonNameAttribute, XmlNameAttribute, DataContractAttribute
			return (attribute == null) ? attribute.Name : null;
		}

		#endregion Methods

		#region Methods

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
		/// Gets the attribute T for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <typeparam name="T">Attribute Type</typeparam>
		/// <returns>attribte</returns>
		private static T GetAttribute<T>(object value)
			where T : Attribute
		{
			if (value == null)
			{
				return default(T);
			}

			Type type = value.GetType();
			MemberInfo memberInfo = null;

			if (type.IsEnum)
			{
				string name = Enum.GetName(type, value);
				if (String.IsNullOrEmpty(name))
				{
					return default(T);
				}
				memberInfo = type.GetField(name);
			}
			else
			{
				memberInfo = value as MemberInfo;
			}

			if (memberInfo == null)
			{
				throw new ArgumentException("Value is not able to be reflected.", "value");
			}

			if (!Attribute.IsDefined(memberInfo, typeof(T)))
			{
				return default(T);
			}
			return (T)Attribute.GetCustomAttribute(memberInfo, typeof(T));
		}

		#endregion Methods
	}
}
