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

		#region Primitive Methods

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

		#endregion Primitive Methods

		#region Object Methods

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
		/// Determines if the root object has any properties which satisfies the name <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>true if any properties match the predicate</returns>
		public static bool HasProperty(this TokenSequence source, Func<DataName, bool> predicate)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IStream<CommonToken> stream = Stream<CommonToken>.Create(source);
			if (stream.IsCompleted ||
				stream.Pop().TokenType != CommonTokenType.ObjectBegin)
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
						stream.Pop();
						continue;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Gets all properties of the root object
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>all properties for the object</returns>
		public static IEnumerable<KeyValuePair<DataName, TokenSequence>> ObjectProperties(this TokenSequence source)
		{
			return CommonSubsequencer.ObjectProperties(source, null);
		}

		/// <summary>
		/// Gets the properties of the root object which satisfies the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>matching properties for the root object</returns>
		public static IEnumerable<KeyValuePair<DataName, TokenSequence>> ObjectProperties(this TokenSequence source, Func<DataName, bool> predicate)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			IStream<CommonToken> stream = Stream<CommonToken>.Create(source);
			if (stream.IsCompleted ||
				stream.Pop().TokenType != CommonTokenType.ObjectBegin)
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
						stream.Pop();
						continue;
					}
				}
			}
		}

		#endregion Object Methods

		#region Array Methods

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
		/// Gets all the items of the array
		/// </summary>
		/// <param name="source"></param>
		/// <returns>all items of the array</returns>
		public static IEnumerable<TokenSequence> ArrayItems(this TokenSequence source)
		{
			return CommonSubsequencer.ArrayItems(source, null);
		}

		/// <summary>
		/// Gets the items of the root array with indexes satisfying the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>items of the root array which statisfy the predicate</returns>
		public static IEnumerable<TokenSequence> ArrayItems(this TokenSequence source, Func<int, bool> predicate)
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

		#endregion Array Methods

		#region Descendants Methods

		/// <summary>
		/// Gets all descendant values below the current root
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IEnumerable<TokenSequence> Descendants(this TokenSequence source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (CommonSubsequencer.IsPrimitive(source))
			{
				yield break;
			}

			if (CommonSubsequencer.IsObject(source))
			{
				foreach (KeyValuePair<DataName, TokenSequence> property in CommonSubsequencer.ObjectProperties(source, null))
				{
					yield return property.Value;

					foreach (TokenSequence descendant in CommonSubsequencer.Descendants(property.Value))
					{
						yield return descendant;
					}
				}
				yield break;
			}

			if (CommonSubsequencer.IsArray(source))
			{
				foreach (TokenSequence item in CommonSubsequencer.ArrayItems(source, null))
				{
					yield return item;

					foreach (TokenSequence descendant in CommonSubsequencer.Descendants(item))
					{
						yield return descendant;
					}
				}
				yield break;
			}
		}

		/// <summary>
		/// Gets all descendant values below the current root, as well as the current root
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IEnumerable<TokenSequence> DescendantsAndSelf(this TokenSequence source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			// and self
			yield return source;

			foreach (TokenSequence descendant in CommonSubsequencer.Descendants(source))
			{
				yield return descendant;
			}
		}

		#endregion Descendants Methods

		#region Utility Methods

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

			stream.BeginChunk();

			int depth = 0;
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
					while (!stream.IsCompleted && depth > 0)
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

					if (depth > 0)
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

			int depth = 0;

			CommonToken token = stream.Pop();
			switch (token.TokenType)
			{
				case CommonTokenType.Primitive:
				{
					return;
				}
				case CommonTokenType.ArrayBegin:
				case CommonTokenType.ObjectBegin:
				{
					depth++;
					while (!stream.IsCompleted && depth > 0)
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

					if (depth > 0)
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

		#endregion Utility Methods
	}
}
