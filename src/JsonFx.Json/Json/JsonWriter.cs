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
using System.Text;

using JsonFx.Serialization;

namespace JsonFx.Json
{
	/// <summary>
	/// JSON serializer
	/// </summary>
	public partial class JsonWriter : DataWriter<JsonTokenType>
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public JsonWriter(DataWriterSettings settings)
			: base(settings)
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the content encoding for the serialized data
		/// </summary>
		public override Encoding ContentEncoding
		{
			get { return Encoding.UTF8; }
		}

		/// <summary>
		/// Gets the supported content type for the serialized data
		/// </summary>
		public override IEnumerable<string> ContentType
		{
			get { yield return "application/json"; }
		}

		/// <summary>
		/// Gets the supported file extension for the serialized data
		/// </summary>
		public override IEnumerable<string> FileExtension
		{
			get { yield return ".json"; }
		}

		#endregion Properties

		#region DataWriter<JsonTokenType> Methods

		/// <summary>
		/// Gets the generator for JSON
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override IDataGenerator<JsonTokenType> GetGenerator(DataWriterSettings settings)
		{
			return new JsonWriter.JsonGenerator(settings);
		}

		/// <summary>
		/// Gets the formatter for JSON
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override IDataFormatter<JsonTokenType> GetFormatter(DataWriterSettings settings)
		{
			return new JsonWriter.JsonFormatter(settings);
		}

		#endregion DataWriter<JsonTokenType> Methods
	}
}
