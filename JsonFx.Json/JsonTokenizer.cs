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
using System.IO;
using System.Text;
using System.Globalization;

namespace JsonFx.Json
{
	/// <summary>
	/// Performs JSON lexical analysis over the input reader.
	/// </summary>
	public class JsonTokenizer : IEnumerator<JsonToken>
	{
		#region Constants

		private const int LongestLiteral = 9;
		internal const string LiteralUndefined = "undefined";
		internal const string LiteralNull = "null";
		internal const string LiteralFalse = "false";
		internal const string LiteralTrue = "true";
		internal const string LiteralNotANumber = "NaN";
		internal const string LiteralPositiveInfinity = "Infinity";
		internal const string LiteralNegativeInfinity = "-Infinity";

		internal const char OperatorNegate = '-';
		internal const char OperatorUnaryPlus = '+';
		internal const char OperatorArrayStart = '[';
		internal const char OperatorArrayEnd = ']';
		internal const char OperatorObjectStart = '{';
		internal const char OperatorObjectEnd = '}';
		internal const char OperatorStringDelim = '"';
		internal const char OperatorStringDelimAlt = '\'';
		internal const char OperatorValueDelim = ',';
		internal const char OperatorNameDelim = ':';
		internal const char OperatorCharEscape = '\\';

		private const string CommentStart = "/*";
		private const string CommentEnd = "*/";
		private const string CommentLine = "//";
		private const string LineEndings = "\r\n";

		// tokenizing errors
		private const string ErrorUnrecognizedToken = "Illegal JSON sequence.";
		private const string ErrorUnterminatedComment = "Unterminated comment block.";
		private const string ErrorUnterminatedObject = "Unterminated JSON object.";
		private const string ErrorUnterminatedArray = "Unterminated JSON array.";
		private const string ErrorUnterminatedString = "Unterminated JSON string.";
		private const string ErrorIllegalNumber = "Illegal JSON number.";

		// parse errors
		private const string ErrorExpectedString = "Expected JSON string.";
		private const string ErrorExpectedObject = "Expected JSON object.";
		private const string ErrorExpectedArray = "Expected JSON array.";
		private const string ErrorExpectedPropertyName = "Expected JSON object property name.";
		private const string ErrorExpectedPropertyNameDelim = "Expected JSON object property name delimiter.";
		private const string ErrorGenericIDictionary = "Types which implement Generic IDictionary<TKey, TValue> also need to implement IDictionary to be deserialized. ({0})";
		private const string ErrorGenericIDictionaryKeys = "Types which implement Generic IDictionary<TKey, TValue> need to have string keys to be deserialized. ({0})";

		#endregion Constants

		#region Fields

		private readonly BufferedTextReader Reader;
		private readonly char[] PeekBuffer = new char[JsonTokenizer.LongestLiteral];
		private readonly bool allowUnquotedKeys;
		private JsonToken current;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="reader">the input reader</param>
		public JsonTokenizer(TextReader reader, bool allowUnquotedKeys)
		{
			this.Reader = new BufferedTextReader(reader);
			this.allowUnquotedKeys = allowUnquotedKeys;
		}

		#endregion Init

		#region Methods

