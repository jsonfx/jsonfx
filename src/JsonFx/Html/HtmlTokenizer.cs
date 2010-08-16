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

namespace JsonFx.Html
{
	/// <summary>
	/// Generates a sequence of tokens from a generalized model of markup text (e.g. HTML, XML, JBST, ASPX/ASCX, ASP, JSP, PHP, etc.)
	/// </summary>
	/// <remarks>
	/// This generates a stream of tokens like StAX (Streaming API for XML)
	/// Unlike XML, this follows a more permissive markup format with automatic recovery most similar to HTML5.
	/// </remarks>
	public class HtmlTokenizer : ITextTokenizer<MarkupTokenType>
	{
		#region Inner Types

		private class QName
		{
			#region Properties

			public string Prefix { get; set; }

			public string Name { get; set; }

			#endregion Properties

			#region Object Overrides

			public override string ToString()
			{
				if (String.IsNullOrEmpty(this.Prefix))
				{
					return this.Name;
				}

				return String.Concat(
					this.Prefix,
					MarkupGrammar.OperatorPrefixDelim,
					this.Name);
			}

			#endregion Object Overrides
		}

		private class Attrib
		{
			#region Properties

			public QName QName { get; set; }

			public Token<MarkupTokenType> Value { get; set; }

			#endregion Properties

			#region Object Overrides

			public override string ToString()
			{
				return String.Concat(
					this.QName,
					MarkupGrammar.OperatorPairDelim,
					MarkupGrammar.OperatorStringDelim,
					(this.Value != null) ? this.Value.ValueAsString() : String.Empty,
					MarkupGrammar.OperatorStringDelim);
			}

			#endregion Object Overrides
		}

		#endregion Inner Types

		#region Fields

		private const int DefaultBufferSize = 0x20;
		private readonly PrefixScopeChain ScopeChain = new PrefixScopeChain();

		private ITextStream Scanner = TextReaderStream.Null;
		private bool autoBalanceTags;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets and sets a value indicating if should attempt to auto-balance mismatched tags.
		/// </summary>
		public bool AutoBalanceTags
		{
			get { return this.autoBalanceTags; }
			set { this.autoBalanceTags = value; }
		}

		/// <summary>
		/// Gets the total number of characters read from the input
		/// </summary>
		public int Column
		{
			get { return this.Scanner.Column; }
		}

		/// <summary>
		/// Gets the total number of lines read from the input
		/// </summary>
		public int Line
		{
			get { return this.Scanner.Line; }
		}

		/// <summary>
		/// Gets the current position within the input
		/// </summary>
		public long Index
		{
			get { return this.Scanner.Index; }
		}

		#endregion Properties

		#region Scanning Methods

		private void GetTokens(List<Token<MarkupTokenType>> tokens, ITextStream scanner)
		{
			this.ScopeChain.Clear();

			try
			{
				scanner.BeginChunk();
				while (!scanner.IsCompleted)
				{
					switch (scanner.Peek())
					{
						case MarkupGrammar.OperatorElementBegin:
						{
							// emit any leading text
							this.EmitText(tokens, scanner.EndChunk());

							// process tag
							this.ScanTag(tokens, scanner);

							// resume chunking and capture
							scanner.BeginChunk();
							break;
						}
						case MarkupGrammar.OperatorEntityBegin:
						{
							// emit any leading text
							this.EmitText(tokens, scanner.EndChunk());

							// process entity
							this.EmitText(tokens, this.DecodeEntity(scanner));

							// resume chunking and capture
							scanner.BeginChunk();
							break;
						}
						default:
						{
							scanner.Pop();
							break;
						}
					}
				}

				// emit any trailing text
				this.EmitText(tokens, scanner.EndChunk());

				if (this.ScopeChain.HasScope)
				{
					if (this.autoBalanceTags)
					{
						while (this.ScopeChain.HasScope)
						{
							PrefixScopeChain.Scope scope = this.ScopeChain.Pop();

							tokens.Add(MarkupGrammar.TokenElementEnd);
						}
					}
				}
			}
			catch (DeserializationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new DeserializationException(ex.Message, scanner.Index, scanner.Line, scanner.Column, ex);
			}
		}

		private void ScanTag(List<Token<MarkupTokenType>> tokens, ITextStream scanner)
		{
			if (scanner.Pop() != MarkupGrammar.OperatorElementBegin)
			{
				throw new DeserializationException("Invalid tag start char", scanner.Index, scanner.Line, scanner.Column);
			}

			if (scanner.IsCompleted)
			{
				// end of file, just emit as text
				this.EmitText(tokens, Char.ToString(MarkupGrammar.OperatorElementBegin));
				return;
			}

			Token<MarkupTokenType> unparsed = this.ScanUnparsedBlock(scanner);
			if (unparsed != null)
			{
				tokens.Add(unparsed);
				return;
			}

			char ch = scanner.Peek();
			MarkupTokenType tagType = MarkupTokenType.ElementBegin;
			if (ch == MarkupGrammar.OperatorElementClose)
			{
				tagType = MarkupTokenType.ElementEnd;
				scanner.Pop();
				ch = scanner.Peek();
			}

			QName tagName = HtmlTokenizer.ScanQName(scanner);
			if (tagName == null)
			{
				// treat as literal text
				string text = Char.ToString(MarkupGrammar.OperatorElementBegin);
				if (tagType == MarkupTokenType.ElementEnd)
				{
					text += MarkupGrammar.OperatorElementClose;
				}

				this.EmitText(tokens, text);
				return;
			}

			List<Attrib> attributes = null;

			while (!this.IsTagComplete(scanner, ref tagType))
			{
				Attrib attribute = new Attrib
				{
					QName = HtmlTokenizer.ScanQName(scanner),
					Value = this.ScanAttributeValue(scanner)
				};

				if (attribute.QName == null)
				{
					throw new DeserializationException(
						"Malformed attribute name",
						scanner.Index,
						scanner.Line,
						scanner.Column);
				}

				if (attributes == null)
				{
					attributes = new List<Attrib>();
				}

				attributes.Add(attribute);
			}

			this.EmitTag(tokens, tagType, tagName, attributes);
		}

