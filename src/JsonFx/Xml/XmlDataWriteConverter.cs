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
using System.Text;

using JsonFx.Common;
using JsonFx.IO;
using JsonFx.Markup;
using JsonFx.Serialization;

namespace JsonFx.Xml
{
	public partial class XmlWriter
	{
		/// <summary>
		/// Transforms common data tokens into markup tokens using an XML-data model
		/// </summary>
		public class XmlDataWriteConverter : IDataTransformer<CommonTokenType, MarkupTokenType>
		{
			#region Constants

			private const string ErrorUnexpectedToken = "Unexpected token ({0})";

			#endregion Constants

			#region Fields

			private readonly PrefixScopeChain ScopeChain = new PrefixScopeChain();
			private readonly DataWriterSettings Settings;
			private int depth;
			private bool pendingNewLine;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public XmlDataWriteConverter(DataWriterSettings settings)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}

				this.Settings = settings;
			}

			#endregion Init

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
				List<Token<MarkupTokenType>> output = new List<Token<MarkupTokenType>>();

				this.ScopeChain.Clear();

				while (!stream.IsCompleted)
				{
					this.TransformValue(output, stream, DataName.Empty);
				}

				return output;
			}

			#endregion IDataTransformer<MarkupTokenType,CommonTokenType> Members

			#region CommonTokenType to MarkupTokenType Transformation Methods

			/// <summary>
			/// Formats the token sequence to the output
			/// </summary>
			/// <param name="output"></param>
			/// <param name="input"></param>
			private void TransformValue(List<Token<MarkupTokenType>> output, IStream<Token<CommonTokenType>> input, DataName elementName)
			{
				if (this.pendingNewLine)
				{
					if (this.Settings.PrettyPrint)
					{
						this.depth++;
						this.EmitNewLine(output);
					}
					this.pendingNewLine = false;
				}

				Token<CommonTokenType> token = input.Peek();
				switch (token.TokenType)
				{
					case CommonTokenType.ArrayBegin:
					{
						this.TransformArray(output, input, elementName);
						break;
					}
					case CommonTokenType.ObjectBegin:
					{
						this.TransformObject(output, input, elementName);
						break;
					}
					case CommonTokenType.Primitive:
					{
						input.Pop();

						string value = token.ValueAsString();
						if (String.IsNullOrEmpty(value))
						{
							elementName = this.EnsureName(elementName.IsEmpty ? token.Name : elementName, null);
							this.EmitTag(output, elementName, null, MarkupTokenType.ElementVoid);
						}
						else
						{
							elementName = this.EnsureName(elementName.IsEmpty ? token.Name : elementName, token.Value.GetType());

							this.EmitTag(output, elementName, null, MarkupTokenType.ElementBegin);
							output.Add(MarkupGrammar.TokenText(value));
							this.EmitTag(output, elementName, null, MarkupTokenType.ElementEnd);
						}
						break;
					}
					default:
					{
						throw new TokenException<CommonTokenType>(
							token,
							String.Format(ErrorUnexpectedToken, token.TokenType));
					}
				}
			}

			private void TransformArray(List<Token<MarkupTokenType>> output, IStream<Token<CommonTokenType>> input, DataName elementName)
			{
				Token<CommonTokenType> token = input.Pop();

				// ensure element has a name
				elementName = this.EnsureName(elementName.IsEmpty ? token.Name : elementName, typeof(Array));

				// TODO: figure out a way to surface XmlArrayItemAttribute name
				DataName itemName = DataName.Empty;//new DataName("arrayItem");

				this.EmitTag(output, elementName, null, MarkupTokenType.ElementBegin);
				this.pendingNewLine = true;

				bool needsValueDelim = false;
				while (!input.IsCompleted)
				{
					token = input.Peek();
					switch (token.TokenType)
					{
						case CommonTokenType.ArrayEnd:
						{
							input.Pop();

							if (this.pendingNewLine)
							{
								this.pendingNewLine = false;
							}
							else if (this.Settings.PrettyPrint)
							{
								this.depth--;
								this.EmitNewLine(output);
							}

							this.EmitTag(output, elementName, null, MarkupTokenType.ElementEnd);
							this.pendingNewLine = true;
							return;
						}
						case CommonTokenType.ArrayBegin:
						case CommonTokenType.ObjectBegin:
						case CommonTokenType.Primitive:
						{
							if (needsValueDelim)
							{
								throw new TokenException<CommonTokenType>(token, "Missing value delimiter");
							}

							if (this.pendingNewLine)
							{
								if (this.Settings.PrettyPrint)
								{
									this.depth++;
									this.EmitNewLine(output);
								}
								this.pendingNewLine = false;
							}

							this.TransformValue(output, input, itemName);

							this.pendingNewLine = false;
							needsValueDelim = true;
							break;
						}
						case CommonTokenType.ValueDelim:
						{
							input.Pop();

							if (!needsValueDelim)
							{
								throw new TokenException<CommonTokenType>(token, "Missing array item");
							}

							if (this.Settings.PrettyPrint)
							{
								this.EmitNewLine(output);
							}
							needsValueDelim = false;
							break;
						}
						default:
						{
							throw new TokenException<CommonTokenType>(
								token,
								String.Format(ErrorUnexpectedToken, token.TokenType));
						}
					}
				}
			}

			private void TransformObject(List<Token<MarkupTokenType>> output, IStream<Token<CommonTokenType>> input, DataName elementName)
			{
				Token<CommonTokenType> token = input.Pop();

				// ensure element has a name
				elementName = this.EnsureName(elementName.IsEmpty ? token.Name : elementName, typeof(Object));

				bool needsEndTag = true;
				SortedList<DataName, string> attributes = null;

				bool needsValueDelim = false;
				while (!input.IsCompleted)
				{
					token = input.Peek();
					switch (token.TokenType)
					{
						case CommonTokenType.ObjectEnd:
						{
							input.Pop();

							if (needsEndTag)
							{
								needsEndTag = false;
								// write out namespaces and attributes
								this.EmitTag(output, elementName, attributes, MarkupTokenType.ElementBegin);
								this.pendingNewLine = true;
							}

							if (this.pendingNewLine)
							{
								this.pendingNewLine = false;
							}
							else if (this.Settings.PrettyPrint)
							{
								this.depth--;
								this.EmitNewLine(output);
							}

							this.EmitTag(output, elementName, null, MarkupTokenType.ElementEnd);
							this.pendingNewLine = true;
							return;
						}
						case CommonTokenType.Property:
						{
							input.Pop();

							if (needsValueDelim)
							{
								throw new TokenException<CommonTokenType>(token, "Missing value delimiter");
							}

							if (needsEndTag)
							{
								if (token.Name.IsAttribute)
								{
									if (attributes == null)
									{
										// allocate and sort attributes
										attributes = new SortedList<DataName, string>();
									}
									DataName attrName = token.Name;

									// consume attribute value
									token = input.Peek();
									if (token.TokenType != CommonTokenType.Primitive)
									{
										throw new TokenException<CommonTokenType>(token, "Attribute values must be primitive input.");
									}
									input.Pop();

									if (attrName.IsEmpty)
									{
										attrName = token.Name;
									}

									// according to XML rules cannot duplicate attribute names
									if (!attributes.ContainsKey(attrName))
									{
										attributes.Add(attrName, token.ValueAsString());
									}

									this.pendingNewLine = false;
									needsValueDelim = true;
									break;
								}
								else
								{
									needsEndTag = false;

									// end attributes with first non-attribute child
									// write out namespaces and attributes
									this.EmitTag(output, elementName, attributes, MarkupTokenType.ElementBegin);
									this.pendingNewLine = true;
								}
							}

							if (this.pendingNewLine)
							{
								if (this.Settings.PrettyPrint)
								{
									this.depth++;
									this.EmitNewLine(output);
								}
								this.pendingNewLine = false;
							}

							this.TransformValue(output, input, token.Name);

							this.pendingNewLine = false;
							needsValueDelim = true;
							break;
						}
						case CommonTokenType.ValueDelim:
						{
							input.Pop();

							if (!needsValueDelim)
							{
								throw new TokenException<CommonTokenType>(token, "Missing object property");
							}

							if (this.Settings.PrettyPrint)
							{
								this.EmitNewLine(output);
							}
							needsValueDelim = false;
							break;
						}
						default:
						{
							throw new TokenException<CommonTokenType>(
								token,
								String.Format(ErrorUnexpectedToken, token.TokenType));
						}
					}
				}
			}

			#endregion CommonTokenType to MarkupTokenType Transformation Methods

			#region Emit MarkupTokenType Methods

			private void EmitTag(List<Token<MarkupTokenType>> output, DataName elementName, SortedList<DataName, string> attributes, MarkupTokenType tagType)
			{
				if (this.pendingNewLine)
				{
					if (this.Settings.PrettyPrint)
					{
						this.depth++;
						this.EmitNewLine(output);
					}
					this.pendingNewLine = false;
				}

				PrefixScopeChain.Scope scope = new PrefixScopeChain.Scope();
				scope.TagName = elementName;

				if (!this.ScopeChain.ContainsNamespace(elementName.NamespaceUri))
				{
					scope[elementName.Prefix] = elementName.NamespaceUri;
				}
				this.ScopeChain.Push(scope);

				switch (tagType)
				{
					case MarkupTokenType.ElementVoid:
					{
						output.Add(MarkupGrammar.TokenElementVoid(elementName));
						break;
					}
					case MarkupTokenType.ElementEnd:
					{
						output.Add(MarkupGrammar.TokenElementEnd(elementName));
						break;
					}
					default:
					case MarkupTokenType.ElementBegin:
					{
						output.Add(MarkupGrammar.TokenElementBegin(elementName));
						break;
					}
				}

				if (attributes != null)
				{
					foreach (var attr in attributes)
					{
						string prefix = this.ScopeChain.EnsurePrefix(attr.Key.Prefix, attr.Key.NamespaceUri);

						if (!this.ScopeChain.ContainsNamespace(attr.Key.NamespaceUri))
						{
							scope[this.ScopeChain.EnsurePrefix(attr.Key.Prefix, attr.Key.NamespaceUri)] = elementName.NamespaceUri;
						}

						DataName attrName =
							(prefix == attr.Key.Prefix) ?
							attr.Key : new DataName(attr.Key.LocalName, prefix, attr.Key.NamespaceUri, true);

						output.Add(MarkupGrammar.TokenAttribute(attrName));
						output.Add(MarkupGrammar.TokenText(attr.Value));
					}

					attributes.Clear();
				}
			}

			private void EmitNewLine(List<Token<MarkupTokenType>> output)
			{
				bool tabIsEmpty = String.IsNullOrEmpty(this.Settings.Tab);
				if (tabIsEmpty &&
					String.IsNullOrEmpty(this.Settings.NewLine))
				{
					return;
				}

				// emit CRLF
				StringBuilder buffer = new StringBuilder(this.Settings.NewLine);

				if (!tabIsEmpty)
				{
					if (this.Settings.Tab.Length == 1)
					{
						buffer.Append(this.Settings.Tab[0], this.depth);
					}
					else
					{
						for (int i=0; i<this.depth; i++)
						{
							// indent next line accordingly
							buffer.Append(this.Settings.Tab);
						}
					}
				}

				output.Add(MarkupGrammar.TokenWhitespace(buffer.ToString()));
			}

			#endregion Emit MarkupTokenType Methods

			#region Utility Methods

			private DataName EnsureName(DataName name, Type type)
			{
				// String.Empty is a valid DataName.LocalName, so must replace
				if (String.IsNullOrEmpty(name.LocalName))
				{
					return this.Settings.Resolver.LoadTypeName(type);
				}

				// due to a bug in System.Xml.XmlCharType,
				// certain chars are not allowed that XML allows
				// so we must use XmlConvert to encode with same bug

				// XML only supports a subset of chars that DataName.LocalName does
				string localName = System.Xml.XmlConvert.EncodeLocalName(name.LocalName);
				if (name.LocalName != localName)
				{
					return new DataName(localName, name.Prefix, name.NamespaceUri, name.IsAttribute);
				}

				return name;
			}

			#endregion Utility Methods
		}
	}
}
