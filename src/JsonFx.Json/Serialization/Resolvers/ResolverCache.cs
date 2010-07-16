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
using System.Globalization;
using System.Reflection;
using System.Threading;

using JsonFx.CodeGen;

namespace JsonFx.Serialization.Resolvers
{
	public sealed class MemberMap
	{
		#region Fields

		/// <summary>
		/// The original member info
		/// </summary>
		public readonly MemberInfo MemberInfo;

		/// <summary>
		/// The original member name
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The member data name
		/// </summary>
		public readonly DataName DataName;

		/// <summary>
		/// The member type
		/// </summary>
		public readonly Type Type;

		/// <summary>
		/// The getter method
		/// </summary>
		public readonly GetterDelegate Getter;

		/// <summary>
		/// The setter method
		/// </summary>
		public readonly SetterDelegate Setter;

		/// <summary>
		/// The logic for determining if a value is ignored
		/// </summary>
		public readonly ValueIgnoredDelegate IsIgnored;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <param name="dataName"></param>
		/// <param name="isIgnored"></param>
		/// <param name="isAttribute"></param>
		public MemberMap(PropertyInfo propertyInfo, DataName dataName, ValueIgnoredDelegate isIgnored)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("propertyInfo");
			}

			this.DataName = dataName;
			this.MemberInfo = propertyInfo;
			this.Name = propertyInfo.Name;
			this.Type = propertyInfo.PropertyType;
			this.Getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			this.Setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			this.IsIgnored = isIgnored;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="fieldInfo"></param>
		/// <param name="dataName"></param>
		/// <param name="isIgnored"></param>
		/// <param name="isAttribute"></param>
		public MemberMap(FieldInfo fieldInfo, DataName dataName, ValueIgnoredDelegate isIgnored)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}

			this.DataName = dataName;
			this.MemberInfo = fieldInfo;
			this.Name = fieldInfo.Name;
			this.Type = fieldInfo.FieldType;
			this.Getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);
			this.Setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);
			this.IsIgnored = isIgnored;
		}

		#endregion Init
	}

	public sealed class FactoryMap
	{
		#region Constants

		private const string ErrorCannotInstantiate = "Interfaces, Abstract classes, and unsupported ValueTypes cannot be instantiated. ({0})";

		#endregion Constants

		#region Fields

		private readonly IDictionary<Type, FactoryDelegate> CollectionCtors;
		public readonly FactoryDelegate DefaultCtor;
		public readonly ProxyDelegate Add;
		public readonly ProxyDelegate AddRange;
		public readonly Type AddType;
		public readonly Type AddRangeType;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public FactoryMap(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			if (type.IsInterface || type.IsAbstract || type.IsValueType)
			{
				throw new TypeLoadException(String.Format(
					FactoryMap.ErrorCannotInstantiate,
					type.FullName));
			}

			if (!typeof(IEnumerable).IsAssignableFrom(type))
			{
				this.DefaultCtor = DynamicMethodGenerator.GetTypeFactory(type);
				return;
			}

			// many ICollection types take an IEnumerable or ICollection
			// as a constructor argument.  look through constructors for
			// a compatible match.
			ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Instance|BindingFlags.Public);

			this.CollectionCtors = new Dictionary<Type, FactoryDelegate>(ctors.Length);

			foreach (ConstructorInfo ctor in ctors)
			{
				ParameterInfo[] paramList = ctor.GetParameters();
				if (paramList.Length > 1)
				{
					continue;
				}

				if (paramList.Length == 0)
				{
					// save default in case cannot find closer match
					this.DefaultCtor = DynamicMethodGenerator.GetTypeFactory(type);
					continue;
				}

				Type argType = paramList[0].ParameterType;
				if ((argType == typeof(string)) ||
					((argType.GetInterface(typeof(IEnumerable<>).FullName) == null) &&
					(argType.GetInterface(typeof(IEnumerable).FullName) == null)))
				{
					continue;
				}

				// save all constructors that can take an enumerable of objects
				this.CollectionCtors[argType] = DynamicMethodGenerator.GetTypeFactory(ctor);
			}

			if (this.DefaultCtor == null)
			{
				// try to grab a private ctor if exists
				this.DefaultCtor = DynamicMethodGenerator.GetTypeFactory(type);
			}

			// many collection types have an AddRange method
			// which adds a collection of items at once
			MethodInfo methodInfo = type.GetMethod("AddRange");
			if (methodInfo != null)
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == 1)
				{
					this.AddRange = DynamicMethodGenerator.GetMethodProxy(methodInfo);
					this.AddRangeType = parameters[0].ParameterType;
				}
			}

			// many collection types have an Add method
			// which adds items one at a time
			Type collectionType = type.GetInterface(typeof(ICollection<>).FullName);
			if (collectionType != null)
			{
				methodInfo = collectionType.GetMethod("Add");
			}
			else
			{
				methodInfo = type.GetMethod("Add");
			}

			if (methodInfo != null)
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == 1)
				{
					this.Add = DynamicMethodGenerator.GetMethodProxy(methodInfo);
					this.AddType = parameters[0].ParameterType;
				}
			}
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets a sequence of the available factory arguments
		/// </summary>
		public IEnumerable<Type> ArgTypes
		{
			get
			{
				if (this.CollectionCtors == null)
				{
					return Type.EmptyTypes;
				}

				return this.CollectionCtors.Keys;
			}
		}

		/// <summary>
		/// Gets the factory associated with the given argument type
		/// </summary>
		/// <param name="argType"></param>
		/// <returns></returns>
		public FactoryDelegate this[Type argType]
		{
			get
			{
				FactoryDelegate factory;

				if (this.CollectionCtors == null ||
					!this.CollectionCtors.TryGetValue(argType, out factory))
				{
					return null;
				}

				return factory;
			}
		}

		#endregion Properties
	}

	/// <summary>
	/// Caches name resolution mappings for IDataReader / IDataWriter
	/// </summary>
	public sealed class ResolverCache
	{
		#region Constants

		private const string AnonymousTypePrefix = "<>f__AnonymousType";

#if NET20 || NET30
		private const int LockTimeout = 250;
#endif

		#endregion Constants

		#region Fields

#if NET20 || NET30
		private readonly ReaderWriterLock MapLock = new ReaderWriterLock();
#else
		private readonly ReaderWriterLockSlim MapLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
#endif

#if NET20 || NET30
		private readonly ReaderWriterLock FactoryLock = new ReaderWriterLock();
#else
		private readonly ReaderWriterLockSlim FactoryLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
#endif

		private readonly IDictionary<Type, IDictionary<string, MemberMap>> MemberCache = new Dictionary<Type, IDictionary<string, MemberMap>>();
		private readonly IDictionary<Type, IDictionary<Enum, string>> EnumCache = new Dictionary<Type, IDictionary<Enum, string>>();
		private readonly IDictionary<Type, FactoryMap> Factories = new Dictionary<Type, FactoryMap>();
		private readonly IDictionary<Type, DataName> NameCache = new Dictionary<Type, DataName>();

		private readonly IResolverStrategy Strategy;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="strategy"></param>
		public ResolverCache(IResolverStrategy strategy)
		{
			if (strategy == null)
			{
				throw new ArgumentNullException("strategy");
			}
			this.Strategy = strategy;
		}

		#endregion Init

		#region Map Methods

		/// <summary>
		/// Gets the serialized name of the class
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public DataName LoadTypeName(Type type)
		{
			if (type == null)
			{
				return new DataName(type);
			}

			DataName name;

#if NET20 || NET30
			this.MapLock.AcquireReaderLock(ResolverCache.LockTimeout);
#else
			this.MapLock.EnterReadLock();
#endif
			try
			{
				if (this.NameCache.TryGetValue(type, out name))
				{
					return name;
				}
			}
			finally
			{
#if NET20 || NET30
				this.MapLock.ReleaseReaderLock();
#else
				this.MapLock.ExitReadLock();
#endif
			}

			// populate maps for type
			IDictionary<string, MemberMap> maps;
			return this.BuildMap(type, out maps);
		}

		public IDictionary<string, MemberMap> LoadMaps(Type type)
		{
			if (type == null || type == typeof(object))
			{
				return null;
			}

			IDictionary<string, MemberMap> map;

#if NET20 || NET30
			this.MapLock.AcquireReaderLock(ResolverCache.LockTimeout);
#else
			this.MapLock.EnterReadLock();
#endif
			try
			{
				if (this.MemberCache.TryGetValue(type, out map))
				{
					return map;
				}
			}
			finally
			{
#if NET20 || NET30
				this.MapLock.ReleaseReaderLock();
#else
				this.MapLock.ExitReadLock();
#endif
			}

			this.BuildMap(type, out map);
			return map;
		}

		public IDictionary<Enum, string> LoadEnumMaps(Type type)
		{
			if (type == null || !type.IsEnum)
			{
				return null;
			}

			IDictionary<Enum, string> map;

#if NET20 || NET30
			this.MapLock.AcquireReaderLock(ResolverCache.LockTimeout);
#else
			this.MapLock.EnterReadLock();
#endif
			try
			{
				if (this.EnumCache.TryGetValue(type, out map))
				{
					return map;
				}
			}
			finally
			{
#if NET20 || NET30
				this.MapLock.ReleaseReaderLock();
#else
				this.MapLock.ExitReadLock();
#endif
			}

			this.BuildEnumMap(type, out map);
			return map;
		}

		/// <summary>
		/// Removes any cached member mappings.
		/// </summary>
		public void Clear()
		{
#if NET20 || NET30
			this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.MapLock.EnterWriteLock();
#endif
#if NET20 || NET30
			this.FactoryLock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.FactoryLock.EnterWriteLock();
#endif
			try
			{
				this.NameCache.Clear();
				this.MemberCache.Clear();
				this.EnumCache.Clear();
				this.Factories.Clear();
			}
			finally
			{
#if NET20 || NET30
				this.FactoryLock.ReleaseWriterLock();
#else
				this.FactoryLock.ExitWriteLock();
#endif
#if NET20 || NET30
				this.MapLock.ReleaseWriterLock();
#else
				this.MapLock.ExitWriteLock();
#endif
			}
		}

		/// <summary>
		/// Builds a mapping of member name to field/property
		/// </summary>
		/// <param name="objectType"></param>
		private DataName BuildMap(Type objectType, out IDictionary<string, MemberMap> map)
		{
			DataName typeName = this.Strategy.GetName(objectType);
			if (typeName.IsEmpty)
			{
				typeName = new DataName(objectType);
			}

			if (objectType.IsEnum)
			{
				// create special maps for enum types
				IDictionary<Enum, string> enumMap;
				map = this.BuildEnumMap(objectType, out enumMap);
				return typeName;
			}

			// do not incurr the cost of member map for dictionaries
			if (typeof(IDictionary<string, object>).IsAssignableFrom(objectType) ||
				typeof(IDictionary).IsAssignableFrom(objectType))
			{
#if NET20 || NET30
				this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
				this.MapLock.EnterWriteLock();
#endif
				try
				{
					// store marker in cache for future lookups
					map = (this.MemberCache[objectType] = null);
					return (this.NameCache[objectType] = typeName);
				}
				finally
				{
#if NET20 || NET30
					this.MapLock.ReleaseWriterLock();
#else
					this.MapLock.ExitWriteLock();
#endif
				}
			}

			// create new map
			map = new Dictionary<string, MemberMap>();

			bool isImmutableType = objectType.IsGenericType && objectType.Name.StartsWith(ResolverCache.AnonymousTypePrefix, false, CultureInfo.InvariantCulture);

			// load properties into property map
			foreach (PropertyInfo info in objectType.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
			{
				if (this.Strategy.IsPropertyIgnored(info, isImmutableType))
				{
					continue;
				}

				DataName name = this.Strategy.GetName(info);
				if (name.IsEmpty)
				{
					name = new DataName(info.Name);
				}

				ValueIgnoredDelegate isIgnored = this.Strategy.GetValueIgnoredCallback(info);

				map[name.LocalName] = new MemberMap(info, name, isIgnored);
			}

			// load fields into property map
			foreach (FieldInfo info in objectType.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
			{
				if (this.Strategy.IsFieldIgnored(info))
				{
					continue;
				}

				DataName name = this.Strategy.GetName(info);
				if (name.IsEmpty)
				{
					name = new DataName(info.Name);
				}

				ValueIgnoredDelegate isIgnored = this.Strategy.GetValueIgnoredCallback(info);

				map[name.LocalName] = new MemberMap(info, name, isIgnored);
			}

#if NET20 || NET30
			this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.MapLock.EnterWriteLock();
#endif
			try
			{
				// store in cache for future requests
				map = (this.MemberCache[objectType] = map);
				return (this.NameCache[objectType] = typeName);
			}
			finally
			{
#if NET20 || NET30
				this.MapLock.ReleaseWriterLock();
#else
				this.MapLock.ExitWriteLock();
#endif
			}
		}

		private IDictionary<string, MemberMap> BuildEnumMap(Type enumType, out IDictionary<Enum, string> enumMaps)
		{
			if (enumType == null || !enumType.IsEnum)
			{
				enumMaps = null;
				return null;
			}

			// create new maps
			DataName typeName = this.Strategy.GetName(enumType);
			if (typeName.IsEmpty)
			{
				typeName = new DataName(enumType);
			}

			IDictionary<string, MemberMap> maps = new Dictionary<string, MemberMap>();
			enumMaps = new Dictionary<Enum, string>();

			foreach (FieldInfo info in enumType.GetFields(BindingFlags.Static|BindingFlags.Public))
			{
				DataName name = this.Strategy.GetName(info);
				if (name.IsEmpty)
				{
					name = new DataName(info.Name);
				}

				MemberMap enumMap;
				maps[name.LocalName] = enumMap = new MemberMap(info, name, null);
				enumMaps[(Enum)enumMap.Getter(null)] = name.LocalName;
			}

#if NET20 || NET30
			this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.MapLock.EnterWriteLock();
#endif
			try
			{
				// store in caches for future requests
				this.NameCache[enumType] = typeName;
				this.EnumCache[enumType] = enumMaps;
				return (this.MemberCache[enumType] = maps);
			}
			finally
			{
#if NET20 || NET30
				this.MapLock.ReleaseWriterLock();
#else
				this.MapLock.ExitWriteLock();
#endif
			}
		}

		#endregion Map Methods

		#region Factory Methods

		public FactoryMap LoadFactory(Type type)
		{
			if (type == null || type == typeof(object))
			{
				return null;
			}

			FactoryMap map;

#if NET20 || NET30
			this.FactoryLock.AcquireReaderLock(ResolverCache.LockTimeout);
#else
			this.FactoryLock.EnterReadLock();
#endif
			try
			{
				if (this.Factories.TryGetValue(type, out map))
				{
					return map;
				}
			}
			finally
			{
#if NET20 || NET30
				this.FactoryLock.ReleaseReaderLock();
#else
				this.FactoryLock.ExitReadLock();
#endif
			}

			map = new FactoryMap(type);

#if NET20 || NET30
			this.FactoryLock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.FactoryLock.EnterWriteLock();
#endif
			try
			{
				// store in cache for future requests
				return (this.Factories[type] = map);
			}
			finally
			{
#if NET20 || NET30
				this.FactoryLock.ReleaseWriterLock();
#else
				this.FactoryLock.ExitWriteLock();
#endif
			}
		}

		#endregion Factory Methods
	}
}
