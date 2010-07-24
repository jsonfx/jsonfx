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
	/// An <see cref="ITextTokenizer<T>"/> which after transforms tokens to a different type
	/// </summary>
	/// <typeparam name="TIn">input token type</typeparam>
	/// <typeparam name="TOut">output token type</typeparam>
	internal class TransformTokenizer<TIn, TOut> : ITextTokenizer<TOut>
	{
		#region Fields

		private readonly ITextTokenizer<TIn> Tokenizer;
		private readonly IDataTransformer<TIn, TOut> Transformer;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenizer"></param>
		/// <param name="transformer"></param>
		public TransformTokenizer(ITextTokenizer<TIn> tokenizer, IDataTransformer<TIn, TOut> transformer)
		{
			if (tokenizer == null)
			{
				new ArgumentNullException("tokenizer");
			}
			if (transformer == null)
			{
				new ArgumentNullException("transformer");
			}

			this.Tokenizer = tokenizer;
			this.Transformer = transformer;
		}

		#endregion Init

		#region ITextTokenizer<TOut> Members

		public int Column
		{
			get { return this.Tokenizer.Column; }
		}

		public int Line
		{
			get { return this.Tokenizer.Line; }
		}

		public long Index
		{
			get { return this.Tokenizer.Index; }
		}

		public IEnumerable<Token<TOut>> GetTokens(TextReader reader)
		{
			IEnumerable<Token<TIn>> tokens = this.Tokenizer.GetTokens(reader);

			return this.Transformer.Transform(tokens);
		}

		public IEnumerable<Token<TOut>> GetTokens(string text)
		{
			IEnumerable<Token<TIn>> tokens = this.Tokenizer.GetTokens(text);

			return this.Transformer.Transform(tokens);
		}

		#endregion ITextTokenizer<TOut> Members

		#region IDisposable Members

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion IDisposable Members
	}
}
