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
		#region Constants

		private const string ErrorUnexpectedToken = "Unexpected token ({0})";

		#endregion Constants

		/// <summary>
		/// Outputs XML text from a SAX-like input stream of tokens
		/// </summary>
		public class XmlFormatter : ITextFormatter<CommonTokenType>
		{
			#region Fields

			private readonly DataWriterSettings Settings;
			private int depth;
			private bool pendingNewLine;
			private Stack<string> xmlns = new Stack<string>();

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
					this.WriteValue(writer, stream, DataName.Empty);
				}
			}

			/// <summary>
			/// Formats the token sequence to the writer
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			private void WriteValue(TextWriter writer, IStream<Token<CommonTokenType>> tokens, DataName elementName)
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

						// TODO: determine how mixed content (i.e. plain text outside of tag) is represented
						string value = token.ValueAsString();
						if (String.IsNullOrEmpty(value))
						{
							elementName = this.EnsureName(elementName.IsEmpty ? token.Name : elementName, null);
							this.BeginWriteTagOpen(writer, elementName, null, true);
						}
						else
						{
							elementName = this.EnsureName(elementName.IsEmpty ? token.Name : elementName, token.Value.GetType());
							this.BeginWriteTagOpen(writer, elementName, null, false);
							this.WriteCharData(writer, value);
							this.WriteTagClose(writer, elementName);
						}
						break;
					}
					default:
					{
						throw new TokenException<CommonTokenType>(
							token,
							String.Format(ErrorUnexpectedToken, token.TokenType));
					}
				}
			}

			private void WriteArray(TextWriter writer, IStream<Token<CommonTokenType>> tokens, DataName elementName)
			{
				Token<CommonTokenType> token = tokens.Pop();

				// ensure element has a name
				elementName = this.EnsureName(elementName.IsEmpty ? token.Name : elementName, typeof(Array));

				// TODO: figure out a way to surface XmlArrayItemAttribute name
				DataName itemName = DataName.Empty;//new DataName("arrayItem");

				this.BeginWriteTagOpen(writer, elementName, null, false);
				this.pendingNewLine = true;

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

							if (this.pendingNewLine)
							{
								if (this.Settings.PrettyPrint)
								{
									this.depth++;
									this.WriteLine(writer);
								}
								this.pendingNewLine = false;
							}

							this.WriteValue(writer, tokens, itemName);

							this.pendingNewLine = false;
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
							throw new TokenException<CommonTokenType>(
								token,
								String.Format(ErrorUnexpectedToken, token.TokenType));
						}
					}
				}
			}

			private void WriteObject(TextWriter writer, IStream<Token<CommonTokenType>> tokens, DataName elementName)
			{
				Token<CommonTokenType> token = tokens.Pop();

				// ensure element has a name
				elementName = this.EnsureName(elementName.IsEmpty ? token.Name : elementName, typeof(Object));

				bool needsEndTag = true;
				SortedList<DataName, string> attributes = null;

				bool needsValueDelim = false;
				while (!tokens.IsCompleted)
				{
					token = tokens.Peek();
					switch (token.TokenType)
					{
						case CommonTokenType.ObjectEnd:
						{
							tokens.Pop();

							if (needsEndTag)
							{
								needsEndTag = false;
								// write out namespaces and attributes
								this.BeginWriteTagOpen(writer, elementName, attributes, false);
								this.pendingNewLine = true;
							}

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

							if (needsEndTag)
							{
								if (token.Name.IsAttribute)
								{
									if (attributes == null)
									{
										// allocate and sort attributes
										attributes = new SortedList<DataName, string>();
									}
									DataName attrName = token.Name;

									// consume attribute value
									token = tokens.Peek();
									if (token.TokenType != CommonTokenType.Primitive)
									{
										throw new TokenException<CommonTokenType>(token, "Attribute values must be primitive tokens.");
									}
									tokens.Pop();

									if (attrName.IsEmpty)
									{
										attrName = token.Name;
									}

									// according to XML rules cannot duplicate attribute names
									if (!attributes.ContainsKey(attrName))
									{
										attributes.Add(attrName, token.ValueAsString());
									}

									this.pendingNewLine = false;
									needsValueDelim = true;
									break;
								}
								else
								{
									needsEndTag = false;

									// end attributes with first non-attribute child
									// write out namespaces and attributes
									this.BeginWriteTagOpen(writer, elementName, attributes, false);
									this.pendingNewLine = true;
								}
							}

							if (this.pendingNewLine)
							{
								if (this.Settings.PrettyPrint)
								{
									this.depth++;
									this.WriteLine(writer);
								}
								this.pendingNewLine = false;
							}

							this.WriteValue(writer, tokens, token.Name);

							this.pendingNewLine = false;
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
							throw new TokenException<CommonTokenType>(
								token,
								String.Format(ErrorUnexpectedToken, token.TokenType));
						}
					}
				}
			}

			#endregion ITextFormatter<T> Methods

			#region Write Methods

			private void BeginWriteTagOpen(TextWriter writer, DataName elementName, SortedList<DataName, string> attributes, bool isVoidTag)
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
				this.WriteLocalName(writer, elementName.LocalName);

				if ((this.xmlns.Count > 0 && this.xmlns.Peek() != elementName.NamespaceUri) ||
					(this.xmlns.Count < 1 && !String.IsNullOrEmpty(elementName.NamespaceUri)))
				{
					// emit if namespace doesn't match, or has a namespace and none exist
					this.WriteAttribute(writer, null, "xmlns", elementName.NamespaceUri);
				}
				this.xmlns.Push(elementName.NamespaceUri);

				string defaultNS = (this.xmlns.Count > 0) ? this.xmlns.Peek() : String.Empty;
				SortedList<string, string> prefixes = new SortedList<string, string>();

				string prevUri = null;
				if (attributes != null)
				{
					foreach (DataName next in attributes.Keys)
					{
						// NOTE: default namespace does not apply to attributes http://www.w3.org/TR/REC-xml-names/#defaulting
						if (StringComparer.Ordinal.Equals(next.NamespaceUri, prevUri) ||
							(String.IsNullOrEmpty(defaultNS) && String.IsNullOrEmpty(next.NamespaceUri)))
						{
							// dedup xmlns declarations
							continue;
						}

						string prefix = this.GeneratePrefix(next.NamespaceUri, prefixes.Count+1);
						prefixes[next.NamespaceUri] = prefix;
						this.WriteAttribute(writer, "xmlns", prefix, next.NamespaceUri);

						prevUri = next.NamespaceUri;
					}

					foreach (var attr in attributes)
					{
						string prefix;
						if (!prefixes.TryGetValue(attr.Key.NamespaceUri, out prefix))
						{
							prefix = null;
						}

						this.WriteAttribute(writer, prefix, attr.Key.LocalName, attr.Value);
					}

					attributes.Clear();
				}

				if (isVoidTag)
				{
					writer.Write(XmlGrammar.OperatorElementEndEmpty);
				}
				else
				{
					writer.Write(XmlGrammar.OperatorElementEnd);
				}
			}

			private void WriteAttribute(TextWriter writer, string prefix, string name, string value)
			{
				if (String.IsNullOrEmpty(prefix))
				{
					// " "
					writer.Write(XmlGrammar.OperatorValueDelim);
				}
				else
				{
					// " prefix:"
					writer.Write(XmlGrammar.OperatorValueDelim);
					this.WriteLocalName(writer, prefix);
					writer.Write(XmlGrammar.OperatorPrefixDelim);
				}

				// name="value"
				this.WriteLocalName(writer, name);
				writer.Write(XmlGrammar.OperatorPairDelim);
				writer.Write(XmlGrammar.OperatorStringDelim);
				this.WriteAttributeValue(writer, value);
				writer.Write(XmlGrammar.OperatorStringDelim);
			}

			private void WriteTagClose(TextWriter writer, DataName name)
			{
				if (this.xmlns.Count > 0 && this.xmlns.Peek() == name.NamespaceUri)
				{
					this.xmlns.Pop();
				}

				writer.Write(XmlGrammar.OperatorElementBeginClose);
				this.WriteLocalName(writer, name.LocalName);
				writer.Write(XmlGrammar.OperatorElementEnd);
			}

			/// <summary>
			/// Emits a valid XML local-name (i.e. encodes invalid chars including ':')
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="value"></param>
			/// <remarks>
			/// Explicitly escaping ':' to maintain compatibility with XML Namespaces.
			/// From XML 1.0, 5th ed. http://www.w3.org/TR/xml/#sec-common-syn
			///		Name			= NameStartChar (NameChar)*
			///		NameStartChar	= ":"
			///						| [A-Z]
			///						| "_"
			///						| [a-z]
			///						| [#xC0-#xD6]
			///						| [#xD8-#xF6]
			///						| [#xF8-#x2FF]
			///						| [#x370-#x37D]
			///						| [#x37F-#x1FFF]
			///						| [#x200C-#x200D]
			///						| [#x2070-#x218F]
			///						| [#x2C00-#x2FEF]
			///						| [#x3001-#xD7FF]
			///						| [#xF900-#xFDCF]
			///						| [#xFDF0-#xFFFD]
			///						| [#x10000-#xEFFFF]
			///		NameChar		= NameStartChar
			///						| "-"
			///						| "."
			///						| [0-9]
			///						| #xB7
			///						| [#x0300-#x036F]
			///						| [#x203F-#x2040]
			/// </remarks>
			private void WriteLocalName(TextWriter writer, string value)
			{
				int start = 0,
					length = value.Length;

				for (int i=start; i<length; i++)
				{
					char ch = value[i];

					if ((ch >= 'a' && ch <= 'z') ||
						(ch >= 'A' && ch <= 'Z') ||
						(ch == '_') ||
						(ch >= '\u00C0' && ch <= '\u00D6') ||
						(ch >= '\u00D8' && ch <= '\u00F6') ||
						(ch >= '\u00F8' && ch <= '\u02FF') ||
						(ch >= '\u0370' && ch <= '\u037D') ||
						(ch >= '\u037F' && ch <= '\u1FFF') ||
						(ch >= '\u200C' && ch <= '\u200D') ||
						(ch >= '\u2070' && ch <= '\u218F') ||
						(ch >= '\u2C00' && ch <= '\u2FEF') ||
						(ch >= '\u3001' && ch <= '\uD7FF') ||
						(ch >= '\uF900' && ch <= '\uFDCF') ||
						(ch >= '\uFDF0' && ch <= '\uFFFD'))
					{
						// purposefully leaving out ':' to implement namespace prefixes
						// and cannot represent [#x10000-#xEFFFF] as single char so this will incorrectly escape
						continue;
					}

					if ((i > 0) &&
						((ch >= '0' && ch <= '9') ||
						(ch == '-') ||
						(ch == '.') ||
						(ch == '\u00B7') ||
						(ch >= '\u0300' && ch <= '\u036F') ||
						(ch >= '\u203F' && ch <= '\u2040')))
					{
						// these chars are only valid after initial char
						continue;
					}

					if (i > start)
					{
						// copy any leading unescaped chunk
						writer.Write(value.Substring(start, i-start));
					}
					start = i+1;

					// use XmlSerializer-hex-style encoding of UTF-16
					writer.Write("_x");
					writer.Write(Char.ConvertToUtf32(value, i).ToString("X4"));
					writer.Write("_");
				}

				if (length > start)
				{
					// copy any trailing unescaped chunk
					writer.Write(value.Substring(start, length-start));
				}
			}

			/// <summary>
			/// Emits valid XML character data
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="value"></param>
			/// <remarks>
			/// From XML 1.0, 5th ed. http://www.w3.org/TR/xml/#syntax
			///		CharData is defined as all chars except less-than ('&lt;'), ampersand ('&amp;'), the sequence "]]>", optionally encoding greater-than ('>').
			///	
			///	Rather than detect "]]>", this simply encodes all '>'.
			///	From XML 1.0, 5th ed. http://www.w3.org/TR/xml/#sec-line-ends
			///		"the XML processor must behave as if it normalized all line breaks in external parsed entities (including the document entity) on input, before parsing"
			///	Therefore, this encodes all CR ('\r') chars to preserve them in the final output.
			/// </remarks>
			private void WriteCharData(TextWriter writer, string value)
			{
				int start = 0,
					length = value.Length;

				for (int i=start; i<length; i++)
				{
					string entity;
					switch (value[i])
					{
						case '<':
						{
							entity = "&lt;";
							break;
						}
						case '>':
						{
							entity = "&gt;";
							break;
						}
						case '&':
						{
							entity = "&amp;";
							break;
						}
						case '\r':
						{
							// http://www.w3.org/TR/xml/#sec-line-ends
							entity = "&#xD;";
							break;
						}
						default:
						{
							continue;
						}
					}

					if (i > start)
					{
						// copy any leading unescaped chunk
						writer.Write(value.Substring(start, i-start));
					}
					start = i+1;

					// use XML named entity
					writer.Write(entity);
				}

				if (length > start)
				{
					// copy any trailing unescaped chunk
					writer.Write(value.Substring(start, length-start));
				}
			}

			/// <summary>
			/// Emits valid XML attribute character data
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="value"></param>
			/// <remarks>
			/// From XML 1.0, 5th ed. http://www.w3.org/TR/xml/#syntax
			///		CharData is defined as all chars except less-than ('&lt;'), ampersand ('&amp;'), the sequence "]]>", optionally encoding greater-than ('>').
			///		Attributes should additionally encode double-quote ('"') and single-quote ('\'')
			///	Rather than detect "]]>", this simply encodes all '>'.
			/// </remarks>
			private void WriteAttributeValue(TextWriter writer, string value)
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}

				int start = 0,
					length = value.Length;

				for (int i=start; i<length; i++)
				{
					string entity;
					switch (value[i])
					{
						case '<':
						{
							entity = "&lt;";
							break;
						}
						case '>':
						{
							entity = "&gt;";
							break;
						}
						case '&':
						{
							entity = "&amp;";
							break;
						}
						case '"':
						{
							entity = "&quot;";
							break;
						}
						// NOTE: emitting with double-quotes removes need for this escaping
						//case '\'':
						//{
						//    entity = "&apos;";
						//    break;
						//}
						case '\t':
						{
							entity = "&#x9;";
							break;
						}
						case '\r':
						{
							entity = "&#xD;";
							break;
						}
						case '\n':
						{
							entity = "&#xA;";
							break;
						}
						default:
						{
							continue;
						}
					}

					if (i > start)
					{
						// copy any leading unescaped chunk
						writer.Write(value.Substring(start, i-start));
					}
					start = i+1;

					// use XML named entity
					writer.Write(entity);
				}

				if (length > start)
				{
					// copy any trailing unescaped chunk
					writer.Write(value.Substring(start, length-start));
				}
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

			#region Utility Methods

			protected virtual string GeneratePrefix(string namespaceUri, int unique)
			{
				// emit standardized prefixes
				switch (namespaceUri)
				{
					case "http://www.w3.org/XML/1998/namespace":
					{
						// standard for XML
						return "xml";
					}
					case "http://www.w3.org/2001/XMLSchema":
					{
						// standard for XML Schema
						return "xs";
					}
					case "http://www.w3.org/2001/XMLSchema-instance":
					{
						// standard for XML Schema Instance
						return "xsi";
					}
					case "http://www.w3.org/1999/xhtml":
					{
						// standard for XHTML
						return "html";
					}
					case "http://www.w3.org/2005/Atom":
					{
						// standard for Atom 1.0
						return "atom";
					}
					case "http://purl.org/dc/elements/1.1/":
					{
						// standard for Dublin Core
						return "dc";
					}
					case "http://purl.org/syndication/thread/1.0":
					{
						// standard for syndicationthreading
						return "thr";
					}
				}

				// TODO: establish more aesthetically pleasing prefixes
				return String.Concat('q', unique);
			}

			private DataName EnsureName(DataName name, Type type)
			{
				// String.Empty is a valid DataName.LocalName, so must replace
				if (String.IsNullOrEmpty(name.LocalName))
				{
					return this.Settings.Resolver.LoadTypeName(type);
				}

				return name;
			}

			#endregion Utility Methods
		}
	}
}
