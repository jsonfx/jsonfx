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

namespace JsonFx.Xml
{
	public partial class XmlReader
	{
		/// <summary>
		/// Transforms markup tokens into common data tokens using an XML-data model
		/// </summary>
		public class XmlDataReadConverter : IDataTransformer<MarkupTokenType, CommonTokenType>
		{
			#region Constants

			private const string ErrorUnexpectedToken = "Unexpected token ({0})";

			#endregion Constants

			#region Fields

			private readonly PrefixScopeChain ScopeChain = new PrefixScopeChain();
			private readonly DataReaderSettings Settings;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public XmlDataReadConverter(DataReaderSettings settings)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}

				this.Settings = settings;
			}

			#endregion Init

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

				this.ScopeChain.Clear();

				IStream<Token<MarkupTokenType>> stream = new Stream<Token<MarkupTokenType>>(input);
				while (!stream.IsCompleted)
				{
					this.TransformValue(output, stream, DataName.Empty);
				}

				return output;
			}

			#endregion IDataTransformer<CommonTokenType, MarkupTokenType> Members

			#region CommonTokenType to MarkupTokenType Transformation Methods

			/// <summary>
			/// Formats the token sequence to the output
			/// </summary>
			/// <param name="output"></param>
			/// <param name="input"></param>
			private void TransformValue(List<Token<CommonTokenType>> output, IStream<Token<MarkupTokenType>> input, DataName elementName)
			{
				Token<MarkupTokenType> token = input.Peek();
				switch (token.TokenType)
				{
					case MarkupTokenType.ElementVoid:
					case MarkupTokenType.ElementBegin:
					case MarkupTokenType.ElementEnd:
					case MarkupTokenType.Attribute:
					case MarkupTokenType.TextValue:
					case MarkupTokenType.Whitespace:
					{
						input.Pop();
						token = input.Peek();
						break;
					}
					case MarkupTokenType.UnparsedBlock:
					case MarkupTokenType.None:
					default:
					{
						throw new TokenException<MarkupTokenType>(
							token,
							String.Format(ErrorUnexpectedToken, token.TokenType));
					}
				}
			}

			#endregion CommonTokenType to MarkupTokenType Transformation Methods
		}
	}
}
