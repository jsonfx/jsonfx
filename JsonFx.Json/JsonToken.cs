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
using System.Text;

namespace JsonFx.Json
{
	/// <summary>
	/// Represents a single token in a JSON stream
	/// </summary>
	public sealed class JsonToken
	{
		#region Constants

		internal static readonly JsonToken None = new JsonToken(JsonTokenType.None);

		internal static readonly JsonToken ArrayStart = new JsonToken(JsonTokenType.ArrayStart);
		internal static readonly JsonToken ArrayEnd = new JsonToken(JsonTokenType.ArrayEnd);
		internal static readonly JsonToken ObjectStart = new JsonToken(JsonTokenType.ObjectStart);
		internal static readonly JsonToken ObjectEnd = new JsonToken(JsonTokenType.ObjectEnd);
		internal static readonly JsonToken NameDelim = new JsonToken(JsonTokenType.NameDelim);
		internal static readonly JsonToken ValueDelim = new JsonToken(JsonTokenType.ValueDelim);

		internal static readonly JsonToken Undefined = new JsonToken(JsonTokenType.Undefined, null);
		internal static readonly JsonToken Null = new JsonToken(JsonTokenType.Null, null);
		internal static readonly JsonToken False = new JsonToken(JsonTokenType.Boolean, false);
		internal static readonly JsonToken True = new JsonToken(JsonTokenType.Boolean, true);
		internal static readonly JsonToken NotANumber = new JsonToken(JsonTokenType.Number, Double.NaN);
		internal static readonly JsonToken PositiveInfinity = new JsonToken(JsonTokenType.Number, Double.PositiveInfinity);
		internal static readonly JsonToken NegativeInfinity = new JsonToken(JsonTokenType.Number, Double.NegativeInfinity);

		#endregion Constants

		#region Fields

		public readonly JsonTokenType TokenType;
		public readonly object Value;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenType"></param>
		public JsonToken(JsonTokenType tokenType)
			: this(tokenType, null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenType"></param>
		/// <param name="value"></param>
		public JsonToken(JsonTokenType tokenType, object value)
		{
			this.TokenType = tokenType;
			this.Value = value;
		}

		#endregion Init

		#region Object Overrides

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("{ TokenType=");
			builder.Append(this.TokenType);
			if (this.Value != null)
			{
				builder.Append(", Value=");
				builder.Append(this.Value);
			}
			builder.Append(" }");

			return builder.ToString();
		}

		#endregion Object Overrides
	}
}
