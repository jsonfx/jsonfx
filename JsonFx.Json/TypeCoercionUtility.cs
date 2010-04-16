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
//using System.Reflection;

namespace JsonFx.Json
{
	/// <summary>
	/// Controls deserialization settings for IDataReader
	/// </summary>
	public class DataReaderSettings
	{
		#region Constants

		private const string ErrorNullValueType = "{0} does not accept null as a value";
		private const string ErrorDefaultCtor = "Only objects with default constructors can be deserialized. ({0})";
		private const string ErrorCannotInstantiate = "Interfaces, Abstract classes, and unsupported ValueTypes cannot be deserialized. ({0})";

		#endregion Constants

		#region Fields

		private bool allowNullValueTypes = true;
		private DataNameResolver resolver;

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

		/// <summary>
		/// Gets and sets the deserialization settings.
		/// </summary>
		public DataNameResolver Resolver
		{
			get
			{
				if (this.resolver == null)
				{
					this.resolver = new DataNameResolver();
				}
				return this.resolver;
			}
			set { this.resolver = value; }
		}

		#endregion Properties

		#region object Methods

		/// <summary>
		/// Instantiates a new instance of objectType.
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns>objectType instance</returns>
		public object InstantiateObject(Type objectType)
		{
			if (objectType.IsInterface || objectType.IsAbstract || objectType.IsValueType)
			{
				throw new JsonTypeCoercionException(String.Format(
					DataReaderSettings.ErrorCannotInstantiate,
					objectType.FullName));
			}

			ConstructorInfo ctor = objectType.GetConstructor(Type.EmptyTypes);
			if (ctor == null)
			{
				throw new JsonTypeCoercionException(String.Format(
					DataReaderSettings.ErrorDefaultCtor,
					objectType.FullName));
			}
			object result;
			try
			{
				// always try-catch Invoke() to expose real exception
				result = ctor.Invoke(null);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
				{
					throw new JsonTypeCoercionException(ex.InnerException.Message, ex.InnerException);
				}

				throw new JsonTypeCoercionException("Error instantiating " + objectType.FullName, ex);
			}

			return result;
		}

		/// <summary>
		/// Helper method to set value of either a property or a field.
		/// </summary>
		/// <param name="target">the object which owns the member</param>
		/// <param name="memberType">the type of the meme</param>
		/// <param name="value">the member value</param>
		public void SetMemberValue(object target, MemberInfo memberInfo, object value)
		{
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
			{
				if (propertyInfo.CanWrite)
				{
					// set value of public property
					propertyInfo.SetValue(
						target,
						this.CoerceType(propertyInfo.PropertyType, value),
						null);
				}

				return;
			}

			FieldInfo fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
			{
				// set value of public field
				fieldInfo.SetValue(
					target,
					this.CoerceType(fieldInfo.FieldType, value));

				return;
			}

			// all other values are ignored
		}

		#endregion object Methods

		#region Type Methods

		/// <summary>
		/// Coerces the object value to the Type targetType
		/// </summary>
		/// <param name="targetType"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public object CoerceType(Type targetType, object value)
		{
			bool isNullable = DataReaderSettings.IsNullable(targetType);
			if (value == null)
			{
				if (!this.AllowNullValueTypes &&
					targetType.IsValueType &&
					!isNullable)
				{
					throw new JsonTypeCoercionException(String.Format(
						DataReaderSettings.ErrorNullValueType,
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
						// if isn't a defined value perhaps it is the JsonName
						foreach (FieldInfo field in targetType.GetFields())
						{
							string name = this.Resolver.GetName(field);
							if (StringComparer.Ordinal.Equals((string)value, name))
							{
								value = field.Name;
								break;
							}
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
				throw new JsonTypeCoercionException(
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
			object newValue = this.InstantiateObject(targetType);

			IDictionary<string, MemberInfo> memberMap = this.Resolver.GetReadMap(targetType);
			if (memberMap != null)
			{
				// copy any values into new object
				foreach (object key in value.Keys)
				{
					string memberName = (key as String);
					if (String.IsNullOrEmpty(memberName) || !memberMap.ContainsKey(memberName))
					{
						continue;
					}

					this.SetMemberValue(newValue, memberMap[memberName], value[key]);
				}
			}

			return newValue;
		}

		private object CoerceList(Type targetType, Type arrayType, IEnumerable value)
		{
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
					paramList[0].ParameterType.IsAssignableFrom(arrayType))
				{
					try
					{
						// invoke first constructor that can take this value as an argument
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
				throw new JsonTypeCoercionException(String.Format(
					DataReaderSettings.ErrorDefaultCtor,
					targetType.FullName));
			}
			object collection;
			try
			{
				// always try-catch Invoke() to expose real exception
				collection = defaultCtor.Invoke(null);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
				{
					throw new JsonTypeCoercionException(ex.InnerException.Message, ex.InnerException);
				}
				throw new JsonTypeCoercionException("Error instantiating " + targetType.FullName, ex);
			}

			// many ICollection types have an AddRange method
			// which adds all items at once
			MethodInfo method = targetType.GetMethod("AddRange");
			ParameterInfo[] parameters = (method == null) ?
					null : method.GetParameters();
			Type paramType = (parameters == null || parameters.Length != 1) ?
					null : parameters[0].ParameterType;
			if (paramType != null &&
				paramType.IsAssignableFrom(arrayType))
			{
				try
				{
					// always try-catch Invoke() to expose real exception
					// add all members in one method
					method.Invoke(
						collection,
						new object[] { value });
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException != null)
					{
						throw new JsonTypeCoercionException(ex.InnerException.Message, ex.InnerException);
					}
					throw new JsonTypeCoercionException("Error calling AddRange on " + targetType.FullName, ex);
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
								throw new JsonTypeCoercionException(ex.InnerException.Message, ex.InnerException);
							}
							throw new JsonTypeCoercionException("Error calling Add on " + targetType.FullName, ex);
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
				throw new JsonTypeCoercionException(
					String.Format(
						"Error converting {0} to {1}",
						value.GetType().FullName,
						targetType.FullName),
					ex);
			}
		}

		/// <summary>
		/// Coerces an sequence of items into an array of Type elementType
		/// </summary>
		/// <param name="elementType"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private Array CoerceArray(Type elementType, IEnumerable value)
		{
			ArrayList target = new ArrayList();

			foreach (object item in value)
			{
				target.Add(this.CoerceType(elementType, item));
			}

			return target.ToArray(elementType);
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
	}
}
