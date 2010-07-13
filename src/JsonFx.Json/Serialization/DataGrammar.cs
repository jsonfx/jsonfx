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

namespace JsonFx.Serialization
{
	/// <summary>
	/// Formal language of tokens and symbols for general analysis
	/// </summary>
	internal class DataGrammar
	{
		#region Reusable Tokens

		public static readonly Token<DataTokenType> TokenNone = new Token<DataTokenType>(DataTokenType.None);

		public static readonly Token<DataTokenType> TokenArrayBegin = new Token<DataTokenType>(DataTokenType.ArrayBegin);
		public static readonly Token<DataTokenType> TokenArrayEnd = new Token<DataTokenType>(DataTokenType.ArrayEnd);
		public static readonly Token<DataTokenType> TokenObjectBegin = new Token<DataTokenType>(DataTokenType.ObjectBegin);
		public static readonly Token<DataTokenType> TokenObjectEnd = new Token<DataTokenType>(DataTokenType.ObjectEnd);
		public static readonly Token<DataTokenType> TokenValueDelim = new Token<DataTokenType>(DataTokenType.ValueDelim);

		public static readonly Token<DataTokenType> TokenNull = new Token<DataTokenType>(DataTokenType.Value, null);
		public static readonly Token<DataTokenType> TokenFalse = new Token<DataTokenType>(DataTokenType.Value, false);
		public static readonly Token<DataTokenType> TokenTrue = new Token<DataTokenType>(DataTokenType.Value, true);
		public static readonly Token<DataTokenType> TokenNaN = new Token<DataTokenType>(DataTokenType.Value, Double.NaN);
		public static readonly Token<DataTokenType> TokenPositiveInfinity = new Token<DataTokenType>(DataTokenType.Value, Double.PositiveInfinity);
		public static readonly Token<DataTokenType> TokenNegativeInfinity = new Token<DataTokenType>(DataTokenType.Value, Double.NegativeInfinity);

		public static Token<DataTokenType> TokenProperty(object value)
		{
			return new Token<DataTokenType>(DataTokenType.PropertyKey, value);
		}

		//public static Token<DataTokenType> TokenAttribute(object value)
		//{
		//    return new Token<DataTokenType>(DataTokenType.AttributeKey, value);
		//}

		public static Token<DataTokenType> TokenValue(object value)
		{
			return new Token<DataTokenType>(DataTokenType.Value, value);
		}

		#endregion Reusable Tokens
	}
}
