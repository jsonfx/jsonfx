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

using ModelToken=JsonFx.Serialization.Token<JsonFx.Model.ModelTokenType>;
using TokenSequence=System.Collections.Generic.IEnumerable<JsonFx.Serialization.Token<JsonFx.Model.ModelTokenType>>;

namespace JsonFx.Model
{
#if NET20 || NET30
    public delegate TResult Func<T, TResult>(T input);
#endif

	/// <summary>
	/// Extension methods for selecting subsequences of sequences of tokens
	/// </summary>
	public static class ModelSubsequencer
	{
		#region Constants

		private static readonly TokenSequence EmptySequence = new Token<ModelTokenType>[0];

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

			IList<ModelToken> tokenList = source as IList<ModelToken>;
			if (tokenList != null)
			{
				return (tokenList.Count > 0) && (tokenList[0].TokenType == ModelTokenType.Primitive);
			}

			foreach (var token in source)
			{
				return (token.TokenType == ModelTokenType.Primitive);
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

			IList<ModelToken> tokenList = source as IList<ModelToken>;
			if (tokenList != null)
			{
				return (tokenList.Count > 0) && (tokenList[0].TokenType == ModelTokenType.ObjectBegin);
			}

			foreach (var token in source)
			{
				return (token.TokenType == ModelTokenType.ObjectBegin);
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

			IStream<ModelToken> stream = Stream<ModelToken>.Create(source);
			if (stream.IsCompleted ||
				stream.Pop().TokenType != ModelTokenType.ObjectBegin)
			{
				return false;
			}

			int depth = 0;

			while (!stream.IsCompleted)
			{
				ModelToken token = stream.Peek();
				switch (token.TokenType)
				{
					case ModelTokenType.ArrayBegin:
					case ModelTokenType.ObjectBegin:
					{
						depth++;
						stream.Pop();
						continue;
					}
					case ModelTokenType.ArrayEnd:
					{
						depth--;
						stream.Pop();
						continue;
					}
					case ModelTokenType.ObjectEnd:
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
					case ModelTokenType.Property:
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
		public static TokenSequence Property(this TokenSequence source, DataName propertyName)
		{
			using (var enumerator = ModelSubsequencer.Properties(source, name => (name == propertyName)).GetEnumerator())
			{
				// effectively FirstOrDefault()
				return enumerator.MoveNext() ? enumerator.Current.Value : null;
			}
		}

		/// <summary>
		/// Gets all properties of the root object
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>all properties for the object</returns>
		public static IEnumerable<KeyValuePair<DataName, TokenSequence>> Properties(this TokenSequence source)
		{
			return ModelSubsequencer.Properties(source, null);
		}

		/// <summary>
		/// Gets the properties of the root object which satisfies the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>matching properties for the root object</returns>
		public static IEnumerable<KeyValuePair<DataName, TokenSequence>> Properties(this TokenSequence source, Func<DataName, bool> predicate)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (!(source is IList<ModelToken>))
			{
				// ensure buffered
				source = new SequenceBuffer<ModelToken>(source);
			}

			return ModelSubsequencer.PropertiesIterator(source, predicate);
		}

		/// <summary>
		/// Gets the properties of the root object which satisfies the <paramref name="predicate"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns>matching properties for the root object</returns>
		private static IEnumerable<KeyValuePair<DataName, TokenSequence>> PropertiesIterator(TokenSequence source, Func<DataName, bool> predicate)
		{
			IStream<ModelToken> stream = Stream<ModelToken>.Create(source);
			if (stream.IsCompleted ||
				stream.Pop().TokenType != ModelTokenType.ObjectBegin)
			{
				yield break;
			}

			int depth = 0;

			while (!stream.IsCompleted)
			{
				ModelToken token = stream.Peek();
				switch (token.TokenType)
				{
					case ModelTokenType.ArrayBegin:
					case ModelTokenType.ObjectBegin:
					{
						depth++;
						stream.Pop();
						continue;
					}
					case ModelTokenType.ArrayEnd:
					{
						depth--;
						stream.Pop();
						continue;
					}
					case ModelTokenType.ObjectEnd:
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
					case ModelTokenType.Property:
					{
						stream.Pop();

						if (depth != 0 ||
							(predicate != null && !predicate(token.Name)))
						{
							continue;
						}

						// return property value sequence
						yield return new KeyValuePair<DataName, TokenSequence>(token.Name, ModelSubsequencer.SpliceNextValue(stream));
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

			IList<ModelToken> tokenList = source as IList<ModelToken>;
			if (tokenList != null)
			{
				return (tokenList.Count > 0) && (tokenList[0].TokenType == ModelTokenType.ArrayBegin);
			}

			foreach (var token in source)
			{
				return (token.TokenType == ModelTokenType.ArrayBegin);
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
			return ModelSubsequencer.ArrayItems(source, null);
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

			if (!(source is IList<ModelToken>))
			{
				// ensure buffered
				source = new SequenceBuffer<ModelToken>(source);
			}

			return ModelSubsequencer.ArrayItemsIterator(source, predicate);
		}

		/// <summary>
		/// ArrayItems iterator
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		private static IEnumerable<TokenSequence> ArrayItemsIterator(TokenSequence source, Func<int, bool> predicate)
		{
			IStream<ModelToken> stream = Stream<ModelToken>.Create(source);
			if (stream.IsCompleted)
			{
				yield break;
			}

			if (stream.Pop().TokenType != ModelTokenType.ArrayBegin)
			{
				yield return source;
				yield break;
			}

			int index = 0;
			while (!stream.IsCompleted)
			{
				ModelToken token = stream.Peek();
				if (token.TokenType == ModelTokenType.ArrayEnd)
				{
					break;
				}

				if (predicate == null || predicate(index))
				{
					yield return ModelSubsequencer.SpliceNextValue(stream);
				}
				else
				{
					ModelSubsequencer.SkipNextValue(stream);
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

			if (!(source is IList<ModelToken>))
			{
				// ensure buffered
				source = new SequenceBuffer<ModelToken>(source);
			}

			return ModelSubsequencer.DescendantsIterator(source);
		}

		/// <summary>
		/// Descendants iterator
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private static IEnumerable<TokenSequence> DescendantsIterator(TokenSequence source)
		{
			if (ModelSubsequencer.IsPrimitive(source))
			{
				yield break;
			}

			if (ModelSubsequencer.IsObject(source))
			{
				foreach (KeyValuePair<DataName, TokenSequence> property in ModelSubsequencer.Properties(source, null))
				{
					yield return property.Value;

					foreach (TokenSequence descendant in ModelSubsequencer.Descendants(property.Value))
					{
						yield return descendant;
					}
				}
				yield break;
			}

			if (ModelSubsequencer.IsArray(source))
			{
				foreach (TokenSequence item in ModelSubsequencer.ArrayItems(source, null))
				{
					yield return item;

					foreach (TokenSequence descendant in ModelSubsequencer.Descendants(item))
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

			if (!(source is IList<ModelToken>))
			{
				// ensure buffered
				source = new SequenceBuffer<ModelToken>(source);
			}

			return ModelSubsequencer.DescendantsAndSelfIterator(source);
		}

		/// <summary>
		/// DescendantsAndSelf iterator
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private static IEnumerable<TokenSequence> DescendantsAndSelfIterator(TokenSequence source)
		{
			// and self
			yield return source;

			foreach (TokenSequence descendant in ModelSubsequencer.DescendantsIterator(source))
			{
				yield return descendant;
			}
		}

		#endregion Descendants Methods

		#region Utility Methods

		/// <summary>
		/// Covers the sitation where a stream of sequences may be back to back
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		internal static IEnumerable<TokenSequence> SplitValues(this TokenSequence source)
		{
			if (source == null)
			{
				return new TokenSequence[0];
			}

			if (!(source is IList<ModelToken>))
			{
				// ensure buffered
				source = new SequenceBuffer<ModelToken>(source);
			}

			return ModelSubsequencer.SplitValuesIterator(source);
		}

		private static IEnumerable<TokenSequence> SplitValuesIterator(TokenSequence source)
		{
			using (var stream = Stream<ModelToken>.Create(source))
			{
				while (!stream.IsCompleted)
				{
					yield return ModelSubsequencer.SpliceNextValue(stream);
				}
			}
		}

		/// <summary>
		/// Splices out the sequence for the next complete value (object, array, primitive)
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		[Obsolete("TODO: Lazy does not mix well with shared IStream<T>.", true)]
		private static TokenSequence SpliceNextValueLazy(IStream<ModelToken> stream)
		{
			if (stream.IsCompleted)
			{
				yield break;
			}

			int depth = 0;
			ModelToken token = stream.Pop();
			switch (token.TokenType)
			{
				case ModelTokenType.Primitive:
				{
					yield return token;
					yield break;
				}
				case ModelTokenType.ArrayBegin:
				case ModelTokenType.ObjectBegin:
				{
					depth++;
					yield return token;

					while (!stream.IsCompleted && depth > 0)
					{
						token = stream.Pop();
						switch (token.TokenType)
						{
							case ModelTokenType.ArrayBegin:
							case ModelTokenType.ObjectBegin:
							{
								depth++;
								yield return token;
								break;
							}
							case ModelTokenType.ArrayEnd:
							case ModelTokenType.ObjectEnd:
							{
								depth--;
								yield return token;
								break;
							}
							default:
							{
								// skip over all others
								yield return token;
								break;
							}
						}
					}

					if (depth > 0)
					{
						throw new TokenException<ModelTokenType>(
							ModelGrammar.TokenNone,
							ModelSubsequencer.ErrorUnexpectedEndOfInput);
					}

					yield break;
				}
				default:
				{
					throw new TokenException<ModelTokenType>(
						token,
						String.Format(ModelSubsequencer.ErrorInvalidPropertyValue, token.TokenType));
				}
			}
		}

		/// <summary>
		/// Splices out the sequence for the next complete value (object, array, primitive)
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private static TokenSequence SpliceNextValue(IStream<ModelToken> stream)
		{
			if (stream.IsCompleted)
			{
				throw new TokenException<ModelTokenType>(
					ModelGrammar.TokenNone,
					ModelSubsequencer.ErrorUnexpectedEndOfInput);
			}

			stream.BeginChunk();

			int depth = 0;
			ModelToken token = stream.Pop();
			switch (token.TokenType)
			{
				case ModelTokenType.Primitive:
				{
					return stream.EndChunk();
				}
				case ModelTokenType.ArrayBegin:
				case ModelTokenType.ObjectBegin:
				{
					depth++;
					while (!stream.IsCompleted && depth > 0)
					{
						switch (stream.Pop().TokenType)
						{
							case ModelTokenType.ArrayBegin:
							case ModelTokenType.ObjectBegin:
							{
								depth++;
								break;
							}
							case ModelTokenType.ArrayEnd:
							case ModelTokenType.ObjectEnd:
							{
								depth--;
								break;
							}
						}
					}

					if (depth > 0)
					{
						throw new TokenException<ModelTokenType>(
							ModelGrammar.TokenNone,
							ModelSubsequencer.ErrorUnexpectedEndOfInput);
					}
					return stream.EndChunk();
				}
				default:
				{
					throw new TokenException<ModelTokenType>(
						token,
						String.Format(ModelSubsequencer.ErrorInvalidPropertyValue, token.TokenType));
				}
			}
		}

		/// <summary>
		/// Skips over the next complete value (object, array, primitive)
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private static void SkipNextValue(IStream<ModelToken> stream)
		{
			if (stream.IsCompleted)
			{
				return;
			}

			int depth = 0;

			ModelToken token = stream.Pop();
			switch (token.TokenType)
			{
				case ModelTokenType.Primitive:
				{
					return;
				}
				case ModelTokenType.ArrayBegin:
				case ModelTokenType.ObjectBegin:
				{
					depth++;
					while (!stream.IsCompleted && depth > 0)
					{
						switch (stream.Pop().TokenType)
						{
							case ModelTokenType.ArrayBegin:
							case ModelTokenType.ObjectBegin:
							{
								depth++;
								break;
							}
							case ModelTokenType.ArrayEnd:
							case ModelTokenType.ObjectEnd:
							{
								depth--;
								break;
							}
						}
					}

					if (depth > 0)
					{
						throw new TokenException<ModelTokenType>(
							ModelGrammar.TokenNone,
							ModelSubsequencer.ErrorUnexpectedEndOfInput);
					}
					return;
				}
				default:
				{
					throw new TokenException<ModelTokenType>(
						token,
						String.Format(ModelSubsequencer.ErrorInvalidPropertyValue, token.TokenType));
				}
			}
		}

		#endregion Utility Methods
	}
}
