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
using System.Text.RegularExpressions;

using JsonFx.IO;
using JsonFx.Markup;
using JsonFx.Model;
using JsonFx.Serialization;
using JsonFx.Utils;

namespace JsonFx.JsonML
{
	public partial class JsonMLReader
	{
		/// <summary>
		/// Transforms markup tokens into Common Model tokens using the (lossless) JsonML model
		/// </summary>
		/// <remarks>
		/// JsonML Grammer: http://jsonml.org
		/// </remarks>
		public class JsonMLInTransformer : IDataTransformer<MarkupTokenType, ModelTokenType>
		{
			#region Constants

			private static readonly Regex RegexWhitespace = new Regex(@"\s+",
#if !SILVERLIGHT
			RegexOptions.Compiled|
#endif
			RegexOptions.CultureInvariant|RegexOptions.ECMAScript);

			private const string SingleSpace = " ";

			private const string ErrorUnexpectedToken = "Unexpected token ({0})";

			#endregion Constants

			#region Properties

			/// <summary>
			/// Determines how whitespace should be handled
			/// </summary>
			public WhitespaceType Whitespace
			{
				get;
				set;
			}

			#endregion Properties

			#region IDataTransformer<ModelTokenType, MarkupTokenType> Members

			/// <summary>
			/// Consumes a sequence of tokens and produces a token sequence of a different type
			/// </summary>
			public IEnumerable<Token<ModelTokenType>> Transform(IEnumerable<Token<MarkupTokenType>> input)
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
							yield return ModelGrammar.TokenArrayBeginUnnamed;
							// NOTE: JSON doesn't support namespaces so resolve the name to prefix+':'+local-name
							yield return ModelGrammar.TokenPrimitive(tagName.ToPrefixedName());

							PrefixScopeChain.Scope scope = new PrefixScopeChain.Scope();
							string prefix = scopeChain.GetPrefix(tagName.NamespaceUri, false);
							if (!StringComparer.Ordinal.Equals(prefix, tagName.Prefix) &&
								!String.IsNullOrEmpty(tagName.NamespaceUri))
							{
								scope[tagName.Prefix] = tagName.NamespaceUri;

								// new namespace scope so need to emit xmlns
								hasProperties = true;
								yield return ModelGrammar.TokenObjectBeginUnnamed;
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
									yield return ModelGrammar.TokenObjectBeginUnnamed;
								}

								DataName attrName = token.Name;

								prefix = scopeChain.GetPrefix(attrName.NamespaceUri, false);
								if (!StringComparer.Ordinal.Equals(prefix, attrName.Prefix) &&
									!String.IsNullOrEmpty(attrName.NamespaceUri))
								{
									scope[attrName.Prefix] = attrName.NamespaceUri;
								}

								// NOTE: JSON doesn't support namespaces so resolve the name to prefix+':'+local-name
								yield return ModelGrammar.TokenProperty(new DataName(attrName.ToPrefixedName()));

								stream.Pop();
								token = stream.Peek();

								switch (token.TokenType)
								{
									case MarkupTokenType.Primitive:
									{
										yield return token.ChangeType(ModelTokenType.Primitive);
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
										yield return ModelGrammar.TokenProperty("xmlns");
									}
									else
									{
										yield return ModelGrammar.TokenProperty(String.Concat("xmlns:", xmlns.Key));
									}
									yield return ModelGrammar.TokenPrimitive(xmlns.Value);
								}

								yield return ModelGrammar.TokenObjectEnd;
							}

							if (isVoid)
							{
								yield return ModelGrammar.TokenArrayEnd;
								scopeChain.Pop();
							}
							break;
						}
						case MarkupTokenType.ElementEnd:
						{
							if (scopeChain.Count > 0)
							{
								yield return ModelGrammar.TokenArrayEnd;
							}
							scopeChain.Pop();

							stream.Pop();
							token = stream.Peek();
							break;
						}
						case MarkupTokenType.Primitive:
						{
							if (token.Value is ITextFormattable<ModelTokenType> ||
								token.Value is ITextFormattable<MarkupTokenType>)
							{
								yield return token.ChangeType(ModelTokenType.Primitive);

								stream.Pop();
								token = stream.Peek();
								break;
							}

							string value = token.ValueAsString();

							stream.Pop();
							token = stream.Peek();
							while (!stream.IsCompleted &&
								(token.TokenType == MarkupTokenType.Primitive) &&
								!(token.Value is ITextFormattable<ModelTokenType>) &&
								!(token.Value is ITextFormattable<MarkupTokenType>))
							{
								// concatenate adjacent value nodes
								value = String.Concat(value, token.ValueAsString());

								stream.Pop();
								token = stream.Peek();
							}

							switch (this.Whitespace)
							{
								case WhitespaceType.Normalize:
								{
									// replace whitespace chunks with single space (HTML-style normalization)
									value = JsonMLInTransformer.RegexWhitespace.Replace(value, JsonMLInTransformer.SingleSpace);
									goto default;
								}
								case WhitespaceType.None:
								{
									if (CharUtility.IsNullOrWhiteSpace(value))
									{
										break;
									}
									goto default;
								}
								case WhitespaceType.Preserve:
								default:
								{
									yield return ModelGrammar.TokenPrimitive(value);
									break;
								}
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
					yield return ModelGrammar.TokenArrayEnd;
				}
			}

			#endregion IDataTransformer<ModelTokenType, MarkupTokenType> Members
		}
	}
}
