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
using System.Security;

namespace JsonFx.CodeGen
{
	/// <summary>
	/// Generates delegates for getting/setting properties and field and invoking constructors
	/// </summary>
#if NET20
	[SecurityTreatAsSafe]
	[SecurityCritical]
#else
	[SecuritySafeCritical]
#endif
	internal static class DynamicMethodGenerator
	{
		#region Getter / Setter Generators

		/// <summary>
		/// Creates a field getter delegate for the specified property or field
		/// </summary>
		/// <param name="memberInfo">PropertyInfo or FieldInfo</param>
		/// <returns>GetterDelegate for property or field, null otherwise</returns>
		/// <remarks>
		/// Note: use with caution this method will expose private and protected constructors without safety checks.
		/// </remarks>
		public static GetterDelegate GetGetter(MemberInfo memberInfo)
		{
			if (memberInfo is PropertyInfo)
			{
				return DynamicMethodGenerator.GetPropertyGetter((PropertyInfo)memberInfo);
			}

			if (memberInfo is FieldInfo)
			{
				return DynamicMethodGenerator.GetFieldGetter((FieldInfo)memberInfo);
			}

			return null;
		}

		/// <summary>
		/// Creates a field setter delegate for the specified property or field
		/// </summary>
		/// <param name="memberInfo">PropertyInfo or FieldInfo</param>
		/// <returns>SetterDelegate for property or field, null otherwise</returns>
		public static SetterDelegate GetSetter(MemberInfo memberInfo)
		{
			if (memberInfo is PropertyInfo)
			{
				return DynamicMethodGenerator.GetPropertySetter((PropertyInfo)memberInfo);
			}

			if (memberInfo is FieldInfo)
			{
				return DynamicMethodGenerator.GetFieldSetter((FieldInfo)memberInfo);
			}

			return null;
		}

		/// <summary>
		/// Creates a property getter delegate for the specified property
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns>GetterDelegate if property CanRead, otherwise null</returns>
		/// <remarks>
		/// Note: use with caution this method will expose private and protected constructors without safety checks.
		/// </remarks>
		public static GetterDelegate GetPropertyGetter(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("propertyInfo");
			}

			if (!propertyInfo.CanRead)
			{
				return null;
			}

			MethodInfo methodInfo = propertyInfo.GetGetMethod(true);
			if (methodInfo == null)
			{
				return null;
			}

			return delegate(object instance)
			{
				return methodInfo.Invoke(instance,
#if NETCF
	            new Type[]{}
#else
				Type.EmptyTypes
#endif
				);
			};
		}

		/// <summary>
		/// Creates a property setter delegate for the specified property
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns>GetterDelegate if property CanWrite, otherwise null</returns>
		/// <remarks>
		/// Note: use with caution this method will expose private and protected constructors without safety checks.
		/// </remarks>
		public static SetterDelegate GetPropertySetter(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("propertyInfo");
			}

			if (!propertyInfo.CanWrite)
			{
				return null;
			}

			MethodInfo methodInfo = propertyInfo.GetSetMethod(true);
			if (methodInfo == null)
			{
				return null;
			}

