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

namespace JsonFx.Html
{
	/// <summary>
	/// Defines a prioritized taxonomy of tags
	/// </summary>
	/// <remarks>
	/// The types are enumerated in ascending levels of risk for use in filtering HTML input
	/// </remarks>
	[Flags]
	public enum HtmlTaxonomy
	{
		/// <summary>
		/// Literal text, no tags
		/// </summary>
		Text = 0x0000,

		/// <summary>
		/// Inline character level elements and text strings
		/// </summary>
		/// <remarks>
		/// Tags of this type typically do not disrupt the text flow
		/// </remarks>
		Inline = 0x0001,

		/// <summary>
		/// style elements
		/// </summary>
		/// <remarks>
		/// Tags of this type change the visual appearance of text
		/// </remarks>
		Style = 0x0002,

		/// <summary>
		/// list elements
		/// </summary>
		/// <remarks>
		/// Tags of this type denote lists and typically change the text flow
		/// </remarks>
		List = 0x0004,

		/// <summary>
		/// Block-level elements
		/// </summary>
		/// <remarks>
		/// Tags of this type denote sections or change the text flow
		/// </remarks>
		Block = 0x0008,

		/// <summary>
		/// Media elements
		/// </summary>
		/// <remarks>
		/// Tags of this type safely embed media content
		/// </remarks>
		Media = 0x0010,

		/// <summary>
		/// Tabular elements
		/// </summary>
		/// <remarks>
		/// Tags of this type have a very specific structure and their own rendering model
		/// </remarks>
		Table = 0x0020,

		/// <summary>
		/// Form elements
		/// </summary>
		/// <remarks>
		/// Tags of this type are used in the construction of forms for capturing user input
		/// </remarks>
		Form = 0x0040,

		/// <summary>
		/// Script elements
		/// </summary>
		/// <remarks>
		/// Tags of this type represent a security risk to the containing document but must obey the browser security sandbox
		/// </remarks>
		Script = 0x0080,

		/// <summary>
		/// Document elements
		/// </summary>
		/// <remarks>
		/// Tags of this type are used to construct the document itself
		/// </remarks>
		Document = 0x0100,

		/// <summary>
		/// embedded elements
		/// </summary>
		/// <remarks>
		/// Tags of this type represent a large security risk to the containing document as plug-ins may circumvent the browser security sandbox
		/// </remarks>
		Plugin = 0x0200,

		/// <summary>
		/// Unknown elements
		/// </summary>
		Unknown = 0x8000
	}
}