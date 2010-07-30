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
using JsonFx.Serialization.Resolvers;
using JsonFx.Serialization.Filters;
using JsonFx.Serialization;

#if NET40
using JsonObject=System.Dynamic.ExpandoObject;
#else
using JsonObject=System.Collections.Generic.Dictionary<string, object>;
#endif

namespace JsonFx.Common
{
	/// <summary>
	/// Consumes a sequence of tokens to produce an object graph optionally coerced to a given type
	/// </summary>
	public class CommonAnalyzer : ITokenAnalyzer<CommonTokenType>
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
		private readonly IEnumerable<IDataFilter<CommonTokenType>> Filters;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public CommonAnalyzer(DataReaderSettings settings, params IDataFilter<CommonTokenType>[] filters)
			: this(settings, (IEnumerable<IDataFilter<CommonTokenType>>)filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public CommonAnalyzer(DataReaderSettings settings, IEnumerable<IDataFilter<CommonTokenType>> filters)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}
			this.Settings = settings;

			if (filters == null)
			{
				filters = new IDataFilter<CommonTokenType>[0];
			}
			this.Filters = filters;

			foreach (var filter in filters)
			{
				if (filter == null)
				{
					throw new ArgumentNullException("filters");
				}
			}

			this.Coercion = new TypeCoercionUtility(settings, settings.AllowNullValueTypes);
		}

		#endregion Init

		#region ITokenAnalyzer<T> Methods

		/// <summary>
		/// Parses the token stream coercing the result targetType
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		public IEnumerable Analyze(IEnumerable<Token<CommonTokenType>> tokens)
		{
			return this.Analyze(tokens, null);
		}

		/// <summary>
		/// Parses the token stream coercing the result to targetType
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public IEnumerable Analyze(IEnumerable<Token<CommonTokenType>> tokens, Type targetType)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException("tokens");
			}

			IStream<Token<CommonTokenType>> stream = Stream<Token<CommonTokenType>>.Create(tokens);
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
		public IEnumerable<TResult> Analyze<TResult>(IEnumerable<Token<CommonTokenType>> tokens)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException("tokens");
			}

			Type resultType = typeof(TResult);

			IStream<Token<CommonTokenType>> stream = Stream<Token<CommonTokenType>>.Create(tokens);
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
		public IEnumerable<TResult> Analyze<TResult>(IEnumerable<Token<CommonTokenType>> tokens, TResult ignored)
		{
			return this.Analyze<TResult>(tokens);
		}

		#endregion ITokenAnalyzer<T> Methods

		#region Consume Methods

		private object ConsumeValue(IStream<Token<CommonTokenType>> tokens, Type targetType)
		{
			object result;
			if (this.TryReadFilters(tokens, out result))
			{
				return result;
			}

			Token<CommonTokenType> token = tokens.Peek();
			switch (token.TokenType)
			{
				case CommonTokenType.ArrayBegin:
				{
					// found array
					return this.ConsumeArray(tokens, targetType);
				}
				case CommonTokenType.ObjectBegin:
				{
					// found object
					return this.ConsumeObject(tokens, targetType);
				}
				case CommonTokenType.Primitive:
				{
					// found primitive
					tokens.Pop();
					return this.Coercion.CoerceType(targetType, token.Value);
				}
				default:
				{
					// these are invalid here
					tokens.Pop();
					throw new TokenException<CommonTokenType>(
						token,
						String.Format(CommonAnalyzer.ErrorUnexpectedToken, token.TokenType));
				}
			}
		}

		private object ConsumeObject(IStream<Token<CommonTokenType>> tokens, Type targetType)
		{
			Token<CommonTokenType> token = tokens.Pop();

			// verify correct starting state
			if (token.TokenType != CommonTokenType.ObjectBegin)
			{
				throw new TokenException<CommonTokenType>(
					token,
					String.Format(CommonAnalyzer.ErrorExpectedObject, token.TokenType));
			}

			IDictionary<string, MemberMap> maps = this.Settings.Resolver.LoadMaps(targetType);

			Type itemType = TypeCoercionUtility.GetDictionaryItemType(targetType);
			object objectValue = (itemType != null) ?
				this.Coercion.InstantiateObject(targetType, null) :
				new JsonObject();

			while (!tokens.IsCompleted)
			{
				token = tokens.Peek();

				// consume the property key
				string propertyName;
				switch (token.TokenType)
				{
					case CommonTokenType.Property:
					{
						tokens.Pop();
						propertyName = token.Name.LocalName;
						break;
					}
					case CommonTokenType.ObjectEnd:
					{
						// end of the object loop
						tokens.Pop();
						return this.Coercion.CoerceType(targetType, objectValue);
					}
					default:
					{
						// these are invalid here
						tokens.Pop();
						throw new TokenException<CommonTokenType>(
							token,
							String.Format(CommonAnalyzer.ErrorExpectedPropertyName, token.TokenType));
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
			throw new TokenException<CommonTokenType>(
				CommonGrammar.TokenNone,
				CommonAnalyzer.ErrorUnterminatedObject);
		}

		private object ConsumeArray(IStream<Token<CommonTokenType>> tokens, Type arrayType)
		{
			Token<CommonTokenType> token = tokens.Pop();

			// verify correct starting state
			if (token.TokenType != CommonTokenType.ArrayBegin)
			{
				throw new TokenException<CommonTokenType>(
					token,
					String.Format(CommonAnalyzer.ErrorExpectedArray, token.TokenType));
			}

			Type itemType = TypeCoercionUtility.GetArrayItemType(arrayType);

			// if itemType was specified by caller, then isn't just a hint
			bool isItemTypeHint = (itemType == null);

			// using ArrayList since has .ToArray(Type) method
			// cannot create List<T> at runtime
			ArrayList array = new ArrayList();

			while (!tokens.IsCompleted)
			{
				token = tokens.Peek();

				// consume the next item
				object item;
				switch (token.TokenType)
				{
					case CommonTokenType.ArrayBegin:
					{
						// array item
						item = this.ConsumeArray(tokens, isItemTypeHint ? null : itemType);
						break;
					}
					case CommonTokenType.ArrayEnd:
					{
						// end of the array loop
						tokens.Pop();
						return this.Coercion.CoerceArrayList(arrayType, itemType, array);
					}
					case CommonTokenType.ObjectBegin:
					{
						// object item
						item = this.ConsumeObject(tokens, isItemTypeHint ? null : itemType);
						break;
					}
					case CommonTokenType.Primitive:
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
						throw new TokenException<CommonTokenType>(
							token,
							String.Format(CommonAnalyzer.ErrorExpectedArrayItem, token.TokenType));
					}
				}

				// establish common type
				itemType = TypeCoercionUtility.FindCommonType(itemType, item);

				// add item to the array
				array.Add(item);
			}

			// end of input
			throw new TokenException<CommonTokenType>(
				CommonGrammar.TokenNone,
				CommonAnalyzer.ErrorUnterminatedArray);
		}

		private bool TryReadFilters(IStream<Token<CommonTokenType>> tokens, out object result)
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