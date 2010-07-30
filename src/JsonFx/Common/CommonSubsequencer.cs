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
using JsonFx.Serialization;

using CommonToken=JsonFx.Serialization.Token<JsonFx.Common.CommonTokenType>;
using TokenSequence=System.Collections.Generic.IEnumerable<JsonFx.Serialization.Token<JsonFx.Common.CommonTokenType>>;

namespace JsonFx.Common
{
	/// <summary>
	/// Extension methods for selecting subsequences of sequences of tokens
	/// </summary>
	public static class CommonSubsequencer
	{
		#region Constants

		private static readonly TokenSequence EmptySequence = new Token<CommonTokenType>[0];

		private const string ErrorUnexpectedEndOfInput = "Unexpected end of token stream";
		private const string ErrorInvalidPropertyValue = "Invalid property value token ({0})";

		#endregion Constants

		#region Extension Methods

		/// <summary>
		/// Determines if the sequence represents an object
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static bool IsObject(this TokenSequence source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IList<CommonToken> tokenList = source as IList<CommonToken>;
			if (tokenList != null)
			{
				return (tokenList.Count > 0) && (tokenList[0].TokenType == CommonTokenType.ObjectBegin);
			}

			foreach (var token in source)
			{
				return (token.TokenType == CommonTokenType.ObjectBegin);
			}

			return false;
		}

		/// <summary>
		/// Determines if the sequence represents an array
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static bool IsArray(this TokenSequence source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IList<CommonToken> tokenList = source as IList<CommonToken>;
			if (tokenList != null)
			{
				return (tokenList.Count > 0) && (tokenList[0].TokenType == CommonTokenType.ArrayBegin);
			}

			foreach (var token in source)
			{
				return (token.TokenType == CommonTokenType.ArrayBegin);
			}

			return false;
		}

		/// <summary>
		/// Determines if the sequence represents a primitive
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static bool IsPrimitive(this TokenSequence source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IList<CommonToken> tokenList = source as IList<CommonToken>;
			if (tokenList != null)
			{
				return (tokenList.Count > 0) && (tokenList[0].TokenType == CommonTokenType.Primitive);
			}

			foreach (var token in source)
			{
				return (token.TokenType == CommonTokenType.Primitive);
			}

			return false;
		}

		/// <summary>
		/// Determines if the root object has a property name which satisfies the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>properties for the root level object which statisfy the predicate</returns>
		public static bool HasProperty(this TokenSequence source, Func<DataName, bool> predicate)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IStream<CommonToken> stream = Stream<CommonToken>.Create(source);
			if (stream.IsCompleted ||
				stream.Pop().TokenType != CommonTokenType.ArrayBegin)
			{
				return false;
			}

			int depth = 0;

			while (!stream.IsCompleted)
			{
				CommonToken token = stream.Peek();
				switch (token.TokenType)
				{
					case CommonTokenType.ArrayBegin:
					case CommonTokenType.ObjectBegin:
					{
						depth++;
						stream.Pop();
						continue;
					}
					case CommonTokenType.ArrayEnd:
					{
						depth--;
						stream.Pop();
						continue;
					}
					case CommonTokenType.ObjectEnd:
					{
						if (depth != 0)
						{
							depth--;
							stream.Pop();
							continue;
						}

						// don't look beyond end of object
						return false;
					}
					case CommonTokenType.Property:
					{
						stream.Pop();

						if (depth != 0 ||
							(predicate != null && !predicate(token.Name)))
						{
							continue;
						}

						// found a property name that satisfies
						return true;
					}
					default:
					{
						continue;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the value of the any properties which satisfy the <paramref name="namePredicate"/> and <paramref name="valuePredicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<DataName, TokenSequence>> GetProperties(this TokenSequence source, Func<DataName, bool> namePredicate, Func<TokenSequence, bool> valuePredicate)
		{
			foreach (var property in CommonSubsequencer.GetProperties(source, namePredicate))
			{
				if (valuePredicate == null || valuePredicate(property.Value))
				{
					yield return property;
				}
			}
		}

		/// <summary>
		/// Gets all the properties of the root object
		/// </summary>
		/// <param name="source"></param>
		/// <returns>all properties for the root level object</returns>
		public static IEnumerable<KeyValuePair<DataName, TokenSequence>> GetProperties(this TokenSequence source)
		{
			return CommonSubsequencer.GetProperties(source, null);
		}

		/// <summary>
		/// Gets the properties of the root object which satisfies the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>properties for the root level object which statisfy the predicate</returns>
		public static IEnumerable<KeyValuePair<DataName, TokenSequence>> GetProperties(this TokenSequence source, Func<DataName, bool> predicate)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IStream<CommonToken> stream = Stream<CommonToken>.Create(source);
			if (stream.IsCompleted ||
				stream.Pop().TokenType != CommonTokenType.ArrayBegin)
			{
				yield break;
			}

			int depth = 0;

			while (!stream.IsCompleted)
			{
				CommonToken token = stream.Peek();
				switch (token.TokenType)
				{
					case CommonTokenType.ArrayBegin:
					case CommonTokenType.ObjectBegin:
					{
						depth++;
						stream.Pop();
						continue;
					}
					case CommonTokenType.ArrayEnd:
					{
						depth--;
						stream.Pop();
						continue;
					}
					case CommonTokenType.ObjectEnd:
					{
						if (depth != 0)
						{
							depth--;
							stream.Pop();
							continue;
						}

						// don't look beyond end of object
						yield break;
					}
					case CommonTokenType.Property:
					{
						stream.Pop();

						if (depth != 0 ||
							(predicate != null && !predicate(token.Name)))
						{
							continue;
						}

						// return property value sequence

						yield return new KeyValuePair<DataName, TokenSequence>(token.Name, CommonSubsequencer.SpliceNextValue(stream));
						continue;
					}
					default:
					{
						continue;
					}
				}
			}
		}

		/// <summary>
		/// Gets the properties of the root object which satisfies the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>properties for the root level object which statisfy the predicate</returns>
		public static IEnumerable<TokenSequence> GetArrayItem(this TokenSequence source, Func<int, bool> predicate)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IStream<CommonToken> stream = Stream<CommonToken>.Create(source);
			if (stream.IsCompleted ||
				stream.Pop().TokenType != CommonTokenType.ArrayBegin)
			{
				yield break;
			}

			int index = 0;
			while (!stream.IsCompleted)
			{
				CommonToken token = stream.Peek();
				if (token.TokenType == CommonTokenType.ArrayEnd)
				{
					break;
				}

				if (predicate == null || predicate(index))
				{
					yield return CommonSubsequencer.SpliceNextValue(stream);
				}
				else
				{
					CommonSubsequencer.SkipNextValue(stream);
				}
				index++;
			}
		}

		/// <summary>
		/// Gets the properties of the root object which satisfies the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>properties for the root level object which statisfy the predicate</returns>
		public static IEnumerable<TokenSequence> GetArrayItems(this TokenSequence source, Func<TokenSequence, bool> predicate)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IStream<CommonToken> stream = Stream<CommonToken>.Create(source);
			if (stream.IsCompleted ||
				stream.Pop().TokenType != CommonTokenType.ArrayBegin)
			{
				yield break;
			}

			while (!stream.IsCompleted)
			{
				CommonToken token = stream.Peek();
				if (token.TokenType == CommonTokenType.ArrayEnd)
				{
					break;
				}

				var item = CommonSubsequencer.SpliceNextValue(stream);
				if (predicate == null || predicate(item))
				{
					yield return item;
				}
			}
		}

