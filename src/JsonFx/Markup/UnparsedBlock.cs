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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using JsonFx.Serialization;

namespace JsonFx.Markup
{
	/// <summary>
	/// Designates a type as being able to format itself to raw JSON text.
	/// </summary>
	public class UnparsedBlock : ITextFormattable<MarkupTokenType>
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public UnparsedBlock()
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public UnparsedBlock(string begin, string end, string value)
		{
			this.Begin = begin;
			this.End = end;
			this.Value = value;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the starting delimiter
		/// </summary>
		public string Begin
		{
			get;
			set;
		}

		/// <summary>
		/// Gets and sets the ending delimiter
		/// </summary>
		public string End
		{
			get;
			set;
		}

		/// <summary>
		/// Gets and sets the context
		/// </summary>
		public string Value
		{
			get;
			set;
		}

		#endregion Properties

		#region ITextFormattable<MarkupTokenType> Members

		public void Format(ITextFormatter<MarkupTokenType> formatter, TextWriter writer)
		{
			writer.Write(MarkupGrammar.OperatorElementBegin);
			writer.Write(this.Begin);
			writer.Write(this.Value);
			writer.Write(this.End);
			writer.Write(MarkupGrammar.OperatorElementEnd);
		}

		#endregion ITextFormattable<MarkupTokenType> Members

		#region Object Overrides

		public override string ToString()
		{
			var foo = new
			{
				this.Begin,
				this.End,
				this.Value
			};

			return String.Concat(
				MarkupGrammar.OperatorElementBegin,
				this.Begin,
				this.Value,
				this.End,
				MarkupGrammar.OperatorElementEnd);
		}

		public override bool Equals(object value)
		{
			var that = value as UnparsedBlock;
			if (that == null)
			{
				return false;
			}

			return
				StringComparer.Ordinal.Equals(this.Begin, that.Begin) &&
				StringComparer.Ordinal.Equals(this.End, that.End) &&
				StringComparer.Ordinal.Equals(this.Value, that.Value);
		}

		public override int GetHashCode()
		{
			int num = -118650411;
			num = (-1521134295 * num) + StringComparer.Ordinal.GetHashCode(this.Begin);
			num = (-1521134295 * num) + StringComparer.Ordinal.GetHashCode(this.End);
			return ((-1521134295 * num) + StringComparer.Ordinal.GetHashCode(this.Value));
		}

		#endregion Object Overrides
	}
}
