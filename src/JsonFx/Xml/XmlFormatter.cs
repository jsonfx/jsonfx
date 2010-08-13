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
using System.Text;
using System.Xml;

using JsonFx.IO;
using JsonFx.Markup;
using JsonFx.Serialization;

namespace JsonFx.Xml
{
	public partial class XmlWriter
	{
		/// <summary>
		/// Outputs XML text from an input stream of tokens
		/// </summary>
		public class XmlFormatter : ITextFormatter<MarkupTokenType>
		{
			#region XmlWriterAdapter

			/// <summary>
			/// Wraps an XmlWriter as a TextWriter
			/// </summary>
			private class XmlWriterAdapter : TextWriter
			{
				#region Init

				/// <summary>
				/// Ctor
				/// </summary>
				/// <param name="writer"></param>
				public XmlWriterAdapter(System.Xml.XmlWriter writer)
				{
					this.Writer = writer;
				}

				#endregion Init

				#region Properties

				/// <summary>
				/// Gets the underlying XmlWriter
				/// </summary>
				public System.Xml.XmlWriter Writer
				{
					get;
					private set;
				}

				#endregion Properties

				#region Methods

				public override void Write(char value)
				{
					this.Writer.WriteRaw(Char.ToString(value));
				}

				public override void Write(char[] buffer)
				{
					if (buffer != null)
					{
						this.Writer.WriteRaw(buffer, 0, buffer.Length);
					}
				}

				public override void Write(char[] buffer, int index, int count)
				{
					if (buffer != null)
					{
						this.Writer.WriteRaw(buffer, index, count);
					}
				}

				public override void Write(string value)
				{
					this.Writer.WriteRaw(value);
				}

				public override void Flush()
				{
					this.Writer.Flush();
				}

				public override Encoding Encoding
				{
					get { return this.Writer.Settings.Encoding; }
				}

				#endregion Methods
			}

			#endregion XmlWriterAdapter

			#region Constants

			private const string ErrorUnexpectedToken = "Unexpected token ({0})";

			#endregion Constants

			#region Fields

			private readonly DataWriterSettings Settings;

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

			#region ITextFormatter<MarkupTokenType> Methods

			/// <summary>
			/// Formats the token sequence as a string
			/// </summary>
			/// <param name="tokens"></param>
			public string Format(IEnumerable<Token<MarkupTokenType>> tokens)
			{
				using (StringWriter writer = new StringWriter())
				{
					this.Format(tokens, writer);

					return writer.GetStringBuilder().ToString();
				}
			}

			/// <summary>
			/// Formats the token sequence to the writer
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			public void Format(IEnumerable<Token<MarkupTokenType>> tokens, TextWriter writer)
			{
				if (writer == null)
				{
					throw new ArgumentNullException("writer");
				}

				XmlWriterAdapter adapter = writer as XmlWriterAdapter;
				if (adapter != null)
				{
					this.Format(adapter.Writer, tokens);
					return;
				}

				using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(
					writer,
					new XmlWriterSettings
					{
						CheckCharacters = false,
						ConformanceLevel = System.Xml.ConformanceLevel.Auto,
						Encoding = Encoding.UTF8,
						Indent = this.Settings.PrettyPrint,
						IndentChars = this.Settings.Tab,
						NewLineChars = this.Settings.NewLine,
						NewLineHandling = System.Xml.NewLineHandling.None,
						OmitXmlDeclaration = true
					}))
				{
					this.Format(xmlWriter, tokens);
				}
			}

			/// <summary>
			/// Formats the token sequence to the writer
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			public void Format(System.Xml.XmlWriter writer, IEnumerable<Token<MarkupTokenType>> tokens)
			{
				if (writer == null)
				{
					throw new ArgumentNullException("writer");
				}
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				int depth = 0;

				IStream<Token<MarkupTokenType>> stream = Stream<Token<MarkupTokenType>>.Create(tokens);
				Token<MarkupTokenType> token = stream.Peek();
				try
				{
					while (!stream.IsCompleted)
					{
						switch (token.TokenType)
						{
							case MarkupTokenType.ElementBegin:
							{
								depth++;
								writer.WriteStartElement(token.Name.Prefix, token.Name.LocalName, token.Name.NamespaceUri);

								stream.Pop();
								token = stream.Peek();
								break;
							}
							case MarkupTokenType.ElementVoid:
							{
								writer.WriteStartElement(token.Name.Prefix, token.Name.LocalName, token.Name.NamespaceUri);

								stream.Pop();
								token = stream.Peek();

								while (!stream.IsCompleted && token.TokenType == MarkupTokenType.Attribute)
								{
									this.FormatAttribute(writer, stream);
									token = stream.Peek();
								}

								writer.WriteEndElement();
								break;
							}
							case MarkupTokenType.ElementEnd:
							{
								depth--;
								writer.WriteEndElement();

								stream.Pop();
								token = stream.Peek();
								break;
							}
							case MarkupTokenType.Attribute:
							{
								this.FormatAttribute(writer, stream);
								token = stream.Peek();
								break;
							}
							case MarkupTokenType.Primitive:
							{
								ITextFormattable<MarkupTokenType> formattable = token.Value as ITextFormattable<MarkupTokenType>;
								if (formattable != null)
								{
									formattable.Format(this, new XmlWriterAdapter(writer));
								}
								else
								{
									writer.WriteString(token.ValueAsString());
								}

								stream.Pop();
								token = stream.Peek();
								break;
							}
							default:
							{
								throw new TokenException<MarkupTokenType>(
									token,
									String.Format(ErrorUnexpectedToken, token.TokenType));
							}
						}
					}

					while (depth-- > 0)
					{
						// auto close otherwise XmlWriter will choke
						writer.WriteEndElement();
					}
				}
				catch (Exception ex)
				{
					throw new TokenException<MarkupTokenType>(token, ex.Message, ex);
				}
			}

			private void FormatAttribute(System.Xml.XmlWriter writer, IStream<Token<MarkupTokenType>> stream)
			{
				Token<MarkupTokenType> token = stream.Peek();
				writer.WriteStartAttribute(token.Name.Prefix, token.Name.LocalName, token.Name.NamespaceUri);

				stream.Pop();
				token = stream.Peek();

				switch (token.TokenType)
				{
					case MarkupTokenType.Primitive:
					{
						ITextFormattable<MarkupTokenType> formattable = token.Value as ITextFormattable<MarkupTokenType>;
						if (formattable != null)
						{
							formattable.Format(this, new XmlWriterAdapter(writer));
						}
						else
						{
							writer.WriteString(token.ValueAsString());
						}
						break;
					}
					default:
					{
						throw new TokenException<MarkupTokenType>(
							token,
							String.Format(ErrorUnexpectedToken, token));
					}
				}

				stream.Pop();

				writer.WriteEndAttribute();
			}

			#endregion ITextFormatter<MarkupTokenType> Methods
		}
	}
}