		private Token<MarkupTokenType> ScanUnparsedBlock(ITextStream scanner)
		{
			char ch = scanner.Peek();
			switch (ch)
			{
				case MarkupGrammar.OperatorComment:
				{
					// consume '!'
					scanner.Pop();

					switch (scanner.Peek())
					{
						case '-':									// "<!--", "-->"		XML/HTML/SGML comment
						{
							string value = this.ScanUnparsedValue(scanner, MarkupGrammar.OperatorCommentBegin, MarkupGrammar.OperatorCommentEnd);
							if (value != null)
							{
								// emit as an unparsed comment
								return MarkupGrammar.TokenUnparsed("!--", "--", value);
							}

							// process as generic declaration
							goto default;
						}
						case '[':									// "<![CDATA[", "]]>"	CDATA section
						{
							string value = this.ScanUnparsedValue(scanner, MarkupGrammar.OperatorCDataBegin, MarkupGrammar.OperatorCDataEnd);
							if (value != null)
							{
								// convert CData to text
								return MarkupGrammar.TokenPrimitive(value);
							}

							// process as generic declaration
							goto default;
						}
						default:									// "<!", ">"			SGML declaration (e.g. DOCTYPE or server-side include)
						{
							return MarkupGrammar.TokenUnparsed(
								"!", "",
								this.ScanUnparsedValue(scanner, String.Empty, String.Empty));
						}
					}
				}
				case MarkupGrammar.OperatorProcessingInstruction:
				{
					// consume '?'
					scanner.Pop();

					switch (scanner.Peek())
					{
						case MarkupGrammar.OperatorCodeExpression:	// "<?=", "?>"			PHP expression code block
						{
							return MarkupGrammar.TokenUnparsed(
								MarkupGrammar.OperatorPhpExpressionBegin,
								MarkupGrammar.OperatorProcessingInstructionEnd,
								this.ScanUnparsedValue(scanner, MarkupGrammar.OperatorPhpExpressionBegin, MarkupGrammar.OperatorProcessingInstructionEnd));
						}
						default:									// "<?", "?>"			PHP code block / XML processing instruction (e.g. XML declaration)
						{
							return MarkupGrammar.TokenUnparsed(
								MarkupGrammar.OperatorProcessingInstructionBegin,
								MarkupGrammar.OperatorProcessingInstructionEnd,
								this.ScanUnparsedValue(scanner, String.Empty, MarkupGrammar.OperatorProcessingInstructionEnd));
						}
					}
				}
				case MarkupGrammar.OperatorCode:
				{
					// consume '%'
					scanner.Pop();
					ch = scanner.Peek();

					switch (ch)
					{
						case MarkupGrammar.OperatorCommentDelim:		// "<%--", "--%>"		ASP/PSP/JSP-style code comment
						{
							return MarkupGrammar.TokenUnparsed(
								"%--", "--%",
								this.ScanUnparsedValue(scanner, MarkupGrammar.OperatorCommentBegin, String.Concat(MarkupGrammar.OperatorCommentEnd, MarkupGrammar.OperatorCode)));
						}
						case MarkupGrammar.OperatorCodeDirective:		// "<%@",  "%>"			ASP/PSP/JSP directive
						case MarkupGrammar.OperatorCodeExpression:		// "<%=",  "%>"			ASP/PSP/JSP/JBST expression
						case MarkupGrammar.OperatorCodeDeclaration:		// "<%!",  "%>"			JSP/JBST declaration
						case MarkupGrammar.OperatorCodeDataBind:		// "<%#",  "%>"			ASP.NET/JBST databind expression
						case MarkupGrammar.OperatorCodeExtension:		// "<%$",  "%>"			ASP.NET/JBST extension
						case MarkupGrammar.OperatorCodeEncoded:			// "<%:",  "%>"			ASP.NET 4 HTML-encoded expression
						{
							// consume code block type differentiating char
							scanner.Pop();

							return MarkupGrammar.TokenUnparsed(
								String.Concat(MarkupGrammar.OperatorCode, ch),
								MarkupGrammar.OperatorCodeEnd,
								this.ScanUnparsedValue(scanner, String.Empty, MarkupGrammar.OperatorCodeEnd));
						}
						default:										// "<%",   "%>"			ASP/PSP/JSP code block
						{
							// simple code block
							return MarkupGrammar.TokenUnparsed(
								MarkupGrammar.OperatorCodeBlockBegin,
								MarkupGrammar.OperatorCodeEnd,
								this.ScanUnparsedValue(scanner, String.Empty, MarkupGrammar.OperatorCodeEnd));
						}
					}
				}
				case MarkupGrammar.OperatorT4:
				{
					// consume '#'
					scanner.Pop();
					ch = scanner.Peek();

					switch (ch)
					{
						case MarkupGrammar.OperatorCommentDelim:		// "<#--", "--#>"		T4-style code comment
						{
							return MarkupGrammar.TokenUnparsed(
								"#--", "--#",
								this.ScanUnparsedValue(scanner, MarkupGrammar.OperatorCommentBegin, String.Concat(MarkupGrammar.OperatorCommentEnd, MarkupGrammar.OperatorT4)));
						}
						case MarkupGrammar.OperatorT4Directive:			// "<#@",  "#>"			T4 directive
						case MarkupGrammar.OperatorT4Expression:		// "<#=",  "#>"			T4 expression
						case MarkupGrammar.OperatorT4ClassFeature:		// "<#+",  "#>"			T4 ClassFeature blocks
						{
							// consume code block type differentiating char
							scanner.Pop();

							return MarkupGrammar.TokenUnparsed(
								String.Concat(MarkupGrammar.OperatorT4, ch),
								MarkupGrammar.OperatorT4End,
								this.ScanUnparsedValue(scanner, String.Empty, MarkupGrammar.OperatorT4End));
						}
						default:										// "<#",   "#>"			T4 code block
						{
							// simple code block
							return MarkupGrammar.TokenUnparsed(
								MarkupGrammar.OperatorT4BlockBegin,
								MarkupGrammar.OperatorT4End,
								this.ScanUnparsedValue(scanner, String.Empty, MarkupGrammar.OperatorT4End));
						}
					}
				}
			}

			// none matched
			return null;
		}

