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

		public ValueIgnoredDelegate IsIgnored
		{
			get;
			private set;
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
		private readonly ReaderWriterLock Lock = new ReaderWriterLock();
#else
		private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
#endif

		private readonly IDictionary<Type, IDictionary<string, MemberMap>> Cache = new Dictionary<Type, IDictionary<string, MemberMap>>();
		private readonly IDictionary<Type, IDictionary<Enum, string>> EnumCache = new Dictionary<Type, IDictionary<Enum, string>>();
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
			this.Lock.AcquireReaderLock(ResolverCache.LockTimeout);
#else
			this.Lock.EnterReadLock();
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
				this.Lock.ReleaseReaderLock();
#else
				this.Lock.ExitReadLock();
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
			this.Lock.AcquireReaderLock(ResolverCache.LockTimeout);
#else
			this.Lock.EnterReadLock();
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
				this.Lock.ReleaseReaderLock();
#else
				this.Lock.ExitReadLock();
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
			this.Lock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.Lock.EnterWriteLock();
#endif
			try
			{
				this.Cache.Clear();
			}
			finally
			{
#if NET20 || NET30
				this.Lock.ReleaseWriterLock();
#else
				this.Lock.ExitWriteLock();
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
				this.Lock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
				this.Lock.EnterWriteLock();
#endif
				try
				{
					// store marker in cache for future lookups
					return (this.Cache[objectType] = null);
				}
				finally
				{
#if NET20 || NET30
					this.Lock.ReleaseWriterLock();
#else
					this.Lock.ExitWriteLock();
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
			this.Lock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.Lock.EnterWriteLock();
#endif
			try
			{
				// store in cache for future usage
				return (this.Cache[objectType] = maps);
			}
			finally
			{
#if NET20 || NET30
				this.Lock.ReleaseWriterLock();
#else
				this.Lock.ExitWriteLock();
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
			this.Lock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.Lock.EnterWriteLock();
#endif
			try
			{
				// store in caches for future usage
				this.Cache[enumType] = maps;
				return (this.EnumCache[enumType] = enumMaps);
			}
			finally
			{
#if NET20 || NET30
				this.Lock.ReleaseWriterLock();
#else
				this.Lock.ExitWriteLock();
#endif
			}
		}

		#endregion Map Methods
	}
}
