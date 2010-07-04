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

namespace JsonFx.Serialization
{
	public class MemberMap
	{
		#region Fields

		public readonly MemberInfo MemberInfo;

		public readonly string Name;

		public readonly Type Type;

		public readonly GetterDelegate Getter;

		public readonly SetterDelegate Setter;

		public readonly ValueIgnoredDelegate IsIgnored;

		#endregion Fields

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
	}

	public class FactoryMap
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

		private readonly IDictionary<Type, IDictionary<string, MemberMap>> Cache = new Dictionary<Type, IDictionary<string, MemberMap>>();
		private readonly IDictionary<Type, IDictionary<Enum, string>> EnumCache = new Dictionary<Type, IDictionary<Enum, string>>();
		private readonly IDictionary<Type, FactoryMap> Factories = new Dictionary<Type, FactoryMap>();
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
				if (this.Cache.TryGetValue(type, out map))
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

			return this.BuildMap(type);
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

			return this.BuildEnumMap(type);
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
				this.Cache.Clear();
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
		private IDictionary<string, MemberMap> BuildMap(Type objectType)
		{
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
					return (this.Cache[objectType] = null);
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

			IDictionary<string, MemberMap> maps;
			if (objectType.IsEnum)
			{
				// create special maps for enum types
				this.BuildEnumMap(objectType);
				return this.Cache.TryGetValue(objectType, out maps) ? maps : null;
			}

			// create new map
			maps = new Dictionary<string, MemberMap>();

			bool isAnonymousType = objectType.IsGenericType && objectType.Name.StartsWith(ResolverCache.AnonymousTypePrefix, false, CultureInfo.InvariantCulture);

			// load properties into property map
			foreach (PropertyInfo info in objectType.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
			{
				if (this.Strategy.IsPropertyIgnored(info, isAnonymousType))
				{
					continue;
				}

				string name = this.Strategy.GetName(info);
				if (String.IsNullOrEmpty(name))
				{
					name = info.Name;
				}

				maps[name] = new MemberMap(info);
			}

			// load fields into property map
			foreach (FieldInfo info in objectType.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
			{
				if (this.Strategy.IsFieldIgnored(info))
				{
					continue;
				}

				string name = this.Strategy.GetName(info);
				if (String.IsNullOrEmpty(name))
				{
					name = info.Name;
				}

				maps[name] = new MemberMap(info);
			}

#if NET20 || NET30
			this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.MapLock.EnterWriteLock();
#endif
			try
			{
				// store in cache for future requests
				return (this.Cache[objectType] = maps);
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

		private IDictionary<Enum, string> BuildEnumMap(Type enumType)
		{
			if (enumType == null || !enumType.IsEnum)
			{
				return null;
			}

			// create new maps
			IDictionary<string, MemberMap> maps = new Dictionary<string, MemberMap>();
			IDictionary<Enum, string> enumMaps = new Dictionary<Enum, string>();

			foreach (FieldInfo info in enumType.GetFields(BindingFlags.Static|BindingFlags.Public))
			{
				string name = this.Strategy.GetName(info);
				if (String.IsNullOrEmpty(name))
				{
					name = info.Name;
				}

				MemberMap enumMap;
				maps[name] = enumMap = new MemberMap(info);
				enumMaps[(Enum)enumMap.Getter(null)] = name;
			}


#if NET20 || NET30
			this.MapLock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.MapLock.EnterWriteLock();
#endif
			try
			{
				// store in caches for future requests
				this.Cache[enumType] = maps;
				return (this.EnumCache[enumType] = enumMaps);
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
