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
using System.Text;

using JsonFx.Serialization;

namespace JsonFx.Json
{
	public partial class JsonReader
	{
		/// <summary>
		/// Generates a SAX-like sequence of JSON tokens from text
		/// </summary>
		public class JsonTokenizer : IDataTokenizer<JsonTokenType>
		{
			#region Constants

			// tokenizing errors
			private const string ErrorUnrecognizedToken = "Illegal JSON sequence";
			private const string ErrorUnterminatedComment = "Unterminated comment block";
			private const string ErrorUnterminatedString = "Unterminated JSON string";
			private const string ErrorIllegalNumber = "Illegal JSON number";

			#endregion Constants

			#region Fields

			private readonly DataReaderSettings Settings;
			private TextReader Reader = TextReader.Null;
			private int column;
			private int line;
			private long position;
			private bool isEnd;
			private char next;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public JsonTokenizer(DataReaderSettings settings)
			{
				this.Settings = settings;
			}

			#endregion Init

			#region Properties

			/// <summary>
			/// Gets the total number of characters read from the input
			/// </summary>
			public int Column
			{
				get { return this.column; }
			}

			/// <summary>
			/// Gets the total number of lines read from the input
			/// </summary>
			public int Line
			{
				get { return this.line; }
			}

			/// <summary>
			/// Gets the total number of characters read from the input
			/// </summary>
			public long Position
			{
				get { return this.position; }
			}

			/// <summary>
			/// Gets the underlying TextReader
			/// </summary>
			public TextReader TextReader
			{
				get { return this.Reader; }
			}

			#endregion Properties

			#region Methods

			/// <summary>
			/// Returns the next JSON token in the sequence.
			/// </summary>
			/// <returns></returns>
			private Token<JsonTokenType> NextToken()
			{
				// skip comments and whitespace between tokens
				this.SkipCommentsAndWhitespace();

				if (this.isEnd)
				{
					return JsonGrammar.TokenNone;
				}

				bool hasUnaryOp = false;
				switch (this.next)
				{
					case JsonGrammar.OperatorArrayBegin:
					{
						this.Read();
						return JsonGrammar.TokenArrayBegin;
					}
					case JsonGrammar.OperatorArrayEnd:
					{
						this.Read();
						return JsonGrammar.TokenArrayEnd;
					}
					case JsonGrammar.OperatorObjectBegin:
					{
						this.Read();
						return JsonGrammar.TokenObjectBegin;
					}
					case JsonGrammar.OperatorObjectEnd:
					{
						this.Read();
						return JsonGrammar.TokenObjectEnd;
					}
					case JsonGrammar.OperatorValueDelim:
					{
						this.Read();
						return JsonGrammar.TokenValueDelim;
					}
					case JsonGrammar.OperatorPairDelim:
					{
						this.Read();
						return JsonGrammar.TokenPairDelim;
					}
					case JsonGrammar.OperatorStringDelim:
					case JsonGrammar.OperatorStringDelimAlt:
					{
						return this.ScanString();
					}
					case JsonGrammar.OperatorUnaryMinus:
					case JsonGrammar.OperatorUnaryPlus:
					{
						hasUnaryOp = true;
						break;
					}
				}

				// scan for numbers
				Token<JsonTokenType> token = this.ScanNumber();
				if (token != null)
				{
					return token;
				}

				// hold for Infinity
				char unaryOp = '\0';
				if (hasUnaryOp)
				{
					unaryOp = this.next;
					this.Read();
					this.Peek();
				}

				// scan for identifiers, then check if they are keywords
				string ident = this.ScanIdentifier();
				if (!String.IsNullOrEmpty(ident))
				{
					token = this.ScanKeywords(ident, unaryOp);
					if (token != null)
					{
						return token;
					}
				}

				throw new DeserializationException(JsonTokenizer.ErrorUnrecognizedToken, this.position, this.line, this.column);
			}

