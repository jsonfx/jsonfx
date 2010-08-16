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
using System.IO;

using JsonFx.Model;
using JsonFx.Serialization;

namespace JsonFx.EcmaScript
{
	/// <summary>
	/// Represents an ECMAScript identifier for serialization.
	/// </summary>
	public class EcmaScriptIdentifier : ITextFormattable<ModelTokenType>
	{
		#region Fields

		private readonly string identifier;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public EcmaScriptIdentifier()
			: this(null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <remarks></remarks>
		public EcmaScriptIdentifier(string ident)
		{
			this.identifier =
				String.IsNullOrEmpty(ident) ?
				String.Empty :
				EcmaScriptIdentifier.VerifyIdentifier(ident, true);
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the ECMAScript identifier represented by this instance
		/// </summary>
		public string Identifier
		{
			get { return this.identifier; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Verifies is a valid EcmaScript identifier
		/// </summary>
		/// <param name="ident">the identifier</param>
		/// <returns>identifier</returns>
		public static string VerifyIdentifier(string ident, bool nested)
		{
			return EcmaScriptIdentifier.VerifyIdentifier(ident, nested, true);
		}

		/// <summary>
		/// Verifies is a valid EcmaScript identifier
		/// </summary>
		/// <param name="ident">the identifier</param>
		/// <returns>identifier</returns>
		public static string VerifyIdentifier(string ident, bool nested, bool throwOnEmpty)
		{
			if (String.IsNullOrEmpty(ident))
			{
				if (throwOnEmpty)
				{
					throw new ArgumentException("Identifier is empty.");
				}
				return String.Empty;
			}

			ident = ident.Replace(" ", "");

			if (!EcmaScriptIdentifier.IsValidIdentifier(ident, nested))
			{
				throw new ArgumentException("Identifier \""+ident+"\" is not supported.");
			}

			return ident;
		}

		/// <summary>
		/// Verifies is a valid EcmaScript variable expression
		/// </summary>
		/// <param name="ident">the identifier</param>
		/// <returns>identifier</returns>
		/// <remarks>
		/// http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf
		/// 
		/// IdentifierName =
		///		IdentifierStart | IdentifierName IdentifierPart
		/// IdentifierStart =
		///		Letter | '$' | '_'
		/// IdentifierPart =
		///		IdentifierStart | Digit
		/// </remarks>
		public static bool IsValidIdentifier(string ident, bool nested)
		{
			if (String.IsNullOrEmpty(ident))
			{
				return false;
			}

			if (nested)
			{
				string[] parts = ident.Split('.');
				foreach (string part in parts)
				{
					if (!EcmaScriptIdentifier.IsValidIdentifier(part, false))
					{
						return false;
					}
				}
				return true;
			}

			if (EcmaScriptIdentifier.IsReservedWord(ident))
			{
				return false;
			}

			bool indentPart = false;
			for (int i=0, length=ident.Length; i<length; i++)
			{
				char ch = ident[i];
				if (indentPart && ((ch >= '0') && (ch <= '9')))
				{
					// digits are only allowed after first char
					continue;
				}

				// can be start or part
				if (((ch >= 'a') && (ch <= 'z')) ||
					((ch >= 'A') && (ch <= 'Z')) ||
					(ch == '_') || (ch == '$'))
				{
					indentPart = true;
					continue;
				}

				return false;
			}

			return true;
		}

		private static bool IsReservedWord(string ident)
		{
			// TODO: investigate doing this like Rhino does (switch on length check first letter or two)
			switch (ident)
			{
				// literals
				case "null":
				case "false":
				case "true":

				// ES5 Keywords
				case "break":
				case "case":
				case "catch":
				case "continue":
				case "debugger":
				case "default":
				case "delete":
				case "do":
				case "else":
				case "finally":
				case "for":
				case "function":
				case "if":
				case "in":
				case "instanceof":
				case "new":
				case "return":
				case "switch":
				case "this":
				case "throw":
				case "try":
				case "typeof":
				case "var":
				case "void":
				case "while":
				case "with":

				// ES5 Future Reserved Words
				case "abstract":
				case "boolean":
				case "byte":
				case "char":
				case "class":
				case "const":
				case "double":
				case "enum":
				case "export":
				case "extends":
				case "final":
				case "float":
				case "goto":
				case "implements":
				case "import":
				case "int":
				case "interface":
				case "long":
				case "native":
				case "package":
				case "private":
				case "protected":
				case "public":
				case "short":
				case "static":
				case "super":
				case "synchronized":
				case "throws":
				case "transient":
				case "volatile":

				// ES5 Possible Reserved Words
				case "let":
				case "yield":
				{
					return true;
				}
				default:
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Trivial conversion method. Essentially performs a cast.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks>
		/// Supports conversion via System.Web.UI.PropertyConverter.ObjectFromString(Type, MemberInfo, string)
		/// </remarks>
		public static EcmaScriptIdentifier Parse(string value)
		{
			return new EcmaScriptIdentifier(value);
		}

		#endregion Methods

		#region Operators

		/// <summary>
		/// Implicit type conversion allows to be used directly as a String
		/// </summary>
		/// <param name="ident">valid ECMAScript identifier</param>
		/// <returns></returns>
		public static implicit operator string(EcmaScriptIdentifier ident)
		{
			if (ident == null)
			{
				return String.Empty;
			}
			return ident.identifier;
		}

		/// <summary>
		/// Implicit type conversion allows to be used directly with Strings
		/// </summary>
		/// <param name="ident">valid ECMAScript identifier</param>
		/// <returns></returns>
		public static implicit operator EcmaScriptIdentifier(string ident)
		{
			return new EcmaScriptIdentifier(ident);
		}

		#endregion Operators

		#region ITextFormattable<ModelTokenType> Members

		void ITextFormattable<ModelTokenType>.Format(ITextFormatter<ModelTokenType> formatter, TextWriter writer)
		{
			if (String.IsNullOrEmpty(this.identifier))
			{
				writer.Write("null");
			}
			else
			{
				// TODO: determine if this should wrap parens around identifier
				writer.Write(this.identifier);
			}
		}

		#endregion ITextFormattable<ModelTokenType> Members

		#region Object Overrides

		/// <summary>
		/// Returns the identifier
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.identifier;
		}

		/// <summary>
		/// Compares identifiers
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			EcmaScriptIdentifier that = obj as EcmaScriptIdentifier;
			if (that == null)
			{
				return base.Equals(obj);
			}

			if (String.IsNullOrEmpty(this.identifier) && String.IsNullOrEmpty(that.identifier))
			{
				// null and String.Empty are equivalent
				return true;
			}

			return StringComparer.Ordinal.Equals(this.identifier, that.identifier);
		}

		/// <summary>
		/// Returns the hash code for the identifier
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return StringComparer.Ordinal.GetHashCode(this.identifier);
		}

		#endregion Object Overrides
	}
}
