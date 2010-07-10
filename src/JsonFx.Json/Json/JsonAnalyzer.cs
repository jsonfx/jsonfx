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

using JsonFx.IO;
using JsonFx.Serialization;

#if NET40
using JsonObject=System.Dynamic.ExpandoObject;
#else
using JsonObject=System.Collections.Generic.Dictionary<string, object>;
#endif

namespace JsonFx.Json
{
	public partial class JsonReader
	{
		/// <summary>
		/// Consumes a SAX-like sequence of JSON tokens to produce an object graph optionally coerced to a given type
		/// </summary>
		public class JsonAnalyzer : IDataAnalyzer<JsonTokenType>
		{
			#region Constants

			// errors
			private const string ErrorUnexpectedToken = "Unexpected JSON token ({0})";

			private const string ErrorExpectedArray = "Expected JSON array start ({0})";
			private const string ErrorExpectedArrayItem = "Expected JSON array item or end of JSON array ({0})";
			private const string ErrorExpectedArrayItemDelim = "Expected JSON array item delimiter or end of JSON array ({0})";
			private const string ErrorMissingArrayItem = "Missing JSON array item";
			private const string ErrorUnterminatedArray = "Unterminated JSON array";

			private const string ErrorExpectedObject = "Expected JSON object start ({0})";
			private const string ErrorExpectedPropertyName = "Expected JSON object property name or end of JSON object ({0})";
			private const string ErrorExpectedPropertyPairDelim = "Expected JSON object property name/value delimiter ({0})";
			private const string ErrorExpectedObjectValueDelim = "Expected value delimiter or end of JSON object ({0})";
			private const string ErrorMissingObjectProperty = "Missing JSON object property";
			private const string ErrorUnterminatedObject = "Unterminated JSON object";

			#endregion Constants

			#region Fields

			private readonly DataReaderSettings Settings;
			private readonly TypeCoercionUtility Coercion;
			private readonly IEnumerable<IDataFilter<JsonTokenType>> Filters;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			/// <param name="filters"></param>
			public JsonAnalyzer(DataReaderSettings settings, params IDataFilter<JsonTokenType>[] filters)
				: this(settings, (IEnumerable<IDataFilter<JsonTokenType>>)filters)
			{
			}

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public JsonAnalyzer(DataReaderSettings settings, IEnumerable<IDataFilter<JsonTokenType>> filters)
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

				this.Coercion = new TypeCoercionUtility(settings, settings.AllowNullValueTypes);
			}

			#endregion Init

			#region Analyze Methods

			/// <summary>
			/// Parses the token stream coercing the result targetType
			/// </summary>
			/// <param name="tokens"></param>
			/// <returns></returns>
			public IEnumerable Analyze(IEnumerable<Token<JsonTokenType>> tokens)
			{
				return this.Analyze(tokens, null);
			}

			/// <summary>
			/// Parses the token stream coercing the result targetType
			/// </summary>
			/// <param name="tokens"></param>
			/// <param name="targetType"></param>
			/// <returns></returns>
			public IEnumerable Analyze(IEnumerable<Token<JsonTokenType>> tokens, Type targetType)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				IStream<Token<JsonTokenType>> stream = new Stream<Token<JsonTokenType>>(tokens);
				while (!stream.IsCompleted)
				{
					yield return this.ConsumeValue(stream, targetType);
				}
			}

			/// <summary>
			/// Parses the token stream coercing the result to TResult
			/// </summary>
			/// <typeparam name="TResult">the result target type</typeparam>
			/// <param name="tokens"></param>
			/// <returns></returns>
			public IEnumerable<TResult> Analyze<TResult>(IEnumerable<Token<JsonTokenType>> tokens)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				Type resultType = typeof(TResult);

