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

namespace JsonFx.Serialization
{
	/// <summary>
	/// Caches name resolution mappings for IDataReader / IDataWriter
	/// </summary>
	public class MemberCache
	{
		#region Constants

		private const string AnonymousTypePrefix = "<>f__AnonymousType";

		#endregion Constants

		#region Fields

		private readonly Dictionary<Type, IDictionary<string, MemberInfo>> MapCache = new Dictionary<Type, IDictionary<string, MemberInfo>>();
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

		public IDictionary<string, MemberInfo> GetMap(Type objectType)
		{
			lock (this.MapCache)
			{
				if (!this.MapCache.ContainsKey(objectType))
				{
					this.BuildMap(objectType);
				}

				// map was stored in cache
				return this.MapCache[objectType];
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
		private void BuildMap(Type objectType)
		{
			lock (this.MapCache)
			{
				// do not incurr the cost of member map for dictionaries
				if (typeof(IDictionary).IsAssignableFrom(objectType))
				{
					// store marker in cache for future lookups
					this.MapCache[objectType] = null;
					return;
				}

				// create new maps
				IDictionary<string, MemberInfo> map = new Dictionary<string, MemberInfo>();

				if (!objectType.IsEnum)
				{
					bool isAnonymousType = objectType.IsGenericType && objectType.Name.StartsWith(MemberCache.AnonymousTypePrefix);

					// load properties into property map
					foreach (PropertyInfo info in objectType.GetProperties())
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

						map[info.Name] = info;
					}
				}

				// load fields into property map
				foreach (FieldInfo info in objectType.GetFields())
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

					map[name] = info;
				}

				// store in cache for future usage
				this.MapCache[objectType] = map;
			}
		}

		#endregion Map Methods
	}
}
