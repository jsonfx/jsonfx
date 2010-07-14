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

using JsonFx.Common;
using JsonFx.IO;
using JsonFx.Serialization;

namespace JsonFx.Xml
{
	public partial class XmlReader
	{
		/// <summary>
		/// Generates a SAX-like sequence of tokens from XML text
		/// </summary>
		public class XmlTokenizer : ITextTokenizer<CommonTokenType>
		{
			#region Constants

			#endregion Constants

			#region Fields

			private ITextStream Scanner = TextReaderStream.Null;

			#endregion Fields

			#region Properties

			/// <summary>
			/// Gets the total number of characters read from the input
			/// </summary>
			public int Column
			{
				get { return this.Scanner.Column; }
			}

			/// <summary>
			/// Gets the total number of lines read from the input
			/// </summary>
			public int Line
			{
				get { return this.Scanner.Line; }
			}

			/// <summary>
			/// Gets the current position within the input
			/// </summary>
			public long Index
			{
				get { return this.Scanner.Index; }
			}

			#endregion Properties

			#region Scanning Methods

			private static Token<CommonTokenType> NextToken(ITextStream scanner)
			{
				throw new NotImplementedException();
			}

			#endregion Scanning Methods

			#region ITextTokenizer<DataTokenType> Members

			/// <summary>
			/// Gets a token sequence from the TextReader
			/// </summary>
			/// <param name="reader"></param>
			/// <returns></returns>
			public IEnumerable<Token<CommonTokenType>> GetTokens(TextReader reader)
			{
				return this.GetTokens(new TextReaderStream(reader));
			}

			/// <summary>
			/// Gets a token sequence from the string
			/// </summary>
			/// <param name="text"></param>
			/// <returns></returns>
			public IEnumerable<Token<CommonTokenType>> GetTokens(string text)
			{
				return this.GetTokens(new StringStream(text));
			}

			/// <summary>
			/// Gets a token sequence from the scanner
			/// </summary>
			/// <param name="scanner"></param>
			/// <returns></returns>
			protected IEnumerable<Token<CommonTokenType>> GetTokens(ITextStream scanner)
			{
				if (scanner == null)
				{
					throw new ArgumentNullException("scanner");
				}

				this.Scanner = scanner;

				while (true)
				{
					Token<CommonTokenType> token = XmlTokenizer.NextToken(scanner);
					if (token.TokenType == CommonTokenType.None)
					{
						scanner.Dispose();
						this.Scanner = StringStream.Null;
						yield break;
					}
					yield return token;
				};
			}

			#endregion ITextTokenizer<DataTokenType> Members

			#region IDisposable Members

			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
					((IDisposable)this.Scanner).Dispose();
				}
			}

			#endregion IDisposable Members
		}
	}
}
