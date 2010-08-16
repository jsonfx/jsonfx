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

namespace JsonFx.Markup
{
	/// <summary>
	/// Formal language of tokens and symbols for markup
	/// </summary>
	internal class MarkupGrammar
	{
		#region Operators

		public const char OperatorElementBegin = '<';
		public const char OperatorElementEnd = '>';
		public const char OperatorElementClose = '/';
		public const char OperatorValueDelim = ' ';
		public const char OperatorPairDelim = '=';
		public const char OperatorPrefixDelim = ':';

		public const char OperatorStringDelim = '"';
		public const char OperatorStringDelimAlt = '\'';

		public const char OperatorEntityBegin = '&';
		public const char OperatorEntityNum = '#';
		public const char OperatorEntityHex = 'x';
		public const char OperatorEntityHexAlt = 'X';
		public const char OperatorEntityEnd = ';';

		public const char OperatorComment = '!';
		public const char OperatorCommentDelim = '-';
		public const string OperatorCommentBegin = "--";
		public const string OperatorCommentEnd = "--";
		public const string OperatorCDataBegin = "[CDATA[";
		public const string OperatorCDataEnd = "]]";

		public const char OperatorProcessingInstruction = '?';
		public const string OperatorPhpExpressionBegin = "?=";
		public const string OperatorProcessingInstructionBegin = "?";
		public const string OperatorProcessingInstructionEnd = "?";

		public const char OperatorCode = '%';
		public const char OperatorCodeDirective = '@';
		public const char OperatorCodeExpression = '=';
		public const char OperatorCodeDeclaration = '!';
		public const char OperatorCodeEncoded = ':';
		public const char OperatorCodeDataBind = '#';
		public const char OperatorCodeExtension = '$';
		public const string OperatorCodeBlockBegin = "%";
		public const string OperatorCodeEnd = "%";

		public const char OperatorT4 = '#';
		public const char OperatorT4Directive = '@';
		public const char OperatorT4Expression = '=';
		public const char OperatorT4ClassFeature = '+';
		public const string OperatorT4BlockBegin = "#";
		public const string OperatorT4End = "#";

		#endregion Operators

		#region Reusable Tokens

		public static readonly Token<MarkupTokenType> TokenNone = new Token<MarkupTokenType>(MarkupTokenType.None);
		public static readonly Token<MarkupTokenType> TokenElementEnd = new Token<MarkupTokenType>(MarkupTokenType.ElementEnd);

		/// <summary>
		/// Any of a number of unparsed tags which typically contain specialized processing instructions
		/// </summary>
		/// <remarks>
		/// The name of the token is the beginning and ending delimiters as a format string (not including the '&lt;' or '&gt;')
		/// Includes the following types:
		/// 
		///		"&lt;!--", "-->"	XML/HTML/SGML comment
		///		"&lt;!", ">"		XML/SGML declaration (e.g. DOCTYPE or server-side includes)
		/// 
		///		"&lt;?=", "?>"		PHP expression
		///		"&lt;?", "?>"		PHP code block /XML processing instruction (e.g. the XML declaration)
		/// 
		///		"&lt;%--", "--%>"	ASP/PSP/JSP-style code comment
		///		"&lt;%@",  "%>"		ASP/PSP/JSP directive
		///		"&lt;%=",  "%>"		ASP/PSP/JSP/JBST expression
		///		"&lt;%!",  "%>"		JSP/JBST declaration
		///		"&lt;%#",  "%>"		ASP.NET/JBST databind expression
		///		"&lt;%$",  "%>"		ASP.NET/JBST extension
		///		"&lt;%",   "%>"		ASP code block / JSP scriptlet / PSP code block
		/// </remarks>
		public static Token<MarkupTokenType> TokenUnparsed(string begin, string end, string value)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.Primitive, new UnparsedBlock(begin, end, value));
		}

		public static Token<MarkupTokenType> TokenElementBegin(DataName name)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.ElementBegin, name);
		}

		public static Token<MarkupTokenType> TokenElementVoid(DataName name)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.ElementVoid, name);
		}

		public static Token<MarkupTokenType> TokenAttribute(DataName name)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.Attribute, name);
		}

		public static Token<MarkupTokenType> TokenPrimitive(object value)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.Primitive, value);
		}

		#endregion Reusable Tokens
	}
}
