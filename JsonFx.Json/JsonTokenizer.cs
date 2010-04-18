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
using System.Collections;
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
		/// Performs JSON lexical analysis over the input reader
		/// </summary>
		public class JsonTokenizer : IDataTokenizer<JsonTokenType>
		{
			#region Constants

			private const int MinBufferLength = 128; // must hold longest number sequence
			private const int DefaultBufferSize = 1024;

			// tokenizing errors
			private const string ErrorUnrecognizedToken = "Illegal JSON sequence";
			private const string ErrorUnterminatedComment = "Unterminated comment block";
			private const string ErrorUnterminatedString = "Unterminated JSON string";
			private const string ErrorIllegalNumber = "Illegal JSON number";

			#endregion Constants

			#region Fields

			private BufferedTextReader Reader;
			private readonly char[] PeekBuffer;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			public JsonTokenizer()
				: this(JsonTokenizer.DefaultBufferSize)
			{
			}

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="reader">input reader</param>
			/// <param name="bufferSize">read buffer size</param>
			public JsonTokenizer(int bufferSize)
			{
				if (bufferSize < JsonTokenizer.MinBufferLength)
				{
					bufferSize = JsonTokenizer.MinBufferLength;
				}

				this.PeekBuffer = new char[bufferSize];
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
				ch = this.Reader.NextPeek();
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
						while ((ch = this.Reader.NextPeek()) != JsonGrammar.OperatorCommentEnd[0])
						{
							if (ch < 0)
							{
								throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
							}
						}

						if ((ch = this.Reader.NextPeek()) == JsonGrammar.OperatorCommentEnd[1])
						{
							break;
						}
					}

					// move past block comment end token
					ch = this.Reader.NextPeek();
				}
				else
				{
					// skip over everything until reach line ending or end of chars
					while ((ch = this.Reader.NextPeek()) >= 0 && JsonGrammar.LineEndings.IndexOf((char)ch) < 0) ;
				}

				// skip whitespace
				return this.SkipWhitespace(ch);
			}

			private int SkipWhitespace(int ch)
			{
				while (ch >= 0 && Char.IsWhiteSpace((char)ch))
				{
					ch = this.Reader.NextPeek();
				}

				return ch;
			}

			private Token<JsonTokenType> ScanNumber()
			{
				long numberStart = this.Reader.Position;

				int bufferSize = this.Reader.Peek(this.PeekBuffer);
				int start = 0;
				int pos = 0;

				if (this.PeekBuffer[pos] == JsonGrammar.OperatorUnaryPlus)
				{
					// consume positive signing (as is extraneous)
					start++;
					pos++;
				}
				else if (this.PeekBuffer[pos] == JsonGrammar.OperatorUnaryMinus)
				{
					// optional minus part
					pos++;
				}

				if (!Char.IsDigit(this.PeekBuffer[pos]) &&
				this.PeekBuffer[pos] != JsonGrammar.OperatorDecimalPoint)
				{
					// possibly "-Infinity"
					return null;
				}

				// integer part
				while ((pos < bufferSize) && Char.IsDigit(this.PeekBuffer[pos]))
				{
					// consume digit
					pos++;
				}

				bool hasDecimal = false;

				if ((pos < bufferSize) && (this.PeekBuffer[pos] == JsonGrammar.OperatorDecimalPoint))
				{
					hasDecimal = true;

					// consume decimal
					pos++;

					// fraction part
					while ((pos < bufferSize) && Char.IsDigit(this.PeekBuffer[pos]))
					{
						// consume digit
						pos++;
					}
				}

				// note the number of significant digits
				int precision = (pos - start);
				if (hasDecimal)
				{
					precision--;
				}
				if (this.PeekBuffer[start] == JsonGrammar.OperatorUnaryMinus)
				{
					precision--;
				}

				if (precision < 1)
				{
					throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
				}

				bool hasExponent = false;

				// optional exponent part
				if ((pos < bufferSize) && (this.PeekBuffer[pos] == 'e' || this.PeekBuffer[pos] == 'E'))
				{
					hasExponent = true;

					// consume 'e'
					pos++;
					if (pos >= bufferSize)
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
					}

					// optional minus/plus part
					if (this.PeekBuffer[pos] == JsonGrammar.OperatorUnaryMinus ||
					this.PeekBuffer[pos] == JsonGrammar.OperatorUnaryPlus)
					{
						// consume sign
						pos++;
					}

					if (pos >= bufferSize || !Char.IsDigit(this.PeekBuffer[pos]))
					{
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
					}

					// exp part
					do
					{
						// consume digit
						pos++;
					} while (pos < bufferSize && Char.IsDigit(this.PeekBuffer[pos]));
				}

				// at this point, we have the full number string and know its characteristics
				string numberString = new String(this.PeekBuffer, start, pos-start);
				this.Reader.Flush(pos);

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
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
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
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
					}

					// native EcmaScript number (IEEE-754)
					return new Token<JsonTokenType>(JsonTokenType.Number, number);
				}
			}

			private Token<JsonTokenType> ScanString()
			{
				// TODO: simplify this so that it just leverages the BufferedTextReader's buffer
				// then do a performance comparison with original

				// store for unterminated case
				long stringStart = this.Reader.Position;
				char stringDelim = (char)this.Reader.Read();

				StringBuilder builder = new StringBuilder(JsonTokenizer.MinBufferLength);

				// fill buffer
				int count = this.Reader.Peek(this.PeekBuffer);
				while (count > 0)
				{
					int start = 0;
					for (int i=start; i<count; i++)
					{
						char ch = this.PeekBuffer[i];

						// check each character for ending delim
						if (ch == stringDelim)
						{
							// append final segment
							builder.Append(this.PeekBuffer, start, i-start);

							// flush string and closing delim
							this.Reader.Flush(i+1);

							// output string
							return new Token<JsonTokenType>(JsonTokenType.String, builder.ToString());
						}

						if (Char.IsControl(ch) && ch != '\t')
						{
							throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, stringStart);
						}

						if (ch != JsonGrammar.OperatorCharEscape)
						{
							// accumulate
							continue;
						}

						// append before 
						builder.Append(this.PeekBuffer, start, i-start);

						// flush prefix and escape char
						this.Reader.Flush(i+1);

						// ensure full buffer
						count = this.Reader.Peek(this.PeekBuffer);
						start = i = 0;

						if (count < 1)
						{
							// unexpected end of input
							throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, stringStart);
						}

						// decode
						ch = this.PeekBuffer[i];
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

								// unicode ordinal
								int utf16;
								if (start+UnicodeEscapeLength < count &&
						        Int32.TryParse(
									new String(this.PeekBuffer, i+1, UnicodeEscapeLength),
									NumberStyles.AllowHexSpecifier,
									NumberFormatInfo.InvariantInfo,
									out utf16))
								{
									builder.Append(Char.ConvertFromUtf32(utf16));
									i += UnicodeEscapeLength;
									start += UnicodeEscapeLength;
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
								if (Char.IsControl(ch) && ch != '\t')
								{
									throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, stringStart);
								}

								builder.Append(ch);
								break;
							}
						}

						start++;
					}

					// append remaining buffered segment and flush
					builder.Append(this.PeekBuffer, start, count-start);
					this.Reader.Flush(count);

					// refill buffer
					count = this.Reader.Peek(this.PeekBuffer);
				}

				// reached END before string delim
				throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, stringStart);
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
				StringBuilder ident = new StringBuilder(JsonTokenizer.MinBufferLength);

				int bufferSize = this.Reader.Peek(this.PeekBuffer);

				bool identPart = false;
				while (bufferSize > 0)
				{
					int i;
					for (i=0; i<bufferSize; i++)
					{
						char ch = this.PeekBuffer[i];

						// digits are only allowed after first char
						// rest can be in head or tail
						if ((identPart && Char.IsDigit(ch)) ||
						Char.IsLetter(ch) || ch == '_' || ch == '$')
						{
							identPart = true;
							continue;
						}

						// append partial
						ident.Append(this.PeekBuffer, 0, i);
						if (i > 0)
						{
							this.Reader.Flush(i);
						}
						return ident.ToString();
					}

					// append entire buffer
					ident.Append(this.PeekBuffer, 0, bufferSize);
					if (bufferSize > 0)
					{
						this.Reader.Flush(bufferSize);
					}
					bufferSize = this.Reader.Peek(this.PeekBuffer);
				}

				return ident.ToString();
			}

			#endregion Scanning Methods

			#region ITokenizer<JsonTokenType> Members

			public IEnumerable<Token<JsonTokenType>> GetTokens(TextReader reader)
			{
				this.Reader = new BufferedTextReader(reader, this.PeekBuffer.Length);

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
