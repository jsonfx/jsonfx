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
using JsonFx.Model;
using JsonFx.Serialization;
using JsonFx.Utils;

namespace JsonFx.Json
{
	public partial class JsonReader
	{
		/// <summary>
		/// Generates a sequence of tokens from JSON text
		/// </summary>
		public class JsonTokenizer : ITextTokenizer<ModelTokenType>
		{
			#region NeedsValueDelim

			private enum NeedsValueDelim
			{
				/// <summary>
				/// It is an error for the next token to be a value delim
				/// </summary>
				Forbidden,

				/// <summary>
				/// Forbidden but differentiates between empty array/object and just written
				/// </summary>
				CurrentIsDelim,

				/// <summary>
				/// It is an error for the next token to NOT be a value delim
				/// </summary>
				Required,

			}

			#endregion NeedsValueDelim

			#region Constants

			// tokenizing errors
			private const string ErrorUnrecognizedToken = "Illegal JSON sequence";
			private const string ErrorUnterminatedComment = "Unterminated comment block";
			private const string ErrorUnterminatedString = "Unterminated JSON string";
			private const string ErrorIllegalNumber = "Illegal JSON number";
			private const string ErrorMissingValueDelim = "Missing value delimiter";
			private const string ErrorExtraValueDelim = "Extraneous value delimiter";

			private const int DefaultBufferSize = 0x20;

			#endregion Constants

			#region Fields

			private ITextStream Scanner = TextReaderStream.Null;

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

