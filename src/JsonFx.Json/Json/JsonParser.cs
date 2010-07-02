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

namespace JsonFx.Json
{
	public partial class JsonReader
	{
		/// <summary>
		/// Consumes a SAX-like sequence of JSON tokens to produce an object graph optionally coerced to a given type
		/// </summary>
		public class JsonParser : IDataParser<JsonTokenType>
		{
			#region Constants

			// parse errors
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

			private static readonly Type SerializableType = typeof(ISerializable<JsonTokenType>);

			#endregion Constants

			#region Fields

			private readonly TypeCoercionUtility Coercion;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public JsonParser(DataReaderSettings settings)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
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
			public object Parse(IEnumerable<Token<JsonTokenType>> tokens)
			{
				return this.Parse(tokens, null);
			}

			/// <summary>
			/// Parses the token stream coercing the result to TResult
			/// </summary>
			/// <typeparam name="TResult">the result target type</typeparam>
			/// <param name="tokens"></param>
			/// <returns></returns>
			public TResult Parse<TResult>(IEnumerable<Token<JsonTokenType>> tokens)
			{
				return (TResult)this.Parse(tokens, typeof(TResult));
			}

			/// <summary>
			/// Parses the token stream coercing the result targetType
			/// </summary>
			/// <param name="tokens"></param>
			/// <param name="targetType"></param>
			/// <returns></returns>
			public object Parse(IEnumerable<Token<JsonTokenType>> tokens, Type targetType)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				IEnumerator<Token<JsonTokenType>> tokenizer = tokens.GetEnumerator();
				if (!tokenizer.MoveNext())
				{
					// end of input
					return this.Coercion.CoerceType(targetType, null);
				}

				object value = this.ParseValue(tokenizer, targetType);

				// not checking for trailing tokens allows this
				// JSON stream to be parsed inside of another structure
				// for example inside <[CDATA[ ... ]]>

				return value;
			}

			private object ParseValue(IEnumerator<Token<JsonTokenType>> tokens, Type targetType)
			{
				if (targetType != null && JsonParser.SerializableType.IsAssignableFrom(targetType))
				{
					ISerializable<JsonTokenType> serializable = TypeCoercionUtility.InstantiateObject<ISerializable<JsonTokenType>>(targetType);
					return serializable.Read(this.Enumerate(tokens));
				}

				Token<JsonTokenType> token = tokens.Current;
				switch (token.TokenType)
				{
					case JsonTokenType.ArrayBegin:
					{
						// found array
						return this.ParseArray(tokens, targetType);
					}
					case JsonTokenType.ObjectBegin:
					{
						// found object
						return this.ParseObject(tokens, targetType);
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
							String.Format(JsonParser.ErrorUnexpectedToken, token.TokenType));
					}
				}
			}

			private object ParseObject(IEnumerator<Token<JsonTokenType>> tokens, Type targetType)
			{
				if (targetType != null && JsonParser.SerializableType.IsAssignableFrom(targetType))
				{
					ISerializable<JsonTokenType> serializable = TypeCoercionUtility.InstantiateObject<ISerializable<JsonTokenType>>(targetType);
					return serializable.Read(this.Enumerate(tokens));
				}

				Token<JsonTokenType> token = tokens.Current;

				// verify correct starting state
				if (token.TokenType != JsonTokenType.ObjectBegin)
				{
					throw new ParseException<JsonTokenType>(
						token,
						String.Format(JsonParser.ErrorExpectedObject, token.TokenType));
				}

				IDictionary<string, MemberMap> memberMap = this.Coercion.LoadMaps(targetType);

				Type itemType = TypeCoercionUtility.GetDictionaryItemType(targetType);
				object objectValue = (itemType != null) ?
					TypeCoercionUtility.InstantiateObject(targetType) :
					new Dictionary<string, object>();

				bool hasProperties = false;
				while (tokens.MoveNext())
				{
					token = tokens.Current;
					if (hasProperties)
					{
						// parse value delimiter
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
									String.Format(JsonParser.ErrorExpectedObjectValueDelim, token.TokenType));
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

					// parse the property key
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
								JsonParser.ErrorMissingObjectProperty);
						}
						default:
						{
							// these are invalid here
							throw new ParseException<JsonTokenType>(
								token,
								String.Format(JsonParser.ErrorExpectedPropertyName, token.TokenType));
						}
					}

					// move past delim
					if (!tokens.MoveNext())
					{
						// end of input
						break;
					}
					token = tokens.Current;

					// parse pair delimiter
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
								String.Format(JsonParser.ErrorExpectedPropertyPairDelim, token.TokenType));
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
					else if ((memberMap != null) && memberMap.ContainsKey(propertyName))
					{
						propertyMap = memberMap[propertyName];
						propertyType = (propertyMap != null) ? propertyMap.Type : null;
					}
					else
					{
						propertyMap = null;
						propertyType = null;
					}

					// parse the property value
					object propertyValue = this.ParseValue(tokens, propertyType);

					// set member to the result
					this.Coercion.SetMemberValue(objectValue, targetType, propertyMap, propertyName, propertyValue);

					// flag to ensure delimiter
					hasProperties = true;
				}

				// end of input
				throw new ParseException<JsonTokenType>(
					JsonGrammar.TokenNone,
					JsonParser.ErrorUnterminatedObject);
			}

			private object ParseArray(IEnumerator<Token<JsonTokenType>> tokens, Type arrayType)
			{
				if (arrayType != null && JsonParser.SerializableType.IsAssignableFrom(arrayType))
				{
					ISerializable<JsonTokenType> serializable = TypeCoercionUtility.InstantiateObject<ISerializable<JsonTokenType>>(arrayType);
					return serializable.Read(this.Enumerate(tokens));
				}

				Token<JsonTokenType> token = tokens.Current;

				// verify correct starting state
				if (token.TokenType != JsonTokenType.ArrayBegin)
				{
					throw new ParseException<JsonTokenType>(
						token,
						String.Format(JsonParser.ErrorExpectedArray, token.TokenType));
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
						// parse item delimiter
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
									String.Format(JsonParser.ErrorExpectedArrayItemDelim, token.TokenType));
							}
						}

						if (!tokens.MoveNext())
						{
							// end of input
							break;
						}
						token = tokens.Current;
					}

					// parse the next item
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
							item = this.ParseArray(tokens, isItemTypeHint ? null : itemType);
							break;
						}
						case JsonTokenType.ObjectBegin:
						{
							// object item
							item = this.ParseObject(tokens, isItemTypeHint ? null : itemType);
							break;
						}
						case JsonTokenType.Boolean:
						case JsonTokenType.Null:
						case JsonTokenType.Number:
						case JsonTokenType.String:
						case JsonTokenType.Undefined:
						{
							if (itemType != null && JsonParser.SerializableType.IsAssignableFrom(itemType))
							{
								ISerializable<JsonTokenType> serializable = TypeCoercionUtility.InstantiateObject<ISerializable<JsonTokenType>>(itemType);
								item = serializable.Read(this.Enumerate(tokens));
							}
							else
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
							}
							break;
						}
						case JsonTokenType.ValueDelim:
						{
							// extraneous item delimiter
							throw new ParseException<JsonTokenType>(
								token,
								JsonParser.ErrorMissingArrayItem);
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
								String.Format(JsonParser.ErrorExpectedArrayItem, token.TokenType));
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
					JsonParser.ErrorUnterminatedArray);
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