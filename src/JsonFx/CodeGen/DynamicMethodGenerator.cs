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
using System.Reflection.Emit;
using System.Security;

#if NET40 && !WINDOWS_PHONE
using System.Dynamic;
#endif

namespace JsonFx.CodeGen
{
	#region Dynamic Binders
#if NET40 && !WINDOWS_PHONE

	internal class DynamicGetter : GetMemberBinder
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="name"></param>
		public DynamicGetter(string name)
			: base(name, false)
		{
		}

		#endregion Init

		#region GetMemberBinder Members

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			throw new NotSupportedException();
		}

		#endregion GetMemberBinder Members
	}

	internal class DynamicSetter : SetMemberBinder
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="name"></param>
		public DynamicSetter(string name)
			: base(name, false)
		{
		}

		#endregion Init

		#region SetMemberBinder Members

		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			throw new NotSupportedException();
		}

		#endregion SetMemberBinder Members
	}

#endif
	#endregion Dynamic Binders

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

			Module module = methodInfo.DeclaringType.Module;

			// Create a dynamic method with a return type of object, and one parameter taking the instance.
			DynamicMethod dynamicMethod = new DynamicMethod(
				"",//propertyInfo.DeclaringType.FullName+".get_"+propertyInfo.Name,
				typeof(object),
				new Type[] { typeof(object) }
#if !SILVERLIGHT
				, module,
				true
#endif
				);

			// Get an ILGenerator and emit a body for the dynamic method,
			// using a stream size larger than the IL that will be emitted.
			ILGenerator il = dynamicMethod.GetILGenerator(64 * 5);
			if (!methodInfo.IsStatic)
			{
				// Load the target instance onto the evaluation stack
				il.Emit(OpCodes.Ldarg_0);
			}
			// Call the method that returns void
			//il.Emit(methodInfo.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, methodInfo);
			il.Emit(OpCodes.Call, methodInfo);
			if (propertyInfo.PropertyType.IsValueType)
			{
				// Load the return value as a reference type
				il.Emit(OpCodes.Box, propertyInfo.PropertyType);
			}
			// return property value from the method
			il.Emit(OpCodes.Ret);

			// produce a delegate that we can then call
			return (GetterDelegate)dynamicMethod.CreateDelegate(typeof(GetterDelegate));
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

			Module module = methodInfo.DeclaringType.Module;

			// Create a dynamic method with a return type of void, one parameter taking the instance and the other taking the new value.
			DynamicMethod dynamicMethod = new DynamicMethod(
				"",//propertyInfo.DeclaringType.FullName+".set_"+propertyInfo.Name,
				null,
				new Type[] { typeof(object), typeof(object) }
#if !SILVERLIGHT
				, module,
				true
#endif
				);

			// Get an ILGenerator and emit a body for the dynamic method,
			// using a stream size larger than the IL that will be emitted.
			ILGenerator il = dynamicMethod.GetILGenerator(64 * 5);

			if (!methodInfo.IsStatic)
			{
				// Load the target instance onto the evaluation stack
				il.Emit(OpCodes.Ldarg_0);
			}
			// Load the argument onto the evaluation stack
			il.Emit(OpCodes.Ldarg_1);
			if (propertyInfo.PropertyType.IsValueType)
			{
				// unbox the argument as a value type
				il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
			}
			else
			{
				// cast the argument as the corresponding type
				il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
			}
			// Call the method that returns void
			il.Emit(methodInfo.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, methodInfo);
			// return (void) from the method
			il.Emit(OpCodes.Ret);

			// produce a delegate that we can then call
			return (SetterDelegate)dynamicMethod.CreateDelegate(typeof(SetterDelegate));
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

			Module module = fieldInfo.DeclaringType.Module;

			// Create a dynamic method with a return type of object, one parameter taking the instance.
			DynamicMethod dynamicMethod = new DynamicMethod(
				"",//fieldInfo.DeclaringType.FullName+".get_"+fieldInfo.Name,
				typeof(object),
				new Type[] { typeof(object) }
#if !SILVERLIGHT
				, module,
				true
#endif
				);

			// Get an ILGenerator and emit a body for the dynamic method,
			// using a stream size larger than the IL that will be emitted.
			ILGenerator il = dynamicMethod.GetILGenerator(64 * 5);

			if (fieldInfo.IsStatic)
			{
				if (fieldInfo.IsLiteral)
				{
					object value = fieldInfo.GetValue(null);
					switch (Type.GetTypeCode(fieldInfo.FieldType))
					{
						case TypeCode.Boolean:
						{
							il.Emit(OpCodes.Ldc_I4, Object.Equals(true, value) ? 1 : 0);
							break;
						}
						case TypeCode.Byte:
						{
							il.Emit(OpCodes.Ldc_I4_S, (byte)value);
							break;
						}
						case TypeCode.Char:
						{
							il.Emit(OpCodes.Ldc_I4_S, (char)value);
							break;
						}
						case TypeCode.Int16:
						{
							il.Emit(OpCodes.Ldc_I4_S, (short)value);
							break;
						}
						case TypeCode.Int32:
						{
							il.Emit(OpCodes.Ldc_I4, (int)value);
							break;
						}
						case TypeCode.Int64:
						{
							il.Emit(OpCodes.Ldc_I8, (long)value);
							break;
						}
						case TypeCode.SByte:
						{
							il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
							break;
						}
						case TypeCode.String:
						{
							il.Emit(OpCodes.Ldstr, (string)value);
							break;
						}
						case TypeCode.UInt16:
						{
							il.Emit(OpCodes.Ldc_I4_S, (ushort)value);
							break;
						}
						case TypeCode.UInt32:
						{
							il.Emit(OpCodes.Ldc_I4, (uint)value);
							break;
						}
						case TypeCode.UInt64:
						{
							il.Emit(OpCodes.Ldc_I8, (ulong)value);
							break;
						}
						default:
						{
							// not sure how to load these
							return null;
						}
					}
				}
				else
				{
					// Load the static field
					il.Emit(OpCodes.Ldsfld, fieldInfo);
				}
			}
			else
			{
				// Load the target instance onto the evaluation stack
				il.Emit(OpCodes.Ldarg_0);
				// Load the field
				il.Emit(OpCodes.Ldfld, fieldInfo);
			}
			if (fieldInfo.FieldType.IsValueType)
			{
				// box the field value as a reference type
				il.Emit(OpCodes.Box, fieldInfo.FieldType);
			}
			// return field value from the method
			il.Emit(OpCodes.Ret);

			// produce a delegate that we can then call
			return (GetterDelegate)dynamicMethod.CreateDelegate(typeof(GetterDelegate));
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

			Module module = fieldInfo.DeclaringType.Module;

			// Create a dynamic method with a return type of void, one parameter taking the instance and the other taking the new value.
			// Create the method in the module that owns the instance type
			DynamicMethod dynamicMethod = new DynamicMethod(
				"",//fieldInfo.DeclaringType.FullName+".set_"+fieldInfo.Name,
				null,
				new Type[] { typeof(object), typeof(object) }
#if !SILVERLIGHT
				, module,
				true
#endif
				);

			// Get an ILGenerator and emit a body for the dynamic method,
			// using a stream size larger than the IL that will be emitted.
			ILGenerator il = dynamicMethod.GetILGenerator(64 * 5);

			if (!fieldInfo.IsStatic)
			{
				// Load the target instance onto the evaluation stack
				il.Emit(OpCodes.Ldarg_0);
			}
			// Load the argument onto the evaluation stack
			il.Emit(OpCodes.Ldarg_1);
			if (fieldInfo.FieldType.IsValueType)
			{
				// unbox the argument as a value type
				il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
			}
			else
			{
				// cast the argument as the corresponding type
				il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
			}
			if (fieldInfo.IsStatic)
			{
				// Set the static field
				il.Emit(OpCodes.Stsfld, fieldInfo);
			}
			else
			{
				// Set the field
				il.Emit(OpCodes.Stfld, fieldInfo);
			}
			// return (void) from the method
			il.Emit(OpCodes.Ret);

			// produce a delegate that we can then call
			return (SetterDelegate)dynamicMethod.CreateDelegate(typeof(SetterDelegate));
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

			Module module = methodInfo.DeclaringType.Module;

			// Create a dynamic method with a return type of object and one parameter for each argument.
			// Create the method in the module that owns the instance type
			DynamicMethod dynamicMethod = new DynamicMethod(
				"",//methodInfo.DeclaringType.FullName+"."+methodInfo.Name,
				typeof(object),
				new Type[] { typeof(object), typeof(object[]) }
#if !SILVERLIGHT
				, module,
				true
#endif
				);

			ParameterInfo[] args = methodInfo.GetParameters();

			// Get an ILGenerator and emit a body for the dynamic method,
			// using a stream size larger than the IL that will be emitted.
			ILGenerator il = dynamicMethod.GetILGenerator(64 * (args.Length+15));

			// define a label for the if statement
			Label jump = il.DefineLabel();

			// add a check for missing arguments
			// if (params.Length >= N) goto label;
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Ldc_I4, args.Length);
			il.Emit(OpCodes.Bge_S, jump);

			// throw new ArgumentException("Missing method arguments");
			il.Emit(OpCodes.Ldstr, "Missing method arguments");
			il.Emit(OpCodes.Newobj, typeof(ArgumentException).GetConstructor(new Type[] { typeof(string) }));
			il.Emit(OpCodes.Throw);

			// set this as the destination of the jump
			il.MarkLabel(jump);

			if (!methodInfo.IsStatic)
			{
				// Load the target instance onto the evaluation stack
				il.Emit(OpCodes.Ldarg_0);
			}

			for (int i=0; i<args.Length; i++)
			{
				ParameterInfo arg = args[i];
				Type argType = arg.ParameterType;

				if (arg.IsOut)
				{
					throw new NotSupportedException("GetProxyMethod does not support out parameters.");
				}

				// Load the argument from params array onto the evaluation stack
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldc_I4, i);
				il.Emit(OpCodes.Ldelem_Ref);
				if (argType.IsValueType)
				{
					// unbox the argument as a value type
					il.Emit(OpCodes.Unbox_Any, argType);
				}
				else
				{
					// cast the argument as the corresponding type
					il.Emit(OpCodes.Castclass, argType);
				}
			}

			// Call the ctor, passing in the stack of args, result is put back on stack
			// Call the method and return result
			//il.Emit(methodInfo.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, methodInfo);
			il.Emit(OpCodes.Call, methodInfo);

			if (methodInfo.ReturnType == typeof(void))
			{
				// no return type so load null to return
				il.Emit(OpCodes.Ldnull);
			}
			else
			{
				if (methodInfo.ReturnType.IsValueType)
				{
					// box the return value as a reference type
					il.Emit(OpCodes.Box, methodInfo.ReturnType);
				}
			}
			// return any result from the method
			il.Emit(OpCodes.Ret);

			// produce a delegate that we can then call
			return (ProxyDelegate)dynamicMethod.CreateDelegate(typeof(ProxyDelegate));
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

			ConstructorInfo ctor = type.GetConstructor(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
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

			Type type = ctor.DeclaringType;
			ParameterInfo[] args = ctor.GetParameters();

			Module module = type.Module;

			// Create a dynamic method with a return type of object and one parameter for each argument.
			DynamicMethod dynamicMethod = new DynamicMethod(
				"",//type.FullName+".ctor_"+args.Length,
				typeof(object),
				new Type[] { typeof(object[]) }
#if !SILVERLIGHT
				, module,
				true
#endif
				);

			// Get an ILGenerator and emit a body for the dynamic method,
			// using a stream size larger than the IL that will be emitted.
			ILGenerator il = dynamicMethod.GetILGenerator(64 * (args.Length+15));

			// define a label for the if statement
			Label jump = il.DefineLabel();

			// add a check for missing arguments
			// if (params.Length >= N) goto label;
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Ldc_I4, args.Length);
			il.Emit(OpCodes.Bge_S, jump);

			// throw new TypeLoadException("Missing constructor arguments");
			il.Emit(OpCodes.Ldstr, "Missing constructor arguments");
			il.Emit(OpCodes.Newobj, typeof(TypeLoadException).GetConstructor(new Type[] { typeof(string) }));
			il.Emit(OpCodes.Throw);

			// set this as the destination of the jump
			il.MarkLabel(jump);

			for (int i=0; i<args.Length; i++)
			{
				Type argType = args[i].ParameterType;

				// Load the argument from params array onto the evaluation stack
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldc_I4, i);
				il.Emit(OpCodes.Ldelem_Ref);
				if (argType.IsValueType)
				{
					// unbox the argument as a value type
					il.Emit(OpCodes.Unbox_Any, argType);
				}
				else
				{
					// cast the argument as the corresponding type
					il.Emit(OpCodes.Castclass, argType);
				}
			}

			// Call the ctor, passing in the stack of args, result is put back on stack
			il.Emit(OpCodes.Newobj, ctor);
			if (type.IsValueType)
			{
				// box the return value as a reference type
				il.Emit(OpCodes.Box, type);
			}
			// return result from the method
			il.Emit(OpCodes.Ret);

			// produce a delegate that we can then call
			return (FactoryDelegate)dynamicMethod.CreateDelegate(typeof(FactoryDelegate));
		}

		#endregion Type Factory Generators
	}
}
