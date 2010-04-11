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

namespace JsonFx.Json
{
	/// <summary>
	/// Lexical Analysis Tokens
	/// </summary>
	public enum JsonTokenType
	{
		/// <summary>
		/// End of input sequence
		/// </summary>
		None,

		/// <summary>
		/// undefined, null, false, true, NaN, Infinity, -Infinity
		/// </summary>
		Keyword,

		/// <summary>
		/// JSON Number
		/// </summary>
		Number,

		/// <summary>
		/// JSON String
		/// </summary>
		String,

		/// <summary>
		/// Start of JSON Array
		/// </summary>
		ArrayStart,

		/// <summary>
		/// End of JSON Array
		/// </summary>
		ArrayEnd,

		/// <summary>
		/// Start of JSON Object
		/// </summary>
		ObjectStart,

		/// <summary>
		/// End of JSON Object
		/// </summary>
		ObjectEnd,

		/// <summary>
		/// JSON Object Property name/value deliminator
		/// </summary>
		NameDelim,

		/// <summary>
		/// JSON Object Property deliminator
		/// </summary>
		ValueDelim,

		/// <summary>
		/// unquoted Object Property name
		/// </summary>
		UnquotedName
	}
}
