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

using JsonFx.Serialization;

#if SILVERLIGHT
using CanonicalList=System.Collections.Generic.Dictionary<string, string>;
#else
using CanonicalList=System.Collections.Generic.SortedList<string, string>;
#endif

namespace JsonFx.Markup
{
	/// <summary>
	/// Maintains scope chain for namespace prefix mappings
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

			private CanonicalList prefixes;

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
						this.prefixes = new CanonicalList(StringComparer.Ordinal);
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
			/// Returns if this scope boundary contains a mapping for a particular namespace
			/// </summary>
			/// <param name="prefix"></param>
			/// <returns>if this scope boundary contains a mapping for a particular namespace</returns>
			public bool ContainsNamespace(string namespaceUri)
			{
				return (this.prefixes != null) && (this.prefixes.ContainsValue(namespaceUri));
			}

			/// <summary>
			/// Returns if this scope boundary contains a mapping for a particular prefix
			/// setting the namespace URI if one was found.
			/// </summary>
			/// <param name="prefix"></param>
			/// <param name="namespaceUri">the resolved namespace URI</param>
			/// <returns>if this scope boundary contains a mapping for a particular prefix</returns>
			public bool TryGetNamespace(string prefix, out string namespaceUri)
			{
				namespaceUri = null;

				return (this.prefixes != null) && (this.prefixes.TryGetValue(prefix, out namespaceUri));
			}

