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

namespace JsonFx.Serialization.Providers
{
	/// <summary>
	/// Provides lookup capabilities for finding matching IDataReader
	/// </summary>
	public class DataReaderProvider : IDataReaderProvider
	{
		#region Fields

		private readonly IDictionary<string, IDataReader> ReadersByMime = new Dictionary<string, IDataReader>(StringComparer.OrdinalIgnoreCase);

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="readers">inject with all possible readers</param>
		public DataReaderProvider(IEnumerable<IDataReader> readers)
		{
			if (readers != null)
			{
				foreach (IDataReader reader in readers)
				{
					foreach (string contentType in reader.ContentType)
					{
						if (String.IsNullOrEmpty(contentType) ||
							this.ReadersByMime.ContainsKey(contentType))
						{
							continue;
						}

						this.ReadersByMime[contentType] = reader;
					}
				}
			}
		}

		#endregion Init

		#region Methods

		/// <summary>
		/// Finds an IDataReader by content-type header
		/// </summary>
		/// <param name="contentTypeHeader"></param>
		/// <returns></returns>
		public virtual IDataReader Find(string contentTypeHeader)
		{
			// TODO: implement this negotiation
			// http://jsr311.java.net/nonav/releases/1.1/spec/spec3.html#x3-380003.8

			string type = DataProviderUtility.ParseMediaType(contentTypeHeader);

			IDataReader reader;
			if (this.ReadersByMime.TryGetValue(type, out reader))
			{
				return reader;
			}

			return null;
		}

		#endregion Methods
	}
}
