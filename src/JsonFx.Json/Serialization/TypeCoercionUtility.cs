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

namespace JsonFx.Serialization
{
	/// <summary>
	/// Performs type coercion
	/// </summary>
	internal sealed class TypeCoercionUtility :
		IResolverCacheContainer
	{
		#region Constants

		private static readonly string TypeGenericIEnumerable = typeof(IEnumerable<>).FullName;
		private static readonly string TypeGenericIDictionary = typeof(IDictionary<,>).FullName;

		private const string ErrorNullValueType = "{0} does not accept null as a value";
		private const string ErrorDefaultCtor = "Only objects with default constructors can be deserialized. ({0})";
		private const string ErrorCannotInstantiate = "Interfaces, Abstract classes, and unsupported ValueTypes cannot be deserialized. ({0})";
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
		/// Instantiates a new instance of objectType ensuring is a sub-Type of Type T.
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns>objectType instance</returns>
		public static T InstantiateObject<T>(Type objectType)
		{
			if (!objectType.IsSubclassOf(typeof(T)))
			{
				throw new TypeCoercionException(String.Format(
					TypeCoercionUtility.ErrorCannotInstantiateAsT,
					objectType.FullName,
					typeof(T).FullName));
			}

			return (T)TypeCoercionUtility.InstantiateObject(objectType);
		}

		/// <summary>
		/// Instantiates a new instance of objectType.
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns>objectType instance</returns>
		public static object InstantiateObject(Type targetType)
		{
			targetType = TypeCoercionUtility.ResolveInterfaceType(targetType);

			if (targetType.IsInterface || targetType.IsAbstract || targetType.IsValueType)
			{
				throw new TypeCoercionException(String.Format(
					TypeCoercionUtility.ErrorCannotInstantiate,
					targetType.FullName));
			}

			ConstructorInfo ctor = targetType.GetConstructor(null);
			if (ctor == null)
			{
				throw new TypeCoercionException(String.Format(
					TypeCoercionUtility.ErrorDefaultCtor,
					targetType.FullName));
			}
			object result;
			try
			{
				// always try-catch Invoke() to expose real exception
				// TODO :build FactoryDelegate, cache under targetType and execute
				result = ctor.Invoke(null);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
				{
					throw new TypeCoercionException(ex.InnerException.Message, ex.InnerException);
				}

				throw new TypeCoercionException("Error instantiating " + targetType.FullName, ex);
			}

			return result;
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
			else if (targetType != null && targetType.GetInterface(TypeCoercionUtility.TypeGenericIDictionary) != null)
			{
				throw new TypeCoercionException(String.Format(
					TypeCoercionUtility.ErrorGenericIDictionary,
					targetType));
			}
			else if (memberMap != null && memberMap.Setter != null)
			{
				memberMap.Setter(target, this.CoerceType(memberMap.Type, memberValue));
			}

			// ignore non-applicable members
		}

		internal IDictionary<string, MemberMap> LoadMaps(Type type)
		{
			return this.ResolverCache.LoadMaps(type);
		}

		#endregion Object Manipulation Methods

		#region Coercion Methods

		/// <summary>
		/// Coerces the object value to the Type targetType
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
						IDictionary<string, MemberMap> map = this.ResolverCache.LoadMaps(targetType);
						if (map != null && map.ContainsKey((string)value))
						{
							value = map[(string)value].Name;
						}
					}

