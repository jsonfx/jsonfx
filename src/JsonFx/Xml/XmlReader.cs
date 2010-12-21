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

namespace JsonFx.Xml
{
	/// <summary>
	/// XML serializer
	/// </summary>
	public partial class XmlReader : ModelReader
	{
		#region Fields

		private readonly string[] ContentTypes;

		#endregion Fields

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
			: base(settings != null ? settings : new DataReaderSettings())
		{
		}
		
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="contentTypes"></param>
		public XmlReader(DataReaderSettings settings, params string[] contentTypes)
			: base(settings != null ? settings : new DataReaderSettings())
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

				yield return "application/xml";
				yield return "text/xml";
			}
		}

		#endregion Properties

		#region DataReader<DataTokenType> Methods

		/// <summary>
		/// Gets a tokenizer for XML
		/// </summary>
		/// <returns></returns>
		protected override ITextTokenizer<ModelTokenType> GetTokenizer()
		{
			return new TransformTokenizer<MarkupTokenType, ModelTokenType>(new XmlTokenizer(), new XmlInTransformer(this.Settings));
		}

		#endregion DataReader<DataTokenType> Methods
	}
}