		/// <summary>
		/// Gets the properties of the root object which satisfies the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>properties for the root level object which statisfy the predicate</returns>
		public static IEnumerable<TokenSequence> GetArrayItems(this TokenSequence source, Func<TokenSequence, int, bool> predicate)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IStream<CommonToken> stream = Stream<CommonToken>.Create(source);
			if (stream.IsCompleted ||
				stream.Pop().TokenType != CommonTokenType.ArrayBegin)
			{
				yield break;
			}

			int index = 0;
			while (!stream.IsCompleted)
			{
				CommonToken token = stream.Peek();
				if (token.TokenType == CommonTokenType.ArrayEnd)
				{
					break;
				}

				var item = CommonSubsequencer.SpliceNextValue(stream);
				if (predicate == null || predicate(item, index++))
				{
					yield return item;
				}
			}
		}

		/// <summary>
		/// Splices out the sequence for the next complete value (object, array, primitive)
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private static TokenSequence SpliceNextValue(IStream<CommonToken> stream)
		{
			if (stream.IsCompleted)
			{
				throw new TokenException<CommonTokenType>(
					CommonGrammar.TokenNone,
					CommonSubsequencer.ErrorUnexpectedEndOfInput);
			}

			int depth = -1;

			stream.BeginChunk();
			CommonToken token = stream.Pop();

			switch (token.TokenType)
			{
				case CommonTokenType.Primitive:
				{
					return stream.EndChunk();
				}
				case CommonTokenType.ArrayBegin:
				case CommonTokenType.ObjectBegin:
				{
					depth++;
					while (!stream.IsCompleted && depth >= 0)
					{
						switch (stream.Pop().TokenType)
						{
							case CommonTokenType.ArrayBegin:
							case CommonTokenType.ObjectBegin:
							{
								depth++;
								break;
							}
							case CommonTokenType.ArrayEnd:
							case CommonTokenType.ObjectEnd:
							{
								depth--;
								break;
							}
						}
					}

					if (depth >= 0)
					{
						throw new TokenException<CommonTokenType>(
							CommonGrammar.TokenNone,
							CommonSubsequencer.ErrorUnexpectedEndOfInput);
					}
					return stream.EndChunk();
				}
				default:
				{
					throw new TokenException<CommonTokenType>(
						token,
						String.Format(CommonSubsequencer.ErrorInvalidPropertyValue, token.TokenType));
				}
			}
		}

		/// <summary>
		/// Skips over the next complete value (object, array, primitive)
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private static void SkipNextValue(IStream<CommonToken> stream)
		{
			if (stream.IsCompleted)
			{
				return;
			}

			int depth = -1;

			CommonToken token = stream.Pop();

			switch (token.TokenType)
			{
				case CommonTokenType.Property:
				{
					return;
				}
				case CommonTokenType.ArrayBegin:
				case CommonTokenType.ObjectBegin:
				{
					depth++;
					while (!stream.IsCompleted && depth >= 0)
					{
						switch (stream.Pop().TokenType)
						{
							case CommonTokenType.ArrayBegin:
							case CommonTokenType.ObjectBegin:
							{
								depth++;
								break;
							}
							case CommonTokenType.ArrayEnd:
							case CommonTokenType.ObjectEnd:
							{
								depth--;
								break;
							}
						}
					}

					if (depth >= 0)
					{
						throw new TokenException<CommonTokenType>(
							CommonGrammar.TokenNone,
							CommonSubsequencer.ErrorUnexpectedEndOfInput);
					}
					return;
				}
				default:
				{
					throw new TokenException<CommonTokenType>(
						token,
						String.Format(CommonSubsequencer.ErrorInvalidPropertyValue, token.TokenType));
				}
			}
		}

		#endregion Extension Methods
	}
}
