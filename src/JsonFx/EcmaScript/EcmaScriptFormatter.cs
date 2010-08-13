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
using System.Text.RegularExpressions;

using JsonFx.Json;
using JsonFx.Serialization;

namespace JsonFx.EcmaScript
{
	/// <summary>
	/// Formats data as full ECMAScript objects, rather than the limited set of JSON objects.
	/// </summary>
	public class EcmaScriptFormatter : JsonWriter.JsonFormatter
	{
		#region Constants

		private static readonly DateTime EcmaScriptEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		private const string EcmaScriptDateCtor1 = "new Date({0})";
		private const string EcmaScriptDateCtor7 = "new Date({0:0000},{1},{2},{3},{4},{5},{6})";
		private const string EmptyRegExpLiteral = "(?:)";
		private const char RegExpLiteralDelim = '/';
		private const char OperatorCharEscape = '\\';

		private const string NamespaceDelim = ".";
		private static readonly char[] NamespaceDelims = { '.' };

		private const string RootDeclarationDebug =
@"
/* namespace {1} */
var {0};";

		private const string RootDeclaration = @"var {0};";

		private const string NamespaceCheck =
@"if(""undefined""===typeof {0}){{{0}={{}};}}";
		private const string NamespaceCheckDebug =
@"
if (""undefined"" === typeof {0}) {{
	{0} = {{}};
}}";
		private static readonly IList<string> BrowserObjects = new List<string>(new string[]
		{
			"console",
			"document",
			"event",
			"frames",
			"history",
			"location",
			"navigator",
			"opera",
			"screen",
			"window"
		});

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <remarks>
		/// Defaults to encoding &lt; chars for improved embedding within script blocks
		/// </remarks>
		public EcmaScriptFormatter(DataWriterSettings settings)
			: base(settings)
		{
		}

		#endregion Init

		#region Namespace Methods

		/// <summary>
		/// Emits a block of script ensuring that a namespace is declared
		/// </summary>
		/// <param name="writer">the output writer</param>
		/// <param name="ident">the namespace to ensure</param>
		/// <param name="namespaces">list of namespaces already emitted</param>
		/// <param name="debug">determines if should emit pretty-printed</param>
		/// <returns>if was a namespaced identifier</returns>
		public static bool WriteNamespaceDeclaration(TextWriter writer, string ident, List<string> namespaces, bool prettyPrint)
		{
			if (String.IsNullOrEmpty(ident))
			{
				return false;
			}

			if (namespaces == null)
			{
				namespaces = new List<string>();
			}

			string[] nsParts = ident.Split(EcmaScriptFormatter.NamespaceDelims, StringSplitOptions.RemoveEmptyEntries);
			string ns = nsParts[0];

			bool isNested = false;
			for (int i=0; i<nsParts.Length-1; i++)
			{
				isNested = true;

				if (i > 0)
				{
					ns += EcmaScriptFormatter.NamespaceDelim;
					ns += nsParts[i];
				}

				if (namespaces.Contains(ns) ||
					EcmaScriptFormatter.BrowserObjects.Contains(ns))
				{
					// don't emit multiple checks for same namespace
					continue;
				}

				// make note that we've emitted this namespace before
				namespaces.Add(ns);

				if (i == 0)
				{
					if (prettyPrint)
					{
						writer.Write(EcmaScriptFormatter.RootDeclarationDebug, ns,
							String.Join(NamespaceDelim, nsParts, 0, nsParts.Length-1));
					}
					else
					{
						writer.Write(EcmaScriptFormatter.RootDeclaration, ns);
					}
				}

				if (prettyPrint)
				{
					writer.WriteLine(EcmaScriptFormatter.NamespaceCheckDebug, ns);
				}
				else
				{
					writer.Write(EcmaScriptFormatter.NamespaceCheck, ns);
				}
			}

			if (prettyPrint && isNested)
			{
				writer.WriteLine();
			}

			return isNested;
		}

		#endregion Namespace Methods

		#region JsonFormatter Methods

		protected override void WriteNaN(TextWriter writer)
		{
			writer.Write(JsonGrammar.KeywordNaN);
		}

		protected override void WriteNegativeInfinity(TextWriter writer)
		{
			writer.Write('-');
			writer.Write(JsonGrammar.KeywordInfinity);
		}

