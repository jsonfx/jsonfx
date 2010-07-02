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

using JsonFx.IO;
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

			private ITextScanner scanner = TextReaderScanner.Null;

			#endregion Fields

			#region Properties

			/// <summary>
			/// Gets the total number of characters read from the input
			/// </summary>
			public int Column
			{
				get { return this.scanner.Column; }
			}

			/// <summary>
			/// Gets the total number of lines read from the input
			/// </summary>
			public int Line
			{
				get { return this.scanner.Line; }
			}

			/// <summary>
			/// Gets the current position within the input
			/// </summary>
			public long Index
			{
				get { return this.scanner.Index; }
			}

			#endregion Properties

			#region Scanning Methods

			/// <summary>
			/// Returns the next JSON token in the sequence.
			/// </summary>
			/// <returns></returns>
			private Token<JsonTokenType> NextToken()
			{
				// skip comments and whitespace between tokens
				this.SkipCommentsAndWhitespace();

				if (this.scanner.IsEnd)
				{
					return JsonGrammar.TokenNone;
				}

				bool hasUnaryOp = false;

				char ch = this.scanner.Current;
				switch (ch)
				{
					case JsonGrammar.OperatorArrayBegin:
					{
						this.scanner.MoveNext();
						return JsonGrammar.TokenArrayBegin;
					}
					case JsonGrammar.OperatorArrayEnd:
					{
						this.scanner.MoveNext();
						return JsonGrammar.TokenArrayEnd;
					}
					case JsonGrammar.OperatorObjectBegin:
					{
						this.scanner.MoveNext();
						return JsonGrammar.TokenObjectBegin;
					}
					case JsonGrammar.OperatorObjectEnd:
					{
						this.scanner.MoveNext();
						return JsonGrammar.TokenObjectEnd;
					}
					case JsonGrammar.OperatorValueDelim:
					{
						this.scanner.MoveNext();
						return JsonGrammar.TokenValueDelim;
					}
					case JsonGrammar.OperatorPairDelim:
					{
						this.scanner.MoveNext();
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

				// hold for Infinity, clear for others
				if (!hasUnaryOp)
				{
					ch = '\0';
				}

				// scan for identifiers, then check if they are keywords
				string ident = this.ScanIdentifier();
				if (!String.IsNullOrEmpty(ident))
				{
					token = this.ScanKeywords(ident, ch);
					if (token != null)
					{
						return token;
					}
				}

				throw new DeserializationException(JsonTokenizer.ErrorUnrecognizedToken, this.scanner.Index, this.scanner.Line, this.scanner.Column);
			}

			private void SkipCommentsAndWhitespace()
			{
				// skip leading whitespace
				this.SkipWhitespace();

				// check for block and line comments
				if (this.scanner.IsEnd || this.scanner.Current != JsonGrammar.OperatorCommentBegin[0])
				{
					return;
				}

				// store for unterminated case
				long commentStart = this.scanner.Index;
				int commentCol = this.scanner.Column;
				int commentLine = this.scanner.Line;

				// read second char of comment start
				this.scanner.MoveNext();
				if (this.scanner.IsEnd)
				{
					throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
				}

				bool isBlockComment;
				if (this.scanner.Current == JsonGrammar.OperatorCommentBegin[1])
				{
					isBlockComment = true;
				}
				else if (this.scanner.Current == JsonGrammar.OperatorCommentLine[1])
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
							this.scanner.MoveNext();

							if (this.scanner.IsEnd)
							{
								throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
							}
						} while (this.scanner.Current != JsonGrammar.OperatorCommentEnd[0]);

						this.scanner.MoveNext();

						if (this.scanner.IsEnd)
						{
							throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
						}

						if (this.scanner.Current == JsonGrammar.OperatorCommentEnd[1])
						{
							// move past block comment end token
							this.scanner.MoveNext();
							break;
						}
					}
				}
				else
				{
					// skip over everything until reach line ending or end of input
					do
					{
						this.scanner.MoveNext();

						if (this.scanner.IsEnd)
						{
							throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
						}
					} while (!this.scanner.IsEnd && ('\r' != this.scanner.Current) && ('\n' != this.scanner.Current));
				}

				// skip trailing whitespace
				this.SkipWhitespace();
			}

			private void SkipWhitespace()
			{
				while (!this.scanner.IsEnd && Char.IsWhiteSpace(this.scanner.Current))
				{
					this.scanner.MoveNext();
				}
			}

			private Token<JsonTokenType> ScanNumber()
			{
				// store for error cases
				long numPos = this.scanner.Index;
				int numLine = this.scanner.Line;
				int numCol = this.scanner.Column;

				StringBuilder buffer = new StringBuilder();

				bool isNeg = false;
				if (this.scanner.Current == JsonGrammar.OperatorUnaryPlus)
				{
					// consume positive signing (as is extraneous)
					this.scanner.MoveNext();
				}
				else if (this.scanner.Current == JsonGrammar.OperatorUnaryMinus)
				{
					// optional minus part
					buffer.Append(this.scanner.Current);
					this.scanner.MoveNext();
					isNeg = true;
				}

				if (!IsDigit(this.scanner.Current) &&
					this.scanner.Current != JsonGrammar.OperatorDecimalPoint)
				{
					// possibly "-Infinity"
					return null;
				}

				// integer part
				while (!this.scanner.IsEnd && IsDigit(this.scanner.Current))
				{
					// consume digit
					buffer.Append(this.scanner.Current);
					this.scanner.MoveNext();
				}

				bool hasDecimal = false;

				if (!this.scanner.IsEnd && (this.scanner.Current == JsonGrammar.OperatorDecimalPoint))
				{
					// consume decimal
					buffer.Append(this.scanner.Current);
					this.scanner.MoveNext();

					// fraction part
					while (!this.scanner.IsEnd && IsDigit(this.scanner.Current))
					{
						// consume digit
						buffer.Append(this.scanner.Current);
						this.scanner.MoveNext();
						hasDecimal = true;
					}

					if (!hasDecimal)
					{
						// fractional digits required when '.' present
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
					}
				}

				// note the number of significant digits
				int precision = buffer.Length;
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
					// missing digits all together
					throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
				}

				bool hasExponent = false;

				// optional exponent part
				if (!this.scanner.IsEnd && (this.scanner.Current == 'e' || this.scanner.Current == 'E'))
				{
					// consume 'e'
					buffer.Append(this.scanner.Current);
					this.scanner.MoveNext();

					// optional minus/plus part
					if (!this.scanner.IsEnd &&
						this.scanner.Current == JsonGrammar.OperatorUnaryMinus ||
						this.scanner.Current == JsonGrammar.OperatorUnaryPlus)
					{
						// consume sign
						buffer.Append(this.scanner.Current);
						this.scanner.MoveNext();
					}

					// exp part
					while (!this.scanner.IsEnd && IsDigit(this.scanner.Current))
					{
						// consume digit
						buffer.Append(this.scanner.Current);
						this.scanner.MoveNext();

						hasExponent = true;
					}

					if (!hasExponent)
					{
						// exponent digits required when 'e' present
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
					}
				}

				// check for 0x-style hex numbers
				if (!this.scanner.IsEnd && IsLetter(this.scanner.Current))
				{
					throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
				}

				// by this point, we have the full number string and know its characteristics

				if (!hasDecimal && !hasExponent && precision < 19)
				{
					// Integer value
					decimal number;
					if (!Decimal.TryParse(
						buffer.ToString(),
						NumberStyles.Integer,
						NumberFormatInfo.InvariantInfo,
						out number))
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
					}

					if (number >= Int32.MinValue && number <= Int32.MaxValue)
					{
						// int most common
						return JsonGrammar.TokenNumber((int)number);
					}

					if (number >= Int64.MinValue && number <= Int64.MaxValue)
					{
						// long more flexible
						return JsonGrammar.TokenNumber((long)number);
					}

					// decimal most flexible
					return JsonGrammar.TokenNumber(number);
				}
				else
				{
					// Floating Point value
					double number;
					if (!Double.TryParse(
						 buffer.ToString(),
						 NumberStyles.Float,
						 NumberFormatInfo.InvariantInfo,
						 out number))
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
					}

					// native EcmaScript number (IEEE-754)
					return JsonGrammar.TokenNumber(number);
				}
			}

			private Token<JsonTokenType> ScanString()
			{
				// store for unterminated cases
				long strPos = this.scanner.Index;
				int strLine = this.scanner.Line;
				int strCol = this.scanner.Column;

				char stringDelim = this.scanner.Current;
				this.scanner.MoveNext();

				StringBuilder buffer = new StringBuilder();
				while (true)
				{
					// look ahead
					if (this.scanner.IsEnd ||
						Char.IsControl(this.scanner.Current) && this.scanner.Current != '\t')
					{
						// reached end or line break before string delim
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, strPos, strLine, strCol);
					}

					// check each character for ending delim
					if (this.scanner.Current == stringDelim)
					{
						// flush closing delim
						this.scanner.MoveNext();

						// output string
						return JsonGrammar.TokenString(buffer.ToString());
					}

					if (this.scanner.Current != JsonGrammar.OperatorCharEscape)
					{
						// accumulate
						buffer.Append(this.scanner.Current);
						this.scanner.MoveNext();
						continue;
					}

					// flush escape char
					this.scanner.MoveNext();
					if (this.scanner.IsEnd ||
						Char.IsControl(this.scanner.Current) && this.scanner.Current != '\t')
					{
						// reached end or line break before string delim
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, strPos, strLine, strCol);
					}

					// begin decode
					switch (this.scanner.Current)
					{
						case '0':
						{
							// consume and do not allow NULL char '\0'
							// causes CStrings to terminate
							this.scanner.MoveNext();
							break;
						}
						case 'b':
						{
							// backspace
							buffer.Append('\b');
							this.scanner.MoveNext();
							break;
						}
						case 'f':
						{
							// formfeed
							buffer.Append('\f');
							this.scanner.MoveNext();
							break;
						}
						case 'n':
						{
							// newline
							buffer.Append('\n');
							this.scanner.MoveNext();
							break;
						}
						case 'r':
						{
							// carriage return
							buffer.Append('\r');
							this.scanner.MoveNext();
							break;
						}
						case 't':
						{
							// tab
							buffer.Append('\t');
							this.scanner.MoveNext();
							break;
						}
						case 'u':
						{
							// Unicode escape sequence
							// e.g. (c) => "\u00A9"
							const int UnicodeEscapeLength = 4;

							string escapeSeq = String.Empty;
							for (int i=UnicodeEscapeLength; this.scanner.MoveNext() && IsHexDigit(this.scanner.Current) && (i > 0); i--)
							{
								escapeSeq += this.scanner.Current;
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
								buffer.Append(Char.ConvertFromUtf32(utf16));
							}
							else
							{
								// using FireFox-style recovery, if not a valid hex
								// escape sequence then treat as single escaped 'u'
								// followed by rest of string
								buffer.Append('u');
								buffer.Append(escapeSeq);
							}
							break;
						}
						default:
						{
							// all unrecognized sequences are interpreted as plain chars
							buffer.Append(this.scanner.Current);
							this.scanner.MoveNext();
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

				return JsonGrammar.TokenLiteral(ident);
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

				StringBuilder buffer = new StringBuilder();
				while (true)
				{
					char ch = this.scanner.Current;

					// digits are only allowed after first char
					// rest can be in head or tail
					if ((identPart && IsDigit(ch)) ||
						IsLetter(ch) || (ch == '_') || (ch == '$'))
					{
						identPart = true;
						buffer.Append(ch);
						this.scanner.MoveNext();
						continue;
					}

					// get ident string
					return buffer.ToString();
				}
			}

			#endregion Scanning Methods

			#region ITokenizer<JsonTokenType> Members

			/// <summary>
			/// Gets a token sequence from the TextReader
			/// </summary>
			/// <param name="reader"></param>
			/// <returns></returns>
			public IEnumerable<Token<JsonTokenType>> GetTokens(TextReader reader)
			{
				return this.GetTokens(new TextReaderScanner(reader));
			}

			/// <summary>
			/// Gets a token sequence from the string
			/// </summary>
			/// <param name="text"></param>
			/// <returns></returns>
			public IEnumerable<Token<JsonTokenType>> GetTokens(string text)
			{
				return this.GetTokens(new StringScanner(text));
			}

			/// <summary>
			/// Gets a token sequence from the scanner
			/// </summary>
			/// <param name="scanner"></param>
			/// <returns></returns>
			protected IEnumerable<Token<JsonTokenType>> GetTokens(ITextScanner scanner)
			{
				if (scanner == null)
				{
					throw new ArgumentNullException("scanner");
				}

				this.scanner = scanner;

				// initialize
				this.scanner.MoveNext();

				while (true)
				{
					Token<JsonTokenType> token = this.NextToken();
					if (token.TokenType == JsonTokenType.None)
					{
						this.scanner.Dispose();
						this.scanner = StringScanner.Null;
						yield break;
					}
					yield return token;
				};
			}

			#endregion ITokenizer<JsonTokenType> Members

			#region Utility Methods

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

			#endregion Utility Methods

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
					this.scanner.Dispose();
				}
			}

			#endregion IDisposable Members
		}
	}
}
