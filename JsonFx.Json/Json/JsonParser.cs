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

using JsonFx.Serialization;

namespace JsonFx.Json
{
	public partial class JsonReader
	{
		/// <summary>
		/// JSON deserializer
		/// </summary>
		public class JsonParser : IDataParser<JsonTokenType>
		{
			#region Constants

			// parse errors
			private const string ErrorUnexpectedToken = "Unexpected JSON token ({0})";
			private const string ErrorExtraTokens = "Extra JSON tokens at end ({0})";

			private const string ErrorExpectedArray = "Expected JSON array start ({0})";
			private const string ErrorExpectedArrayItem = "Expected JSON array item or end of JSON array ({0})";
			private const string ErrorExpectedArrayItemDelim = "Expected JSON array item delimiter or end of JSON array ({0})";
			private const string ErrorMissingArrayItem = "Missing JSON array item";
			private const string ErrorUnterminatedArray = "Unterminated JSON array";

			private const string ErrorExpectedObject = "Expected JSON object start ({0})";
			private const string ErrorExpectedPropertyName = "Expected JSON object property name or end of JSON object ({0})";
			private const string ErrorExpectedPropertyPairDelim = "Expected JSON object property name/value delimiter ({0})";
			private const string ErrorExpectedPropertyValue = "Expected JSON object property value ({0})";
			private const string ErrorExpectedObjectValueDelim = "Expected value delimiter or end of JSON object ({0})";
			private const string ErrorMissingObjectProperty = "Missing JSON object property";
			private const string ErrorUnterminatedObject = "Unterminated JSON object";

			#endregion Constants

			#region Fields

			private readonly DataReaderSettings Settings;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public JsonParser(DataReaderSettings settings)
			{
				this.Settings = settings;
			}

			#endregion Init

			#region Parsing Methods

			/// <summary>
			/// Parses the token stream coercing the result targetType
			/// </summary>
			/// <param name="tokenizer"></param>
			/// <param name="targetType"></param>
			/// <returns></returns>
			public object Parse(IEnumerable<Token<JsonTokenType>> tokenizer, Type targetType)
			{
				IEnumerator<Token<JsonTokenType>> tokens = tokenizer.GetEnumerator();
				if (!tokens.MoveNext())
				{
					// end of input
					return this.Settings.CoerceType(targetType, null);
				}

				object value;

				Token<JsonTokenType> token = tokens.Current;
				switch (token.TokenType)
				{
					case JsonTokenType.ArrayBegin:
					{
						// found array
						value = this.ParseArray(tokens, targetType);
						break;
					}
					case JsonTokenType.ObjectBegin:
					{
						// found object
						value = this.ParseObject(tokens, targetType);
						break;
					}
					case JsonTokenType.Boolean:
					case JsonTokenType.Number:
					case JsonTokenType.Null:
					case JsonTokenType.String:
					case JsonTokenType.Undefined:
					{
						// found primitive
						value = this.Settings.CoerceType(targetType, token.Value);
						break;
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
						throw new ArgumentException(String.Format(
							JsonParser.ErrorUnexpectedToken,
							token.TokenType));
					}
				}

				if (tokens.MoveNext())
				{
					// TODO: evaluate if is ever valid to have tokens beyond JSON
					throw new ArgumentException(String.Format(
						JsonParser.ErrorExtraTokens,
						tokens.Current.TokenType));
				}

				return value;
			}

			private object ParseObject(IEnumerator<Token<JsonTokenType>> tokens, Type objectType)
			{
				Token<JsonTokenType> token = tokens.Current;

				// verify correct starting state
				if (token.TokenType != JsonTokenType.ObjectBegin)
				{
					throw new ArgumentException(String.Format(
						JsonParser.ErrorExpectedObject,
						token.TokenType));
				}

				Type itemType = DataReaderSettings.GetDictionaryItemType(objectType);
				object objectValue = (itemType != null) ?
					this.Settings.InstantiateObject(objectType) :
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
								return this.Settings.CoerceType(objectType, objectValue);
							}

