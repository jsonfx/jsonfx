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

using JsonFx.IO;
using JsonFx.Markup;
using JsonFx.Serialization;

#if SILVERLIGHT
using CanonicalList=System.Collections.Generic.Dictionary<JsonFx.Serialization.DataName, JsonFx.Serialization.Token<JsonFx.Markup.MarkupTokenType>>;
#else
using CanonicalList=System.Collections.Generic.SortedList<JsonFx.Serialization.DataName, JsonFx.Serialization.Token<JsonFx.Markup.MarkupTokenType>>;
#endif

namespace JsonFx.Html
{
	/// <summary>
	/// Outputs markup text from an input stream of tokens
	/// </summary>
	public class HtmlFormatter : ITextFormatter<MarkupTokenType>
	{
		#region EmptyAttributeType

		public enum EmptyAttributeType
		{
			/// <summary>
			/// HTML-style empty attributes do not emit a quoted string
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/html5/syntax.html#attributes-0
			/// </remarks>
			Html,

			/// <summary>
			/// XHTML-style empty attributes repeat the attribute name as its value
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/xhtml-media-types/#C_10
			/// http://www.w3.org/TR/xhtml1/#C_10
			/// http://www.w3.org/TR/html5/the-xhtml-syntax.html
			/// </remarks>
			Xhtml,

			/// <summary>
			/// XML-style empty attributes emit an empty quoted string
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/xml/#sec-starttags
			/// </remarks>
			Xml
		}

		#endregion EmptyAttributeType

		#region HtmlTaxonomy

		/// <summary>
		/// Defines a general taxonomy of tags
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html#sec_5.2.
		/// </remarks>
		[Flags]
		public enum HtmlTaxonomy
		{
			/// <summary>
			/// Plain text, no tag
			/// </summary>
			None = 0x0000,

			/// <summary>
			/// HTML comments
			/// </summary>
			Comment = 0x0001,

			/// <summary>
			/// textual elements
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/html401/struct/text.html
			/// </remarks>
			Text = 0x0002,

			/// <summary>
			/// character level elements and text strings
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/html401/struct/text.html
			/// </remarks>
			Inline = 0x0004,

			/// <summary>
			/// block-like elements
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/html401/struct/text.html
			/// </remarks>
			Block = 0x0008,

			/// <summary>
			/// list elements
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/html401/struct/lists.html
			/// </remarks>
			List = 0x0010,

			/// <summary>
			/// tabular elements
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/html401/struct/tables.html
			/// </remarks>
			Table = 0x0020,

			/// <summary>
			/// style elements
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/html401/present/styles.html
			/// http://www.w3.org/TR/html401/present/graphics.html
			/// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html#s_presentationmodule
			/// </remarks>
			Style = 0x0040,

			/// <summary>
			/// form elements
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html#s_forms
			/// </remarks>
			Form = 0x0080,

			/// <summary>
			/// script elements
			/// </summary>
			Script = 0x0100,

			/// <summary>
			/// embedded elements
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/html401/struct/objects.html
			/// </remarks>
			Embeded = 0x0200,

			/// <summary>
			/// document elements
			/// </summary>
			/// <remarks>
			/// http://www.w3.org/TR/html401/struct/global.html
			/// </remarks>
			Document = 0x0400,

			/// <summary>
			/// unknown elements
			/// </summary>
			Unknown = 0x8000
		}

		#endregion HtmlTaxonomy

		#region Constants

		private const string ErrorUnexpectedToken = "Unexpected token ({0})";

		#endregion Constants

		#region Fields

		private readonly DataWriterSettings Settings;
		private readonly PrefixScopeChain ScopeChain = new PrefixScopeChain();

