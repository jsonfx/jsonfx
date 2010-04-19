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
				// TODO: render tokens
				foreach (Token<JsonTokenType> token in tokens)
				{
					switch (token.TokenType)
					{
						case JsonTokenType.ArrayBegin:
						{
							writer.Write(JsonGrammar.OperatorArrayBegin);
							this.WriteLine(writer);
							break;
						}
						case JsonTokenType.ArrayEnd:
						{
							writer.Write(JsonGrammar.OperatorArrayEnd);
							break;
						}
						case JsonTokenType.Boolean:
						{
							writer.Write(true.Equals(token.Value) ? JsonGrammar.KeywordTrue : JsonGrammar.KeywordFalse);
							break;
						}
						case JsonTokenType.Literal:
						{
							writer.Write(token.Value);
							break;
						}
						case JsonTokenType.Null:
						{
							writer.Write(JsonGrammar.KeywordNull);
							break;
						}
						case JsonTokenType.Number:
						{
							writer.Write(token.Value);
							break;
						}
						case JsonTokenType.ObjectBegin:
						{
							writer.Write(JsonGrammar.OperatorObjectBegin);
							break;
						}
						case JsonTokenType.ObjectEnd:
						{
							this.WriteLine(writer);
							writer.Write(JsonGrammar.OperatorObjectEnd);
							break;
						}
						case JsonTokenType.PairDelim:
						{
							writer.Write(JsonGrammar.OperatorPairDelim);
							break;
						}
						case JsonTokenType.String:
						{
							writer.Write(JsonGrammar.OperatorStringDelim);
							// TODO: escape string
							writer.Write(token.Value);
							writer.Write(JsonGrammar.OperatorStringDelim);
							break;
						}
						case JsonTokenType.Undefined:
						{
							writer.Write(JsonGrammar.KeywordUndefined);
							break;
						}
						case JsonTokenType.ValueDelim:
						{
							writer.Write(JsonGrammar.OperatorValueDelim);
							this.WriteLine(writer);
							break;
						}
						case JsonTokenType.None:
						default:
						{
							break;
						}
					}
				}
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
