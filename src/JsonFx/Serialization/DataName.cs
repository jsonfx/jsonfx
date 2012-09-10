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
using System.Globalization;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Represents a property or document name, and a corresponding namespace URI (or empty string).
	/// Analogous to XML "expanded name": http://www.w3.org/TR/REC-xml-names/#dt-expname
	/// </summary>
	/// <remarks>
	/// Namespaces must be a URI, but local-name can be any non-null string.
	/// It is up to formatters to determine how to properly represent names which are invalid for the format.
	/// </remarks>
	public struct DataName : IComparable<DataName>
	{
		#region Constants

		public static readonly DataName Empty = new DataName();

		#endregion Constants

		#region Fields

		/// <summary>
		/// local-name
		/// </summary>
		public readonly string LocalName;

		/// <summary>
		/// alias for the namespace
		/// </summary>
		public readonly string Prefix;

		/// <summary>
		/// namespace
		/// </summary>
		public readonly string NamespaceUri;

		/// <summary>
		/// Determines if name should be treated like an attribute
		/// </summary>
		public readonly bool IsAttribute;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="type">a CLR Type used to generate the local-name</param>
		/// <remarks>
		/// This constructor implicitly delcares the namespace to be empty.
		/// </remarks>
		public DataName(Type type)
			: this(DataName.GetTypeName(type))
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="localName">any string is a valid local-name</param>
		/// <remarks>
		/// This constructor implicitly delcares the namespace to be empty.
		/// </remarks>
		public DataName(string localName)
			: this(localName, null, null, false)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="localName">any string is a valid local-name</param>
		/// <param name="namespaceUri">an absolute URI string, or null</param>
		/// <exception cref="ArgumentNullException">thrown if <paramref name="localName"/> is null</exception>
		/// <exception cref="ArgumentNullException">thrown if <paramref name="namespaceUri"/> is an invalid absolute URI</exception>
		/// <remarks>
		/// The namespace field follows XML recommendation of absolute URIs.
		/// Relative URIs are officially deprecated for namespaces: http://www.w3.org/2000/09/xppa
		/// </remarks>
		public DataName(string localName, string prefix, string namespaceUri)
			: this(localName, prefix, namespaceUri, false)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="localName">any string is a valid local-name</param>
		/// <param name="namespaceUri">an absolute URI string, or null</param>
		/// <param name="isAttribute">determines if name should be an attribute name</param>
		/// <exception cref="ArgumentNullException">thrown if <paramref name="localName"/> is null</exception>
		/// <exception cref="ArgumentNullException">thrown if <paramref name="namespaceUri"/> is an invalid absolute URI</exception>
		/// <remarks>
		/// The namespace field follows XML recommendation of absolute URIs.
		/// Relative URIs are officially deprecated for namespaces: http://www.w3.org/2000/09/xppa
		/// </remarks>
		public DataName(string localName, string prefix, string namespaceUri, bool isAttribute)
		{
			if (localName == null)
			{
				throw new ArgumentNullException("localName");
			}

			if (prefix == null)
			{
				prefix = String.Empty;
			}

			if (String.IsNullOrEmpty(namespaceUri))
			{
				namespaceUri = String.Empty;
			}
			else if (!Uri.IsWellFormedUriString(namespaceUri, UriKind.Absolute))
			{
				throw new ArgumentException("Namespace must be an absolute URI or empty", "namespaceUri");
			}

			this.LocalName = localName;
			this.Prefix = prefix;
			this.NamespaceUri = namespaceUri;
			this.IsAttribute = isAttribute;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Determines if this is an empty DataName
		/// </summary>
		public bool IsEmpty
		{
			get { return this.LocalName == null; }
		}

		#endregion Properties

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

			if (typeof(IDictionary).IsAssignableFrom(type)
#if !NETCF
			 || type.GetInterface(TypeCoercionUtility.TypeGenericIDictionary, false) != null
#endif
			   )
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

			if ((isGeneric &&
#if SILVERLIGHT
				type.Name.StartsWith(TypeCoercionUtility.AnonymousTypePrefix, StringComparison.InvariantCulture)))
#else
				type.Name.StartsWith(TypeCoercionUtility.AnonymousTypePrefix, false, CultureInfo.InvariantCulture)))