					return Enum.Parse(targetType, (string)value);
				}
				else
				{
					value = this.CoerceType(Enum.GetUnderlyingType(targetType), value);
					return Enum.ToObject(targetType, value);
				}
			}

			if (value is IDictionary)
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
					if (DateTime.TryParse(
						(string)value,
						DateTimeFormatInfo.InvariantInfo,
						DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault,
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
			}
			else if (targetType == typeof(TimeSpan))
			{
				return new TimeSpan((long)this.CoerceType(typeof(Int64), value));
			}

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

			try
			{
				// fall back to basics
				return Convert.ChangeType(value, targetType);
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
		private object CoerceType(Type targetType, IDictionary value)
		{
			object newValue = TypeCoercionUtility.InstantiateObject(targetType);

			IDictionary<string, MemberMap> maps = this.ResolverCache.LoadMaps(targetType);
			if (maps != null)
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

			// many ICollection types take an IEnumerable or ICollection
			// as a constructor argument.  look through constructors for
			// a compatible match.
			ConstructorInfo[] ctors = targetType.GetConstructors();
			ConstructorInfo defaultCtor = null;
			foreach (ConstructorInfo ctor in ctors)
			{
				ParameterInfo[] paramList = ctor.GetParameters();
				if (paramList.Length == 0)
				{
					// save for in case cannot find closer match
					defaultCtor = ctor;
					continue;
				}

				if (paramList.Length == 1 &&
					paramList[0].ParameterType.IsAssignableFrom(valueType))
				{
					try
					{
						// invoke first constructor that can take this value as an argument
						// TODO :build FactoryDelegate, cache under targetType and execute
						return ctor.Invoke(
								new object[] { value }
							);
					}
					catch
					{
						// there might exist a better match
						continue;
					}
				}
			}

			if (defaultCtor == null)
			{
				throw new TypeCoercionException(String.Format(
					TypeCoercionUtility.ErrorDefaultCtor,
					targetType.FullName));
			}
			object collection;
			try
			{
				// always try-catch Invoke() to expose real exception
				// TODO :build FactoryDelegate, cache under targetType and execute
				collection = defaultCtor.Invoke(null);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
				{
					throw new TypeCoercionException(ex.InnerException.Message, ex.InnerException);
				}
				throw new TypeCoercionException("Error instantiating " + targetType.FullName, ex);
			}

			// many ICollection types have an AddRange method
			// which adds all items at once
			MethodInfo method = targetType.GetMethod("AddRange");
			ParameterInfo[] parameters = (method == null) ?
					null : method.GetParameters();
			Type paramType = (parameters == null || parameters.Length != 1) ?
					null : parameters[0].ParameterType;

			if (paramType != null &&
				paramType.IsAssignableFrom(valueType))
			{
				try
				{
					// always try-catch Invoke() to expose real exception
					// add all members in one method
					// TODO :build FactoryDelegate, cache under targetType and execute
					method.Invoke(
						collection,
						new object[] { value });
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException != null)
					{
						throw new TypeCoercionException(ex.InnerException.Message, ex.InnerException);
					}
					throw new TypeCoercionException("Error calling AddRange on " + targetType.FullName, ex);
				}
				return collection;
			}
			else
			{
				// many ICollection types have an Add method
				// which adds items one at a time
				method = targetType.GetMethod("Add");
				parameters = (method == null) ?
						null : method.GetParameters();
				paramType = (parameters == null || parameters.Length != 1) ?
						null : parameters[0].ParameterType;

				if (paramType != null)
				{
					// loop through adding items to collection
					foreach (object item in value)
					{
						try
						{
							// always try-catch Invoke() to expose real exception
							// TODO :build FactoryDelegate, cache under targetType and execute
							method.Invoke(
								collection,
								new object[] {
								this.CoerceType(paramType, item)
							});
						}
						catch (TargetInvocationException ex)
						{
							if (ex.InnerException != null)
							{
								throw new TypeCoercionException(ex.InnerException.Message, ex.InnerException);
							}
							throw new TypeCoercionException("Error calling Add on " + targetType.FullName, ex);
						}
					}
					return collection;
				}
			}

			try
			{
				// fall back to basics
				return Convert.ChangeType(value, targetType);
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

		internal object CoerceArrayList(Type targetType, Type itemType, ArrayList value)
		{
			if (targetType != null && targetType != typeof(object))
			{
				// convert to requested array type
				return this.CoerceList(targetType, typeof(ArrayList), value);
			}

			if (itemType != null && itemType != typeof(object))
			{
				// if all items are of same type then convert to array of that type
				return value.ToArray(itemType);
			}

			// convert to an object array for consistency
			return value.ToArray();
		}

		/// <summary>
		/// Coerces an sequence of items into an array of Type elementType
		/// </summary>
		/// <param name="elementType"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private Array CoerceArray(Type itemType, IEnumerable value)
		{
			// TODO: if not NET20, optimize this with LINQ

			ArrayList target = value as ArrayList;

			if (target == null)
			{
				// attempt to ensure enough room
				target = (value is ICollection) ?
					new ArrayList(((ICollection)value).Count) :
					new ArrayList();

				foreach (object item in value)
				{
					// convert each as is added
					target.Add(this.CoerceType(itemType, item));
				}
			}

			if (itemType != null && itemType != typeof(object))
			{
				// if all items are of same type then convert to array of that type
				return target.ToArray(itemType);
			}

			// convert to an object array for consistency
			return target.ToArray();
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
			// TODO: if NET40, replace Dictionary<,> with ExpandoObject

			if (targetType.IsInterface)
			{
				if (targetType.IsGenericType)
				{
					Type genericType = targetType.GetGenericTypeDefinition();

					if (genericType == typeof(IList<>) ||
						genericType == typeof(IEnumerable<>) ||
						genericType == typeof(ICollection<>))
					{
						targetType = typeof(List<>).MakeGenericType(targetType.GetGenericArguments());
					}
					else if (genericType == typeof(IDictionary<,>))
					{
						targetType = typeof(Dictionary<,>).MakeGenericType(targetType.GetGenericArguments());
					}
				}
				else if (targetType == typeof(IList) ||
					targetType == typeof(IEnumerable) ||
					targetType == typeof(ICollection))
				{
					targetType = typeof(object[]);
				}
				else if (targetType == typeof(IDictionary))
				{
					targetType = typeof(Dictionary<string, object>);
				}
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

			Type dictionaryType = targetType.GetInterface(TypeCoercionUtility.TypeGenericIDictionary);
			if (dictionaryType == null)
			{
				// not an IDictionary<TKey, TVal>
				return null;
			}

			Type[] genericArgs = dictionaryType.GetGenericArguments();
			if (genericArgs.Length != 2 ||
				(genericArgs[0] != typeof(string) && genericArgs[0] != typeof(object)))
			{
				// only supports IDictionary<string, TVal>
				throw new ArgumentException(String.Format(
					TypeCoercionUtility.ErrorGenericIDictionaryKeys,
					targetType));

				// TODO: support serialization as KeyValue<TKey, TVal>[]?
			}

			return genericArgs[1];
		}

		internal static Type GetArrayItemType(Type targetType)
		{
			if (targetType == null)
			{
				return null;
			}

			if (targetType.HasElementType)
			{
				// found array element type
				return targetType.GetElementType();
			}

			Type arrayType = targetType.GetInterface(TypeCoercionUtility.TypeGenericIEnumerable);
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
		public static bool HasAttribute<T>(MemberInfo info)
			where T : Attribute
		{
			return (info != null && Attribute.IsDefined(info, typeof(T)));
		}

		/// <summary>
		/// Gets the attribute T for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <typeparam name="T">Attribute Type</typeparam>
		/// <returns>requested attribute or not if not defined</returns>
		public static T GetAttribute<T>(MemberInfo info)
			where T : Attribute
		{
			if (info == null || !Attribute.IsDefined(info, typeof(T)))
			{
				return default(T);
			}
			return (T)Attribute.GetCustomAttribute(info, typeof(T));
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
