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
    /// Outputs text from an input stream of JSON tokens
    /// </summary>
    /// <typeparam name="T">token type</typeparam>
    public interface ITextFormatter<T>
    {
        #region Methods

        /// <summary>
        /// Formats the token sequence to the output writer
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="writer"></param>
        void Format(IEnumerable<Token<T>> tokens, TextWriter writer);

        /// <summary>
        /// Formats the token sequence as a string
        /// </summary>
        /// <param name="tokens"></param>
        string Format(IEnumerable<Token<T>> tokens);

        #endregion Methods
    }
}
