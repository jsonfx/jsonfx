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

namespace JsonFx.Serialization.Resolvers
{
#if NET40 && !SILVERLIGHT
	using EnumCacheDictionary=System.Collections.Concurrent.ConcurrentDictionary<Type, IDictionary<Enum, string>>;
	using FactoriesDictionary=System.Collections.Concurrent.ConcurrentDictionary<Type, FactoryMap>;
	using MemberCacheDictionary=System.Collections.Concurrent.ConcurrentDictionary<Type, IDictionary<string, MemberMap>>;
	using NameCacheDictionary=System.Collections.Concurrent.ConcurrentDictionary<Type, IEnumerable<DataName>>;
#else
	using MemberCacheDictionary=System.Collections.Generic.Dictionary<Type, IDictionary<string, MemberMap>>;
	using EnumCacheDictionary=System.Collections.Generic.Dictionary<Type, IDictionary<Enum, string>>;
	using FactoriesDictionary=System.Collections.Generic.Dictionary<Type, FactoryMap>;
	using NameCacheDictionary=System.Collections.Generic.Dictionary<Type, IEnumerable<DataName>>;
#endif

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

		/// <summary>
		/// Determines if map name is alternate (i.e. only used for deserialization)
		/// </summary>
		public readonly bool IsAlternate;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="map">MemberMap to clone</param>
		/// <param name="dataName">alternate name</param>
		public MemberMap(MemberMap map, DataName dataName)
		{
			this.MemberInfo = map.MemberInfo;
			this.Type = map.Type;
			this.Getter = map.Getter;
			this.Setter = map.Setter;
			this.IsIgnored = map.IsIgnored;

			this.DataName = dataName;
			this.IsAlternate = true;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <param name="dataName"></param>
		/// <param name="isIgnored"></param>
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
		public readonly FactoryDelegate Ctor;
		public readonly ParameterInfo[] CtorArgs;
		public readonly ProxyDelegate Add;
		public readonly Type AddType;
		public readonly ProxyDelegate AddRange;
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

			if (FactoryMap.IsInvalidType(type))
			{
				throw new TypeLoadException(String.Format(
					FactoryMap.ErrorCannotInstantiate,
					type.FullName));
			}

			this.Ctor = DynamicMethodGenerator.GetTypeFactory(type);

			ConstructorInfo[] ctors;
			if (!typeof(IEnumerable).IsAssignableFrom(type))
			{
				if (this.Ctor != null)
				{
					return;
				}

				ctors = type.GetConstructors(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy);
				if (ctors.Length == 1)
				{
					ConstructorInfo ctor = ctors[0];
					this.Ctor = DynamicMethodGenerator.GetTypeFactory(ctor);
					this.CtorArgs = ctor.GetParameters();
				}
				return;
			}

			// many ICollection types take an IEnumerable or ICollection
			// as a constructor argument.  look through constructors for
			// a compatible match.
			ctors = type.GetConstructors(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy);

			this.CollectionCtors = new Dictionary<Type, FactoryDelegate>(ctors.Length);

			foreach (ConstructorInfo ctor in ctors)
			{
				ParameterInfo[] paramList = ctor.GetParameters();
				if (paramList.Length != 1)
				{
					continue;
				}

				Type argType = paramList[0].ParameterType;
				if ((argType == typeof(string)) ||
					(
#if !NETCF
					(argType.GetInterface(TypeCoercionUtility.TypeGenericIEnumerable, false) == null) &&
#endif
					(typeof(IEnumerable).IsAssignableFrom(argType))))
				{
					continue;
				}

				// save all constructors that can take an enumerable of objects
				this.CollectionCtors[argType] = DynamicMethodGenerator.GetTypeFactory(ctor);
			}

			if (this.Ctor == null)
			{
				// try to grab a private ctor if exists
				this.Ctor = DynamicMethodGenerator.GetTypeFactory(type);
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
			Type collectionType = null;
#if !NETCF
			collectionType = type.GetInterface(TypeCoercionUtility.TypeGenericICollection, false);
#endif
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
#if NETCF
					return new Type[]{ };
#else
					return Type.EmptyTypes;
#endif
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

		#region Utility Methods

		internal static bool IsInvalidType(Type type)
		{
			// this blows up for some reason with KeyValue<string, object>
			//return (type.IsInterface || type.IsAbstract || (type.IsValueType && !type.IsSerializable));

			return (type.IsInterface || type.IsAbstract || type.IsValueType);
		}

		internal static bool IsImmutableType(Type type)
		{
			// anonymous and other immutable types typically take all properties as params having no default constructor
			return
				!type.IsInterface &&
				!type.IsAbstract &&
				(type.IsValueType ||
				 (type.GetConstructor(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy,
				                      null,
#if NETCF
									  new Type[]{ },
#else
				                      Type.EmptyTypes,
#endif
				                      null) == null));
		}

		#endregion Utility Methods
	}

	/// <summary>
	/// Cache of name resolution mappings for IDataReader / IDataWriter
	/// </summary>
	public sealed class ResolverCache
	{
		#region Fields

#if SILVERLIGHT && (NET20 || NET30 || NET35)
		// no reader-writer lock implementation
#elif NET20 || NET30
		private const int LockTimeout = 250;

		private readonly System.Threading.ReaderWriterLock MapLock = new System.Threading.ReaderWriterLock();
		private readonly System.Threading.ReaderWriterLock FactoryLock = new System.Threading.ReaderWriterLock();
#elif NET35
		private readonly System.Threading.ReaderWriterLockSlim MapLock = new System.Threading.ReaderWriterLockSlim();
		private readonly System.Threading.ReaderWriterLockSlim FactoryLock = new System.Threading.ReaderWriterLockSlim();
#endif

		private readonly IDictionary<Type, IDictionary<string, MemberMap>> MemberCache = new MemberCacheDictionary();
		private readonly IDictionary<Type, IDictionary<Enum, string>> EnumCache = new EnumCacheDictionary();
		private readonly IDictionary<Type, FactoryMap> Factories = new FactoriesDictionary();
		private readonly IDictionary<Type, IEnumerable<DataName>> NameCache = new NameCacheDictionary();

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

		#region Sort Methods

		public IEnumerable<MemberMap> SortMembers(IEnumerable<MemberMap> members)
		{
			return this.Strategy.SortMembers(members) ?? members;
		}

		#endregion Sort Methods

		#region Map Methods

		/// <summary>
		/// Gets the serialized name of the class
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<DataName> LoadTypeName(Type type)
		{
			if (type == null)
			{
				return new [] { new DataName(type) };
			}

			IEnumerable<DataName> name;

#if SILVERLIGHT && (NET20 || NET30 || NET35)
			lock (this.NameCache)
#elif NET20 || NET30
			this.MapLock.AcquireReaderLock(ResolverCache.LockTimeout);
#elif NET35
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
#if SILVERLIGHT && (NET20 || NET30 || NET35)
				// noop
#elif NET20 || NET30
				this.MapLock.ReleaseReaderLock();
#elif NET35
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

#if SILVERLIGHT && (NET20 || NET30 || NET35)
			lock (this.MemberCache)
#elif NET20 || NET30
			this.MapLock.AcquireReaderLock(ResolverCache.LockTimeout);
#elif NET35
			this.MapLock.EnterReadLock();
#endif
#if !NET40
			try
#endif
			{
				if (this.MemberCache.TryGetValue(type, out map))
				{
					return map;
				}
			}
#if !NET40
			finally
#endif
			{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
				// noop
#elif NET20 || NET30
				this.MapLock.ReleaseReaderLock();
#elif NET35
				this.MapLock.ExitReadLock();
#endif
			}

			this.BuildMap(type, out map);
			return map;
		}

		public MemberMap LoadMemberMap(MemberInfo member)
		{
			IDictionary<string, MemberMap> maps = this.LoadMaps(member.DeclaringType);

			foreach (MemberMap map in maps.Values)
			{
				if (Object.Equals(map.MemberInfo, member))
				{
					return map;
				}
			}

			return null;
		}

		public IDictionary<Enum, string> LoadEnumMaps(Type type)
		{
			if (type == null || !type.IsEnum)
			{
				return null;
			}

			IDictionary<Enum, string> map;

#if SILVERLIGHT && (NET20 || NET30 || NET35)
			lock (this.EnumCache)
#elif NET20 || NET30
			this.MapLock.AcquireReaderLock(ResolverCache.LockTimeout);
#elif NET35
			this.MapLock.EnterReadLock();
#endif
#if !NET40
			try
#endif
			{
				if (this.EnumCache.TryGetValue(type, out map))
				{
					return map;
				}
			}
#if !NET40
			finally
#endif
			{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
				// noop
#elif NET20 || NET30
				this.MapLock.ReleaseReaderLock();
#elif NET35
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
#if SILVERLIGHT && (NET20 || NET30 || NET35)
			lock (this.NameCache)
			lock (this.MemberCache)
			lock (this.EnumCache)
			lock (this.Factories)
#elif NET20 || NET30
			this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
			this.FactoryLock.AcquireWriterLock(ResolverCache.LockTimeout);
#elif NET35
			this.MapLock.EnterWriteLock();
			this.FactoryLock.EnterWriteLock();
#endif
#if !NET40
			try
#endif
			{
				this.NameCache.Clear();
				this.MemberCache.Clear();
				this.EnumCache.Clear();
				this.Factories.Clear();
			}
#if !NET40
			finally
#endif
			{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
				// noop
#elif NET20 || NET30
				this.FactoryLock.ReleaseWriterLock();
				this.MapLock.ReleaseWriterLock();
#elif NET35
				this.FactoryLock.ExitWriteLock();
				this.MapLock.ExitWriteLock();
#endif
			}
		}

		/// <summary>
		/// Builds a mapping of member name to field/property
		/// </summary>
		/// <param name="objectType"></param>
		private IEnumerable<DataName> BuildMap(Type objectType, out IDictionary<string, MemberMap> maps)
		{
			bool hasName = false;
			IEnumerable<DataName> typeNames = this.Strategy.GetName(objectType);
			if (typeNames != null)
			{
				foreach (DataName typeName in typeNames)
				{
					if (!typeName.IsEmpty)
					{
						hasName = true;
						break;
					}
				}
			}
			if (!hasName)
			{
				typeNames = new [] { new DataName(objectType) };
			}

			if (objectType.IsEnum)
			{
				// create special maps for enum types
				IDictionary<Enum, string> enumMap;
				maps = this.BuildEnumMap(objectType, out enumMap);
				return typeNames;
			}

			// do not incurr the cost of member map for dictionaries
			if (typeof(IDictionary<string, object>).IsAssignableFrom(objectType) ||
				typeof(IDictionary).IsAssignableFrom(objectType))
			{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
				lock (this.MemberCache)
				lock (this.NameCache)
#elif NET20 || NET30
				this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
#elif NET35
				this.MapLock.EnterWriteLock();
#endif
#if !NET40
				try
#endif
				{
					// store marker in cache for future lookups
					maps = (this.MemberCache[objectType] = null);
					return (this.NameCache[objectType] = typeNames);
				}
#if !NET40
				finally
#endif
				{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
					// noop
#elif NET20 || NET30
					this.MapLock.ReleaseWriterLock();
#elif NET35
					this.MapLock.ExitWriteLock();
#endif
				}
			}

			// create new mapping
			maps = new Dictionary<string, MemberMap>();

			bool isImmutableType = FactoryMap.IsImmutableType(objectType);

			// load properties into property map
			foreach (PropertyInfo info in objectType.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy))
			{
				// Ignore indexer properties as aren't serialized
				// https://bugzilla.xamarin.com/show_bug.cgi?id=6821#c5
				if (info.GetIndexParameters().Length != 0 ||
					this.Strategy.IsPropertyIgnored(info, isImmutableType))
				{
					continue;
				}

				IEnumerable<DataName> names = this.Strategy.GetName(info);
				hasName = false;
				if (names != null)
				{
					foreach (DataName name in names)
					{
						if (!name.IsEmpty)
						{
							hasName = true;
							break;
						}
					}
				}
				if (!hasName)
				{
					names = new[] { new DataName(info.Name) };
				}

				ValueIgnoredDelegate isIgnored = this.Strategy.GetValueIgnoredCallback(info);
				MemberMap map = null;

				foreach (DataName name in names)
				{
					if (name.IsEmpty || maps.ContainsKey(name.LocalName))
					{
						continue;
					}

					if (map == null)
					{
						maps[name.LocalName] = map = new MemberMap(info, name, isIgnored);
					}
					else
					{
						maps[name.LocalName] = new MemberMap(map, name);
					}
				}
			}

			// load fields into property map
			foreach (FieldInfo info in objectType.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy))
			{
				if (this.Strategy.IsFieldIgnored(info))
				{
					continue;
				}

				IEnumerable<DataName> names = this.Strategy.GetName(info);
				hasName = false;
				if (names != null)
				{
					foreach (DataName name in names)
					{
						if (!name.IsEmpty)
						{
							hasName = true;
							break;
						}
					}
				}
				if (!hasName)
				{
					names = new[] { new DataName(info.Name) };
				}

				ValueIgnoredDelegate isIgnored = this.Strategy.GetValueIgnoredCallback(info);
				MemberMap map = null;

				foreach (DataName name in names)
				{
					if (name.IsEmpty || maps.ContainsKey(name.LocalName))
					{
						continue;
					}

					if (map == null)
					{
						maps[name.LocalName] = map = new MemberMap(info, name, isIgnored);
					}
					else
					{
						maps[name.LocalName] = new MemberMap(map, name);
					}
				}
			}

#if SILVERLIGHT && (NET20 || NET30 || NET35)
			lock (this.MemberCache)
			lock (this.NameCache)
#elif NET20 || NET30
			this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
#elif NET35
			this.MapLock.EnterWriteLock();
#endif
#if !NET40
			try
#endif
			{
				// store in cache for future requests
				this.MemberCache[objectType] = maps;
				return (this.NameCache[objectType] = typeNames);
			}
#if !NET40
			finally
#endif
			{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
				// noop
#elif NET20 || NET30
				this.MapLock.ReleaseWriterLock();
#elif NET35
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
			bool hasName = false;
			IEnumerable<DataName> typeNames = this.Strategy.GetName(enumType);
			if (typeNames != null)
			{
				foreach (DataName typeName in typeNames)
				{
					if (!typeName.IsEmpty)
					{
						hasName = true;
						break;
					}
				}
			}
			if (!hasName)
			{
				typeNames = new[] { new DataName(enumType) };
			}

			IDictionary<string, MemberMap> maps = new Dictionary<string, MemberMap>();
			enumMaps = new Dictionary<Enum, string>();

			foreach (FieldInfo info in enumType.GetFields(BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy))
			{
				hasName = false;
				IEnumerable<DataName> names = this.Strategy.GetName(info);
				if (names != null)
				{
					foreach (DataName name in names)
					{
						if (!name.IsEmpty)
						{
							hasName = true;
							break;
						}
					}
				}
				if (!hasName)
				{
					names = new[] { new DataName(info.Name) };
				}

				MemberMap map = null;

				foreach (DataName name in names)
				{
					if (name.IsEmpty || maps.ContainsKey(name.LocalName))
					{
						continue;
					}

					if (map == null)
					{
						maps[name.LocalName] = map = new MemberMap(info, name, null);
						enumMaps[(Enum)map.Getter(null)] = name.LocalName;
					}
					else
					{
						maps[name.LocalName] = new MemberMap(map, name);
					}
				}
			}

#if SILVERLIGHT && (NET20 || NET30 || NET35)
			lock(this.NameCache)
			lock(this.EnumCache)
			lock(this.MemberCache)
#elif NET20 || NET30
			this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
#elif NET35
			this.MapLock.EnterWriteLock();
#endif
#if !NET40
			try
#endif
			{
				// store in caches for future requests
				this.NameCache[enumType] = typeNames;
				this.EnumCache[enumType] = enumMaps;
				return (this.MemberCache[enumType] = maps);
			}
#if !NET40
			finally
#endif
			{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
				// noop
#elif NET20 || NET30
				this.MapLock.ReleaseWriterLock();
#elif NET35
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

#if SILVERLIGHT && (NET20 || NET30 || NET35)
			lock (this.Factories)
#elif NET20 || NET30
			this.FactoryLock.AcquireReaderLock(ResolverCache.LockTimeout);
#elif NET35
			this.FactoryLock.EnterReadLock();
#endif
#if !NET40
			try
#endif
			{
				if (this.Factories.TryGetValue(type, out map))
				{
					return map;
				}
			}
#if !NET40
			finally
#endif
			{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
				// noop
#elif NET20 || NET30
				this.FactoryLock.ReleaseReaderLock();
#elif NET35
				this.FactoryLock.ExitReadLock();
#endif
			}

			map = new FactoryMap(type);

#if SILVERLIGHT && (NET20 || NET30 || NET35)
			lock (this.Factories)
#elif NET20 || NET30
			this.FactoryLock.AcquireWriterLock(ResolverCache.LockTimeout);
#elif NET35
			this.FactoryLock.EnterWriteLock();
#endif
#if !NET40
			try
#endif
			{
				// store in cache for future requests
				return (this.Factories[type] = map);
			}
#if !NET40
			finally
#endif
			{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
				// noop
#elif NET20 || NET30
				this.FactoryLock.ReleaseWriterLock();
#elif NET35
				this.FactoryLock.ExitWriteLock();
#endif
			}
		}

		#endregion Factory Methods
	}
}