		protected override void WritePositiveInfinity(TextWriter writer)
		{
			writer.Write(JsonGrammar.KeywordInfinity);
		}

		protected override void WritePrimitive(TextWriter writer, object value)
		{
			if (value is DateTime)
			{
				// write as ECMAScript Date constructor
				EcmaScriptFormatter.WriteEcmaScriptDate(writer, (DateTime)value);
				return;
			}
			if (value is Regex)
			{
				EcmaScriptFormatter.WriteEcmaScriptRegExp(writer, (Regex)value);
				return;
			}

			base.WritePrimitive(writer, value);
		}

		protected override void WritePropertyName(TextWriter writer, string propertyName)
		{
			if (EcmaScriptIdentifier.IsValidIdentifier(propertyName, false))
			{
				// write out without quoting
				writer.Write(propertyName);
			}
			else
			{
				// write out as an escaped string
				base.WritePropertyName(writer, propertyName);
			}
		}

		#endregion JsonFormatter Methods

		#region Formatting Methods

		public static void WriteEcmaScriptDate(TextWriter writer, DateTime value)
		{
			if (value.Kind == DateTimeKind.Unspecified)
			{
				// unknown timezones serialize directly to become browser-local
				writer.Write(
					EcmaScriptFormatter.EcmaScriptDateCtor7,
					value.Year,			// yyyy
					value.Month-1,		// 0-11
					value.Day,			// 1-31
					value.Hour,			// 0-23
					value.Minute,		// 0-60
					value.Second,		// 0-60
					value.Millisecond);	// 0-999
				return;
			}

			if (value.Kind == DateTimeKind.Local)
			{
				// convert server-local to UTC
				value = value.ToUniversalTime();
			}

			// find the time since Jan 1, 1970
			TimeSpan duration = value.Subtract(EcmaScriptFormatter.EcmaScriptEpoch);

			// get the total milliseconds
			long ticks = (long)duration.TotalMilliseconds;

			// write out as a Date constructor
			writer.Write(
				EcmaScriptFormatter.EcmaScriptDateCtor1,
				ticks);
		}

		/// <summary>
		/// Outputs a .NET Regex as an ECMAScript RegExp literal.
		/// Defaults to global matching off.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="regex"></param>
		/// <remarks>
		/// http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf
		/// </remarks>
		public static void WriteEcmaScriptRegExp(TextWriter writer, Regex regex)
		{
			EcmaScriptFormatter.WriteEcmaScriptRegExp(writer, regex, false);
		}

		/// <summary>
		/// Outputs a .NET Regex as an ECMAScript RegExp literal.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="regex"></param>
		/// <param name="isGlobal"></param>
		/// <remarks>
		/// http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf
		/// </remarks>
		public static void WriteEcmaScriptRegExp(TextWriter writer, Regex regex, bool isGlobal)
		{
			if (regex == null)
			{
				writer.Write(JsonGrammar.KeywordNull);
				return;
			}

			// Regex.ToString() returns the original pattern
			string pattern = regex.ToString();
			if (String.IsNullOrEmpty(pattern))
			{
				// must output something otherwise becomes a code comment
				pattern = EcmaScriptFormatter.EmptyRegExpLiteral;
			}

			string modifiers = isGlobal ? "g" : "";
			switch (regex.Options & (RegexOptions.IgnoreCase|RegexOptions.Multiline))
			{
				case RegexOptions.IgnoreCase:
				{
					modifiers += "i";
					break;
				}
				case RegexOptions.Multiline:
				{
					modifiers += "m";
					break;
				}
				case RegexOptions.IgnoreCase|RegexOptions.Multiline:
				{
					modifiers += "im";
					break;
				}
			}

			writer.Write(EcmaScriptFormatter.RegExpLiteralDelim);

			int length = pattern.Length;
			int start = 0;

			for (int i = start; i < length; i++)
			{
				switch (pattern[i])
				{
					case EcmaScriptFormatter.RegExpLiteralDelim:
					{
						writer.Write(pattern.Substring(start, i - start));
						start = i + 1;
						writer.Write(EcmaScriptFormatter.OperatorCharEscape);
						writer.Write(pattern[i]);
						break;
					}
				}
			}

			writer.Write(pattern.Substring(start, length - start));
			writer.Write(EcmaScriptFormatter.RegExpLiteralDelim);
			writer.Write(modifiers);
		}

		#endregion Writer Methods
	}
}
