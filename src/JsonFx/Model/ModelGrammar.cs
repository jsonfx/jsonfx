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

namespace JsonFx.Model
{
	/// <summary>
	/// Common Model Language grammar helper
	/// </summary>
	/// <remarks>
	/// Simplifies and guides syntax, and provides a set of reusable tokens to reduce redundant token instantiations
	/// </remarks>
	public static class ModelGrammar
	{
		#region Reusable Tokens

		public static readonly Token<ModelTokenType> TokenNone = new Token<ModelTokenType>(ModelTokenType.None);

		public static readonly Token<ModelTokenType> TokenArrayEnd = new Token<ModelTokenType>(ModelTokenType.ArrayEnd);
		public static readonly Token<ModelTokenType> TokenObjectEnd = new Token<ModelTokenType>(ModelTokenType.ObjectEnd);

		public static readonly Token<ModelTokenType> TokenNull = new Token<ModelTokenType>(ModelTokenType.Primitive);
		public static readonly Token<ModelTokenType> TokenFalse = new Token<ModelTokenType>(ModelTokenType.Primitive, false);
		public static readonly Token<ModelTokenType> TokenTrue = new Token<ModelTokenType>(ModelTokenType.Primitive, true);

		public static readonly Token<ModelTokenType> TokenArrayBeginUnnamed = new Token<ModelTokenType>(ModelTokenType.ArrayBegin);
		public static readonly Token<ModelTokenType> TokenObjectBeginUnnamed = new Token<ModelTokenType>(ModelTokenType.ObjectBegin);

		internal static readonly Token<ModelTokenType> TokenNaN = new Token<ModelTokenType>(ModelTokenType.Primitive, Double.NaN);
		internal static readonly Token<ModelTokenType> TokenPositiveInfinity = new Token<ModelTokenType>(ModelTokenType.Primitive, Double.PositiveInfinity);
		internal static readonly Token<ModelTokenType> TokenNegativeInfinity = new Token<ModelTokenType>(ModelTokenType.Primitive, Double.NegativeInfinity);

		#endregion Reusable Tokens

		#region Token Factories

		/// <summary>
		/// Marks the beginning of an array
		/// </summary>
		/// <param name="name">the local name of the array</param>
		/// <returns>ArrayBegin Token</returns>
		public static Token<ModelTokenType> TokenArrayBegin(string name)
		{
			return new Token<ModelTokenType>(ModelTokenType.ArrayBegin, new DataName(name));
		}

		/// <summary>
		/// Marks the beginning of an array
		/// </summary>
		/// <param name="name">the local name of the array</param>
		/// <param name="namespaceUri">the namespace of the document</param>
		/// <returns>ArrayBegin Token</returns>
		public static Token<ModelTokenType> TokenArrayBegin(string name, string prefix, string namespaceUri)
		{
			return new Token<ModelTokenType>(ModelTokenType.ArrayBegin, new DataName(name, prefix, namespaceUri));
		}

		/// <summary>
		/// Marks the beginning of an array
		/// </summary>
		/// <param name="name">the name of the array</param>
		/// <returns>ArrayBegin Token</returns>
		public static Token<ModelTokenType> TokenArrayBegin(DataName name)
		{
			return new Token<ModelTokenType>(ModelTokenType.ArrayBegin, name);
		}

		/// <summary>
		/// Marks the beginning of an object
		/// </summary>
		/// <param name="name">the local name of the object</param>
		/// <returns>ObjectBegin Token</returns>
		public static Token<ModelTokenType> TokenObjectBegin(string name)
		{
			return new Token<ModelTokenType>(ModelTokenType.ObjectBegin, new DataName(name));
		}

		/// <summary>
		/// Marks the beginning of an object
		/// </summary>
		/// <param name="name">the name of the object</param>
		/// <returns>ObjectBegin Token</returns>
		public static Token<ModelTokenType> TokenObjectBegin(DataName name)
		{
			return new Token<ModelTokenType>(ModelTokenType.ObjectBegin, name);
		}

		/// <summary>
		/// Marks the beginning of an object property
		/// </summary>
		/// <param name="localName">the local name of the property</param>
		/// <returns>PropertyKey Token</returns>
		internal static Token<ModelTokenType> TokenProperty(object localName)
		{
			string name = Token<ModelTokenType>.ToString(localName);

			return new Token<ModelTokenType>(ModelTokenType.Property, new DataName(name));
		}

		/// <summary>
		/// Marks the beginning of an object property
		/// </summary>
		/// <param name="localName">the local name of the property</param>
		/// <returns>PropertyKey Token</returns>
		public static Token<ModelTokenType> TokenProperty(string localName)
		{
			return new Token<ModelTokenType>(ModelTokenType.Property, new DataName(localName));
		}

		/// <summary>
		/// Marks the beginning of an object property
		/// </summary>
		/// <param name="name">the name of the property</param>
		/// <returns>PropertyKey Token</returns>
		public static Token<ModelTokenType> TokenProperty(DataName name)
		{
			return new Token<ModelTokenType>(ModelTokenType.Property, name);
		}

		/// <summary>
		/// A simple scalar value (typically serialized as a single primitive value)
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Value Token</returns>
		public static Token<ModelTokenType> TokenPrimitive(object value)
		{
			return new Token<ModelTokenType>(ModelTokenType.Primitive, value);
		}

		#endregion Token Factories
	}
}
