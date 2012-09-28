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
using System.Globalization;
using System.Reflection;

using JsonFx.CodeGen;
using JsonFx.Serialization.Resolvers;

#if !NET20 && !NET30 && !WINDOWS_PHONE
using System.Linq;
#endif

#if NET40 && !WINDOWS_PHONE
using JsonObject=System.Dynamic.ExpandoObject;
#else
using JsonObject=System.Collections.Generic.Dictionary<string, object>;
#endif

namespace JsonFx.Serialization
{
	/// <summary>
	/// Type Coercion Utility
	/// </summary>
	public sealed class TypeCoercionUtility :
		IResolverCacheContainer
	{
		#region Constants

		internal const string AnonymousTypePrefix = "<>f__AnonymousType";

		internal static readonly string TypeGenericIEnumerable = typeof(IEnumerable<>).FullName;
		internal static readonly string TypeGenericICollection = typeof(ICollection<>).FullName;
		internal static readonly string TypeGenericIDictionary = typeof(IDictionary<,>).FullName;

		private const string ErrorNullValueType = "{0} does not accept null as a value";
		private const string ErrorCtor = "Unable to find a suitable constructor for instantiating the target Type. ({0})";
		private const string ErrorCannotInstantiateAsT = "Type {0} is not of Type {1}";

		private const string ErrorGenericIDictionary = "Types which implement Generic IDictionary<TKey, TValue> also need to implement IDictionary to be deserialized. ({0})";
		private const string ErrorGenericIDictionaryKeys = "Types which implement Generic IDictionary<TKey, TValue> need to have string keys to be deserialized. ({0})";

		#endregion Constants

		#region Fields

		private readonly bool AllowNullValueTypes;
		private readonly ResolverCache ResolverCache;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="cacheContainer"></param>
		/// <param name="allowNullValueTypes"></param>
		public TypeCoercionUtility(IResolverCacheContainer cacheContainer, bool allowNullValueTypes)
			: this(cacheContainer.ResolverCache, allowNullValueTypes)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="resolverCache"></param>
		/// <param name="allowNullValueTypes"></param>
		public TypeCoercionUtility(ResolverCache resolverCache, bool allowNullValueTypes)
		{
			if (resolverCache == null)
			{
				throw new ArgumentNullException("resolverCache");
			}
			this.ResolverCache = resolverCache;
			this.AllowNullValueTypes = allowNullValueTypes;
		}

		#endregion Init

		#region Object Manipulation Methods

		/// <summary>
		/// Instantiates a new instance of objectType.
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns>objectType instance</returns>
		internal object InstantiateObjectDefaultCtor(Type targetType)
		{
			if (targetType == null ||
				targetType.IsValueType ||
				targetType.IsAbstract ||
				targetType == typeof(object) ||
				targetType == typeof(string))
			{
				return new JsonObject();
			}

			targetType = TypeCoercionUtility.ResolveInterfaceType(targetType);

			if (targetType.IsInterface)
			{
				return new JsonObject();
			}

			FactoryMap factory = this.ResolverCache.LoadFactory(targetType);
			if ((factory == null) || (factory.Ctor == null) || ((factory.CtorArgs != null) && (factory.CtorArgs.Length > 0)))
			{
				return new JsonObject();
			}

			// default constructor
			return factory.Ctor();
		}

		/// <summary>
		/// Instantiates a new instance of objectType.
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns>objectType instance</returns>
		internal object InstantiateObject(Type targetType, object args)
		{
			targetType = TypeCoercionUtility.ResolveInterfaceType(targetType);

			FactoryMap factory = this.ResolverCache.LoadFactory(targetType);
			if ((factory == null) || (factory.Ctor == null))
			{
				throw new TypeCoercionException(String.Format(
					TypeCoercionUtility.ErrorCtor,
					targetType.FullName));
			}

			if ((factory.CtorArgs == null) || (factory.CtorArgs.Length < 1))
			{
				// default constructor
				return factory.Ctor();
			}

			object[] ctorArgs = new object[factory.CtorArgs.Length];

			IDictionary<string, object> genericArgs = args as IDictionary<string, object>;
			if (genericArgs != null)
			{
				for (int i=0, length=ctorArgs.Length; i<length; i++)
				{
					string name = factory.CtorArgs[i].Name;
					Type type = factory.CtorArgs[i].ParameterType;

					foreach (string key in genericArgs.Keys)
					{
						try
						{
							if (StringComparer.OrdinalIgnoreCase.Equals(key, name))
							{
								ctorArgs[i] = this.CoerceType(type, genericArgs[key]);
								break;
							}
						}
						catch { }
					}
				}
			}
			else
			{
				IDictionary otherArgs = args as IDictionary;
				if (otherArgs != null)
				{
					for (int i=0, length=ctorArgs.Length; i<length; i++)
					{
						string name = factory.CtorArgs[i].Name;
						Type type = factory.CtorArgs[i].ParameterType;

						foreach (string key in otherArgs.Keys)
						{
							try
							{
								if (StringComparer.OrdinalIgnoreCase.Equals(key, name))
								{
									ctorArgs[i] = this.CoerceType(type, otherArgs[key]);
									break;
								}
							}
							catch { }
						}
					}
				}
			}

			// use a custom constructor
			return factory.Ctor(ctorArgs);
		}

		/// <summary>
		/// Helper method to set value of a member.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="targetType"></param>
		/// <param name="memberMap"></param>
		/// <param name="memberName"></param>
		/// <param name="memberValue"></param>
		internal void SetMemberValue(object target, Type targetType, MemberMap memberMap, string memberName, object memberValue)
		{
			if (target == null)
			{
				return;
			}

			if (target is IDictionary<string, object>)
			{
				// needed for ExpandoObject which unfortunately does not implement IDictionary
				((IDictionary<string, object>)target)[memberName] = memberValue;
			}
			else if (target is IDictionary)
			{
				((IDictionary)target)[memberName] = memberValue;
			}
#if NET40 && !WINDOWS_PHONE
			else if (target is System.Dynamic.DynamicObject)
			{
				// TODO: expand to all IDynamicMetaObjectProvider?
				((System.Dynamic.DynamicObject)target).TrySetMember(new DynamicSetter(memberName), memberValue);
			}
#endif
			else if (targetType != null
#if !NETCF
			&& targetType.GetInterface(TypeCoercionUtility.TypeGenericIDictionary, false) != null
#endif
			)
			{
				throw new TypeCoercionException(String.Format(
					TypeCoercionUtility.ErrorGenericIDictionary,
					targetType));
			}
			else if (memberMap != null && memberMap.Setter != null)
			{
				if (memberValue == null)
				{
					memberMap.Setter(target,
						memberMap.Type.IsValueType ?
						Activator.CreateInstance(memberMap.Type, true) :
						null);
				}
				else
				{
					memberMap.Setter(target, this.CoerceType(memberMap.Type, memberValue));
				}
			}

			// ignore non-applicable members
		}

		#endregion Object Manipulation Methods

		#region Coercion Methods

		/// <summary>
		/// Coerces the object value to Type <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public T CoerceType<T>(object value)
		{
			return (T)this.CoerceType(typeof(T), value);
		}

		/// <summary>
		/// Coerces the object value to Type of <paramref name="targetType"/>
		/// </summary>
		/// <param name="targetType"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public object CoerceType(Type targetType, object value)
		{
			if (targetType == null || targetType == typeof(object))
			{
				return value;
			}

			bool isNullable = TypeCoercionUtility.IsNullable(targetType);
			if (value == null)
			{
				// TODO: validate that this is what we want

				if (!this.AllowNullValueTypes &&
					targetType.IsValueType &&
					!isNullable)
				{
					throw new TypeCoercionException(String.Format(
						TypeCoercionUtility.ErrorNullValueType,
						targetType.FullName));
				}
				return value;
			}

			if (isNullable)
			{
				// nullable types have a real underlying struct
				Type[] genericArgs = targetType.GetGenericArguments();
				if (genericArgs.Length == 1)
				{
					targetType = genericArgs[0];
				}
			}

			Type actualType = value.GetType();
			if (targetType.IsAssignableFrom(actualType))
			{
				return value;
			}

			if (targetType.IsEnum)
			{
				if (value is String)
				{
					if (!Enum.IsDefined(targetType, value))
					{
						IDictionary<string, MemberMap> maps = this.ResolverCache.LoadMaps(targetType);
						if (maps != null && maps.ContainsKey((string)value))
						{
							value = maps[(string)value].Name;
						}
					}

					return Enum.Parse(targetType, (string)value, false);
				}
				else
				{
					value = this.CoerceType(Enum.GetUnderlyingType(targetType), value);
					return Enum.ToObject(targetType, value);
				}
			}

			if (value is IDictionary<string, object>)
			{
				return this.CoerceType(targetType, (IDictionary<string, object>)value);
			}
			else if (value is IDictionary)
			{
				return this.CoerceType(targetType, (IDictionary)value);
			}

			if (typeof(IEnumerable).IsAssignableFrom(targetType) &&
				typeof(IEnumerable).IsAssignableFrom(actualType))
			{
				return this.CoerceList(targetType, actualType, (IEnumerable)value);
			}

			if (value is String)
			{
				if (targetType == typeof(DateTime))
				{
					DateTime date;
					try{
					    date = DateTime.Parse(
						(string)value,
						CultureInfo.InvariantCulture,
						DateTimeStyles.RoundtripKind|DateTimeStyles.AllowWhiteSpaces|DateTimeStyles.NoCurrentDateDefault);
						if (date.Kind == DateTimeKind.Local)
						{
							return date.ToUniversalTime();
						}
						return date;
					}catch(Exception){}

					if (JsonFx.Model.Filters.MSAjaxDateFilter.TryParseMSAjaxDate(
						(string)value,
						out date))
					{
						return date;
					}
				}
				else if (targetType == typeof(Guid))
				{
					// try-catch is pointless since will throw upon generic conversion
					return new Guid((string)value);
				}
				else if (targetType == typeof(Char))
				{
					if (((string)value).Length == 1)
					{
						return ((string)value)[0];
					}
				}
				else if (targetType == typeof(Uri))
				{
					Uri uri;
					if (Uri.TryCreate((string)value, UriKind.RelativeOrAbsolute, out uri))
					{
						return uri;
					}
				}
				else if (targetType == typeof(Version))
				{
					// try-catch is pointless since will throw upon generic conversion
					return new Version((string)value);
				}
				else if (targetType == typeof(TimeSpan))
				{
					try{
					    return TimeSpan.FromTicks(Int64.Parse((string)value));
					}catch(Exception){}
					try{
					    return TimeSpan.Parse((string)value);
					}catch(Exception){}
				}
			}
			else if (targetType == typeof(TimeSpan))
			{
				return new TimeSpan((long)this.CoerceType(typeof(Int64), value));
			}

#if !SILVERLIGHT
			TypeConverter converter = TypeDescriptor.GetConverter(targetType);
			if (converter.CanConvertFrom(actualType))
			{
				return converter.ConvertFrom(value);
			}

			converter = TypeDescriptor.GetConverter(actualType);
			if (converter.CanConvertTo(targetType))
			{
				return converter.ConvertTo(value, targetType);
			}
#endif

			try
			{
				// fall back to basics
				return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw new TypeCoercionException(
					String.Format(
						"Error converting {0} to {1}",
						value.GetType().FullName,
						targetType.FullName),
					ex);
			}
		}

		/// <summary>
		/// Populates the properties of an object with the dictionary values.
		/// </summary>
		/// <param name="targetType"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private object CoerceType(Type targetType, IDictionary<string, object> value)
		{
			object newValue = this.InstantiateObject(targetType, value);

			IDictionary<string, MemberMap> maps = this.ResolverCache.LoadMaps(targetType);
			if (maps == null)
			{
				IDictionary<string, object> genericDictionary = newValue as IDictionary<string, object>;
				if (genericDictionary != null)
				{
					// copy all values into new object
					foreach (string memberName in value.Keys)
					{
						genericDictionary[memberName] = value[memberName];
					}
				}
				else
				{
					IDictionary dictionary = newValue as IDictionary;
					if (dictionary != null)
					{
						// copy all values into new object
						foreach (string memberName in value.Keys)
						{
							dictionary[memberName] = value[memberName];
						}
					}
				}
			}
			else
			{
				// copy any values into new object
				foreach (string memberName in value.Keys)
				{
					MemberMap map;

					if (maps.TryGetValue(memberName, out map) &&
						map != null && map.Setter != null)
					{
						map.Setter(newValue, value[memberName]);
					}
				}
			}

			return newValue;
		}

		/// <summary>
		/// Populates the properties of an object with the dictionary values.
		/// </summary>
		/// <param name="targetType"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private object CoerceType(Type targetType, IDictionary value)
		{
			object newValue = this.InstantiateObject(targetType, value);

			IDictionary<string, MemberMap> maps = this.ResolverCache.LoadMaps(targetType);
			if (maps == null)
			{
				IDictionary<string, object> genericDictionary = newValue as IDictionary<string, object>;
				if (genericDictionary != null)
				{
					// copy all values into new object
					foreach (object key in value.Keys)
					{
						string memberName = Convert.ToString(key, CultureInfo.InvariantCulture);
						genericDictionary[memberName] = value[memberName];
					}
				}
				else
				{
					IDictionary dictionary = newValue as IDictionary;
					if (dictionary != null)
					{
						// copy all values into new object
						foreach (object memberName in value.Keys)
						{
							dictionary[memberName] = value[memberName];
						}
					}
				}
			}
			else
			{
				// copy any values into new object
				foreach (object key in value.Keys)
				{
					string memberName = Convert.ToString(key, CultureInfo.InvariantCulture);

					MemberMap map;

					if (maps.TryGetValue(memberName, out map) &&
						map != null && map.Setter != null)
					{
						map.Setter(newValue, value[key]);
					}
				}
			}

			return newValue;
		}

		private object CoerceList(Type targetType, Type valueType, IEnumerable value)
		{
			targetType = TypeCoercionUtility.ResolveInterfaceType(targetType);

			if (targetType.IsArray)
			{
				// arrays are much simpler to create
				return this.CoerceArray(targetType.GetElementType(), value);
			}

			// targetType serializes as a JSON array but is not an array
			// assume is an ICollection or IEnumerable with AddRange, Add,
			// or custom Constructor with which we can populate it

			FactoryMap factory = this.ResolverCache.LoadFactory(targetType);
			if (factory == null)
			{
				throw new TypeCoercionException(String.Format(
					TypeCoercionUtility.ErrorCtor,
					targetType.FullName));
			}

			foreach (Type argType in factory.ArgTypes)
			{
				if (argType.IsAssignableFrom(valueType))
				{
					try
					{
						// invoke first constructor that can take this value as an argument
						return factory[argType](value);
					}
					catch
					{
						// there might exist a better match
						continue;
					}
				}
			}

			if (factory.Ctor == null)
			{
				throw new TypeCoercionException(String.Format(
					TypeCoercionUtility.ErrorCtor,
					targetType.FullName));
			}

			// attempt bulk insert first as is most efficient
			if (factory.AddRange != null &&
				factory.AddRangeType != null &&
				factory.AddRangeType.IsAssignableFrom(valueType))
			{
				object collection = factory.Ctor();
				factory.AddRange(collection, value);
				return collection;
			}

			// attempt sequence of single inserts next
			if (factory.Add != null &&
				factory.AddType != null)
			{
				object collection = factory.Ctor();
				Type addType = factory.AddType;

				// loop through adding items to collection
				foreach (object item in value)
				{
					factory.Add(collection, this.CoerceType(addType, item));
				}
				return collection;
			}

			try
			{
				// finally fall back to basics
				return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw new TypeCoercionException(
					String.Format(
						"Error converting {0} to {1}",
						value.GetType().FullName,
						targetType.FullName),
					ex);
			}
		}

		internal object CoerceCollection(Type targetType, Type itemType, ICollection value)
		{
			if (targetType != null && targetType != typeof(object))
			{
				// convert to requested type
				return this.CoerceList(targetType, value.GetType(), value);
			}

			// if all items are of same type then convert to array of that type
			Array array = Array.CreateInstance(itemType ?? typeof(object), value.Count);
			value.CopyTo(array, 0);
			return array;
		}

		/// <summary>
		/// Coerces an sequence of items into an array of Type elementType
		/// </summary>
		/// <param name="elementType"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private Array CoerceArray(Type itemType, IEnumerable value)
		{
			ICollection collection = value as ICollection;
			if (collection == null)
			{
				List<object> list = new List<object>();

				foreach (object item in value)
				{
					// convert each as is added
					list.Add(this.CoerceType(itemType, item));
				}

				collection = list;
			}

			// if all items are of same type then convert to array of that type
			Array array = Array.CreateInstance(itemType ?? typeof(object), collection.Count);
			collection.CopyTo(array, 0);
			return array;
		}

		#endregion Coercion Methods

		#region Type Methods

		/// <summary>
		/// Finds a suitable concrete class for common collection interface types
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>
		private static Type ResolveInterfaceType(Type targetType)
		{
			if (targetType.IsInterface)
			{
				if (targetType.IsGenericType)
				{
					Type genericType = targetType.GetGenericTypeDefinition();

					if (genericType == typeof(IList<>) ||
						genericType == typeof(IEnumerable<>) ||
#if !NET20 && !NET30 && !WINDOWS_PHONE
						genericType == typeof(IQueryable<>) ||
						genericType == typeof(IOrderedQueryable<>) ||
#endif
						genericType == typeof(ICollection<>))
					{
						Type[] genericArgs = targetType.GetGenericArguments();
						targetType = typeof(List<>).MakeGenericType(genericArgs);
					}
					else if (genericType == typeof(IDictionary<,>))
					{
						Type[] genericArgs = targetType.GetGenericArguments();
						if (genericArgs.Length == 2 &&
							genericArgs[0] == typeof(string) &&
							genericArgs[0] == typeof(object))
						{
							// allow ExpandoObject in NET40, otherwise Dictionary<string, object>
							targetType = typeof(JsonObject);
						}
						else
						{
							targetType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
						}
					}
				}
				else if (targetType == typeof(IList) ||
					targetType == typeof(IEnumerable) ||
#if !NET20 && !NET30 && !WINDOWS_PHONE
					targetType == typeof(IQueryable) ||
					targetType == typeof(IOrderedQueryable) ||
#endif
					targetType == typeof(ICollection))
				{
					targetType = typeof(object[]);
				}
				else if (targetType == typeof(IDictionary))
				{
					// <rant>cannot use ExpandoObject here because it does not implement IDictionary</rant>
					targetType = typeof(Dictionary<string, object>);
				}
#if NET40 && !WINDOWS_PHONE
				else if (targetType == typeof(System.Dynamic.IDynamicMetaObjectProvider))
				{
					targetType = typeof(System.Dynamic.ExpandoObject);
				}
#endif
			}
			return targetType;
		}

		/// <summary>
		/// Allows specific IDictionary&lt;string, TVal&gt; to deserialize as TVal
		/// </summary>
		/// <param name="targetType">IDictionary&lt;string, TVal&gt; Type</param>
		/// <returns>TVal Type</returns>
		internal static Type GetDictionaryItemType(Type targetType)
		{
			if (targetType == null)
			{
				return null;
			}

			Type dictionaryType = null;
#if !NETCF
			dictionaryType = targetType.GetInterface(TypeCoercionUtility.TypeGenericIDictionary, false);
#endif
			if (dictionaryType == null)
			{
				// not an IDictionary<TKey, TVal>
				return null;
			}

			Type[] genericArgs = dictionaryType.GetGenericArguments();
			if (genericArgs.Length != 2 ||
				!genericArgs[0].IsAssignableFrom(typeof(string)))
			{
				if (typeof(IDictionary).IsAssignableFrom(targetType))
				{
					// can build from non-generic IDictionary
					return null;
				}

				// only supports variants of IDictionary<string, TVal>
				throw new ArgumentException(String.Format(
					TypeCoercionUtility.ErrorGenericIDictionaryKeys,
					targetType));
			}

			return genericArgs[1];
		}

		internal static Type GetElementType(Type targetType)
		{
			if (targetType == null || targetType == typeof(string))
			{
				// not array type
				return null;
			}

			if (targetType.HasElementType)
			{
				// found array element type
				return targetType.GetElementType();
			}

			Type arrayType = null;
#if !NETCF
			arrayType = targetType.GetInterface(TypeCoercionUtility.TypeGenericIEnumerable, false);
#endif
			if (arrayType == null)
			{
				// not an IEnumerable<T>
				return null;
			}

			// found list or enumerable type
			Type[] genericArgs = arrayType.GetGenericArguments();
			return (genericArgs.Length == 1) ? genericArgs[0] : null;
		}

		/// <summary>
		/// Returns a common type which can hold previous values and the new value
		/// </summary>
		/// <param name="itemType"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		internal static Type FindCommonType(Type itemType, object value)
		{
			// establish if array is of common type
			if (value == null)
			{
				if (itemType != null && itemType.IsValueType)
				{
					// must use plain object to hold null
					itemType = typeof(object);
				}
			}
			else if (itemType == null)
			{
				// try out a hint type
				// if hasn't been set before
				itemType = value.GetType();
			}
			else
			{
				Type valueType = value.GetType();
				if (!itemType.IsAssignableFrom(valueType))
				{
					if (valueType.IsAssignableFrom(itemType))
					{
						// attempt to use the more general type
						itemType = valueType;
					}
					else
					{
						// use plain object to hold value
						// TODO: find a common ancestor?
						itemType = typeof(object);
					}
				}
			}

			return itemType;
		}

		/// <summary>
		/// Determines if type can be assigned a null value.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static bool IsNullable(Type type)
		{
			return type.IsGenericType && (typeof(Nullable<>) == type.GetGenericTypeDefinition());
		}

		#endregion Type Methods

		#region Attribute Methods

		/// <summary>
		/// Gets the attribute T for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <typeparam name="T">Attribute Type</typeparam>
		/// <returns>true if defined</returns>
		internal static bool HasAttribute<T>(MemberInfo info)
			where T : Attribute
		{
			return (info != null && Attribute.IsDefined(info, typeof(T)));
		}

		/// <summary>
		/// Gets the attribute of Type <param name="type" /> for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>true if defined</returns>
		internal static bool HasAttribute(MemberInfo info, Type type)
		{
			return (info != null && type != null && Attribute.IsDefined(info, type));
		}

		/// <summary>
		/// Gets the attribute T for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <typeparam name="T">Attribute Type</typeparam>
		/// <returns>requested attribute or not if not defined</returns>
		internal static T GetAttribute<T>(MemberInfo info)
			where T : Attribute
		{
			if (info == null || !Attribute.IsDefined(info, typeof(T)))
			{
				return default(T);
			}
			return (T)Attribute.GetCustomAttribute(info, typeof(T));
		}

		/// <summary>
		/// Gets the attribute of Type <param name="type" /> for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>requested attribute or not if not defined</returns>
		internal static Attribute GetAttribute(MemberInfo info, Type type)
		{
			if (info == null || type == null || !Attribute.IsDefined(info, type))
			{
				return default(Attribute);
			}
			return Attribute.GetCustomAttribute(info, type);
		}

		#endregion Attribute Methods

		#region IResolverCacheContainer Members

		ResolverCache IResolverCacheContainer.ResolverCache
		{
			get { return this.ResolverCache; }
		}

		#endregion IResolverCacheContainer Members
	}
}
