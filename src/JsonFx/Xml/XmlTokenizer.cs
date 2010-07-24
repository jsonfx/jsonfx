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
using System.Xml;

using JsonFx.Markup;
using JsonFx.Serialization;

namespace JsonFx.Xml
{
	/// <summary>
	/// Generates a sequence of tokens from XML text
	/// </summary>
	/// <remarks>
	/// Implemented as an Adapter between <see cref="ITextTokenizer<XmlTokenType>"/> and <see cref="System.Xml.XmlReader"/>
	/// </remarks>
	public class XmlTokenizer : ITextTokenizer<MarkupTokenType>
	{
		#region Fields

		private readonly XmlReaderSettings Settings;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public XmlTokenizer()
			: this(null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public XmlTokenizer(XmlReaderSettings settings)
		{
			if (settings == null)
			{
				this.Settings = new XmlReaderSettings
				{
					ConformanceLevel = ConformanceLevel.Auto
				};
			}
			else
			{
				this.Settings = settings;
			}
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the total number of characters read from the input
		/// </summary>
		public int Column
		{
			get { return 0; }
		}

		/// <summary>
		/// Gets the total number of lines read from the input
		/// </summary>
		public int Line
		{
			get { return 0; }
		}

		/// <summary>
		/// Gets the current position within the input
		/// </summary>
		public long Index
		{
			get { return -1L; }
		}

		#endregion Properties

		#region ITextTokenizer<DataTokenType> Members

		/// <summary>
		/// Gets a token sequence from the string
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public IEnumerable<Token<MarkupTokenType>> GetTokens(string text)
		{
			return this.GetTokens(new StringReader(text ?? String.Empty));
		}

		/// <summary>
		/// Gets a token sequence from the TextReader
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public IEnumerable<Token<MarkupTokenType>> GetTokens(TextReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}

			var xmlReader = XmlReader.Create(reader, this.Settings);
			XmlTextReader xmlTextReader = xmlReader as XmlTextReader;
			if (xmlTextReader != null)
			{
				xmlTextReader.Normalization = false;
				xmlTextReader.WhitespaceHandling = WhitespaceHandling.All;
			}
			return this.GetTokens(xmlReader);
		}

		/// <summary>
		/// Gets a token sequence from the XmlReader
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public IEnumerable<Token<MarkupTokenType>> GetTokens(XmlReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}

			while (true)
			{
				// have to isolate try-catch away from yields
				try
				{
					if (!reader.Read())
					{
						((IDisposable)reader).Dispose();
						break;
					}
				}
				catch (XmlException ex)
				{
					throw new DeserializationException(ex.Message, ex.LinePosition, ex.LineNumber, -1, ex);
				}
				catch (Exception ex)
				{
					throw new DeserializationException(ex.Message, -1, ex);
				}

				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
					{
						DataName tagName = new DataName(reader.LocalName, reader.Prefix, reader.NamespaceURI);
						bool isVoidTag = reader.IsEmptyElement;

						SortedList<DataName, string> attributes;
						if (reader.HasAttributes)
						{
							attributes = new SortedList<DataName, string>();
							while (reader.MoveToNextAttribute())
							{
								if (String.IsNullOrEmpty(reader.Prefix) && reader.LocalName == "xmlns" ||
									reader.Prefix == "xmlns")
								{
									continue;
								}

								attributes[new DataName(reader.LocalName, reader.Prefix, reader.NamespaceURI)] = reader.Value;
							}
						}
						else
						{
							attributes = null;
						}

						if (isVoidTag)
						{
							yield return MarkupGrammar.TokenElementVoid(tagName);
						}
						else
						{
							yield return MarkupGrammar.TokenElementBegin(tagName);
						}

						if (attributes != null)
						{
							foreach (var attribute in attributes)
							{
								yield return MarkupGrammar.TokenAttribute(attribute.Key);
								yield return MarkupGrammar.TokenText(attribute.Value);
							}
						}
						break;
					}
					case XmlNodeType.EndElement:
					{
						yield return MarkupGrammar.TokenElementEnd(new DataName(reader.LocalName, reader.Prefix, reader.NamespaceURI));
						break;
					}
					case XmlNodeType.Attribute:
					{
						yield return MarkupGrammar.TokenAttribute(new DataName(reader.Name, reader.Prefix, reader.NamespaceURI, true));
						yield return MarkupGrammar.TokenText(reader.Value);
						break;
					}
					case XmlNodeType.Text:
					{
						yield return MarkupGrammar.TokenText(reader.Value);
						break;
					}
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Whitespace:
					{
						yield return MarkupGrammar.TokenWhitespace(reader.Value);
						break;
					}
					case XmlNodeType.CDATA:
					{
						yield return MarkupGrammar.TokenText(reader.Value);
						break;
					}
					case XmlNodeType.Entity:
					case XmlNodeType.EntityReference:
					case XmlNodeType.EndEntity:
					{
						break;
					}
					case XmlNodeType.ProcessingInstruction:
					case XmlNodeType.XmlDeclaration:
					{
						yield return MarkupGrammar.TokenUnparsed("?{0}?", reader.Name+" "+reader.Value);
						break;
					}
					case XmlNodeType.Comment:
					{
						yield return MarkupGrammar.TokenUnparsed("!--{0}--", reader.Value);
						break;
					}
					case XmlNodeType.DocumentType:
					{
						yield return MarkupGrammar.TokenUnparsed("!DOCTYPE {0}", reader.Value);
						break;
					}
					case XmlNodeType.Notation:
					{
						yield return MarkupGrammar.TokenUnparsed("!NOTATION {0}", reader.Value);
						break;
					}
					case XmlNodeType.None:
					{
						((IDisposable)reader).Dispose();
						yield break;
					}
					case XmlNodeType.Document:
					case XmlNodeType.DocumentFragment:
					default:
					{
						continue;
					}
				}
			};
		}

		#endregion ITextTokenizer<DataTokenType> Members

		#region IDisposable Members

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				//((IDisposable)this.Scanner).Dispose();
			}
		}

		#endregion IDisposable Members
	}
}
