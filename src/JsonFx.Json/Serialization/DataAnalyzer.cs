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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using JsonFx.IO;

#if NET40
using JsonObject=System.Dynamic.ExpandoObject;
#else
using JsonObject=System.Collections.Generic.Dictionary<string, object>;
#endif

namespace JsonFx.Serialization
{
	public partial class DataReader
	{
		/// <summary>
		/// Consumes a SAX-like sequence of tokens to produce an object graph optionally coerced to a given type
		/// </summary>
		public class DataAnalyzer : IDataAnalyzer<DataTokenType>
		{
			#region Constants

			// errors
			private const string ErrorUnexpectedToken = "Unexpected token ({0})";

			private const string ErrorExpectedArray = "Expected array start ({0})";
			private const string ErrorExpectedArrayItem = "Expected array item or end of array ({0})";
			private const string ErrorExpectedArrayItemDelim = "Expected array item delimiter or end of array ({0})";
			private const string ErrorMissingArrayItem = "Missing array item";
			private const string ErrorUnterminatedArray = "Unterminated array";

			private const string ErrorExpectedObject = "Expected object start ({0})";
			private const string ErrorExpectedPropertyName = "Expected object property name or end of object ({0})";
			private const string ErrorExpectedObjectValueDelim = "Expected value delimiter or end of object ({0})";
			private const string ErrorMissingObjectProperty = "Missing object property";
			private const string ErrorUnterminatedObject = "Unterminated object";

			#endregion Constants

			#region Fields

			private readonly DataReaderSettings Settings;
			private readonly TypeCoercionUtility Coercion;
			private readonly IEnumerable<IDataFilter<DataTokenType>> Filters;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			/// <param name="filters"></param>
			public DataAnalyzer(DataReaderSettings settings, params IDataFilter<DataTokenType>[] filters)
				: this(settings, (IEnumerable<IDataFilter<DataTokenType>>)filters)
			{
			}

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="settings"></param>
			public DataAnalyzer(DataReaderSettings settings, IEnumerable<IDataFilter<DataTokenType>> filters)
			{
				if (settings == null)
				{
					throw new ArgumentNullException("settings");
				}
				this.Settings = settings;

				if (filters == null)
				{
					filters = new IDataFilter<DataTokenType>[0];
				}
				this.Filters = filters;

				foreach (var filter in this.Filters)
				{
					if (filter == null)
					{
						throw new ArgumentNullException("filters");
					}
				}

				this.Coercion = new TypeCoercionUtility(settings, settings.AllowNullValueTypes);
			}

			#endregion Init

			#region Analyze Methods

			/// <summary>
			/// Parses the token stream coercing the result targetType
			/// </summary>
			/// <param name="tokens"></param>
			/// <returns></returns>
			public IEnumerable Analyze(IEnumerable<Token<DataTokenType>> tokens)
			{
				return this.Analyze(tokens, null);
			}

			/// <summary>
			/// Parses the token stream coercing the result targetType
			/// </summary>
			/// <param name="tokens"></param>
			/// <param name="targetType"></param>
			/// <returns></returns>
			public IEnumerable Analyze(IEnumerable<Token<DataTokenType>> tokens, Type targetType)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				IStream<Token<DataTokenType>> stream = new Stream<Token<DataTokenType>>(tokens);
				while (!stream.IsCompleted)
				{
					yield return this.ConsumeValue(stream, targetType);
				}
			}

			/// <summary>
			/// Parses the token stream coercing the result to TResult
			/// </summary>
			/// <typeparam name="TResult">the result target type</typeparam>
			/// <param name="tokens"></param>
			/// <returns></returns>
			public IEnumerable<TResult> Analyze<TResult>(IEnumerable<Token<DataTokenType>> tokens)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				Type resultType = typeof(TResult);

				IStream<Token<DataTokenType>> stream = new Stream<Token<DataTokenType>>(tokens);
				while (!stream.IsCompleted)
				{
					// cast each of the values accordingly
					yield return (TResult)this.ConsumeValue(stream, resultType);
				}
			}

			#endregion Analyze Methods

			#region Consume Methods

