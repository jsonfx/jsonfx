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

			#region Parsing Methods

			/// <summary>
			/// Parses the token stream coercing the result targetType
			/// </summary>
			/// <param name="tokens"></param>
			/// <returns></returns>
			public IEnumerable<object> Analyze(IEnumerable<Token<JsonTokenType>> tokens)
			{
				return this.Analyze(tokens, null);
			}

			/// <summary>
			/// Parses the token stream coercing the result to TResult
			/// </summary>
			/// <typeparam name="TResult">the result target type</typeparam>
			/// <param name="tokens"></param>
			/// <returns></returns>
			public IEnumerable<TResult> Analyze<TResult>(IEnumerable<Token<JsonTokenType>> tokens)
			{
				// cast each of the values accordingly
				foreach (object value in this.Analyze(tokens, typeof(TResult))) { yield return (TResult)value; }
			}

			/// <summary>
			/// Parses the token stream coercing the result targetType
			/// </summary>
			/// <param name="tokens"></param>
			/// <param name="targetType"></param>
			/// <returns></returns>
			public IEnumerable<object> Analyze(IEnumerable<Token<JsonTokenType>> tokens, Type targetType)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				IEnumerator<Token<JsonTokenType>> tokenizer = tokens.GetEnumerator();
				while (tokenizer.MoveNext())
				{
					yield return this.ConsumeValue(tokenizer, targetType);
				}
			}

			private object ConsumeValue(IEnumerator<Token<JsonTokenType>> tokens, Type targetType)
			{
				Token<JsonTokenType> token = tokens.Current;
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
						throw new ParseException<JsonTokenType>(
							token,
							String.Format(JsonAnalyzer.ErrorUnexpectedToken, token.TokenType));
					}
				}
			}

			private object ConsumeObject(IEnumerator<Token<JsonTokenType>> tokens, Type targetType)
			{
				Token<JsonTokenType> token = tokens.Current;

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
				while (tokens.MoveNext())
				{
					token = tokens.Current;
					if (hasProperties)
					{
						// consume value delimiter
						switch (token.TokenType)
						{
							case JsonTokenType.ValueDelim:
							{
								break;
							}
							case JsonTokenType.ObjectEnd:
							{
								// end of the object loop
								return this.Coercion.CoerceType(targetType, objectValue);
							}

							default:
							{
								// these are invalid here
								throw new ParseException<JsonTokenType>(
									token,
									String.Format(JsonAnalyzer.ErrorExpectedObjectValueDelim, token.TokenType));
							}
						}

						// move past delim
						if (!tokens.MoveNext())
						{
							// end of input
							break;
						}
						token = tokens.Current;
					}

					// consume the property key
					string propertyName;
					switch (token.TokenType)
					{
						case JsonTokenType.Literal:
						case JsonTokenType.String:
						case JsonTokenType.Number:
						{
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
							return this.Coercion.CoerceType(targetType, objectValue);
						}
						case JsonTokenType.ValueDelim:
						{
							// extraneous item delimiter
							throw new ParseException<JsonTokenType>(
								token,
								JsonAnalyzer.ErrorMissingObjectProperty);
						}
						default:
						{
							// these are invalid here
							throw new ParseException<JsonTokenType>(
								token,
								String.Format(JsonAnalyzer.ErrorExpectedPropertyName, token.TokenType));
						}
					}

					// move past delim
					if (!tokens.MoveNext())
					{
						// end of input
						break;
					}
					token = tokens.Current;

					// consume pair delimiter
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

					// move past delim
					if (!tokens.MoveNext())
					{
						// end of input
						break;
					}
					token = tokens.Current;

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

			private object ConsumeArray(IEnumerator<Token<JsonTokenType>> tokens, Type arrayType)
			{
				Token<JsonTokenType> token = tokens.Current;

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
				while (tokens.MoveNext())
				{
					token = tokens.Current;
					if (array.Count > 0)
					{
						// consume item delimiter
						switch (token.TokenType)
						{
							case JsonTokenType.ValueDelim:
							{
								break;
							}
							case JsonTokenType.ArrayEnd:
							{
								// end of array loop
								return this.Coercion.CoerceArrayList(arrayType, itemType, array);
							}
							default:
							{
								// these are invalid here
								throw new ParseException<JsonTokenType>(
									token,
									String.Format(JsonAnalyzer.ErrorExpectedArrayItemDelim, token.TokenType));
							}
						}

						if (!tokens.MoveNext())
						{
							// end of input
							break;
						}
						token = tokens.Current;
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
							if (isItemTypeHint)
							{
								item = token.Value;
							}
							else
							{
								item = this.Coercion.CoerceType(itemType, token.Value);
							}
							break;
						}
						case JsonTokenType.ValueDelim:
						{
							// extraneous item delimiter
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

			#endregion Parsing Methods

			#region Utility Methods

			/// <summary>
			/// Allows an IEnumerator&lt;T&gt; to be continue to be used as an IEnumerable&lt;T&gt;.
			/// </summary>
			/// <param name="enumerator"></param>
			/// <returns></returns>
			/// <remarks>
			/// Assumes that the Current value still needs to be consumed.
			/// </remarks>
			private IEnumerable<T> Enumerate<T>(IEnumerator<T> enumerator)
			{
				do
				{
					yield return enumerator.Current;

				} while (enumerator.MoveNext());
			}

			#endregion Utility Methods
		}
	}
}