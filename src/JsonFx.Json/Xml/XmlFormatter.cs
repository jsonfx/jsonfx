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

using JsonFx.Common;
using JsonFx.IO;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Xml
{
	public partial class XmlWriter
	{
		/// <summary>
		/// Outputs XML text from a SAX-like input stream of tokens
		/// </summary>
		public class XmlFormatter : ITextFormatter<CommonTokenType>
		{
			#region Fields

			private readonly DataWriterSettings Settings;
			private int depth;
			private bool pendingNewLine;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public XmlFormatter(DataWriterSettings settings)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}

				this.Settings = settings;
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

				IStream<Token<CommonTokenType>> stream = new Stream<Token<CommonTokenType>>(tokens);

				while (!stream.IsCompleted)
				{
					this.WriteValue(writer, stream, null);
				}
			}

			/// <summary>
			/// Formats the token sequence to the writer
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			public void WriteValue(TextWriter writer, IStream<Token<CommonTokenType>> tokens, string elementName)
			{
				if (this.pendingNewLine)
				{
					if (this.Settings.PrettyPrint)
					{
						this.depth++;
						this.WriteLine(writer);
					}
					this.pendingNewLine = false;
				}

				Token<CommonTokenType> token = tokens.Peek();
				switch (token.TokenType)
				{
					case CommonTokenType.ArrayBegin:
					{
						this.WriteArray(writer, tokens, elementName);
						break;
					}
					case CommonTokenType.ObjectBegin:
					{
						this.WriteObject(writer, tokens, elementName);
						break;
					}
					case CommonTokenType.Primitive:
					{
						tokens.Pop();

						if (String.IsNullOrEmpty(elementName))
						{
							elementName = "Data";
						}

						string value = token.ValueAsString();
						if (String.IsNullOrEmpty(value))
						{
							WriteTagEmpty(writer, elementName);
						}
						else
						{
							WriteTagOpen(writer, elementName);
							writer.Write(value);
							WriteTagClose(writer, elementName);
						}
						break;
					}
					default:
					{
						throw new TokenException<CommonTokenType>(token, "Unexpected token");
					}
				}
			}

			private void WriteArray(TextWriter writer, IStream<Token<CommonTokenType>> tokens, string elementName)
			{
				Token<CommonTokenType> token = tokens.Pop();

				// ensure element has a name
				if (String.IsNullOrEmpty(elementName))
				{
					elementName = (token.Name ?? DataName.Empty).LocalName;

					if (String.IsNullOrEmpty(elementName))
					{
						elementName = "Data";
					}
				}

				WriteTagOpen(writer, elementName);
				pendingNewLine = true;

				bool needsValueDelim = false;
				while (!tokens.IsCompleted)
				{
					token = tokens.Peek();
					switch (token.TokenType)
					{
						case CommonTokenType.ArrayEnd:
						{
							tokens.Pop();

							if (this.pendingNewLine)
							{
								this.pendingNewLine = false;
							}
							else if (this.Settings.PrettyPrint)
							{
								this.depth--;
								this.WriteLine(writer);
							}

							WriteTagClose(writer, elementName);
							this.pendingNewLine = true;
							return;
						}
						case CommonTokenType.ArrayBegin:
						case CommonTokenType.ObjectBegin:
						case CommonTokenType.Primitive:
						{
							if (needsValueDelim)
							{
								throw new TokenException<CommonTokenType>(token, "Missing value delimiter");
							}

							if (pendingNewLine)
							{
								if (this.Settings.PrettyPrint)
								{
									this.depth++;
									this.WriteLine(writer);
								}
								pendingNewLine = false;
							}

							string propertyName = (token.Name ?? DataName.Empty).LocalName;

							this.WriteValue(writer, tokens, propertyName);

							pendingNewLine = false;
							needsValueDelim = true;
							break;
						}
						case CommonTokenType.ValueDelim:
						{
							tokens.Pop();

							if (!needsValueDelim)
							{
								throw new TokenException<CommonTokenType>(token, "Missing array item");
							}

							if (this.Settings.PrettyPrint)
							{
								this.WriteLine(writer);
							}
							needsValueDelim = false;
							break;
						}
						default:
						{
							throw new TokenException<CommonTokenType>(token, "Unexpected token");
						}
					}
				}
			}

			private void WriteObject(TextWriter writer, IStream<Token<CommonTokenType>> tokens, string elementName)
			{
				Token<CommonTokenType> token = tokens.Pop();

				// ensure element has a name
				if (String.IsNullOrEmpty(elementName))
				{
					elementName = (token.Name ?? DataName.Empty).LocalName;

					if (String.IsNullOrEmpty(elementName))
					{
						elementName = "Data";
					}
				}

				WriteTagOpen(writer, elementName);
				this.pendingNewLine = true;

				bool needsValueDelim = false;
				while (!tokens.IsCompleted)
				{
					token = tokens.Peek();
					switch (token.TokenType)
					{
						case CommonTokenType.ObjectEnd:
						{
							tokens.Pop();

							if (this.pendingNewLine)
							{
								this.pendingNewLine = false;
							}
							else if (this.Settings.PrettyPrint)
							{
								this.depth--;
								this.WriteLine(writer);
							}

							WriteTagClose(writer, elementName);
							this.pendingNewLine = true;
							return;
						}
						case CommonTokenType.Property:
						{
							tokens.Pop();

							if (needsValueDelim)
							{
								throw new TokenException<CommonTokenType>(token, "Missing value delimiter");
							}

							if (pendingNewLine)
							{
								if (this.Settings.PrettyPrint)
								{
									this.depth++;
									this.WriteLine(writer);
								}
								pendingNewLine = false;
							}

							string propertyName = (token.Name ?? DataName.Empty).LocalName;

							this.WriteValue(writer, tokens, propertyName);

							pendingNewLine = false;
							needsValueDelim = true;
							break;
						}
						case CommonTokenType.ValueDelim:
						{
							tokens.Pop();

							if (!needsValueDelim)
							{
								throw new TokenException<CommonTokenType>(token, "Missing object property");
							}

							if (this.Settings.PrettyPrint)
							{
								this.WriteLine(writer);
							}
							needsValueDelim = false;
							break;
						}
						default:
						{
							throw new TokenException<CommonTokenType>(token, "Unexpected token");
						}
					}
				}
			}

			#endregion ITextFormatter<T> Methods

			#region Write Methods

			private void WriteTagOpen(TextWriter writer, string name)
			{
				if (this.pendingNewLine)
				{
					if (this.Settings.PrettyPrint)
					{
						this.depth++;
						this.WriteLine(writer);
					}
					this.pendingNewLine = false;
				}

				writer.Write(XmlGrammar.OperatorElementBegin);
				writer.Write(name);
				writer.Write(XmlGrammar.OperatorElementEnd);
			}

			private void WriteTagClose(TextWriter writer, string name)
			{
				writer.Write(XmlGrammar.OperatorElementBegin);
				writer.Write(XmlGrammar.OperatorElementClose);
				writer.Write(name);
				writer.Write(XmlGrammar.OperatorElementEnd);
			}

			private void WriteTagEmpty(TextWriter writer, string name)
			{
				writer.Write(XmlGrammar.OperatorElementBegin);
				writer.Write(name);
				writer.Write(XmlGrammar.OperatorValueDelim);
				writer.Write(XmlGrammar.OperatorElementClose);
				writer.Write(XmlGrammar.OperatorElementEnd);
			}

			private void WriteLine(TextWriter writer)
			{
				// emit CRLF
				writer.Write(this.Settings.NewLine);
				for (int i=0; i<this.depth; i++)
				{
					// indent next line accordingly
					writer.Write(this.Settings.Tab);
				}
			}

			#endregion Write Methods
		}
	}
}
