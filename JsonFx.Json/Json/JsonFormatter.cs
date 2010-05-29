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
using System.IO;

using JsonFx.Serialization;

namespace JsonFx.Json
{
	public partial class JsonWriter
	{
		/// <summary>
		/// Ouputs JSON text from a SAX-like input stream of JSON tokens
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
				this.Settings = settings;
			}

			#endregion Init

			#region Properties

			/// <summary>
			/// Gets the total number of characters read from the input
			/// </summary>
			public int Depth
			{
				get { return this.depth; }
			}

			/// <summary>
			/// Gets the underlying TextWriter
			/// </summary>
			public TextWriter TextWriter
			{
				get { return this.Writer; }
			}

			#endregion Properties

			#region Methods

			/// <summary>
			/// Formats the token sequence
			/// </summary>
			/// <param name="generator"></param>
			public void Write(TextWriter writer, IEnumerable<Token<JsonTokenType>> tokens)
			{
				foreach (Token<JsonTokenType> token in tokens)
				{
					switch (token.TokenType)
					{
						case JsonTokenType.ArrayBegin:
						{
							writer.Write(JsonGrammar.OperatorArrayBegin);
							this.WriteLine(writer);
							continue;
						}
						case JsonTokenType.ArrayEnd:
						{
							writer.Write(JsonGrammar.OperatorArrayEnd);
							continue;
						}
						case JsonTokenType.Boolean:
						{
							writer.Write(true.Equals(token.Value) ? JsonGrammar.KeywordTrue : JsonGrammar.KeywordFalse);
							continue;
						}
						case JsonTokenType.Literal:
						{
							writer.Write(token.Value);
							continue;
						}
						case JsonTokenType.Null:
						{
							writer.Write(JsonGrammar.KeywordNull);
							continue;
						}
						case JsonTokenType.Number:
						{
							writer.Write(token.Value);
							continue;
						}
						case JsonTokenType.ObjectBegin:
						{
							writer.Write(JsonGrammar.OperatorObjectBegin);
							continue;
						}
						case JsonTokenType.ObjectEnd:
						{
							this.WriteLine(writer);
							writer.Write(JsonGrammar.OperatorObjectEnd);
							continue;
						}
						case JsonTokenType.PairDelim:
						{
							writer.Write(JsonGrammar.OperatorPairDelim);
							continue;
						}
						case JsonTokenType.String:
						{
							if (token.Value == null)
							{
								goto case JsonTokenType.Null;
							}

							this.WriteString(writer, (string)token.Value);
							continue;
						}
						case JsonTokenType.Undefined:
						{
							writer.Write(JsonGrammar.KeywordUndefined);
							continue;
						}
						case JsonTokenType.ValueDelim:
						{
							writer.Write(JsonGrammar.OperatorValueDelim);
							this.WriteLine(writer);
							continue;
						}
						case JsonTokenType.None:
						default:
						{
							this.WriteUnknown(writer, token);
							continue;
						}
					}
				}
			}

			private void WriteString(TextWriter writer, string value)
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

			private void WriteUnknown(TextWriter writer, Token<JsonTokenType> token)
			{
#if DEBUG
				// TODO: determine if this is ever valid
				throw new NotSupportedException("Unexpected JSON token: "+token);
#endif
			}

			private void WriteLine(TextWriter writer)
			{
				if (this.Settings.PrettyPrint)
				{
					writer.Write(this.Settings.NewLine);
					for (int i=0; i<this.depth; i++)
					{
						writer.Write(this.Settings.Tab);
					}
				}
			}

			#endregion Methods
		}
	}
}
