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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace JsonFx.Serialization.Resolvers
{
	/// <summary>
	/// Represents a property or document name, and a corresponding namespace URI if applicable
	/// </summary>
	/// <remarks>
	/// Namespaces must be a URI, but local-name can be any non-null string.
	/// It is up to formatters to determine how to properly represent names which are invalid for the format.
	/// </remarks>
	public class DataName
	{
		#region Constants

		private const string AnonymousTypePrefix = "<>f__AnonymousType";
		private static readonly string TypeGenericIDictionary = typeof(IDictionary<,>).FullName;

		public static readonly DataName Empty = new DataName(String.Empty);

		#endregion Constants

		#region Fields

		public readonly string LocalName;
		public readonly string NamespaceUri;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="type"></param>
		public DataName(Type type)
			: this(DataName.GetTypeName(type))
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="localName"></param>
		public DataName(string localName)
			: this(localName, null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="localName"></param>
		/// <param name="namespaceUri"></param>
		public DataName(string localName, string namespaceUri)
		{
			if (localName == null)
			{
				throw new ArgumentNullException("localName");
			}

			if (String.IsNullOrEmpty(namespaceUri))
			{
				namespaceUri = String.Empty;
			}
			else if (!Uri.IsWellFormedUriString(namespaceUri, UriKind.Absolute))
			{
				throw new ArgumentNullException("namespaceUri");
			}

			this.LocalName = localName;
			this.NamespaceUri = namespaceUri;
		}

		#endregion Init

		#region Utility Methods

		/// <summary>
		/// Gets the local-name for a Type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static string GetTypeName(Type type)
		{
			if (type == typeof(object))
			{
				type = null;
			}

			// using XSD-style defaults for .NET primitives
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.DBNull:
				case TypeCode.Empty:
				{
					return "object";
				}
				case TypeCode.Boolean:
				{
					return "boolean";
				}
				case TypeCode.Byte:
				{
					return "unsignedByte";
				}
				case TypeCode.Char:
				{
					return "Char";
				}
				case TypeCode.Decimal:
				{
					return "decimal";
				}
				case TypeCode.Double:
				{
					return "double";
				}
				case TypeCode.Int16:
				{
					return "short";
				}
				case TypeCode.Int32:
				{
					return "int";
				}
				case TypeCode.Int64:
				{
					return "long";
				}
				case TypeCode.SByte:
				{
					return "byte";
				}
				case TypeCode.Single:
				{
					return "float";
				}
				case TypeCode.UInt16:
				{
					return "unsignedShort";
				}
				case TypeCode.UInt32:
				{
					return "unsignedInt";
				}
				case TypeCode.UInt64:
				{
					return "unsignedLong";
				}
				case TypeCode.String:
				{
					return "string";
				}
				case TypeCode.DateTime:
				{
					return "dateTime";
				}
				case TypeCode.Object:
				{
					break;
				}
			}

			if (typeof(IDictionary).IsAssignableFrom(type) || type.GetInterface(DataName.TypeGenericIDictionary) != null)
			{
				// generic IDictionary or IDictionary<,>
				return "object";
			}

			if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type))
			{
				// generic IEnumerable collection
				return "array";
			}

			bool isGeneric = type.IsGenericType;

			if ((isGeneric && type.Name.StartsWith(DataName.AnonymousTypePrefix, false, CultureInfo.InvariantCulture)))
			{
				return "object";
			}

			string typeName = type.Name;

			int tick = isGeneric ? typeName.IndexOf('`') : -1;
			if (tick >= 0)
			{
				typeName = typeName.Substring(0, tick);
			}

			return typeName;
		}

		#endregion Utility Methods

		#region Object Overrides

		public override string ToString()
		{
			if (String.IsNullOrEmpty(this.NamespaceUri))
			{
				return this.LocalName;
			}

			return String.Concat(
				"{",
				this.NamespaceUri,
				"}",
				this.LocalName);
		}

		public override bool Equals(object obj)
		{
			DataName that = obj as DataName;
			if (that == null)
			{
				return false;
			}

			return
				StringComparer.Ordinal.Equals(this.NamespaceUri, that.NamespaceUri) &&
				StringComparer.Ordinal.Equals(this.LocalName, that.LocalName);
		}

		public override int GetHashCode()
		{
			int hash = 0x36294b26;

			hash = (-1521134295 * hash) + StringComparer.Ordinal.GetHashCode(this.LocalName);
			hash = (-1521134295 * hash) + StringComparer.Ordinal.GetHashCode(this.NamespaceUri);

			return hash;
		}

		#endregion Object Overrides
	}
}
