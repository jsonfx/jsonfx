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
using System.IO;

namespace JsonFx.Serialization
{
	/// <summary>
	/// An <see cref="ITextFormatter<T>"/> which first transforms tokens of a different type
	/// </summary>
	/// <typeparam name="TIn">input token type</typeparam>
	/// <typeparam name="TOut">output token type</typeparam>
	internal class TransformFormatter<TIn, TOut> : ITextFormatter<TIn>
	{
		#region Fields

		private readonly ITextFormatter<TOut> Formatter;
		private readonly IDataTransformer<TIn, TOut> Transformer;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="formatter"></param>
		/// <param name="transformer"></param>
		public TransformFormatter(ITextFormatter<TOut> formatter, IDataTransformer<TIn, TOut> transformer)
		{
			if (formatter == null)
			{
				new ArgumentNullException("formatter");
			}
			if (transformer == null)
			{
				new ArgumentNullException("transformer");
			}

			this.Formatter = formatter;
			this.Transformer = transformer;
		}

		#endregion Init

		#region ITextFormatter<ModelTokenType> Members

		public void Format(IEnumerable<Token<TIn>> tokens, TextWriter writer)
		{
			IEnumerable<Token<TOut>> markup = this.Transformer.Transform(tokens);

			this.Formatter.Format(markup, writer);
		}

		public string Format(IEnumerable<Token<TIn>> tokens)
		{
			IEnumerable<Token<TOut>> markup = this.Transformer.Transform(tokens);

			return this.Formatter.Format(markup);
		}

		#endregion ITextFormatter<ModelTokenType> Members
	}
}
