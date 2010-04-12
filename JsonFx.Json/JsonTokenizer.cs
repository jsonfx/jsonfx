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

namespace JsonFx.Json
{
	/// <summary>
	/// Performs JSON lexical analysis over the input reader.
	/// </summary>
	public class JsonTokenizer : IEnumerable<JsonToken>
	{
		#region Constants

		private const int MinBufferLength = 128; // must hold longest number sequence
		private const int DefaultBufferSize = 1024;

		private const string KeywordUndefined = "undefined";
		private const string KeywordNull = "null";
		private const string KeywordFalse = "false";
		private const string KeywordTrue = "true";
		private const string KeywordNaN = "NaN";
		private const string KeywordInfinity = "Infinity";

		private const int EndOfSequence = -1;

		private const char OperatorUnaryMinus = '-';
		private const char OperatorUnaryPlus = '+';
		private const char OperatorDecimalPoint = '.';
		private const char OperatorArrayStart = '[';
		private const char OperatorArrayEnd = ']';
		private const char OperatorObjectStart = '{';
		private const char OperatorObjectEnd = '}';
		private const char OperatorStringDelim = '"';
		private const char OperatorStringDelimAlt = '\'';
		private const char OperatorValueDelim = ',';
		private const char OperatorPairDelim = ':';
		private const char OperatorCharEscape = '\\';

		private const string CommentStart = "/*";
		private const string CommentEnd = "*/";
		private const string CommentLine = "//";
		private const string LineEndings = "\r\n";

		// tokenizing errors
		private const string ErrorUnrecognizedToken = "Illegal JSON sequence.";
		private const string ErrorUnterminatedComment = "Unterminated comment block.";
		private const string ErrorUnterminatedString = "Unterminated JSON string.";
		private const string ErrorIllegalNumber = "Illegal JSON number.";

		// parse errors
		//private const string ErrorUnterminatedObject = "Unterminated JSON object.";
		//private const string ErrorUnterminatedArray = "Unterminated JSON array.";
		//private const string ErrorExpectedString = "Expected JSON string.";
		//private const string ErrorExpectedObject = "Expected JSON object.";
		//private const string ErrorExpectedArray = "Expected JSON array.";
		//private const string ErrorExpectedPropertyName = "Expected JSON object property name.";
		//private const string ErrorExpectedPropertyPairDelim = "Expected JSON object property name/value delimiter.";
		//private const string ErrorGenericIDictionary = "Types which implement Generic IDictionary<TKey, TValue> also need to implement IDictionary to be deserialized. ({0})";
		//private const string ErrorGenericIDictionaryKeys = "Types which implement Generic IDictionary<TKey, TValue> need to have string keys to be deserialized. ({0})";

		#endregion Constants

		#region Fields

		private readonly BufferedTextReader Reader;
		private readonly char[] PeekBuffer;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="reader">input reader</param>
		public JsonTokenizer(TextReader reader)
			: this(reader, JsonTokenizer.DefaultBufferSize)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="reader">input reader</param>
		/// <param name="bufferSize">read buffer size</param>
		public JsonTokenizer(TextReader reader, int bufferSize)
		{
			if (bufferSize < JsonTokenizer.MinBufferLength)
			{
				bufferSize = JsonTokenizer.MinBufferLength;
			}

			this.PeekBuffer = new char[bufferSize];
			this.Reader = new BufferedTextReader(reader, bufferSize);
		}

		#endregion Init

		#region Methods

		/// <summary>
		/// Returns the next JSON token in the sequence.
		/// </summary>
		/// <returns></returns>
		private JsonToken Tokenize()
		{
			// read next char
			int ch = this.Reader.Peek();

			// skip comments and whitespace between tokens
			ch = this.SkipCommentsAndWhitespace(ch);

			bool unaryOp = false;
			switch (ch)
			{
				case JsonTokenizer.EndOfSequence:
				{
					return JsonToken.None;
				}
				case JsonTokenizer.OperatorArrayStart:
				{
					this.Reader.Flush(1);
					return JsonToken.ArrayStart;
				}
				case JsonTokenizer.OperatorArrayEnd:
				{
					this.Reader.Flush(1);
					return JsonToken.ArrayEnd;
				}
				case JsonTokenizer.OperatorObjectStart:
				{
					this.Reader.Flush(1);
					return JsonToken.ObjectStart;
				}
				case JsonTokenizer.OperatorObjectEnd:
				{
					this.Reader.Flush(1);
					return JsonToken.ObjectEnd;
				}
				case JsonTokenizer.OperatorValueDelim:
				{
					this.Reader.Flush(1);
					return JsonToken.ValueDelim;
				}
				case JsonTokenizer.OperatorPairDelim:
				{
					this.Reader.Flush(1);
					return JsonToken.PairDelim;
				}
				case JsonTokenizer.OperatorStringDelim:
				case JsonTokenizer.OperatorStringDelimAlt:
				{
					return this.ScanString();
				}
				case JsonTokenizer.OperatorUnaryMinus:
				case JsonTokenizer.OperatorUnaryPlus:
				{
					unaryOp = true;
					break;
				}
			}

			// scan for numbers
			JsonToken token = this.ScanNumber();
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

			throw new JsonDeserializationException(JsonTokenizer.ErrorUnrecognizedToken, this.Reader.Position);
		}

