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
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using JsonFx.Common;
using JsonFx.Serialization;

namespace JsonFx.Json
{
	public partial class JsonWriter
	{
		#region Constants

		private const string ErrorUnexpectedToken = "Unexpected token ({0})";

		#endregion Constants

		/// <summary>
		/// Outputs JSON text from an input stream of tokens
		/// </summary>
		public class JsonFormatter : ITextFormatter<CommonTokenType>
		{
			#region Constants

#if WINDOWS_PHONE
			private static readonly JsonFx.CodeGen.ProxyDelegate EnumGetValues = JsonFx.CodeGen.DynamicMethodGenerator.GetMethodProxy(typeof(Enum), "GetValues");
#elif SILVERLIGHT
			private static readonly JsonFx.CodeGen.ProxyDelegate EnumGetValues = JsonFx.CodeGen.DynamicMethodGenerator.GetMethodProxy(typeof(Enum), "InternalGetValues");
#endif

			#endregion Constants

			#region Fields

			private readonly DataWriterSettings Settings;

			// TODO: find a way to generalize this setting
			private readonly bool EncodeLessThan;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public JsonFormatter(DataWriterSettings settings)
				: this(settings, false)
			{
			}

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public JsonFormatter(DataWriterSettings settings, bool encodeLessThan)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}

				this.Settings = settings;
				this.EncodeLessThan = encodeLessThan;
			}

			#endregion Init

			#region ITextFormatter<T> Methods

			/// <summary>
			/// Formats the token sequence as a string
			/// </summary>
			/// <param name="tokens"></param>
			public string Format(IEnumerable<Token<CommonTokenType>> tokens)
			{
				using (StringWriter writer = new StringWriter())
				{
					this.Format(writer, tokens);

					return writer.GetStringBuilder().ToString();
				}
			}

			/// <summary>
			/// Formats the token sequence to the writer
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			public void Format(TextWriter writer, IEnumerable<Token<CommonTokenType>> tokens)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				bool prettyPrint = this.Settings.PrettyPrint;

				// allows us to keep basic context without resorting to a push-down automata
				bool pendingNewLine = false;
				bool needsValueDelim = false;
				int depth = 0;

