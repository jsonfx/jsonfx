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

using JsonFx.Serialization;

namespace JsonFx.Common
{
	/// <summary>
	/// Common Data Language grammar helper
	/// </summary>
	/// <remarks>
	/// Simplifies and guides syntax, and provides a set of reusable tokens to reduce redundant token instantiations
	/// </remarks>
	public static class CommonGrammar
	{
		#region Reusable Tokens

		public static readonly Token<CommonTokenType> TokenNone = new Token<CommonTokenType>(CommonTokenType.None);

		public static readonly Token<CommonTokenType> TokenArrayEnd = new Token<CommonTokenType>(CommonTokenType.ArrayEnd);
		public static readonly Token<CommonTokenType> TokenObjectEnd = new Token<CommonTokenType>(CommonTokenType.ObjectEnd);

		public static readonly Token<CommonTokenType> TokenNull = new Token<CommonTokenType>(CommonTokenType.Primitive);
		public static readonly Token<CommonTokenType> TokenFalse = new Token<CommonTokenType>(CommonTokenType.Primitive, false);
		public static readonly Token<CommonTokenType> TokenTrue = new Token<CommonTokenType>(CommonTokenType.Primitive, true);

		public static readonly Token<CommonTokenType> TokenArrayBeginNoName = new Token<CommonTokenType>(CommonTokenType.ArrayBegin);
		public static readonly Token<CommonTokenType> TokenObjectBeginNoName = new Token<CommonTokenType>(CommonTokenType.ObjectBegin);

		internal static readonly Token<CommonTokenType> TokenNaN = new Token<CommonTokenType>(CommonTokenType.Primitive, Double.NaN);
		internal static readonly Token<CommonTokenType> TokenPositiveInfinity = new Token<CommonTokenType>(CommonTokenType.Primitive, Double.PositiveInfinity);
		internal static readonly Token<CommonTokenType> TokenNegativeInfinity = new Token<CommonTokenType>(CommonTokenType.Primitive, Double.NegativeInfinity);

		#endregion Reusable Tokens

		#region Token Factories

		/// <summary>
		/// Marks the beginning of an array
		/// </summary>
		/// <param name="name">the local name of the array</param>
		/// <returns>ArrayBegin Token</returns>
		public static Token<CommonTokenType> TokenArrayBegin(string name)
		{
			return new Token<CommonTokenType>(CommonTokenType.ArrayBegin, new DataName(name));
		}

		/// <summary>
		/// Marks the beginning of an array
		/// </summary>
		/// <param name="name">the local name of the array</param>
		/// <param name="namespaceUri">the namespace of the document</param>
		/// <returns>ArrayBegin Token</returns>
		public static Token<CommonTokenType> TokenArrayBegin(string name, string prefix, string namespaceUri)
		{
			return new Token<CommonTokenType>(CommonTokenType.ArrayBegin, new DataName(name, prefix, namespaceUri));
		}

		/// <summary>
		/// Marks the beginning of an array
		/// </summary>
		/// <param name="name">the name of the array</param>
		/// <returns>ArrayBegin Token</returns>
		public static Token<CommonTokenType> TokenArrayBegin(DataName name)
		{
			return new Token<CommonTokenType>(CommonTokenType.ArrayBegin, name);
		}

		/// <summary>
		/// Marks the beginning of an object
		/// </summary>
		/// <param name="name">the local name of the object</param>
		/// <returns>ObjectBegin Token</returns>
		public static Token<CommonTokenType> TokenObjectBegin(string name)
		{
			return new Token<CommonTokenType>(CommonTokenType.ObjectBegin, new DataName(name));
		}

		/// <summary>
		/// Marks the beginning of an object
		/// </summary>
		/// <param name="name">the name of the object</param>
		/// <returns>ObjectBegin Token</returns>
		public static Token<CommonTokenType> TokenObjectBegin(DataName name)
		{
			return new Token<CommonTokenType>(CommonTokenType.ObjectBegin, name);
		}

		/// <summary>
		/// Marks the beginning of an object property
		/// </summary>
		/// <param name="localName">the local name of the property</param>
		/// <returns>PropertyKey Token</returns>
		internal static Token<CommonTokenType> TokenProperty(object localName)
		{
			string name = Token<CommonTokenType>.ToString(localName);

			return new Token<CommonTokenType>(CommonTokenType.Property, new DataName(name));
		}

		/// <summary>
		/// Marks the beginning of an object property
		/// </summary>
		/// <param name="localName">the local name of the property</param>
		/// <returns>PropertyKey Token</returns>
		public static Token<CommonTokenType> TokenProperty(string localName)
		{
			return new Token<CommonTokenType>(CommonTokenType.Property, new DataName(localName));
		}

		/// <summary>
		/// Marks the beginning of an object property
		/// </summary>
		/// <param name="name">the name of the property</param>
		/// <returns>PropertyKey Token</returns>
		public static Token<CommonTokenType> TokenProperty(DataName name)
		{
			return new Token<CommonTokenType>(CommonTokenType.Property, name);
		}

		/// <summary>
		/// A simple scalar value (typically serialized as a single primitive value)
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Value Token</returns>
		public static Token<CommonTokenType> TokenValue(object value)
		{
			return new Token<CommonTokenType>(CommonTokenType.Primitive, value);
		}

		#endregion Token Factories
	}
}