		private int SkipCommentsAndWhitespace(int ch)
		{
			ch = this.SkipWhitespace(ch);

			// skip block and line comments
			if (ch != JsonTokenizer.CommentStart[0])
			{
				return ch;
			}

			// store index for unterminated case
			long commentStart = this.Reader.Position;

			// read second char of comment start
			ch = this.Reader.NextPeek();
			if (ch < 0)
			{
				throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
			}

			bool isBlockComment;
			if (ch == JsonTokenizer.CommentStart[1])
			{
				isBlockComment = true;
			}
			else if (ch == JsonTokenizer.CommentLine[1])
			{
				isBlockComment = false;
			}
			else
			{
				throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
			}

			// start reading comment content
			if (isBlockComment)
			{
				// skip over everything until reach block comment ending
				while (true)
				{
					while ((ch = this.Reader.NextPeek()) != JsonTokenizer.CommentEnd[0])
					{
						if (ch < 0)
						{
							throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
						}
					}

					if ((ch = this.Reader.NextPeek()) == JsonTokenizer.CommentEnd[1])
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
				while ((ch = this.Reader.NextPeek()) >= 0 && JsonTokenizer.LineEndings.IndexOf((char)ch) < 0);
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

		private JsonToken ScanNumber()
		{
			long numberStart = this.Reader.Position;

			int bufferSize = this.Reader.Peek(this.PeekBuffer);
			int start = 0;
			int pos = 0;

			if (this.PeekBuffer[pos] == JsonTokenizer.OperatorUnaryPlus)
			{
				// consume positive signing (as is extraneous)
				start++;
				pos++;
			}
			else if (this.PeekBuffer[pos] == JsonTokenizer.OperatorUnaryMinus)
			{
				// optional minus part
				pos++;
			}

			if (!Char.IsDigit(this.PeekBuffer[pos]) &&
				this.PeekBuffer[pos] != JsonTokenizer.DefaultBufferSize)
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

			if ((pos < bufferSize) && (this.PeekBuffer[pos] == JsonTokenizer.OperatorDecimalPoint))
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
			if (this.PeekBuffer[start] == JsonTokenizer.OperatorUnaryMinus)
			{
				precision--;
			}

			if (precision < 1)
			{
				throw new JsonDeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
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
					throw new JsonDeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
				}

				// optional minus/plus part
				if (this.PeekBuffer[pos] == JsonTokenizer.OperatorUnaryMinus ||
					this.PeekBuffer[pos] == JsonTokenizer.OperatorUnaryPlus)
				{
					// consume sign
					pos++;
				}

				if (pos >= bufferSize || !Char.IsDigit(this.PeekBuffer[pos]))
				{
					throw new JsonDeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
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
					throw new JsonDeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
				}

				if (number >= Int32.MinValue && number <= Int32.MaxValue)
				{
					// most common
					return new JsonToken(JsonTokenType.Number, (int)number);
				}

				if (number >= Int64.MinValue && number <= Int64.MaxValue)
				{
					// more flexible
					return new JsonToken(JsonTokenType.Number, (long)number);
				}

				// most flexible
				return new JsonToken(JsonTokenType.Number, number);
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
					throw new JsonDeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
				}

				// native EcmaScript number (IEEE-754)
				return new JsonToken(JsonTokenType.Number, number);
			}
		}

		private JsonToken ScanString()
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
					// check each character for ending delim
					if (this.PeekBuffer[i] == stringDelim)
					{
						// append final segment
						builder.Append(this.PeekBuffer, start, i-start);

						// flush string and closing delim
						this.Reader.Flush(i+1);

						// output string
						return new JsonToken(JsonTokenType.String, builder.ToString());
					}

					if (this.PeekBuffer[i] != JsonTokenizer.OperatorCharEscape)
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
						throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedString, stringStart);
					}

					// decode
					switch (this.PeekBuffer[i])
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
							builder.Append(this.PeekBuffer[i]);
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
			throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedString, stringStart);
		}

		private JsonToken ScanKeywords(string ident, int unary)
		{
			switch (ident)
			{
				case JsonTokenizer.KeywordFalse:
				{
					if (unary >= 0)
					{
						return null;
					}
					return JsonToken.False;
				}
				case JsonTokenizer.KeywordTrue:
				{
					if (unary < 0)
					{
						return JsonToken.True;
					}

					return null;
				}
				case JsonTokenizer.KeywordNull:
				{
					if (unary < 0)
					{
						return JsonToken.Null;
					}

					return null;
				}
				case JsonTokenizer.KeywordNaN:
				{
					if (unary < 0)
					{
						return JsonToken.NaN;
					}

					return null;
				}
				case JsonTokenizer.KeywordInfinity:
				{
					if (unary < 0 || unary == JsonTokenizer.OperatorUnaryPlus)
					{
						return JsonToken.PositiveInfinity;
					}
					
					if (unary == JsonTokenizer.OperatorUnaryMinus)
					{
						return JsonToken.NegativeInfinity;
					}

					return null;
				}
				case JsonTokenizer.KeywordUndefined:
				{
					if (unary < 0)
					{
						return JsonToken.Undefined;
					}

					return null;
				}
			}

			if (unary < 0)
			{
				return new JsonToken(JsonTokenType.Identifier, ident);
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

		#region IEnumerable<JsonToken> Members

		public IEnumerator<JsonToken> GetEnumerator()
		{
			while (true)
			{
				JsonToken token = this.Tokenize();
				if (token.TokenType == JsonTokenType.None)
				{
					yield break;
				}
				yield return token;
			};
		}

		#endregion IEnumerable<JsonToken> Members

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion IEnumerable Members

		#region IDisposable Members

		/// <summary>
		/// Disposes the underlying input reader.
		/// </summary>
		public void Dispose()
		{
			this.Reader.Dispose();
		}

		#endregion IDisposable Members
	}
}
