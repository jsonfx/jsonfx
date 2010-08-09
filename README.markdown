# [JsonFx][1]- JSON Serialization Framework (v2)
# Distributed under the terms of an [MIT-style license][2]

### Compatible Runtimes:
- .NET Framework 2.0, 3.0, 3.5, and 4.0
- Silverlight 3.0, 4.0
- Windows Phone 7
- Mono Framework 2.6

### Serialization Features:
- Unified interface for reading / writing [JSON][3], [BSON][4], XML, [JsonML][5]
- Implements true LINQ-to-JSON (not simply LINQ-to-Objects over JSON types)
- Naturally deserializes to standard CLR types, not JSON/XML-specific types
- Supports reading/writing POCO classes
- Supports reading/writing using DataContract, XmlSerialization, JsonName, attributes
- Supports reading/writing using convention-based property renaming
- Supports reading/writing C# 4.0 dynamic types
- Supports reading/writing C# 3.0 Anonymous objects
- Supports reading/writing LINQ queries
- Supports custom reading/writing extensions & name resolution strategies
- Dependency-injection-friendly for extremely flexible custom configurations
- Stream-based serialization for reading/writing right off the wire
- Provider allows automatic selection of serializer from Content-Type and Accept-Types HTTP headers

### Basic Examples:

#### Serialize to/from dynamic types (default for .NET 4.0):
	var reader = new JsonReader(); var writer = new JsonWriter();

	string input = @"{ ""foo"": true, ""array"": [ 42, false, ""Hello!"", null ] }";
	dynamic output = reader.Read(input);
	Console.WriteLine(output.array[0]); // 42
	string json = writer.Write(output);
	Console.WriteLine(json); // {"foo":true,"array":[42,false,"Hello!",null]}

#### Serialize to/from standard CLR types (default for .NET 2.0/3.5):
	string input = @"{ ""first"": ""Foo"", ""last"": ""Bar"" }";
	var output = reader.Read<Dictionary<string, object>>(input);
	Console.WriteLine(output["first"]); // Foo
	output["middle"] = "Blah";
	string json = writer.Write(output);
	Console.WriteLine(json); // {"first":"Foo","last":"Bar","middle":"Blah"}

#### Serialze to/from Anonymous types
	string input = @"{ ""first"": ""Foo"", ""last"": ""Bar"" }";
	var template = new { first=String.Empty, middle=String.Empty, last=String.Empty };
	var output = reader.Read(input, template);
	Console.WriteLine(output.first); // Foo
	output = new { output.first, middle="Blah", output.last };
	string json = writer.Write(output);
	Console.WriteLine(json); // {"first":"Foo","middle":"Blah","last":"Bar"}

#### Serialze to/from custom types and LINQ queries

	[DataContract]
	public class Person
	{
		[DataMember(Name="id")] public long PersonID { get; set; }
		[DataMember(Name="first")] public string FirstName { get; set; }
		[DataMember(Name="last")] public string LastName { get; set; }
	}

	// respect DataContracts on the way in
	var reader = new JsonReader(new DataReaderSettings(new DataContractResolverStrategy()));
	// use convention over configuration on the way out
	var writer = new JsonWriter(new DataWriterSettings(new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-")));

	string input =
	@"[
		{ ""id"": 1, ""first"": ""Foo"", ""last"": ""Bar"" },
		{ ""id"": 2, ""first"": ""etc."", ""last"": ""et al."" },
		{ ""id"": 3, ""first"": ""Blah"", ""last"": ""Yada"" }
	]";

	var people = reader.Query<Person>(input);
	var query =
		from person in people.ArrayItems()
		where person.PersonID == 1 || person.FirstName == "Blah"
		orderby person.PersonID
		select person;

	Console.WriteLine(query.Last().LastName); // Yada
	string json = writer.Write(query);
	Console.WriteLine(json); // [{"person-id":1,"first-name":"Foo","last-name":"Bar"},{"person-id":3,"first-name":"Blah","last-name":"Yada"}]

#### Fully customizable name resolution strategies

	// accept all variations! list in order of priority
	var resolver = new CombinedResolverStrategy(
		new JsonResolverStrategy(),   															// simple JSON attributes
		new DataContractResolverStrategy(),   													// DataContract attributes
		new XmlResolverStrategy(),   															// XmlSerializer attributes
		new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.PascalCase),		// DotNetStyle
		new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.CamelCase),		// jsonStyle
		new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-"),	// xml-style
		new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Uppercase, "_"));	// CONST_STYLE

	// pass the combined resolver strategy into the settings object
	var reader = new JsonReader(new DataReaderSettings(resolver));

	// link the settings objects to share resolver strategies and name lookup cache
	var writer = new JsonWriter(new DataWriterSettings(reader.Settings) { PrettyPrint=true });

#### Build REST services using dependency injection to configure auto-serializer selection

	// setup once for the lifespan of the application

	// POCO name resolution, share lookups among all instances
	var readerSettings = new DataReaderSettings();				
	var writerSettings = new DataWriterSettings(readerSettings);

	var jsonReader = new JsonFx.Json.JsonReader(readerSettings);
	var jsonWriter = new JsonFx.Json.JsonWriter(writerSettings);

	var xmlReader = new JsonFx.Xml.XmlReader(readerSettings);
	var xmlWriter = new JsonFx.Xml.XmlWriter(writerSettings);

	// list all the readers
	var readerProvider = new DataReaderProvider(
		jsonReader,
		xmlReader);

	// list all the writers
	var writerProvider = new DataWriterProvider(
		jsonWriter,
		xmlWriter);

	// ...later on a request comes in

	// incoming HTTP request headers
	string contentTypeHeader = myRequest.Headers[HttpRequestHeader.ContentType];
	string acceptHeader = myRequest.Headers[HttpRequestHeader.Accept];

	IDataReader deserializer = readerProvider.Find(contentTypeHeader);

	var requestData;
	using (var textReader = new StreamReader(myRequest.GetRequestStream()))
	{
		requestData = deserializer.Read(textReader);
	}
	
	// ...consume the data, generate a response
	var myResponse = ...;
	var responseData = ...;

	IDataWriter serializer = writerProvider.Find(acceptHeader, contentTypeHeader);
	using (var textWriter = new StreamWriter(myResponse.GetResponseStream()))
	{
		serializer.Write(responseData);
	}

  [1]: http://jsonfx.net
  [2]: http://jsonfx.net/license
  [3]: http://json.org
  [4]: http://bsonspec.org
  [5]: http://jsonml.org
  