			private object ConsumeValue(IStream<Token<DataTokenType>> tokens, Type targetType)
			{
				object result;
				if (this.TryReadFilters(tokens, out result))
				{
					return result;
				}

				Token<DataTokenType> token = tokens.Peek();
				switch (token.TokenType)
				{
					case DataTokenType.ArrayBegin:
					{
						// found array
						return this.ConsumeArray(tokens, targetType);
					}
					case DataTokenType.ObjectBegin:
					{
						// found object
						return this.ConsumeObject(tokens, targetType);
					}
					case DataTokenType.Value:
					{
						// found primitive
						tokens.Pop();
						return this.Coercion.CoerceType(targetType, token.Value);
					}
					default:
					{
						// these are invalid here
						tokens.Pop();
						throw new AnalyzerException<DataTokenType>(
							token,
							String.Format(DataAnalyzer.ErrorUnexpectedToken, token.TokenType));
					}
				}
			}

			private object ConsumeObject(IStream<Token<DataTokenType>> tokens, Type targetType)
			{
				Token<DataTokenType> token = tokens.Pop();

				// verify correct starting state
				if (token.TokenType != DataTokenType.ObjectBegin)
				{
					throw new AnalyzerException<DataTokenType>(
						token,
						String.Format(DataAnalyzer.ErrorExpectedObject, token.TokenType));
				}

				IDictionary<string, MemberMap> memberMap = this.Coercion.LoadMaps(targetType);

				Type itemType = TypeCoercionUtility.GetDictionaryItemType(targetType);
				object objectValue = (itemType != null) ?
					this.Coercion.InstantiateObject(targetType) :
					new JsonObject();

				bool needsValueDelim = false;
				while (!tokens.IsCompleted)
				{
					token = tokens.Peek();

					// consume value delimiter
					switch (token.TokenType)
					{
						case DataTokenType.ValueDelim:
						{
							if (!needsValueDelim)
							{
								// extraneous item delimiter
								tokens.Pop();
								throw new AnalyzerException<DataTokenType>(
									token,
									DataAnalyzer.ErrorMissingObjectProperty);
							}

							// consume delim
							tokens.Pop();
							if (tokens.IsCompleted)
							{
								// end of input
								continue;
							}
							token = tokens.Peek();
							break;
						}
						case DataTokenType.ObjectEnd:
						{
							// end of the object loop
							tokens.Pop();
							return this.Coercion.CoerceType(targetType, objectValue);
						}
						default:
						{
							if (needsValueDelim)
							{
								// these are invalid here
								tokens.Pop();
								throw new AnalyzerException<DataTokenType>(
									token,
									String.Format(DataAnalyzer.ErrorExpectedObjectValueDelim, token.TokenType));
							}
							else
							{
								needsValueDelim = true;
							}
							break;
						}
					}

					// consume the property key
					string propertyName;
					switch (token.TokenType)
					{
						case DataTokenType.PropertyKey:
						{
							tokens.Pop();
							propertyName = Convert.ToString(token.Value, CultureInfo.InvariantCulture);
							break;
						}
						case DataTokenType.ObjectEnd:
						case DataTokenType.ValueDelim:
						{
							// extraneous item delimiter
							tokens.Pop();
							throw new AnalyzerException<DataTokenType>(
								token,
								DataAnalyzer.ErrorMissingObjectProperty);
						}
						default:
						{
							// these are invalid here
							tokens.Pop();
							throw new AnalyzerException<DataTokenType>(
								token,
								String.Format(DataAnalyzer.ErrorExpectedPropertyName, token.TokenType));
						}
					}

					MemberMap propertyMap;
					Type propertyType;
					if (itemType != null)
					{
						// properties all of the same type
						propertyMap = null;
						propertyType = itemType;
					}
					else if ((memberMap != null) &&
					memberMap.TryGetValue(propertyName, out propertyMap))
					{
						propertyType = (propertyMap != null) ? propertyMap.Type : null;
					}
					else
					{
						propertyMap = null;
						propertyType = null;
					}

					// consume the property value
					object propertyValue = this.ConsumeValue(tokens, propertyType);

					// set member to the result
					this.Coercion.SetMemberValue(objectValue, targetType, propertyMap, propertyName, propertyValue);
				}

