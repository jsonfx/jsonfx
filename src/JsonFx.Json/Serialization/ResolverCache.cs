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

		#endregion Properties
	}

	/// <summary>
	/// Caches name resolution mappings for IDataReader / IDataWriter
	/// </summary>
	public sealed class ResolverCache :
		IResolverStrategy
	{
		#region Constants

		private const string AnonymousTypePrefix = "<>f__AnonymousType";
		private const int LockTimeout = 250;

		#endregion Constants

		#region Fields

#if NET20 || NET30
		private readonly ReaderWriterLock Lock = new ReaderWriterLock();
#else
		private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
#endif

		private readonly IDictionary<Type, IDictionary<string, MemberMap>> Cache = new Dictionary<Type, IDictionary<string, MemberMap>>();
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

#if NET20 || NET30
			this.Lock.AcquireReaderLock(ResolverCache.LockTimeout);
#else
			this.Lock.EnterReadLock();
#endif
			try
			{
				if (this.Cache.ContainsKey(type))
				{
					return this.Cache[type];
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

			// create new map
			IDictionary<string, MemberMap> map = new Dictionary<string, MemberMap>();

			if (!objectType.IsEnum)
			{
				bool isAnonymousType = objectType.IsGenericType && objectType.Name.StartsWith(ResolverCache.AnonymousTypePrefix);

				// load properties into property map
				foreach (PropertyInfo info in objectType.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
				{
					if (this.Strategy.IsPropertyIgnored(info, isAnonymousType) ||
							this.Strategy.IsIgnored(info))
					{
						continue;
					}

					string name = this.Strategy.GetName(info);
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
				if (this.Strategy.IsFieldIgnored(info) ||
						this.Strategy.IsIgnored(info))
				{
					continue;
				}

				string name = this.Strategy.GetName(info);
				if (String.IsNullOrEmpty(name))
				{
					name = info.Name;
				}

				map[name] = new MemberMap(info);
			}

#if NET20 || NET30
			this.Lock.AcquireWriterLock(ResolverCache.LockTimeout);
#else
			this.Lock.EnterWriteLock();
#endif
			try
			{
				// store in cache for future usage
				return (this.Cache[objectType] = map);
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

		#region IResolverStrategy Members

		public bool IsPropertyIgnored(PropertyInfo member, bool isAnonymousType)
		{
			return this.Strategy.IsPropertyIgnored(member, isAnonymousType);
		}

		public bool IsFieldIgnored(FieldInfo member)
		{
			return this.Strategy.IsFieldIgnored(member);
		}

		public bool IsIgnored(MemberInfo member)
		{
			return this.Strategy.IsIgnored(member);
		}

		public bool IsValueIgnored(MemberInfo member, object target, object value)
		{
			return this.Strategy.IsValueIgnored(member, target, value);
		}

		public bool IsDefaultValue(MemberInfo member, object value)
		{
			return this.Strategy.IsDefaultValue(member, value);
		}

		public string GetName(MemberInfo member)
		{
			return this.Strategy.GetName(member);
		}

		public string GetName(Enum value)
		{
			return this.Strategy.GetName(value);
		}

		#endregion IResolverStrategy Members
	}
}
