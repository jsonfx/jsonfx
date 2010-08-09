# [JsonFx][1] v2 - JSON Serialization Framework
## Distributed under the terms of an MIT-style license

### Compatible Runtimes:
- .NET Framework 2.0, 3.0, 3.5, and 4.0
- Silverlight 3.0, 4.0
- Windows Phone 7
- Mono Framework 2.6

### Serialization Features:
- unified interface for reading / writing [JSON][2], [BSON][3], XML, [JsonML][4]
- implements true LINQ-to-JSON (not simply LINQ-to-Objects over JSON types)
- naturally deserializes to standard CLR types, not JSON/XML-specific types
- supports reading/writing POCO classes
- supports reading/writing using DataContract, XmlSerialization, JsonName, attributes
- supports reading/writing using convention-based property renaming
- supports reading/writing C# 4.0 dynamic types
- supports reading/writing C# 3.0 Anonymous objects
- supports reading/writing LINQ queries
- supports custom reading/writing extensions & name resolution strategies
- dependency-injection-friendly for extremely flexible custom configurations
- stream-based serialization for reading/writing right off the wire
- provider allows automatic selection of serializer from Content-Type and Accept-Types HTTP headers

### Basic Examples:

#### serialize to/from dynamic types (default for .NET 4.0):
	var reader = new JsonReader(); var writer = new JsonWriter();

	string input = @"{ ""foo"": true, ""array"": [ 42, false, ""Hello!"", null ] }";
	dynamic output = reader.Read(input);
	Console.WriteLine(output.array[0]); // 42
	string json = writer.Write(output);
	Console.WriteLine(json); // {"foo":true,"array":[42,false,"Hello!",null]}

#### serialize to/from standard CLR types (default for .NET 2.0/3.5):
	string input = @"{ ""first"": ""Foo"", ""last"": ""Bar"" }";
	var output = reader.Read<Dictionary<string, object>>(input);
	Console.WriteLine(output["first"]); // Foo
	output["middle"] = "Blah";
	string json = writer.Write(output);
	Console.WriteLine(json); // {"first":"Foo","last":"Bar","middle":"Blah"}

#### serialze to/from Anonymous types
	string input = @"{ ""first"": ""Foo"", ""last"": ""Bar"" }";
	var template = new { first=String.Empty, middle=String.Empty, last=String.Empty };
	var output = reader.Read(input, template);
	Console.WriteLine(output.first); // Foo
	output = new { output.first, middle="Blah", output.last };
	string json = writer.Write(output);
	Console.WriteLine(json); // {"first":"Foo","middle":"Blah","last":"Bar"}

#### serialze to/from LINQ queries

	[DataContract]
	public class Person
	{
		[DataMember(Name="id")] public long PersonID { get; set; }
		[DataMember(Name="first")] public string FirstName { get; set; }
		[DataMember(Name="last")] public string LastName { get; set; }
	}

	var reader = new JsonReader(new DataReaderSettings(new DataContractResolverStrategy()));
	var writer = new JsonWriter(new DataWriterSettings(new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.Lowercase)));

	string input = @"[ { ""id"": 1, ""first"": ""Foo"", ""last"": ""Bar"" },  { ""id"": 2, ""first"": ""etc."", ""last"": ""et al."" }, { ""id"": 3, ""first"": ""Blah"", ""last"": ""Yada"" } ]";

	var people = reader.Query<Person>(input);
	var query =
		from person in people.ArrayItems()
		where person.PersonID == 2 || person.FirstName == "Blah"
		orderby person.PersonID
		select person;

	Console.WriteLine(query.Last().LastName); // Yada
	string json = writer.Write(query);
	Console.WriteLine(json); // [{"person-id":2,"first-name":"etc.","last-name":"et al."},{"person-id":3,"first-name":"Blah","last-name":"Yada"}]

  [1]: http://jsonfx.net
  [2]: http://json.org
  [3]: http://bsonspec.org
  [4]: http://jsonml.org
  