#endif
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

		internal static string GetStandardPrefix(string namespaceUri)
		{
			// match standardized prefixes
			switch (namespaceUri)
			{
				case "http://www.w3.org/XML/1998/namespace":
				{
					// standard for XML
					return "xml";
				}
				case "http://www.w3.org/2001/XMLSchema":
				{
					// standard for XML Schema
					return "xs";
				}
				case "http://www.w3.org/2001/XMLSchema-instance":
				{
					// standard for XML Schema Instance
					return "xsi";
				}
				case "http://www.w3.org/2000/xmlns/":
				{
					return "xmlns";
				}
				case "http://www.w3.org/1999/xhtml":
				{
					// standard for XHTML
					return "html";
				}
				case "http://www.w3.org/2005/Atom":
				{
					// standard for Atom 1.0
					return "atom";
				}
				case "http://purl.org/dc/elements/1.1/":
				{
					// standard for Dublin Core
					return "dc";
				}
				case "http://purl.org/syndication/thread/1.0":
				{
					// standard for syndication threading
					return "thr";
				}
				default:
				{
					return null;
				}
			}
		}

		#endregion Utility Methods

		#region ToString Methods

		/// <summary>
		/// Gets the prefixed name or simply local-name if is not a fully qualified name
		/// </summary>
		/// <param name="addPrefixIfQualifiedAndEmpty">true to generate a prefix if the prefix is empty but not the namespace</param>
		/// <returns></returns>
		public string ToPrefixedName()
		{
			if (String.IsNullOrEmpty(this.Prefix))
			{
				return this.LocalName;
			}

			string prefix = this.Prefix;
			if (String.IsNullOrEmpty(prefix))
			{
				prefix = DataName.GetStandardPrefix(this.NamespaceUri) ?? "pfx";
			}

			return String.Concat(
				prefix,
				":",
				this.LocalName);
		}

		/// <summary>
		/// Gets the namespaced name or simply local-name if is not a fully qualified name
		/// </summary>
		/// <returns></returns>
		public string ToQualifiedName()
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

		#endregion ToString Methods

		#region Object Overrides

		public override string ToString()
		{
			if (String.IsNullOrEmpty(this.Prefix))
			{
				return this.ToQualifiedName();
			}

			return this.ToPrefixedName();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is DataName))
			{
				return false;
			}

			return this.Equals((DataName)obj);
		}

		public bool Equals(DataName that)
		{
			if (that == null)
			{
				return false;
			}

			return
				StringComparer.Ordinal.Equals(this.NamespaceUri, that.NamespaceUri) &&
				StringComparer.Ordinal.Equals(this.Prefix, that.Prefix) &&
				StringComparer.Ordinal.Equals(this.LocalName, that.LocalName);
		}

		public override int GetHashCode()
		{
			int hash = 0x36294b26;

			hash = (-1521134295 * hash) + StringComparer.Ordinal.GetHashCode(this.LocalName ?? String.Empty);
			hash = (-1521134295 * hash) + StringComparer.Ordinal.GetHashCode(this.Prefix ?? String.Empty);
			hash = (-1521134295 * hash) + StringComparer.Ordinal.GetHashCode(this.NamespaceUri ?? String.Empty);

			return hash;
		}

		#endregion Object Overrides

		#region Operators

		public static bool operator ==(DataName a, DataName b)
		{
			if (Object.ReferenceEquals(a, null))
			{
				return Object.ReferenceEquals(b, null);
			}

			return a.Equals(b);
		}

		public static bool operator !=(DataName a, DataName b)
		{
			return !(a == b);
		}

		#endregion Operators

		#region IComparable<DataName> Members

		/// <summary>
		/// Compares two <see cref="DataName"/> values and returns an indication of their relative sort order.
		/// </summary>
		/// <param name="that"></param>
		/// <returns></returns>
		/// <remarks>
		/// Performs ordering according to XML Canonicalization document order http://www.w3.org/TR/xml-c14n#DocumentOrder
		/// "An element's namespace nodes are sorted lexicographically by local name (the default namespace node, if one exists, has no local name and is therefore lexicographically least)."
		/// "An element's attribute nodes are sorted lexicographically with namespace URI as the primary key and local name as the secondary key (an empty namespace URI is lexicographically least)."
		/// </remarks>
		public int CompareTo(DataName that)
		{
			int nsCompare = StringComparer.Ordinal.Compare(this.NamespaceUri, that.NamespaceUri);
			if (nsCompare != 0)
			{
				return nsCompare;
			}

			return StringComparer.Ordinal.Compare(this.LocalName, that.LocalName);
		}

		#endregion IComparable<DataName> Members
	}
}