		private JsonToken Tokenize()
		{
			// read next char
			int ch = this.Reader.Read();

			// skip comments and whitespace between tokens
			ch = this.SkipCommentsAndWhitespace(ch);

			switch (ch)
			{
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
				case JsonTokenizer.OperatorNameDelim:
				{
					return JsonToken.NameDelim;
				}
				case JsonTokenizer.OperatorStringDelim:
				case JsonTokenizer.OperatorStringDelimAlt:
				{
					return new JsonToken(JsonTokenType.String, this.ScanString((char)ch));
				}
			}

			if (ch < 0)
			{
				return JsonToken.End;
			}

			// number
			if (Char.IsDigit((char)ch) ||
				(ch == JsonTokenizer.OperatorNegate) ||
				(ch == JsonTokenizer.OperatorUnaryPlus))
			{
				return new JsonToken(JsonTokenType.Number, this.ScanNumber(ch));
			}

			string literal = this.ScanKeywords();
			if (literal != null)
			{
				return new JsonToken(JsonTokenType.Keyword, literal);
			}

			if (this.allowUnquotedKeys)
			{
				return new JsonToken(JsonTokenType.UnquotedName, this.ScanUnquotedKey());
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

			// read second char of comment start
			ch = this.Reader.Read();
			if (ch < 0)
			{
				throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedComment, this.Reader.Position);
			}

			bool isBlockComment = false;
			if (ch == JsonTokenizer.CommentStart[1])
			{
				isBlockComment = true;
			}
			else if (ch != JsonTokenizer.CommentLine[1])
			{
				throw new JsonDeserializationException(JsonTokenizer.ErrorUnrecognizedToken, this.Reader.Position);
			}

			// start reading comment content
			ch = this.Reader.Read();
			if (isBlockComment)
			{
				// store index for unterminated cases
				long commentStart = this.Reader.Position-2L;

				if (this.Reader.Peek() < 0)
				{
					throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
				}

				// skip over everything until reach block comment ending
				while (this.Reader.Read() != JsonTokenizer.CommentEnd[0] ||
					this.Reader.Peek() != JsonTokenizer.CommentEnd[1])
				{
					ch = this.Reader.Read();
					if (this.Reader.Peek() < 0)
					{
						throw new JsonDeserializationException(JsonTokenizer.ErrorUnterminatedComment, commentStart);
					}
				}

				// skip block comment end token
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

		private string ScanNumber(int ch)
		{
			// consume positive signing (as is extraneous)
			if (ch == JsonTokenizer.OperatorUnaryPlus)
			{
				ch = this.Reader.Read();
				if (ch < 0)
				{
					throw new JsonDeserializationException(JsonTokenizer.ErrorUnrecognizedToken, this.Reader.Position);
				}
			}

			// TODO: scan number
			throw new NotImplementedException();
		}

		private string ScanString(char startStringDelim)
		{
			// TODO: scan string
			throw new NotImplementedException();
		}

		private string ScanUnquotedKey()
		{
			// TODO: scan unquoted string
			throw new NotImplementedException();
		}

		private string ScanKeywords()
		{
			int bufferSize = this.Reader.Peek(this.PeekBuffer, 0, this.PeekBuffer.Length);

			// "false" literal
			if (this.IsLiteral(JsonTokenizer.LiteralFalse, this.PeekBuffer, bufferSize))
			{
				return JsonTokenizer.LiteralFalse;
			}

			// "true" literal
			if (this.IsLiteral(JsonTokenizer.LiteralTrue, this.PeekBuffer, bufferSize))
			{
				return JsonTokenizer.LiteralTrue;
			}

			// "null" literal
			if (this.IsLiteral(JsonTokenizer.LiteralNull, this.PeekBuffer, bufferSize))
			{
				return JsonTokenizer.LiteralNull;
			}

			// "NaN" literal
			if (this.IsLiteral(JsonTokenizer.LiteralNotANumber, this.PeekBuffer, bufferSize))
			{
				return JsonTokenizer.LiteralNotANumber;
			}

			// "Infinity" literal
			if (this.IsLiteral(JsonTokenizer.LiteralPositiveInfinity, this.PeekBuffer, bufferSize))
			{
				return JsonTokenizer.LiteralPositiveInfinity;
			}

			// "-Infinity" literal
			if (this.IsLiteral(JsonTokenizer.LiteralNegativeInfinity, this.PeekBuffer, bufferSize))
			{
				return JsonTokenizer.LiteralNegativeInfinity;
			}

			// "undefined" literal
			if (this.IsLiteral(JsonTokenizer.LiteralUndefined, this.PeekBuffer, bufferSize))
			{
				return JsonTokenizer.LiteralUndefined;
			}

			return null;
		}

		private bool IsLiteral(string literal, char[] buffer, int bufferSize)
		{
			int length = literal.Length;

			if (bufferSize < length)
			{
				return false;
			}

			for (int i=0; i<length; i++)
			{
				if (literal[i] != buffer[i])
				{
					return false;
				}
			}

			return true;
		}

		#endregion Scanning Methods

		#region IEnumerator<JsonToken> Members

		/// <summary>
		/// Gets the current token from the input reader.
		/// </summary>
		public JsonToken Current
		{
			get { return this.current; }
		}

		#endregion IEnumerator<JsonToken> Members

		#region IEnumerator Members

		/// <summary>
		/// Gets the current token from the input reader.
		/// </summary>
		object IEnumerator.Current
		{
			get { return this.current; }
		}

		/// <summary>
		/// Moves to the next token, returning if any more tokens are available.
		/// </summary>
		/// <returns></returns>
		public bool MoveNext()
		{
			this.Tokenize();

			return (this.current.TokenType != JsonTokenType.End);
		}

		/// <summary>
		/// Throws NotSupportedException
		/// </summary>
		void IEnumerator.Reset()
		{
			throw new NotSupportedException("JsonTokenizer cannot reset the token sequence.");
		}

		#endregion IEnumerator Members

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
