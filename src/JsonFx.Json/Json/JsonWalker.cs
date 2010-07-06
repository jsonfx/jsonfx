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

namespace JsonFx.Json
{
	public partial class JsonWriter
	{
		/// <summary>
		/// Generates a SAX-like sequence of JSON tokens from an object graph
		/// </summary>
		public class JsonWalker : IDataWalker<JsonTokenType>
		{
			#region Fields

			private readonly DataWriterSettings Settings;
			private readonly IEnumerable<IDataFilter<JsonTokenType>> Filters;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			/// <param name="filters"></param>
			public JsonWalker(DataWriterSettings settings, params IDataFilter<JsonTokenType>[] filters)
				: this(settings, (IEnumerable<IDataFilter<JsonTokenType>>)filters)
			{
			}

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			/// <param name="filters"></param>
			public JsonWalker(DataWriterSettings settings, IEnumerable<IDataFilter<JsonTokenType>> filters)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}
				this.Settings = settings;

				if (filters == null)
				{
					filters = new IDataFilter<JsonTokenType>[0];
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
			public IEnumerable<Token<JsonTokenType>> GetTokens(object value)
			{
				Queue<Token<JsonTokenType>> tokens = new Queue<Token<JsonTokenType>>();

				this.GetTokens(tokens, value);

				return tokens;
			}

			private void GetTokens(Queue<Token<JsonTokenType>> tokens, object value)
			{
				foreach (var filter in this.Filters)
				{
					IEnumerable<Token<JsonTokenType>> filterTokens;
					if (filter.TryWrite(this.Settings, value, out filterTokens))
					{
						// found a successful match
						foreach (Token<JsonTokenType> token in filterTokens)
						{
							tokens.Enqueue(token);
						}
						return;
					}
				}

				if (value == null)
				{
					tokens.Enqueue(JsonGrammar.TokenNull);
					return;
				}

				Type type = value.GetType();

				// must test enumerations before other value types
				if (type.IsEnum)
				{
					tokens.Enqueue(JsonGrammar.TokenString((Enum)value));
					return;
				}

				// Type.GetTypeCode() allows us to more efficiently switch type
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Boolean:
					{
						tokens.Enqueue(true.Equals(value) ? JsonGrammar.TokenTrue : JsonGrammar.TokenFalse);
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
						tokens.Enqueue(JsonGrammar.TokenNumber((ValueType)value));
						return;
					}
					case TypeCode.Double:
					{
						double doubleVal = (double)value;

						if (Double.IsNaN(doubleVal))
						{
							tokens.Enqueue(JsonGrammar.TokenNaN);
						}
						else if (Double.IsPositiveInfinity(doubleVal))
						{
							tokens.Enqueue(JsonGrammar.TokenPositiveInfinity);
						}
						else if (Double.IsNegativeInfinity(doubleVal))
						{
							tokens.Enqueue(JsonGrammar.TokenNegativeInfinity);
						}
						else
						{
							tokens.Enqueue(JsonGrammar.TokenNumber(doubleVal));
						}
						return;
					}
					case TypeCode.Single:
					{
						float floatVal = (float)value;

						if (Single.IsNaN(floatVal))
						{
							tokens.Enqueue(JsonGrammar.TokenNaN);
						}
						else if (Single.IsPositiveInfinity(floatVal))
						{
							tokens.Enqueue(JsonGrammar.TokenPositiveInfinity);
						}
						else if (Single.IsNegativeInfinity(floatVal))
						{
							tokens.Enqueue(JsonGrammar.TokenNegativeInfinity);
						}
						else
						{
							tokens.Enqueue(JsonGrammar.TokenNumber(floatVal));
						}
						return;
					}
					case TypeCode.Char:
					case TypeCode.DateTime:
					case TypeCode.String:
					{
						tokens.Enqueue(JsonGrammar.TokenString(value));
						return;
					}
					case TypeCode.DBNull:
					case TypeCode.Empty:
					{
						tokens.Enqueue(JsonGrammar.TokenNull);
						return;
					}
				}

				if (value is IEnumerable)
				{
					this.GetArrayTokens(tokens, (IEnumerable)value);
					return;
				}

				if (value is Guid || value is Uri || value is Version)
				{
					tokens.Enqueue(JsonGrammar.TokenString(value));
					return;
				}

				if (value is TimeSpan)
				{
					tokens.Enqueue(JsonGrammar.TokenNumber((TimeSpan)value));
					return;
				}

				// all other structs and classes
				this.GetObjectTokens(tokens, value, type);
			}