		private string ScanUnparsedValue(ITextStream scanner, string begin, string end)
		{
			char ch = scanner.Peek();

			int beginLength = begin.Length;
			for (int i=0; i<beginLength; i++)
			{
				if (!scanner.IsCompleted &&
					ch == begin[i])
				{
					scanner.Pop();
					ch = scanner.Peek();
					continue;
				}

				if (i == 0)
				{
					// didn't match anything but didn't consume either
					return null;
				}

				throw new DeserializationException(
					"Unrecognized unparsed tag",
					scanner.Index,
					scanner.Line,
					scanner.Column);
			}

			end += MarkupGrammar.OperatorElementEnd;
			scanner.BeginChunk();

			int endLength = end.Length;
			for (int i=0; !scanner.IsCompleted; )
			{
				if (ch == end[i])
				{
					i++;

					if (i >= endLength)
					{
						string value = scanner.EndChunk();

						// consume '>'
						scanner.Pop();

						endLength--;
						if (endLength > 0)
						{
							// trim ending delimiter
							value = value.Remove(value.Length-endLength);
						}

						return value;
					}
				}
				else
				{
					i = 0;
				}

				scanner.Pop();
				ch = scanner.Peek();
			}

			throw new DeserializationException(
				"Unexpected end of file",
				scanner.Index,
				scanner.Line,
				scanner.Column);
		}

		private Token<MarkupTokenType> ScanAttributeValue(ITextStream scanner)
		{
			HtmlTokenizer.SkipWhitespace(scanner);

			if (scanner.Peek() != MarkupGrammar.OperatorPairDelim)
			{
				return MarkupGrammar.TokenPrimitive(String.Empty);
			}

			scanner.Pop();
			HtmlTokenizer.SkipWhitespace(scanner);

			char stringDelim = scanner.Peek();
			if (stringDelim == MarkupGrammar.OperatorStringDelim ||
				stringDelim == MarkupGrammar.OperatorStringDelimAlt)
			{
				scanner.Pop();
				char ch = scanner.Peek();

				// start chunking
				scanner.BeginChunk();

				if (ch == MarkupGrammar.OperatorElementBegin)
				{
					scanner.Pop();
					Token<MarkupTokenType> unparsed = this.ScanUnparsedBlock(scanner);
					if (unparsed != null)
					{
						ch = scanner.Peek();
						while (!scanner.IsCompleted &&
							!HtmlTokenizer.IsWhiteSpace(ch) &&
							ch != stringDelim)
						{
							// consume until ending delim
							scanner.Pop();
							ch = scanner.Peek();
						}

						if (scanner.IsCompleted ||
							ch != stringDelim)
						{
							throw new DeserializationException(
								"Missing attribute value closing delimiter",
								scanner.Index,
								scanner.Line,
								scanner.Column);
						}

						// flush closing delim
						scanner.Pop();
						return unparsed;
					}

					// otherwise treat as less than
				}

				// check each character for ending delim
				while (!scanner.IsCompleted &&
					ch != stringDelim)
				{
					// accumulate
					scanner.Pop();
					ch = scanner.Peek();
				}

				if (scanner.IsCompleted)
				{
					throw new DeserializationException(
						"Unexpected end of file",
						scanner.Index,
						scanner.Line,
						scanner.Column);
				}

				// end chunking
				string value = scanner.EndChunk();

				// flush closing delim
				scanner.Pop();

				// output string
				return MarkupGrammar.TokenPrimitive(value);
			}
			else
			{
				// start chunking
				scanner.BeginChunk();

				if (stringDelim == MarkupGrammar.OperatorElementBegin)
				{
					scanner.Pop();
					Token<MarkupTokenType> unparsed = this.ScanUnparsedBlock(scanner);
					if (unparsed != null)
					{
						return unparsed;
					}

					// otherwise treat as less than
				}

				char ch = scanner.Peek();

				// check each character for ending delim
				while (!scanner.IsCompleted &&
					ch != MarkupGrammar.OperatorElementClose &&
					ch != MarkupGrammar.OperatorElementEnd &&
					!HtmlTokenizer.IsWhiteSpace(ch))
				{
					// accumulate
					scanner.Pop();
					ch = scanner.Peek();
				}

				if (scanner.IsCompleted)
				{
					throw new DeserializationException(
						"Unexpected end of file",
						scanner.Index,
						scanner.Line,
						scanner.Column);
				}

				// return chunk
				return MarkupGrammar.TokenPrimitive(scanner.EndChunk());
			}
		}

		private static QName ScanQName(ITextStream scanner)
		{
			char ch = scanner.Peek();
			if (!HtmlTokenizer.IsNameStartChar(ch))
			{
				return null;
			}

			// start chunking
			scanner.BeginChunk();

			do
			{
				// consume until reach non-name char
				scanner.Pop();
				ch = scanner.Peek();
			} while (!scanner.IsCompleted && HtmlTokenizer.IsNameChar(ch));

			string name = scanner.EndChunk();

			QName qName;
			string[] nameParts = name.Split(':');
			switch (nameParts.Length)
			{
				case 1:
				{
					qName = new QName
					{
						Name = nameParts[0]
					};
					break;
				}
				case 2:
				{
					qName = new QName
					{
						Prefix = nameParts[0],
						Name = nameParts[1]
					};
					break;
				}
				default:
				{
					throw new DeserializationException(
						String.Format("Invalid element name {0}", name),
						scanner.Index,
						scanner.Line,
						scanner.Column);
				}
			}

			return qName;
		}

