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

using JsonFx.Model;
using JsonFx.IO;
using JsonFx.Markup;
using JsonFx.Serialization;

#if SILVERLIGHT
using CanonicalList=System.Collections.Generic.Dictionary<JsonFx.Serialization.DataName, JsonFx.Serialization.Token<JsonFx.Model.ModelTokenType>>;
#else
using CanonicalList=System.Collections.Generic.SortedList<JsonFx.Serialization.DataName, JsonFx.Serialization.Token<JsonFx.Model.ModelTokenType>>;
#endif

namespace JsonFx.Xml
{
	public partial class XmlWriter
	{
		/// <summary>
		/// Transforms Common Model tokens into markup tokens using an XML-data model
		/// </summary>
		public class XmlOutTransformer : IDataTransformer<ModelTokenType, MarkupTokenType>
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
			public XmlOutTransformer(DataWriterSettings settings)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}

				this.Settings = settings;
			}

			#endregion Init

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

				this.ScopeChain.Clear();

				while (!stream.IsCompleted)
				{
					this.TransformValue(output, stream, DataName.Empty);
				}

				return output;
			}

			#endregion IDataTransformer<MarkupTokenType,ModelTokenType> Members

			#region ModelTokenType to MarkupTokenType Transformation Methods

			/// <summary>
			/// Formats the token sequence to the output
			/// </summary>
			/// <param name="output"></param>
			/// <param name="input"></param>
			private void TransformValue(List<Token<MarkupTokenType>> output, IStream<Token<ModelTokenType>> input, DataName propertyName)
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

				Token<ModelTokenType> token = input.Peek();
				switch (token.TokenType)
				{
					case ModelTokenType.ArrayBegin:
					{
						this.TransformArray(output, input, propertyName);
						break;
					}
					case ModelTokenType.ObjectBegin:
					{
						this.TransformObject(output, input, propertyName);
						break;
					}
					case ModelTokenType.Primitive:
					{
						input.Pop();

						if (propertyName.IsEmpty)
						{
							propertyName = token.Name;
						}

						if (token.Value == null)
						{
							propertyName = this.EncodeName(propertyName, null);
							this.EmitTag(output, propertyName, null, MarkupTokenType.ElementVoid);
						}
						else
						{
							propertyName = this.EncodeName(propertyName, token.Value.GetType());

							this.EmitTag(output, propertyName, null, MarkupTokenType.ElementBegin);
							output.Add(token.ChangeType(MarkupTokenType.Primitive));
							this.EmitTag(output, propertyName, null, MarkupTokenType.ElementEnd);
						}
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

			private void TransformArray(List<Token<MarkupTokenType>> output, IStream<Token<ModelTokenType>> input, DataName propertyName)
			{
				Token<ModelTokenType> token = input.Pop();

				// ensure element has a name
				propertyName = this.EncodeName(propertyName.IsEmpty ? token.Name : propertyName, typeof(Array));

				// TODO: figure out a way to surface XmlArrayItemAttribute name
				DataName itemName = new DataName("item");

				this.EmitTag(output, propertyName, null, MarkupTokenType.ElementBegin);
				this.pendingNewLine = true;

				bool needsValueDelim = false;
				while (!input.IsCompleted)
				{
					token = input.Peek();
					switch (token.TokenType)
					{
						case ModelTokenType.ArrayEnd:
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

							this.EmitTag(output, propertyName, null, MarkupTokenType.ElementEnd);
							this.pendingNewLine = true;
							return;
						}
						case ModelTokenType.ArrayBegin:
						case ModelTokenType.ObjectBegin:
						case ModelTokenType.Primitive:
						{
							if (needsValueDelim)
							{
								if (this.Settings.PrettyPrint)
								{
									this.EmitNewLine(output);
								}
								needsValueDelim = false;
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
						default:
						{
							throw new TokenException<ModelTokenType>(
								token,
								String.Format(ErrorUnexpectedToken, token.TokenType));
						}
					}
				}
			}

			private void TransformObject(List<Token<MarkupTokenType>> output, IStream<Token<ModelTokenType>> input, DataName propertyName)
			{
				Token<ModelTokenType> token = input.Pop();

				// ensure element has a name
				propertyName = this.EncodeName(propertyName.IsEmpty ? token.Name : propertyName, typeof(Object));

				bool needsBeginTag = true;
				IDictionary<DataName, Token<ModelTokenType>> attributes = null;

				bool needsValueDelim = false;
				while (!input.IsCompleted)
				{
					token = input.Peek();
					switch (token.TokenType)
					{
						case ModelTokenType.ObjectEnd:
						{
							input.Pop();

							if (needsBeginTag)
							{
								needsBeginTag = false;
								// write out namespaces and attributes
								this.EmitTag(output, propertyName, attributes, MarkupTokenType.ElementBegin);
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

							this.EmitTag(output, propertyName, null, MarkupTokenType.ElementEnd);
							this.pendingNewLine = true;
							return;
						}
						case ModelTokenType.Property:
						{
							input.Pop();

							if (needsValueDelim)
							{
								if (this.Settings.PrettyPrint)
								{
									this.EmitNewLine(output);
								}
								needsValueDelim = false;
							}

							if (needsBeginTag)
							{
								if (token.Name.IsAttribute)
								{
									if (attributes == null)
									{
										// allocate and sort attributes
										attributes = new CanonicalList();
									}
									DataName attrName = token.Name;

									// consume attribute value
									token = input.Peek();
									if (token.TokenType != ModelTokenType.Primitive)
									{
										throw new TokenException<ModelTokenType>(token, "Attribute values must be primitive input.");
									}
									input.Pop();

									if (attrName.IsEmpty)
									{
										attrName = token.Name;
									}

									// according to XML rules cannot duplicate attribute names
									if (!attributes.ContainsKey(attrName))
									{
										attributes.Add(attrName, token);
									}

									this.pendingNewLine = false;
									needsValueDelim = true;
									break;
								}
								else
								{
									needsBeginTag = false;

									// end attributes with first non-attribute child
									// write out namespaces and attributes
									this.EmitTag(output, propertyName, attributes, MarkupTokenType.ElementBegin);
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
						default:
						{
							throw new TokenException<ModelTokenType>(
								token,
								String.Format(ErrorUnexpectedToken, token.TokenType));
						}
					}
				}
			}

			#endregion ModelTokenType to MarkupTokenType Transformation Methods

			#region Emit MarkupTokenType Methods

			private void EmitTag(List<Token<MarkupTokenType>> output, DataName elementName, IDictionary<DataName, Token<ModelTokenType>> attributes, MarkupTokenType tagType)
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
						output.Add(MarkupGrammar.TokenElementEnd);
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
						output.Add(MarkupGrammar.TokenAttribute(attr.Key));
						output.Add(attr.Value.ChangeType(MarkupTokenType.Primitive));
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

				output.Add(MarkupGrammar.TokenPrimitive(buffer.ToString()));
			}

			#endregion Emit MarkupTokenType Methods

			#region Utility Methods

			private DataName EncodeName(DataName name, Type type)
			{
				// String.Empty is a valid DataName.LocalName, so must replace with type name
				if (String.IsNullOrEmpty(name.LocalName))
				{
					foreach (DataName typeName in this.Settings.Resolver.LoadTypeName(type))
					{
						if (typeName.IsEmpty)
						{
							continue;
						}

						return typeName;
					}
				}

				// due to a bug in System.Xml.XmlCharType,
				// certain chars are not allowed that XML allows
				// so we must use XmlConvert to encode with same bug

				// XML only supports a subset of chars that DataName.LocalName does
				string localName = System.Xml.XmlConvert.EncodeLocalName(name.LocalName);
				if (name.LocalName == localName)
				{
					return name;
				}

				return new DataName(localName, name.Prefix, name.NamespaceUri, name.IsAttribute);
			}

			#endregion Utility Methods
		}
	}
}