				IStream<Token<JsonTokenType>> stream = new Stream<Token<JsonTokenType>>(tokens);
				while (!stream.IsCompleted)
				{
					// cast each of the values accordingly
					yield return (TResult)this.ConsumeValue(stream, resultType);
				}
			}

			#endregion Analyze Methods

			#region Consume Methods

			private object ConsumeValue(IStream<Token<JsonTokenType>> tokens, Type targetType)
			{
				object result;
				if (this.TryReadFilters(tokens, out result))
				{
					return result;
				}

				Token<JsonTokenType> token = tokens.Peek();
				switch (token.TokenType)
				{
					case JsonTokenType.ArrayBegin:
					{
						// found array
						return this.ConsumeArray(tokens, targetType);
					}
					case JsonTokenType.ObjectBegin:
					{
						// found object
						return this.ConsumeObject(tokens, targetType);
					}
					case JsonTokenType.Boolean:
					case JsonTokenType.Number:
					case JsonTokenType.Null:
					case JsonTokenType.String:
					case JsonTokenType.Undefined:
					{
						// found primitive
						tokens.Pop();
						return this.Coercion.CoerceType(targetType, token.Value);
					}
					case JsonTokenType.ArrayEnd:
					case JsonTokenType.Literal:
					case JsonTokenType.None:
					case JsonTokenType.ObjectEnd:
					case JsonTokenType.PairDelim:
					case JsonTokenType.ValueDelim:
					default:
					{
						// these are invalid here
						tokens.Pop();
						throw new ParseException<JsonTokenType>(
							token,
							String.Format(JsonAnalyzer.ErrorUnexpectedToken, token.TokenType));
					}
				}
			}

			private object ConsumeObject(IStream<Token<JsonTokenType>> tokens, Type targetType)
			{
				Token<JsonTokenType> token = tokens.Pop();

				// verify correct starting state
				if (token.TokenType != JsonTokenType.ObjectBegin)
				{
					throw new ParseException<JsonTokenType>(
						token,
						String.Format(JsonAnalyzer.ErrorExpectedObject, token.TokenType));
				}

				IDictionary<string, MemberMap> memberMap = this.Coercion.LoadMaps(targetType);

				Type itemType = TypeCoercionUtility.GetDictionaryItemType(targetType);
				object objectValue = (itemType != null) ?
					this.Coercion.InstantiateObject(targetType) :
					new JsonObject();

				bool hasProperties = false;
				while (!tokens.IsCompleted)
				{
					token = tokens.Peek();
					if (hasProperties)
					{
						// consume value delimiter
						switch (token.TokenType)
						{
							case JsonTokenType.ValueDelim:
							{
								// move past delim
								tokens.Pop();
								break;
							}
							case JsonTokenType.ObjectEnd:
							{
								// end of the object loop
								tokens.Pop();
								return this.Coercion.CoerceType(targetType, objectValue);
							}

							default:
							{
								// these are invalid here
								tokens.Pop();
								throw new ParseException<JsonTokenType>(
									token,
									String.Format(JsonAnalyzer.ErrorExpectedObjectValueDelim, token.TokenType));
							}
						}

						if (tokens.IsCompleted)
						{
							// end of input
							break;
						}
						token = tokens.Peek();
					}

					// consume the property key
					string propertyName;
					switch (token.TokenType)
					{
						case JsonTokenType.Literal:
						case JsonTokenType.String:
						case JsonTokenType.Number:
						{
							tokens.Pop();
							propertyName = Convert.ToString(token.Value, CultureInfo.InvariantCulture);
							break;
						}
						case JsonTokenType.ObjectEnd:
						{
							if (hasProperties)
							{
								// not allowed after value delim
								goto case JsonTokenType.ValueDelim;
							}

							// end of the object loop
							tokens.Pop();
							return this.Coercion.CoerceType(targetType, objectValue);
						}
						case JsonTokenType.ValueDelim:
						{
							// extraneous item delimiter
							tokens.Pop();
							throw new ParseException<JsonTokenType>(
								token,
								JsonAnalyzer.ErrorMissingObjectProperty);
						}
						default:
						{
							// these are invalid here
							tokens.Pop();
							throw new ParseException<JsonTokenType>(
								token,
								String.Format(JsonAnalyzer.ErrorExpectedPropertyName, token.TokenType));
						}
					}

					// move past delim
					if (tokens.IsCompleted)
					{
						// end of input
						break;
					}

					// consume pair delimiter
					token = tokens.Pop();
					switch (token.TokenType)
					{
						case JsonTokenType.PairDelim:
						{
							break;
						}
						default:
						{
							// these are invalid here
							throw new ParseException<JsonTokenType>(
								token,
								String.Format(JsonAnalyzer.ErrorExpectedPropertyPairDelim, token.TokenType));
						}
					}

					if (tokens.IsCompleted)
					{
						// end of input
						break;
					}
					token = tokens.Peek();

					MemberMap propertyMap;
					Type propertyType;
					if (itemType != null)
					{
						// properties all of the same type
						propertyMap = null;
						propertyType = itemType;
					}
					else if ((memberMap != null) &&
						memberMap.TryGetValue(propertyName, out propertyMap))
					{
						propertyType = (propertyMap != null) ? propertyMap.Type : null;
					}
					else
					{
						propertyMap = null;
						propertyType = null;
					}

					// consume the property value
					object propertyValue = this.ConsumeValue(tokens, propertyType);

					// set member to the result
					this.Coercion.SetMemberValue(objectValue, targetType, propertyMap, propertyName, propertyValue);

					// flag to ensure delimiter
					hasProperties = true;
				}

				// end of input
				throw new ParseException<JsonTokenType>(
					JsonGrammar.TokenNone,
					JsonAnalyzer.ErrorUnterminatedObject);
			}

			private object ConsumeArray(IStream<Token<JsonTokenType>> tokens, Type arrayType)
			{
				Token<JsonTokenType> token = tokens.Pop();

				// verify correct starting state
				if (token.TokenType != JsonTokenType.ArrayBegin)
				{
					throw new ParseException<JsonTokenType>(
						token,
						String.Format(JsonAnalyzer.ErrorExpectedArray, token.TokenType));
				}

				Type itemType = TypeCoercionUtility.GetArrayItemType(arrayType);

				// if itemType was specified by caller, then isn't just a hint
				bool isItemTypeHint = (itemType == null);

				// using ArrayList since has .ToArray(Type) method
				// cannot create List<T> at runtime
				ArrayList array = new ArrayList();
				while (!tokens.IsCompleted)
				{
					token = tokens.Peek();
					if (array.Count > 0)
					{
						// consume item delimiter
						switch (token.TokenType)
						{
							case JsonTokenType.ValueDelim:
							{
								tokens.Pop();
								break;
							}
							case JsonTokenType.ArrayEnd:
							{
								// end of array loop
								tokens.Pop();
								return this.Coercion.CoerceArrayList(arrayType, itemType, array);
							}
							default:
							{
								// these are invalid here
								tokens.Pop();
								throw new ParseException<JsonTokenType>(
									token,
									String.Format(JsonAnalyzer.ErrorExpectedArrayItemDelim, token.TokenType));
							}
						}

						if (tokens.IsCompleted)
						{
							// end of input
							break;
						}
						token = tokens.Peek();
					}

					// consume the next item
					object item;
					switch (token.TokenType)
					{
						case JsonTokenType.ArrayEnd:
						{
							if (array.Count > 0)
							{
								// not allowed after value delim
								goto case JsonTokenType.ValueDelim;
							}

							// end of array loop
							tokens.Pop();
							return this.Coercion.CoerceArrayList(arrayType, itemType, array);
						}
						case JsonTokenType.ArrayBegin:
						{
							// array item
							item = this.ConsumeArray(tokens, isItemTypeHint ? null : itemType);
							break;
						}
						case JsonTokenType.ObjectBegin:
						{
							// object item
							item = this.ConsumeObject(tokens, isItemTypeHint ? null : itemType);
							break;
						}
						case JsonTokenType.Boolean:
						case JsonTokenType.Null:
						case JsonTokenType.Number:
						case JsonTokenType.String:
						case JsonTokenType.Undefined:
						{
							// primitive item
							if (!this.TryReadFilters(tokens, out item))
							{
								// TODO: evaluate how to ensure that filters didn't take anything
								token = tokens.Pop();
								item = (token != null) ? token.Value : null;
							}

							if (!isItemTypeHint)
							{
								item = this.Coercion.CoerceType(itemType, item);
							}
							break;
						}
						case JsonTokenType.ValueDelim:
						{
							// extraneous item delimiter
							tokens.Pop();
							throw new ParseException<JsonTokenType>(
								token,
								JsonAnalyzer.ErrorMissingArrayItem);
						}
						case JsonTokenType.Literal:
						case JsonTokenType.None:
						case JsonTokenType.ObjectEnd:
						case JsonTokenType.PairDelim:
						default:
						{
							// these are invalid here
							tokens.Pop();
							throw new ParseException<JsonTokenType>(
								token,
								String.Format(JsonAnalyzer.ErrorExpectedArrayItem, token.TokenType));
						}
					}

					// establish common type
					itemType = TypeCoercionUtility.FindCommonType(itemType, item);

					// add item to the array
					array.Add(item);
				}

				// end of input
				throw new ParseException<JsonTokenType>(
					JsonGrammar.TokenNone,
					JsonAnalyzer.ErrorUnterminatedArray);
			}

			private bool TryReadFilters(IStream<Token<JsonTokenType>> tokens, out object result)
			{
				if (!tokens.IsCompleted)
				{
					foreach (var filter in this.Filters)
					{
						if (filter.TryRead(this.Settings, tokens, out result))
						{
							// found a successful match
							return true;
						}
					}
				}

				result = null;
				return false;
			}

			#endregion Consume Methods
		}
	}
}