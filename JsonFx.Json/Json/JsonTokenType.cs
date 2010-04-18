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
	/// JSON Lexical Analysis Tokens
	/// </summary>
	public enum JsonTokenType
	{
		/// <summary>
		/// No token
		/// </summary>
		/// <remarks>
		/// The state before and after an object graph
		/// </remarks>
		None,

		/// <summary>
		/// Undefined value
		/// </summary>
		Undefined,

		/// <summary>
		/// Null value
		/// </summary>
		Null,

		/// <summary>
		/// Boolean value
		/// </summary>
		Boolean,

		/// <summary>
		/// Number value
		/// </summary>
		Number,

		/// <summary>
		/// String value
		/// </summary>
		String,

		/// <summary>
		/// Begin Array
		/// </summary>
		ArrayBegin,

		/// <summary>
		/// End Array
		/// </summary>
		ArrayEnd,

		/// <summary>
		/// Begin Object
		/// </summary>
		ObjectBegin,

		/// <summary>
		/// End Object
		/// </summary>
		ObjectEnd,

		/// <summary>
		/// JSON Object name/value pair deliminator
		/// </summary>
		PairDelim,

		/// <summary>
		/// JSON value deliminator
		/// </summary>
		ValueDelim,

		/// <summary>
		/// Identifiers, Literal text
		/// </summary>
		Literal
	}
}
