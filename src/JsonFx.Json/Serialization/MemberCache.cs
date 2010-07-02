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

using JsonFx.CodeGen;

namespace JsonFx.Serialization
{
	public class MemberMap
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="propertyInfo"></param>
		public MemberMap(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("propertyInfo");
			}

			this.MemberInfo = propertyInfo;
			this.Name = propertyInfo.Name;
			this.Type = propertyInfo.PropertyType;
			this.Getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			this.Setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="fieldInfo"></param>
		public MemberMap(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}

			this.MemberInfo = fieldInfo;
			this.Name = fieldInfo.Name;
			this.Type = fieldInfo.FieldType;
			this.Getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);
			this.Setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);
		}

		#endregion Init

		#region Properties

		public MemberInfo MemberInfo
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public Type Type
		{
			get;
			private set;
		}

		public GetterDelegate Getter
		{
			get;
			private set;
		}

		public SetterDelegate Setter
		{
			get;
			private set;
		}

		#endregion Properties
	}

	/// <summary>
	/// Caches name resolution mappings for IDataReader / IDataWriter
	/// </summary>
	public class MemberCache
	{
		// TODO: replace lock with ReaderWriterLockSlim and ReaderWriterLock for NET20

		#region Constants

		private const string AnonymousTypePrefix = "<>f__AnonymousType";

		#endregion Constants

		#region Fields

		private readonly Dictionary<Type, IDictionary<string, MemberMap>> MapCache = new Dictionary<Type, IDictionary<string, MemberMap>>();
		internal readonly IDataNameResolver Resolver;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="resolver"></param>
		public MemberCache(IDataNameResolver resolver)
		{
			if (resolver == null)
			{
				throw new ArgumentNullException("resolver");
			}
			this.Resolver = resolver;
		}

		#endregion Init

		#region Map Methods

		public IDictionary<string, MemberMap> LoadMaps(Type type)
		{
			if (type == null || type == typeof(object))
			{
				return null;
			}

			lock (this.MapCache)
			{
				return this.MapCache.ContainsKey(type) ? this.MapCache[type] : this.BuildMap(type);
			}
		}

		/// <summary>
		/// Removes any cached member mappings.
		/// </summary>
		public void Clear()
		{
			lock (this.MapCache)
			{
				this.MapCache.Clear();
			}
		}

		/// <summary>
		/// Builds a mapping of member name to field/property
		/// </summary>
		/// <param name="objectType"></param>
		private IDictionary<string, MemberMap> BuildMap(Type objectType)
		{
			lock (this.MapCache)
			{
				// do not incurr the cost of member map for dictionaries
				if (typeof(IDictionary).IsAssignableFrom(objectType))
				{
					// store marker in cache for future lookups
					return (this.MapCache[objectType] = null);
				}

				// create new map
				IDictionary<string, MemberMap> map = new Dictionary<string, MemberMap>();

				if (!objectType.IsEnum)
				{
					bool isAnonymousType = objectType.IsGenericType && objectType.Name.StartsWith(MemberCache.AnonymousTypePrefix);

					// load properties into property map
					foreach (PropertyInfo info in objectType.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
					{
						if (this.Resolver.IsPropertyIgnored(info, isAnonymousType) ||
							this.Resolver.IsIgnored(info))
						{
							continue;
						}

						string name = this.Resolver.GetName(info);
						if (String.IsNullOrEmpty(name))
						{
							name = info.Name;
						}

						map[name] = new MemberMap(info);
					}
				}

				// load fields into property map
				foreach (FieldInfo info in objectType.GetFields(objectType.IsEnum ? BindingFlags.Static|BindingFlags.Public : BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
				{
					if (this.Resolver.IsFieldIgnored(info) ||
						this.Resolver.IsIgnored(info))
					{
						continue;
					}

					string name = this.Resolver.GetName(info);
					if (String.IsNullOrEmpty(name))
					{
						name = info.Name;
					}

					map[name] = new MemberMap(info);
				}

				// store in cache for future usage
				return (this.MapCache[objectType] = map);
			}
		}

		#endregion Map Methods
	}
}
