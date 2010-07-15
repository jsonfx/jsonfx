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
