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

using JsonFx.Serialization;

namespace JsonFx.Json
{
	public partial class JsonWriter
	{
		/// <summary>
		/// Outputs JSON text from a SAX-like input stream of JSON tokens
		/// </summary>
		public class JsonFormatter : IDataFormatter<JsonTokenType>
		{
			#region Fields

			private readonly DataWriterSettings Settings;
			private TextWriter Writer = TextWriter.Null;
			private int depth = 0;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public JsonFormatter(DataWriterSettings settings)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}

				this.Settings = settings;
			}

			#endregion Init

			#region Methods

			/// <summary>
			/// Formats the token sequence as a string
			/// </summary>
			/// <param name="tokens"></param>
			public string Format(IEnumerable<Token<JsonTokenType>> tokens)
			{
				using (StringWriter writer = new StringWriter())
				{
					this.Format(writer, tokens);

					return writer.GetStringBuilder().ToString();
				}
			}

			/// <summary>
			/// Formats the token sequence to the output writer
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			public void Format(TextWriter writer, IEnumerable<Token<JsonTokenType>> tokens)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				// allows us to keep basic context without resorting to a push-down automata
				bool pendingNewLine = false;

				foreach (Token<JsonTokenType> token in tokens)
				{
					switch (token.TokenType)
					{
						case JsonTokenType.ArrayBegin:
						{
							if (pendingNewLine)
							{
								this.WriteLine(writer, +1);
								pendingNewLine = false;
							}
							writer.Write(JsonGrammar.OperatorArrayBegin);
							pendingNewLine = true;
							continue;
						}
						case JsonTokenType.ArrayEnd:
						{
							if (!pendingNewLine)
							{
								this.WriteLine(writer, -1);
							}
							pendingNewLine = false;
							writer.Write(JsonGrammar.OperatorArrayEnd);
							continue;
						}
						case JsonTokenType.Boolean:
						{
							if (pendingNewLine)
							{
								this.WriteLine(writer, +1);
								pendingNewLine = false;
							}
							writer.Write(true.Equals(token.Value) ? JsonGrammar.KeywordTrue : JsonGrammar.KeywordFalse);
							continue;
						}
						case JsonTokenType.Literal:
						{
							if (token.Value == null)
							{
								goto case JsonTokenType.Null;
							}

							if (pendingNewLine)
							{
								this.WriteLine(writer, +1);
								pendingNewLine = false;
							}

							// emit without further introspection as this is an extension point
							writer.Write(Convert.ToString(token.Value, CultureInfo.InvariantCulture));
							continue;
						}
						case JsonTokenType.Null:
						{
							if (pendingNewLine)
							{
								this.WriteLine(writer, +1);
								pendingNewLine = false;
							}
							writer.Write(JsonGrammar.KeywordNull);
							continue;
						}
						case JsonTokenType.Number:
						{
							if (pendingNewLine)
							{
								this.WriteLine(writer, +1);
								pendingNewLine = false;
							}
							this.FormatNumber(writer, token);
							continue;
						}
						case JsonTokenType.ObjectBegin:
						{
							if (pendingNewLine)
							{
								this.WriteLine(writer, +1);
								pendingNewLine = false;
							}
							writer.Write(JsonGrammar.OperatorObjectBegin);
							pendingNewLine = true;
							continue;
						}
						case JsonTokenType.ObjectEnd:
						{
							if (!pendingNewLine)
							{
								this.WriteLine(writer, -1);
							}
							pendingNewLine = false;
							writer.Write(JsonGrammar.OperatorObjectEnd);
							continue;
						}
						case JsonTokenType.PairDelim:
						{
							if (this.Settings.PrettyPrint)
							{
								writer.Write(" ");
								writer.Write(JsonGrammar.OperatorPairDelim);
								writer.Write(" ");
							}
							else
							{
								writer.Write(JsonGrammar.OperatorPairDelim);
							}
							continue;
						}
						case JsonTokenType.String:
						{
							if (token.Value == null)
							{
								goto case JsonTokenType.Null;
							}

							if (pendingNewLine)
							{
								this.WriteLine(writer, +1);
								pendingNewLine = false;
							}
							this.FormatString(writer, Convert.ToString(token.Value, CultureInfo.InvariantCulture));
							continue;
						}
						case JsonTokenType.Undefined:
						{
							if (pendingNewLine)
							{
								this.WriteLine(writer, +1);
								pendingNewLine = false;
							}
							writer.Write(JsonGrammar.KeywordUndefined);
							continue;
						}
						case JsonTokenType.ValueDelim:
						{
							writer.Write(JsonGrammar.OperatorValueDelim);
							this.WriteLine(writer, 0);
							continue;
						}
						case JsonTokenType.None:
						default:
						{
							throw new NotSupportedException("Unexpected JSON token: "+token);
						}
					}
				}
			}

			protected virtual void FormatNumber(TextWriter writer, Token<JsonTokenType> token)
			{
				if (token.TokenType != JsonTokenType.Number || token.Value == null)
				{
					throw new SerializationException("Invalid Number token: "+token);
				}

				bool overflowsIEEE754 = false;

				string number;
				switch (Type.GetTypeCode(token.Value.GetType()))
				{
					case TypeCode.Boolean:
					{
						number = true.Equals(token.Value) ? "1" : "0";
						break;
					}
					case TypeCode.Byte:
					{
						number = ((byte)token.Value).ToString("g", CultureInfo.InvariantCulture);
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
					case TypeCode.Decimal:
					{
						overflowsIEEE754 = true;
						number = ((decimal)token.Value).ToString("g", CultureInfo.InvariantCulture);
						break;
					}
					case TypeCode.Int64:
					{
						overflowsIEEE754 = true;
						number = ((long)token.Value).ToString("g", CultureInfo.InvariantCulture);
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
						throw new SerializationException("Invalid Number token: "+token);
					}
				}

				if (overflowsIEEE754 && this.InvalidIEEE754(Convert.ToDecimal(token.Value)))
				{
					// checks for IEEE-754 overflow and emit as strings
					this.FormatString(writer, number);
				}
				else
				{
					// fits within an IEEE-754 floating point so emit directly
					writer.Write(number);
				}
			}

			protected virtual void FormatString(TextWriter writer, string value)
			{
				int start = 0,
					length = value.Length;

				writer.Write(JsonGrammar.OperatorStringDelim);

				for (int i=start; i<length; i++)
				{
					char ch = value[i];

					if (ch <= '\u001F' ||
						ch >= '\u007F' ||
						ch == '<' || // improves compatibility within script blocks
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
								writer.Write(Char.ConvertToUtf32(value, i).ToString("X4"));
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

			#endregion Format Methods

			#region PrettyPrint Methods

			private void WriteLine(TextWriter writer, int depthChange)
			{
				if (depthChange != 0)
				{
					this.depth += depthChange;
					if (this.depth < 0)
					{
						// depth should never be negative
						throw new SerializationException("Formatter depth cannot be negative");
					}
					else if (this.depth >= this.Settings.MaxDepth)
					{
						// TODO: should this move to generator along with an option to check for cycles rather than depth?
						throw new SerializationException("Maximum depth exceeded: potential graph cycle detected.");
					}
				}

				if (this.Settings.PrettyPrint)
				{
					// emit CRLF
					writer.Write(this.Settings.NewLine);
					for (int i=0; i<this.depth; i++)
					{
						// indent next line accordingly
						writer.Write(this.Settings.Tab);
					}
				}
			}

			#endregion PrettyPrint Methods

			#region Utility Methods

			/// <summary>
			/// Determines if a numberic value cannot be represented as IEEE-754.
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			protected virtual bool InvalidIEEE754(decimal value)
			{
				// http://stackoverflow.com/questions/1601646

				try
				{
					return (decimal)(Decimal.ToDouble(value)) != value;
				}
				catch
				{
					return true;
				}
			}

			#endregion Utility Methods
		}
	}
}
