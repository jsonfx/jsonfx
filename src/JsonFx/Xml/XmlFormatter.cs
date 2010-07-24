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
					this.Format(writer, tokens);

					return writer.GetStringBuilder().ToString();
				}
			}

			/// <summary>
			/// Formats the token sequence to the writer
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			public void Format(TextWriter writer, IEnumerable<Token<MarkupTokenType>> tokens)
			{
				if (writer == null)
				{
					throw new ArgumentNullException("writer");
				}

				using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(
					writer,
					new XmlWriterSettings
					{
						ConformanceLevel = System.Xml.ConformanceLevel.Auto,
						Encoding = Encoding.UTF8,
						Indent = this.Settings.PrettyPrint,
						IndentChars = this.Settings.Tab,
						NewLineChars = this.Settings.NewLine,
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

				IStream<Token<MarkupTokenType>> stream = new Stream<Token<MarkupTokenType>>(tokens);
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
								writer.WriteStartAttribute(token.Name.Prefix, token.Name.LocalName, token.Name.NamespaceUri);

								stream.Pop();
								token = stream.Peek();

								switch (token.TokenType)
								{
									case MarkupTokenType.UnparsedBlock:
									{
										string format = token.Name.LocalName;
										if (String.IsNullOrEmpty(format))
										{
											writer.WriteRaw(token.ValueAsString());
										}
										else
										{
											writer.WriteRaw(Char.ToString(MarkupGrammar.OperatorElementBegin));
											writer.WriteRaw(String.Format(format, token.ValueAsString()));
											writer.WriteRaw(Char.ToString(MarkupGrammar.OperatorElementEnd));
										}
										break;
									}
									case MarkupTokenType.TextValue:
									case MarkupTokenType.Whitespace:
									{
										writer.WriteString(token.ValueAsString());
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
								token = stream.Peek();

								writer.WriteEndAttribute();
								break;
							}
							case MarkupTokenType.TextValue:
							case MarkupTokenType.Whitespace:
							{
								writer.WriteString(token.ValueAsString());

								stream.Pop();
								token = stream.Peek();
								break;
							}
							case MarkupTokenType.UnparsedBlock:
							{
								string format = token.Name.LocalName;
								if (String.IsNullOrEmpty(format))
								{
									writer.WriteRaw(token.ValueAsString());
								}
								else
								{
									writer.WriteRaw(Char.ToString(MarkupGrammar.OperatorElementBegin));
									writer.WriteRaw(String.Format(format, token.ValueAsString()));
									writer.WriteRaw(Char.ToString(MarkupGrammar.OperatorElementEnd));
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

			#endregion ITextFormatter<MarkupTokenType> Methods
		}
	}
}
