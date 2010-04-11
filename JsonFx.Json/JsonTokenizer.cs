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

		private const int MinBufferLength = 128;
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
		private const char OperatorDecimal = '.';
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
			int ch = this.Reader.Read();

			// skip comments and whitespace between tokens
			ch = this.SkipCommentsAndWhitespace(ch);

			switch (ch)
			{
				case JsonTokenizer.EndOfSequence:
				{
					return JsonToken.None;
				}
				case JsonTokenizer.OperatorArrayStart:
				{
					return JsonToken.ArrayStart;
				}
				case JsonTokenizer.OperatorArrayEnd:
				{
					return JsonToken.ArrayEnd;
				}
				case JsonTokenizer.OperatorObjectStart:
				{
					return JsonToken.ObjectStart;
				}
				case JsonTokenizer.OperatorObjectEnd:
				{
					return JsonToken.ObjectEnd;
				}
				case JsonTokenizer.OperatorValueDelim:
				{
					return JsonToken.ValueDelim;
				}
				case JsonTokenizer.OperatorPairDelim:
				{
					return JsonToken.PairDelim;
				}
				case JsonTokenizer.OperatorStringDelim:
				case JsonTokenizer.OperatorStringDelimAlt:
				{
					return new JsonToken(JsonTokenType.String, this.ScanString((char)ch));
				}
			}

			// number
			if (Char.IsDigit((char)ch) ||
				Char.IsDigit((char)this.Reader.Peek()) &&
				((ch == JsonTokenizer.OperatorUnaryMinus) || (ch == JsonTokenizer.OperatorUnaryPlus)))
			{
				return new JsonToken(JsonTokenType.Number, this.ScanNumber((char)ch));
			}

			JsonToken token = this.ScanKeywords((char)ch);
			if (token != null)
			{
				return token;
			}

			//if (this.allowUnquotedKeys)
			//{
			//    return new JsonToken(JsonTokenType.UnquotedName, this.ScanUnquotedKey());
			//}

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
			ch = this.Reader.Read();
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
			ch = this.Reader.Read();
			if (isBlockComment)
			{
				// skip over everything until reach block comment ending
				do
				{
					if (this.Reader.Peek() < 0)
					{
						throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
					}
				} while ((ch = this.Reader.Read()) != JsonTokenizer.CommentEnd[0] || this.Reader.Peek() != JsonTokenizer.CommentEnd[1]);

				// move past block comment end token
				ch = this.Reader.Read();
				ch = this.Reader.Read();
			}
			else
			{
				// skip over everything until reach line ending
				while (ch >= 0 && JsonTokenizer.LineEndings.IndexOf((char)ch) < 0)
				{
					ch = this.Reader.Read();
				}
			}

			// skip whitespace
			return this.SkipWhitespace(ch);
		}

		private int SkipWhitespace(int ch)
		{
			while (ch >= 0 && Char.IsWhiteSpace((char)ch))
			{
				ch = this.Reader.Read();
			}

			return ch;
		}

		private ValueType ScanNumber(char ch)
		{
			long numberStart = this.Reader.Position;

			this.PeekBuffer[0] = ch;
			int bufferSize = this.Reader.Peek(this.PeekBuffer, 1, this.PeekBuffer.Length-1);
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

			// integer part
			while ((pos < bufferSize) && Char.IsDigit(this.PeekBuffer[pos]))
			{
				// consume digit
				pos++;
			}

			bool hasDecimal = false;

			if ((pos < bufferSize) && (this.PeekBuffer[pos] == JsonTokenizer.OperatorDecimal))
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
			int exponent = 0;

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

				int expStart = pos;

				// optional minus/plus part
				if (this.PeekBuffer[pos] == JsonTokenizer.OperatorUnaryMinus ||
					this.PeekBuffer[pos] == JsonTokenizer.OperatorUnaryPlus)
				{
					// consume sign
					pos++;
					if (pos >= bufferSize || !Char.IsDigit(this.PeekBuffer[pos]))
					{
						throw new JsonDeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
					}
				}
				else
				{
					if (!Char.IsDigit(this.PeekBuffer[pos]))
					{
						throw new JsonDeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
					}
				}

				// exp part
				while (pos < bufferSize && Char.IsDigit(this.PeekBuffer[pos]))
				{
					// consume digit
					pos++;
				}

				if (!Int32.TryParse(
						new String(this.PeekBuffer, expStart, pos-expStart),
						NumberStyles.Integer,
						NumberFormatInfo.InvariantInfo,
						out exponent))
				{
					throw new JsonDeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
				}
			}

			// at this point, we have the full number string and know its characteristics
			string numberString = new String(this.PeekBuffer, start, pos-start);
			this.Reader.Flush(pos-1);

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
					return (int)number;
				}

				if (number >= Int64.MinValue && number <= Int64.MaxValue)
				{
					// more flexible
					return (long)number;
				}

				// most flexible
				return number;
			}
			else
			{
				// Floating Point value
				// native EcmaScript number (IEEE-754)
				double number;
				if (!Double.TryParse(
					 numberString,
					 NumberStyles.Float,
					 NumberFormatInfo.InvariantInfo,
					 out number))
				{
					throw new JsonDeserializationException(JsonTokenizer.ErrorIllegalNumber, numberStart);
				}

				return number;
			}
		}

		private string ScanString(char stringDelim)
		{
			// store for unterminated case
			long stringStart = this.Reader.Position-1L;

			StringBuilder builder = new StringBuilder(this.PeekBuffer.Length);

			// fill buffer
			int count = this.Reader.Peek(this.PeekBuffer, 0, this.PeekBuffer.Length);
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
						return builder.ToString();
					}

					//if (Char.IsControl(this.PeekBuffer[i]))
					//{
					//    // control characters not allowed within strings
					//    throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedString, stringStart);
					//}

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
					count = this.Reader.Peek(this.PeekBuffer, 0, this.PeekBuffer.Length);
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
				if (count > 1)
				{
					builder.Append(this.PeekBuffer, start, count-1);
				}

				// refill buffer
				count = this.Reader.Peek(this.PeekBuffer, 0, this.PeekBuffer.Length);
			}

			// reached END before string delim
			throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedString, stringStart);
		}

		private JsonToken ScanKeywords(char ch)
		{
			this.PeekBuffer[0] = ch;
			int bufferSize = this.Reader.Peek(this.PeekBuffer, 1, this.PeekBuffer.Length-1);

			// "false" keyword
			if (this.IsKeyword(JsonTokenizer.KeywordFalse, this.PeekBuffer, 0, bufferSize))
			{
				this.Reader.Flush(JsonTokenizer.KeywordFalse.Length-1);
				return JsonToken.False;
			}

			// "true" keyword
			if (this.IsKeyword(JsonTokenizer.KeywordTrue, this.PeekBuffer, 0, bufferSize))
			{
				this.Reader.Flush(JsonTokenizer.KeywordTrue.Length-1);
				return JsonToken.True;
			}

			// "null" keyword
			if (this.IsKeyword(JsonTokenizer.KeywordNull, this.PeekBuffer, 0, bufferSize))
			{
				this.Reader.Flush(JsonTokenizer.KeywordNull.Length-1);
				return JsonToken.Null;
			}

			// numbers can be modified with unary +/- operators
			int unaryOpShift = 0;
			if ((ch == JsonTokenizer.OperatorUnaryMinus) || (ch == JsonTokenizer.OperatorUnaryPlus))
			{
				unaryOpShift++;
			}

			// "NaN" keyword
			if (this.IsKeyword(JsonTokenizer.KeywordNaN, this.PeekBuffer, unaryOpShift, bufferSize-unaryOpShift))
			{
				this.Reader.Flush(unaryOpShift+JsonTokenizer.KeywordNaN.Length-1);
				return JsonToken.NaN;
			}

			// "Infinity" keyword
			if (this.IsKeyword(JsonTokenizer.KeywordInfinity, this.PeekBuffer, unaryOpShift, bufferSize-unaryOpShift))
			{
				this.Reader.Flush(unaryOpShift+JsonTokenizer.KeywordInfinity.Length-1);
				if (ch == JsonTokenizer.OperatorUnaryMinus)
				{
					return JsonToken.NegativeInfinity;
				}
				return JsonToken.PositiveInfinity;
			}

			// "undefined" keyword
			if (this.IsKeyword(JsonTokenizer.KeywordUndefined, this.PeekBuffer, 0, bufferSize))
			{
				this.Reader.Flush(JsonTokenizer.KeywordUndefined.Length-1);
				return JsonToken.Undefined;
			}

			return null;
		}

		private bool IsKeyword(string literal, char[] buffer, int index, int bufferSize)
		{
			int length = literal.Length;

			if (bufferSize < length)
			{
				return false;
			}

			for (int i=0; i<length; i++)
			{
				if (literal[i] != buffer[i+index])
				{
					return false;
				}
			}

			return true;
		}

		private string ScanUnquotedKey()
		{
			// TODO: scan unquoted string
			throw new NotImplementedException();
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
