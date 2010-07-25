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
	public partial class JsonMLReader
	{
		/// <summary>
		/// Transforms markup tokens into common data tokens using a JsonML model
		/// </summary>
		public class JsonMLReadConverter : IDataTransformer<MarkupTokenType, CommonTokenType>
		{
			#region Constants

			private const string ErrorUnexpectedToken = "Unexpected token ({0})";

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

			#region IDataTransformer<CommonTokenType, MarkupTokenType> Members

			/// <summary>
			/// Consumes a sequence of tokens and produces a token sequence of a different type
			/// </summary>
			public IEnumerable<Token<CommonTokenType>> Transform(IEnumerable<Token<MarkupTokenType>> input)
			{
				if (input == null)
				{
					throw new ArgumentNullException("input");
				}

				List<Token<CommonTokenType>> output = new List<Token<CommonTokenType>>();
				IStream<Token<MarkupTokenType>> stream = new Stream<Token<MarkupTokenType>>(input);

				Token<MarkupTokenType> token = stream.Peek();
				bool needsValueDelim = false;
				while (!stream.IsCompleted)
				{
					switch (token.TokenType)
					{
						case MarkupTokenType.ElementBegin:
						case MarkupTokenType.ElementVoid:
						{
							bool isVoid = (token.TokenType == MarkupTokenType.ElementVoid);
							output.Add(CommonGrammar.TokenArrayBeginNoName);
							output.Add(CommonGrammar.TokenValue(token.Name));

							needsValueDelim = false;

							stream.Pop();
							token = stream.Peek();
							while (!stream.IsCompleted &&
								token.TokenType == MarkupTokenType.Attribute)
							{
								if (!needsValueDelim)
								{
									needsValueDelim = true;
									output.Add(CommonGrammar.TokenObjectBeginNoName);
								}
								else
								{
									output.Add(CommonGrammar.TokenValueDelim);
								}

								output.Add(CommonGrammar.TokenProperty(token.Name));

								stream.Pop();
								token = stream.Peek();

								switch (token.TokenType)
								{
									case MarkupTokenType.TextValue:
									case MarkupTokenType.Whitespace:
									{
										output.Add(CommonGrammar.TokenValue(token.Value));
										break;
									}
									case MarkupTokenType.UnparsedBlock:
									{
										output.Add(new Token<CommonTokenType>(CommonTokenType.Primitive, token.Name, token.Value));
										break;
									}
									default:
									{
										throw new TokenException<MarkupTokenType>(
											token,
											String.Format(ErrorUnexpectedToken, token.TokenType));
									}
								}

								stream.Pop();
								token = stream.Peek();
							}

							if (needsValueDelim)
							{
								output.Add(CommonGrammar.TokenObjectEnd);
							}

							if (isVoid)
							{
								goto case MarkupTokenType.ElementEnd;
							}
							break;
						}
						case MarkupTokenType.ElementEnd:
						{
							output.Add(CommonGrammar.TokenArrayEnd);
							needsValueDelim = true;
							break;
						}
						case MarkupTokenType.TextValue:
						case MarkupTokenType.Whitespace:
						{
							if (needsValueDelim)
							{
								output.Add(CommonGrammar.TokenValueDelim);
							}
							output.Add(CommonGrammar.TokenValue(token.Value));
							break;
						}
						case MarkupTokenType.UnparsedBlock:
						{
							if (needsValueDelim)
							{
								output.Add(CommonGrammar.TokenValueDelim);
							}
							output.Add(new Token<CommonTokenType>(CommonTokenType.Primitive, token.Name, token.Value));
							break;
						}
						case MarkupTokenType.Attribute:
						default:
						{
							throw new TokenException<MarkupTokenType>(
								token,
								String.Format(ErrorUnexpectedToken, token.TokenType));
						}
					}
				}

				return output;
			}

			#endregion IDataTransformer<CommonTokenType, MarkupTokenType> Members
		}
	}
}
