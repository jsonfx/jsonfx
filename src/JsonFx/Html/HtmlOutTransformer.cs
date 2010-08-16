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

namespace JsonFx.Html
{
	/// <summary>
	/// Renders Common Model Tokens into a semantic HTML representation of the data structure
	/// </summary>
	public class HtmlOutTransformer : IDataTransformer<ModelTokenType, MarkupTokenType>
	{
		#region Constants

		private const string ErrorUnexpectedToken = "Unexpected token ({0})";

		private static readonly DataName RootTagName = new DataName("div");

		private static readonly DataName ArrayTagName = new DataName("ol");
		private static readonly DataName ArrayItemTagName = new DataName("li");

		private static readonly DataName ObjectTagName = new DataName("dl");
		private static readonly DataName ObjectPropertyKeyTagName = new DataName("dt");
		private static readonly DataName ObjectPropertyValueTagName = new DataName("dd");

		private static readonly DataName PrimitiveTagName = new DataName("span");

		private static readonly DataName HintAttributeName = new DataName("title");

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
			List<Token<MarkupTokenType>> output = new List<Token<MarkupTokenType>>();

			while (!stream.IsCompleted)
			{
				output.Add(MarkupGrammar.TokenElementBegin(HtmlOutTransformer.RootTagName));
				this.TransformValue(output, stream);
				output.Add(MarkupGrammar.TokenElementEnd);
			}

			return output;
		}

		#endregion IDataTransformer<MarkupTokenType,ModelTokenType> Members

		#region Transformation Methods

		/// <summary>
		/// Formats the token sequence to the output
		/// </summary>
		/// <param name="output"></param>
		/// <param name="input"></param>
		private void TransformValue(List<Token<MarkupTokenType>> output, IStream<Token<ModelTokenType>> input)
		{
			Token<ModelTokenType> token = input.Peek();
			switch (token.TokenType)
			{
				case ModelTokenType.ArrayBegin:
				{
					this.TransformArray(output, input);
					break;
				}
				case ModelTokenType.ObjectBegin:
				{
					this.TransformObject(output, input);
					break;
				}
				case ModelTokenType.Primitive:
				{
					this.TransformPrimitive(output, input);
					break;
				}
				default:
				{
					throw new TokenException<ModelTokenType>(
						token,
						String.Format(ErrorUnexpectedToken, token.TokenType));
				}
			}
		}

		private void TransformPrimitive(List<Token<MarkupTokenType>> output, IStream<Token<ModelTokenType>> input)
		{
			Token<ModelTokenType> token = input.Pop();

			bool hasName = !token.Name.IsEmpty;
			if (hasName)
			{
				output.Add(MarkupGrammar.TokenElementBegin(HtmlOutTransformer.PrimitiveTagName));
				output.Add(MarkupGrammar.TokenAttribute(HtmlOutTransformer.HintAttributeName));
				output.Add(MarkupGrammar.TokenPrimitive(token.Name));
			}
			output.Add(token.ChangeType(MarkupTokenType.Primitive));
			if (hasName)
			{
				output.Add(MarkupGrammar.TokenElementEnd);
			}
		}

		private void TransformArray(List<Token<MarkupTokenType>> output, IStream<Token<ModelTokenType>> input)
		{
			Token<ModelTokenType> token = input.Pop();

			output.Add(MarkupGrammar.TokenElementBegin(HtmlOutTransformer.ArrayTagName));
			if (!token.Name.IsEmpty)
			{
				output.Add(MarkupGrammar.TokenAttribute(HtmlOutTransformer.HintAttributeName));
				output.Add(MarkupGrammar.TokenPrimitive(token.Name));
			}

			while (!input.IsCompleted)
			{
				token = input.Peek();
				switch (token.TokenType)
				{
					case ModelTokenType.ArrayEnd:
					{
						input.Pop();

						output.Add(MarkupGrammar.TokenElementEnd);
						return;
					}
					case ModelTokenType.ArrayBegin:
					case ModelTokenType.ObjectBegin:
					case ModelTokenType.Primitive:
					{
						output.Add(MarkupGrammar.TokenElementBegin(HtmlOutTransformer.ArrayItemTagName));
						this.TransformValue(output, input);
						output.Add(MarkupGrammar.TokenElementEnd);
						break;
					}
					default:
					{
						throw new TokenException<ModelTokenType>(
							token,
							String.Format(ErrorUnexpectedToken, token.TokenType));
					}
				}
			}
		}

		private void TransformObject(List<Token<MarkupTokenType>> output, IStream<Token<ModelTokenType>> input)
		{
			Token<ModelTokenType> token = input.Pop();

			output.Add(MarkupGrammar.TokenElementBegin(HtmlOutTransformer.ObjectTagName));
			if (!token.Name.IsEmpty)
			{
				output.Add(MarkupGrammar.TokenAttribute(HtmlOutTransformer.HintAttributeName));
				output.Add(MarkupGrammar.TokenPrimitive(token.Name));
			}

			while (!input.IsCompleted)
			{
				token = input.Peek();
				switch (token.TokenType)
				{
					case ModelTokenType.ObjectEnd:
					{
						input.Pop();

						output.Add(MarkupGrammar.TokenElementEnd);
						return;
					}
					case ModelTokenType.Property:
					{
						input.Pop();

						output.Add(MarkupGrammar.TokenElementBegin(HtmlOutTransformer.ObjectPropertyKeyTagName));
						output.Add(MarkupGrammar.TokenPrimitive(token.Name));
						output.Add(MarkupGrammar.TokenElementEnd);

						output.Add(MarkupGrammar.TokenElementBegin(HtmlOutTransformer.ObjectPropertyValueTagName));
						this.TransformValue(output, input);
						output.Add(MarkupGrammar.TokenElementEnd);
						break;
					}
					default:
					{
						throw new TokenException<ModelTokenType>(
							token,
							String.Format(ErrorUnexpectedToken, token.TokenType));
					}
				}
			}
		}

		#endregion Transformation Methods
	}
}
