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
using System.ComponentModel;
using System.Reflection;

using JsonFx.CodeGen;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Json.Resolvers
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter using JsonNameAttribute / JsonIgnoreAttribute / JsonPropertySpecifiedAttribute
	/// </summary>
	/// <remarks>
	/// This is the default strategy from JsonFx v1.0
	/// </remarks>
	public class JsonResolverStrategy : PocoResolverStrategy
	{
		#region Name Resolution Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="isImmutableType"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be read/write properties, or immutable</remarks>
		public override bool IsPropertyIgnored(PropertyInfo member, bool isImmutableType)
		{
			// must satisfy POCO rules and not be ignored
			return
				base.IsPropertyIgnored(member, isImmutableType) ||
				TypeCoercionUtility.HasAttribute<JsonIgnoreAttribute>(member);
		}

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be public, non-readonly field</remarks>
		public override bool IsFieldIgnored(FieldInfo member)
		{
			// must satisfy POCO rules and not be ignored
			return
				base.IsFieldIgnored(member) ||
				TypeCoercionUtility.HasAttribute<JsonIgnoreAttribute>(member);
		}

		/// <summary>
		/// Gets a delegate which determines if the property or field should not be serialized based upon its value.
		/// </summary>
		/// <param name="member"></param>
		/// <returns>if has a value equivalent to the DefaultValueAttribute</returns>
		/// <remarks>
		/// This is useful for excluding serialization of default values.
		/// </remarks>
		public override ValueIgnoredDelegate GetValueIgnoredCallback(MemberInfo member)
		{
			Type objType = member.ReflectedType ?? member.DeclaringType;
			JsonSpecifiedPropertyAttribute specifiedPropertyAttr = TypeCoercionUtility.GetAttribute<JsonSpecifiedPropertyAttribute>(member);

			// look up specified property to see if exists
			GetterDelegate specifiedPropertyGetter = null;
			if (specifiedPropertyAttr != null && !String.IsNullOrEmpty(specifiedPropertyAttr.SpecifiedProperty))
			{
				PropertyInfo specProp = objType.GetProperty(specifiedPropertyAttr.SpecifiedProperty, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);

				// ensure is correct return type
				if (specProp != null && specProp.PropertyType == typeof(bool))
				{
					specifiedPropertyGetter = DynamicMethodGenerator.GetPropertyGetter(specProp);
				}
			}

			DefaultValueAttribute defaultAttr = TypeCoercionUtility.GetAttribute<DefaultValueAttribute>(member);
			if (defaultAttr == null)
			{
				if (specifiedPropertyGetter == null)
				{
					// no need to even create a delegate
					return null;
				}

				// create a delegate which simply calls the specified property
				return delegate(object target, object value)
				{
					return Object.Equals(false, specifiedPropertyGetter(target));
				};
			}

			// extract default value since cannot change (is constant in attribute)
			object defaultValue = defaultAttr.Value;

			if (specifiedPropertyGetter == null)
			{
				// create a specific delegate which only has to compare the default value to the current value
				return delegate(object target, object value)
				{
					return Object.Equals(defaultValue, value);
				};
			}

			// create a combined delegate which checks both states
			return delegate(object target, object value)
			{
				return
					Object.Equals(defaultValue, value) ||
					Object.Equals(false, specifiedPropertyGetter(target));
			};
		}

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public override IEnumerable<DataName> GetName(MemberInfo member)
		{
			JsonNameAttribute attr = TypeCoercionUtility.GetAttribute<JsonNameAttribute>(member);

			// NOTE: JSON allows String.Empty as a valid property name
			if ((attr == null) || (attr.Name == null))
			{
				yield break;
			}

			yield return new DataName(attr.Name);
		}

		#endregion Name Resolution Methods
	}
}
