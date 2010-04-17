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
using System.IO;
using System.Reflection;

namespace JsonFx.Json
{
	/// <summary>
	/// JSON deserializer
	/// </summary>
	public class JsonReader : DataReaderBase
	{
		#region Constants
		
		// parse errors
		private const string ErrorUnexpectedToken = "Unexpected token ({0})";
		private const string ErrorUnterminatedObject = "Unterminated JSON object.";
		private const string ErrorUnterminatedArray = "Unterminated JSON array.";
		private const string ErrorMissingArrayValue = "Missing array value";
		private const string ErrorExpectedString = "Expected JSON string.";
		private const string ErrorExpectedObject = "Expected JSON object.";
		private const string ErrorExpectedArray = "Expected JSON array.";
		private const string ErrorExpectedPropertyName = "Expected JSON object property name.";
		private const string ErrorExpectedPropertyPairDelim = "Expected JSON object property name/value delimiter.";
		private const string ErrorGenericIDictionary = "Types which implement Generic IDictionary<TKey, TValue> also need to implement IDictionary to be deserialized. ({0})";
		private const string ErrorGenericIDictionaryKeys = "Types which implement Generic IDictionary<TKey, TValue> need to have string keys to be deserialized. ({0})";

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public JsonReader(DataReaderSettings settings)
			: base(settings)
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the supported content type of the serialized data
		/// </summary>
		public override string ContentType
		{
			get { return "application/json"; }
		}

		#endregion Properties

		#region IDataReader Methods

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		public override object Deserialize(TextReader input, Type targetType)
		{
			// TODO: figure out how to generically surface ability to swap tokenizers without making interface cumbersome
			// TODO: will buffering prevent multiple chunked reads?
			ITokenizer<JsonTokenType> tokenizer = this.GetTokenizer(input);

			try
			{
				return this.Parse(tokenizer.GetEnumerator(), targetType);
			}
			catch (Exception ex)
			{
				throw new JsonDeserializationException(ex.Message, tokenizer.Position, ex);
			}
		}

		#endregion IDataReader Methods

		#region Parsing Methods

		private object Parse(IEnumerator<Token<JsonTokenType>> tokens, Type targetType)
		{
			if (!tokens.MoveNext())
			{
				// end of input
				if (targetType != null && targetType != typeof(object))
				{
					return this.Settings.CoerceType(targetType, null);
				}

				return null;
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
					// foudn primitive
					if (targetType != null && targetType != typeof(object))
					{
						return this.Settings.CoerceType(targetType, token.Value);
					}
					return token.Value;
				}
				case JsonTokenType.ArrayEnd:
				case JsonTokenType.Identifier:
				case JsonTokenType.None:
				case JsonTokenType.ObjectEnd:
				case JsonTokenType.PairDelim:
				case JsonTokenType.ValueDelim:
				default:
				{
					// these are invalid here
					throw new ArgumentException(String.Format(
						JsonReader.ErrorUnexpectedToken,
						token.TokenType));
				}
			}
		}

		private object ParseObject(IEnumerator<Token<JsonTokenType>> tokens, Type objectType)
		{
			Token<JsonTokenType> token = tokens.Current;

			// verify correct starting state
			if (token.TokenType != JsonTokenType.ObjectBegin)
			{
				throw new ArgumentException(String.Format(
					JsonReader.ErrorUnexpectedToken,
					token.TokenType));
			}

			Dictionary<string, object> value = new Dictionary<string, object>();