			/// <summary>
			/// Gets a token sequence from the scanner stream
			/// </summary>
			/// <param name="scanner"></param>
			/// <returns></returns>
			protected IEnumerable<Token<ModelTokenType>> GetTokens(ITextStream scanner)
			{
				if (scanner == null)
				{
					throw new ArgumentNullException("scanner");
				}

				// store for external access to Index/Line/Column
				this.Scanner = scanner;

				int depth = 0;
				NeedsValueDelim needsValueDelim = NeedsValueDelim.Forbidden;

				while (true)
				{
					// skip comments and whitespace between tokens
					JsonTokenizer.SkipCommentsAndWhitespace(scanner);

					if (scanner.IsCompleted)
					{
						this.Scanner = StringStream.Null;
						scanner.Dispose();
						yield break;
					}

					bool hasUnaryOp = false;

					char ch = scanner.Peek();
					switch (ch)
					{
						case JsonGrammar.OperatorArrayBegin:
						{
							scanner.Pop();
							if (needsValueDelim == NeedsValueDelim.Required)
							{
								throw new DeserializationException(JsonTokenizer.ErrorMissingValueDelim, scanner.Index, scanner.Line, scanner.Column);
							}

							yield return ModelGrammar.TokenArrayBeginUnnamed;
							depth++;
							needsValueDelim = NeedsValueDelim.Forbidden;
							continue;
						}
						case JsonGrammar.OperatorArrayEnd:
						{
							scanner.Pop();
							if (needsValueDelim == NeedsValueDelim.CurrentIsDelim)
							{
								throw new DeserializationException(JsonTokenizer.ErrorExtraValueDelim, scanner.Index, scanner.Line, scanner.Column);
							}

							yield return ModelGrammar.TokenArrayEnd;

							// resetting at zero allows streaming mode
							depth--;
							needsValueDelim = (depth > 0) ? NeedsValueDelim.Required : NeedsValueDelim.Forbidden;
							continue;
						}
						case JsonGrammar.OperatorObjectBegin:
						{
							scanner.Pop();
							if (needsValueDelim == NeedsValueDelim.Required)
							{
								throw new DeserializationException(JsonTokenizer.ErrorMissingValueDelim, scanner.Index, scanner.Line, scanner.Column);
							}

							yield return ModelGrammar.TokenObjectBeginUnnamed;
							depth++;
							needsValueDelim = NeedsValueDelim.Forbidden;
							continue;
						}
						case JsonGrammar.OperatorObjectEnd:
						{
							scanner.Pop();
							if (needsValueDelim == NeedsValueDelim.CurrentIsDelim)
							{
								throw new DeserializationException(JsonTokenizer.ErrorExtraValueDelim, scanner.Index, scanner.Line, scanner.Column);
							}

							yield return ModelGrammar.TokenObjectEnd;

							// resetting at zero allows streaming mode
							depth--;
							needsValueDelim = (depth > 0) ? NeedsValueDelim.Required : NeedsValueDelim.Forbidden;
							continue;
						}
						case JsonGrammar.OperatorStringDelim:
						case JsonGrammar.OperatorStringDelimAlt:
						{
							if (needsValueDelim == NeedsValueDelim.Required)
							{
								throw new DeserializationException(JsonTokenizer.ErrorMissingValueDelim, scanner.Index, scanner.Line, scanner.Column);
							}

							string value = JsonTokenizer.ScanString(scanner);

							JsonTokenizer.SkipCommentsAndWhitespace(scanner);
							if (scanner.Peek() == JsonGrammar.OperatorPairDelim)
							{
								scanner.Pop();
								yield return ModelGrammar.TokenProperty(new DataName(value));
								needsValueDelim = NeedsValueDelim.Forbidden;
								continue;
							}

							yield return ModelGrammar.TokenPrimitive(value);
							needsValueDelim = NeedsValueDelim.Required;
							continue;
						}
						case JsonGrammar.OperatorUnaryMinus:
						case JsonGrammar.OperatorUnaryPlus:
						{
							hasUnaryOp = true;
							break;
						}
						case JsonGrammar.OperatorValueDelim:
						{
							scanner.Pop();

							if (needsValueDelim != NeedsValueDelim.Required)
							{
								throw new DeserializationException(JsonTokenizer.ErrorExtraValueDelim, scanner.Index, scanner.Line, scanner.Column);
							}

							needsValueDelim = NeedsValueDelim.CurrentIsDelim;
							continue;
						}
						case JsonGrammar.OperatorPairDelim:
						{
							throw new DeserializationException(JsonTokenizer.ErrorUnrecognizedToken, scanner.Index+1, scanner.Line, scanner.Column);
						}
					}

					if (needsValueDelim == NeedsValueDelim.Required)
					{
						throw new DeserializationException(JsonTokenizer.ErrorMissingValueDelim, scanner.Index, scanner.Line, scanner.Column);
					}

					// scan for numbers
					Token<ModelTokenType> token = JsonTokenizer.ScanNumber(scanner);
					if (token != null)
					{
						yield return token;
						needsValueDelim = NeedsValueDelim.Required;
						continue;
					}

					// hold for Infinity, clear for others
					if (!hasUnaryOp)
					{
						ch = default(char);
					}

					// store for unterminated cases
					long strPos = scanner.Index+1;
					int strLine = scanner.Line;
					int strCol = scanner.Column;

					// scan for identifiers, then check if they are keywords
					string ident = JsonTokenizer.ScanIdentifier(scanner);
					if (!String.IsNullOrEmpty(ident))
					{
						token = JsonTokenizer.ScanKeywords(scanner, ident, ch, out needsValueDelim);
						if (token != null)
						{
							yield return token;
							continue;
						}
					}

					throw new DeserializationException(JsonTokenizer.ErrorUnrecognizedToken, strPos, strLine, strCol);
				}
			}

			private static void SkipCommentsAndWhitespace(ITextStream scanner)
			{
				while (true)
				{
					// skip leading and trailing whitespace
					JsonTokenizer.SkipWhitespace(scanner);

					// check for block and line comments
					if (scanner.IsCompleted || scanner.Peek() != JsonGrammar.OperatorCommentBegin[0])
					{
						return;
					}

					// read first char of comment start
					scanner.Pop();

					// store for unterminated case
					long commentStart = scanner.Index;
					int commentCol = scanner.Column;
					int commentLine = scanner.Line;

					if (scanner.IsCompleted)
					{
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
					}

					// peek second char of comment start
					char ch = scanner.Peek();
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
								scanner.Pop();

								if (scanner.IsCompleted)
								{
									throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
								}
							} while (scanner.Peek() != JsonGrammar.OperatorCommentEnd[0]);

							scanner.Pop();

							if (scanner.IsCompleted)
							{
								throw new DeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart, commentLine, commentCol);
							}