			return delegate(object instance, object value)
			{
				methodInfo.Invoke(instance, new object[] { value });
			};
		}

		/// <summary>
		/// Creates a field getter delegate for the specified field
		/// </summary>
		/// <param name="fieldInfo"></param>
		/// <returns>GetterDelegate which returns field unless is enum in which will return enum value</returns>
		/// <remarks>
		/// Note: use with caution this method will expose private and protected constructors without safety checks.
		/// </remarks>
		public static GetterDelegate GetFieldGetter(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}

			return delegate(object instance)
			{
				return fieldInfo.GetValue(instance);
			};
		}

		/// <summary>
		/// Creates a field setter delegate for the specified field
		/// </summary>
		/// <param name="fieldInfo"></param>
		/// <returns>SetterDelegate unless field IsInitOnly then returns null</returns>
		/// <remarks>
		/// Note: use with caution this method will expose private and protected constructors without safety checks.
		/// </remarks>
		public static SetterDelegate GetFieldSetter(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}

			if (fieldInfo.IsInitOnly ||
				fieldInfo.IsLiteral)
			{
				return null;
			}

			return delegate(object instance, object value)
			{
				fieldInfo.SetValue(instance, value);
			};
		}

		#endregion Getter / Setter Generators

		#region Method Generators

		public static ProxyDelegate GetMethodProxy(Type declaringType, string methodName, params Type[] argTypes)
		{
			if (declaringType == null)
			{
				throw new ArgumentNullException("declaringType");
			}
			if (String.IsNullOrEmpty(methodName))
			{
				throw new ArgumentNullException("methodName");
			}

			MethodInfo methodInfo;
			if (argTypes.Length > 0)
			{
				methodInfo = declaringType.GetMethod(
					methodName,
					BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy,
					null,
					argTypes,
					null);
			}
			else
			{
				methodInfo = declaringType.GetMethod(
					methodName,
					BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);
			}

			if (methodInfo == null)
			{
				return null;
			}

			return DynamicMethodGenerator.GetMethodProxy(methodInfo);
		}

		/// <summary>
		/// Creates a proxy delegate accepting a target instance and corresponding arguments
		/// </summary>
		/// <param name="methodInfo">method to proxy</param>
		/// <returns>ProxyDelegate or null if cannot be invoked</returns>
		/// <remarks>
		/// Note: use with caution this method will expose private and protected methods without safety checks.
		/// </remarks>
		public static ProxyDelegate GetMethodProxy(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}

			return delegate(object instance, object[] args)
			{
				return methodInfo.Invoke(instance, args);
			};
		}

		#endregion Method Generators

		#region Type Factory Generators

		/// <summary>
		/// Creates a default constructor delegate
		/// </summary>
		/// <param name="type">type to be created</param>
		/// <returns>FactoryDelegate or null if default constructor not found</returns>
		/// <remarks>
		/// Note: use with caution this method will expose private and protected constructors without safety checks.
		/// </remarks>
		public static FactoryDelegate GetTypeFactory(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			ConstructorInfo ctor = type.GetConstructor(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy, null,
#if NETCF
            new Type[]{},
#else
			Type.EmptyTypes,
#endif
			null);
			if (ctor == null)
			{
				return null;
			}

			return DynamicMethodGenerator.GetTypeFactory(ctor);
		}

		/// <summary>
		/// Creates a constructor delegate accepting specified arguments
		/// </summary>
		/// <param name="type">type to be created</param>
		/// <param name="args">constructor arguments type list</param>
		/// <returns>FactoryDelegate or null if constructor not found</returns>
		/// <remarks>
		/// Note: use with caution this method will expose private and protected constructors without safety checks.
		/// </remarks>
		public static FactoryDelegate GetTypeFactory(Type type, params Type[] args)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			ConstructorInfo ctor = type.GetConstructor(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy, null, args, null);
			if (ctor == null)
			{
				return null;
			}

			return DynamicMethodGenerator.GetTypeFactory(ctor);
		}

		/// <summary>
		/// Creates a constructor delegate accepting specified arguments
		/// </summary>
		/// <param name="type">type to be created</param>
		/// <param name="args">constructor arguments type list</param>
		/// <returns>FactoryDelegate or null if constructor not found</returns>
		/// <remarks>
		/// Note: use with caution this method will expose private and protected constructors without safety checks.
		/// </remarks>
		public static FactoryDelegate GetTypeFactory(ConstructorInfo ctor)
		{
			if (ctor == null)
			{
				throw new ArgumentNullException("ctor");
			}

			return delegate(object[] args)
			{
				return ctor.Invoke(args);
			};
		}

		#endregion Type Factory Generators
	}
}
