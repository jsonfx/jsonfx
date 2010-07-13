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

using JsonFx.Serialization.GraphCycles;

namespace JsonFx.Serialization
{
	public partial class DataWriter
	{
		/// <summary>
		/// Generates a SAX-like sequence of tokens from an object graph
		/// </summary>
		public class DataWalker : IDataWalker<DataTokenType>
		{
			#region Fields

			private readonly DataWriterSettings Settings;
			private readonly IEnumerable<IDataFilter<DataTokenType>> Filters;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			/// <param name="filters"></param>
			public DataWalker(DataWriterSettings settings, params IDataFilter<DataTokenType>[] filters)
				: this(settings, (IEnumerable<IDataFilter<DataTokenType>>)filters)
			{
			}

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			/// <param name="filters"></param>
			public DataWalker(DataWriterSettings settings, IEnumerable<IDataFilter<DataTokenType>> filters)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}
				this.Settings = settings;

				if (filters == null)
				{
					filters = new IDataFilter<DataTokenType>[0];
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

			#region Walker Methods

			/// <summary>
			/// Generates a sequence of tokens representing the value
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public IEnumerable<Token<DataTokenType>> GetTokens(object value)
			{
				Queue<Token<DataTokenType>> tokens = new Queue<Token<DataTokenType>>();

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

			private void GetTokens(Queue<Token<DataTokenType>> tokens, ICycleDetector detector, object value)
			{
				if (value == null)
				{
					tokens.Enqueue(DataGrammar.TokenNull);
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
							tokens.Enqueue(DataGrammar.TokenNull);
							return;
						}
					}
				}

				try
				{
					foreach (var filter in this.Filters)
					{
						IEnumerable<Token<DataTokenType>> filterTokens;
						if (filter.TryWrite(this.Settings, value, out filterTokens))
						{
							// found a successful match
							foreach (Token<DataTokenType> token in filterTokens)
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
						tokens.Enqueue(DataGrammar.TokenValue((Enum)value));
						return;
					}

					// Type.GetTypeCode() allows us to more efficiently switch type
					switch (Type.GetTypeCode(type))
					{
						case TypeCode.Boolean:
						{
							tokens.Enqueue(true.Equals(value) ? DataGrammar.TokenTrue : DataGrammar.TokenFalse);
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
							tokens.Enqueue(DataGrammar.TokenValue((ValueType)value));
							return;
						}
						case TypeCode.Double:
						{
							double doubleVal = (double)value;

							if (Double.IsNaN(doubleVal))
							{
								tokens.Enqueue(DataGrammar.TokenNaN);
							}
							else if (Double.IsPositiveInfinity(doubleVal))
							{
								tokens.Enqueue(DataGrammar.TokenPositiveInfinity);
							}
							else if (Double.IsNegativeInfinity(doubleVal))
							{
								tokens.Enqueue(DataGrammar.TokenNegativeInfinity);
							}
							else
							{
								tokens.Enqueue(DataGrammar.TokenValue(doubleVal));
							}
							return;
						}
						case TypeCode.Single:
						{
							float floatVal = (float)value;

							if (Single.IsNaN(floatVal))
							{
								tokens.Enqueue(DataGrammar.TokenNaN);
							}
							else if (Single.IsPositiveInfinity(floatVal))
							{
								tokens.Enqueue(DataGrammar.TokenPositiveInfinity);
							}
							else if (Single.IsNegativeInfinity(floatVal))
							{
								tokens.Enqueue(DataGrammar.TokenNegativeInfinity);
							}
							else
							{
								tokens.Enqueue(DataGrammar.TokenValue(floatVal));
							}
							return;
						}
						case TypeCode.Char:
						case TypeCode.DateTime:
						case TypeCode.String:
						{
							tokens.Enqueue(DataGrammar.TokenValue(value));
							return;
						}
						case TypeCode.DBNull:
						case TypeCode.Empty:
						{
							tokens.Enqueue(DataGrammar.TokenNull);
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
						tokens.Enqueue(DataGrammar.TokenValue(value));
						return;
					}

					if (value is TimeSpan)
					{
						tokens.Enqueue(DataGrammar.TokenValue((TimeSpan)value));
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

			private void GetArrayTokens(Queue<Token<DataTokenType>> tokens, ICycleDetector detector, IEnumerable value)
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

				tokens.Enqueue(DataGrammar.TokenArrayBegin);

				while (enumerator.MoveNext())
				{
					this.GetTokens(tokens, detector, enumerator.Current);
				}

				tokens.Enqueue(DataGrammar.TokenArrayEnd);
			}

			private void GetObjectTokens(Queue<Token<DataTokenType>> tokens, ICycleDetector detector, IDictionaryEnumerator enumerator)
			{
				tokens.Enqueue(DataGrammar.TokenObjectBegin);

				while (enumerator.MoveNext())
				{
					this.GetPropertyTokens(tokens, detector, enumerator.Key, enumerator.Value);
				}

				tokens.Enqueue(DataGrammar.TokenObjectEnd);
			}

			private void GetObjectTokens(Queue<Token<DataTokenType>> tokens, ICycleDetector detector, IEnumerator<KeyValuePair<string, object>> enumerator)
			{
				tokens.Enqueue(DataGrammar.TokenObjectBegin);

				while (enumerator.MoveNext())
				{
					KeyValuePair<string, object> pair = enumerator.Current;
					this.GetPropertyTokens(tokens, detector, pair.Key, pair.Value);
				}

				tokens.Enqueue(DataGrammar.TokenObjectEnd);
			}

			private void GetObjectTokens(Queue<Token<DataTokenType>> tokens, ICycleDetector detector, object value, Type type)
			{
				tokens.Enqueue(DataGrammar.TokenObjectBegin);

				IDictionary<string, MemberMap> maps = this.Settings.Resolver.LoadMaps(type);
				if (maps == null)
				{
					// TODO: verify no other valid situations here
					tokens.Enqueue(DataGrammar.TokenObjectEnd);
					return;
				}

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

					this.GetPropertyTokens(tokens, detector, map.Key, propertyValue);
				}

				tokens.Enqueue(DataGrammar.TokenObjectEnd);
			}

			private void GetPropertyTokens(Queue<Token<DataTokenType>> tokens, ICycleDetector detector, object key, object value)
			{
				tokens.Enqueue(DataGrammar.TokenProperty(key));
				this.GetTokens(tokens, detector, value);
			}

			#endregion Walker Methods
		}
	}
}