			private void SkipCommentsAndWhitespace()
			{
				this.Peek();

				// skip leading whitespace
				this.SkipWhitespace();

				// check for block and line comments
				if (this.next != JsonGrammar.OperatorCommentBegin[0])
				{
					return;
				}

				// store for unterminated case
				long commentStart = this.position;
				int commentCol = this.column;
				int commentLine = this.line;

				// read second char of comment start
				this.Read();
				this.Peek();
				if (this.isEnd)
				{
					throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
				}

				bool isBlockComment;
				if (this.next == JsonGrammar.OperatorCommentBegin[1])
				{
					isBlockComment = true;
				}
				else if (this.next == JsonGrammar.OperatorCommentLine[1])
				{
					isBlockComment = false;
				}
				else
				{
					throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
				}

				// start reading comment content
				if (isBlockComment)
				{
					// skip over everything until reach block comment ending
					while (true)
					{
						do
						{
							this.Read();
							this.Peek();

							if (this.isEnd)
							{
								throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
							}
						} while (this.next != JsonGrammar.OperatorCommentEnd[0]);

						this.Read();
						this.Peek();

						if (this.isEnd)
						{
							throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
						}

						if (this.next == JsonGrammar.OperatorCommentEnd[1])
						{
							// move past block comment end token
							this.Read();
							this.Peek();
							break;
						}
					}
				}
				else
				{
					// skip over everything until reach line ending or end of input
					do
					{
						this.Read();
						this.Peek();

						if (this.isEnd)
						{
							throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
						}
					} while (!this.isEnd && (JsonGrammar.LineEndings.IndexOf(this.next) < 0));
				}

				// skip trailing whitespace
				this.SkipWhitespace();
			}

			private void SkipWhitespace()
			{
				while (!this.isEnd && Char.IsWhiteSpace(this.next))
				{
					this.Read();
					this.Peek();
				}
			}

			private Token<JsonTokenType> ScanNumber()
			{
				StringBuilder numberStr = new StringBuilder();
				this.Peek();

				bool isNeg = false;
				if (this.next == JsonGrammar.OperatorUnaryPlus)
				{
					// consume positive signing (as is extraneous)
					this.Read();
					this.Peek();
				}
				else if (this.next == JsonGrammar.OperatorUnaryMinus)
				{
					// optional minus part
					numberStr.Append(this.next);
					this.Read();
					this.Peek();
					isNeg = true;
				}

				if (!Char.IsDigit(this.next) &&
					this.next != JsonGrammar.OperatorDecimalPoint)
				{
					// possibly "-Infinity"
					return null;
				}

				// integer part
				while (!this.isEnd && Char.IsDigit(this.next))
				{
					// consume digit
					numberStr.Append(this.next);
					this.Read();
					this.Peek();
				}

				bool hasDecimal = false;

				if (!this.isEnd && (this.next == JsonGrammar.OperatorDecimalPoint))
				{
					hasDecimal = true;

					// consume decimal
					numberStr.Append(this.next);
					this.Read();
					this.Peek();

					// fraction part
					while (!this.isEnd && Char.IsDigit(this.next))
					{
						// consume digit
						numberStr.Append(this.next);
						this.Read();
						this.Peek();
					}
				}

				// note the number of significant digits
				int precision = numberStr.Length;
				if (hasDecimal)
				{
					precision--;
				}
				if (isNeg)
				{
					precision--;
				}

				if (precision < 1)
				{
					throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.position, this.line, this.column);
				}

				bool hasExponent = false;

				// optional exponent part
				if (!this.isEnd && (this.next == 'e' || this.next == 'E'))
				{
					hasExponent = true;

					// consume 'e'
					numberStr.Append(this.next);
					this.Read();
					this.Peek();
					if (this.isEnd)
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.position, this.line, this.column);
					}

					// optional minus/plus part
					if (this.next == JsonGrammar.OperatorUnaryMinus ||
						this.next == JsonGrammar.OperatorUnaryPlus)
					{
						// consume sign
						numberStr.Append(this.next);
						this.Read();
						this.Peek();
					}

