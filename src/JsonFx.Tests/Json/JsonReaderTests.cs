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
using System.IO;

using JsonFx.Model.Filters;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;
using System.Net;

namespace JsonFx.Json
{
	public class JsonReaderTests
	{
		#region Constants

		private const string TraitName = "JSON";
		private const string TraitValue = "Reader";

		#endregion Constants

		#region Array Tests

		public class Foo2
		{
			public List<Bar2> Bars { get; set; }

			public override bool Equals(object obj)
			{
				Foo2 that = obj as Foo2;
				if (that == null)
				{
					return false;
				}

				if (this.Bars == null ? that.Bars != null : that.Bars == null)
				{
					return false;
				}

				if (this.Bars != null)
				{
					int length = this.Bars.Count;
					if (length != that.Bars.Count)
					{
						return false;
					}
					for (int i=0; i<length; i++)
					{
						Bar2 thisItem = this.Bars[i];
						Bar2 thatItem = that.Bars[i];
						if (thisItem == null ? thatItem != null : thatItem == null)
						{
							return false;
						}
						if (thisItem != null && !thisItem.Equals(thatItem))
						{
							return false;
						}
					}
				}

				return true;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}

		public class Bar2
		{
			public string Baz { get; set; }

			public override bool Equals(object obj)
			{
				Bar2 that = obj as Bar2;
				if (that == null)
				{
					return false;
				}

				return String.Equals(this.Baz, that.Baz);
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_NestedList_DeserializesList()
		{
			var input = @"{""Bars"":[{""Baz"":""Test""}]}";
			var expected = new Foo2
				{
					Bars = new List<Bar2>
					{
						new Bar2 { Baz="Test"}
					}
				};

			var actual = new JsonReader().Read<Foo2>(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_StringArrayDefaults_DeserializesArray()
		{
			var input = @"[""aaa"", ""bbb""]";
			var expected = new string[]
				{
					"aaa",
					"bbb"
				};

			var actual = new JsonReader().Read(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_OnlyDefaults_DeserializesIso8601DateTimeFormat()
		{
			var input = @"[ ""Normal string before"", ""2008-02-29T23:59:59.999Z"", ""2010-07-05T10:51:17.768"", ""Normal string after""]";
			var expected = new object[]
				{
					"Normal string before",
					new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc),
					new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Unspecified),
					"Normal string after"
				};

			var actual = new JsonReader().Read(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_RecognizesFilters_DeserializesMultipleDateTimeFormats()
		{
			var input = @"[ ""Normal string before"", ""2008-02-29T23:59:59.999Z"", ""/Date(1278327077768)/"", ""Normal string after""]";
			var expected = new object[]
		        {
		            "Normal string before",
		            new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc),
		            new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Utc),
		            "Normal string after"
		        };

			var reader = new JsonReader(
				new DataReaderSettings(
					new Iso8601DateFilter { Format=Iso8601DateFilter.Precision.Ticks },
					new MSAjaxDateFilter()) { AllowTrailingContent = true });

			var actual = reader.Read(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_AllowTrailingContent_IgnoresTrailingContent()
		{
			var input = new StringReader(@"[""Content embedded inside other structure"", true,null, 42]</xml>");
			var expected = new object[]
				{
					"Content embedded inside other structure",
					true,
					null,
					42
				};

			var reader = new JsonReader(new DataReaderSettings { AllowTrailingContent = true });

			var actual = reader.Read(input);

			Assert.Equal(expected, actual);

			// Didn't consume remaining content
			Assert.Equal("</xml>", input.ReadToEnd());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_ArrayCommaAfterClose_ThrowsDeserializationException()
		{
			// input from fail7.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""Comma after the close""],";

			var reader = new JsonReader(new DataReaderSettings { AllowTrailingContent=false });

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = reader.Read(input);
				});

			// verify exception is coming from expected position
			Assert.Equal(25L, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_ArrayExtraClose_ThrowsDeserializationException()
		{
			// input from fail8.json in test suite at http://www.json.org/JSON_checker/
			var input = @"[""Extra close""]]";

			var reader = new JsonReader(new DataReaderSettings { AllowTrailingContent=false });

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = reader.Read(input);
				});

			// verify exception is coming from expected position
			Assert.Equal(15L, ex.Index);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_TimeSpans_DeserializesCorrectly()
		{
			var input = @"[ ""-9223372036854775808"", 0, 316223999990000, ""9223372036854775807""]";
			var expected = new TimeSpan[]
				{
					TimeSpan.MinValue,
					new TimeSpan(),
					new TimeSpan(365, 23, 59, 59, 999),
					TimeSpan.MaxValue
				};

			var actual = new JsonReader().Read<TimeSpan[]>(input);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_ObjectExtraValueAfterClose_ThrowsDeserializationException()
		{
			// input from fail10.json in test suite at http://www.json.org/JSON_checker/
			var input = @"{""Extra value after close"": true} ""misplaced quoted value""";

			var reader = new JsonReader(new DataReaderSettings { AllowTrailingContent=false });

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate
				{
					var actual = reader.Read(input);
				});

			// verify exception is coming from expected position
			// note the reader doesn't see the 2nd object until it is read
			// so the index is after the trailing value
			Assert.Equal(57L, ex.Index);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void ReadMany_ObjectExtraValueAfterClose_DeserializesStreamOfObject()
		{
			// input from fail10.json in test suite at http://www.json.org/JSON_checker/
			var input = new StringReader(@"{""Extra value after close"": true} ""misplaced quoted value""");

			var reader = new JsonReader(new DataReaderSettings());

			var enumerator = reader.ReadMany(input).GetEnumerator();

			Assert.True(enumerator.MoveNext());
			Assert.Equal(new Dictionary<string, object>
				{
					{ "Extra value after close", true }
				},
				enumerator.Current,
				false);

			Assert.True(enumerator.MoveNext());
			Assert.Equal(
				"misplaced quoted value",
				enumerator.Current);

			Assert.False(enumerator.MoveNext());
		}

		[Fact(Skip="Brittle test requires network access and external API")]
		[Trait(TraitName, TraitValue)]
		public void Read_DeserializesStreamOfObject()
		{
			using (var stream = WebRequest.Create("https://api.twitter.com/1/statuses/user_timeline.json?screen_name=jsonfx&count=2")
				.GetResponse()
				.GetResponseStream())
			{
				TextReader input = new StreamReader(stream);

				JsonReader reader = new JsonReader(new DataReaderSettings());

				dynamic actual = reader.Read(input);

				Assert.Equal(2, actual.Length);
			}
		}

		#endregion Object Tests

		#region Enum Tests

		public class Foo
		{
			public Bar Baz { get; set; }

			public override bool Equals(object obj)
			{
				Foo that = obj as Foo;
				if (that == null)
				{
					return false;
				}

				return this.Baz.Equals(that.Baz);
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}

		public enum Bar
		{
			First,
			Second,
			Third
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Read_ObjectContainingEnum_DeserializesObject()
		{
			var input = "{\"Baz\":\"First\"}";
			var expected = new Foo { Baz = Bar.First };

			var actual = new JsonReader().Read<Foo>(input);

			Assert.Equal(expected, actual);
		}

		#endregion Enum Tests
		
		#region Comment Tests
		
		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IgnoreMutipleSingleLineComments()
		{
			var input = "//comment1\r\n//comment2\r\n//comment3\r\n{ \"Bars\": [{\"Baz\": \"Test\"}]}";
			var expected = new Foo2
			{
				Bars = new List<Bar2>
				{
					new Bar2 { Baz="Test" }
				}
			};

			var actual = new JsonReader().Read<Foo2>(input);
		
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IgnoreMutilineComments()
		{
			var input = "/*comment1\r\ncomment2\r\ncomment3*/\r\n{ \"Bars\": [{\"Baz\": \"Test\"}]}";
			var expected = new Foo2
			{
				Bars = new List<Bar2>
				{
					new Bar2 { Baz="Test"}
				}
			};

			var actual = new JsonReader().Read<Foo2>(input);
			
			Assert.Equal(expected, actual);
		}
		
		#endregion Comment Tests
	}
}