		private bool IsTagComplete(
			ITextStream scanner,
			ref MarkupTokenType tagType)
		{
			if (scanner.IsCompleted)
			{
				throw new DeserializationException(
					"Unexpected end of file",
					scanner.Index,
					scanner.Line,
					scanner.Column);
			}

			HtmlTokenizer.SkipWhitespace(scanner);

			switch (scanner.Peek())
			{
				case MarkupGrammar.OperatorElementClose:
				{
					scanner.Pop();
					if (scanner.Peek() == MarkupGrammar.OperatorElementEnd)
					{
						if (tagType != MarkupTokenType.ElementBegin)
						{
							throw new DeserializationException(
								"Malformed element tag",
								scanner.Index,
								scanner.Line,
								scanner.Column);
						}

						scanner.Pop();
						tagType = MarkupTokenType.ElementVoid;
						return true;
					}

					throw new DeserializationException(
						"Malformed element tag",
						scanner.Index,
						scanner.Line,
						scanner.Column);
				}
				case MarkupGrammar.OperatorElementEnd:
				{
					scanner.Pop();
					return true;
				}
				default:
				{
					return false;
				}
			}
		}

		private void EmitTag(List<Token<MarkupTokenType>> tokens, MarkupTokenType tagType, QName qName, List<Attrib> attributes)
		{
			PrefixScopeChain.Scope scope;

			if (tagType == MarkupTokenType.ElementEnd)
			{
				DataName closeTagName = new DataName(qName.Name, qName.Prefix, this.ScopeChain.GetNamespace(qName.Prefix, false));

				scope = this.ScopeChain.Pop();
				if (scope == null ||
					scope.TagName != closeTagName)
				{
					if (!this.autoBalanceTags)
					{
						// restore scope item
						if (scope != null)
						{
							this.ScopeChain.Push(scope);
						}

						if (!String.IsNullOrEmpty(closeTagName.Prefix) &&
							!this.ScopeChain.ContainsPrefix(closeTagName.Prefix))
						{
							if (String.IsNullOrEmpty(closeTagName.NamespaceUri) &&
								!this.ScopeChain.ContainsPrefix(String.Empty))
							{
								closeTagName = new DataName(closeTagName.LocalName);
							}
						}

						// no known scope to end prefixes but can close element
						tokens.Add(MarkupGrammar.TokenElementEnd);
						return;
					}

					if (!this.ScopeChain.ContainsTag(closeTagName))
					{
						// restore scope item
						if (scope != null)
						{
							this.ScopeChain.Push(scope);
						}

						// auto-balance just ignores extraneous close tags
						return;
					}
				}

				do
				{
					tokens.Add(MarkupGrammar.TokenElementEnd);

				} while (scope.TagName != closeTagName &&
					(scope = this.ScopeChain.Pop()) != null);

				return;
			}

			// create new element scope
			scope = new PrefixScopeChain.Scope();

			if (attributes != null)
			{
				// search in reverse removing xmlns attributes
				for (int i=attributes.Count-1; i>=0; i--)
				{
					var attribute = attributes[i];

					if (String.IsNullOrEmpty(attribute.QName.Prefix))
					{
						if (attribute.QName.Name == "xmlns")
						{
							// begin tracking new default namespace
							if (attribute.Value == null)
							{
								throw new InvalidOperationException("xmlns value was null");
							}
							scope[String.Empty] = attribute.Value.ValueAsString();
							attributes.RemoveAt(i);
							continue;
						}

					}

					if (attribute.QName.Prefix == "xmlns")
					{
						if (attribute.Value == null)
						{
							throw new InvalidOperationException("xmlns value was null");
						}
						scope[attribute.QName.Name] = attribute.Value.ValueAsString();
						attributes.RemoveAt(i);
						continue;
					}
				}
			}

			// add to scope chain, resolve QName, and store tag name
			this.ScopeChain.Push(scope);

			if (!String.IsNullOrEmpty(qName.Prefix) &&
				!this.ScopeChain.ContainsPrefix(qName.Prefix))
			{
				if (this.ScopeChain.ContainsPrefix(String.Empty))
				{
					scope[qName.Prefix] = String.Empty;
				}
			}

			scope.TagName = new DataName(qName.Name, qName.Prefix, this.ScopeChain.GetNamespace(qName.Prefix, false));

			if (tagType == MarkupTokenType.ElementVoid)
			{
				tokens.Add(MarkupGrammar.TokenElementVoid(scope.TagName));
			}
			else
			{
				tokens.Add(MarkupGrammar.TokenElementBegin(scope.TagName));
			}

			if (attributes != null)
			{
				foreach (var attr in attributes)
				{
					DataName attrName = new DataName(attr.QName.Name, attr.QName.Prefix, this.ScopeChain.GetNamespace(attr.QName.Prefix, false));
					tokens.Add(MarkupGrammar.TokenAttribute(attrName));
					tokens.Add(attr.Value);
				}
			}

			if (tagType == MarkupTokenType.ElementVoid)
			{
				// immediately remove from scope chain
				this.ScopeChain.Pop();
			}
		}

		private void EmitText(List<Token<MarkupTokenType>> tokens, string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return;
			}

			int count = tokens.Count;

			// get last token
			var token = (count > 0) ? tokens[count-1] : null;
			if (token != null &&
				token.TokenType == MarkupTokenType.Primitive &&
				token.Value is string &&
				// prevent appending to attribute values
				(count == 1 || tokens[count-2].TokenType != MarkupTokenType.Attribute))
			{
				// concatenate string literals into single value
				tokens[count-1] = MarkupGrammar.TokenPrimitive(String.Concat(token.Value, value));
				return;
			}

