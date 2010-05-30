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

namespace JsonFx.Json
{
	/// <summary>
	/// The formal language of tokens and symbols for JSON
	/// </summary>
	internal class JsonGrammar
	{
		#region Reusable Tokens

		public static readonly Token<JsonTokenType> TokenNone = new Token<JsonTokenType>(JsonTokenType.None);

		public static readonly Token<JsonTokenType> TokenArrayBegin = new Token<JsonTokenType>(JsonTokenType.ArrayBegin);
		public static readonly Token<JsonTokenType> TokenArrayEnd = new Token<JsonTokenType>(JsonTokenType.ArrayEnd);
		public static readonly Token<JsonTokenType> TokenObjectBegin = new Token<JsonTokenType>(JsonTokenType.ObjectBegin);
		public static readonly Token<JsonTokenType> TokenObjectEnd = new Token<JsonTokenType>(JsonTokenType.ObjectEnd);
		public static readonly Token<JsonTokenType> TokenPairDelim = new Token<JsonTokenType>(JsonTokenType.PairDelim);
		public static readonly Token<JsonTokenType> TokenValueDelim = new Token<JsonTokenType>(JsonTokenType.ValueDelim);

		public static readonly Token<JsonTokenType> TokenUndefined = new Token<JsonTokenType>(JsonTokenType.Undefined);
		public static readonly Token<JsonTokenType> TokenNull = new Token<JsonTokenType>(JsonTokenType.Null);
		public static readonly Token<JsonTokenType> TokenFalse = new Token<JsonTokenType>(JsonTokenType.Boolean, false);
		public static readonly Token<JsonTokenType> TokenTrue = new Token<JsonTokenType>(JsonTokenType.Boolean, true);
		public static readonly Token<JsonTokenType> TokenNaN = new Token<JsonTokenType>(JsonTokenType.Number, Double.NaN);
		public static readonly Token<JsonTokenType> TokenPositiveInfinity = new Token<JsonTokenType>(JsonTokenType.Number, Double.PositiveInfinity);
		public static readonly Token<JsonTokenType> TokenNegativeInfinity = new Token<JsonTokenType>(JsonTokenType.Number, Double.NegativeInfinity);

		#endregion Reusable Tokens

		#region Keyword Literals

		public const string KeywordUndefined = "undefined";
		public const string KeywordNull = "null";
		public const string KeywordFalse = "false";
		public const string KeywordTrue = "true";
		public const string KeywordNaN = "NaN";
		public const string KeywordInfinity = "Infinity";

		#endregion Keyword Literals

		#region Operators

		public const char OperatorArrayBegin = '[';
		public const char OperatorArrayEnd = ']';
		public const char OperatorObjectBegin = '{';
		public const char OperatorObjectEnd = '}';
		public const char OperatorValueDelim = ',';
		public const char OperatorPairDelim = ':';

		public const char OperatorStringDelim = '"';
		public const char OperatorStringDelimAlt = '\'';
		public const char OperatorCharEscape = '\\';

		public const char OperatorUnaryMinus = '-';
		public const char OperatorUnaryPlus = '+';
		public const char OperatorDecimalPoint = '.';

		public const string OperatorCommentBegin = "/*";
		public const string OperatorCommentEnd = "*/";
		public const string OperatorCommentLine = "//";

		#endregion Operators
	}
}