							default:
							{
								// these are invalid here
								throw new ArgumentException(String.Format(
									JsonParser.ErrorExpectedObjectValueDelim,
									token.TokenType));
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
					string memberName;
					switch (token.TokenType)
					{
						case JsonTokenType.Literal:
						case JsonTokenType.String:
						case JsonTokenType.Number:
						{
							memberName = Convert.ToString(token.Value);
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
							return this.Settings.CoerceType(objectType, objectValue);
						}
						case JsonTokenType.ValueDelim:
						{
							// extraneous item delimiter
							throw new ArgumentException(JsonParser.ErrorMissingObjectProperty);
						}
						default:
						{
							// these are invalid here
							throw new ArgumentException(String.Format(
								JsonParser.ErrorExpectedPropertyName,
								token.TokenType));
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
							throw new ArgumentException(String.Format(
								JsonParser.ErrorExpectedPropertyPairDelim,
								token.TokenType));
						}
					}

					// move past delim
					if (!tokens.MoveNext())
					{
						// end of input
						break;
					}
					token = tokens.Current;

					// find the member type info
					MemberInfo memberInfo;
					Type memberType = this.Settings.GetMemberInfo(objectType, itemType, memberName, out memberInfo);

					// parse the property value
					object memberValue;
					switch (token.TokenType)
					{
						case JsonTokenType.ArrayBegin:
						{
							memberValue = this.ParseArray(tokens, memberType);
							break;
						}
						case JsonTokenType.ObjectBegin:
						{
							memberValue = this.ParseObject(tokens, memberType);
							break;
						}
						case JsonTokenType.Boolean:
						case JsonTokenType.Null:
						case JsonTokenType.Number:
						case JsonTokenType.String:
						case JsonTokenType.Undefined:
						{
							memberValue = this.Settings.CoerceType(memberType, token.Value);
							break;
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
							throw new ArgumentException(String.Format(
								JsonParser.ErrorExpectedPropertyValue,
								token.TokenType));
						}
					}

					this.Settings.SetMemberValue(objectValue, objectType, memberInfo, memberName, memberValue);

					hasProperties = true;
				}

				// end of input
				throw new ArgumentException(JsonParser.ErrorUnterminatedObject);
			}

			private object ParseArray(IEnumerator<Token<JsonTokenType>> tokens, Type arrayType)
			{
				Token<JsonTokenType> token = tokens.Current;

				// verify correct starting state
				if (token.TokenType != JsonTokenType.ArrayBegin)
				{
					throw new ArgumentException(String.Format(
						JsonParser.ErrorExpectedArray,
						token.TokenType));
				}

				Type itemType = DataReaderSettings.GetArrayItemType(arrayType);

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
								return this.Settings.CoerceArrayList(arrayType, itemType, array);
							}
							default:
							{
								// these are invalid here
								throw new ArgumentException(String.Format(
									JsonParser.ErrorExpectedArrayItemDelim,
									token.TokenType));
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
							return this.Settings.CoerceArrayList(arrayType, itemType, array);
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
							// primitive item
							if (isItemTypeHint)
							{
								item = token.Value;
							}
							else
							{
								item = this.Settings.CoerceType(itemType, token.Value);
							}
							break;
						}
						case JsonTokenType.ValueDelim:
						{
							// extraneous item delimiter
							throw new ArgumentException(JsonParser.ErrorMissingArrayItem);
						}
						case JsonTokenType.Literal:
						case JsonTokenType.None:
						case JsonTokenType.ObjectEnd:
						case JsonTokenType.PairDelim:
						default:
						{
							// these are invalid here
							throw new ArgumentException(String.Format(
								JsonParser.ErrorExpectedArrayItem,
								token.TokenType));
						}
					}

					// establish common type
					itemType = DataReaderSettings.FindCommonType(itemType, item);

					// add item to the array
					array.Add(item);
				}

				// end of input
				throw new ArgumentException(JsonParser.ErrorUnterminatedArray);
			}

			#endregion Parsing Methods
		}
	}
}