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

namespace JsonFx.Serialization.Providers
{
	/// <summary>
	/// Provides lookup capabilities for finding an IDataWriter
	/// </summary>
	public class DataWriterProvider : IDataWriterProvider
	{
		#region Fields

		private readonly IDataWriter DefaultWriter;
		private readonly IDictionary<string, IDataWriter> WritersByExt = new Dictionary<string, IDataWriter>(StringComparer.OrdinalIgnoreCase);
		private readonly IDictionary<string, IDataWriter> WritersByMime = new Dictionary<string, IDataWriter>(StringComparer.OrdinalIgnoreCase);

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="writers">inject with all possible writers</param>
		public DataWriterProvider(IEnumerable<IDataWriter> writers)
		{
			if (writers != null)
			{
				foreach (IDataWriter writer in writers)
				{
					if (this.DefaultWriter == null)
					{
						// TODO: decide less arbitrary way to choose default
						// without hardcoding value into IDataWriter.
						// Currently first DataWriter wins default.
						this.DefaultWriter = writer;
					}

					foreach (string contentType in writer.ContentType)
					{
						if (String.IsNullOrEmpty(contentType) ||
							this.WritersByMime.ContainsKey(contentType))
						{
							continue;
						}

						this.WritersByMime[contentType] = writer;
					}

					foreach (string fileExt in writer.FileExtension)
					{
						if (String.IsNullOrEmpty(fileExt) ||
							this.WritersByExt.ContainsKey(fileExt))
						{
							continue;
						}

						string ext = DataProviderUtility.NormalizeExtension(fileExt);
						this.WritersByExt[ext] = writer;
					}
				}
			}
		}

		#endregion Init

		#region Properties

		public IDataWriter DefaultDataWriter
		{
			get { return this.DefaultWriter; }
		}

		#endregion Properties

		#region Methods

		public virtual IDataWriter Find(string extension)
		{
			extension = DataProviderUtility.NormalizeExtension(extension);

			IDataWriter writer;
			if (this.WritersByExt.TryGetValue(extension, out writer))
			{
				return writer;
			}

			return null;
		}

		public virtual IDataWriter Find(string acceptHeader, string contentTypeHeader)
		{
			// TODO: implement this negotiation
			// http://jsr311.java.net/nonav/releases/1.1/spec/spec3.html#x3-380003.8

			IDataWriter writer;
			foreach (string type in DataProviderUtility.ParseHeaders(acceptHeader, contentTypeHeader))
			{
				if (this.WritersByMime.TryGetValue(type, out writer))
				{
					return writer;
				}
			}

			return null;
		}

		#endregion Methods
	}
}
