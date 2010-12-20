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

using JsonFx.Model;
using JsonFx.Serialization;

namespace JsonFx.Json
{
	/// <summary>
	/// JSON serializer
	/// </summary>
	public partial class JsonWriter : ModelWriter
	{
		#region Fields

		private readonly string[] ContentTypes;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public JsonWriter()
			: base(new DataWriterSettings())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public JsonWriter(DataWriterSettings settings)
			: base(settings != null ? settings : new DataWriterSettings())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="contentTypes"></param>
		public JsonWriter(DataWriterSettings settings, params string[] contentTypes)
			: base(settings != null ? settings : new DataWriterSettings())
		{
			if (contentTypes == null)
			{
				throw new NullReferenceException("contentTypes");
			}

			// copy values so cannot be modified from outside
			this.ContentTypes = new string[contentTypes.Length];
			Array.Copy(contentTypes, this.ContentTypes, contentTypes.Length);
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the supported content type for the serialized data
		/// </summary>
		public override IEnumerable<string> ContentType
		{
			get
			{
				if (this.ContentTypes != null)
				{
					foreach (string contentType in this.ContentTypes)
					{
						yield return contentType;
					}
					yield break;
				}

				yield return "application/json";
				yield return "text/json";
				yield return "text/x-json";
			}
		}

		/// <summary>
		/// Gets the supported file extension for the serialized data
		/// </summary>
		public override IEnumerable<string> FileExtension
		{
			get { yield return ".json"; }
		}

		#endregion Properties

		#region DataWriter<DataTokenType> Methods

		/// <summary>
		/// Gets the formatter for JSON
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override ITextFormatter<ModelTokenType> GetFormatter()
		{
			return new JsonWriter.JsonFormatter(this.Settings);
		}

		#endregion DataWriter<DataTokenType> Methods
	}
}
