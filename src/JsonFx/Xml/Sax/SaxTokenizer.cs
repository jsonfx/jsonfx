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

namespace JsonFx.Xml.Sax
{
	/// <summary>
	/// Generates a SAX-like sequence of tokens from XML text
	/// </summary>
	/// <remarks>
	/// Like SAX, this generates a stream of tokens (similar to SAX-events) but
	/// unlike SAX, this follows a more permissive markup format with automatic recovery
	/// most similar to HTML5.
	/// </remarks>
	public class SaxTokenizer : ITextTokenizer<SaxTokenType>
	{
		#region TagType

		/// <summary>
		/// Defines the type of tag
		/// </summary>
		private enum TagType
		{
			/// <summary>
			/// Not set
			/// </summary>
			None,

			/// <summary>
			/// Unparsed block
			/// </summary>
			Unparsed,

			/// <summary>
			/// Opening tag
			/// </summary>
			BeginTag,

			/// <summary>
			/// Closing tag
			/// </summary>
			EndTag,

			/// <summary>
			/// Empty tag
			/// </summary>
			VoidTag
		}

		#endregion TagType

		#region Inner Types

		private class SaxQName
		{
			#region Properties

			public string Prefix { get; set; }

			public string Name { get; set; }

			#endregion Properties

			#region Object Overrides

			public override string ToString()
			{
				return String.Concat(
					this.Prefix,
					SaxGrammar.OperatorPrefixDelim,
					this.Name);
			}

			#endregion Object Overrides
		}

		private class SaxAttribute
		{
			#region Properties

			public SaxQName QName { get; set; }

			public string Value { get; set; }

			#endregion Properties

			#region Object Overrides

			public override string ToString()
			{
				return String.Concat(
					this.QName,
					SaxGrammar.OperatorPairDelim,
					SaxGrammar.OperatorStringDelim,
					this.Value,
					SaxGrammar.OperatorStringDelim);
			}

			#endregion Object Overrides
		}

		#endregion Inner Types

		#region Fields

		private const int DefaultBufferSize = 0x20;

		private ITextStream Scanner = TextReaderStream.Null;
		private bool strictMode = true;

		private PrefixScopeChain ScopeChain = new PrefixScopeChain();

		#endregion Fields

		#region Properties

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

		private void GetTokens(List<Token<SaxTokenType>> tokens, ITextStream scanner)
		{
			scanner.BeginChunk();
			while (!scanner.IsCompleted)
			{
				if (scanner.Peek() == SaxGrammar.OperatorElementBegin)
				{
					// pause chunking and process capture
					string chunk = scanner.EndChunk();
					if (!String.IsNullOrEmpty(chunk))
					{
						if (SaxTokenizer.IsNullOrWhiteSpace(chunk))
						{
							tokens.Add(SaxGrammar.TokenWhitespace(chunk));
						}
						else
						{
							tokens.Add(SaxGrammar.TokenText(chunk));
						}
					}

					// process tag
					this.ScanTag(tokens, scanner);

					// resume chunking and process capture
					scanner.BeginChunk();
					continue;
				}
			}
		}

		private void ScanTag(List<Token<SaxTokenType>> tokens, ITextStream scanner)
		{
			if (scanner.Pop() != SaxGrammar.OperatorElementBegin)
			{
				throw new DeserializationException("Invalid tag start char", scanner.Index, scanner.Line, scanner.Column);
			}

			if (scanner.IsCompleted)
			{
				// end of file
				if (this.strictMode)
				{
					throw new DeserializationException("Unexpected end of file", scanner.Index, scanner.Line, scanner.Column);
				}
				tokens.Add(SaxGrammar.TokenText(SaxGrammar.OperatorElementBegin));
				return;
			}

			string unparsed = this.ScanUnparsedTag(scanner);
			if (!String.IsNullOrEmpty(unparsed))
			{
				this.EmitUnparsedTag(tokens, unparsed);
				return;
			}

			char ch = scanner.Peek();
			TagType tagType = TagType.BeginTag;
			if (ch == SaxGrammar.OperatorElementClose)
			{
				tagType = TagType.EndTag;
				scanner.Pop();
				ch = scanner.Peek();
			}

			SaxQName tagName = SaxTokenizer.ScanQName(scanner);
			if (tagName == null)
			{
				if (this.strictMode)
				{
					throw new DeserializationException(
						"Maltformed element name",
						scanner.Index,
						scanner.Line,
						scanner.Column);
				}

				// treat as literal text
				string text = Char.ToString(SaxGrammar.OperatorElementBegin);
				if (tagType == TagType.EndTag)
				{
					text += SaxGrammar.OperatorElementClose;
				}

				tokens.Add(SaxGrammar.TokenText(text));
				return;
			}

			List<SaxAttribute> attributes = null;

			while (!this.IsTagComplete(scanner, ref tagType))
			{
				SaxAttribute attribute = new SaxAttribute
				{
					QName = SaxTokenizer.ScanQName(scanner),
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
					attributes = new List<SaxAttribute>();
				}

				attributes.Add(attribute);
			}

			this.EmitTag(tokens, tagType, tagName, attributes);
		}

