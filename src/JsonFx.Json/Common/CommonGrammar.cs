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

namespace JsonFx.Common
{
	/// <summary>
	/// Common formal language of tokens for analysis
	/// </summary>
	internal class CommonGrammar
	{
		#region Reusable Tokens

		public static readonly Token<CommonTokenType> TokenNone = new Token<CommonTokenType>(CommonTokenType.None);

		public static readonly Token<CommonTokenType> TokenArrayBegin = new Token<CommonTokenType>(CommonTokenType.ArrayBegin);
		public static readonly Token<CommonTokenType> TokenArrayEnd = new Token<CommonTokenType>(CommonTokenType.ArrayEnd);
		public static readonly Token<CommonTokenType> TokenObjectBegin = new Token<CommonTokenType>(CommonTokenType.ObjectBegin);
		public static readonly Token<CommonTokenType> TokenObjectEnd = new Token<CommonTokenType>(CommonTokenType.ObjectEnd);
		public static readonly Token<CommonTokenType> TokenValueDelim = new Token<CommonTokenType>(CommonTokenType.ValueDelim);

		public static readonly Token<CommonTokenType> TokenNull = new Token<CommonTokenType>(CommonTokenType.Value, null);
		public static readonly Token<CommonTokenType> TokenFalse = new Token<CommonTokenType>(CommonTokenType.Value, false);
		public static readonly Token<CommonTokenType> TokenTrue = new Token<CommonTokenType>(CommonTokenType.Value, true);
		public static readonly Token<CommonTokenType> TokenNaN = new Token<CommonTokenType>(CommonTokenType.Value, Double.NaN);
		public static readonly Token<CommonTokenType> TokenPositiveInfinity = new Token<CommonTokenType>(CommonTokenType.Value, Double.PositiveInfinity);
		public static readonly Token<CommonTokenType> TokenNegativeInfinity = new Token<CommonTokenType>(CommonTokenType.Value, Double.NegativeInfinity);

		public static Token<CommonTokenType> TokenProperty(object value)
		{
			return new Token<CommonTokenType>(CommonTokenType.PropertyKey, value);
		}

		//public static Token<DataTokenType> TokenAttribute(object value)
		//{
		//    return new Token<DataTokenType>(DataTokenType.AttributeKey, value);
		//}

		public static Token<CommonTokenType> TokenValue(object value)
		{
			return new Token<CommonTokenType>(CommonTokenType.Value, value);
		}

		#endregion Reusable Tokens
	}
}
