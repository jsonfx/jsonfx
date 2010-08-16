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

using JsonFx.CodeGen;
using JsonFx.Serialization;
using JsonFx.Serialization.Filters;
using JsonFx.Serialization.GraphCycles;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Common
{
	/// <summary>
	/// Generates a sequence of tokens from an object graph
	/// </summary>
	public class CommonWalker : IObjectWalker<CommonTokenType>
	{
		#region Fields

		private readonly DataWriterSettings Settings;
		private readonly IEnumerable<IDataFilter<CommonTokenType>> Filters;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public CommonWalker(DataWriterSettings settings, params IDataFilter<CommonTokenType>[] filters)
			: this(settings, (IEnumerable<IDataFilter<CommonTokenType>>)filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public CommonWalker(DataWriterSettings settings, IEnumerable<IDataFilter<CommonTokenType>> filters)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}
			this.Settings = settings;

			if (filters == null)
			{
				filters = new IDataFilter<CommonTokenType>[0];
			}
			this.Filters = filters;

			foreach (var filter in filters)
			{
				if (filter == null)
				{
					throw new ArgumentNullException("filters");
				}
			}
		}

		#endregion Init

		#region IObjectWalker<T> Methods

		/// <summary>
		/// Generates a sequence of tokens representing the value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public IEnumerable<Token<CommonTokenType>> GetTokens(object value)
		{
			ICycleDetector detector;
			switch (this.Settings.GraphCycles)
			{
				case GraphCycleType.MaxDepth:
				{
					detector = new DepthCounter(this.Settings.MaxDepth);
					break;
				}
				default:
				{
					detector = new ReferenceSet();
					break;
				}
			}

			List<Token<CommonTokenType>> tokens = new List<Token<CommonTokenType>>();
			this.GetTokens(tokens, detector, value);
			return tokens;
		}

		#endregion IObjectWalker<T> Methods

		#region Walker Methods

		private void GetTokens(List<Token<CommonTokenType>> tokens, ICycleDetector detector, object value)
		{
			if (value == null)
			{
				tokens.Add(CommonGrammar.TokenNull);
				return;
			}

			// test for cycles
			if (detector.Add(value))
			{
				switch (this.Settings.GraphCycles)
				{
					case GraphCycleType.Reference:
					{
						// no need to remove value as was duplicate reference
						throw new GraphCycleException(GraphCycleType.Reference, "Graph cycle detected: repeated references");
					}
					case GraphCycleType.MaxDepth:
					{
						throw new GraphCycleException(GraphCycleType.MaxDepth, "Graph cycle potentially detected: maximum depth exceeded");
					}
					default:
					case GraphCycleType.Ignore:
					{
						// no need to remove value as was duplicate reference
						// replace cycle with null
						tokens.Add(CommonGrammar.TokenNull);
						return;
					}
				}
			}

			try
			{
				foreach (var filter in this.Filters)
				{
					IEnumerable<Token<CommonTokenType>> filterResult;
					if (filter.TryWrite(this.Settings, value, out filterResult))
					{
						// found a successful match
						tokens.AddRange(filterResult);
						return;
					}
				}

				Type type = value.GetType();

				// must test enumerations before other value types
				if (type.IsEnum)
				{
					tokens.Add(CommonGrammar.TokenPrimitive((Enum)value));
					return;
				}

				// Type.GetTypeCode() allows us to more efficiently switch type
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Boolean:
					{
						tokens.Add(true.Equals(value) ? CommonGrammar.TokenTrue : CommonGrammar.TokenFalse);
						return;
					}
					case TypeCode.Byte:
					case TypeCode.Decimal:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.SByte:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
					case TypeCode.UInt64:
					{
						tokens.Add(CommonGrammar.TokenPrimitive((ValueType)value));
						return;
					}
					case TypeCode.Double:
					{
						double doubleVal = (double)value;

						if (Double.IsNaN(doubleVal))
						{
							tokens.Add(CommonGrammar.TokenNaN);
						}
						else if (Double.IsPositiveInfinity(doubleVal))
						{
							tokens.Add(CommonGrammar.TokenPositiveInfinity);
						}
						else if (Double.IsNegativeInfinity(doubleVal))
						{
							tokens.Add(CommonGrammar.TokenNegativeInfinity);
						}
						else
						{
							tokens.Add(CommonGrammar.TokenPrimitive(doubleVal));
						}
						return;
					}
					case TypeCode.Single:
					{
						float floatVal = (float)value;

						if (Single.IsNaN(floatVal))
						{
							// use the Double equivalent
							tokens.Add(CommonGrammar.TokenNaN);
						}
						else if (Single.IsPositiveInfinity(floatVal))
						{
							// use the Double equivalent
							tokens.Add(CommonGrammar.TokenPositiveInfinity);
						}
						else if (Single.IsNegativeInfinity(floatVal))
						{
							// use the Double equivalent
							tokens.Add(CommonGrammar.TokenNegativeInfinity);
						}
						else
						{
							tokens.Add(CommonGrammar.TokenPrimitive(floatVal));
						}
						return;
					}
					case TypeCode.Char:
					case TypeCode.DateTime:
					case TypeCode.String:
					{
						tokens.Add(CommonGrammar.TokenPrimitive(value));
						return;
					}
					case TypeCode.DBNull:
					case TypeCode.Empty:
					{
						tokens.Add(CommonGrammar.TokenNull);
						return;
					}
				}

				if (value is IEnumerable)
				{
					this.GetArrayTokens(tokens, detector, (IEnumerable)value);
					return;
				}

				if (value is Guid || value is Uri || value is Version)
				{
					tokens.Add(CommonGrammar.TokenPrimitive(value));
					return;
				}

				if (value is TimeSpan)
				{
					tokens.Add(CommonGrammar.TokenPrimitive((TimeSpan)value));
					return;
				}

#if NET40 && !WINDOWS_PHONE
				if (value is System.Dynamic.DynamicObject)
				{
					// TODO: expand to all IDynamicMetaObjectProvider?
					this.GetObjectTokens(tokens, detector, type, (System.Dynamic.DynamicObject)value);
					return;
				}
#endif

				// all other structs and classes
				this.GetObjectTokens(tokens, detector, type, value);
			}
			finally
			{
				detector.Remove(value);
			}
		}

		private void GetArrayTokens(List<Token<CommonTokenType>> tokens, ICycleDetector detector, IEnumerable value)
		{
			DataName typeName = this.GetTypeName(value);
			IEnumerator enumerator = value.GetEnumerator();

			if (enumerator is IEnumerator<KeyValuePair<string, object>>)
			{
				this.GetObjectTokens(tokens, detector, typeName, (IEnumerator<KeyValuePair<string, object>>)enumerator);
				return;
			}

			if (enumerator is IDictionaryEnumerator)
			{
				this.GetObjectTokens(tokens, detector, typeName, (IDictionaryEnumerator)enumerator);
				return;
			}

			tokens.Add(CommonGrammar.TokenArrayBegin(typeName));

			while (enumerator.MoveNext())
			{
				this.GetTokens(tokens, detector, enumerator.Current);
			}

			tokens.Add(CommonGrammar.TokenArrayEnd);
		}

		private void GetObjectTokens(List<Token<CommonTokenType>> tokens, ICycleDetector detector, DataName typeName, IDictionaryEnumerator enumerator)
		{
			tokens.Add(CommonGrammar.TokenObjectBegin(typeName));

			while (enumerator.MoveNext())
			{
				tokens.Add(CommonGrammar.TokenProperty(enumerator.Key));
				this.GetTokens(tokens, detector, enumerator.Value);
			}

			tokens.Add(CommonGrammar.TokenObjectEnd);
		}

		private void GetObjectTokens(List<Token<CommonTokenType>> tokens, ICycleDetector detector, DataName typeName, IEnumerator<KeyValuePair<string, object>> enumerator)
		{
			tokens.Add(CommonGrammar.TokenObjectBegin(typeName));

			while (enumerator.MoveNext())
			{
				KeyValuePair<string, object> pair = enumerator.Current;
				tokens.Add(CommonGrammar.TokenProperty(pair.Key));
				this.GetTokens(tokens, detector, pair.Value);
			}

			tokens.Add(CommonGrammar.TokenObjectEnd);
		}

