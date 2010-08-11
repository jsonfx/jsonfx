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

using JsonFx.Markup;
using JsonFx.Serialization;

namespace JsonFx.Html
{
	/// <summary>
	/// Defines a strategy for filtering HTML tags/attributes/styles/literals
	/// </summary>
	public interface IHtmlFilterStrategy
	{
		#region Methods

		/// <summary>
		/// Filters tags, optionally allowing altering of tag
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <returns>if true tag should be rendered</returns>
		bool FilterTag(DataName tag, MarkupTokenType type, HtmlTaxonomy taxonomy);

		/// <summary>
		/// Filters attributes, optionally allowing altering of attribute value
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="attribute">attribute name</param>
		/// <param name="value">attribute value</param>
		/// <returns>if true attribute should be rendered</returns>
		bool FilterAttribute(DataName tag, DataName attribute, ref object value);

		/// <summary>
		/// Filters styles, optionally allowing altering of style value
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="style">style name</param>
		/// <param name="value">style value</param>
		/// <returns>if true style should be rendered</returns>
		bool FilterStyle(DataName tag, string style, ref object value);

		/// <summary>
		/// Filters literals, optionally allowing replacement of literal value
		/// </summary>
		/// <param name="value">the literal value</param>
		/// <returns>if true should be rendered</returns>
		bool FilterLiteral(ref object value);

		#endregion Methods
	}
}