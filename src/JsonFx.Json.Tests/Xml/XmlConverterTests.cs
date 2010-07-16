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
using System.Text;

using JsonFx.Json;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Xml
{
	public class XmlConverterTests
	{
		#region Round Trip Tests

		[Fact]
		public void ConvertJson2Xml_HelloWorld_RoundTripsJsonToXmlAndBack()
		{
			// input from example at http://xmlspec.org/#/specification
			var inputJson =
@"{
	""hello"" : ""world""
}";

			var expectedXml = @"<object><hello>world</hello></object>";

			var expectedJson = @"{""hello"":""world""}";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputJson);

			var xmlFormatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint = false });
			var actualXml = xmlFormatter.Format(tokens1);

			Assert.Equal(expectedXml, actualXml);

			// TODO: enable when ready
			//var xmlTokenizer = new XmlReader.XmlTokenizer();
			//var tokens2 = xmlTokenizer.GetTokens(actualXml);

			//var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = false });
			//var actualJson = jsonFormatter.Format(tokens2);

			//Assert.Equal(expectedJson, actualJson);
		}

		[Fact]
		public void ConvertJson2Xml_ArrayAsProperty_RoundTripsJsonToXmlAndBack()
		{
			// input from example at http://xmlspec.org/#/specification
			var inputJson =
@"{
	""BSON"" : [
		""awesome"",
		5.05,
		1986
	]
}";

			var expectedXml = @"<object><BSON><string>awesome</string><double>5.05</double><int>1986</int></BSON></object>";

			var expectedJson = @"{""BSON"":[""awesome"",5.05,1986]}";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputJson);

			var xmlFormatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint = false });
			var actualXml = xmlFormatter.Format(tokens1);

			Assert.Equal(expectedXml, actualXml);

			// TODO: enable when ready
			//var xmlTokenizer = new XmlReader.XmlTokenizer();
			//var tokens2 = xmlTokenizer.GetTokens(actualXml);

			//var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = false });
			//var actualJson = jsonFormatter.Format(tokens2);

			//Assert.Equal(expectedJson, actualJson);
		}

		[Fact]
		public void ConvertJson2Xml_BooleanValue_RoundTripsJsonToXmlAndBack()
		{
			// input from example at http://codebetter.com/blogs/karlseguin/archive/2010/03/05/xml-serialization.aspx
			var inputJson = @"{valid:true}";

			var expectedJson =
@"{
	""valid"" : true
}";
			var expectedXml =
@"<object>
	<valid>true</valid>
</object>";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputJson);

			var xmlFormatter = new XmlWriter.XmlFormatter(new DataWriterSettings { PrettyPrint = true });
			var actualXml = xmlFormatter.Format(tokens1);

			Assert.Equal(expectedXml, actualXml);

			// TODO: enable when ready
			//var xmlTokenizer = new XmlReader.XmlTokenizer();
			//var tokens2 = xmlTokenizer.GetTokens(actualXml);

			//var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = true });
			//var actualJson = jsonFormatter.Format(tokens2);

			//Assert.Equal(expectedJson, actualJson);
		}

		#endregion Round Trip Tests
	}
}
