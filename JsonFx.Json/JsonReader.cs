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
				if (targetType != null)
				{
					return this.Settings.CoerceType(targetType, null);
				}

				return null;
			}

			switch (tokens.Current.TokenType)
			{
				case JsonTokenType.ArrayBegin:
				{
					return this.ParseArray(tokens, targetType);
				}
				case JsonTokenType.ObjectBegin:
				{
					return this.ParseObject(tokens, targetType);
				}
				case JsonTokenType.Boolean:
				case JsonTokenType.Number:
				case JsonTokenType.Null:
				case JsonTokenType.String:
				case JsonTokenType.Undefined:
				{
					if (targetType != null)
					{
						return this.Settings.CoerceType(targetType, tokens.Current.Value);
					}
					return tokens.Current.Value;
				}
				default:
				{
					throw new ArgumentException(String.Format(
						JsonReader.ErrorUnexpectedToken,
						tokens.Current.TokenType));
				}
			}
		}

		private object ParseArray(IEnumerator<Token<JsonTokenType>> tokens, Type arrayType)
		{
			bool isArrayItemTypeSet = (arrayType != null);
			bool isArrayTypeAHint = !isArrayItemTypeSet;
			Type arrayItemType = null;

			if (isArrayItemTypeSet)
			{
				if (arrayType.HasElementType)
				{
					arrayItemType = arrayType.GetElementType();
				}
				else if (arrayType.IsGenericType)
				{
					Type[] generics = arrayType.GetGenericArguments();
					if (generics.Length == 1)
					{
						// could use the first or last, but this more correct
						arrayItemType = generics[0];
					}
				}
			}

			// using ArrayList since has .ToArray(Type) method
			// cannot create generic list at runtime
			ArrayList array = new ArrayList();

			do
			{
				if (!tokens.MoveNext())
				{
					break;
				}

				Token<JsonTokenType> token = tokens.Current;
				switch (token.TokenType)
				{
					case JsonTokenType.ArrayEnd:
					{
						if (arrayType != null)
						{
							return this.Settings.CoerceType(arrayType, array);
						}

						return array;
					}
					case JsonTokenType.Boolean:
					case JsonTokenType.Null:
					case JsonTokenType.Number:
					case JsonTokenType.String:
					case JsonTokenType.Undefined:
					{
						// primitives
						object value = token.Value;

						if (arrayItemType != null)
						{
							// TODO: establish common type
						}

						array.Add(value);
						break;
					}
					case JsonTokenType.ObjectBegin:
					{
						object value = this.ParseObject(tokens, arrayItemType);

						// TODO: establish common type

						array.Add(value);
						break;
					}
					case JsonTokenType.ValueDelim:
					{
						throw new ArgumentException(JsonReader.ErrorMissingArrayValue);
					}
				}
			} while (tokens.MoveNext() && tokens.Current.TokenType == JsonTokenType.ValueDelim);

			throw new ArgumentException(JsonReader.ErrorUnterminatedArray);
		}

		private object ParseObject(IEnumerator<Token<JsonTokenType>> tokens, Type objectType)
		{
			object value = null;

			while (tokens.MoveNext())
			{
				switch (tokens.Current.TokenType)
				{
					case JsonTokenType.ObjectEnd:
					{
						if (objectType != null)
						{
							return this.Settings.CoerceType(objectType, value);
						}

						return value;
					}
				}
			}

			throw new ArgumentException(JsonReader.ErrorUnterminatedObject);
		}

		private ITokenizer<JsonTokenType> GetTokenizer(TextReader input)
		{
			return new JsonTokenizer(input);
		}

		#endregion Parsing Methods
	}
}
