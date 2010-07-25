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

using JsonFx.Common;
using JsonFx.IO;
using JsonFx.Markup;
using JsonFx.Serialization;

namespace JsonFx.JsonML
{
	public partial class JsonMLWriter
	{
		/// <summary>
		/// Transforms common data tokens into markup tokens using a JsonML model
		/// </summary>
		public class JsonMLWriteConverter : IDataTransformer<CommonTokenType, MarkupTokenType>
		{
			#region Constants

			private const string ErrorUnexpectedToken = "Unexpected token ({0})";

			#endregion Constants

			#region IDataTransformer<MarkupTokenType,CommonTokenType> Members

			/// <summary>
			/// Consumes a sequence of tokens and produces a token sequence of a different type
			/// </summary>
			public IEnumerable<Token<MarkupTokenType>> Transform(IEnumerable<Token<CommonTokenType>> input)
			{
				if (input == null)
				{
					throw new ArgumentNullException("input");
				}

				IStream<Token<CommonTokenType>> stream = new Stream<Token<CommonTokenType>>(input);

				Token<CommonTokenType> token = stream.Peek();
				while (!stream.IsCompleted)
				{
					switch (token.TokenType)
					{
						case CommonTokenType.ArrayBegin:
						{
							yield return MarkupGrammar.TokenElementEnd(new DataName());

							stream.Pop();
							token = stream.Peek();
							break;
						}
						case CommonTokenType.ArrayEnd:
						{
							yield return MarkupGrammar.TokenElementEnd(new DataName());

							stream.Pop();
							token = stream.Peek();
							break;
						}
						case CommonTokenType.Primitive:
						{
							stream.Pop();
							token = stream.Peek();
							break;
						}
						case CommonTokenType.ValueDelim:
						{
							// consume, has no counterpart in markup
							stream.Pop();
							token = stream.Peek();
							break;
						}
						case CommonTokenType.ObjectBegin:
						case CommonTokenType.ObjectEnd:
						case CommonTokenType.Property:
						default:
						{
							throw new TokenException<CommonTokenType>(
								token,
								String.Format(ErrorUnexpectedToken, token.TokenType));
						}
					}
				}
			}

			#endregion IDataTransformer<MarkupTokenType,CommonTokenType> Members
		}
	}
}