					if (this.isEnd || !Char.IsDigit(this.next))
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.position, this.line, this.column);
					}

					// exp part
					do
					{
						// consume digit
						numberStr.Append(this.next);
						this.Read();
						this.Peek();
					} while (!this.isEnd && Char.IsDigit(this.next));
				}

				// at this point, we have the full number string and know its characteristics

				if (!hasDecimal && !hasExponent && precision < 19)
				{
					// Integer value
					decimal number;
					if (!Decimal.TryParse(
						numberStr.ToString(),
						NumberStyles.Integer,
						NumberFormatInfo.InvariantInfo,
						out number))
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.position, this.line, this.column);
					}

					if (number >= Int32.MinValue && number <= Int32.MaxValue)
					{
						// most common
						return new Token<JsonTokenType>(JsonTokenType.Number, (int)number);
					}

					if (number >= Int64.MinValue && number <= Int64.MaxValue)
					{
						// more flexible
						return new Token<JsonTokenType>(JsonTokenType.Number, (long)number);
					}

					// most flexible
					return new Token<JsonTokenType>(JsonTokenType.Number, number);
				}
				else
				{
					// Floating Point value
					double number;
					if (!Double.TryParse(
						 numberStr.ToString(),
						 NumberStyles.Float,
						 NumberFormatInfo.InvariantInfo,
						 out number))
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.position, this.line, this.column);
					}

					// native EcmaScript number (IEEE-754)
					return new Token<JsonTokenType>(JsonTokenType.Number, number);
				}
			}

			private Token<JsonTokenType> ScanString()
			{
				// store for unterminated cases
				long strPos = this.position;
				int strLine = this.line;
				int strCol = this.column;

				char stringDelim = this.next;
				this.Read();

				StringBuilder builder = new StringBuilder(20);
				while (true)
				{
					// look ahead
					this.Peek();
					if (this.isEnd)
					{
						// reached END before string delim
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, strPos, strLine, strCol);
					}

					// check each character for ending delim
					if (this.next == stringDelim)
					{
						// flush closing delim
						this.Read();

						// output string
						return new Token<JsonTokenType>(JsonTokenType.String, builder.ToString());
					}

					if (Char.IsControl(this.next) && this.next != '\t')
					{
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, strPos, strLine, strCol);
					}

					if (this.next != JsonGrammar.OperatorCharEscape)
					{
						// accumulate
						builder.Append(this.next);
						this.Read();
						continue;
					}

					// flush escape char
					this.Read();
					this.Peek();
					if (this.isEnd)
					{
						// unexpected end of input
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, strPos, strLine, strCol);
					}

					// begin decode
					switch (this.next)
					{
						case '0':
						{
							// don't allow NULL char '\0'
							// causes CStrings to terminate
							this.Read();
							break;
						}
						case 'b':
						{
							// backspace
							builder.Append('\b');
							this.Read();
							break;
						}
						case 'f':
						{
							// formfeed
							builder.Append('\f');
							this.Read();
							break;
						}
						case 'n':
						{
							// newline
							builder.Append('\n');
							this.Read();
							break;
						}
						case 'r':
						{
							// carriage return
							builder.Append('\r');
							this.Read();
							break;
						}
						case 't':
						{
							// tab
							builder.Append('\t');
							this.Read();
							break;
						}
						case 'u':
						{
							// Unicode escape sequence
							// e.g. (c) => "\u00A9"
							const int UnicodeEscapeLength = 4;

							string escapeSeq = String.Empty;

							this.Read();
							this.Peek();
							for (int i=0; Char.IsDigit(this.next) && (i < UnicodeEscapeLength); i++)
							{
								escapeSeq += this.next;
								this.Read();
								this.Peek();
							}

							// unicode ordinal
							int utf16;
							if (escapeSeq.Length == UnicodeEscapeLength &&
						        Int32.TryParse(
									escapeSeq,
									NumberStyles.AllowHexSpecifier,
									NumberFormatInfo.InvariantInfo,
									out utf16))
							{
								builder.Append(Char.ConvertFromUtf32(utf16));
							}
							else
							{
								// using FireFox style recovery, if not a valid hex
								// escape sequence then treat as single escaped 'u'
								// followed by rest of string
								builder.Append('u');
								builder.Append(escapeSeq);
							}
							break;
						}
						default:
						{
							if (Char.IsControl(this.next) && this.next != '\t')
							{
								throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, strPos, strLine, strCol);
							}

							builder.Append(this.next);
							this.Read();
							break;
						}
					}
				}
			}

			private Token<JsonTokenType> ScanKeywords(string ident, char unary)
			{
				switch (ident)
				{
					case JsonGrammar.KeywordFalse:
					{
						if (unary != '\0')
						{
							return null;
						}

						return JsonGrammar.TokenFalse;
					}
					case JsonGrammar.KeywordTrue:
					{
						if (unary != '\0')
						{
							return null;
						}

						return JsonGrammar.TokenTrue;
					}
					case JsonGrammar.KeywordNull:
					{
						if (unary != '\0')
						{
							return null;
						}

						return JsonGrammar.TokenNull;
					}
					case JsonGrammar.KeywordNaN:
					{
						if (unary != '\0')
						{
							return null;
						}

						return JsonGrammar.TokenNaN;
					}
					case JsonGrammar.KeywordInfinity:
					{
						if (unary == '\0' || unary == JsonGrammar.OperatorUnaryPlus)
						{
							return JsonGrammar.TokenPositiveInfinity;
						}

						if (unary == JsonGrammar.OperatorUnaryMinus)
						{
							return JsonGrammar.TokenNegativeInfinity;
						}

						return null;
					}
					case JsonGrammar.KeywordUndefined:
					{
						if (unary != '\0')
						{
							return null;
						}

						return JsonGrammar.TokenUndefined;
					}
				}

				if (unary != '\0')
				{
					ident = unary.ToString()+ident;
				}

				return new Token<JsonTokenType>(JsonTokenType.Literal, ident);
			}

			/// <summary>
			/// Scans for the longest valid EcmaScript identifier
			/// </summary>
			/// <returns>identifier</returns>
			/// <remarks>
			/// http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf
			/// 
			/// IdentifierName =
			///		IdentifierStart | IdentifierName IdentifierPart
			/// IdentifierStart =
			///		Letter | '$' | '_'
			/// IdentifierPart =
			///		IdentifierStart | Digit
			/// </remarks>
			private string ScanIdentifier()
			{
				bool identPart = false;

				StringBuilder ident = new StringBuilder();
				while (true)
				{
					// look ahead
					this.Peek();

					// digits are only allowed after first char
					// rest can be in head or tail
					if ((identPart && Char.IsDigit(this.next)) ||
						Char.IsLetter(this.next) || this.next == '_' || this.next == '$')
					{
						identPart = true;
						ident.Append(this.next);
						this.Read();
						this.Peek();
						continue;
					}

					// get ident string
					return ident.ToString();
				}
			}

			private void Peek()
			{
				int ch = this.Reader.Peek();
				if (ch < 0)
				{
					this.isEnd = true;
					this.next = '\0';
					return;
				}

				this.next = (char)ch;
			}

			private void Read()
			{
				int ch = this.Reader.Read();
				if (ch < 0)
				{
					this.isEnd = true;
					this.next = '\0';
					return;
				}

				this.next = (char)ch;

				// check for lines
				if (JsonGrammar.LineEndings.IndexOf(this.next) < 0)
				{
					this.column++;
				}
				else
				{
					this.line++;
					this.column = 1;
				}
				this.position++;
			}

			#endregion Scanning Methods

			#region ITokenizer<JsonTokenType> Members

			public IEnumerable<Token<JsonTokenType>> GetTokens(TextReader reader)
			{
				this.Reader = reader;
				this.column = 1;
				this.line = 1;
				this.position = 0;
				this.isEnd = false;

				while (true)
				{
					Token<JsonTokenType> token = this.NextToken();
					if (token.TokenType == JsonTokenType.None)
					{
						yield break;
					}
					yield return token;
				};
			}

			#endregion ITokenizer<JsonTokenType> Members
		}
	}
}