			// just append a new literal
			tokens.Add(MarkupGrammar.TokenPrimitive(value));
		}

		#endregion Scanning Methods

		#region Utility Methods

		private static void SkipWhitespace(ITextStream scanner)
		{
			while (!scanner.IsCompleted && HtmlTokenizer.IsWhiteSpace(scanner.Peek()))
			{
				scanner.Pop();
			}
		}

		/// <summary>
		/// Decodes HTML-style entities into special characters
		/// </summary>
		/// <param name="scanner"></param>
		/// <returns>the entity text</returns>
		/// <remarks>
		/// TODO: validate against HTML5-style entities
		/// http://www.w3.org/TR/html5/tokenization.html#consume-a-character-reference
		/// </remarks>
		public string DecodeEntity(ITextStream scanner)
		{
			// consume '&'
			if (scanner.Pop() != MarkupGrammar.OperatorEntityBegin)
			{
				throw new DeserializationException(
					"Malformed entity",
					scanner.Index,
					scanner.Line,
					scanner.Column);
			}

			string entity, chunk;

			char ch = scanner.Peek();
			if (scanner.IsCompleted ||
				HtmlTokenizer.IsWhiteSpace(ch) ||
				ch == MarkupGrammar.OperatorEntityBegin ||
				ch == MarkupGrammar.OperatorElementBegin)
			{
				return Char.ToString(MarkupGrammar.OperatorEntityBegin);
			}

			if (ch == MarkupGrammar.OperatorEntityNum)
			{
				// entity is Unicode Code Point

				// consume '#'
				scanner.Pop();
				ch = scanner.Peek();

				bool isHex = false;
				if (!scanner.IsCompleted &&
					((ch == MarkupGrammar.OperatorEntityHex) ||
					(ch == MarkupGrammar.OperatorEntityHexAlt)))
				{
					isHex = true;

					// consume 'x'
					scanner.Pop();
					ch = scanner.Peek();
				}

				scanner.BeginChunk();

				while (!scanner.IsCompleted &&
					HtmlTokenizer.IsHexDigit(ch))
				{
					// consume [0-9a-fA-F]
					scanner.Pop();
					ch = scanner.Peek();
				}

				chunk = scanner.EndChunk();

				int utf16;
				if (Int32.TryParse(
					chunk,
					isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None,
					CultureInfo.InvariantCulture,
					out utf16))
				{
					entity = ConvertFromUtf32(utf16);

					if (!scanner.IsCompleted &&
						ch == MarkupGrammar.OperatorEntityEnd)
					{
						scanner.Pop();
					}
					return entity;
				}
				else if (isHex)
				{
					// NOTE this potentially changes "&#X..." to "&#x...";
					return String.Concat(
						MarkupGrammar.OperatorEntityBegin,
						MarkupGrammar.OperatorEntityNum,
						MarkupGrammar.OperatorEntityHex,
						chunk);
				}
				else
				{
					return String.Concat(
						MarkupGrammar.OperatorEntityBegin,
						MarkupGrammar.OperatorEntityNum,
						chunk);
				}
			}

			scanner.BeginChunk();
			while (!scanner.IsCompleted &&
				HtmlTokenizer.IsLetter(ch))
			{
				// consume [a-zA-Z]
				scanner.Pop();
				ch = scanner.Peek();
			}

			chunk = scanner.EndChunk();
			entity = HtmlTokenizer.DecodeEntityName(chunk);
			if (String.IsNullOrEmpty(entity))
			{
				return String.Concat(
					MarkupGrammar.OperatorEntityBegin,
					chunk);
			}

			if (!scanner.IsCompleted &&
				ch == MarkupGrammar.OperatorEntityEnd)
			{
				scanner.Pop();
			}
			return entity;
		}

