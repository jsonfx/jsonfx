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

using JsonFx.Serialization;
using JsonFx.Serialization.Filters;
using JsonFx.Serialization.GraphCycles;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Common
{
	/// <summary>
	/// Generates a SAX-like sequence of tokens from an object graph
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

			foreach (var filter in this.Filters)
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
			Queue<Token<CommonTokenType>> tokens = new Queue<Token<CommonTokenType>>();

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

			this.GetTokens(tokens, detector, value);

			return tokens;
		}

		#endregion IObjectWalker<T> Methods

		#region Walker Methods

		private void GetTokens(Queue<Token<CommonTokenType>> tokens, ICycleDetector detector, object value)
		{
			if (value == null)
			{
				tokens.Enqueue(CommonGrammar.TokenNull);
				return;
			}

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
						tokens.Enqueue(CommonGrammar.TokenNull);
						return;
					}
				}
			}

			try
			{
				foreach (var filter in this.Filters)
				{
					IEnumerable<Token<CommonTokenType>> filterTokens;
					if (filter.TryWrite(this.Settings, value, out filterTokens))
					{
						// found a successful match
						foreach (Token<CommonTokenType> token in filterTokens)
						{
							tokens.Enqueue(token);
						}
						return;
					}
				}

				Type type = value.GetType();

				// must test enumerations before other value types
				if (type.IsEnum)
				{
					tokens.Enqueue(CommonGrammar.TokenValue((Enum)value));
					return;
				}

				// Type.GetTypeCode() allows us to more efficiently switch type
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Boolean:
					{
						tokens.Enqueue(true.Equals(value) ? CommonGrammar.TokenTrue : CommonGrammar.TokenFalse);
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
						tokens.Enqueue(CommonGrammar.TokenValue((ValueType)value));
						return;
					}
					case TypeCode.Double:
					{
						double doubleVal = (double)value;

						if (Double.IsNaN(doubleVal))
						{
							tokens.Enqueue(CommonGrammar.TokenNaN);
						}
						else if (Double.IsPositiveInfinity(doubleVal))
						{
							tokens.Enqueue(CommonGrammar.TokenPositiveInfinity);
						}
						else if (Double.IsNegativeInfinity(doubleVal))
						{
							tokens.Enqueue(CommonGrammar.TokenNegativeInfinity);
						}
						else
						{
							tokens.Enqueue(CommonGrammar.TokenValue(doubleVal));
						}
						return;
					}
					case TypeCode.Single:
					{
						float floatVal = (float)value;

						if (Single.IsNaN(floatVal))
						{
							tokens.Enqueue(CommonGrammar.TokenNaN);
						}
						else if (Single.IsPositiveInfinity(floatVal))
						{
							tokens.Enqueue(CommonGrammar.TokenPositiveInfinity);
						}
						else if (Single.IsNegativeInfinity(floatVal))
						{
							tokens.Enqueue(CommonGrammar.TokenNegativeInfinity);
						}
						else
						{
							tokens.Enqueue(CommonGrammar.TokenValue(floatVal));
						}
						return;
					}
					case TypeCode.Char:
					case TypeCode.DateTime:
					case TypeCode.String:
					{
						tokens.Enqueue(CommonGrammar.TokenValue(value));
						return;
					}
					case TypeCode.DBNull:
					case TypeCode.Empty:
					{
						tokens.Enqueue(CommonGrammar.TokenNull);
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
					tokens.Enqueue(CommonGrammar.TokenValue(value));
					return;
				}

				if (value is TimeSpan)
				{
					tokens.Enqueue(CommonGrammar.TokenValue((TimeSpan)value));
					return;
				}

				// all other structs and classes
				this.GetObjectTokens(tokens, detector, value, type);
			}
			finally
			{
				detector.Remove(value);
			}
		}

		private void GetArrayTokens(Queue<Token<CommonTokenType>> tokens, ICycleDetector detector, IEnumerable value)
		{
			IEnumerator enumerator = value.GetEnumerator();

			if (enumerator is IEnumerator<KeyValuePair<string, object>>)
			{
				this.GetObjectTokens(tokens, detector, (IEnumerator<KeyValuePair<string, object>>)enumerator);
				return;
			}

			if (enumerator is IDictionaryEnumerator)
			{
				this.GetObjectTokens(tokens, detector, (IDictionaryEnumerator)enumerator);
				return;
			}

			tokens.Enqueue(CommonGrammar.TokenArrayBegin);

			bool appendDelim = false;

			while (enumerator.MoveNext())
			{
				if (appendDelim)
				{
					tokens.Enqueue(CommonGrammar.TokenValueDelim);
				}
				else
				{
					appendDelim = true;
				}

				this.GetTokens(tokens, detector, enumerator.Current);
			}

			tokens.Enqueue(CommonGrammar.TokenArrayEnd);
		}

		private void GetObjectTokens(Queue<Token<CommonTokenType>> tokens, ICycleDetector detector, IDictionaryEnumerator enumerator)
		{
			tokens.Enqueue(CommonGrammar.TokenObjectBegin);

			bool appendDelim = false;

			while (enumerator.MoveNext())
			{
				if (appendDelim)
				{
					tokens.Enqueue(CommonGrammar.TokenValueDelim);
				}
				else
				{
					appendDelim = true;
				}

				this.GetPropertyTokens(tokens, detector, enumerator.Key, enumerator.Value);
			}

			tokens.Enqueue(CommonGrammar.TokenObjectEnd);
		}

		private void GetObjectTokens(Queue<Token<CommonTokenType>> tokens, ICycleDetector detector, IEnumerator<KeyValuePair<string, object>> enumerator)
		{
			tokens.Enqueue(CommonGrammar.TokenObjectBegin);

			bool appendDelim = false;

			while (enumerator.MoveNext())
			{
				if (appendDelim)
				{
					tokens.Enqueue(CommonGrammar.TokenValueDelim);
				}
				else
				{
					appendDelim = true;
				}

				KeyValuePair<string, object> pair = enumerator.Current;
				this.GetPropertyTokens(tokens, detector, pair.Key, pair.Value);
			}

			tokens.Enqueue(CommonGrammar.TokenObjectEnd);
		}

		private void GetObjectTokens(Queue<Token<CommonTokenType>> tokens, ICycleDetector detector, object value, Type type)
		{
			tokens.Enqueue(CommonGrammar.TokenObjectBegin);

			IDictionary<string, MemberMap> maps = this.Settings.Resolver.LoadMaps(type);
			if (maps == null)
			{
				// TODO: verify no other valid situations here
				tokens.Enqueue(CommonGrammar.TokenObjectEnd);
				return;
			}

			bool appendDelim = false;

			foreach (var map in maps)
			{
				if (map.Value.Getter == null)
				{
					continue;
				}

				object propertyValue = map.Value.Getter(value);
				if (map.Value.IsIgnored != null &&
					map.Value.IsIgnored(value, propertyValue))
				{
					continue;
				}

				if (appendDelim)
				{
					tokens.Enqueue(CommonGrammar.TokenValueDelim);
				}
				else
				{
					appendDelim = true;
				}

				this.GetPropertyTokens(tokens, detector, map.Key, propertyValue);
			}

			tokens.Enqueue(CommonGrammar.TokenObjectEnd);
		}

		private void GetPropertyTokens(Queue<Token<CommonTokenType>> tokens, ICycleDetector detector, object key, object value)
		{
			tokens.Enqueue(CommonGrammar.TokenProperty(key));
			this.GetTokens(tokens, detector, value);
		}

		#endregion Walker Methods
	}
}