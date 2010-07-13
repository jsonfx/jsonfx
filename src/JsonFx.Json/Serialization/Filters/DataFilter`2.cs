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

using JsonFx.IO;

namespace JsonFx.Serialization.Filters
{
	/// <summary>
	/// Partially implements an IDataFilter
	/// </summary>
	public abstract class DataFilter<TTokenType, TResult> : IDataFilter<TTokenType, TResult>
	{
		#region IDataFilter<TTokenType,TResult> Members

		public abstract bool TryRead(DataReaderSettings settings, IStream<Token<TTokenType>> tokens, out TResult value);

		public abstract bool TryWrite(DataWriterSettings settings, TResult value, out IEnumerable<Token<TTokenType>> tokens);

		#endregion IDataFilter<TTokenType,TResult> Members

		#region IDataFilter<TTokenType> Members

		bool IDataFilter<TTokenType>.TryRead(DataReaderSettings settings, IStream<Token<TTokenType>> tokens, out object value)
		{
			TResult result;
			bool success = this.TryRead(settings, tokens, out result);

			value = result;
			return success;
		}

		bool IDataFilter<TTokenType>.TryWrite(DataWriterSettings settings, object value, out IEnumerable<Token<TTokenType>> tokens)
		{
			tokens = null;

			return (value is TResult) && (this.TryWrite(settings, (TResult)value, out tokens));
		}

		#endregion IDataFilter<TTokenType> Members
	}
}