		/// <summary>
		/// Decodes most known named entities
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private static string DecodeEntityName(string name)
		{
			// http://www.w3.org/TR/REC-html40/sgml/entities.html
			// http://www.bigbaer.com/sidebars/entities/
			// NOTE: entity names are case-sensitive
			switch (name)
			{
				case "quot": { return ConvertFromUtf32(34); }
				case "amp": { return ConvertFromUtf32(38); }
				case "lt": { return ConvertFromUtf32(60); }
				case "gt": { return ConvertFromUtf32(62); }
				case "nbsp": { return ConvertFromUtf32(160); }
				case "iexcl": { return ConvertFromUtf32(161); }
				case "cent": { return ConvertFromUtf32(162); }
				case "pound": { return ConvertFromUtf32(163); }
				case "curren": { return ConvertFromUtf32(164); }
				case "yen": { return ConvertFromUtf32(165); }
				case "euro": { return ConvertFromUtf32(8364); }
				case "brvbar": { return ConvertFromUtf32(166); }
				case "sect": { return ConvertFromUtf32(167); }
				case "uml": { return ConvertFromUtf32(168); }
				case "copy": { return ConvertFromUtf32(169); }
				case "ordf": { return ConvertFromUtf32(170); }
				case "laquo": { return ConvertFromUtf32(171); }
				case "not": { return ConvertFromUtf32(172); }
				case "shy": { return ConvertFromUtf32(173); }
				case "reg": { return ConvertFromUtf32(174); }
				case "trade": { return ConvertFromUtf32(8482); }
				case "macr": { return ConvertFromUtf32(175); }
				case "deg": { return ConvertFromUtf32(176); }
				case "plusmn": { return ConvertFromUtf32(177); }
				case "sup2": { return ConvertFromUtf32(178); }
				case "sup3": { return ConvertFromUtf32(179); }
				case "acute": { return ConvertFromUtf32(180); }
				case "micro": { return ConvertFromUtf32(181); }
				case "para": { return ConvertFromUtf32(182); }
				case "middot": { return ConvertFromUtf32(183); }
				case "cedil": { return ConvertFromUtf32(184); }
				case "sup1": { return ConvertFromUtf32(185); }
				case "ordm": { return ConvertFromUtf32(186); }
				case "raquo": { return ConvertFromUtf32(187); }
				case "frac14": { return ConvertFromUtf32(188); }
				case "frac12": { return ConvertFromUtf32(189); }
				case "frac34": { return ConvertFromUtf32(190); }
				case "iquest": { return ConvertFromUtf32(191); }
				case "times": { return ConvertFromUtf32(215); }
				case "divide": { return ConvertFromUtf32(247); }
				case "Agrave": { return ConvertFromUtf32(192); }
				case "Aacute": { return ConvertFromUtf32(193); }
				case "Acirc": { return ConvertFromUtf32(194); }
				case "Atilde": { return ConvertFromUtf32(195); }
				case "Auml": { return ConvertFromUtf32(196); }
				case "Aring": { return ConvertFromUtf32(197); }
				case "AElig": { return ConvertFromUtf32(198); }
				case "Ccedil": { return ConvertFromUtf32(199); }
				case "Egrave": { return ConvertFromUtf32(200); }
				case "Eacute": { return ConvertFromUtf32(201); }
				case "Ecirc": { return ConvertFromUtf32(202); }
				case "Euml": { return ConvertFromUtf32(203); }
				case "Igrave": { return ConvertFromUtf32(204); }
				case "Iacute": { return ConvertFromUtf32(205); }
				case "Icirc": { return ConvertFromUtf32(206); }
				case "Iuml": { return ConvertFromUtf32(207); }
				case "ETH": { return ConvertFromUtf32(208); }
				case "Ntilde": { return ConvertFromUtf32(209); }
				case "Ograve": { return ConvertFromUtf32(210); }
				case "Oacute": { return ConvertFromUtf32(211); }
				case "Ocirc": { return ConvertFromUtf32(212); }
				case "Otilde": { return ConvertFromUtf32(213); }
				case "Ouml": { return ConvertFromUtf32(214); }
				case "Oslash": { return ConvertFromUtf32(216); }
				case "Ugrave": { return ConvertFromUtf32(217); }
				case "Uacute": { return ConvertFromUtf32(218); }
				case "Ucirc": { return ConvertFromUtf32(219); }
				case "Uuml": { return ConvertFromUtf32(220); }
				case "Yacute": { return ConvertFromUtf32(221); }
				case "THORN": { return ConvertFromUtf32(222); }
				case "szlig": { return ConvertFromUtf32(223); }
				case "agrave": { return ConvertFromUtf32(224); }
				case "aacute": { return ConvertFromUtf32(225); }
				case "acirc": { return ConvertFromUtf32(226); }
				case "atilde": { return ConvertFromUtf32(227); }
				case "auml": { return ConvertFromUtf32(228); }
				case "aring": { return ConvertFromUtf32(229); }
				case "aelig": { return ConvertFromUtf32(230); }
				case "ccedil": { return ConvertFromUtf32(231); }
				case "egrave": { return ConvertFromUtf32(232); }
				case "eacute": { return ConvertFromUtf32(233); }
				case "ecirc": { return ConvertFromUtf32(234); }
				case "euml": { return ConvertFromUtf32(235); }
				case "igrave": { return ConvertFromUtf32(236); }
				case "iacute": { return ConvertFromUtf32(237); }
				case "icirc": { return ConvertFromUtf32(238); }
				case "iuml": { return ConvertFromUtf32(239); }
				case "eth": { return ConvertFromUtf32(240); }
				case "ntilde": { return ConvertFromUtf32(241); }
				case "ograve": { return ConvertFromUtf32(242); }
				case "oacute": { return ConvertFromUtf32(243); }
				case "ocirc": { return ConvertFromUtf32(244); }
				case "otilde": { return ConvertFromUtf32(245); }
				case "ouml": { return ConvertFromUtf32(246); }
				case "oslash": { return ConvertFromUtf32(248); }
				case "ugrave": { return ConvertFromUtf32(249); }
				case "uacute": { return ConvertFromUtf32(250); }
				case "ucirc": { return ConvertFromUtf32(251); }
				case "uuml": { return ConvertFromUtf32(252); }
				case "yacute": { return ConvertFromUtf32(253); }
				case "thorn": { return ConvertFromUtf32(254); }
				case "yuml": { return ConvertFromUtf32(255); }
				case "OElig": { return ConvertFromUtf32(338); }
				case "oelig": { return ConvertFromUtf32(339); }
				case "Scaron": { return ConvertFromUtf32(352); }
				case "scaron": { return ConvertFromUtf32(353); }
				case "Yuml": { return ConvertFromUtf32(376); }
				case "circ": { return ConvertFromUtf32(710); }
				case "tilde": { return ConvertFromUtf32(732); }
				case "ensp": { return ConvertFromUtf32(8194); }
				case "emsp": { return ConvertFromUtf32(8195); }
				case "thinsp": { return ConvertFromUtf32(8201); }
				case "zwnj": { return ConvertFromUtf32(8204); }
				case "zwj": { return ConvertFromUtf32(8205); }
				case "lrm": { return ConvertFromUtf32(8206); }
				case "rlm": { return ConvertFromUtf32(8207); }
				case "ndash": { return ConvertFromUtf32(8211); }
				case "mdash": { return ConvertFromUtf32(8212); }
				case "lsquo": { return ConvertFromUtf32(8216); }
				case "rsquo": { return ConvertFromUtf32(8217); }
				case "sbquo": { return ConvertFromUtf32(8218); }
				case "ldquo": { return ConvertFromUtf32(8220); }
				case "rdquo": { return ConvertFromUtf32(8221); }
				case "bdquo": { return ConvertFromUtf32(8222); }
				case "dagger": { return ConvertFromUtf32(8224); }
				case "Dagger": { return ConvertFromUtf32(8225); }
				case "permil": { return ConvertFromUtf32(8240); }
				case "lsaquo": { return ConvertFromUtf32(8249); }
				case "rsaquo": { return ConvertFromUtf32(8250); }
				case "fnof": { return ConvertFromUtf32(402); }
				case "bull": { return ConvertFromUtf32(8226); }
				case "hellip": { return ConvertFromUtf32(8230); }
				case "prime": { return ConvertFromUtf32(8242); }
				case "Prime": { return ConvertFromUtf32(8243); }
				case "oline": { return ConvertFromUtf32(8254); }
				case "frasl": { return ConvertFromUtf32(8260); }
				case "weierp": { return ConvertFromUtf32(8472); }
				case "image": { return ConvertFromUtf32(8465); }
				case "real": { return ConvertFromUtf32(8476); }
				case "alefsym": { return ConvertFromUtf32(8501); }
				case "larr": { return ConvertFromUtf32(8592); }
				case "uarr": { return ConvertFromUtf32(8593); }
				case "rarr": { return ConvertFromUtf32(8594); }
				case "darr": { return ConvertFromUtf32(8495); }
				case "harr": { return ConvertFromUtf32(8596); }
				case "crarr": { return ConvertFromUtf32(8629); }
				case "lArr": { return ConvertFromUtf32(8656); }
				case "uArr": { return ConvertFromUtf32(8657); }
				case "rArr": { return ConvertFromUtf32(8658); }
				case "dArr": { return ConvertFromUtf32(8659); }
				case "hArr": { return ConvertFromUtf32(8660); }
				case "forall": { return ConvertFromUtf32(8704); }
				case "part": { return ConvertFromUtf32(8706); }
				case "exist": { return ConvertFromUtf32(8707); }
				case "empty": { return ConvertFromUtf32(8709); }
				case "nabla": { return ConvertFromUtf32(8711); }
				case "isin": { return ConvertFromUtf32(8712); }
				case "notin": { return ConvertFromUtf32(8713); }
				case "ni": { return ConvertFromUtf32(8715); }
				case "prod": { return ConvertFromUtf32(8719); }
				case "sum": { return ConvertFromUtf32(8721); }
				case "minus": { return ConvertFromUtf32(8722); }
				case "lowast": { return ConvertFromUtf32(8727); }
				case "radic": { return ConvertFromUtf32(8730); }
				case "prop": { return ConvertFromUtf32(8733); }
				case "infin": { return ConvertFromUtf32(8734); }
				case "ang": { return ConvertFromUtf32(8736); }
				case "and": { return ConvertFromUtf32(8743); }
				case "or": { return ConvertFromUtf32(8744); }
				case "cap": { return ConvertFromUtf32(8745); }
				case "cup": { return ConvertFromUtf32(8746); }
				case "int": { return ConvertFromUtf32(8747); }
				case "there4": { return ConvertFromUtf32(8756); }
				case "sim": { return ConvertFromUtf32(8764); }
				case "cong": { return ConvertFromUtf32(8773); }
				case "asymp": { return ConvertFromUtf32(8776); }
				case "ne": { return ConvertFromUtf32(8800); }
				case "equiv": { return ConvertFromUtf32(8801); }
				case "le": { return ConvertFromUtf32(8804); }
				case "ge": { return ConvertFromUtf32(8805); }
				case "sub": { return ConvertFromUtf32(8834); }
				case "sup": { return ConvertFromUtf32(8835); }
				case "nsub": { return ConvertFromUtf32(8836); }
				case "sube": { return ConvertFromUtf32(8838); }
				case "supe": { return ConvertFromUtf32(8839); }
				case "oplus": { return ConvertFromUtf32(8853); }
				case "otimes": { return ConvertFromUtf32(8855); }
				case "perp": { return ConvertFromUtf32(8869); }
				case "sdot": { return ConvertFromUtf32(8901); }
				case "lceil": { return ConvertFromUtf32(8968); }
				case "rceil": { return ConvertFromUtf32(8969); }
				case "lfloor": { return ConvertFromUtf32(8970); }
				case "rfloor": { return ConvertFromUtf32(8971); }
				case "lang": { return ConvertFromUtf32(9001); }
				case "rang": { return ConvertFromUtf32(9002); }
				case "loz": { return ConvertFromUtf32(9674); }
				case "spades": { return ConvertFromUtf32(9824); }
				case "clubs": { return ConvertFromUtf32(9827); }
				case "hearts": { return ConvertFromUtf32(9829); }
				case "diams": { return ConvertFromUtf32(9830); }
				case "Alpha": { return ConvertFromUtf32(913); }
				case "Beta": { return ConvertFromUtf32(914); }
				case "Gamma": { return ConvertFromUtf32(915); }
				case "Delta": { return ConvertFromUtf32(916); }
				case "Epsilon": { return ConvertFromUtf32(917); }
				case "Zeta": { return ConvertFromUtf32(918); }
				case "Eta": { return ConvertFromUtf32(919); }
				case "Theta": { return ConvertFromUtf32(920); }
				case "Iota": { return ConvertFromUtf32(921); }
				case "Kappa": { return ConvertFromUtf32(922); }
				case "Lambda": { return ConvertFromUtf32(923); }
				case "Mu": { return ConvertFromUtf32(924); }
				case "Nu": { return ConvertFromUtf32(925); }
				case "Xi": { return ConvertFromUtf32(926); }
				case "Omicron": { return ConvertFromUtf32(927); }
				case "Pi": { return ConvertFromUtf32(928); }
				case "Rho": { return ConvertFromUtf32(929); }
				case "Sigma": { return ConvertFromUtf32(931); }
				case "Tau": { return ConvertFromUtf32(932); }
				case "Upsilon": { return ConvertFromUtf32(933); }
				case "Phi": { return ConvertFromUtf32(934); }
				case "Chi": { return ConvertFromUtf32(935); }
				case "Psi": { return ConvertFromUtf32(936); }
				case "Omega": { return ConvertFromUtf32(937); }
				case "alpha": { return ConvertFromUtf32(945); }
				case "beta": { return ConvertFromUtf32(946); }
				case "gamma": { return ConvertFromUtf32(947); }
				case "delta": { return ConvertFromUtf32(948); }
				case "epsilon": { return ConvertFromUtf32(949); }
				case "zeta": { return ConvertFromUtf32(950); }
				case "eta": { return ConvertFromUtf32(951); }
				case "theta": { return ConvertFromUtf32(952); }
				case "iota": { return ConvertFromUtf32(953); }
				case "kappa": { return ConvertFromUtf32(954); }
				case "lambda": { return ConvertFromUtf32(955); }
				case "mu": { return ConvertFromUtf32(956); }
				case "nu": { return ConvertFromUtf32(957); }
				case "xi": { return ConvertFromUtf32(958); }
				case "omicron": { return ConvertFromUtf32(959); }
				case "pi": { return ConvertFromUtf32(960); }
				case "rho": { return ConvertFromUtf32(961); }
				case "sigmaf": { return ConvertFromUtf32(962); }
				case "sigma": { return ConvertFromUtf32(963); }
				case "tau": { return ConvertFromUtf32(964); }
				case "upsilon": { return ConvertFromUtf32(965); }
				case "phi": { return ConvertFromUtf32(966); }
				case "chi": { return ConvertFromUtf32(967); }
				case "psi": { return ConvertFromUtf32(968); }
				case "omega": { return ConvertFromUtf32(969); }
				case "thetasym": { return ConvertFromUtf32(977); }
				case "upsih": { return ConvertFromUtf32(978); }
				case "piv": { return ConvertFromUtf32(982); }
				default: { return null; }
			}
		}

