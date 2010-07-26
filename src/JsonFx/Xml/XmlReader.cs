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

using JsonFx.Common;
using JsonFx.Markup;
using JsonFx.Serialization;
using JsonFx.Serialization.Filters;

namespace JsonFx.Xml
{
	/// <summary>
	/// XML serializer
	/// </summary>
	public partial class XmlReader : DataReader<CommonTokenType>
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public XmlReader()
			: this(new DataReaderSettings())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public XmlReader(DataReaderSettings settings)
			: base(settings, null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public XmlReader(DataReaderSettings settings, params IDataFilter<CommonTokenType>[] filters)
			: base(settings, (IEnumerable<IDataFilter<CommonTokenType>>)filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public XmlReader(DataReaderSettings settings, IEnumerable<IDataFilter<CommonTokenType>> filters)
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

		#endregion Properties

		#region DataReader<DataTokenType> Methods

		/// <summary>
		/// Gets an analyzer for XML
		/// </summary>
		/// <returns></returns>
		protected override ITokenAnalyzer<CommonTokenType> GetAnalyzer()
		{
			return new CommonAnalyzer(this.Settings, this.Filters);
		}

		/// <summary>
		/// Gets a tokenizer for XML
		/// </summary>
		/// <returns></returns>
		protected override ITextTokenizer<CommonTokenType> GetTokenizer()
		{
			return new TransformTokenizer<MarkupTokenType, CommonTokenType>(new XmlTokenizer(), new XmlInTransformer(this.Settings));
		}

		#endregion DataReader<DataTokenType> Methods
	}
}
