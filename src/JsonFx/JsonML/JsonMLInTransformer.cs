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
		/// Transforms markup tokens into common data tokens using the (lossless) JsonML model
		/// </summary>
		/// <remarks>
		/// JsonML Grammer: http://jsonml.org
		/// </remarks>
		public class JsonMLInTransformer : IDataTransformer<MarkupTokenType, CommonTokenType>
		{
			#region Constants

			private const string ErrorUnexpectedToken = "Unexpected token ({0})";

			#endregion Constants

			#region Fields

			private ITextStream Scanner = TextReaderStream.Null;

			#endregion Fields

			#region Properties

			/// <summary>
			/// Determines if whitespace nodes should be ommited
			/// </summary>
			/// <remarks>
			/// True to keep whitespace
			/// </remarks>
			public bool PreserveWhitespace
			{
				get;
				set;
			}

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

				IStream<Token<MarkupTokenType>> stream = Stream<Token<MarkupTokenType>>.Create(input);

				PrefixScopeChain scopeChain = new PrefixScopeChain();

				Token<MarkupTokenType> token = stream.Peek();
				while (!stream.IsCompleted)
				{
					switch (token.TokenType)
					{
						case MarkupTokenType.ElementBegin:
						case MarkupTokenType.ElementVoid:
						{
							bool hasProperties = false;
							bool isVoid = (token.TokenType == MarkupTokenType.ElementVoid);

							DataName tagName = token.Name;
							yield return CommonGrammar.TokenArrayBeginUnnamed;
							// NOTE: JSON doesn't support namespaces so resolve the name to prefix+':'+local-name
							yield return CommonGrammar.TokenPrimitive(tagName.ToPrefixedName());

							PrefixScopeChain.Scope scope = new PrefixScopeChain.Scope();
							string prefix = scopeChain.GetPrefix(tagName.NamespaceUri, false);
							if (!StringComparer.Ordinal.Equals(prefix, tagName.Prefix) &&
								!String.IsNullOrEmpty(tagName.NamespaceUri))
							{
								scope[tagName.Prefix] = tagName.NamespaceUri;

								// new namespace scope so need to emit xmlns
								hasProperties = true;
								yield return CommonGrammar.TokenObjectBeginUnnamed;
							}
							scope.TagName = tagName;
							scopeChain.Push(scope);

							stream.Pop();
							token = stream.Peek();
							while (!stream.IsCompleted &&
								token.TokenType == MarkupTokenType.Attribute)
							{
								if (!hasProperties)
								{
									hasProperties = true;
									yield return CommonGrammar.TokenObjectBeginUnnamed;
								}

								DataName attrName = token.Name;

								prefix = scopeChain.GetPrefix(attrName.NamespaceUri, false);
								if (!StringComparer.Ordinal.Equals(prefix, attrName.Prefix) &&
									!String.IsNullOrEmpty(attrName.NamespaceUri))
								{
									scope[attrName.Prefix] = attrName.NamespaceUri;
								}

								// NOTE: JSON doesn't support namespaces so resolve the name to prefix+':'+local-name
								yield return CommonGrammar.TokenProperty(new DataName(attrName.ToPrefixedName()));

								stream.Pop();
								token = stream.Peek();

								switch (token.TokenType)
								{
									case MarkupTokenType.Primitive:
									{
										yield return token.ChangeType(CommonTokenType.Primitive);
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

							if (hasProperties)
							{
								foreach (var xmlns in scope)
								{
									if (String.IsNullOrEmpty(xmlns.Key))
									{
										yield return CommonGrammar.TokenProperty("xmlns");
									}
									else
									{
										yield return CommonGrammar.TokenProperty(String.Concat("xmlns:", xmlns.Key));
									}
									yield return CommonGrammar.TokenPrimitive(xmlns.Value);
								}

								yield return CommonGrammar.TokenObjectEnd;
							}

							if (isVoid)
							{
								yield return CommonGrammar.TokenArrayEnd;
								scopeChain.Pop();
							}
							break;
						}
						case MarkupTokenType.ElementEnd:
						{
							if (scopeChain.Count > 0)
							{
								yield return CommonGrammar.TokenArrayEnd;
							}
							scopeChain.Pop();

							stream.Pop();
							token = stream.Peek();
							break;
						}
						case MarkupTokenType.Primitive:
						{
							if (token.Value is ITextFormattable<CommonTokenType> ||
								token.Value is ITextFormattable<MarkupTokenType>)
							{
								yield return token.ChangeType(CommonTokenType.Primitive);

								stream.Pop();
								token = stream.Peek();
								break;
							}

							string value = token.ValueAsString();

							stream.Pop();
							token = stream.Peek();
							while (!stream.IsCompleted &&
								(token.TokenType == MarkupTokenType.Primitive) &&
								!(token.Value is ITextFormattable<CommonTokenType>) &&
								!(token.Value is ITextFormattable<MarkupTokenType>))
							{
								// concatenate adjacent value nodes
								value = String.Concat(value, token.ValueAsString());

								stream.Pop();
								token = stream.Peek();
							}

							if (this.PreserveWhitespace || !IsNullOrWhiteSpace(value))
							{
								yield return CommonGrammar.TokenPrimitive(value);
							}
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

				while (scopeChain.Count > 0)
				{
					scopeChain.Pop();
					yield return CommonGrammar.TokenArrayEnd;
				}
			}

			#endregion IDataTransformer<CommonTokenType, MarkupTokenType> Members

			#region Utility Methods

			/// <summary>
			/// Checks if string is null, empty or entirely made up of whitespace
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			/// <remarks>
			/// Essentially the same as String.IsNullOrWhiteSpace from .NET 4.0
			/// with a simpler view of whitespace.
			/// </remarks>
			private static bool IsNullOrWhiteSpace(string value)
			{
				if (value != null)
				{
					for (int i=0, length=value.Length; i<length; i++)
					{
						if (!IsWhiteSpace(value[i]))
						{
							return false;
						}
					}
				}
				return true;
			}

			/// <summary>
			/// Checks if character is line ending, tab or space
			/// </summary>
			/// <param name="ch"></param>
			/// <returns></returns>
			private static bool IsWhiteSpace(char ch)
			{
				return
					(ch == ' ') |
					(ch == '\n') ||
					(ch == '\r') ||
					(ch == '\t');
			}

			#endregion Utility Methods
		}
	}
}