		/// <summary>
		/// Checks for element start char
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/xml/#sec-common-syn
		/// </remarks>
		private static bool IsNameStartChar(char ch)
		{
			return
				(ch >= 'a' && ch <= 'z') ||
				(ch >= 'A' && ch <= 'Z') ||
				(ch == ':') ||
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
				(ch >= '\uFDF0' && ch <= '\uFFFD');
				//(ch >= '\u10000' && ch <= '\uEFFFF');
		}

		/// <summary>
		/// Checks for element name char
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/xml/#sec-common-syn
		/// </remarks>
		private static bool IsNameChar(char ch)
		{
			return
				HtmlTokenizer.IsNameStartChar(ch) ||
				(ch >= '0' && ch <= '9') ||
				(ch == '-') ||
				(ch == '.') ||
				(ch == '\u00B7') ||
				(ch >= '\u0300' && ch <= '\u036F') ||
				(ch >= '\u203F' && ch <= '\u2040');
		}

		/// <summary>
		/// Checks if character matches [A-Za-z]
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		private static bool IsLetter(char ch)
		{
			return
				((ch >= 'a') && (ch <= 'z')) ||
				((ch >= 'A') && (ch <= 'Z'));
		}

		/// <summary>
		/// Checks if character matches [0-9]
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		private static bool IsDigit(char ch)
		{
			return (ch >= '0') && (ch <= '9');
		}

