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
		private readonly TextWriter Writer;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="writer"></param>
		public JsonFormatter(DataWriterSettings settings, TextWriter writer)
		{
			this.Settings = settings;
			this.Writer = writer;
		}

		#endregion Init

		#region Methods

		/// <summary>
		/// Formats the token sequence
		/// </summary>
		/// <param name="generator"></param>
		public void Format(IEnumerable<Token<JsonTokenType>> generator)
		{
			foreach (Token<JsonTokenType> token in generator)
			{
				switch (token.TokenType)
				{
					case JsonTokenType.ArrayBegin:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.ArrayEnd:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Boolean:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Identifier:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.None:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Null:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Number:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.ObjectBegin:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.ObjectEnd:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.PairDelim:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.String:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.Undefined:
					{
						this.Writer.WriteLine(token);
						break;
					}
					case JsonTokenType.ValueDelim:
					{
						this.Writer.WriteLine(token);
						break;
					}
				}
			}
		}

		#endregion Methods
	}
}