#if NET40 && !WINDOWS_PHONE
		private void GetObjectTokens(List<Token<CommonTokenType>> tokens, ICycleDetector detector, Type type, System.Dynamic.DynamicObject value)
		{
			DataName typeName = this.GetTypeName(value);
			tokens.Add(CommonGrammar.TokenObjectBegin(typeName));

			foreach (var memberName in value.GetDynamicMemberNames())
			{
				object propertyValue;
				if (!value.TryGetMember(new DynamicGetter(memberName), out propertyValue))
				{
					continue;
				}

				tokens.Add(CommonGrammar.TokenProperty(memberName));
				this.GetTokens(tokens, detector, propertyValue);
			}

			tokens.Add(CommonGrammar.TokenObjectEnd);
		}
#endif

		private void GetObjectTokens(List<Token<CommonTokenType>> tokens, ICycleDetector detector, Type type, object value)
		{
			DataName typeName = this.GetTypeName(value);
			tokens.Add(CommonGrammar.TokenObjectBegin(typeName));

			IDictionary<string, MemberMap> maps = this.Settings.Resolver.LoadMaps(type);
			if (maps == null)
			{
				// TODO: verify no other valid situations here
				tokens.Add(CommonGrammar.TokenObjectEnd);
				return;
			}

			// allow the resolver to optionally sort the members
			IEnumerable<MemberMap> members = this.Settings.Resolver.SortMembers(maps.Values);

			foreach (var map in members)
			{
				if (map.IsAlternate ||
					map.Getter == null)
				{
					continue;
				}

				object propertyValue = map.Getter(value);
				if (map.IsIgnored != null &&
					map.IsIgnored(value, propertyValue))
				{
					continue;
				}

				tokens.Add(CommonGrammar.TokenProperty(map.DataName));
				this.GetTokens(tokens, detector, propertyValue);
			}

			tokens.Add(CommonGrammar.TokenObjectEnd);
		}

		#endregion Walker Methods

		#region Utility Methods

		private DataName GetTypeName(object value)
		{
			IEnumerable<DataName> typeNames = this.Settings.Resolver.LoadTypeName((value != null) ? value.GetType() : null);

			if (typeNames != null)
			{
				foreach (DataName n in typeNames)
				{
					if (!n.IsEmpty)
					{
						return n;
					}
				}
			}

			return DataName.Empty;
		}

		#endregion Utility Methods
	}
}