		/// <summary>
		/// Checks if character matches [0-9A-Fa-f]
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		private static bool IsHexDigit(char ch)
		{
			return
				(ch >= '0' && ch <= '9') ||
				(ch >= 'A' && ch <= 'F') ||
				(ch >= 'a' && ch <= 'f');
		}

		/// <summary>
		/// Checks if character is line ending, tab or space
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		private static bool IsWhiteSpace(char ch)
		{
			return
				(ch == ' ') |
				(ch == '\n') ||
				(ch == '\r') ||
				(ch == '\t');
		}

		/// <summary>
		/// Checks if string is null, empty or entirely made up of whitespace
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks>
		/// Essentially the same as String.IsNullOrWhiteSpace from .NET 4.0
		/// with a simpler view of whitespace.
		/// </remarks>
		private static bool IsNullOrWhiteSpace(string value)
		{
			if (value != null)
			{
				for (int i=0, length=value.Length; i<length; i++)
				{
					if (!IsWhiteSpace(value[i]))
					{
						return false;
					}
				}
			}
			return true;
		}

		private static string ConvertFromUtf32(int utf32)
		{
#if SILVERLIGHT
			if (utf32 <= 0xFFFF)
			{
				return new string((char)utf32, 1);
			}

			utf32 -= 0x10000;

			return new string(
				new char[]
				{
					(char)((utf32 / 0x400) + 0xD800),
					(char)((utf32 % 0x400) + 0xDC00)
				});
#else
			return Char.ConvertFromUtf32(utf32);
#endif
		}

		#endregion Utility Methods

		#region ITextTokenizer<DataTokenType> Members

		/// <summary>
		/// Gets a token sequence from the TextReader
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public IEnumerable<Token<MarkupTokenType>> GetTokens(TextReader reader)
		{
			List<Token<MarkupTokenType>> tokens = new List<Token<MarkupTokenType>>();

			this.GetTokens(tokens, (this.Scanner = new TextReaderStream(reader)));

			return tokens;
		}

		/// <summary>
		/// Gets a token sequence from the string
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public IEnumerable<Token<MarkupTokenType>> GetTokens(string text)
		{
			List<Token<MarkupTokenType>> tokens = new List<Token<MarkupTokenType>>();

			this.GetTokens(tokens, (this.Scanner = new StringStream(text)));

			return tokens;
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
				((IDisposable)this.Scanner).Dispose();
			}
		}

		#endregion IDisposable Members
	}
}