			private void GetArrayTokens(Queue<Token<JsonTokenType>> tokens, IEnumerable value)
			{
				IEnumerator enumerator = value.GetEnumerator();

				if (enumerator is IEnumerator<KeyValuePair<string, object>> ||
					enumerator is IDictionaryEnumerator)
				{
					this.GetObjectTokens(tokens, enumerator);
					return;
				}

				tokens.Enqueue(JsonGrammar.TokenArrayBegin);

				bool appendDelim = false;
				while (enumerator.MoveNext())
				{
					if (appendDelim)
					{
						tokens.Enqueue(JsonGrammar.TokenValueDelim);
					}
					else
					{
						appendDelim = true;
					}

					this.GetTokens(tokens, enumerator.Current);
				}

				tokens.Enqueue(JsonGrammar.TokenArrayEnd);
			}

			private void GetObjectTokens(Queue<Token<JsonTokenType>> tokens, IEnumerator enumerator)
			{
				tokens.Enqueue(JsonGrammar.TokenObjectBegin);

				IEnumerator<KeyValuePair<string, object>> keyValueEnumerator = enumerator as IEnumerator<KeyValuePair<string, object>>;
				IDictionaryEnumerator dictionaryEnumerator = enumerator as IDictionaryEnumerator;

				bool appendDelim = false;

				if (keyValueEnumerator != null)
				{
					while (enumerator.MoveNext())
					{
						if (appendDelim)
						{
							tokens.Enqueue(JsonGrammar.TokenValueDelim);
						}
						else
						{
							appendDelim = true;
						}

						KeyValuePair<string, object> pair = keyValueEnumerator.Current;
						this.GetPropertyTokens(tokens, pair.Key, pair.Value);
					}
				}
				else if (dictionaryEnumerator != null)
				{
					while (enumerator.MoveNext())
					{
						if (appendDelim)
						{
							tokens.Enqueue(JsonGrammar.TokenValueDelim);
						}
						else
						{
							appendDelim = true;
						}

						this.GetPropertyTokens(tokens, dictionaryEnumerator.Key, dictionaryEnumerator.Value);
					}
				}
				else
				{
					throw new ArgumentException("enumerator", "Expected IDictionaryEnumerator or IEnumerator<KeyValuePair<string, object>>");
				}

				tokens.Enqueue(JsonGrammar.TokenObjectEnd);
			}

			private void GetObjectTokens(Queue<Token<JsonTokenType>> tokens, object value, Type type)
			{
				tokens.Enqueue(JsonGrammar.TokenObjectBegin);

				IDictionary<string, MemberMap> maps = this.Settings.Resolver.LoadMaps(type);
				if (maps == null)
				{
					// TODO: verify no other valid situations here
					tokens.Enqueue(JsonGrammar.TokenObjectEnd);
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
						tokens.Enqueue(JsonGrammar.TokenValueDelim);
					}
					else
					{
						appendDelim = true;
					}

					this.GetPropertyTokens(tokens, map.Key, propertyValue);
				}

				tokens.Enqueue(JsonGrammar.TokenObjectEnd);
			}

			private void GetPropertyTokens(Queue<Token<JsonTokenType>> tokens, object key, object value)
			{
				tokens.Enqueue(JsonGrammar.TokenString(key));
				tokens.Enqueue(JsonGrammar.TokenPairDelim);

				this.GetTokens(tokens, value);
			}

			#endregion Walker Methods
		}
	}
}