		public bool canonicalForm;
		private EmptyAttributeType emptyAttributes;
		private bool encodeNonAscii;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public HtmlFormatter(DataWriterSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			this.Settings = settings;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets a value indicating if should emit canonical form
		/// </summary>
		/// <remarks>
		/// http://www.w3.org/TR/xml-c14n
		/// </remarks>
		public bool CanonicalForm
		{
			get { return this.canonicalForm; }
			set { this.canonicalForm = value; }
		}

		/// <summary>
		/// Gets and sets a value indicating how should emit empty attributes
		/// </summary>
		public EmptyAttributeType EmptyAttributes
		{
			get { return this.emptyAttributes; }
			set { this.emptyAttributes = value; }
		}

		/// <summary>
		/// Gets and sets a value indicating if should encode text chars above the ASCII range
		/// </summary>
		/// <remarks>
		/// This option can help when the output is being embedded within an unknown encoding
		/// </remarks>
		public bool EncodeNonAscii
		{
			get { return this.encodeNonAscii; }
			set { this.encodeNonAscii = value; }
		}

		#endregion Properties

		#region HTML Tag Methods

		/// <summary>
		/// Determines if is full (i.e. empty) tag
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns>if is a full tag</returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/elements.html
		/// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html#sec_5.2.
		/// http://www.w3.org/TR/WD-html40-970917/index/elements.html
		/// </remarks>
		private static bool FullTagRequired(string tag)
		{
			switch (tag)
			{
				case "area":
				case "base":
				case "basefont":
				case "br":
				case "col":
				case "frame":
				case "hr":
				case "img":
				case "input":
				case "isindex":
				case "link":
				case "meta":
				case "param":
				case "wbr":
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines if the tag is required to be closed (in HTML 4.01)
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/elements.html
		/// http://www.w3.org/TR/WD-html40-970917/index/elements.html
		/// </remarks>
		private static bool CloseTagRequired(string tag)
		{
			switch (tag)
			{
				case "body":
				case "colgroup":
				case "dd":
				case "dt":
				case "embed":
				case "head":
				case "html":
				case "li":
				case "option":
				case "p":
				case "tbody":
				case "td":
				case "tfoot":
				case "th":
				case "thead":
				case "tr":
				case "!--":
				case "![CDATA[":
				case "!":
				case "?":
				case "%":
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Categorizes the tag for heuristics about markup type
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns>the box type for a particular element</returns>
		private static HtmlTaxonomy GetTaxonomy(string tag)
		{
			// http://www.w3.org/TR/html401/
			// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html
			// http://www.w3.org/html/wg/html5/#elements0
			// non-standard: http://www.mountaindragon.com/html/text.htm
			switch (tag)
			{
				case "!--":
				{
					return HtmlTaxonomy.Comment;
				}

				case "a":
				case "abbr":
				case "acronym":
				case "address":
				case "area":
				case "bdo":
				case "cite":
				case "code":
				case "dfn":
				case "em":
				case "img":
				case "isindex":
				case "kbd":
				case "label":
				case "legend":
				case "map":
				case "q":
				case "samp":
				case "span":
				case "strong":
				case "var":
				case "wbr":
				{
					return HtmlTaxonomy.Text|HtmlTaxonomy.Inline;
				}

				case "b":
				case "big":
				case "blink":
				case "font":
				case "i":
				case "marquee":
				case "s":
				case "small":
				case "strike":
				case "sub":
				case "sup":
				case "tt":
				case "u":
				{
					return HtmlTaxonomy.Text|HtmlTaxonomy.Style|HtmlTaxonomy.Inline;
				}

				case "blockquote":
				case "bq":
				case "br":
				case "center":
				case "del":
				case "div":
				case "fieldset":
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
				case "hr":
				case "ins":
				case "nobr":
				case "p":
				case "pre":
				{
					return HtmlTaxonomy.Text|HtmlTaxonomy.Block;
				}

				case "dl":
				case "dd":
				case "dir":
				case "dt":
				case "lh":
				case "li":
				case "menu":
				case "ol":
				case "ul":
				{
					return HtmlTaxonomy.List;
				}

				case "table":
				case "tbody":
				case "td":
				case "th":
				case "thead":
				case "tfoot":
				case "tr":
				case "caption":
				case "col":
				case "colgroup":
				{
					return HtmlTaxonomy.Table;
				}

				case "button":
				case "form":
				case "input":
				case "optgroup":
				case "option":
				case "select":
				case "textarea":
				{
					return HtmlTaxonomy.Form;
				}

				case "applet":
				case "bgsound":
				case "embed":
				case "noembed":
				case "object":
				case "param":
				case "sound":
				{
					return HtmlTaxonomy.Embeded;
				}

				case "basefont":
				case "style":
				{
					return HtmlTaxonomy.Style|HtmlTaxonomy.Document;
				}

				case "%":
				case "noscript":
				case "script":
				{
					return HtmlTaxonomy.Script|HtmlTaxonomy.Document;
				}

				case "!":
				case "?":
				case "![cdata[":
				case "base":
				case "body":
				case "head":
				case "html":
				case "frameset":
				case "frame":
				case "iframe":
				case "link":
				case "meta":
				case "noframes":
				case "title":
				{
					return HtmlTaxonomy.Document;
				}
			}
			return HtmlTaxonomy.Unknown;
		}

		#endregion HTML Tag Methods

		#region ITextFormatter<T> Methods

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
			if (tokens == null)
			{
				throw new ArgumentNullException("tokens");
			}

			IStream<Token<MarkupTokenType>> stream = Stream<Token<MarkupTokenType>>.Create(tokens);

			PrefixScopeChain.Scope scope = null;
			while (!stream.IsCompleted)
			{
				Token<MarkupTokenType> token = stream.Peek();
				switch (token.TokenType)
				{
					case MarkupTokenType.ElementBegin:
					case MarkupTokenType.ElementVoid:
					{
						DataName tagName = token.Name;
						MarkupTokenType tagType = token.TokenType;

						stream.Pop();
						token = stream.Peek();

						scope = new PrefixScopeChain.Scope();

						if (this.ScopeChain.ContainsNamespace(tagName.NamespaceUri) ||
							(String.IsNullOrEmpty(tagName.NamespaceUri) && !this.ScopeChain.ContainsPrefix(String.Empty)))
						{
							string prefix = this.ScopeChain.GetPrefix(tagName.NamespaceUri, false);
							scope.TagName = new DataName(tagName.LocalName, prefix, tagName.NamespaceUri);
						}
						else
						{
							scope[tagName.Prefix] = tagName.NamespaceUri;
							scope.TagName = tagName;
						}

						this.ScopeChain.Push(scope);

						IDictionary<DataName, Token<MarkupTokenType>> attributes = null;
						while (!stream.IsCompleted && token.TokenType == MarkupTokenType.Attribute)
						{
							if (attributes == null)
							{
								attributes = this.canonicalForm ?
									(IDictionary<DataName, Token<MarkupTokenType>>)new CanonicalList() :
									(IDictionary<DataName, Token<MarkupTokenType>>)new Dictionary<DataName, Token<MarkupTokenType>>();
							}
							DataName attrName = token.Name;

							string prefix = this.ScopeChain.EnsurePrefix(attrName.Prefix, attrName.NamespaceUri);
							if (prefix != attrName.Prefix)
							{
								attrName = new DataName(attrName.LocalName, prefix, attrName.NamespaceUri, true);
							}

							if (!this.ScopeChain.ContainsNamespace(attrName.NamespaceUri) &&
								(!String.IsNullOrEmpty(attrName.NamespaceUri) || this.ScopeChain.ContainsPrefix(String.Empty)))
							{
								scope[prefix] = attrName.NamespaceUri;
							}

							stream.Pop();
							token = stream.Peek();

							attributes[attrName] = token;

							stream.Pop();
							token = stream.Peek();
						}

						this.WriteTag(writer, tagType, tagName, attributes, scope);

						scope = null;
						break;
					}
					case MarkupTokenType.ElementEnd:
					{
						if (this.ScopeChain.HasScope)
						{
							this.WriteTag(writer, MarkupTokenType.ElementEnd, this.ScopeChain.Peek().TagName, null, null);
							this.ScopeChain.Pop();
						}
						else
						{
							// TODO: decide if this is should throw an exception
						}

						stream.Pop();
						token = stream.Peek();
						break;
					}
					case MarkupTokenType.Primitive:
					{
						IMarkupFormattable formattable = token.Value as IMarkupFormattable;
						if (formattable != null)
						{
							formattable.Format(this, writer);
						}
						else
						{
							this.WriteCharData(writer, token.ValueAsString());
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
		}

		#endregion ITextFormatter<T> Methods

		#region Write Methods

		private void WriteTag(
			TextWriter writer,
			MarkupTokenType type,
			DataName tagName,
			IEnumerable<KeyValuePair<DataName, Token<MarkupTokenType>>> attributes,
			IEnumerable<KeyValuePair<string, string>> prefixDeclarations)
		{
			string tagPrefix = this.ScopeChain.EnsurePrefix(tagName.Prefix, tagName.NamespaceUri);

			// "<"
			writer.Write(MarkupGrammar.OperatorElementBegin);
			if (type == MarkupTokenType.ElementEnd)
			{
				// "/"
				writer.Write(MarkupGrammar.OperatorElementClose);
			}

			if (!String.IsNullOrEmpty(tagPrefix))
			{
				// "prefix:"
				this.WriteLocalName(writer, tagPrefix);
				writer.Write(MarkupGrammar.OperatorPrefixDelim);
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
					// Not sure if this is correct: http://stackoverflow.com/questions/3312390
					// "The namespace name for an unprefixed attribute name always has no value"
					// "The attribute value in a default namespace declaration MAY be empty.
					// This has the same effect, within the scope of the declaration, of there being no default namespace."
					// http://www.w3.org/TR/xml-names/#defaulting
					string attrPrefix = this.ScopeChain.EnsurePrefix(attribute.Key.Prefix, attribute.Key.NamespaceUri);

					this.WriteAttribute(writer, attrPrefix, attribute.Key.LocalName, attribute.Value);
				}
			}

			if (!this.canonicalForm &&
				type == MarkupTokenType.ElementVoid)
			{
				// " /"
				writer.Write(MarkupGrammar.OperatorValueDelim);
				writer.Write(MarkupGrammar.OperatorElementClose);
			}
			// ">"
			writer.Write(MarkupGrammar.OperatorElementEnd);

			if (this.canonicalForm &&
				type == MarkupTokenType.ElementVoid)
			{
				// http://www.w3.org/TR/xml-c14n#Terminology
				this.WriteTag(writer, MarkupTokenType.ElementEnd, tagName, null, null);
			}
		}

		private void WriteXmlns(TextWriter writer, string prefix, string namespaceUri)
		{
			// " xmlns"
			writer.Write(MarkupGrammar.OperatorValueDelim);
			this.WriteLocalName(writer, "xmlns");

			if (!String.IsNullOrEmpty(prefix))
			{
				// ":prefix"
				writer.Write(MarkupGrammar.OperatorPrefixDelim);
				this.WriteLocalName(writer, prefix);
			}

			// ="value"
			writer.Write(MarkupGrammar.OperatorPairDelim);
			writer.Write(MarkupGrammar.OperatorStringDelim);
			this.WriteAttributeValue(writer, namespaceUri);
			writer.Write(MarkupGrammar.OperatorStringDelim);
		}

		private void WriteAttribute(TextWriter writer, string prefix, string localName, Token<MarkupTokenType> value)
		{
			// " "
			writer.Write(MarkupGrammar.OperatorValueDelim);

			if (!String.IsNullOrEmpty(prefix))
			{
				// "prefix:"
				this.WriteLocalName(writer, prefix);
				writer.Write(MarkupGrammar.OperatorPrefixDelim);
			}

			// local-name
			this.WriteLocalName(writer, localName);

			IMarkupFormattable formattable = value.Value as IMarkupFormattable;
			string attrValue = (formattable == null) ? value.ValueAsString() : null;

			if ((formattable == null) &&
				String.IsNullOrEmpty(attrValue))
			{
				switch (this.EmptyAttributes)
				{
					case EmptyAttributeType.Html:
					{
						return;
					}
					case EmptyAttributeType.Xhtml:
					{
						attrValue = localName;
						break;
					}
					case EmptyAttributeType.Xml:
					{
						break;
					}
				}
			}

			// ="value"
			writer.Write(MarkupGrammar.OperatorPairDelim);
			writer.Write(MarkupGrammar.OperatorStringDelim);

			switch (value.TokenType)
			{
				case MarkupTokenType.Primitive:
				{
					if (formattable != null)
					{
						formattable.Format(this, writer);
					}
					else
					{
						this.WriteAttributeValue(writer, attrValue);
					}
					break;
				}
				default:
				{
					throw new TokenException<MarkupTokenType>(
						value,
						String.Format(HtmlFormatter.ErrorUnexpectedToken, value));
				}
			}

			writer.Write(MarkupGrammar.OperatorStringDelim);
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
				writer.Write(ConvertToUtf32(value, i).ToString("X4"));
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
				char ch = value[i];

				string entity;
				switch (ch)
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
						if (!this.canonicalForm)
						{
							continue;
						}

						// Line breaks normalized to '\n'
						// http://www.w3.org/TR/xml-c14n#Terminology
						entity = String.Empty;
						break;
					}
					default:
					{
						if (((ch < ' ') && (ch != '\n') && (ch != '\t')) ||
							(this.encodeNonAscii && (ch >= 0x7F)) ||
							((ch >= 0x7F) && (ch <= 0x84)) ||
							((ch >= 0x86) && (ch <= 0x9F)) ||
							((ch >= 0xFDD0) && (ch <= 0xFDEF)))
						{
							// encode all control chars except CRLF/Tab: http://www.w3.org/TR/xml/#charsets
							int utf16 = ConvertToUtf32(value, i);
							entity = String.Concat("&#x", utf16.ToString("X", CultureInfo.InvariantCulture), ';');
							break;
						}
						continue;
					}
				}

				if (i > start)
				{
					// copy any leading unescaped chunk
					writer.Write(value.Substring(start, i-start));
				}
				start = i+1;

				// emit XML entity
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
				char ch = value[i];

				string entity;
				switch (ch)
				{
					case '<':
					{
						entity = "&lt;";
						break;
					}
					case '>':
					{
						if (this.canonicalForm)
						{
							// http://www.w3.org/TR/xml-c14n#ProcessingModel
							continue;
						}

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
					case '\'':
					{
						if (!this.canonicalForm)
						{
							continue;
						}

						// http://www.w3.org/TR/xml-c14n#ProcessingModel
					    entity = "&apos;";
					    break;
					}
					default:
					{
						if ((ch < ' ') ||
							(this.encodeNonAscii && (ch >= 0x7F)) ||
							((ch >= 0x7F) && (ch <= 0x84)) ||
							((ch >= 0x86) && (ch <= 0x9F)) ||
							((ch >= 0xFDD0) && (ch <= 0xFDEF)))
						{
							// encode all control chars: http://www.w3.org/TR/xml/#charsets
							int utf16 = ConvertToUtf32(value, i);
							entity = String.Concat("&#x", utf16.ToString("X", CultureInfo.InvariantCulture), ';');
							break;
						}
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

		#endregion Write Methods

		#region Utility Methods

		private static int ConvertToUtf32(string value, int i)
		{
#if SILVERLIGHT
			return (int)value[i];
#else
			return Char.ConvertToUtf32(value, i);
#endif
		}

		#endregion Utility Methods
	}
}