			while (!tokens.MoveNext())
			{
				token = tokens.Current;
				if (value.Count > 0)
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
							if (objectType != null && objectType != typeof(object))
							{
								return this.Settings.CoerceType(objectType, value);
							}

							return value;
						}
						default:
						{
							// these are invalid here
							throw new ArgumentException(String.Format(
								JsonReader.ErrorUnexpectedToken,
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
					case JsonTokenType.Identifier:
					case JsonTokenType.String:
					case JsonTokenType.Number:
					{
						memberName = Convert.ToString(token.Value);
						break;
					}
					case JsonTokenType.ObjectEnd:
					{
						if (value.Count > 0)
						{
							goto default;
						}

						// end of the object loop
						if (objectType != null && objectType != typeof(object))
						{
							return this.Settings.CoerceType(objectType, value);
						}

						return value;
					}
					default:
					{
						// these are invalid here
						throw new ArgumentException(String.Format(
							JsonReader.ErrorUnexpectedToken,
							token.TokenType));
					}
				}

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
							JsonReader.ErrorUnexpectedToken,
							token.TokenType));
					}
				}

				// find the member type
				Type memberType = null;
				if (objectType != null && objectType != typeof(object))
				{
					MemberInfo info = this.Settings[objectType, memberName];
					if (info is PropertyInfo)
					{
						memberType = ((PropertyInfo)info).PropertyType;
					}
					else if (info is FieldInfo)
					{
						memberType = ((FieldInfo)info).FieldType;
					}
				}

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
						memberValue = token.Value;
						break;
					}
					case JsonTokenType.ArrayEnd:
					case JsonTokenType.Identifier:
					case JsonTokenType.None:
					case JsonTokenType.ObjectEnd:
					case JsonTokenType.PairDelim:
					case JsonTokenType.ValueDelim:
					default:
					{
						// these are invalid here
						throw new ArgumentException(String.Format(
							JsonReader.ErrorUnexpectedToken,
							token.TokenType));
					}
				}

				if (memberType != null && memberType != typeof(object))
				{
					memberValue = this.Settings.CoerceType(memberType, memberValue);
				}
				value[memberName] = memberValue;

				// move past delim
				if (!tokens.MoveNext())
				{
					// end of input
					break;
				}
				token = tokens.Current;
			}

			// end of input
			throw new ArgumentException(JsonReader.ErrorUnterminatedObject);
		}

		private object ParseArray(IEnumerator<Token<JsonTokenType>> tokens, Type arrayType)
		{
			Token<JsonTokenType> token = tokens.Current;

			// verify correct starting state
			if (token.TokenType != JsonTokenType.ArrayBegin)
			{
				throw new ArgumentException(String.Format(
					JsonReader.ErrorUnexpectedToken,
					token.TokenType));
			}

			Type arrayItemType = null;
			if (arrayType != null)
			{
				if (arrayType.HasElementType)
				{
					// found array element type
					arrayItemType = arrayType.GetElementType();
				}
				else if (arrayType.IsGenericType)
				{
					Type[] generics = arrayType.GetGenericArguments();
					if (generics.Length == 1)
					{
						// found list or enumerable type
						arrayItemType = generics[0];
					}
				}
			}

			// if arrayItemType was specified by caller, then isn't just a hint
			bool isArrayTypeHint = (arrayItemType != null);

			// using ArrayList since has .ToArray(Type) method
			// cannot create List<T> at runtime
			ArrayList array = new ArrayList();

			while (!tokens.MoveNext())
			{
				token = tokens.Current;
				if (array.Count > 0)
				{
					// parse item delimiter
					if (token.TokenType != JsonTokenType.ValueDelim)
					{
						// these are invalid here
						throw new ArgumentException(String.Format(
							JsonReader.ErrorUnexpectedToken,
							token.TokenType));
					}

					if (!tokens.MoveNext())
					{
						// end of input
						break;
					}
					token = tokens.Current;
				}

				// parse the item
				switch (token.TokenType)
				{
					case JsonTokenType.ArrayEnd:
					{
						// end of the array
						if (arrayType != null && arrayType != typeof(object))
						{
							// convert to requested array type
							return this.Settings.CoerceType(arrayType, array);
						}

						if (arrayItemType != null && arrayItemType != typeof(object))
						{
							// if all items are of same type then convert to array of that type
							return array.ToArray(arrayItemType);
						}

						// convert to an object array for consistency
						return array.ToArray();
					}
					case JsonTokenType.ArrayBegin:
					{
						// array item
						object item = this.ParseArray(tokens, isArrayTypeHint ? null : arrayItemType);

						// establish common type
						arrayItemType = this.FindCommonType(arrayItemType, item);

						array.Add(item);
						break;
					}
					case JsonTokenType.ObjectBegin:
					{
						// object item
						object item = this.ParseObject(tokens, isArrayTypeHint ? null : arrayItemType);

						// establish common type
						arrayItemType = this.FindCommonType(arrayItemType, item);

						array.Add(item);
						break;
					}
					case JsonTokenType.Boolean:
					case JsonTokenType.Null:
					case JsonTokenType.Number:
					case JsonTokenType.String:
					case JsonTokenType.Undefined:
					{
						// primitive item
						object item = token.Value;

						// establish common type
						arrayItemType = this.FindCommonType(arrayItemType, item);

						array.Add(item);
						break;
					}
					case JsonTokenType.ValueDelim:
					{
						// extraneous item delimiter
						throw new ArgumentException(JsonReader.ErrorMissingArrayValue);
					}
					case JsonTokenType.Identifier:
					case JsonTokenType.None:
					case JsonTokenType.ObjectEnd:
					case JsonTokenType.PairDelim:
					default:
					{
						// these are invalid here
						throw new ArgumentException(String.Format(
							JsonReader.ErrorUnexpectedToken,
							token.TokenType));
					}
				}
			}

			// end of input
			throw new ArgumentException(JsonReader.ErrorUnterminatedArray);
		}

		private Type FindCommonType(Type arrayItemType, object value)
		{
			// establish if array is of common type
			if (value == null)
			{
				if (arrayItemType != null && arrayItemType.IsValueType)
				{
					// must use plain object to hold null
					arrayItemType = typeof(object);
				}
			}
			else if (arrayItemType == null)
			{
				// try out a hint type
				// if hasn't been set before
				arrayItemType = value.GetType();
			}
			else if (!arrayItemType.IsAssignableFrom(value.GetType()))
			{
				if (value.GetType().IsAssignableFrom(arrayItemType))
				{
					// attempt to use the more general type
					arrayItemType = value.GetType();
				}
				else
				{
					// use plain object to hold value
					arrayItemType = typeof(object);
				}
			}

			return arrayItemType;
		}

		private ITokenizer<JsonTokenType> GetTokenizer(TextReader input)
		{
			return new JsonTokenizer(input);
		}

		#endregion Parsing Methods
	}
}
