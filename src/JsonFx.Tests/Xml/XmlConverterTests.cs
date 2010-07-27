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

using JsonFx.Common;
using JsonFx.Json;
using JsonFx.Markup;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Xml
{
	public class XmlConverterTests
	{
		#region Constants

		private const string TraitName = "XML";
		private const string TraitValue = "Converter";

		#endregion Constants

		#region Round Trip Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ConvertJson2Xml_HelloWorld_RoundTripsJsonToXmlAndBack()
		{
			// input from example at http://xmlspec.org/#/specification
			var inputJson =
@"{
	""hello"" : ""world""
}";

			var expectedXml = @"<object><hello>world</hello></object>";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputJson);

			var writerSettings = new DataWriterSettings { PrettyPrint = false };
			var xmlFormatter = new TransformFormatter<CommonTokenType, MarkupTokenType>(new XmlWriter.XmlFormatter(writerSettings), new XmlWriter.XmlOutTransformer(writerSettings));
			var actualXml = xmlFormatter.Format(tokens1);

			Assert.Equal(expectedXml, actualXml);

			var expectedJson = @"{""hello"":""world""}";

			var readerSettings = new DataReaderSettings(writerSettings.Resolver);
			var xmlTokenizer = new TransformTokenizer<MarkupTokenType, CommonTokenType>(new XmlReader.XmlTokenizer(), new XmlReader.XmlInTransformer(readerSettings));
			var tokens2 = xmlTokenizer.GetTokens(actualXml);

			var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = false });
			var actualJson = jsonFormatter.Format(tokens2);

			Assert.Equal(expectedJson, actualJson);
		}

		[Fact(Skip="Broken will be rewriting transformer shortly")]
		[Trait(TraitName, TraitValue)]
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

			var expectedXml = @"<object><BSON><item>awesome</item><item>5.05</item><item>1986</item></BSON></object>";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputJson);

			var writerSettings = new DataWriterSettings { PrettyPrint = false };
			var xmlFormatter = new TransformFormatter<CommonTokenType, MarkupTokenType>(new XmlWriter.XmlFormatter(writerSettings), new XmlWriter.XmlOutTransformer(writerSettings));
			var actualXml = xmlFormatter.Format(tokens1);

			Assert.Equal(expectedXml, actualXml);

			var expectedJson = @"{""BSON"":[""awesome"",5.05,1986]}";

			var readerSettings = new DataReaderSettings(writerSettings.Resolver);
			var xmlTokenizer = new TransformTokenizer<MarkupTokenType, CommonTokenType>(new XmlReader.XmlTokenizer(), new XmlReader.XmlInTransformer(readerSettings));
			var tokens2 = xmlTokenizer.GetTokens(actualXml);

			var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = false });
			var actualJson = jsonFormatter.Format(tokens2);

			Assert.Equal(expectedJson, actualJson);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ConvertJson2Xml_BooleanValue_RoundTripsJsonToXmlAndBack()
		{
			// input from example at http://codebetter.com/blogs/karlseguin/archive/2010/03/05/xml-serialization.aspx
			var inputJson = @"{valid:true}";

			var expectedXml =
@"<object>
	<valid>true</valid>
</object>";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputJson);

			var writerSettings = new DataWriterSettings { PrettyPrint = true };
			var xmlFormatter = new TransformFormatter<CommonTokenType, MarkupTokenType>(new XmlWriter.XmlFormatter(writerSettings), new XmlWriter.XmlOutTransformer(writerSettings));
			var actualXml = xmlFormatter.Format(tokens1);

			Assert.Equal(expectedXml, actualXml);

            var expectedJson =
@"{
	""valid"" : ""true""
}";

			var readerSettings = new DataReaderSettings(writerSettings.Resolver);
			var xmlTokenizer = new TransformTokenizer<MarkupTokenType, CommonTokenType>(new XmlReader.XmlTokenizer(), new XmlReader.XmlInTransformer(readerSettings));
			var tokens2 = xmlTokenizer.GetTokens(actualXml);

			var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = true });
			var actualJson = jsonFormatter.Format(tokens2);

			Assert.Equal(expectedJson, actualJson);
		}

		[Fact(Skip="Broken will be rewriting transformer shortly")]
		[Trait(TraitName, TraitValue)]
		public void ConvertJson2Xml_ComplexGraph_RoundTripsJsonToXmlAndBack()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			const string inputJson = @"[
    ""JSON Test Pattern pass1"",
    {""object with 1 member"":[""array with 1 element""]},
    {},
    [],
    -42,
    true,
    false,
    null,
    {
        ""integer"": 1234567890,
        ""real"": -9876.543210,
        ""e"": 0.123456789e-12,
        ""E"": 1.234567890E+34,
        """":  23456789012E66,
        ""zero"": 0,
        ""one"": 1,
        ""space"": "" "",
        ""quote"": ""\"""",
        ""backslash"": ""\\"",
        ""controls"": ""\b\f\n\r\t"",
        ""slash"": ""/ & \/"",
        ""alpha"": ""abcdefghijklmnopqrstuvwyz"",
        ""ALPHA"": ""ABCDEFGHIJKLMNOPQRSTUVWYZ"",
        ""digit"": ""0123456789"",
        ""0123456789"": ""digit"",
        ""special"": ""`1~!@#$%^&*()_+-={':[,]}|;.</>?"",
        ""hex"": ""\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"",
        ""true"": true,
        ""false"": false,
        ""null"": null,
        ""array"":[  ],
        ""object"":{  },
        ""address"": ""50 St. James Street"",
        ""url"": ""http://www.JSON.org/"",
        ""comment"": ""// /* <!-- --"",
        ""# -- --> */"": "" "",
        "" s p a c e d "" :[1,2 , 3

,

4 , 5        ,          6           ,7        ],""compact"":[1,2,3,4,5,6,7],
        ""jsontext"": ""{\""object with 1 member\"":[\""array with 1 element\""]}"",
        ""quotes"": ""&#34; \u0022 %22 0x22 034 &#x22;"",
        ""\/\\\""\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?""
: ""A key can be any string""
    },
    0.5 ,98.6
,
99.44
,

1066,
1e1,
0.1e1,
1e-1,
1e00,2e+00,2e-00
,""rosebud""]";

			var expectedXml =
@"<array>
	<item>JSON Test Pattern pass1</item>
	<item>
		<object_x0020_with_x0020_1_x0020_member>
			<item>array with 1 element</item>
		</object_x0020_with_x0020_1_x0020_member>
	</item>
	<item />
	<item />
	<item>-42</item>
	<item>true</item>
	<item>false</item>
	<item />
	<item>
		<integer>1234567890</integer>
		<real>-9876.54321</real>
		<e>1.23456789e-13</e>
		<E>1.23456789e+34</E>
		<double>2.3456789012e+76</double>
		<zero>0</zero>
		<one>1</one>
		<space> </space>
		<quote>""</quote>
		<backslash>\</backslash>
		<controls>"+"&#x8;&#xC;\n\r\t"+@"</controls>
		<slash>/ &amp; /</slash>
		<alpha>abcdefghijklmnopqrstuvwyz</alpha>
		<ALPHA>ABCDEFGHIJKLMNOPQRSTUVWYZ</ALPHA>
		<digit>0123456789</digit>
		<_x0030_123456789>digit</_x0030_123456789>
		<special>`1~!@#$%^&amp;*()_+-={':[,]}|;.&lt;/&gt;?</special>
		<hex>"+"\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A"+@"</hex>
		<true>true</true>
		<false>false</false>
		<null />
		<array />
		<object />
		<address>50 St. James Street</address>
		<url>http://www.JSON.org/</url>
		<comment>// /* &lt;!-- --</comment>
		<_x0023__x0020_--_x0020_--_x003E__x0020__x002A__x002F_> </_x0023__x0020_--_x0020_--_x003E__x0020__x002A__x002F_>
		<_x0020_s_x0020_p_x0020_a_x0020_c_x0020_e_x0020_d_x0020_>
			<item>1</item>
			<item>2</item>
			<item>3</item>
			<item>4</item>
			<item>5</item>
			<item>6</item>
			<item>7</item>
		</_x0020_s_x0020_p_x0020_a_x0020_c_x0020_e_x0020_d_x0020_>
		<compact>
			<item>1</item>
			<item>2</item>
			<item>3</item>
			<item>4</item>
			<item>5</item>
			<item>6</item>
			<item>7</item>
		</compact>
		<jsontext>{""object with 1 member"":[""array with 1 element""]}</jsontext>
		<quotes>&amp;#34; "" %22 0x22 034 &amp;#x22;</quotes>
		<"+"_x002F__x005C__x0022_\uCAFE\uBABE_xAB98__xFCDE_\uBCDA_xEF4A__x0008__x000C__x000A__x000D__x0009__x0060_1_x007E__x0021__x0040__x0023__x0024__x0025__x005E__x0026__x002A__x0028__x0029___x002B_-_x003D__x005B__x005D__x007B__x007D__x007C__x003B__x003A__x0027__x002C_._x002F__x003C__x003E__x003F_"+@">A key can be any string</"+"_x002F__x005C__x0022_\uCAFE\uBABE_xAB98__xFCDE_\uBCDA_xEF4A__x0008__x000C__x000A__x000D__x0009__x0060_1_x007E__x0021__x0040__x0023__x0024__x0025__x005E__x0026__x002A__x0028__x0029___x002B_-_x003D__x005B__x005D__x007B__x007D__x007C__x003B__x003A__x0027__x002C_._x002F__x003C__x003E__x003F_"+@">
	</item>
	<item>0.5</item>
	<item>98.6</item>
	<item>99.44</item>
	<item>1066</item>
	<item>10</item>
	<item>1</item>
	<item>0.1</item>
	<item>1</item>
	<item>2</item>
	<item>2</item>
	<item>rosebud</item>
</array>";

			var jsonTokenizer = new JsonReader.JsonTokenizer();
			var tokens1 = jsonTokenizer.GetTokens(inputJson);

			var writerSettings = new DataWriterSettings { PrettyPrint = true };
			var xmlFormatter = new TransformFormatter<CommonTokenType, MarkupTokenType>(new XmlWriter.XmlFormatter(writerSettings), new XmlWriter.XmlOutTransformer(writerSettings));
			var actualXml = xmlFormatter.Format(tokens1);

			Assert.Equal(expectedXml, actualXml);

			const string expectedJson = @"[""JSON Test Pattern pass1"",{""object with 1 member"":[""array with 1 element""]},{},[],-42,true,false,null,{""integer"":1234567890,""real"":-9876.54321,""e"":1.23456789e-13,""E"":1.23456789e+34,"""":2.3456789012e+76,""zero"":0,""one"":1,""space"":"" "",""quote"":""\"""",""backslash"":""\\"",""controls"":""\b\f\n\r\t"",""slash"":""/ & /"",""alpha"":""abcdefghijklmnopqrstuvwyz"",""ALPHA"":""ABCDEFGHIJKLMNOPQRSTUVWYZ"",""digit"":""0123456789"",""0123456789"":""digit"",""special"":""`1~!@#$%^&*()_+-={':[,]}|;.\u003C/>?"",""hex"":""\u0123\u4567\u89AB\uCDEF\uABCD\uEF4A"",""true"":true,""false"":false,""null"":null,""array"":[],""object"":{},""address"":""50 St. James Street"",""url"":""http://www.JSON.org/"",""comment"":""// /* \u003C!-- --"",""# -- --> */"":"" "","" s p a c e d "":[1,2,3,4,5,6,7],""compact"":[1,2,3,4,5,6,7],""jsontext"":""{\""object with 1 member\"":[\""array with 1 element\""]}"",""quotes"":""&#34; \"" %22 0x22 034 &#x22;"",""/\\\""\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./\u003C>?"":""A key can be any string""},0.5,98.6,99.44,1066,10,1,0.1,1,2,2,""rosebud""]";

			var readerSettings = new DataReaderSettings(writerSettings.Resolver);
			var xmlTokenizer = new TransformTokenizer<MarkupTokenType, CommonTokenType>(new XmlReader.XmlTokenizer(), new XmlReader.XmlInTransformer(readerSettings));
			var tokens2 = xmlTokenizer.GetTokens(actualXml);

			var jsonFormatter = new JsonWriter.JsonFormatter(new DataWriterSettings { PrettyPrint = false });
			var actualJson = jsonFormatter.Format(tokens2);

			Assert.Equal(expectedJson, actualJson);
		}

		#endregion Round Trip Tests
	}
}
