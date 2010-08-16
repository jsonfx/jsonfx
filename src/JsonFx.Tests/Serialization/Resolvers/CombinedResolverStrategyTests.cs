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
using System.Linq;

using JsonFx.Model;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Serialization.Resolvers
{
	public class CombinedResolverStrategyTests
	{
		#region Constants

		private const string TraitName = "Resolvers";
		private const string TraitValue = "CombinedResolverStrategy";

		#endregion Constants

		#region Test Types

		class NamingTest
		{
			#region Properties

			public string Little_BITOfEverything123456789MixedIn
			{
				get;
				set;
			}

			#endregion Properties
		}

		#endregion Test Types

		#region Combo Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetName_MultipleConventions_ReturnsListDataNames()
		{
			var input = typeof(NamingTest).GetProperty("Little_BITOfEverything123456789MixedIn");
			Assert.NotNull(input);

			var expected = new[]
			{
				new DataName("Little BIT Of Everything 123456789 Mixed In"),
				new DataName("LittleBitOfEverything123456789MixedIn"),
				new DataName("littleBitOfEverything123456789MixedIn"),
				new DataName("little-bit-of-everything-123456789-mixed-in"),
				new DataName("LITTLE_BIT_OF_EVERYTHING_123456789_MIXED_IN")
			};

			var resolver = new CombinedResolverStrategy(
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.NoChange, " "),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.PascalCase),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.CamelCase),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-"),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Uppercase, "_"));
			var actual = resolver.GetName(input);

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultipleConventions_ReturnsSingleDataName()
		{
			var input = new NamingTest
			{
				Little_BITOfEverything123456789MixedIn = "Foo."
			};

			var expected = new[]
			{
				ModelGrammar.TokenObjectBegin("Naming Test"),
				ModelGrammar.TokenProperty("Little BIT Of Everything 123456789 Mixed In"),
				ModelGrammar.TokenPrimitive("Foo."),
				ModelGrammar.TokenObjectEnd
			};

			var resolver = new CombinedResolverStrategy(
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.NoChange, " "),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.PascalCase),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.CamelCase),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-"),
				new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Uppercase, "_"));

			var actual = new ModelWalker(new DataWriterSettings(resolver)).GetTokens(input).ToArray();

			Assert.Equal(expected, actual, false);
		}

		#endregion Combo Tests
	}
}
