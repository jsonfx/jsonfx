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

namespace JsonFx.Xml.Sax
{
	/// <summary>
	/// SAX-like tokens
	/// </summary>
	public enum SaxTokenType
	{
		/// <summary>
		/// No token
		/// </summary>
		/// <remarks>
		/// The token <see cref="Token<CommonTokenType>.Value"/> and <see cref="Token<CommonTokenType>.Name"/> must be null.
		/// Represents the absence of a token and is not represented in the document.
		/// </remarks>
		None,

		/// <summary>
		/// Begin element, the token contains the element Name
		/// </summary>
		/// <remarks>
		/// The element <see cref="Token<CommonTokenType>.Name"/> is required.
		/// Marks the beginning of an element and all its children including attributes.
		/// </remarks>
		ElementBegin,

		/// <summary>
		/// End object
		/// </summary>
		/// <remarks>
		/// The token <see cref="Token<CommonTokenType>.Value"/> and <see cref="Token<CommonTokenType>.Name"/> must be left empty.
		/// The element Name is determined by the <see cref="SaxTokenType.ElementBegin"/> token.
		/// Marks the end of the last object property as well as the object itself.
		/// </remarks>
		ElementEnd,

		/// <summary>
		/// Begin mapping a prefix to a namespace URI
		/// </summary>
		/// <remarks>
		/// The element <see cref="Token<CommonTokenType>.Name"/> is required.
		/// Marks the beginning of a namespace prefix scope.
		/// </remarks>
		PrefixBegin,

		/// <summary>
		/// End mapping a prefix to a namespace URI
		/// </summary>
		/// <remarks>
		/// The element <see cref="Token<CommonTokenType>.Name"/> is required.
		/// Marks the end of a namespace prefix scope.
		/// </remarks>
		PrefixEnd,

		/// <summary>
		/// Begin attribute, the token contains both the attribute Name and Value
		/// </summary>
		/// <remarks>
		/// The property <see cref="Token<CommonTokenType>.Name"/> is required as all formats name their properties.
		/// The <see cref="Token<CommonTokenType>.Value"/> is optional, i.e. null and whitespace are a valid values.
		/// Typically <see cref="Token<CommonTokenType>.Value"/> is serialized directly as a string.
		/// If the <see cref="Token<CommonTokenType>.Value"/> is not a CLR primitive, then the value must implement IConvertable, IFormatable or be meaningful when cast to a string.
		/// </remarks>
		Attribute,

		/// <summary>
		/// A block of text, the token contains the text value
		/// </summary>
		/// <remarks>
		/// The <see cref="Token<CommonTokenType>.Value"/> is optional, i.e. null and whitespace are a valid values.
		/// Typically <see cref="Token<CommonTokenType>.Value"/> is serialized directly as a string.
		/// If the <see cref="Token<CommonTokenType>.Value"/> is not a CLR primitive, then the value must implement IConvertable, IFormatable or be meaningful when cast to a string.
		/// </remarks>
		TextValue,

		/// <summary>
		/// A block of text made entirely of whitespace, the token contains the text value
		/// </summary>
		/// <remarks>
		/// The <see cref="Token<CommonTokenType>.Value"/> is optional, i.e. null and whitespace are a valid values.
		/// Typically <see cref="Token<CommonTokenType>.Value"/> is serialized directly as a string.
		/// If the <see cref="Token<CommonTokenType>.Value"/> is not a CLR primitive, then the value must implement IConvertable, IFormatable or be meaningful when cast to a string.
		/// </remarks>
		Whitespace
	}
}
