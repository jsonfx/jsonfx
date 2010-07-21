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

using JsonFx.Serialization.Resolvers;

namespace JsonFx.Xml.Stax
{
	/// <summary>
	/// Maintains scope chain for StAX prefixes
	/// </summary>
	internal class PrefixScopeChain
	{
		#region Scope

		/// <summary>
		/// Represents a scope boundary within a prefix scope chain
		/// </summary>
		public class Scope : IEnumerable<KeyValuePair<string, string>>
		{
			#region Fields

			private SortedList<string, string> prefixes;

			#endregion Fields

			#region Properties

			/// <summary>
			/// Gets and sets the tagname associated with this scope boundary
			/// </summary>
			public DataName TagName
			{
				get;
				set;
			}

			/// <summary>
			/// Gets and sets mappings between prefix and namespace URIs
			/// </summary>
			/// <param name="prefix"></param>
			/// <returns></returns>
			public string this[string prefix]
			{
				get
				{
					string namespaceUri;
					if (this.prefixes == null || !this.prefixes.TryGetValue(prefix, out namespaceUri))
					{
						return null;
					}

					return namespaceUri;
				}
				set
				{
					if (this.prefixes == null)
					{
						this.prefixes = new SortedList<string, string>(StringComparer.Ordinal);
					}

					this.prefixes[prefix] = value;
				}
			}

			#endregion Properties

			#region Methods

			/// <summary>
			/// Returns if this scope boundary contains a mapping for a particular prefix
			/// </summary>
			/// <param name="prefix"></param>
			/// <returns>if this scope boundary contains a mapping for a particular prefix</returns>
			public bool ContainsPrefix(string prefix)
			{
				return (this.prefixes != null) && (this.prefixes.ContainsKey(prefix));
			}

			/// <summary>
			/// Returns if this scope boundary contains a mapping for a particular prefix
			/// setting the namespace URI if one was found.
			/// </summary>
			/// <param name="prefix"></param>
			/// <param name="namespaceUri">the resolved namespace URI</param>
			/// <returns>if this scope boundary contains a mapping for a particular prefix</returns>
			public bool TryGetPrefix(string prefix, out string namespaceUri)
			{
				namespaceUri = null;

				return (this.prefixes != null) && (this.prefixes.TryGetValue(prefix, out namespaceUri));
			}

			#endregion Methods

			#region IEnumerable<KeyValuePair<string,string>> Members

			public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
			{
				if (this.prefixes == null)
				{
					yield break;
				}

				foreach (var mapping in this.prefixes)
				{
					yield return mapping;
				}
			}

			#endregion IEnumerable<KeyValuePair<string,string>> Members

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion IEnumerable Members
		}

		#endregion Scope

		#region Fields

		private readonly List<Scope> Chain = new List<Scope>();

		#endregion Fields

		#region Properties

		public int Count
		{
			get { return this.Chain.Count; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Adds a new scope to the chain
		/// </summary>
		/// <param name="scope"></param>
		public void Push(Scope scope)
		{
			if (scope == null)
			{
				throw new ArgumentNullException("scope");
			}

			this.Chain.Add(scope);
		}

		/// <summary>
		/// Gets the last scope off the chain
		/// </summary>
		public Scope Peek()
		{
			int last = this.Chain.Count-1;
			if (last < 0)
			{
				return null;
			}

			return this.Chain[last];
		}

		/// <summary>
		/// Gets and removes the last scope off the chain
		/// </summary>
		/// <returns></returns>
		public Scope Pop()
		{
			int last = this.Chain.Count-1;
			if (last < 0)
			{
				return null;
			}

			Scope value = this.Chain[last];

			this.Chain.RemoveAt(last);

			return value;
		}

		/// <summary>
		/// Resolves the namespace URI for a given prefix within the curren scope chain
		/// </summary>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public string Resolve(string prefix)
		{
			if (prefix == null)
			{
				prefix = String.Empty;
			}

			// find the last scope in chain that resolves prefix
			Scope scope = this.Chain.FindLast(m => m.ContainsPrefix(prefix));

			return (scope != null) ? scope[prefix] : null;
		}

		#endregion Methods
	}
}
