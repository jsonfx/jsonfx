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

using JsonFx.Markup;
using JsonFx.Serialization;

#if SILVERLIGHT
using CanonicalList=System.Collections.Generic.Dictionary<JsonFx.Serialization.DataName, string>;
#else
using CanonicalList=System.Collections.Generic.SortedList<JsonFx.Serialization.DataName, string>;
#endif

namespace JsonFx.Xml
{
	/// <summary>
	/// XML serializer
	/// </summary>
	public partial class XmlReader
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

			private readonly System.Xml.XmlReaderSettings Settings;

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
			public XmlTokenizer(System.Xml.XmlReaderSettings settings)
			{
				if (settings == null)
				{
					this.Settings = new System.Xml.XmlReaderSettings
					{
						CheckCharacters = false,
						ConformanceLevel = System.Xml.ConformanceLevel.Auto
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

				var xmlReader = System.Xml.XmlReader.Create(reader, this.Settings);
#if !SILVERLIGHT
				System.Xml.XmlTextReader xmlTextReader = xmlReader as System.Xml.XmlTextReader;
				if (xmlTextReader != null)
				{
					xmlTextReader.Normalization = false;
					xmlTextReader.WhitespaceHandling = System.Xml.WhitespaceHandling.All;
				}
#endif
				return this.GetTokens(xmlReader);
			}

			/// <summary>
			/// Gets a token sequence from the XmlReader
			/// </summary>
			/// <param name="reader"></param>
			/// <returns></returns>
			public IEnumerable<Token<MarkupTokenType>> GetTokens(System.Xml.XmlReader reader)
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
					catch (System.Xml.XmlException ex)
					{
						throw new DeserializationException(ex.Message, ex.LinePosition, ex.LineNumber, -1, ex);
					}
					catch (Exception ex)
					{
						throw new DeserializationException(ex.Message, -1, ex);
					}

					switch (reader.NodeType)
					{
						case System.Xml.XmlNodeType.Element:
						{
							DataName tagName = new DataName(reader.LocalName, reader.Prefix, reader.NamespaceURI);
							bool isVoidTag = reader.IsEmptyElement;

							IDictionary<DataName, string> attributes;
							if (reader.HasAttributes)
							{
								attributes = new CanonicalList();
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
									yield return MarkupGrammar.TokenPrimitive(attribute.Value);
								}
							}
							break;
						}
						case System.Xml.XmlNodeType.EndElement:
						{
							yield return MarkupGrammar.TokenElementEnd;
							break;
						}
						case System.Xml.XmlNodeType.Attribute:
						{
							yield return MarkupGrammar.TokenAttribute(new DataName(reader.Name, reader.Prefix, reader.NamespaceURI, true));
							yield return MarkupGrammar.TokenPrimitive(reader.Value);
							break;
						}
						case System.Xml.XmlNodeType.Text:
						{
							yield return MarkupGrammar.TokenPrimitive(reader.Value);
							break;
						}
						case System.Xml.XmlNodeType.SignificantWhitespace:
						case System.Xml.XmlNodeType.Whitespace:
						{
							yield return MarkupGrammar.TokenPrimitive(reader.Value);
							break;
						}
						case System.Xml.XmlNodeType.CDATA:
						{
							yield return MarkupGrammar.TokenPrimitive(reader.Value);
							break;
						}
						case System.Xml.XmlNodeType.Entity:
						case System.Xml.XmlNodeType.EntityReference:
						case System.Xml.XmlNodeType.EndEntity:
						{
							break;
						}
						case System.Xml.XmlNodeType.ProcessingInstruction:
						case System.Xml.XmlNodeType.XmlDeclaration:
						{
							yield return MarkupGrammar.TokenUnparsed("?", "?", reader.Name+" "+reader.Value);
							break;
						}
						case System.Xml.XmlNodeType.Comment:
						{
							yield return MarkupGrammar.TokenUnparsed("!--", "--", reader.Value);
							break;
						}
						case System.Xml.XmlNodeType.DocumentType:
						{
							yield return MarkupGrammar.TokenUnparsed("!DOCTYPE ", "", reader.Value);
							break;
						}
						case System.Xml.XmlNodeType.Notation:
						{
							yield return MarkupGrammar.TokenUnparsed("!NOTATION ", "", reader.Value);
							break;
						}
						case System.Xml.XmlNodeType.None:
						{
							((IDisposable)reader).Dispose();
							yield break;
						}
						case System.Xml.XmlNodeType.Document:
						case System.Xml.XmlNodeType.DocumentFragment:
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
}