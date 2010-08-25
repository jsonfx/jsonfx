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

using JsonFx.IO;
using JsonFx.Serialization;
using JsonFx.Serialization.Filters;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Model
{
	/// <summary>
	/// Consumes a sequence of tokens to produce an object graph optionally coerced to a given type
	/// </summary>
	public class ModelAnalyzer : ITokenAnalyzer<ModelTokenType>
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
		private readonly IEnumerable<IDataFilter<ModelTokenType>> Filters;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public ModelAnalyzer(DataReaderSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}
			this.Settings = settings;

			var filters = new List<IDataFilter<ModelTokenType>>();
			if (settings.Filters != null)
			{
				foreach (var filter in settings.Filters)
				{
					if (filter != null)
					{
						filters.Add(filter);
					}
				}
			}
			this.Filters = filters;

			this.Coercion = new TypeCoercionUtility(settings, settings.AllowNullValueTypes);
		}

		#endregion Init

		#region Properties

		DataReaderSettings ITokenAnalyzer<ModelTokenType>.Settings
		{
			get { return this.Settings; }
		}

		#endregion Properties

		#region ITokenAnalyzer<T> Methods

		/// <summary>
		/// Parses the token stream coercing the result targetType
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		public IEnumerable Analyze(IEnumerable<Token<ModelTokenType>> tokens)
		{
			return this.Analyze(tokens, null);
		}

		/// <summary>
		/// Parses the token stream coercing the result to targetType
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public IEnumerable Analyze(IEnumerable<Token<ModelTokenType>> tokens, Type targetType)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException("tokens");
			}

			IStream<Token<ModelTokenType>> stream = Stream<Token<ModelTokenType>>.Create(tokens);
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
		public IEnumerable<TResult> Analyze<TResult>(IEnumerable<Token<ModelTokenType>> tokens)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException("tokens");
			}

			Type resultType = typeof(TResult);

			IStream<Token<ModelTokenType>> stream = Stream<Token<ModelTokenType>>.Create(tokens);
			while (!stream.IsCompleted)
			{
				// cast each of the values accordingly
				yield return (TResult)this.ConsumeValue(stream, resultType);
			}
		}

		/// <summary>
		/// Parses the token stream coercing the result to TResult (inferred from <paramref name="ignored"/>)
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="tokens"></param>
		/// <param name="ignored">an example value used solely for Type inference</param>
		/// <returns></returns>
		public IEnumerable<TResult> Analyze<TResult>(IEnumerable<Token<ModelTokenType>> tokens, TResult ignored)
		{
			return this.Analyze<TResult>(tokens);
		}

		#endregion ITokenAnalyzer<T> Methods

		#region Consume Methods

		private object ConsumeValue(IStream<Token<ModelTokenType>> tokens, Type targetType)
		{
			object result;
			if (this.TryReadFilters(tokens, out result))
			{
				return result;
			}

			Token<ModelTokenType> token = tokens.Peek();
			switch (token.TokenType)
			{
				case ModelTokenType.ArrayBegin:
				{
					// found array
					return this.ConsumeArray(tokens, targetType);
				}
				case ModelTokenType.ObjectBegin:
				{
					// found object
					return this.ConsumeObject(tokens, targetType);
				}
				case ModelTokenType.Primitive:
				{
					// found primitive
					tokens.Pop();
					return this.Coercion.CoerceType(targetType, token.Value);
				}
				default:
				{
					// these are invalid here
					tokens.Pop();
					throw new TokenException<ModelTokenType>(
						token,
						String.Format(ModelAnalyzer.ErrorUnexpectedToken, token.TokenType));
				}
			}
		}

		private object ConsumeObject(IStream<Token<ModelTokenType>> tokens, Type targetType)
		{
			Token<ModelTokenType> token = tokens.Pop();

			// verify correct starting state
			if (token.TokenType != ModelTokenType.ObjectBegin)
			{
				throw new TokenException<ModelTokenType>(
					token,
					String.Format(ModelAnalyzer.ErrorExpectedObject, token.TokenType));
			}

			IDictionary<string, MemberMap> maps = this.Settings.Resolver.LoadMaps(targetType);

			Type itemType = TypeCoercionUtility.GetDictionaryItemType(targetType);
			object objectValue = this.Coercion.InstantiateObjectDefaultCtor(targetType);

			while (!tokens.IsCompleted)
			{
				token = tokens.Peek();

				// consume the property key
				string propertyName;
				switch (token.TokenType)
				{
					case ModelTokenType.Property:
					{
						tokens.Pop();
						propertyName = token.Name.LocalName;
						break;
					}
					case ModelTokenType.ObjectEnd:
					{
						// end of the object loop
						tokens.Pop();
						return this.Coercion.CoerceType(targetType, objectValue);
					}
					default:
					{
						// these are invalid here
						tokens.Pop();
						throw new TokenException<ModelTokenType>(
							token,
							String.Format(ModelAnalyzer.ErrorExpectedPropertyName, token.TokenType));
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
				else if ((maps != null) &&
					maps.TryGetValue(propertyName, out propertyMap))
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
			throw new TokenException<ModelTokenType>(
				ModelGrammar.TokenNone,
				ModelAnalyzer.ErrorUnterminatedObject);
		}

		private object ConsumeArray(IStream<Token<ModelTokenType>> tokens, Type arrayType)
		{
			Token<ModelTokenType> token = tokens.Pop();

			// verify correct starting state
			if (token.TokenType != ModelTokenType.ArrayBegin)
			{
				throw new TokenException<ModelTokenType>(
					token,
					String.Format(ModelAnalyzer.ErrorExpectedArray, token.TokenType));
			}

			Type itemType = TypeCoercionUtility.GetElementType(arrayType);

			// if itemType was specified by caller, then isn't just a hint
			bool isItemTypeHint = (itemType == null);

			IList array = new List<object>();
			while (!tokens.IsCompleted)
			{
				token = tokens.Peek();

				// consume the next item
				object item;
				switch (token.TokenType)
				{
					case ModelTokenType.ArrayBegin:
					{
						// array item
						item = this.ConsumeArray(tokens, isItemTypeHint ? null : itemType);
						break;
					}
					case ModelTokenType.ArrayEnd:
					{
						// end of the array loop
						tokens.Pop();
						return this.Coercion.CoerceCollection(arrayType, itemType, array);
					}
					case ModelTokenType.ObjectBegin:
					{
						// object item
						item = this.ConsumeObject(tokens, isItemTypeHint ? null : itemType);
						break;
					}
					case ModelTokenType.Primitive:
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
					default:
					{
						// these are invalid here
						tokens.Pop();
						throw new TokenException<ModelTokenType>(
							token,
							String.Format(ModelAnalyzer.ErrorExpectedArrayItem, token.TokenType));
					}
				}

				// establish common type
				itemType = TypeCoercionUtility.FindCommonType(itemType, item);

				// add item to the array
				array.Add(item);
			}

			// end of input
			throw new TokenException<ModelTokenType>(
				ModelGrammar.TokenNone,
				ModelAnalyzer.ErrorUnterminatedArray);
		}

		private bool TryReadFilters(IStream<Token<ModelTokenType>> tokens, out object result)
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