		private string ScanUnparsedTag(ITextStream scanner)
		{
			char ch = scanner.Peek();
			switch (ch)
			{
				case '%':
				{
					// "<%--", "--%>"	// ASP/JSP-style code comment
					// "<%@", "%>"		// ASP/JSP directive
					// "<%=", "%>"		// ASP/JSP/JBST expression
					// "<%!", "%>"		// JSP/JBST declaration
					// "<%#", "%>"		// ASP.NET/JBST databind expression
					// "<%$", "%>"		// ASP.NET/JBST expression
					// "<%", "%>"		// ASP wrapper / JSP scriptlet
					throw new NotImplementedException("code block");
				}
				case '!':
				{
					// "<!--", "-->"		// HTML/XML/SGML comment
					// "<![CDATA[", "]]>"	// CDATA section
					// SGML processing instruction (e.g. DOCTYPE or SSI)
					throw new NotImplementedException("SGML comment");
				}
				case '?':
				{
					// "<?", ">"	// XML processing instruction (e.g. XML declaration)
					throw new NotImplementedException("XML processing instruction");
				}
			}

			return null;
		}

		private bool IsTagComplete(
			ITextStream scanner,
			ref TagType tagType)
		{
			if (scanner.IsCompleted)
			{
				throw new DeserializationException(
					"Unexpected end of file",
					scanner.Index,
					scanner.Line,
					scanner.Column);
			}

			SaxTokenizer.SkipWhitespace(scanner);

			switch (scanner.Peek())
			{
				case SaxGrammar.OperatorElementClose:
				{
					scanner.Pop();
					if (scanner.Peek() == SaxGrammar.OperatorElementEnd)
					{
						if (tagType != TagType.BeginTag)
						{
							if (this.strictMode)
							{
								throw new DeserializationException(
									"Malformed element tag",
									scanner.Index,
									scanner.Line,
									scanner.Column);
							}
						}

						scanner.Pop();
						tagType = TagType.VoidTag;
						return true;
					}

					if (this.strictMode)
					{
						throw new DeserializationException(
							"Malformed element tag",
							scanner.Index,
							scanner.Line,
							scanner.Column);
					}

					throw new NotImplementedException("Error recovery");
				}
				case SaxGrammar.OperatorElementEnd:
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

		private void EmitUnparsedTag(List<Token<SaxTokenType>> tokens, string unparsed)
		{
			throw new NotImplementedException("emit unparsed tag");
		}

		private void EmitTag(List<Token<SaxTokenType>> tokens, TagType tagType, SaxQName qName, List<SaxAttribute> attributes)
		{
			PrefixScopeChain.Scope scope;

			if (tagType == TagType.EndTag)
			{
				DataName closeTagName = new DataName(qName.Name, this.ScopeChain.Resolve(qName.Prefix));
				scope = this.ScopeChain.Pop();

				if (scope.TagName != closeTagName)
				{
					if (this.strictMode)
					{
						throw new DeserializationException(
							String.Format("Tag not balanced: {0}", closeTagName),
							this.Index,
							this.Line,
							this.Column);
					}

					throw new NotImplementedException("Auto tag balancing");
				}

				tokens.Add(SaxGrammar.TokenElementEnd(scope.TagName));

				foreach (var mapping in scope)
				{
					tokens.Add(SaxGrammar.TokenPrefixEnd(mapping.Key, mapping.Value));
				}
				return;
			}

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
							scope[String.Empty] = attribute.Value;
							attributes.RemoveAt(i);
							continue;
						}

					}

					if (attribute.QName.Prefix == "xmlns")
					{
						scope[attribute.QName.Name] = attribute.Value;
						attributes.RemoveAt(i);
						continue;
					}
				}
			}

