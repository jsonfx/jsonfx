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
using JsonFx.Markup;
using JsonFx.Model;
using JsonFx.Serialization;

namespace JsonFx.JsonML
{
	public partial class JsonMLWriter
	{
		/// <summary>
		/// Transforms Common Model tokens into markup tokens using the (lossless) JsonML model
		/// </summary>
		/// <remarks>
		/// JsonML Grammer: http://jsonml.org
		/// </remarks>
		public class JsonMLOutTransformer : IDataTransformer<ModelTokenType, MarkupTokenType>
		{
			#region Constants

			private const string ErrorUnexpectedToken = "Unexpected token ({0})";
			private const string ErrorMissingTagName = "Missing JsonML tag name";
			private const string ErrorInvalidAttributeValue = "Invalid attribute value token ({0})";
			private const string ErrorUnterminatedAttributeBlock = "Unterminated attribute block ({0})";

			#endregion Constants

			#region IDataTransformer<MarkupTokenType,ModelTokenType> Members

			/// <summary>
			/// Consumes a sequence of tokens and produces a token sequence of a different type
			/// </summary>
			public IEnumerable<Token<MarkupTokenType>> Transform(IEnumerable<Token<ModelTokenType>> input)
			{
				if (input == null)
				{
					throw new ArgumentNullException("input");
				}

				IStream<Token<ModelTokenType>> stream = Stream<Token<ModelTokenType>>.Create(input);

				Token<ModelTokenType> token = stream.Peek();
				while (!stream.IsCompleted)
				{
					switch (token.TokenType)
					{
						case ModelTokenType.ArrayBegin:
						{
							// consume array begin token
							stream.Pop();
							token = stream.Peek();

							if (token.TokenType != ModelTokenType.Primitive)
							{
								throw new TokenException<ModelTokenType>(
									token,
									JsonMLOutTransformer.ErrorMissingTagName);
							}

							DataName tagName;
							if (token.Value is DataName)
							{
								tagName = (DataName)token.Value;
							}
							else
							{
								// use as local-name
								tagName = new DataName(token.ValueAsString());
							}

							// consume name value token
							stream.Pop();
							token = stream.Peek();

							//if (token.TokenType == ModelTokenType.ArrayEnd)
							//{
							//    // consume array end token
							//    stream.Pop();
							//    token = stream.Peek();

							//    // is a void tag
							//    yield return MarkupGrammar.TokenElementVoid(tagName);
							//    break;
							//}

							// TODO: evaluate if worth queuing up attributes for void tags with attribs
							yield return MarkupGrammar.TokenElementBegin(tagName);

							if (token.TokenType != ModelTokenType.ObjectBegin)
							{
								// no attributes, but has children
								break;
							}

							// consume object begin token
							stream.Pop();
							token = stream.Peek();

							while (token.TokenType == ModelTokenType.Property)
							{
								yield return token.ChangeType(MarkupTokenType.Attribute);

								// consume attribute name token
								stream.Pop();
								token = stream.Peek();

								switch (token.TokenType)
								{
									case ModelTokenType.Primitive:
									{
										yield return token.ChangeType(MarkupTokenType.Primitive);
										break;
									}
									default:
									{
										throw new TokenException<ModelTokenType>(
											token,
											String.Format(JsonMLOutTransformer.ErrorInvalidAttributeValue, token.TokenType));
									}
								}

								// consume attribute value token
								stream.Pop();
								token = stream.Peek();
							}

							if (token.TokenType != ModelTokenType.ObjectEnd)
							{
								throw new TokenException<ModelTokenType>(
									token,
									String.Format(JsonMLOutTransformer.ErrorUnterminatedAttributeBlock, token.TokenType));
							}

							// consume object end token
							stream.Pop();
							token = stream.Peek();
							break;
						}
						case ModelTokenType.ArrayEnd:
						{
							yield return MarkupGrammar.TokenElementEnd;

							stream.Pop();
							token = stream.Peek();
							break;
						}
						case ModelTokenType.Primitive:
						{
							yield return token.ChangeType(MarkupTokenType.Primitive);

							stream.Pop();
							token = stream.Peek();
							break;
						}
						default:
						{
							// the rest are invalid outside of attribute block
							throw new TokenException<ModelTokenType>(
								token,
								String.Format(JsonMLOutTransformer.ErrorUnexpectedToken, token.TokenType));
						}
					}
				}
			}

			#endregion IDataTransformer<MarkupTokenType,ModelTokenType> Members
		}
	}
}
