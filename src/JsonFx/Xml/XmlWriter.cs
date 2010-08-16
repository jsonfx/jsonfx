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

using JsonFx.Markup;
using JsonFx.Model;
using JsonFx.Serialization;
using JsonFx.Serialization.Filters;

namespace JsonFx.Xml
{
	/// <summary>
	/// XML serializer
	/// </summary>
	public partial class XmlWriter : ModelWriter
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public XmlWriter()
			: this(new DataWriterSettings())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public XmlWriter(DataWriterSettings settings)
			: base(settings, null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public XmlWriter(DataWriterSettings settings, params IDataFilter<ModelTokenType>[] filters)
			: base(settings, (IEnumerable<IDataFilter<ModelTokenType>>)filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public XmlWriter(DataWriterSettings settings, IEnumerable<IDataFilter<ModelTokenType>> filters)
			: base(settings, filters)
		{
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
				yield return "application/xml";
				yield return "application/xml+xhtml";
				yield return "text/xml";
				yield return "text/html";
			}
		}

		/// <summary>
		/// Gets the supported file extension for the serialized data
		/// </summary>
		public override IEnumerable<string> FileExtension
		{
			get { yield return ".xml"; }
		}

		#endregion Properties

		#region DataWriter<DataTokenType> Methods

		/// <summary>
		/// Gets the formatter for XML
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override ITextFormatter<ModelTokenType> GetFormatter()
		{
			return new TransformFormatter<ModelTokenType, MarkupTokenType>(new XmlFormatter(this.Settings), new XmlOutTransformer(this.Settings));
		}

		#endregion DataWriter<DataTokenType> Methods
	}
}
