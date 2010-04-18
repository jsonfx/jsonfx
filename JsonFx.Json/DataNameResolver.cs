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
using System.ComponentModel;
using System.Reflection;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter
	/// </summary>
	public class DataNameResolver
	{
		#region Constants

		private const string AnonymousTypePrefix = "<>f__AnonymousType";

		#endregion Constants

		#region Fields

		private readonly object SyncLock = new object();
		private readonly IDictionary<Type, IDictionary<string, MemberInfo>> ReadMapCache = new Dictionary<Type, IDictionary<string, MemberInfo>>();
		private readonly IDictionary<Type, IDictionary<MemberInfo, string>> WriteMapCache = new Dictionary<Type, IDictionary<MemberInfo, string>>();

		#endregion Fields

		#region Name Resolution Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be read/write properties, or read-only anonymous</remarks>
		protected virtual bool IsPropertyIgnored(PropertyInfo info, bool isAnonymousType)
		{
			return (!info.CanRead || (!info.CanWrite && !isAnonymousType));
		}

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be public, non-readonly field</remarks>
		protected virtual bool IsFieldIgnored(FieldInfo info)
		{
			return (!info.IsPublic || info.IsStatic || info.IsInitOnly);
		}

		/// <summary>
		/// Gets a value indicating if the member is to be serialized.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks>default implementation checks for JsonIgnoreAttribute</remarks>
		protected virtual bool IsIgnored(MemberInfo info)
		{
			if (DataNameResolver.GetAttribute<DataIgnoreAttribute>(info) != null)
			{
				return true;
			}

			// TODO: extend here for JsonIgnoreAttribute, XmlIgnoreAttribute, DataContractAttribute, JsonSpecifiedPropertyAttribute
			//if (DataReaderSettings.GetAttribute<XmlIgnoreAttribute>(info) != null)
			//{
			//    return true;
			//}

			return false;
		}

		/// <summary>
		/// Determines if the property or field should not be serialized.
		/// </summary>
		/// <param name="objType"></param>
		/// <param name="member"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected internal virtual bool IsValueIgnored(MemberInfo info, object target, out object value)
		{
			value = DataNameResolver.GetMemberValue(target, info);
			if (this.IsDefaultValue(info, value))
			{
				return true;
			}

			Type objType = info.ReflectedType ?? info.DeclaringType;

			DataSpecifiedPropertyAttribute specifiedProperty = DataNameResolver.GetAttribute<DataSpecifiedPropertyAttribute>(info);
			if (!String.IsNullOrEmpty(specifiedProperty.SpecifiedProperty))
			{
				PropertyInfo specProp = objType.GetProperty(specifiedProperty.SpecifiedProperty);
				if (specProp != null)
				{
					object isSpecified = specProp.GetValue(target, null);
					if (isSpecified is Boolean && !Convert.ToBoolean(isSpecified))
					{
						return true;
					}
				}
			}

			//if (this.UseXmlSerializationAttributes)
			//{
			//    PropertyInfo specProp = objType.GetProperty(info.Name+"Specified");
			//    if (specProp != null)
			//    {
			//        object isSpecified = specProp.GetValue(obj, null);
			//        if (isSpecified is Boolean && !Convert.ToBoolean(isSpecified))
			//        {
			//            return true;
			//        }
			//    }
			//}

			return false;
		}

		/// <summary>
		/// Determines if the member value matches the DefaultValue attribute
		/// </summary>
		/// <returns>if has a value equivalent to the DefaultValueAttribute</returns>
		protected bool IsDefaultValue(MemberInfo member, object value)
		{
			DefaultValueAttribute attribute = DataNameResolver.GetAttribute<DefaultValueAttribute>(member);
			if (attribute == null)
			{
				return false;
			}

			return (attribute.Value == value);
		}

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected internal virtual string GetName(MemberInfo value)
		{
			DataNameAttribute attribute = DataNameResolver.GetAttribute<DataNameAttribute>(value);

			// TODO: extend here for XmlNameAttribute, DataContractAttribute
			return (attribute != null) ? attribute.Name : null;
		}

		/// <summary>
		/// Gets the serialized name for the Enum value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		internal string GetName(Enum value)
		{
			return this.GetName(DataNameResolver.GetMemberInfo(value));
		}

		#endregion Name Resolution Methods

		#region Map Methods

		internal IDictionary<string, MemberInfo> GetReadMap(Type objectType)
		{
			lock (this.SyncLock)
			{
				if (!this.ReadMapCache.ContainsKey(objectType))
				{
					this.CreateMaps(objectType);
				}

				// map was stored in cache
				return this.ReadMapCache[objectType];
			}
		}

		internal IDictionary<MemberInfo, string> GetWriteMap(Type objectType)
		{
			lock (this.SyncLock)
			{
				if (!this.WriteMapCache.ContainsKey(objectType))
				{
					this.CreateMaps(objectType);
				}

				// map was stored in cache
				return this.WriteMapCache[objectType];
			}
		}

		/// <summary>
		/// Removes any cached member mappings.
		/// </summary>
		public void Clear()
		{
			lock (this.SyncLock)
			{
				this.ReadMapCache.Clear();
				this.WriteMapCache.Clear();
			}
		}

		/// <summary>
		/// Builds a mapping of member name to field/property
		/// </summary>
		/// <param name="objectType"></param>
		private void CreateMaps(Type objectType)
		{
			lock (this.SyncLock)
			{
				// do not incurr the cost of member map for dictionaries
				if (typeof(IDictionary).IsAssignableFrom(objectType))
				{
					// store in cache for future usage
					this.ReadMapCache[objectType] = null;
					this.WriteMapCache[objectType] = null;
					return;
				}

				// create new maps
				IDictionary<string, MemberInfo> readMap = new Dictionary<string, MemberInfo>();
				IDictionary<MemberInfo, string> writeMap = new Dictionary<MemberInfo, string>();

				bool isAnonymousType = objectType.IsGenericType && objectType.Name.StartsWith(DataNameResolver.AnonymousTypePrefix);

				// load properties into property map
				foreach (PropertyInfo info in objectType.GetProperties())
				{
					if (this.IsPropertyIgnored(info, isAnonymousType) ||
						this.IsIgnored(info))
					{
						continue;
					}

					string name = this.GetName(info);
					if (String.IsNullOrEmpty(name))
					{
						name = info.Name;
					}

					readMap[info.Name] = info;
					writeMap[info] = name;
				}

				// load fields into property map
				foreach (FieldInfo info in objectType.GetFields())
				{
					if (this.IsFieldIgnored(info) ||
						this.IsIgnored(info))
					{
						continue;
					}

					string name = this.GetName(info);
					if (String.IsNullOrEmpty(name))
					{
						name = info.Name;
					}

					readMap[name] = info;
					writeMap[info] = name;
				}

				// store in cache for future usage
				this.ReadMapCache[objectType] = readMap;
				this.WriteMapCache[objectType] = writeMap;
			}
		}

		#endregion Map Methods

		#region Reflection Methods

		/// <summary>
		/// Gets the attribute T for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <typeparam name="T">Attribute Type</typeparam>
		/// <returns>attribte</returns>
		protected static T GetAttribute<T>(MemberInfo info)
			where T : Attribute
		{
			if (info == null || !Attribute.IsDefined(info, typeof(T)))
			{
				return default(T);
			}
			return (T)Attribute.GetCustomAttribute(info, typeof(T));
		}

		private static object GetMemberValue(object target, MemberInfo info)
		{
			PropertyInfo propertyInfo = info as PropertyInfo;
			if (propertyInfo != null && propertyInfo.CanRead)
			{
				return propertyInfo.GetValue(target, null);
			}

			FieldInfo fieldInfo = info as FieldInfo;
			if (fieldInfo != null)
			{
				return fieldInfo.GetValue(target);
			}

			return null;
		}

		/// <summary>
		/// Gets the Enum field for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static MemberInfo GetMemberInfo(Enum value)
		{
			Type type = value.GetType();

			string name = Enum.GetName(type, value);
			if (String.IsNullOrEmpty(name))
			{
				return null;
			}

			return type.GetField(name);
		}

		#endregion Reflection Methods
	}
}
