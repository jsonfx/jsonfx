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

using JsonFx.IO;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Xml.Stax
{
	/// <summary>
	/// Outputs XML text from a StAX-like input stream of tokens
	/// </summary>
	public class StaxFormatter : ITextFormatter<StaxTokenType>
	{
		#region Constants

		private const string ErrorUnexpectedToken = "Unexpected token ({0})";

		#endregion Constants

		#region Fields

		private readonly DataWriterSettings Settings;
		private readonly PrefixScopeChain ScopeChain = new PrefixScopeChain();
		private int nsCounter;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public StaxFormatter(DataWriterSettings settings)
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
		public string Format(IEnumerable<Token<StaxTokenType>> tokens)
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
		public void Format(TextWriter writer, IEnumerable<Token<StaxTokenType>> tokens)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException("tokens");
			}

			IStream<Token<StaxTokenType>> stream = new Stream<Token<StaxTokenType>>(tokens);

			PrefixScopeChain.Scope scope = null;
			while (!stream.IsCompleted)
			{
				Token<StaxTokenType> token = stream.Peek();
				switch (token.TokenType)
				{
					case StaxTokenType.PrefixBegin:
					{
						do
						{
							scope[token.Name.LocalName] = token.Name.NamespaceUri;

							stream.Pop();
							token = stream.Peek();
						} while (!stream.IsCompleted && token.TokenType == StaxTokenType.PrefixBegin);
						break;
					}
					case StaxTokenType.ElementBegin:
					{
						if (scope == null)
						{
							scope = new PrefixScopeChain.Scope();
						}
						scope.TagName = token.Name;
						this.ScopeChain.Push(scope);

						stream.Pop();
						token = stream.Peek();

						SortedList<DataName, Token<StaxTokenType>> attributes = null;
						while (!stream.IsCompleted && token.TokenType == StaxTokenType.Attribute)
						{
							if (attributes == null)
							{
								attributes = new SortedList<DataName, Token<StaxTokenType>>();
							}
							DataName attrName = token.Name;

							stream.Pop();
							token = stream.Peek();

							attributes[attrName] = token;

							stream.Pop();
							token = stream.Peek();
						}

						this.WriteTag(writer, StaxTagType.BeginTag, scope.TagName, attributes, scope);

						scope = null;
						break;
					}
					case StaxTokenType.ElementEnd:
					{
						this.WriteTag(writer, StaxTagType.EndTag, token.Name, null, null);

						if (this.ScopeChain.HasScope &&
							this.ScopeChain.Peek().TagName == token.Name)
						{
							this.ScopeChain.Pop();
						}
						else
						{
							// TODO: decide how to manage this
						}

						stream.Pop();
						token = stream.Peek();
						break;
					}
					case StaxTokenType.TextValue:
					case StaxTokenType.Whitespace:
					{
						this.WriteCharData(writer, token.ValueAsString());

						stream.Pop();
						token = stream.Peek();
						break;
					}
					case StaxTokenType.UnparsedBlock:
					{
						this.WriteUnparsedBlock(writer, token.Name.LocalName, token.ValueAsString());

						stream.Pop();
						token = stream.Peek();
						break;
					}
					case StaxTokenType.PrefixEnd:
					{
						stream.Pop();
						token = stream.Peek();

						// TODO: figure out if we really need to do anything
						break;
					}
					default:
					{
						throw new TokenException<StaxTokenType>(
							token,
							String.Format(ErrorUnexpectedToken, token.TokenType));
					}
				}
			}
		}

		#endregion ITextFormatter<T> Methods

		#region Write Methods

		private void WriteTag(
			TextWriter writer,
			StaxTagType type,
			DataName tagName,
			IEnumerable<KeyValuePair<DataName, Token<StaxTokenType>>> attributes,
			IEnumerable<KeyValuePair<string, string>> prefixDeclarations)
		{
			string tagPrefix = this.ResolvePrefix(tagName.NamespaceUri);

			// "<"
			writer.Write(StaxGrammar.OperatorElementBegin);
			if (type == StaxTagType.EndTag)
			{
				// "/"
				writer.Write(StaxGrammar.OperatorElementClose);
			}

			if (!String.IsNullOrEmpty(tagPrefix))
			{
				// "prefix:"
				this.WriteLocalName(writer, tagPrefix);
				writer.Write(StaxGrammar.OperatorPrefixDelim);
			}
			// "local-name"
			this.WriteLocalName(writer, tagName.LocalName);

			// emit all namespaces first, sorted by prefix
			if (prefixDeclarations != null)
			{
				foreach (var declaration in prefixDeclarations)
				{
					this.WriteXmlns(writer, declaration.Key, declaration.Value);
				}
			}

			if (attributes != null)
			{
				foreach (var attribute in attributes)
				{
					string attrPrefix = this.ResolvePrefix(attribute.Key.NamespaceUri);

					this.WriteAttribute(writer, attrPrefix, attribute.Key.LocalName, attribute.Value);
				}
			}

			if (type == StaxTagType.VoidTag)
			{
				// "/"
				writer.Write(StaxGrammar.OperatorElementClose);
			}
			// ">"
			writer.Write(StaxGrammar.OperatorElementEnd);
		}

		private string ResolvePrefix(string namespaceUri)
		{
			string prefix = this.ScopeChain.GetPrefix(namespaceUri, false);
			if (prefix == null && !String.IsNullOrEmpty(namespaceUri))
			{
				prefix = this.GeneratePrefix(namespaceUri);
			}

			return prefix;
		}

		private void WriteXmlns(TextWriter writer, string prefix, string namespaceUri)
		{
			// " xmlns"
			writer.Write(StaxGrammar.OperatorValueDelim);
			this.WriteLocalName(writer, "xmlns");

			if (!String.IsNullOrEmpty(prefix))
			{
				// ":prefix"
				writer.Write(StaxGrammar.OperatorPrefixDelim);
				this.WriteLocalName(writer, prefix);
			}

			// ="value"
			writer.Write(StaxGrammar.OperatorPairDelim);
			writer.Write(StaxGrammar.OperatorStringDelim);
			this.WriteAttributeValue(writer, namespaceUri);
			writer.Write(StaxGrammar.OperatorStringDelim);
		}

		private void WriteAttribute(TextWriter writer, string prefix, string localName, Token<StaxTokenType> value)
		{
			// " "
			writer.Write(StaxGrammar.OperatorValueDelim);

			if (!String.IsNullOrEmpty(prefix))
			{
				// "prefix:"
				this.WriteLocalName(writer, prefix);
				writer.Write(StaxGrammar.OperatorPrefixDelim);
			}

			// name="value"
			this.WriteLocalName(writer, localName);
			writer.Write(StaxGrammar.OperatorPairDelim);
			writer.Write(StaxGrammar.OperatorStringDelim);

			switch (value.TokenType)
			{
				case StaxTokenType.TextValue:
				case StaxTokenType.Whitespace:
				{
					this.WriteAttributeValue(writer, value.ValueAsString());
					break;
				}
				case StaxTokenType.UnparsedBlock:
				{
					this.WriteUnparsedBlock(writer, value.Name.LocalName, value.ValueAsString());
					break;
				}
				default:
				{
					throw new TokenException<StaxTokenType>(
						value,
						String.Format(StaxFormatter.ErrorUnexpectedToken, value));
				}
			}

			writer.Write(StaxGrammar.OperatorStringDelim);
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
					//case '\r':
					//{
					//    // http://www.w3.org/TR/xml/#sec-line-ends
					//    entity = "&#xD;";
					//    break;
					//}
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

		private void WriteUnparsedBlock(TextWriter writer, string name, string value)
		{
			if (name == null)
			{
				writer.Write(value);
			}

			writer.Write(StaxGrammar.OperatorElementBegin);
			writer.Write(name, value);
			writer.Write(StaxGrammar.OperatorElementEnd);
		}

		#endregion Write Methods

		#region Utility Methods

		protected virtual string GeneratePrefix(string namespaceUri)
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
			return String.Concat('q', ++nsCounter);
		}

		#endregion Utility Methods
	}
}
