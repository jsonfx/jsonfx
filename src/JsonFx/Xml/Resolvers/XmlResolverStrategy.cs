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
using System.Xml.Serialization;

using JsonFx.CodeGen;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Xml.Resolvers
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter using attributes and conventions similar to XmlSerializer semantics
	/// </summary>
	/// <remarks>
	/// http://msdn.microsoft.com/en-us/library/83y7df3e.aspx
	/// </remarks>
	public class XmlResolverStrategy : PocoResolverStrategy
	{
		#region Constants

		private const string SpecifiedSuffix = "Specified";
		private const string ShouldSerializePrefix = "ShouldSerialize";

		#endregion Constants

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
				TypeCoercionUtility.HasAttribute<XmlIgnoreAttribute>(member);
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
				TypeCoercionUtility.HasAttribute<XmlIgnoreAttribute>(member);
		}

		/// <summary>
		/// Gets a delegate which determines if the property or field should not be serialized based upon its value.
		/// </summary>
		/// <param name="member"></param>
		/// <returns>if has a value equivalent to the DefaultValueAttribute or has a property named XXXSpecified which determines visibility</returns>
		/// <remarks>
		/// This is useful when default values need not be serialized.
		/// Under these situations XmlSerializer ignores properties based upon value:
		/// - DefaultValue: http://msdn.microsoft.com/en-us/library/system.componentmodel.defaultvalueattribute.aspx
		/// - Specified Properies: http://msdn.microsoft.com/en-us/library/bb402199.aspx
		/// - ShouldSerialize Methods: http://msdn.microsoft.com/en-us/library/53b8022e.aspx
		/// </remarks>
		public override ValueIgnoredDelegate GetValueIgnoredCallback(MemberInfo member)
		{
			Type objType = member.ReflectedType ?? member.DeclaringType;

			// look up specified property to see if exists
			GetterDelegate specifiedPropertyGetter = null;
			PropertyInfo specProp = objType.GetProperty(member.Name+SpecifiedSuffix, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);

			// ensure is correct return type
			if (specProp != null && specProp.PropertyType == typeof(bool))
			{
				specifiedPropertyGetter = DynamicMethodGenerator.GetPropertyGetter(specProp);
			}

			// look up specified property to see if exists
			ProxyDelegate shouldSerializeProxy = null;
			MethodInfo shouldSerialize = objType.GetMethod(ShouldSerializePrefix+member.Name, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);

			// ensure is correct return type
			if (shouldSerialize != null && shouldSerialize.ReturnType == typeof(bool) && shouldSerialize.GetParameters().Length == 0)
			{
				shouldSerializeProxy = DynamicMethodGenerator.GetMethodProxy(shouldSerialize);
			}

			// to be most efficient must create a different delegate for each of 8 combinations so not performing extra work

			DefaultValueAttribute defaultAttr = TypeCoercionUtility.GetAttribute<DefaultValueAttribute>(member);
			if (defaultAttr == null)
			{
				if (specifiedPropertyGetter == null)
				{
					if (shouldSerializeProxy == null)
					{
						// situation 1: only need to check if equal to null
						return delegate(object target, object value)
						{
							return (value == null);
						};
					}

					// situation 2: create a delegate which simply calls the should serialize method
					return delegate(object target, object value)
					{
						return
							(value == null) ||
							Object.Equals(false, shouldSerializeProxy(target));
					};
				}

				if (shouldSerializeProxy == null)
				{
					// situation 3: create a delegate which simply calls the specified property
					return delegate(object target, object value)
					{
						return
							(value == null) ||
							Object.Equals(false, specifiedPropertyGetter(target));
					};
				}

				// situation 4: create a delegate which calls both the specified property and the should serialize method
				return delegate(object target, object value)
				{
					return
						(value == null) ||
						Object.Equals(false, shouldSerializeProxy(target)) ||
						Object.Equals(false, specifiedPropertyGetter(target));
				};
			}

			// extract default value since cannot change (is constant in attribute)
			object defaultValue = defaultAttr.Value;

			if (specifiedPropertyGetter == null)
			{
				if (shouldSerializeProxy == null)
				{
					// situation 5: create a specific delegate which only has to compare the default value to the current value
					return delegate(object target, object value)
					{
						return
							(value == null) ||
							Object.Equals(defaultValue, value);
					};
				}

				// situation 6: create a specific delegate which both compares to default value and calls should serialize method
				return delegate(object target, object value)
				{
					return
						(value == null) ||
						Object.Equals(defaultValue, value) ||
						Object.Equals(false, shouldSerializeProxy(target));
				};
			}

			if (shouldSerializeProxy == null)
			{
				// situation 7: create a specific delegate which both compares to default value and checks specified property
				return delegate(object target, object value)
				{
					return
						(value == null) ||
						Object.Equals(defaultValue, value) ||
						Object.Equals(false, specifiedPropertyGetter(target));
				};
			}

			// situation 8: create a combined delegate which checks all states
			return delegate(object target, object value)
			{
				return
					(value == null) ||
					Object.Equals(defaultValue, value) ||
					Object.Equals(false, shouldSerializeProxy(target)) ||
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
			if (member is Type)
			{
				XmlRootAttribute rootAttr = TypeCoercionUtility.GetAttribute<XmlRootAttribute>(member);
				if (rootAttr != null && !String.IsNullOrEmpty(rootAttr.ElementName))
				{
					yield return new DataName(rootAttr.ElementName, null, rootAttr.Namespace);
				}

				XmlTypeAttribute typeAttr = TypeCoercionUtility.GetAttribute<XmlTypeAttribute>(member);
				if (typeAttr != null && !String.IsNullOrEmpty(typeAttr.TypeName))
				{
					yield return new DataName(typeAttr.TypeName, null, typeAttr.Namespace);
				}

				yield break;
			}

			XmlElementAttribute elemAttr = TypeCoercionUtility.GetAttribute<XmlElementAttribute>(member);
			if (elemAttr != null && !String.IsNullOrEmpty(elemAttr.ElementName))
			{
				yield return new DataName(elemAttr.ElementName, null, elemAttr.Namespace);
			}

			XmlAttributeAttribute attrAttr = TypeCoercionUtility.GetAttribute<XmlAttributeAttribute>(member);
			if (attrAttr != null && !String.IsNullOrEmpty(attrAttr.AttributeName))
			{
				yield return new DataName(attrAttr.AttributeName, null, attrAttr.Namespace, true);
			}

			XmlArrayAttribute arrayAttr = TypeCoercionUtility.GetAttribute<XmlArrayAttribute>(member);
			if (arrayAttr != null && !String.IsNullOrEmpty(arrayAttr.ElementName))
			{
				// TODO: figure out a way to surface XmlArrayItemAttribute name too

				yield return new DataName(arrayAttr.ElementName, null, arrayAttr.Namespace);
			}

			if (member is FieldInfo && ((FieldInfo)member).DeclaringType.IsEnum)
			{
				XmlEnumAttribute enumAttr = TypeCoercionUtility.GetAttribute<XmlEnumAttribute>(member);
				if (enumAttr != null && !String.IsNullOrEmpty(enumAttr.Name))
				{
					yield return new DataName(enumAttr.Name);
				}
			}
		}

		/// <summary>
		/// Sorts members to ensure proper document order where attributes precede all child elements.
		/// </summary>
		/// <param name="members"></param>
		/// <returns></returns>
		/// <remarks>
		/// Performs the first stage of XML Canonicalization document order http://www.w3.org/TR/xml-c14n#DocumentOrder
		/// "An element's namespace and attribute nodes have a document order position greater than the element but less than any child node of the element."
		/// </remarks>
		public override IEnumerable<MemberMap> SortMembers(IEnumerable<MemberMap> members)
		{
			// need to keep the order but partition into two groups: attributes and elements

			int count;
			if (members is ICollection<MemberMap>)
			{
				count = ((ICollection<MemberMap>)members).Count;

				if (count <= 1)
				{
					// special cases which don't require reordering
					foreach (MemberMap map in members) { yield return map; }
					yield break;
				}
			}
			else
			{
				count = 5;
			}

			// assume most are elements so will need to have same size queue
			List<MemberMap> elements = new List<MemberMap>(count);

			foreach (MemberMap map in members)
			{
				if (map.DataName.IsAttribute)
				{
					// pull out all the attributes first
					yield return map;
					continue;
				}

				elements.Add(map);
			}

			foreach (MemberMap map in elements)
			{
				// pull out all the elements next
				yield return map;
			}
		}

		#endregion Name Resolution Methods
	}
}
