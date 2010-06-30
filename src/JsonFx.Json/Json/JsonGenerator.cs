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
using System.Xml;

using JsonFx.Serialization;

namespace JsonFx.Json
{
	public partial class JsonWriter
	{
		/// <summary>
		/// Generates a SAX-like sequence of JSON tokens from an object graph
		/// </summary>
		public class JsonGenerator : IDataGenerator<JsonTokenType>
		{
			#region Fields

			private readonly DataWriterSettings Settings;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public JsonGenerator(DataWriterSettings settings)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}

				this.Settings = settings;
			}

			#endregion Init

			#region Generator Methods

			/// <summary>
			/// Generates a sequence of tokens representing the value
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public IEnumerable<Token<JsonTokenType>> GetTokens(object value)
			{
				if (value == null)
				{
					yield return JsonGrammar.TokenNull;
					yield break;
				}

				ISerializable<JsonTokenType> serializable = value as ISerializable<JsonTokenType>;
				if (serializable != null)
				{
					foreach (Token<JsonTokenType> token in serializable.Write())
					{
						yield return token;
					}
					yield break;
				}

				Type type = value.GetType();

				// must test enumerations before other value types
				if (type.IsEnum)
				{
					yield return JsonGrammar.TokenString((Enum)value);
					yield break;
				}

				// Type.GetTypeCode() allows us to more efficiently switch type
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Boolean:
					{
						yield return true.Equals(value) ? JsonGrammar.TokenTrue : JsonGrammar.TokenFalse;
						yield break;
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
						yield return JsonGrammar.TokenNumber((ValueType)value);
						yield break;
					}
					case TypeCode.Double:
					{
						double doubleVal = (double)value;

						if (Double.IsNaN(doubleVal))
						{
							yield return JsonGrammar.TokenNaN;
						}
						else if (Double.IsPositiveInfinity(doubleVal))
						{
							yield return JsonGrammar.TokenPositiveInfinity;
						}
						else if (Double.IsNegativeInfinity(doubleVal))
						{
							yield return JsonGrammar.TokenNegativeInfinity;
						}
						else
						{
							yield return JsonGrammar.TokenNumber(doubleVal);
						}
						yield break;
					}
					case TypeCode.Single:
					{
						float floatVal = (float)value;

						if (Single.IsNaN(floatVal))
						{
							yield return JsonGrammar.TokenNaN;
						}
						else if (Single.IsPositiveInfinity(floatVal))
						{
							yield return JsonGrammar.TokenPositiveInfinity;
						}
						else if (Single.IsNegativeInfinity(floatVal))
						{
							yield return JsonGrammar.TokenNegativeInfinity;
						}
						else
						{
							yield return JsonGrammar.TokenNumber(floatVal);
						}
						yield break;
					}
					case TypeCode.Char:
					case TypeCode.DateTime:
					case TypeCode.String:
					{
						yield return JsonGrammar.TokenString(value);
						yield break;
					}
					case TypeCode.DBNull:
					case TypeCode.Empty:
					{
						yield return JsonGrammar.TokenNull;
						yield break;
					}
				}

				if (value is IEnumerable)
				{
					if (value is XmlNode)
					{
						foreach (Token<JsonTokenType> token in this.GetXmlTokens((XmlNode)value))
						{
							yield return token;
						}
						yield break;
					}

					foreach (Token<JsonTokenType> token in this.GetArrayTokens((IEnumerable)value))
					{
						yield return token;
					}
					yield break;
				}

				if (value is Guid || value is Uri || value is Version)
				{
					yield return JsonGrammar.TokenString(value);
					yield break;
				}

				if (value is TimeSpan)
				{
					yield return JsonGrammar.TokenNumber((TimeSpan)value);
					yield break;
				}

				// all other structs and classes
				foreach (Token<JsonTokenType> token in this.GetObjectTokens(value, type))
				{
					yield return token;
				}
			}

			private IEnumerable<Token<JsonTokenType>> GetArrayTokens(IEnumerable value)
			{
				IEnumerator enumerator = value.GetEnumerator();

				if (enumerator is IDictionaryEnumerator)
				{
					foreach (Token<JsonTokenType> token in this.GetObjectTokens((IDictionaryEnumerator)enumerator))
					{
						yield return token;
					}
					yield break;
				}

				yield return JsonGrammar.TokenArrayBegin;

				bool appendDelim = false;
				while (enumerator.MoveNext())
				{
					if (appendDelim)
					{
						yield return JsonGrammar.TokenValueDelim;
					}
					else
					{
						appendDelim = true;
					}

					foreach (Token<JsonTokenType> token in this.GetTokens(enumerator.Current))
					{
						yield return token;
					}
				}

				yield return JsonGrammar.TokenArrayEnd;
			}

			private IEnumerable<Token<JsonTokenType>> GetObjectTokens(IDictionaryEnumerator enumerator)
			{
				bool appendDelim = false;

				yield return JsonGrammar.TokenObjectBegin;

				while (enumerator.MoveNext())
				{
					DictionaryEntry entry = enumerator.Entry;

					if (appendDelim)
					{
						yield return JsonGrammar.TokenValueDelim;
					}
					else
					{
						appendDelim = true;
					}

					foreach (Token<JsonTokenType> token in this.GetPropertyTokens(entry.Key, entry.Value))
					{
						yield return token;
					}
				}

				yield return JsonGrammar.TokenObjectEnd;
			}

			private IEnumerable<Token<JsonTokenType>> GetPropertyTokens(object key, object value)
			{
				yield return JsonGrammar.TokenString(key);
				yield return JsonGrammar.TokenPairDelim;

				foreach (Token<JsonTokenType> token in this.GetTokens(value))
				{
					yield return token;
				}
			}

			private IEnumerable<Token<JsonTokenType>> GetObjectTokens(object value, Type type)
			{
				yield return JsonGrammar.TokenObjectBegin;
				yield return JsonGrammar.TokenObjectEnd;
			}

			private IEnumerable<Token<JsonTokenType>> GetXmlTokens(XmlNode value)
			{
				// TODO: translate XML to JsonML?
				yield return JsonGrammar.TokenString(value.OuterXml);
			}

			#endregion Generator Methods
		}
	}
}