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
	/// Provides a mechanism for filtering HTML streams based upon a tag taxonomy
	/// </summary>
	public class HtmlFilter
	{
		#region HTML Tag Methods

		/// <summary>
		/// Determines if is "void" (i.e. "empty" or "full") tag
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns>if is a void tag</returns>
		/// <remarks>
		/// http://www.w3.org/TR/html5/semantics.html
		/// http://www.w3.org/TR/html401/index/elements.html
		/// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html#sec_5.2.
		/// http://www.w3.org/TR/WD-html40-970917/index/elements.html
		/// </remarks>
		private static bool VoidTagRequired(string tag)
		{
			switch (tag)
			{
				case "area":
				case "base":
				case "basefont":
				case "br":
				case "col":
				case "frame":
				case "hr":
				case "img":
				case "input":
				case "isindex":
				case "keygen":
				case "link":
				case "meta":
				case "param":
				case "source":
				case "wbr":
				{
					return true;
				}
				default:
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Determines if the tag is required to be closed
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns>if closing tag is optional</returns>
		/// <remarks>
		/// http://www.w3.org/TR/html5/semantics.html
		/// http://www.w3.org/TR/html401/index/elements.html
		/// http://www.w3.org/TR/WD-html40-970917/index/elements.html
		/// </remarks>
		private static bool CloseTagOptional(string tag)
		{
			switch (tag)
			{
				case "body":
				case "colgroup":
				case "dd":
				case "dt":
				case "embed":
				case "head":
				case "html":
				case "li":
				case "option":
				case "p":
				case "tbody":
				case "td":
				case "tfoot":
				case "th":
				case "thead":
				case "tr":
				{
					return true;
				}
				default:
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Categorizes the tag for heuristics about markup type
		/// </summary>
		/// <param name="tag">lowercase tag name</param>
		/// <returns>the box type for a particular element</returns>
		/// <remarks>
		/// http://www.w3.org/TR/html5/semantics.html
		/// </remarks>
		private static HtmlTaxonomy GetTaxonomy(string tag)
		{
			// http://www.w3.org/TR/html5/spec.html#contents
			// http://www.w3.org/TR/html401/
			// http://www.w3.org/TR/xhtml-modularization/abstract_modules.html
			// http://www.w3.org/html/wg/html5/#elements0
			// non-standard: http://www.mountaindragon.com/html/text.htm
			switch (tag)
			{
				case "a":
				case "abbr":
				case "acronym":
				case "area":
				case "bdo":
				case "cite":
				case "code":
				case "dfn":
				case "em":
				case "isindex":
				case "kbd":
				case "map":
				case "q":
				case "samp":
				case "span":
				case "strong":
				case "time":
				case "var":
				case "wbr":
				{
					return HtmlTaxonomy.Inline;
				}

				case "audio":
				case "bgsound":
				case "img":
				case "sound":
				case "source":
				{
					return HtmlTaxonomy.Media|HtmlTaxonomy.Inline;
				}

				case "canvas":
				case "math":
				case "svg":
				case "video":
				{
					// TODO: decide if it is worth adding all the SVG elements now that HTML5 recognizes
					// http://www.w3.org/TR/SVGTiny12/elementTable.html

					// TODO: decide if it is worth adding all the MathML elements now that HTML5 recognizes
					// http://www.w3.org/TR/MathML/chapter2.html#fund.overview

					return HtmlTaxonomy.Media|HtmlTaxonomy.Block;
				}

				case "b":
				case "big":
				case "blink":
				case "figcaption":
				case "font":
				case "i":
				case "marquee":
				case "mark":
				case "rp":
				case "rt":
				case "ruby":
				case "s":
				case "small":
				case "strike":
				case "sub":
				case "sup":
				case "tt":
				case "u":
				{
					return HtmlTaxonomy.Style|HtmlTaxonomy.Inline;
				}

				case "address":
				case "article":
				case "asside":
				case "blockquote":
				case "bq":
				case "br":
				case "center":
				case "del":
				case "details":
				case "div":
				case "figure":
				case "footer":
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
				case "header":
				case "hgroup":
				case "hr":
				case "ins":
				case "nav":
				case "nobr":
				case "p":
				case "pre":
				case "section":
				case "summary":
				{
					return HtmlTaxonomy.Block;
				}

				case "command":
				case "dl":
				case "dd":
				case "dir":
				case "dt":
				case "lh":
				case "li":
				case "menu":
				case "ol":
				case "ul":
				{
					return HtmlTaxonomy.List;
				}

				case "caption":
				case "col":
				case "colgroup":
				case "table":
				case "tbody":
				case "td":
				case "th":
				case "thead":
				case "tfoot":
				case "tr":
				{
					return HtmlTaxonomy.Table;
				}

				case "button":
				case "datalist":
				case "fieldset":
				case "form":
				case "keygen":
				case "input":
				case "label":
				case "legend":
				case "meter":
				case "optgroup":
				case "option":
				case "output":
				case "progress":
				case "select":
				case "textarea":
				{
					return HtmlTaxonomy.Form;
				}

				case "applet":
				case "embed":
				case "noembed":
				case "object":
				case "param":
				{
					return HtmlTaxonomy.Plugin;
				}

				case "basefont":
				case "link":
				case "style":
				{
					return HtmlTaxonomy.Style|HtmlTaxonomy.Document;
				}

				case "noscript":
				case "script":
				{
					return HtmlTaxonomy.Script|HtmlTaxonomy.Document;
				}

				case "base":
				case "body":
				case "frame":
				case "frameset":
				case "head":
				case "html":
				case "iframe":
				case "meta":
				case "noframes":
				case "title":
				{
					return HtmlTaxonomy.Document;
				}
			}
			return HtmlTaxonomy.Unknown;
		}

		#endregion HTML Tag Methods
	}
}