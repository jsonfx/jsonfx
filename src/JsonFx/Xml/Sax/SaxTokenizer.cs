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
				switch (scanner.Peek())
				{
					case SaxGrammar.OperatorElementBegin:
					{
						// emit any leading text
						this.EmitText(tokens, scanner.EndChunk());

						// process tag
						this.ScanTag(tokens, scanner);

						// resume chunking and capture
						scanner.BeginChunk();
						break;
					}
					case SaxGrammar.OperatorEntityBegin:
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
				case SaxGrammar.OperatorCode:
				{
					// "<%--", "--%>"	// ASP/JSP-style code comment
					// "<%@",  "%>"		// ASP/JSP directive
					// "<%=",  "%>"		// ASP/JSP/JBST expression
					// "<%!",  "%>"		// JSP/JBST declaration
					// "<%#",  "%>"		// ASP.NET/JBST databind expression
					// "<%$",  "%>"		// ASP.NET/JBST extension
					// "<%",   "%>"		// ASP code block / JSP scriptlet

					// TODO: scan code block
					throw new NotImplementedException("scan code block");
				}
				case SaxGrammar.OperatorComment:
				{
					// "<!--", "-->"		// HTML/XML/SGML comment
					// "<![CDATA[", "]]>"	// CDATA section
					// "<!", ">"			// SGML declaration (e.g. DOCTYPE or SSI)

					// TODO: scan comment
					throw new NotImplementedException("scan comment");
				}
				case SaxGrammar.OperatorProcessingInstruction:
				{
					// "<?", "?>"	// XML processing instruction (e.g. XML declaration)

					// TODO: scan processing instruction
					throw new NotImplementedException("scan processing instruction");
				}
			}

			return null;
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

					// TODO: error recovery
					throw new NotImplementedException("error recovery");
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

					// TODO: auto tag balancing
					throw new NotImplementedException("auto tag balancing");
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

		private void EmitUnparsedTag(List<Token<SaxTokenType>> tokens, string unparsed)
		{
			// TODO: emit unparsed tag
			throw new NotImplementedException("emit unparsed tag");
		}

		private void EmitText(List<Token<SaxTokenType>> tokens, string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return;
			}

			if (SaxTokenizer.IsNullOrWhiteSpace(value))
			{
				tokens.Add(SaxGrammar.TokenWhitespace(value));
			}
			else
			{
				tokens.Add(SaxGrammar.TokenText(value));
			}
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
			if (scanner.Pop() != SaxGrammar.OperatorEntityBegin)
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
				SaxTokenizer.IsWhiteSpace(ch) ||
				ch == SaxGrammar.OperatorEntityBegin ||
				ch == SaxGrammar.OperatorElementBegin)
			{
				return Char.ToString(SaxGrammar.OperatorEntityBegin);
			}

			if (ch == SaxGrammar.OperatorEntityNum)
			{
				// entity is Unicode Code Point

				// consume '#'
				scanner.Pop();
				ch = scanner.Peek();

				bool isHex = false;
				if (!scanner.IsCompleted &&
					((ch == SaxGrammar.OperatorEntityHex) ||
					(ch == SaxGrammar.OperatorEntityHexAlt)))
				{
					isHex = true;

					// consume 'x'
					scanner.Pop();
					ch = scanner.Peek();
				}

				scanner.BeginChunk();

				while (!scanner.IsCompleted &&
					SaxTokenizer.IsHexDigit(ch))
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
					entity = Char.ConvertFromUtf32(utf16);

					if (!scanner.IsCompleted &&
						ch == SaxGrammar.OperatorEntityEnd)
					{
						scanner.Pop();
					}
					return entity;
				}
				else if (isHex)
				{
					// NOTE this potentially changes "&#X..." to "&#x...";
					return String.Concat(
						SaxGrammar.OperatorEntityBegin,
						SaxGrammar.OperatorEntityNum,
						SaxGrammar.OperatorEntityHex,
						chunk);
				}
				else
				{
					return String.Concat(
						SaxGrammar.OperatorEntityBegin,
						SaxGrammar.OperatorEntityNum,
						chunk);
				}
			}

			scanner.BeginChunk();
			while (!scanner.IsCompleted &&
				SaxTokenizer.IsLetter(ch))
			{
				// consume [a-zA-Z]
				scanner.Pop();
				ch = scanner.Peek();
			}

			chunk = scanner.EndChunk();
			entity = SaxTokenizer.DecodeEntityName(chunk);
			if (String.IsNullOrEmpty(entity))
			{
				return String.Concat(
					SaxGrammar.OperatorEntityBegin,
					chunk);
			}

			if (!scanner.IsCompleted &&
				ch == SaxGrammar.OperatorEntityEnd)
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
				case "quot": { return Char.ConvertFromUtf32(34); }
				case "amp": { return Char.ConvertFromUtf32(38); }
				case "lt": { return Char.ConvertFromUtf32(60); }
				case "gt": { return Char.ConvertFromUtf32(62); }
				case "nbsp": { return Char.ConvertFromUtf32(160); }
				case "iexcl": { return Char.ConvertFromUtf32(161); }
				case "cent": { return Char.ConvertFromUtf32(162); }
				case "pound": { return Char.ConvertFromUtf32(163); }
				case "curren": { return Char.ConvertFromUtf32(164); }
				case "yen": { return Char.ConvertFromUtf32(165); }
				case "euro": { return Char.ConvertFromUtf32(8364); }
				case "brvbar": { return Char.ConvertFromUtf32(166); }
				case "sect": { return Char.ConvertFromUtf32(167); }
				case "uml": { return Char.ConvertFromUtf32(168); }
				case "copy": { return Char.ConvertFromUtf32(169); }
				case "ordf": { return Char.ConvertFromUtf32(170); }
				case "laquo": { return Char.ConvertFromUtf32(171); }
				case "not": { return Char.ConvertFromUtf32(172); }
				case "shy": { return Char.ConvertFromUtf32(173); }
				case "reg": { return Char.ConvertFromUtf32(174); }
				case "trade": { return Char.ConvertFromUtf32(8482); }
				case "macr": { return Char.ConvertFromUtf32(175); }
				case "deg": { return Char.ConvertFromUtf32(176); }
				case "plusmn": { return Char.ConvertFromUtf32(177); }
				case "sup2": { return Char.ConvertFromUtf32(178); }
				case "sup3": { return Char.ConvertFromUtf32(179); }
				case "acute": { return Char.ConvertFromUtf32(180); }
				case "micro": { return Char.ConvertFromUtf32(181); }
				case "para": { return Char.ConvertFromUtf32(182); }
				case "middot": { return Char.ConvertFromUtf32(183); }
				case "cedil": { return Char.ConvertFromUtf32(184); }
				case "sup1": { return Char.ConvertFromUtf32(185); }
				case "ordm": { return Char.ConvertFromUtf32(186); }
				case "raquo": { return Char.ConvertFromUtf32(187); }
				case "frac14": { return Char.ConvertFromUtf32(188); }
				case "frac12": { return Char.ConvertFromUtf32(189); }
				case "frac34": { return Char.ConvertFromUtf32(190); }
				case "iquest": { return Char.ConvertFromUtf32(191); }
				case "times": { return Char.ConvertFromUtf32(215); }
				case "divide": { return Char.ConvertFromUtf32(247); }
				case "Agrave": { return Char.ConvertFromUtf32(192); }
				case "Aacute": { return Char.ConvertFromUtf32(193); }
				case "Acirc": { return Char.ConvertFromUtf32(194); }
				case "Atilde": { return Char.ConvertFromUtf32(195); }
				case "Auml": { return Char.ConvertFromUtf32(196); }
				case "Aring": { return Char.ConvertFromUtf32(197); }
				case "AElig": { return Char.ConvertFromUtf32(198); }
				case "Ccedil": { return Char.ConvertFromUtf32(199); }
				case "Egrave": { return Char.ConvertFromUtf32(200); }
				case "Eacute": { return Char.ConvertFromUtf32(201); }
				case "Ecirc": { return Char.ConvertFromUtf32(202); }
				case "Euml": { return Char.ConvertFromUtf32(203); }
				case "Igrave": { return Char.ConvertFromUtf32(204); }
				case "Iacute": { return Char.ConvertFromUtf32(205); }
				case "Icirc": { return Char.ConvertFromUtf32(206); }
				case "Iuml": { return Char.ConvertFromUtf32(207); }
				case "ETH": { return Char.ConvertFromUtf32(208); }
				case "Ntilde": { return Char.ConvertFromUtf32(209); }
				case "Ograve": { return Char.ConvertFromUtf32(210); }
				case "Oacute": { return Char.ConvertFromUtf32(211); }
				case "Ocirc": { return Char.ConvertFromUtf32(212); }
				case "Otilde": { return Char.ConvertFromUtf32(213); }
				case "Ouml": { return Char.ConvertFromUtf32(214); }
				case "Oslash": { return Char.ConvertFromUtf32(216); }
				case "Ugrave": { return Char.ConvertFromUtf32(217); }
				case "Uacute": { return Char.ConvertFromUtf32(218); }
				case "Ucirc": { return Char.ConvertFromUtf32(219); }
				case "Uuml": { return Char.ConvertFromUtf32(220); }
				case "Yacute": { return Char.ConvertFromUtf32(221); }
				case "THORN": { return Char.ConvertFromUtf32(222); }
				case "szlig": { return Char.ConvertFromUtf32(223); }
				case "agrave": { return Char.ConvertFromUtf32(224); }
				case "aacute": { return Char.ConvertFromUtf32(225); }
				case "acirc": { return Char.ConvertFromUtf32(226); }
				case "atilde": { return Char.ConvertFromUtf32(227); }
				case "auml": { return Char.ConvertFromUtf32(228); }
				case "aring": { return Char.ConvertFromUtf32(229); }
				case "aelig": { return Char.ConvertFromUtf32(230); }
				case "ccedil": { return Char.ConvertFromUtf32(231); }
				case "egrave": { return Char.ConvertFromUtf32(232); }
				case "eacute": { return Char.ConvertFromUtf32(233); }
				case "ecirc": { return Char.ConvertFromUtf32(234); }
				case "euml": { return Char.ConvertFromUtf32(235); }
				case "igrave": { return Char.ConvertFromUtf32(236); }
				case "iacute": { return Char.ConvertFromUtf32(237); }
				case "icirc": { return Char.ConvertFromUtf32(238); }
				case "iuml": { return Char.ConvertFromUtf32(239); }
				case "eth": { return Char.ConvertFromUtf32(240); }
				case "ntilde": { return Char.ConvertFromUtf32(241); }
				case "ograve": { return Char.ConvertFromUtf32(242); }
				case "oacute": { return Char.ConvertFromUtf32(243); }
				case "ocirc": { return Char.ConvertFromUtf32(244); }
				case "otilde": { return Char.ConvertFromUtf32(245); }
				case "ouml": { return Char.ConvertFromUtf32(246); }
				case "oslash": { return Char.ConvertFromUtf32(248); }
				case "ugrave": { return Char.ConvertFromUtf32(249); }
				case "uacute": { return Char.ConvertFromUtf32(250); }
				case "ucirc": { return Char.ConvertFromUtf32(251); }
				case "uuml": { return Char.ConvertFromUtf32(252); }
				case "yacute": { return Char.ConvertFromUtf32(253); }
				case "thorn": { return Char.ConvertFromUtf32(254); }
				case "yuml": { return Char.ConvertFromUtf32(255); }
				case "OElig": { return Char.ConvertFromUtf32(338); }
				case "oelig": { return Char.ConvertFromUtf32(339); }
				case "Scaron": { return Char.ConvertFromUtf32(352); }
				case "scaron": { return Char.ConvertFromUtf32(353); }
				case "Yuml": { return Char.ConvertFromUtf32(376); }
				case "circ": { return Char.ConvertFromUtf32(710); }
				case "tilde": { return Char.ConvertFromUtf32(732); }
				case "ensp": { return Char.ConvertFromUtf32(8194); }
				case "emsp": { return Char.ConvertFromUtf32(8195); }
				case "thinsp": { return Char.ConvertFromUtf32(8201); }
				case "zwnj": { return Char.ConvertFromUtf32(8204); }
				case "zwj": { return Char.ConvertFromUtf32(8205); }
				case "lrm": { return Char.ConvertFromUtf32(8206); }
				case "rlm": { return Char.ConvertFromUtf32(8207); }
				case "ndash": { return Char.ConvertFromUtf32(8211); }
				case "mdash": { return Char.ConvertFromUtf32(8212); }
				case "lsquo": { return Char.ConvertFromUtf32(8216); }
				case "rsquo": { return Char.ConvertFromUtf32(8217); }
				case "sbquo": { return Char.ConvertFromUtf32(8218); }
				case "ldquo": { return Char.ConvertFromUtf32(8220); }
				case "rdquo": { return Char.ConvertFromUtf32(8221); }
				case "bdquo": { return Char.ConvertFromUtf32(8222); }
				case "dagger": { return Char.ConvertFromUtf32(8224); }
				case "Dagger": { return Char.ConvertFromUtf32(8225); }
				case "permil": { return Char.ConvertFromUtf32(8240); }
				case "lsaquo": { return Char.ConvertFromUtf32(8249); }
				case "rsaquo": { return Char.ConvertFromUtf32(8250); }
				case "fnof": { return Char.ConvertFromUtf32(402); }
				case "bull": { return Char.ConvertFromUtf32(8226); }
				case "hellip": { return Char.ConvertFromUtf32(8230); }
				case "prime": { return Char.ConvertFromUtf32(8242); }
				case "Prime": { return Char.ConvertFromUtf32(8243); }
				case "oline": { return Char.ConvertFromUtf32(8254); }
				case "frasl": { return Char.ConvertFromUtf32(8260); }
				case "weierp": { return Char.ConvertFromUtf32(8472); }
				case "image": { return Char.ConvertFromUtf32(8465); }
				case "real": { return Char.ConvertFromUtf32(8476); }
				case "alefsym": { return Char.ConvertFromUtf32(8501); }
				case "larr": { return Char.ConvertFromUtf32(8592); }
				case "uarr": { return Char.ConvertFromUtf32(8593); }
				case "rarr": { return Char.ConvertFromUtf32(8594); }
				case "darr": { return Char.ConvertFromUtf32(8495); }
				case "harr": { return Char.ConvertFromUtf32(8596); }
				case "crarr": { return Char.ConvertFromUtf32(8629); }
				case "lArr": { return Char.ConvertFromUtf32(8656); }
				case "uArr": { return Char.ConvertFromUtf32(8657); }
				case "rArr": { return Char.ConvertFromUtf32(8658); }
				case "dArr": { return Char.ConvertFromUtf32(8659); }
				case "hArr": { return Char.ConvertFromUtf32(8660); }
				case "forall": { return Char.ConvertFromUtf32(8704); }
				case "part": { return Char.ConvertFromUtf32(8706); }
				case "exist": { return Char.ConvertFromUtf32(8707); }
				case "empty": { return Char.ConvertFromUtf32(8709); }
				case "nabla": { return Char.ConvertFromUtf32(8711); }
				case "isin": { return Char.ConvertFromUtf32(8712); }
				case "notin": { return Char.ConvertFromUtf32(8713); }
				case "ni": { return Char.ConvertFromUtf32(8715); }
				case "prod": { return Char.ConvertFromUtf32(8719); }
				case "sum": { return Char.ConvertFromUtf32(8721); }
				case "minus": { return Char.ConvertFromUtf32(8722); }
				case "lowast": { return Char.ConvertFromUtf32(8727); }
				case "radic": { return Char.ConvertFromUtf32(8730); }
				case "prop": { return Char.ConvertFromUtf32(8733); }
				case "infin": { return Char.ConvertFromUtf32(8734); }
				case "ang": { return Char.ConvertFromUtf32(8736); }
				case "and": { return Char.ConvertFromUtf32(8743); }
				case "or": { return Char.ConvertFromUtf32(8744); }
				case "cap": { return Char.ConvertFromUtf32(8745); }
				case "cup": { return Char.ConvertFromUtf32(8746); }
				case "int": { return Char.ConvertFromUtf32(8747); }
				case "there4": { return Char.ConvertFromUtf32(8756); }
				case "sim": { return Char.ConvertFromUtf32(8764); }
				case "cong": { return Char.ConvertFromUtf32(8773); }
				case "asymp": { return Char.ConvertFromUtf32(8776); }
				case "ne": { return Char.ConvertFromUtf32(8800); }
				case "equiv": { return Char.ConvertFromUtf32(8801); }
				case "le": { return Char.ConvertFromUtf32(8804); }
				case "ge": { return Char.ConvertFromUtf32(8805); }
				case "sub": { return Char.ConvertFromUtf32(8834); }
				case "sup": { return Char.ConvertFromUtf32(8835); }
				case "nsub": { return Char.ConvertFromUtf32(8836); }
				case "sube": { return Char.ConvertFromUtf32(8838); }
				case "supe": { return Char.ConvertFromUtf32(8839); }
				case "oplus": { return Char.ConvertFromUtf32(8853); }
				case "otimes": { return Char.ConvertFromUtf32(8855); }
				case "perp": { return Char.ConvertFromUtf32(8869); }
				case "sdot": { return Char.ConvertFromUtf32(8901); }
				case "lceil": { return Char.ConvertFromUtf32(8968); }
				case "rceil": { return Char.ConvertFromUtf32(8969); }
				case "lfloor": { return Char.ConvertFromUtf32(8970); }
				case "rfloor": { return Char.ConvertFromUtf32(8971); }
				case "lang": { return Char.ConvertFromUtf32(9001); }
				case "rang": { return Char.ConvertFromUtf32(9002); }
				case "loz": { return Char.ConvertFromUtf32(9674); }
				case "spades": { return Char.ConvertFromUtf32(9824); }
				case "clubs": { return Char.ConvertFromUtf32(9827); }
				case "hearts": { return Char.ConvertFromUtf32(9829); }
				case "diams": { return Char.ConvertFromUtf32(9830); }
				case "Alpha": { return Char.ConvertFromUtf32(913); }
				case "Beta": { return Char.ConvertFromUtf32(914); }
				case "Gamma": { return Char.ConvertFromUtf32(915); }
				case "Delta": { return Char.ConvertFromUtf32(916); }
				case "Epsilon": { return Char.ConvertFromUtf32(917); }
				case "Zeta": { return Char.ConvertFromUtf32(918); }
				case "Eta": { return Char.ConvertFromUtf32(919); }
				case "Theta": { return Char.ConvertFromUtf32(920); }
				case "Iota": { return Char.ConvertFromUtf32(921); }
				case "Kappa": { return Char.ConvertFromUtf32(922); }
				case "Lambda": { return Char.ConvertFromUtf32(923); }
				case "Mu": { return Char.ConvertFromUtf32(924); }
				case "Nu": { return Char.ConvertFromUtf32(925); }
				case "Xi": { return Char.ConvertFromUtf32(926); }
				case "Omicron": { return Char.ConvertFromUtf32(927); }
				case "Pi": { return Char.ConvertFromUtf32(928); }
				case "Rho": { return Char.ConvertFromUtf32(929); }
				case "Sigma": { return Char.ConvertFromUtf32(931); }
				case "Tau": { return Char.ConvertFromUtf32(932); }
				case "Upsilon": { return Char.ConvertFromUtf32(933); }
				case "Phi": { return Char.ConvertFromUtf32(934); }
				case "Chi": { return Char.ConvertFromUtf32(935); }
				case "Psi": { return Char.ConvertFromUtf32(936); }
				case "Omega": { return Char.ConvertFromUtf32(937); }
				case "alpha": { return Char.ConvertFromUtf32(945); }
				case "beta": { return Char.ConvertFromUtf32(946); }
				case "gamma": { return Char.ConvertFromUtf32(947); }
				case "delta": { return Char.ConvertFromUtf32(948); }
				case "epsilon": { return Char.ConvertFromUtf32(949); }
				case "zeta": { return Char.ConvertFromUtf32(950); }
				case "eta": { return Char.ConvertFromUtf32(951); }
				case "theta": { return Char.ConvertFromUtf32(952); }
				case "iota": { return Char.ConvertFromUtf32(953); }
				case "kappa": { return Char.ConvertFromUtf32(954); }
				case "lambda": { return Char.ConvertFromUtf32(955); }
				case "mu": { return Char.ConvertFromUtf32(956); }
				case "nu": { return Char.ConvertFromUtf32(957); }
				case "xi": { return Char.ConvertFromUtf32(958); }
				case "omicron": { return Char.ConvertFromUtf32(959); }
				case "pi": { return Char.ConvertFromUtf32(960); }
				case "rho": { return Char.ConvertFromUtf32(961); }
				case "sigmaf": { return Char.ConvertFromUtf32(962); }
				case "sigma": { return Char.ConvertFromUtf32(963); }
				case "tau": { return Char.ConvertFromUtf32(964); }
				case "upsilon": { return Char.ConvertFromUtf32(965); }
				case "phi": { return Char.ConvertFromUtf32(966); }
				case "chi": { return Char.ConvertFromUtf32(967); }
				case "psi": { return Char.ConvertFromUtf32(968); }
				case "omega": { return Char.ConvertFromUtf32(969); }
				case "thetasym": { return Char.ConvertFromUtf32(977); }
				case "upsih": { return Char.ConvertFromUtf32(978); }
				case "piv": { return Char.ConvertFromUtf32(982); }
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
				SaxTokenizer.IsNameStartChar(ch) ||
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
