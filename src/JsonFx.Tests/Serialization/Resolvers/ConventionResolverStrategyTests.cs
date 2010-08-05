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

using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization.Resolvers
{
	public class ConventionResolverStrategyTests
	{
		#region Constants

		private const string TraitName = "Resolvers";
		private const string TraitValue = "ConventionResolverStrategy";

		#endregion Constants

		#region Test Types

		class NamingTest
		{
			#region Properties

			public string PascalCaseProperty
			{
				get;
				set;
			}

			public string camelCaseProperty
			{
				get;
				set;
			}

			public string lowercase
			{
				get;
				set;
			}

			public string UPPERCASE
			{
				get;
				set;
			}

			public string UILeadingAcronym
			{
				get;
				set;
			}

			public string TrailingAcronymIO
			{
				get;
				set;
			}

			public string AcronymIOMiddle
			{
				get;
				set;
			}

			public string _LeadingUnderscore
			{
				get;
				set;
			}

			public string TrailingUnderscore_
			{
				get;
				set;
			}

			public string MultiWord_Underscores
			{
				get;
				set;
			}

			public string MultiWord__DoubleUnderscores
			{
				get;
				set;
			}

			public string Numbers123456789Middle
			{
				get;
				set;
			}

			public string NumbersTrailing123456789
			{
				get;
				set;
			}

			public string Little_BITOfEverything123456789MixedIn
			{
				get;
				set;
			}

			#endregion Properties
		}

		#endregion Test Types

		#region Word Splitting Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_PascalCaseProperty_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("PascalCaseProperty");
			Assert.NotNull(input);

			var expected = new[] { new DataName("Pascal-Case-Property") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_camelCaseProperty_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("camelCaseProperty");
			Assert.NotNull(input);

			var expected = new[] { new DataName("camel-Case-Property") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_lowercase_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("lowercase");
			Assert.NotNull(input);

			var expected = new[] { new DataName("lowercase") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_UPPERCASE_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("UPPERCASE");
			Assert.NotNull(input);

			var expected = new[] { new DataName("UPPERCASE") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_UILeadingAcronym_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("UILeadingAcronym");
			Assert.NotNull(input);

			var expected = new[] { new DataName("UI-Leading-Acronym") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_TrailingAcronymIO_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("TrailingAcronymIO");
			Assert.NotNull(input);

			var expected = new[] { new DataName("Trailing-Acronym-IO") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_AcronymIOMiddle_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("AcronymIOMiddle");
			Assert.NotNull(input);

			var expected = new[] { new DataName("Acronym-IO-Middle") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_LeadingUnderscore_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("_LeadingUnderscore");
			Assert.NotNull(input);

			var expected = new[] { new DataName("Leading-Underscore") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_TrailingUnderscore_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("TrailingUnderscore_");
			Assert.NotNull(input);

			var expected = new[] { new DataName("Trailing-Underscore") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_MultiWordUnderscores_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("MultiWord_Underscores");
			Assert.NotNull(input);

			var expected = new[] { new DataName("Multi-Word-Underscores") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_MultiWordDoubleUnderscores_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("MultiWord__DoubleUnderscores");
			Assert.NotNull(input);

			var expected = new[] { new DataName("Multi-Word-Double-Underscores") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_Numbers123456789Middle_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("Numbers123456789Middle");
			Assert.NotNull(input);

			var expected = new[] { new DataName("Numbers-123456789-Middle") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_NumbersEnd123456789_ReturnsCorrectWordSplit()
		{
			var input = typeof(NamingTest).GetProperty("NumbersTrailing123456789");
			Assert.NotNull(input);

			var expected = new[] { new DataName("Numbers-Trailing-123456789") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		#endregion Word Splitting Tests

		#region Casing Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_NoChange_ReturnsCorrectCasing()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new [] { new DataName("LittleBITOfEverything123456789MixedIn") };

			var resolver = new ConventionResolverStrategy("", ConventionResolverStrategy.WordCasing.NoChange);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_PascalCase_ReturnsCorrectCasing()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new [] { new DataName("LittleBitOfEverything123456789MixedIn") };

			var resolver = new ConventionResolverStrategy("", ConventionResolverStrategy.WordCasing.PascalCase);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_CamelCase_ReturnsCorrectCasing()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new [] { new DataName("littleBitOfEverything123456789MixedIn") };

			var resolver = new ConventionResolverStrategy("", ConventionResolverStrategy.WordCasing.CamelCase);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_Lowercase_ReturnsCorrectCasing()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new [] { new DataName("littlebitofeverything123456789mixedin") };

			var resolver = new ConventionResolverStrategy("", ConventionResolverStrategy.WordCasing.Lowercase);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_Uppercase_ReturnsCorrectCasing()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new [] { new DataName("LITTLEBITOFEVERYTHING123456789MIXEDIN") };

			var resolver = new ConventionResolverStrategy("", ConventionResolverStrategy.WordCasing.Uppercase);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		#endregion Casing Tests

		#region Classic Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_XmlStyle_ReturnsCorrectCasing()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new[] { new DataName("little-bit-of-everything-123456789-mixed-in") };

			var resolver = new ConventionResolverStrategy("-", ConventionResolverStrategy.WordCasing.Lowercase);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_JavaStyle_ReturnsCorrectCasing()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new[] { new DataName("littleBitOfEverything123456789MixedIn") };

			var resolver = new ConventionResolverStrategy("", ConventionResolverStrategy.WordCasing.CamelCase);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_DotNetPropertyStyle_ReturnsCorrectCasing()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new[] { new DataName("LittleBitOfEverything123456789MixedIn") };

			var resolver = new ConventionResolverStrategy("", ConventionResolverStrategy.WordCasing.PascalCase);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_KnrCConstantStyle_ReturnsCorrectCasing()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new[] { new DataName("LITTLE_BIT_OF_EVERYTHING_123456789_MIXED_IN") };

			var resolver = new ConventionResolverStrategy("_", ConventionResolverStrategy.WordCasing.Uppercase);
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		#endregion Classic Tests
	}
}
