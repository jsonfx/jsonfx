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
		public class XmlInTransformer : IDataTransformer<MarkupTokenType, CommonTokenType>
		{
			#region Constants

			private const string ErrorUnexpectedToken = "Unexpected token ({0})";
			private const string ErrorUnterminatedObject = "Unterminated object";
			private const string ErrorInvalidAttribute = "Invalid attribute value token ({0})";

			private static DataName DefaultObjectName = new DataName(typeof(Object));
			private static DataName DefaultArrayName = new DataName(typeof(Array));
			private static DataName DefaultItemName = new DataName("item");

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
			public XmlInTransformer(DataReaderSettings settings)
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

				this.ScopeChain.Clear();

				IStream<Token<MarkupTokenType>> stream = Stream<Token<MarkupTokenType>>.Create(input);
				while (!stream.IsCompleted)
				{
					foreach (var token in this.TransformValue(stream, true))
					{
						yield return token;
					}
				}
			}

			#endregion IDataTransformer<CommonTokenType, MarkupTokenType> Members

			#region CommonTokenType to MarkupTokenType Transformation Methods

			/// <summary>
			/// Formats the token sequence to the output
			/// </summary>
			/// <param name="output"></param>
			/// <param name="input"></param>
			private IList<Token<CommonTokenType>> TransformValue(IStream<Token<MarkupTokenType>> input, bool isStandAlone)
			{
				Token<MarkupTokenType> token = input.Peek();
				switch (token.TokenType)
				{
					case MarkupTokenType.Primitive:
					case MarkupTokenType.UnparsedBlock:
					{
						input.Pop();

						return new []
							{
								token.ChangeType(CommonTokenType.Primitive)
							};
					}
					case MarkupTokenType.ElementBegin:
					case MarkupTokenType.ElementVoid:
					{
						return this.TransformElement(input, isStandAlone);
					}
					default:
					{
						throw new TokenException<MarkupTokenType>(
							token,
							String.Format(XmlInTransformer.ErrorUnexpectedToken, token.TokenType));
					}
				}
			}

			private IList<Token<CommonTokenType>> TransformElement(IStream<Token<MarkupTokenType>> input, bool isStandAlone)
			{
				Token<MarkupTokenType> token = input.Peek();

				DataName elementName = this.DecodeName(token.Name, typeof(Object));
				bool isVoid = (token.TokenType == MarkupTokenType.ElementVoid);
				input.Pop();

				IDictionary<DataName, IList<IList<Token<CommonTokenType>>>> children = null;
				while (!input.IsCompleted)
				{
					token = input.Peek();
					if (token.TokenType == MarkupTokenType.ElementEnd ||
						(isVoid && token.TokenType != MarkupTokenType.Attribute))
					{
						if (!isVoid)
						{
							input.Pop();
						}

						List<Token<CommonTokenType>> output = new List<Token<CommonTokenType>>();

						if ((children == null) ||
							(children.Count <= 1) ||
							elementName == XmlInTransformer.DefaultArrayName)
						{
							DataName childName = DataName.Empty;
							IList<IList<Token<CommonTokenType>>> items = null;
							if (children != null)
							{
								// grab the first
								using (var enumerator = children.GetEnumerator())
								{
									if (enumerator.MoveNext())
									{
										items = enumerator.Current.Value;
										childName = enumerator.Current.Key;
									}
								}
							}

							if ((items != null && items.Count > 1) ||
								(items == null && elementName == XmlInTransformer.DefaultArrayName) ||
								childName == XmlInTransformer.DefaultItemName)
							{
								// if only child has more than one grandchild
								// then whole element is acutally an array
								output.Add(elementName.IsEmpty ? CommonGrammar.TokenArrayBeginUnnamed : CommonGrammar.TokenArrayBegin(this.DecodeName(elementName, typeof(Array))));

								if (items != null)
								{
									foreach (var item in items)
									{
										output.AddRange(item);
									}
								}

								output.Add(CommonGrammar.TokenArrayEnd);
								return output;
							}
						}

						if (isStandAlone)
						{
							output.Add(elementName.IsEmpty ? CommonGrammar.TokenObjectBeginUnnamed : CommonGrammar.TokenObjectBegin(elementName));
						}

						if (children != null)
						{
							foreach (var property in children)
							{
								if (property.Value.Count == 1)
								{
									if (isStandAlone)
									{
										// if the parent is a stand alone object then child is a property
										DataName name = this.DecodeName(property.Key, typeof(Object));
										output.Add(name.IsEmpty ? CommonGrammar.TokenProperty(elementName) : CommonGrammar.TokenProperty(name));
									}
									output.AddRange(property.Value[0]);
									continue;
								}

								if (property.Key.IsEmpty)
								{
									// skip mixed content
									continue;
								}

								// wrap values in array
								output.Add(property.Key.IsEmpty ? CommonGrammar.TokenArrayBeginUnnamed : CommonGrammar.TokenArrayBegin(this.DecodeName(property.Key, typeof(Array))));
								foreach (var item in property.Value)
								{
									output.AddRange(item);
								}
								output.Add(CommonGrammar.TokenArrayEnd);
							}
						}
						else if (!isStandAlone)
						{
							output.Add(CommonGrammar.TokenNull);
						}

						if (isStandAlone)
						{
							output.Add(CommonGrammar.TokenObjectEnd);
						}

						return output;
					}

					DataName propertyName = token.Name;
					if (token.TokenType == MarkupTokenType.Attribute)
					{
						input.Pop();
					}

					if (children == null)
					{
						children = new Dictionary<DataName, IList<IList<Token<CommonTokenType>>>>();
					}
					if (!children.ContainsKey(propertyName))
					{
						children[propertyName] = new List<IList<Token<CommonTokenType>>>();
					}

					var child = this.TransformValue(input, !isStandAlone);
					if (child.Count == 1 &&
						child[0].TokenType == CommonTokenType.Primitive &&
						child[0].Value != null &&
						IsNullOrWhiteSpace(child[0].ValueAsString()))
					{
						// skip whitespace mixed content
						continue;
					}

					children[propertyName].Add(child);
				}

				throw new TokenException<MarkupTokenType>(
					token,
					XmlInTransformer.ErrorUnterminatedObject);
			}

			#endregion CommonTokenType to MarkupTokenType Transformation Methods

			#region Utility Methods

			private DataName DecodeName(DataName name, Type type)
			{
				IEnumerable<DataName> defaultNames = this.Settings.Resolver.LoadTypeName(type);
				if (defaultNames != null)
				{
					foreach (DataName defaultName in defaultNames)
					{
						// String.Empty is a valid DataName.LocalName, so may have been replaced by type name
						if (name == defaultName)
						{
							return DataName.Empty;
						}
					}
				}

				// due to a bug in System.Xml.XmlCharType,
				// certain chars are not allowed that XML allows
				// so we must use XmlConvert to encode with same bug

				// XML only supports a subset of chars that DataName.LocalName does
				string localName = System.Xml.XmlConvert.DecodeName(name.LocalName);
				if (name.LocalName == localName)
				{
					return name;
				}

				return new DataName(localName, name.Prefix, name.NamespaceUri, name.IsAttribute);
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

			#endregion Utility Methods
		}
	}
}
