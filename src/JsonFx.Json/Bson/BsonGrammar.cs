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

using JsonFx.Serialization;

namespace JsonFx.Bson
{
	/// <summary>
	/// Formal language of tokens and symbols for BSON
	/// </summary>
	/// <remarks>
	/// http://bsonspec.org/
	/// http://codebetter.com/blogs/karlseguin/archive/2010/03/05/bson-serialization.aspx
	/// </remarks>
	internal class BsonGrammar
	{
		#region Reusable Tokens

		public static readonly Token<BsonTokenType> TokenNone = new Token<BsonTokenType>(BsonTokenType.None);

		public static readonly Token<BsonTokenType> TokenDocumentBegin = new Token<BsonTokenType>(BsonTokenType.DocumentBegin);
		public static readonly Token<BsonTokenType> TokenDocumentEnd = new Token<BsonTokenType>(BsonTokenType.DocumentEnd);

		public static readonly Token<BsonTokenType> TokenFalse = new Token<BsonTokenType>(BsonTokenType.Boolean, false);
		public static readonly Token<BsonTokenType> TokenTrue = new Token<BsonTokenType>(BsonTokenType.Boolean, true);

		public static Token<BsonTokenType> TokenElementType(BsonElementType value)
		{
			return new Token<BsonTokenType>(BsonTokenType.ElementType, value);
		}

		public static Token<BsonTokenType> TokenBinarySubtype(BsonBinarySubtype value)
		{
			return new Token<BsonTokenType>(BsonTokenType.BinarySubtype, value);
		}

		public static Token<BsonTokenType> TokenInt32(int value)
		{
			return new Token<BsonTokenType>(BsonTokenType.Int32, value);
		}

		public static Token<BsonTokenType> TokenInt64(long value)
		{
			return new Token<BsonTokenType>(BsonTokenType.Int64, value);
		}

		public static Token<BsonTokenType> TokenDouble(double value)
		{
			return new Token<BsonTokenType>(BsonTokenType.Double, value);
		}

		public static Token<BsonTokenType> TokenString(string value)
		{
			return new Token<BsonTokenType>(BsonTokenType.String, value);
		}

		public static Token<BsonTokenType> TokenCString(string value)
		{
			return new Token<BsonTokenType>(BsonTokenType.CString, value);
		}

		public static Token<BsonTokenType> TokenUtcDateTime(DateTime value)
		{
			return new Token<BsonTokenType>(BsonTokenType.UtcDateTime, value);
		}

		public static Token<BsonTokenType> TokenByteArray(byte[] value)
		{
			return new Token<BsonTokenType>(BsonTokenType.ByteArray, value);
		}

		#endregion Reusable Tokens
	}
}