							if (scanner.Peek() == JsonGrammar.OperatorCommentEnd[1])
							{
								// move past block comment end token
								scanner.Pop();
								break;
							}
						}
					}
					else
					{
						// skip over everything until reach line ending or end of input
						do
						{
							scanner.Pop();
							ch = scanner.Peek();
						} while (!scanner.IsCompleted && ('\r' != ch) && ('\n' != ch));
					}
				}
			}

			private static void SkipWhitespace(ITextStream scanner)
			{
				while (!scanner.IsCompleted && CharUtility.IsWhiteSpace(scanner.Peek()))
				{
					scanner.Pop();
				}
			}

			private static Token<ModelTokenType> ScanNumber(ITextStream scanner)
			{
				// store for error cases
				long numPos = scanner.Index+1;
				int numLine = scanner.Line;
				int numCol = scanner.Column;

				scanner.BeginChunk();

				char ch = scanner.Peek();
				bool isNeg = false;
				if (ch == JsonGrammar.OperatorUnaryPlus)
				{
					// consume positive signing (as is extraneous)
					scanner.Pop();
					ch = scanner.Peek();

					// reset buffering
					scanner.BeginChunk();
				}
				else if (ch == JsonGrammar.OperatorUnaryMinus)
				{
					// optional minus part
					scanner.Pop();
					ch = scanner.Peek();
					isNeg = true;
				}

				if (!CharUtility.IsDigit(ch) &&
					ch != JsonGrammar.OperatorDecimalPoint)
				{
					// possibly "-Infinity"
					scanner.EndChunk();
					return null;
				}

				// integer part
				while (!scanner.IsCompleted && CharUtility.IsDigit(ch))
				{
					// consume digit
					scanner.Pop();
					ch = scanner.Peek();
				}

				bool hasDecimal = false;

				if (!scanner.IsCompleted && (ch == JsonGrammar.OperatorDecimalPoint))
				{
					// consume decimal
					scanner.Pop();
					ch = scanner.Peek();

					// fraction part
					while (!scanner.IsCompleted && CharUtility.IsDigit(ch))
					{
						// consume digit
						scanner.Pop();
						ch = scanner.Peek();
						hasDecimal = true;
					}

					if (!hasDecimal)
					{
						// fractional digits required when '.' present
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
					}
				}

				// note the number of significant digits
				int precision = scanner.ChunkSize;
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
				if (!scanner.IsCompleted && (ch == 'e' || ch == 'E'))
				{
					// consume 'e'
					scanner.Pop();
					ch = scanner.Peek();

					// optional minus/plus part
					if (!scanner.IsCompleted &&
						ch == JsonGrammar.OperatorUnaryMinus ||
						ch == JsonGrammar.OperatorUnaryPlus)
					{
						// consume sign
						scanner.Pop();
						ch = scanner.Peek();
					}

					// exp part
					while (!scanner.IsCompleted && CharUtility.IsDigit(ch))
					{
						// consume digit
						scanner.Pop();
						ch = scanner.Peek();

						hasExponent = true;
					}

					if (!hasExponent)
					{
						// exponent digits required when 'e' present
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
					}
				}

				// specifically check for 0x-style hex numbers
				if (!scanner.IsCompleted && CharUtility.IsLetter(ch))
				{
					throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
				}

				// by this point, we have the full number string and know its characteristics

				string buffer = scanner.EndChunk();
				if (!hasDecimal && !hasExponent && precision < 19)
				{
					// Integer value
					decimal number = 0;
					try{
					    number = Decimal.Parse(
    						buffer,
    						NumberStyles.Integer,
    						NumberFormatInfo.InvariantInfo);
					}catch(Exception){
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
					}

					if (number >= Int32.MinValue && number <= Int32.MaxValue)
					{
						// int most common
						return ModelGrammar.TokenPrimitive((int)number);
					}

					if (number >= Int64.MinValue && number <= Int64.MaxValue)
					{
						// long more flexible
						return ModelGrammar.TokenPrimitive((long)number);
					}

					// decimal most flexible
					return ModelGrammar.TokenPrimitive(number);
				}
				else
				{
					// Floating Point value
					double number;
					try{
					    number = Double.Parse(
						    buffer,
						    NumberStyles.Float,
						    NumberFormatInfo.InvariantInfo);
					}catch(Exception){
						throw new DeserializationException(JsonTokenizer.ErrorIllegalNumber, numPos, numLine, numCol);
					}

					// native EcmaScript number (IEEE-754)
					return ModelGrammar.TokenPrimitive(number);
				}
			}

			private static string ScanString(ITextStream scanner)
			{
				// store for unterminated cases
				long strPos = scanner.Index+1;
				int strLine = scanner.Line;
				int strCol = scanner.Column;

				char stringDelim = scanner.Peek();
				scanner.Pop();
				char ch = scanner.Peek();

				// start chunking
				scanner.BeginChunk();
				StringBuilder buffer = new StringBuilder(JsonTokenizer.DefaultBufferSize);
				
				while (true)
				{
					// look ahead
					if (scanner.IsCompleted ||
						CharUtility.IsControl(ch) && ch != '\t')
					{
						// reached end or line break before string delim
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, strPos, strLine, strCol);
					}

					// check each character for ending delim
					if (ch == stringDelim)
					{
						// end chunking
						scanner.EndChunk(buffer);

						// flush closing delim
						scanner.Pop();

						// output string
						return buffer.ToString();
					}

					if (ch != JsonGrammar.OperatorCharEscape)
					{
						// accumulate
						scanner.Pop();
						ch = scanner.Peek();
						continue;
					}

					// pause chunking to replace escape char
					scanner.EndChunk(buffer);

					// flush escape char
					scanner.Pop();
					ch = scanner.Peek();

					if (scanner.IsCompleted ||
						CharUtility.IsControl(ch) && ch != '\t')
					{
						// reached end or line break before string delim
						throw new DeserializationException(JsonTokenizer.ErrorUnterminatedString, strPos, strLine, strCol);
					}

					// begin decode
					switch (ch)
					{
						case '0':
						{
							// consume and do not allow NULL char '\0'
							// causes CStrings to terminate
							scanner.Pop();
							ch = scanner.Peek();
							break;
						}
						case 'b':
						{
							// backspace
							buffer.Append('\b');
							scanner.Pop();
							ch = scanner.Peek();
							break;
						}
						case 'f':
						{
							// formfeed
							buffer.Append('\f');
							scanner.Pop();
							ch = scanner.Peek();
							break;
						}
						case 'n':
						{
							// newline
							buffer.Append('\n');
							scanner.Pop();
							ch = scanner.Peek();
							break;
						}
						case 'r':
						{
							// carriage return
							buffer.Append('\r');
							scanner.Pop();
							ch = scanner.Peek();
							break;
						}
						case 't':
						{
							// tab
							buffer.Append('\t');
							scanner.Pop();
							ch = scanner.Peek();
							break;
						}
						case 'u':
						{
							// Unicode escape sequence
							// e.g. (c) => "\u00A9"
							const int UnicodeEscapeLength = 4;

							scanner.Pop();
							ch = scanner.Peek();

							string escapeSeq = String.Empty;
							for (int i=UnicodeEscapeLength; !scanner.IsCompleted && CharUtility.IsHexDigit(ch) && (i > 0); i--)
							{
								escapeSeq += ch;
								scanner.Pop();
								ch = scanner.Peek();
							}

							// unicode ordinal
							int utf16 = 0;
							bool parsed = true;
							try{
						        utf16 = Int32.Parse(
    								escapeSeq,
    								NumberStyles.AllowHexSpecifier,
    								NumberFormatInfo.InvariantInfo);
    						}catch(Exception){ parsed = false; };
							if (escapeSeq.Length == UnicodeEscapeLength && parsed)
							{
								buffer.Append(CharUtility.ConvertFromUtf32(utf16));
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
							buffer.Append(ch);
							scanner.Pop();
							ch = scanner.Peek();
							break;
						}
					}

					// resume chunking
					scanner.BeginChunk();
				}
			}

			private static Token<ModelTokenType> ScanKeywords(ITextStream scanner, string ident, char unary, out NeedsValueDelim needsValueDelim)
			{
				needsValueDelim = NeedsValueDelim.Required;

				switch (ident)
				{
					case JsonGrammar.KeywordFalse:
					{
						if (unary != default(char))
						{
							return null;
						}

						return ModelGrammar.TokenFalse;
					}
					case JsonGrammar.KeywordTrue:
					{
						if (unary != default(char))
						{
							return null;
						}

						return ModelGrammar.TokenTrue;
					}
					case JsonGrammar.KeywordNull:
					{
						if (unary != default(char))
						{
							return null;
						}

						return ModelGrammar.TokenNull;
					}
					case JsonGrammar.KeywordNaN:
					{
						if (unary != default(char))
						{
							return null;
						}

						return ModelGrammar.TokenNaN;
					}
					case JsonGrammar.KeywordInfinity:
					{
						if (unary == default(char) || unary == JsonGrammar.OperatorUnaryPlus)
						{
							return ModelGrammar.TokenPositiveInfinity;
						}

						if (unary == JsonGrammar.OperatorUnaryMinus)
						{
							return ModelGrammar.TokenNegativeInfinity;
						}

						return null;
					}
					case JsonGrammar.KeywordUndefined:
					{
						if (unary != default(char))
						{
							return null;
						}

						return ModelGrammar.TokenNull;
					}
				}

				if (unary != default(char))
				{
					ident = Char.ToString(unary)+ident;
				}

				JsonTokenizer.SkipCommentsAndWhitespace(scanner);
				if (scanner.Peek() == JsonGrammar.OperatorPairDelim)
				{
					scanner.Pop();

					needsValueDelim = NeedsValueDelim.Forbidden;
					return ModelGrammar.TokenProperty(new DataName(ident));
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
			private static string ScanIdentifier(ITextStream scanner)
			{
				bool identPart = false;

				scanner.BeginChunk();
				while (true)
				{
					char ch = scanner.Peek();

					// digits are only allowed after first char
					// rest can be in head or tail
					if ((identPart && CharUtility.IsDigit(ch)) ||
						CharUtility.IsLetter(ch) || (ch == '_') || (ch == '$'))
					{
						identPart = true;
						scanner.Pop();
						ch = scanner.Peek();
						continue;
					}

					// get ident string
					return scanner.EndChunk();
				}
			}

			#endregion Scanning Methods

			#region ITextTokenizer<DataTokenType> Members

			/// <summary>
			/// Gets a token sequence from the TextReader
			/// </summary>
			/// <param name="reader"></param>
			/// <returns></returns>
			public IEnumerable<Token<ModelTokenType>> GetTokens(TextReader reader)
			{
				// buffer the output so multiple passes over the results can access it
				// otherwise second pass would result in empty list since stream is used up
				return new SequenceBuffer<Token<ModelTokenType>>(this.GetTokens(new TextReaderStream(reader)));
			}

			/// <summary>
			/// Gets a token sequence from the string
			/// </summary>
			/// <param name="text"></param>
			/// <returns></returns>
			public IEnumerable<Token<ModelTokenType>> GetTokens(string text)
			{
				// buffer the output so multiple passes over the results can access it
				// otherwise second pass would result in empty list since stream is used up
				return new SequenceBuffer<Token<ModelTokenType>>(this.GetTokens(new StringStream(text)));
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
					this.Scanner.Dispose();
				}
			}

			#endregion IDisposable Members
		}
	}
}
