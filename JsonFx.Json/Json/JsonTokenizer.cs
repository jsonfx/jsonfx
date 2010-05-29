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

			private readonly DataReaderSettings Settings;
			private PeekReader Reader = StringPeekReader.Null;

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
			public long Position
			{
				get { return this.Reader.Position; }
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
				// read next char
				int ch = this.Reader.Peek();

				// skip comments and whitespace between tokens
				ch = this.SkipCommentsAndWhitespace(ch);

				bool unaryOp = false;
				switch (ch)
				{
					case JsonGrammar.EndOfSequence:
					{
						return JsonGrammar.TokenNone;
					}
					case JsonGrammar.OperatorArrayBegin:
					{
						this.Reader.Flush(1);
						return JsonGrammar.TokenArrayBegin;
					}
					case JsonGrammar.OperatorArrayEnd:
					{
						this.Reader.Flush(1);
						return JsonGrammar.TokenArrayEnd;
					}
					case JsonGrammar.OperatorObjectBegin:
					{
						this.Reader.Flush(1);
						return JsonGrammar.TokenObjectBegin;
					}
					case JsonGrammar.OperatorObjectEnd:
					{
						this.Reader.Flush(1);
						return JsonGrammar.TokenObjectEnd;
					}
					case JsonGrammar.OperatorValueDelim:
					{
						this.Reader.Flush(1);
						return JsonGrammar.TokenValueDelim;
					}
					case JsonGrammar.OperatorPairDelim:
					{
						this.Reader.Flush(1);
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
						unaryOp = true;
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
				ch = unaryOp ? this.Reader.Read() : -1;

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

				throw new DeserializationException(JsonTokenizer.ErrorUnrecognizedToken, this.Reader.Position);
			}

			private int SkipCommentsAndWhitespace(int ch)
			{
				ch = this.SkipWhitespace(ch);

				// skip block and line comments
				if (ch != JsonGrammar.OperatorCommentBegin[0])
				{
					return ch;
				}

				// store index for unterminated case
				long commentStart = this.Reader.Position;

				// read second char of comment start
				ch = this.Reader.FlushPeek();
				if (ch < 0)
				{
					throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
				}

				bool isBlockComment;
				if (ch == JsonGrammar.OperatorCommentBegin[1])
				{
					isBlockComment = true;
				}
				else if (ch == JsonGrammar.OperatorCommentLine[1])
				{
					isBlockComment = false;
				}
				else
				{
					throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
				}

				// start reading comment content
				if (isBlockComment)
				{
					// skip over everything until reach block comment ending
					while (true)
					{
						while ((ch = this.Reader.FlushPeek()) != JsonGrammar.OperatorCommentEnd[0])
						{
							if (ch < 0)
							{
								throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
							}
						}

						if ((ch = this.Reader.FlushPeek()) == JsonGrammar.OperatorCommentEnd[1])
						{
							break;
						}
					}

					// move past block comment end token
					ch = this.Reader.FlushPeek();
				}
				else
				{
					// skip over everything until reach line ending or end of chars
					while ((ch = this.Reader.FlushPeek()) >= 0 && JsonGrammar.LineEndings.IndexOf((char)ch) < 0) ;
				}

				// skip whitespace
				return this.SkipWhitespace(ch);
			}

			private int SkipWhitespace(int ch)
			{
				while (ch >= 0 && Char.IsWhiteSpace((char)ch))
				{
					ch = this.Reader.FlushPeek();
				}

				return ch;
			}

			private Token<JsonTokenType> ScanNumber()
			{
				int pos = 0;
				int ch = this.Reader.Peek(pos);

				bool isNeg = false;
				if (ch == JsonGrammar.OperatorUnaryPlus)
				{
					// consume positive signing (as is extraneous)
					ch = this.Reader.FlushPeek();
				}
				else if (ch == JsonGrammar.OperatorUnaryMinus)
				{
					// optional minus part
					pos++;
					ch = this.Reader.Peek(pos);
					isNeg = true;
				}

				if (!Char.IsDigit((char)ch) &&
					ch != JsonGrammar.OperatorDecimalPoint)
				{
					// possibly "-Infinity"
					return null;
				}

				// integer part
				while ((ch >= 0) && Char.IsDigit((char)ch))
				{
					// consume digit
					pos++;
					ch = this.Reader.Peek(pos);
				}

				bool hasDecimal = false;

				if ((ch >= 0) && (ch == JsonGrammar.OperatorDecimalPoint))
				{
					hasDecimal = true;

					// consume decimal
					pos++;
					ch = this.Reader.Peek(pos);

					// fraction part
					while ((ch >= 0) && Char.IsDigit((char)ch))
					{
						// consume digit
						pos++;
						ch = this.Reader.Peek(pos);
					}
				}

				// note the number of significant digits
				int precision = pos;
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
					throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.Reader.Position);
				}

				bool hasExponent = false;

				// optional exponent part
				if ((ch >= 0) && (ch == 'e' || ch == 'E'))
				{
					hasExponent = true;

					// consume 'e'
					pos++;
					ch = this.Reader.Peek(pos);
					if (ch < 0)
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.Reader.Position);
					}

					// optional minus/plus part
					if (ch == JsonGrammar.OperatorUnaryMinus ||
						ch == JsonGrammar.OperatorUnaryPlus)
					{
						// consume sign
						pos++;
						ch = this.Reader.Peek(pos);
					}

					if ((ch < 0) || !Char.IsDigit((char)ch))
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.Reader.Position);
					}

					// exp part
					do
					{
						// consume digit
						pos++;
						ch = this.Reader.Peek(pos);
					} while ((ch >= 0) && Char.IsDigit((char)ch));
				}

				// at this point, we have the full number string and know its characteristics
				string numberString;
				this.Reader.Flush(pos, out numberString);

				if (!hasDecimal && !hasExponent && precision < 19)
				{
					// Integer value
					decimal number;
					if (!Decimal.TryParse(
							numberString,
							NumberStyles.Integer,
							NumberFormatInfo.InvariantInfo,
							out number))
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.Reader.Position);
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
						 numberString,
						 NumberStyles.Float,
						 NumberFormatInfo.InvariantInfo,
						 out number))
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, this.Reader.Position);
					}

					// native EcmaScript number (IEEE-754)
					return new Token<JsonTokenType>(JsonTokenType.Number, number);
				}
			}

			private Token<JsonTokenType> ScanString()
			{
				int stringDelim = this.Reader.Read();
				StringBuilder builder = new StringBuilder();

				int pos = 0;
				while (true)
				{
					// look ahead
					int ch = this.Reader.Peek(pos);
					if (ch < 0)
					{
						// reached END before string delim
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, this.Reader.Position-1);
					}

					// check each character for ending delim
					if (ch == stringDelim)
					{
						if (pos > 0)
						{
							// append final segment and flush string
							this.Reader.Flush(pos, builder);
						}

						// flush closing delim
						this.Reader.Flush(1);

						// output string
						return new Token<JsonTokenType>(JsonTokenType.String, builder.ToString());
					}

					if (Char.IsControl((char)ch) && ch != '\t')
					{
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, this.Reader.Position-1);
					}

					if (ch != JsonGrammar.OperatorCharEscape)
					{
						// accumulate
						pos++;
						continue;
					}

					if (pos > 0)
					{
						// append segment before escape
						this.Reader.Flush(pos, builder);

						// reset counter
						pos = 0;
					}

					// flush escape char
					this.Reader.Flush(1);

					// decode
					ch = (char)this.Reader.Read();
					if (ch < 0)
					{
						// unexpected end of input
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, this.Reader.Position-1);
					}

					switch (ch)
					{
						case '0':
						{
							// don't allow NULL char '\0'
							// causes CStrings to terminate
							break;
						}
						case 'b':
						{
							// backspace
							builder.Append('\b');
							break;
						}
						case 'f':
						{
							// formfeed
							builder.Append('\f');
							break;
						}
						case 'n':
						{
							// newline
							builder.Append('\n');
							break;
						}
						case 'r':
						{
							// carriage return
							builder.Append('\r');
							break;
						}
						case 't':
						{
							// tab
							builder.Append('\t');
							break;
						}
						case 'u':
						{
							// Unicode escape sequence
							// e.g. Copyright: "\u00A9"
							const int UnicodeEscapeLength = 4;

							string escapeSeq;
							this.Reader.Peek(UnicodeEscapeLength, out escapeSeq);

							// unicode ordinal
							int utf16;
							if (escapeSeq != null &&
								escapeSeq.Length == UnicodeEscapeLength &&
						        Int32.TryParse(
									escapeSeq,
									NumberStyles.AllowHexSpecifier,
									NumberFormatInfo.InvariantInfo,
									out utf16))
							{
								builder.Append(Char.ConvertFromUtf32(utf16));

								// flush escape char
								this.Reader.Flush(escapeSeq.Length);
							}
							else
							{
								// using FireFox style recovery, if not a valid hex
								// escape sequence then treat as single escaped 'u'
								// followed by rest of string
								goto default;
							}
							break;
						}
						default:
						{
							if (Char.IsControl((char)ch) && ch != '\t')
							{
								throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, this.Reader.Position-1);
							}

							builder.Append(ch);
							break;
						}
					}
				}
			}

			private Token<JsonTokenType> ScanKeywords(string ident, int unary)
			{
				switch (ident)
				{
					case JsonGrammar.KeywordFalse:
					{
						if (unary >= 0)
						{
							return null;
						}
						return JsonGrammar.TokenFalse;
					}
					case JsonGrammar.KeywordTrue:
					{
						if (unary < 0)
						{
							return JsonGrammar.TokenTrue;
						}

						return null;
					}
					case JsonGrammar.KeywordNull:
					{
						if (unary < 0)
						{
							return JsonGrammar.TokenNull;
						}

						return null;
					}
					case JsonGrammar.KeywordNaN:
					{
						if (unary < 0)
						{
							return JsonGrammar.TokenNaN;
						}

						return null;
					}
					case JsonGrammar.KeywordInfinity:
					{
						if (unary < 0 || unary == JsonGrammar.OperatorUnaryPlus)
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
						if (unary < 0)
						{
							return JsonGrammar.TokenUndefined;
						}

						return null;
					}
				}

				if (unary < 0)
				{
					return new Token<JsonTokenType>(JsonTokenType.Literal, ident);
				}

				return null;
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

				int pos = 0;
				while (true)
				{
					// look ahead
					int ch = this.Reader.Peek(pos);

					// digits are only allowed after first char
					// rest can be in head or tail
					if ((identPart && Char.IsDigit((char)ch)) ||
						Char.IsLetter((char)ch) || ch == '_' || ch == '$')
					{
						identPart = true;
						pos++;
						continue;
					}

					// get ident string
					string ident;

					if (pos > 0)
					{
						this.Reader.Flush(pos, out ident);
					}
					else
					{
						ident = String.Empty;
					}

					return ident;
				}
			}

			#endregion Scanning Methods

			#region ITokenizer<JsonTokenType> Members

			public IEnumerable<Token<JsonTokenType>> GetTokens(TextReader reader)
			{
				this.Reader = PeekReader.CreateReader(reader, this.Settings.Performance);

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
