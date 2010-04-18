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
using System.IO;

namespace JsonFx.Json
{
	/// <summary>
	/// Formats a sequence of JSON tokens
	/// </summary>
	public class JsonFormatter : IFormatter<JsonTokenType>
	{
		#region Fields

		private readonly DataWriterSettings Settings;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public JsonFormatter(DataWriterSettings settings)
		{
			this.Settings = settings;
		}

		#endregion Init

		#region Methods

		/// <summary>
		/// Formats the token sequence
		/// </summary>
		/// <param name="generator"></param>
		public void Write(TextWriter writer, IEnumerable<Token<JsonTokenType>> tokens)
		{
			// TODO: render tokens
			foreach (Token<JsonTokenType> token in tokens)
			{
				switch (token.TokenType)
				{
					case JsonTokenType.ArrayBegin:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.ArrayEnd:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Boolean:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Identifier:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.None:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Null:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Number:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.ObjectBegin:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.ObjectEnd:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.PairDelim:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.String:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Undefined:
					{
						writer.WriteLine(token);
						break;
					}
					case JsonTokenType.ValueDelim:
					{
						writer.WriteLine(token);
						break;
					}
				}
			}
		}

		#endregion Methods
	}
}