			// add to scope chain, resolve QName, and store tag name
			this.ScopeChain.Push(scope);
			scope.TagName = new DataName(qName.Name, this.ScopeChain.Resolve(qName.Prefix));

			foreach (var mapping in scope)
			{
				tokens.Add(SaxGrammar.TokenPrefixBegin(mapping.Key, mapping.Value));
			}

			tokens.Add(SaxGrammar.TokenElementBegin(scope.TagName));

			if (attributes != null)
			{
				foreach (var attr in attributes)
				{
					DataName attrName = new DataName(attr.QName.Name, this.ScopeChain.Resolve(attr.QName.Prefix));
					tokens.Add(SaxGrammar.TokenAttribute(attrName, attr.Value));
				}
			}

			if (tagType == TagType.VoidTag)
			{
				// immediately remove from scope chain
				this.ScopeChain.Pop();

				tokens.Add(SaxGrammar.TokenElementEnd(scope.TagName));

				foreach (var mapping in scope)
				{
					tokens.Add(SaxGrammar.TokenPrefixEnd(mapping.Key, mapping.Value));
				}
			}
		}

		private string ScanAttributeValue(ITextStream scanner)
		{
			SaxTokenizer.SkipWhitespace(scanner);

			if (scanner.Peek() != SaxGrammar.OperatorPairDelim)
			{
				return String.Empty;
			}

			scanner.Pop();
			SaxTokenizer.SkipWhitespace(scanner);

			char stringDelim = scanner.Peek();
			if (stringDelim == SaxGrammar.OperatorStringDelim ||
				stringDelim == SaxGrammar.OperatorStringDelimAlt)
			{
				scanner.Pop();
				char ch = scanner.Peek();

				if (ch == SaxGrammar.OperatorElementBegin)
				{
					// TODO: scan for code blocks
				}

				// start chunking
				scanner.BeginChunk();

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
				return value;
			}
			else
			{
				if (stringDelim == SaxGrammar.OperatorElementBegin)
				{
					scanner.Pop();
					string codeBlock = this.ScanUnparsedTag(scanner);

					// TODO: scan for code blocks
				}

				// start chunking
				scanner.BeginChunk();

				char ch = scanner.Peek();

				// check each character for ending delim
				while (!scanner.IsCompleted &&
					ch != SaxGrammar.OperatorElementClose &&
					ch != SaxGrammar.OperatorElementEnd &&
					!SaxTokenizer.IsWhiteSpace(ch))
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
				return scanner.EndChunk();
			}
		}

		private static SaxQName ScanQName(ITextStream scanner)
		{
			char ch = scanner.Peek();
			if (!SaxTokenizer.IsNameStartChar(ch))
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
			} while (!scanner.IsCompleted && SaxTokenizer.IsNameChar(ch));

			string name = scanner.EndChunk();

			SaxQName qName;
			string[] nameParts = name.Split(':');
			switch (nameParts.Length)
			{
				case 1:
				{
					qName = new SaxQName
					{
						Name = nameParts[0]
					};
					break;
				}
				case 2:
				{
					qName = new SaxQName
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

		#endregion Scanning Methods

		#region Utility Methods

		private static void SkipWhitespace(ITextStream scanner)
		{
			while (!scanner.IsCompleted && SaxTokenizer.IsWhiteSpace(scanner.Peek()))
			{
				scanner.Pop();
			}
		}

		/// <summary>
		/// 
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
		/// 
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/xml/#sec-common-syn
		/// </remarks>
		private static bool IsNameChar(char ch)
		{
			return
				SaxTokenizer.IsNameStartChar(ch) ||
				(ch >= '0' && ch <= '9') ||
				(ch == '-') ||
				(ch == '.') ||
				(ch == '\u00B7') ||
				(ch >= '\u0300' && ch <= '\u036F') ||
				(ch >= '\u203F' && ch <= '\u2040');
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

		#endregion Utility Methods

		#region ITextTokenizer<DataTokenType> Members

		/// <summary>
		/// Gets a token sequence from the TextReader
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public IEnumerable<Token<SaxTokenType>> GetTokens(TextReader reader)
		{
			List<Token<SaxTokenType>> tokens = new List<Token<SaxTokenType>>();

			this.GetTokens(tokens, (this.Scanner = new TextReaderStream(reader)));

			return tokens;
		}

		/// <summary>
		/// Gets a token sequence from the string
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public IEnumerable<Token<SaxTokenType>> GetTokens(string text)
		{
			List<Token<SaxTokenType>> tokens = new List<Token<SaxTokenType>>();

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
