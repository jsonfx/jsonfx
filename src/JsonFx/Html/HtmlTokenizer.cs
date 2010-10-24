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
using JsonFx.Utils;

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
			#region Constants

			private static readonly char[] NameDelim = new[] { MarkupGrammar.OperatorPrefixDelim };

			#endregion Constants

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			public QName(string name)
			{
				if (String.IsNullOrEmpty(name))
				{
					throw new ArgumentNullException("name");
				}

				string[] nameParts = name.Split(NameDelim, StringSplitOptions.RemoveEmptyEntries);
				switch (nameParts.Length)
				{
					case 1:
					{
						this.Prefix = String.Empty;
						this.Name = nameParts[0];
						break;
					}
					case 2:
					{
						this.Prefix = nameParts[0];
						this.Name = nameParts[1];
						break;
					}
					default:
					{
						throw new ArgumentException("name");
					}
				}
			}

			#endregion Init

			#region Properties

			public readonly string Prefix;

			public readonly string Name;

			#endregion Properties

			#region Operators

			public static bool operator ==(QName a, QName b)
			{
				if (Object.ReferenceEquals(a, null))
				{
					return Object.ReferenceEquals(b, null);
				}

				return a.Equals(b);
			}

			public static bool operator !=(QName a, QName b)
			{
				return !(a == b);
			}

			#endregion Operators

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

			public override int GetHashCode()
			{
				int hash = 0x36294b26;

				hash = (-1521134295 * hash) + StringComparer.Ordinal.GetHashCode(this.Name ?? String.Empty);
				hash = (-1521134295 * hash) + StringComparer.Ordinal.GetHashCode(this.Prefix ?? String.Empty);

				return hash;
			}

			public override bool Equals(object obj)
			{
				return this.Equals(obj as QName, null);
			}

			public bool Equals(QName that)
			{
				return this.Equals(that, null);
			}

			public bool Equals(QName that, IEqualityComparer<string> comparer)
			{
				if (comparer == null)
				{
					comparer = StringComparer.Ordinal;
				}

				if (that == null)
				{
					return false;
				}

				return comparer.Equals(this.Prefix, that.Prefix) && comparer.Equals(this.Name, that.Name);
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
		private IList<QName> unparsedTags;
		private bool autoBalanceTags;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets and sets if should attempt to auto-balance mismatched tags.
		/// </summary>
		public bool AutoBalanceTags
		{
			get { return this.autoBalanceTags; }
			set { this.autoBalanceTags = value; }
		}

		/// <summary>
		/// Gets and sets if should unwrap comments inside <see cref="HtmlTokenizer.UnparsedTags"/>.
		/// </summary>
		/// <remarks>
		/// For example, in HTML this would include "script" and "style" tags.
		/// </remarks>
		public bool UnwrapUnparsedComments
		{
			get;
			set;
		}

		/// <summary>
		/// Gets and sets a set of tags which should not have their content parsed.
		/// </summary>
		/// <remarks>
		/// For example, in HTML this would include "script" and "style" tags.
		/// </remarks>
		public IEnumerable<string> UnparsedTags
		{
			get
			{
				if (this.unparsedTags == null)
				{
					yield return null;
				}

				foreach (QName name in this.unparsedTags)
				{
					yield return name.ToString();
				}
			}
			set
			{
				if (value == null)
				{
					this.unparsedTags = null;
					return;
				}

				this.unparsedTags = new List<QName>();
				foreach (string name in value)
				{
					this.unparsedTags.Add(new QName(name));
				}

				if (this.unparsedTags.Count < 1)
				{
					this.unparsedTags = null;
				}
			}
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
				QName unparseBlock = null;

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
							QName tagName = this.ScanTag(tokens, scanner, unparseBlock);
							if (unparseBlock == null)
							{
								if (tagName != null)
								{
									// suspend parsing until matching tag
									unparseBlock = tagName;
								}
							}
							else if (tagName == unparseBlock)
							{
								// matching tag found, resume parsing
								unparseBlock = null;
							}

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

		private QName ScanTag(List<Token<MarkupTokenType>> tokens, ITextStream scanner, QName unparsedName)
		{
			if (scanner.Pop() != MarkupGrammar.OperatorElementBegin)
			{
				throw new DeserializationException("Invalid tag start char", scanner.Index, scanner.Line, scanner.Column);
			}

			if (scanner.IsCompleted)
			{
				// end of file, just emit as text
				this.EmitText(tokens, Char.ToString(MarkupGrammar.OperatorElementBegin));
				return null;
			}

			Token<MarkupTokenType> unparsed = this.ScanUnparsedBlock(scanner);
			if (unparsed != null)
			{
				if (this.UnwrapUnparsedComments &&
					unparsedName != null)
				{
					UnparsedBlock block = unparsed.Value as UnparsedBlock;
					if (block != null && block.Begin == "!--")
					{
						// unwrap comments inside unparsed tags
						unparsed = new Token<MarkupTokenType>(unparsed.TokenType, unparsed.Name, block.Value);
					}
				}
				tokens.Add(unparsed);
				return null;
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
				return null;
			}

			if (unparsedName != null)
			{
				if (tagName != unparsedName ||
					tagType != MarkupTokenType.ElementEnd)
				{
					string text = Char.ToString(MarkupGrammar.OperatorElementBegin);
					if (tagType == MarkupTokenType.ElementEnd)
					{
						text += MarkupGrammar.OperatorElementClose;
					}
					text += tagName.ToString();

					this.EmitText(tokens, text);
					return null;
				}
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

			return (this.unparsedTags != null && this.unparsedTags.Contains(tagName)) ? tagName : null;
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
							!CharUtility.IsWhiteSpace(ch) &&
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
					!CharUtility.IsWhiteSpace(ch))
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
			try
			{
				return new QName(name);
			}
			catch (Exception ex)
			{
				throw new DeserializationException(
					String.Format("Invalid element name ({0})", name),
					scanner.Index,
					scanner.Line,
					scanner.Column,
					ex);
			}
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
			while (!scanner.IsCompleted && CharUtility.IsWhiteSpace(scanner.Peek()))
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
				CharUtility.IsWhiteSpace(ch) ||
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
					CharUtility.IsHexDigit(ch))
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
					entity = CharUtility.ConvertFromUtf32(utf16);

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
				CharUtility.IsLetter(ch))
			{
				// consume [a-zA-Z]
				scanner.Pop();
				ch = scanner.Peek();
			}

			chunk = scanner.EndChunk();
			int codePoint = HtmlTokenizer.DecodeEntityName(chunk);
			if (codePoint < 0)
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
			return CharUtility.ConvertFromUtf32(codePoint);
		}

		/// <summary>
		/// Decodes most known named entities
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private static int DecodeEntityName(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				return -1;
			}

			// http://www.w3.org/TR/REC-html40/sgml/entities.html
			// http://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references#Character_entity_references_in_HTML
			// NOTE: entity names are case-sensitive
			switch (name[0])
			{
				case 'A':
					if (name.Equals("AElig")) { return 198; }
					if (name.Equals("Aacute")) { return 193; }
					if (name.Equals("Acirc")) { return 194; }
					if (name.Equals("Agrave")) { return 192; }
					if (name.Equals("Alpha")) { return 913; }
					if (name.Equals("Aring")) { return 197; }
					if (name.Equals("Atilde")) { return 195; }
					if (name.Equals("Auml")) { return 196; }
					break;
				case 'B':
					if (name.Equals("Beta")) { return 914; }
					break;
				case 'C':
					if (name.Equals("Ccedil")) { return 199; }
					if (name.Equals("Chi")) { return 935; }
					break;
				case 'D':
					if (name.Equals("Dagger")) { return 8225; }
					if (name.Equals("Delta")) { return 916; }
					break;
				case 'E':
					if (name.Equals("ETH")) { return 208; }
					if (name.Equals("Eacute")) { return 201; }
					if (name.Equals("Ecirc")) { return 202; }
					if (name.Equals("Egrave")) { return 200; }
					if (name.Equals("Epsilon")) { return 917; }
					if (name.Equals("Eta")) { return 919; }
					if (name.Equals("Euml")) { return 203; }
					break;
				case 'G':
					if (name.Equals("Gamma")) { return 915; }
					break;
				case 'I':
					if (name.Equals("Iacute")) { return 205; }
					if (name.Equals("Icirc")) { return 206; }
					if (name.Equals("Igrave")) { return 204; }
					if (name.Equals("Iota")) { return 921; }
					if (name.Equals("Iuml")) { return 207; }
					break;
				case 'K':
					if (name.Equals("Kappa")) { return 922; }
					break;
				case 'L':
					if (name.Equals("Lambda")) { return 923; }
					break;
				case 'M':
					if (name.Equals("Mu")) { return 924; }
					break;
				case 'N':
					if (name.Equals("Ntilde")) { return 209; }
					if (name.Equals("Nu")) { return 925; }
					break;
				case 'O':
					if (name.Equals("OElig")) { return 338; }
					if (name.Equals("Oacute")) { return 211; }
					if (name.Equals("Ocirc")) { return 212; }
					if (name.Equals("Ograve")) { return 210; }
					if (name.Equals("Omega")) { return 937; }
					if (name.Equals("Omicron")) { return 927; }
					if (name.Equals("Oslash")) { return 216; }
					if (name.Equals("Otilde")) { return 213; }
					if (name.Equals("Ouml")) { return 214; }
					break;
				case 'P':
					if (name.Equals("Phi")) { return 934; }
					if (name.Equals("Pi")) { return 928; }
					if (name.Equals("Prime")) { return 8243; }
					if (name.Equals("Psi")) { return 936; }
					break;
				case 'R':
					if (name.Equals("Rho")) { return 929; }
					break;
				case 'S':
					if (name.Equals("Scaron")) { return 352; }
					if (name.Equals("Sigma")) { return 931; }
					break;
				case 'T':
					if (name.Equals("THORN")) { return 222; }
					if (name.Equals("Tau")) { return 932; }
					if (name.Equals("Theta")) { return 920; }
					break;
				case 'U':
					if (name.Equals("Uacute")) { return 218; }
					if (name.Equals("Ucirc")) { return 219; }
					if (name.Equals("Ugrave")) { return 217; }
					if (name.Equals("Upsilon")) { return 933; }
					if (name.Equals("Uuml")) { return 220; }
					break;
				case 'X':
					if (name.Equals("Xi")) { return 926; }
					break;
				case 'Y':
					if (name.Equals("Yacute")) { return 221; }
					if (name.Equals("Yuml")) { return 376; }
					break;
				case 'Z':
					if (name.Equals("Zeta")) { return 918; }
					break;
				case 'a':
					if (name.Equals("aacute")) { return 225; }
					if (name.Equals("acirc")) { return 226; }
					if (name.Equals("acute")) { return 180; }
					if (name.Equals("aelig")) { return 230; }
					if (name.Equals("agrave")) { return 224; }
					if (name.Equals("alefsym")) { return 8501; }
					if (name.Equals("alpha")) { return 945; }
					if (name.Equals("amp")) { return 38; }
					if (name.Equals("and")) { return 8743; }
					if (name.Equals("ang")) { return 8736; }
					if (name.Equals("apos")) { return 39; }
					if (name.Equals("aring")) { return 229; }
					if (name.Equals("asymp")) { return 8776; }
					if (name.Equals("atilde")) { return 227; }
					if (name.Equals("auml")) { return 228; }
					break;
				case 'b':
					if (name.Equals("bdquo")) { return 8222; }
					if (name.Equals("beta")) { return 946; }
					if (name.Equals("brvbar")) { return 166; }
					if (name.Equals("bull")) { return 8226; }
					break;
				case 'c':
					if (name.Equals("cap")) { return 8745; }
					if (name.Equals("ccedil")) { return 231; }
					if (name.Equals("cedil")) { return 184; }
					if (name.Equals("cent")) { return 162; }
					if (name.Equals("chi")) { return 967; }
					if (name.Equals("circ")) { return 710; }
					if (name.Equals("clubs")) { return 9827; }
					if (name.Equals("cong")) { return 8773; }
					if (name.Equals("copy")) { return 169; }
					if (name.Equals("crarr")) { return 8629; }
					if (name.Equals("cup")) { return 8746; }
					if (name.Equals("curren")) { return 164; }
					break;
				case 'd':
					if (name.Equals("dArr")) { return 8659; }
					if (name.Equals("dagger")) { return 8224; }
					if (name.Equals("darr")) { return 8495; }
					if (name.Equals("deg")) { return 176; }
					if (name.Equals("delta")) { return 948; }
					if (name.Equals("diams")) { return 9830; }
					if (name.Equals("divide")) { return 247; }
					break;
				case 'e':
					if (name.Equals("eacute")) { return 233; }
					if (name.Equals("ecirc")) { return 234; }
					if (name.Equals("egrave")) { return 232; }
					if (name.Equals("empty")) { return 8709; }
					if (name.Equals("emsp")) { return 8195; }
					if (name.Equals("ensp")) { return 8194; }
					if (name.Equals("epsilon")) { return 949; }
					if (name.Equals("equiv")) { return 8801; }
					if (name.Equals("eta")) { return 951; }
					if (name.Equals("eth")) { return 240; }
					if (name.Equals("euml")) { return 235; }
					if (name.Equals("euro")) { return 8364; }
					if (name.Equals("exist")) { return 8707; }
					break;
				case 'f':
					if (name.Equals("fnof")) { return 402; }
					if (name.Equals("forall")) { return 8704; }
					if (name.Equals("frac12")) { return 189; }
					if (name.Equals("frac14")) { return 188; }
					if (name.Equals("frac34")) { return 190; }
					if (name.Equals("frasl")) { return 8260; }
					break;
				case 'g':
					if (name.Equals("gamma")) { return 947; }
					if (name.Equals("ge")) { return 8805; }
					if (name.Equals("gt")) { return 62; }
					break;
				case 'h':
					if (name.Equals("hArr")) { return 8660; }
					if (name.Equals("harr")) { return 8596; }
					if (name.Equals("hearts")) { return 9829; }
					if (name.Equals("hellip")) { return 8230; }
					break;
				case 'i':
					if (name.Equals("iacute")) { return 237; }
					if (name.Equals("icirc")) { return 238; }
					if (name.Equals("iexcl")) { return 161; }
					if (name.Equals("igrave")) { return 236; }
					if (name.Equals("image")) { return 8465; }
					if (name.Equals("infin")) { return 8734; }
					if (name.Equals("int")) { return 8747; }
					if (name.Equals("iota")) { return 953; }
					if (name.Equals("iquest")) { return 191; }
					if (name.Equals("isin")) { return 8712; }
					if (name.Equals("iuml")) { return 239; }
					break;
				case 'k':
					if (name.Equals("kappa")) { return 954; }
					break;
				case 'l':
					if (name.Equals("lArr")) { return 8656; }
					if (name.Equals("lambda")) { return 955; }
					if (name.Equals("lang")) { return 9001; }
					if (name.Equals("laquo")) { return 171; }
					if (name.Equals("larr")) { return 8592; }
					if (name.Equals("lceil")) { return 8968; }
					if (name.Equals("ldquo")) { return 8220; }
					if (name.Equals("le")) { return 8804; }
					if (name.Equals("lfloor")) { return 8970; }
					if (name.Equals("lowast")) { return 8727; }
					if (name.Equals("loz")) { return 9674; }
					if (name.Equals("lrm")) { return 8206; }
					if (name.Equals("lsaquo")) { return 8249; }
					if (name.Equals("lsquo")) { return 8216; }
					if (name.Equals("lt")) { return 60; }
					break;
				case 'm':
					if (name.Equals("macr")) { return 175; }
					if (name.Equals("mdash")) { return 8212; }
					if (name.Equals("micro")) { return 181; }
					if (name.Equals("middot")) { return 183; }
					if (name.Equals("minus")) { return 8722; }
					if (name.Equals("mu")) { return 956; }
					break;
				case 'n':
					if (name.Equals("nabla")) { return 8711; }
					if (name.Equals("nbsp")) { return 160; }
					if (name.Equals("ndash")) { return 8211; }
					if (name.Equals("ne")) { return 8800; }
					if (name.Equals("ni")) { return 8715; }
					if (name.Equals("not")) { return 172; }
					if (name.Equals("notin")) { return 8713; }
					if (name.Equals("nsub")) { return 8836; }
					if (name.Equals("ntilde")) { return 241; }
					if (name.Equals("nu")) { return 957; }
					break;
				case 'o':
					if (name.Equals("oacute")) { return 243; }
					if (name.Equals("ocirc")) { return 244; }
					if (name.Equals("oelig")) { return 339; }
					if (name.Equals("ograve")) { return 242; }
					if (name.Equals("oline")) { return 8254; }
					if (name.Equals("omega")) { return 969; }
					if (name.Equals("omicron")) { return 959; }
					if (name.Equals("oplus")) { return 8853; }
					if (name.Equals("or")) { return 8744; }
					if (name.Equals("ordf")) { return 170; }
					if (name.Equals("ordm")) { return 186; }
					if (name.Equals("oslash")) { return 248; }
					if (name.Equals("otilde")) { return 245; }
					if (name.Equals("otimes")) { return 8855; }
					if (name.Equals("ouml")) { return 246; }
					break;
				case 'p':
					if (name.Equals("para")) { return 182; }
					if (name.Equals("part")) { return 8706; }
					if (name.Equals("permil")) { return 8240; }
					if (name.Equals("perp")) { return 8869; }
					if (name.Equals("phi")) { return 966; }
					if (name.Equals("pi")) { return 960; }
					if (name.Equals("piv")) { return 982; }
					if (name.Equals("plusmn")) { return 177; }
					if (name.Equals("pound")) { return 163; }
					if (name.Equals("prime")) { return 8242; }
					if (name.Equals("prod")) { return 8719; }
					if (name.Equals("prop")) { return 8733; }
					if (name.Equals("psi")) { return 968; }
					break;
				case 'q':
					if (name.Equals("quot")) { return 34; }
					break;
				case 'r':
					if (name.Equals("rArr")) { return 8658; }
					if (name.Equals("radic")) { return 8730; }
					if (name.Equals("rang")) { return 9002; }
					if (name.Equals("raquo")) { return 187; }
					if (name.Equals("rarr")) { return 8594; }
					if (name.Equals("rceil")) { return 8969; }
					if (name.Equals("rdquo")) { return 8221; }
					if (name.Equals("real")) { return 8476; }
					if (name.Equals("reg")) { return 174; }
					if (name.Equals("rfloor")) { return 8971; }
					if (name.Equals("rho")) { return 961; }
					if (name.Equals("rlm")) { return 8207; }
					if (name.Equals("rsaquo")) { return 8250; }
					if (name.Equals("rsquo")) { return 8217; }
					break;
				case 's':
					if (name.Equals("sbquo")) { return 8218; }
					if (name.Equals("scaron")) { return 353; }
					if (name.Equals("sdot")) { return 8901; }
					if (name.Equals("sect")) { return 167; }
					if (name.Equals("shy")) { return 173; }
					if (name.Equals("sigma")) { return 963; }
					if (name.Equals("sigmaf")) { return 962; }
					if (name.Equals("sim")) { return 8764; }
					if (name.Equals("spades")) { return 9824; }
					if (name.Equals("sub")) { return 8834; }
					if (name.Equals("sube")) { return 8838; }
					if (name.Equals("sum")) { return 8721; }
					if (name.Equals("sup")) { return 8835; }
					if (name.Equals("sup1")) { return 185; }
					if (name.Equals("sup2")) { return 178; }
					if (name.Equals("sup3")) { return 179; }
					if (name.Equals("supe")) { return 8839; }
					if (name.Equals("szlig")) { return 223; }
					break;
				case 't':
					if (name.Equals("tau")) { return 964; }
					if (name.Equals("there4")) { return 8756; }
					if (name.Equals("theta")) { return 952; }
					if (name.Equals("thetasym")) { return 977; }
					if (name.Equals("thinsp")) { return 8201; }
					if (name.Equals("thorn")) { return 254; }
					if (name.Equals("tilde")) { return 732; }
					if (name.Equals("times")) { return 215; }
					if (name.Equals("trade")) { return 8482; }
					break;
				case 'u':
					if (name.Equals("uArr")) { return 8657; }
					if (name.Equals("uacute")) { return 250; }
					if (name.Equals("uarr")) { return 8593; }
					if (name.Equals("ucirc")) { return 251; }
					if (name.Equals("ugrave")) { return 249; }
					if (name.Equals("uml")) { return 168; }
					if (name.Equals("upsih")) { return 978; }
					if (name.Equals("upsilon")) { return 965; }
					if (name.Equals("uuml")) { return 252; }
					break;
				case 'w':
					if (name.Equals("weierp")) { return 8472; }
					break;
				case 'x':
					if (name.Equals("xi")) { return 958; }
					break;
				case 'y':
					if (name.Equals("yacute")) { return 253; }
					if (name.Equals("yen")) { return 165; }
					if (name.Equals("yuml")) { return 255; }
					break;
				case 'z':
					if (name.Equals("zeta")) { return 950; }
					if (name.Equals("zwj")) { return 8205; }
					if (name.Equals("zwnj")) { return 8204; }
					break;
			}
			return -1;
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