			/// <summary>
			/// Returns if this scope boundary contains a mapping for a particular prefix
			/// setting the prefix if one was found.
			/// </summary>
			/// <param name="namespaceUri"></param>
			/// <param name="prefix">the resolved prefix</param>
			/// <returns>if this scope boundary contains a mapping for a particular prefix</returns>
			public bool TryGetPrefix(string namespaceUri, out string prefix)
			{
				prefix = null;
				if (this.prefixes == null)
				{
					return false;
				}

#if SILVERLIGHT
				if (!this.prefixes.ContainsValue(namespaceUri))
				{
					return false;
				}

				foreach (var pair in this.prefixes)
				{
					if (pair.Value == namespaceUri)
					{
						prefix = pair.Key;
						break;
					}
				}
#else
				int index = this.prefixes.IndexOfValue(namespaceUri);
				if (index < 0)
				{
					return false;
				}

				prefix = this.prefixes.Keys[index];
#endif
				return true;
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
		private int nsCounter;

		#endregion Fields

		#region Properties

		public int Count
		{
			get { return this.Chain.Count; }
		}

		public bool HasScope
		{
			get { return (this.Chain.Count > 0); }
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

		public bool ContainsPrefix(string prefix)
		{
#if SILVERLIGHT
			foreach (var item in this.Chain)
			{
				if (item.ContainsPrefix(prefix))
				{
					return true;
				}
			}
			return false;
#else
			// internal find is more efficient
			return (this.Chain.FindLastIndex(item => item.ContainsPrefix(prefix)) >= 0);
#endif
		}

		public bool ContainsNamespace(string namespaceUri)
		{
#if SILVERLIGHT
			foreach (var item in this.Chain)
			{
				if (item.ContainsNamespace(namespaceUri))
				{
					return true;
				}
			}
			return false;
#else
			// internal find is more efficient
			return (this.Chain.FindLastIndex(item => item.ContainsNamespace(namespaceUri)) >= 0);
#endif
		}

		/// <summary>
		/// Finds the namespace URI for a given prefix within the curren scope chain
		/// </summary>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public string GetNamespace(string prefix, bool throwOnUndeclared)
		{
			if (prefix == null)
			{
				prefix = String.Empty;
			}

			// find the last scope in chain that resolves prefix
#if SILVERLIGHT
			Scope scope = null;
			for (int i=this.Chain.Count-1; i >= 0; i--)
			{
				var item = this.Chain[i];
				if (item.ContainsPrefix(prefix))
				{
					scope = item;
					break;
				}
			}
#else
			// internal find is more efficient
			Scope scope = this.Chain.FindLast(item => item.ContainsPrefix(prefix));
#endif
			if (scope == null &&
				!String.IsNullOrEmpty(prefix))
			{
				if (throwOnUndeclared)
				{
					throw new InvalidOperationException(
						String.Format("Unknown scope prefix ({0})", prefix));
				}

#if SILVERLIGHT
				foreach (var item in this.Chain)
				{
					if (item.ContainsPrefix(String.Empty))
					{
						scope = item;
						break;
					}
				}
#else
				// internal find is more efficient
				scope = this.Chain.FindLast(item => item.ContainsPrefix(String.Empty));
#endif
			}

			string namespaceUri;
			if (scope != null &&
				scope.TryGetNamespace(prefix, out namespaceUri))
			{
				return namespaceUri;
			}

			return null;
		}

		/// <summary>
		/// Finds the prefix for a given namespace URI within the curren scope chain
		/// </summary>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public string GetPrefix(string namespaceUri, bool throwOnUndeclared)
		{
			if (namespaceUri == null)
			{
				namespaceUri = String.Empty;
			}

			// find the last scope in chain that resolves prefix
#if SILVERLIGHT
			Scope scope = null;
			for (int i=this.Chain.Count-1; i>=0; i--)
			{
				var item = this.Chain[i];
				if (item.ContainsNamespace(namespaceUri))
				{
					scope = item;
					break;
				}
			}
#else
			// internal find is more efficient
			Scope scope = this.Chain.FindLast(item => item.ContainsNamespace(namespaceUri));
#endif
			if (scope == null &&
				!String.IsNullOrEmpty(namespaceUri))
			{
				if (throwOnUndeclared)
				{
					throw new InvalidOperationException(
						String.Format("Unknown scope prefix ({0})", namespaceUri));
				}

#if SILVERLIGHT
				foreach (var item in this.Chain)
				{
					if (item.ContainsNamespace(String.Empty))
					{
						scope = item;
						break;
					}
				}
#else
				// internal find is more efficient
				scope = this.Chain.FindLast(item => item.ContainsNamespace(String.Empty));
#endif
			}

			string prefix;
			if (scope != null &&
				scope.TryGetPrefix(namespaceUri, out prefix))
			{
				return prefix;
			}

			return null;
		}

		/// <summary>
		/// Checks if the matching begin tag exists on the stack
		/// </summary>
		/// <returns></returns>
		public bool ContainsTag(DataName closeTag)
		{
#if SILVERLIGHT
			foreach (var item in this.Chain)
			{
				if (item.TagName == closeTag)
				{
					return true;
				}
			}
			return false;
#else
			// internal find is more efficient
			int index = this.Chain.FindLastIndex(item => item.TagName == closeTag);
			return (index >= 0);
#endif
		}

		/// <summary>
		/// Resets the internal state of the scope chain.
		/// </summary>
		public void Clear()
		{
			this.Chain.Clear();
		}

		#endregion Methods

		#region Utility Methods

		/// <summary>
		/// Looks up the prefix for the given namespace
		/// </summary>
		/// <param name="preferredPrefix"></param>
		/// <param name="namespaceUri"></param>
		/// <returns>null if namespace is empty and no default prefix found</returns>
		public string EnsurePrefix(string preferredPrefix, string namespaceUri)
		{
			string storedPrefix = this.GetPrefix(namespaceUri, false);
			if (storedPrefix == null && !String.IsNullOrEmpty(namespaceUri))
			{
				// TODO: either add to top scope or notify caller that prefix wasn't stored

				if (String.IsNullOrEmpty(preferredPrefix))
				{
					storedPrefix = this.GeneratePrefix(namespaceUri);
				}
				else
				{
					storedPrefix = preferredPrefix;
				}
			}

			return storedPrefix;
		}

		public string GeneratePrefix(string namespaceUri)
		{
			string prefix = DataName.GetStandardPrefix(namespaceUri);

			// TODO: establish more aesthetically pleasing prefixes
			return prefix ?? String.Concat('q', ++nsCounter);
		}

		#endregion Utility Methods
	}
}