				foreach (Token<CommonTokenType> token in tokens)
				{
					switch (token.TokenType)
					{
						case CommonTokenType.ArrayBegin:
						{
							if (needsValueDelim)
							{
								writer.Write(JsonGrammar.OperatorValueDelim);
								if (prettyPrint)
								{
									this.WriteLine(writer, depth);
								}
							}

							if (pendingNewLine)
							{
								if (prettyPrint)
								{
									this.WriteLine(writer, ++depth);
								}
								pendingNewLine = false;
							}
							writer.Write(JsonGrammar.OperatorArrayBegin);
							pendingNewLine = true;
							needsValueDelim = false;
							continue;
						}
						case CommonTokenType.ArrayEnd:
						{
							if (pendingNewLine)
							{
								pendingNewLine = false;
							}
							else if (prettyPrint)
							{
								this.WriteLine(writer, --depth);
							}
							writer.Write(JsonGrammar.OperatorArrayEnd);
							needsValueDelim = true;
							continue;
						}
						case CommonTokenType.Primitive:
						{
							if (needsValueDelim)
							{
								writer.Write(JsonGrammar.OperatorValueDelim);
								if (prettyPrint)
								{
									this.WriteLine(writer, depth);
								}
							}

							if (pendingNewLine)
							{
								if (prettyPrint)
								{
									this.WriteLine(writer, ++depth);
								}
								pendingNewLine = false;
							}

							Type valueType =
								(token.Value == null) ? null :
								token.Value.GetType();

							TypeCode typeCode = Type.GetTypeCode(valueType);

							switch (typeCode)
							{
								case TypeCode.Boolean:
								{
									writer.Write(true.Equals(token.Value) ? JsonGrammar.KeywordTrue : JsonGrammar.KeywordFalse);
									break;
								}
								case TypeCode.Byte:
								case TypeCode.Decimal:
								case TypeCode.Double:
								case TypeCode.Int16:
								case TypeCode.Int32:
								case TypeCode.Int64:
								case TypeCode.SByte:
								case TypeCode.Single:
								case TypeCode.UInt16:
								case TypeCode.UInt32:
								case TypeCode.UInt64:
								{
									if (valueType.IsEnum)
									{
										goto default;
									}

									this.WriteNumber(writer, token, typeCode);
									break;
								}
								case TypeCode.DBNull:
								case TypeCode.Empty:
								{
									writer.Write(JsonGrammar.KeywordNull);
									break;
								}
								default:
								{
									IJsonFormattable formattable = token.Value as IJsonFormattable;
									if (formattable != null)
									{
										formattable.Format(this, writer);
										break;
									}

									if (token.Value is TimeSpan)
									{
										this.WriteNumber(writer, token, typeCode);
										break;
									}

									this.WriteString(writer, this.FormatString(token.Value));
									break;
								}
								// TODO: Literals?
								//{
								//    // emit without further introspection as this is a raw extension point
								//    writer.Write(this.FormatString(token.Value));
								//    break;
								//}
							}
							needsValueDelim = true;
							continue;
						}
						case CommonTokenType.ObjectBegin:
						{
							if (needsValueDelim)
							{
								writer.Write(JsonGrammar.OperatorValueDelim);
								if (prettyPrint)
								{
									this.WriteLine(writer, depth);
								}
							}

							if (pendingNewLine)
							{
								if (prettyPrint)
								{
									this.WriteLine(writer, ++depth);
								}
								pendingNewLine = false;
							}
							writer.Write(JsonGrammar.OperatorObjectBegin);
							pendingNewLine = true;
							needsValueDelim = false;
							continue;
						}
						case CommonTokenType.ObjectEnd:
						{
							if (pendingNewLine)
							{
								pendingNewLine = false;
							}
							else if (prettyPrint)
							{
								this.WriteLine(writer, --depth);
							}
							writer.Write(JsonGrammar.OperatorObjectEnd);
							needsValueDelim = true;
							continue;
						}
						case CommonTokenType.Property:
						{
							if (needsValueDelim)
							{
								writer.Write(JsonGrammar.OperatorValueDelim);
								if (prettyPrint)
								{
									this.WriteLine(writer, depth);
								}
							}

							if (pendingNewLine)
							{
								if (prettyPrint)
								{
									this.WriteLine(writer, ++depth);
								}
								pendingNewLine = false;
							}

							string propertyName = token.Name.LocalName;

							this.WriteString(writer, this.FormatString(propertyName));

							if (prettyPrint)
							{
								writer.Write(" ");
								writer.Write(JsonGrammar.OperatorPairDelim);
								writer.Write(" ");
							}
							else
							{
								writer.Write(JsonGrammar.OperatorPairDelim);
							}
							needsValueDelim = false;
							continue;
						}
						case CommonTokenType.None:
						default:
						{
							throw new TokenException<CommonTokenType>(
								token,
								String.Format(ErrorUnexpectedToken, token.TokenType));
						}
					}
				}
			}

			#endregion ITextFormatter<T> Methods

			#region Write Methods

			protected virtual void WriteNumber(TextWriter writer, Token<CommonTokenType> token, TypeCode typeCode)
			{
				bool overflowsIEEE754 = false;

				string number;
				switch (typeCode)
				{
					case TypeCode.Byte:
					{
						number = ((byte)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.Boolean:
					{
						number = true.Equals(token.Value) ? "1" : "0";
						break;
					}
					case TypeCode.Decimal:
					{
						overflowsIEEE754 = true;
						number = ((decimal)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.Double:
					{
						number = ((double)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.Int16:
					{
						number = ((short)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.Int32:
					{
						number = ((int)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.Int64:
					{
						overflowsIEEE754 = true;
						number = ((long)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.SByte:
					{
						number = ((sbyte)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.Single:
					{
						number = ((float)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.UInt16:
					{
						number = ((ushort)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.UInt32:
					{
						overflowsIEEE754 = true;
						number = ((uint)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.UInt64:
					{
						overflowsIEEE754 = true;
						number = ((ulong)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					default:
					{
						if (token.Value is TimeSpan)
						{
							overflowsIEEE754 = true;
							number = ((TimeSpan)token.Value).Ticks.ToString("g", CultureInfo.InvariantCulture);
							break;
						}

						throw new TokenException<CommonTokenType>(token, "Invalid number token");
					}
				}

				if (overflowsIEEE754 && this.InvalidIEEE754(Convert.ToDecimal(token.Value)))
				{
					// checks for IEEE-754 overflow and emit as strings
					this.WriteString(writer, number);
				}
				else
				{
					// fits within an IEEE-754 floating point so emit directly
					writer.Write(number);
				}
			}

			protected virtual void WriteString(TextWriter writer, string value)
			{
				int start = 0,
					length = value.Length;

				writer.Write(JsonGrammar.OperatorStringDelim);

				for (int i=start; i<length; i++)
				{
					char ch = value[i];

					if (ch <= '\u001F' ||
						ch >= '\u007F' ||
						(this.EncodeLessThan && ch == '<') || // improves compatibility within script blocks
						ch == JsonGrammar.OperatorStringDelim ||
						ch == JsonGrammar.OperatorCharEscape)
					{
						if (i > start)
						{
							writer.Write(value.Substring(start, i-start));
						}
						start = i+1;

						switch (ch)
						{
							case JsonGrammar.OperatorStringDelim:
							case JsonGrammar.OperatorCharEscape:
							{
								writer.Write(JsonGrammar.OperatorCharEscape);
								writer.Write(ch);
								continue;
							}
							case '\b':
							{
								writer.Write("\\b");
								continue;
							}
							case '\f':
							{
								writer.Write("\\f");
								continue;
							}
							case '\n':
							{
								writer.Write("\\n");
								continue;
							}
							case '\r':
							{
								writer.Write("\\r");
								continue;
							}
							case '\t':
							{
								writer.Write("\\t");
								continue;
							}
							default:
							{
								writer.Write("\\u");
								writer.Write(ConvertToUtf32(value, i).ToString("X4"));
								continue;
							}
						}
					}
				}

				if (length > start)
				{
					writer.Write(value.Substring(start, length-start));
				}

				writer.Write(JsonGrammar.OperatorStringDelim);
			}

			private void WriteLine(TextWriter writer, int depth)
			{
				// emit CRLF
				writer.Write(this.Settings.NewLine);
				for (int i=0; i<depth; i++)
				{
					// indent next line accordingly
					writer.Write(this.Settings.Tab);
				}
			}

			#endregion Write Methods

			#region String Methods

			/// <summary>
			/// Converts an object to its string representation
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			private string FormatString(object value)
			{
				if (value is Enum)
				{
					return this.FormatEnum((Enum)value);
				}

				return Token<CommonTokenType>.ToString(value);
			}

			#endregion String Methods

			#region Enum Methods

			/// <summary>
			/// Converts an enum to its string representation
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			private string FormatEnum(Enum value)
			{
				Type type = value.GetType();
				IDictionary<Enum, string> map = this.Settings.Resolver.LoadEnumMaps(type);

				string enumName;
				if (type.IsDefined(typeof(FlagsAttribute), true) && !Enum.IsDefined(type, value))
				{
					Enum[] flags = JsonFormatter.GetFlagList(type, value);
					string[] flagNames = new string[flags.Length];
					for (int i=0; i<flags.Length; i++)
					{
						if (!map.TryGetValue(flags[i], out flagNames[i]) ||
							String.IsNullOrEmpty(flagNames[i]))
						{
							flagNames[i] = flags[i].ToString("f");
						}
					}
					enumName = String.Join(", ", flagNames);
				}
				else
				{
					if (!map.TryGetValue(value, out enumName) ||
						String.IsNullOrEmpty(enumName))
					{
						enumName = value.ToString("f");
					}
				}

				return enumName;
			}

			/// <summary>
			/// Splits a bitwise-OR'd set of enums into a list.
			/// </summary>
			/// <param name="enumType">the enum type</param>
			/// <param name="value">the combined value</param>
			/// <returns>list of flag enums</returns>
			/// <remarks>
			/// from PseudoCode.EnumHelper
			/// </remarks>
			private static Enum[] GetFlagList(Type enumType, object value)
			{
				ulong longVal = Convert.ToUInt64(value);
#if SILVERLIGHT
				ulong[] enumValues = (ulong[])JsonFormatter.EnumGetValues(enumType);
#else
				Array enumValues = Enum.GetValues(enumType);
#endif

				List<Enum> enums = new List<Enum>(enumValues.Length);

				// check for empty
				if (longVal == 0L)
				{
					// Return the value of empty, or zero if none exists
					enums.Add((Enum)Convert.ChangeType(value, enumType, CultureInfo.InvariantCulture));
					return enums.ToArray();
				}

				for (int i = enumValues.Length-1; i >= 0; i--)
				{
					ulong enumValue = Convert.ToUInt64(enumValues.GetValue(i));

					if ((i == 0) && (enumValue == 0L))
					{
						continue;
					}

					// matches a value in enumeration
					if ((longVal & enumValue) == enumValue)
					{
						// remove from val
						longVal -= enumValue;

						// add enum to list
						enums.Add(enumValues.GetValue(i) as Enum);
					}
				}

				if (longVal != 0x0L)
				{
					enums.Add(Enum.ToObject(enumType, longVal) as Enum);
				}

				return enums.ToArray();
			}

			#endregion Enum Methods

			#region Number Methods

			/// <summary>
			/// Determines if a numberic value cannot be represented as IEEE-754.
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			/// <remarks>
			/// http://stackoverflow.com/questions/1601646
			/// </remarks>
			protected virtual bool InvalidIEEE754(decimal value)
			{
				try
				{
					return (decimal)(Decimal.ToDouble(value)) != value;
				}
				catch
				{
					return true;
				}
			}

			#endregion Number Methods
		}

		#region Utility Methods

		private static int ConvertToUtf32(string value, int i)
		{
#if SILVERLIGHT
			return (int)value[i];
#else
			return Char.ConvertToUtf32(value, i);
#endif
		}

		#endregion Utility Methods
	}
}
