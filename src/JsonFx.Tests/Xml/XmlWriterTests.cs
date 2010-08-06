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
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;
using JsonFx.Xml.Resolvers;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Xml
{
	public class XmlWriterTests
	{
		#region Constants

		private const string TraitName = "XML";
		private const string TraitValue = "Writer";

		#endregion Constants

		#region Test Types

		public class Person
		{
			public string Name { get; set; }
			public Person Father { get; set; }
			public Person Mother { get; set; }

			[XmlAttribute("theNickname")]
			public string Nickname { get; set; }

			public Person[] Children { get; set; }
		}

		[DataContract(Name = "PersonContract", Namespace = "http://schemas.contoso.com")]
		public class Person2
		{
			[DataMember(Name = "AddressMember")]
			public Address theAddress;
		}

		[DataContract(Name = "AddressContract", Namespace = "http://schemas.contoso.com")]
		public class Address
		{
			[DataMember(Name = "StreetMember")]
			public string street;
		}

		#endregion Test Types

		#region XmlSerializer Comparison Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_ComparePersonOutputToXmlSerializer_Serializes()
		{
			var input = new Person
			{
				Name = "John, Jr.",
				Nickname = "Junior",
				Father = new Person
				{
					Name = "John, Sr.",
					Nickname = "Jack"
				},
				Mother = new Person
				{
					Name = "Sally",
					Nickname = "Sal"
				}
			};

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareObjectOutputToXmlSerializer_Serializes()
		{
			var input = new
			{
				False = false,
				LeapDay = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc),
				FortyTwo = 42,
				Text = "Ordinary string"
			};

			// XmlSerializer cannot serialize Anonymous objects
			InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
				delegate()
				{
					var expected = SystemXmlWriter(input, CreateSettings(false));
				});

			//var actual = new XmlWriter().Serialize(input);

			//Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareGenericDictionaryOutputToXmlSerializer_Serializes()
		{
			var input = new Dictionary<string, object>
			{
				{ "False", false },
				{ "LeapDay", new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc) },
				{ "FortyTwo", 42},
				{ "Text", "Ordinary string" }
			};

			// XmlSerializer cannot serialize type Dictionary<string, object>
			NotSupportedException ex = Assert.Throws<NotSupportedException>(
				delegate()
				{
					var expected = SystemXmlWriter(input, CreateSettings(false));
				});

			//var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Serialize(input);

			//Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareDictionaryOutputToXmlSerializer_Serializes()
		{
			var input = new Hashtable()
			{
				{ "False", false },
				{ "LeapDay", new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc) },
				{ "FortyTwo", 42},
				{ "Text", "Ordinary string" }
			};

			// XmlSerializer cannot serialize type Hashtable
			NotSupportedException ex = Assert.Throws<NotSupportedException>(
				delegate()
				{
					var expected = SystemXmlWriter(input, CreateSettings(false));
				});

			//var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Serialize(input);

			//Assert.Equal(expected.ToString(), actual);
		}

		//[Fact(Skip="System.Xml.XmlSerializer array output is worthless.")]
		public void Write_CompareArrayListOutputToXmlSerializer_Serializes()
		{
			var input = new ArrayList
				{
					false,
					new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc),
					42,
					"Ordinary string"
				};

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		//[Fact(Skip="System.Xml.XmlSerializer array output is worthless.")]
		public void Write_CompareArrayOutputToXmlSerializer_Serializes()
		{
			var input = new object[]
				{
					false,
					new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc),
					42,
					"Ordinary string"
				};

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareStringOutputToXmlSerializer_Serializes()
		{
			var input = "Ordinary string";

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareDateTimeOutputToXmlSerializer_Serializes()
		{
			var input = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc);

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareInt16OutputToXmlSerializer_Serializes()
		{
			var input = (short)42;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareUInt16OutputToXmlSerializer_Serializes()
		{
			var input = UInt16.MaxValue;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareInt32OutputToXmlSerializer_Serializes()
		{
			var input = 42;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareUInt32OutputToXmlSerializer_Serializes()
		{
			var input = UInt32.MaxValue;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareInt64OutputToXmlSerializer_Serializes()
		{
			var input = Int64.MaxValue;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareUInt64OutputToXmlSerializer_Serializes()
		{
			var input = UInt64.MaxValue;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareDecimalOutputToXmlSerializer_Serializes()
		{
			var input = Decimal.MaxValue;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareDoubleOutputToXmlSerializer_Serializes()
		{
			var input = 3.14;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareSingleOutputToXmlSerializer_Serializes()
		{
			var input = 3.14f;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareByteOutputToXmlSerializer_Serializes()
		{
			var input = (byte)0xF;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Write_CompareSByteOutputToXmlSerializer_Serializes()
		{
			var input = SByte.MaxValue;

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		//[Fact(Skip="System.Xml.XmlSerializer outputs the ordinal value of chars.")]
		public void Write_CompareCharOutputToXmlSerializer_Serializes()
		{
			var input = 'a';

			var expected = SystemXmlWriter(input, CreateSettings(false));
			var actual = new XmlWriter(new DataWriterSettings(new XmlResolverStrategy())).Write(input);

			Assert.Equal(expected.ToString(), actual);
		}

		#endregion Comparison Tests

		#region Namespace Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Format_DataContract_CorrectlySerializesNamespaces()
		{
			var input = new Person2
			{
				theAddress = new Address
				{
					street = "123 Main Street"
				}
			};

			const string expected = 
@"<PersonContract xmlns=""http://schemas.contoso.com"">
	<AddressMember>
		<StreetMember>123 Main Street</StreetMember>
	</AddressMember>
</PersonContract>";

			var writer = new XmlWriter(new DataWriterSettings(new DataContractResolverStrategy()) { PrettyPrint=true });
			var actual = writer.Write(input);

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Utility Method

		private static string SystemXmlWriter<T>(T value, System.Xml.XmlWriterSettings settings)
		{
			if (value == null)
			{
				return String.Empty;
			}

			var buffer = new System.Text.StringBuilder();
			var writer = System.Xml.XmlWriter.Create(buffer, settings);

			var namespaces = new System.Xml.Serialization.XmlSerializerNamespaces();
			namespaces.Add(String.Empty, String.Empty);// tricks the serializer into not emitting default xmlns attributes

			// serialize feed
			var serializer = new System.Xml.Serialization.XmlSerializer(value.GetType());
			serializer.Serialize(writer, value, namespaces);

			return buffer.ToString();
		}

		private static System.Xml.XmlWriterSettings CreateSettings(bool prettyPrint)
		{
			// setup document formatting
			var settings = new System.Xml.XmlWriterSettings();
			settings.CheckCharacters = true;
			settings.CloseOutput = false;
			settings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
			settings.Encoding = Encoding.UTF8;
			settings.OmitXmlDeclaration = true;

			if (prettyPrint)
			{
				// make human readable
				settings.Indent = true;
				settings.IndentChars = "\t";
			}
			else
			{
				// compact
				settings.Indent = false;
				settings.NewLineChars = String.Empty;
			}
			settings.NewLineHandling = System.Xml.NewLineHandling.Replace;

			return settings;
		}

		#endregion Utility Method
	}
}