				// end of input
				throw new AnalyzerException<DataTokenType>(
					DataGrammar.TokenNone,
					DataAnalyzer.ErrorUnterminatedObject);
			}

			private object ConsumeArray(IStream<Token<DataTokenType>> tokens, Type arrayType)
			{
				Token<DataTokenType> token = tokens.Pop();

				// verify correct starting state
				if (token.TokenType != DataTokenType.ArrayBegin)
				{
					throw new AnalyzerException<DataTokenType>(
						token,
						String.Format(DataAnalyzer.ErrorExpectedArray, token.TokenType));
				}

				Type itemType = TypeCoercionUtility.GetArrayItemType(arrayType);

				// if itemType was specified by caller, then isn't just a hint
				bool isItemTypeHint = (itemType == null);

				// using ArrayList since has .ToArray(Type) method
				// cannot create List<T> at runtime
				ArrayList array = new ArrayList();

				bool needsValueDelim = false;
				while (!tokens.IsCompleted)
				{
					token = tokens.Peek();

					// consume value delimiter
					switch (token.TokenType)
					{
						case DataTokenType.ValueDelim:
						{
							if (!needsValueDelim)
							{
								// extraneous item delimiter
								tokens.Pop();
								throw new AnalyzerException<DataTokenType>(
									token,
									DataAnalyzer.ErrorMissingObjectProperty);
							}

							// consume delim
							tokens.Pop();
							if (tokens.IsCompleted)
							{
								// end of input
								continue;
							}
							token = tokens.Peek();
							break;
						}
						case DataTokenType.ArrayEnd:
						{
							// end of the array loop
							tokens.Pop();
							return this.Coercion.CoerceArrayList(arrayType, itemType, array);
						}
						default:
						{
							if (needsValueDelim)
							{
								// these are invalid here
								tokens.Pop();
								throw new AnalyzerException<DataTokenType>(
									token,
									String.Format(DataAnalyzer.ErrorExpectedArrayItemDelim, token.TokenType));
							}
							else
							{
								needsValueDelim = true;
							}
							break;
						}
					}

					// consume the next item
					object item;
					switch (token.TokenType)
					{
						case DataTokenType.ArrayBegin:
						{
							// array item
							item = this.ConsumeArray(tokens, isItemTypeHint ? null : itemType);
							break;
						}
						case DataTokenType.ObjectBegin:
						{
							// object item
							item = this.ConsumeObject(tokens, isItemTypeHint ? null : itemType);
							break;
						}
						case DataTokenType.Value:
						{
							// primitive item
							if (!this.TryReadFilters(tokens, out item))
							{
								// TODO: evaluate how to ensure that filters didn't take anything
								token = tokens.Pop();
								item = (token != null) ? token.Value : null;
							}

							if (!isItemTypeHint)
							{
								item = this.Coercion.CoerceType(itemType, item);
							}
							break;
						}
						case DataTokenType.ArrayEnd:
						case DataTokenType.ValueDelim:
						{
							// extraneous item delimiter
							tokens.Pop();
							throw new AnalyzerException<DataTokenType>(
								token,
								DataAnalyzer.ErrorMissingArrayItem);
						}
						default:
						{
							// these are invalid here
							tokens.Pop();
							throw new AnalyzerException<DataTokenType>(
								token,
								String.Format(DataAnalyzer.ErrorExpectedArrayItem, token.TokenType));
						}
					}

					// establish common type
					itemType = TypeCoercionUtility.FindCommonType(itemType, item);

					// add item to the array
					array.Add(item);
				}

				// end of input
				throw new AnalyzerException<DataTokenType>(
					DataGrammar.TokenNone,
					DataAnalyzer.ErrorUnterminatedArray);
			}

			private bool TryReadFilters(IStream<Token<DataTokenType>> tokens, out object result)
			{
				if (!tokens.IsCompleted)
				{
					foreach (var filter in this.Filters)
					{
						if (filter.TryRead(this.Settings, tokens, out result))
						{
							// found a successful match
							return true;
						}
					}
				}

				result = null;
				return false;
			}

			#endregion Consume Methods
		}
	}
}