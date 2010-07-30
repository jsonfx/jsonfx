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
		/// Gets the value of the first property which satisfies the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <returns>all properties for the root level object</returns>
		public static TokenSequence GetProperty(this TokenSequence source, Func<DataName, bool> predicate)
		{
			foreach (var property in CommonSubsequencer.GetProperties(source, predicate))
			{
				if (predicate == null || predicate(property.Key))
				{
					return property.Value;
				}
			}

			return CommonSubsequencer.EmptySequence;
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

			if (!source.IsObject())
			{
				yield break;
			}

			int depth = -1;

			IStream<CommonToken> stream = Stream<CommonToken>.Create(source);
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

			if (!source.IsObject())
			{
				return false;
			}

			int depth = -1;

			IStream<CommonToken> stream = Stream<CommonToken>.Create(source);
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
		/// Splices out the next complete value
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
				case CommonTokenType.Property:
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

		#endregion Extension Methods
	}
}
