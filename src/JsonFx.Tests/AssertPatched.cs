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
using System.Reflection;

using Xunit;
using Xunit.Sdk;

namespace JsonFx
{
	public class AssertPatchedTests
	{
		#region Constants

		private const string TraitName = "Utilities";
		private const string TraitValue = "AssertPatched";

		#endregion Constants

		#region AssertPatched Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Assert_ExactlyEqualNestedArrays_ThrowsEqualException()
		{
			// this works as of v1.6.1
			Assert.Equal(
				new[] { new[] { "Foo" }, new[] { "Bar" } },
				new[] { new[] { "Foo" }, new[] { "Bar" } });
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void AssertPatched_ExactlyEqualNestedArrays_Passes()
		{
			AssertPatched.Equal(
				new[] { new[] { "Foo" }, new[] { "Bar" } },
				new[] { new[] { "Foo" }, new[] { "Bar" } });
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Assert_EquivalentNestedArrays_ThrowsEqualException()
		{
			Assert.Throws<EqualException>(
				delegate()
				{
					Assert.Equal(
						new[] { new string[] { "Foo" }, new string[] { "Bar" } },
						new[] { new object[] { "Foo" }, new object[] { "Bar" } });
				});
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void AssertPatched_EquivalentStrictNestedArrays_ThrowsEqualException()
		{
			Assert.Throws<EqualException>(
				delegate()
				{
					AssertPatched.Equal(
						new[] { new string[] { "Foo" }, new string[] { "Bar" } },
						new[] { new object[] { "Foo" }, new object[] { "Bar" } },
						true);
				});
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void AssertPatched_EquivalentNotStrictNestedArrays_Passes()
		{
			AssertPatched.Equal(
				new[] { new string[] { "Foo" }, new string[] { "Bar" } },
				new[] { new object[] { "Foo" }, new object[] { "Bar" } },
				false);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void AssertPatched_EquivalentStrictDictionaries_ThrowsEqualException()
		{
			var x = new Dictionary<string, object>
			{
				{ "Key", "Value" }
			};

			dynamic y = new System.Dynamic.ExpandoObject();
			y.Key = "Value";

			Assert.Throws<EqualException>(
				delegate()
				{
					AssertPatched.Equal<IDictionary<string, object>>(x, y, true);
				});
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void AssertPatched_EquivalentNotStrictDictionaries_Passes()
		{
			var x = new Dictionary<string, object>
			{
				{ "Key", "Value" }
			};

			dynamic y = new System.Dynamic.ExpandoObject();
			y.Key = "Value";

			AssertPatched.Equal<IDictionary<string, object>>(x, y, false);
		}

		#endregion AssertPatched Tests
	}

	/// <summary>
	/// Patches xunit.Assert to allow less strict comparisons (doesn't enforce exact type)
	/// </summary>
	internal class AssertPatched : Assert
	{
		#region AssertEqualityComparer<T> Entry Points

		/// <summary>
		/// Verifies that a collection contains a given object.
		/// </summary>
		/// <typeparam name="T">The type of the object to be verified</typeparam>
		/// <param name="expected">The object expected to be in the collection</param>
		/// <param name="collection">The collection to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the object is not present in the collection</exception>
		public new static void Contains<T>(T expected, IEnumerable<T> collection)
		{
			Contains(expected, collection, GetEqualityComparer<T>());
		}

		/// <summary>
		/// Verifies that a collection does not contain a given object.
		/// </summary>
		/// <typeparam name="T">The type of the object to be compared</typeparam>
		/// <param name="expected">The object that is expected not to be in the collection</param>
		/// <param name="collection">The collection to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the object is present inside the container</exception>
		public new static void DoesNotContain<T>(T expected, IEnumerable<T> collection)
		{
			DoesNotContain(expected, collection, GetEqualityComparer<T>());
		}

		/// <summary>
		/// Verifies that two objects are equal, using a default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <exception cref="EqualException">Thrown when the objects are not equal</exception>
		public new static void Equal<T>(T expected, T actual)
		{
			Equal(expected, actual, GetEqualityComparer<T>());
		}

		/// <summary>
		/// Verifies that two objects are equal, using a default comparer. Allows less strict comparison.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <exception cref="EqualException">Thrown when the objects are not equal</exception>
		public static void Equal<T>(T expected, T actual, bool strict)
		{
			Equal(expected, actual, GetEqualityComparer<T>(strict));
		}

		/// <summary>
		/// Verifies that two objects are not equal, using a default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected object</param>
		/// <param name="actual">The actual object</param>
		/// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
		public new static void NotEqual<T>(T expected, T actual)
		{
			NotEqual(expected, actual, GetEqualityComparer<T>());
		}

		/// <summary>
		/// Verifies that two objects are not equal, using a default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected object</param>
		/// <param name="actual">The actual object</param>
		/// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
		public static void NotEqual<T>(T expected, T actual, bool strict)
		{
			NotEqual(expected, actual, GetEqualityComparer<T>(strict));
		}

		#endregion AssertEqualityComparer<T> Entry Points

		#region AssertComparer<T> Entry Points

		/// <summary>
		/// Verifies that a value is within a given range.
		/// </summary>
		/// <typeparam name="T">The type of the value to be compared</typeparam>
		/// <param name="actual">The actual value to be evaluated</param>
		/// <param name="low">The (inclusive) low value of the range</param>
		/// <param name="high">The (inclusive) high value of the range</param>
		/// <exception cref="InRangeException">Thrown when the value is not in the given range</exception>
		public new static void InRange<T>(T actual, T low, T high)
		{
			InRange(actual, low, high, GetComparer<T>());
		}

		/// <summary>
		/// Verifies that a value is not within a given range, using the default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the value to be compared</typeparam>
		/// <param name="actual">The actual value to be evaluated</param>
		/// <param name="low">The (inclusive) low value of the range</param>
		/// <param name="high">The (inclusive) high value of the range</param>
		/// <exception cref="NotInRangeException">Thrown when the value is in the given range</exception>
		public new static void NotInRange<T>(T actual, T low, T high)
		{
			NotInRange(actual, low, high, GetComparer<T>());
		}

		#endregion AssertComparer<T> Entry Points

		#region Factory Methods

		static IEqualityComparer<T> GetEqualityComparer<T>()
		{
			return GetEqualityComparer<T>(true);
		}

		static IEqualityComparer<T> GetEqualityComparer<T>(bool strict)
		{
			return new AssertEqualityComparer<T>(strict);
		}

		static IComparer<T> GetComparer<T>()
		{
			return GetComparer<T>(true);
		}

		static IComparer<T> GetComparer<T>(bool strict)
		{
			return new AssertEqualityComparer<T>(strict);
		}

		#endregion Factory Methods

		#region AssertEqualityComparer<T>

        class AssertEqualityComparer<T> :
			IEqualityComparer<T>,
			IComparer<T>
        {
			static AssertEqualityComparer<object> innerComparer = new AssertEqualityComparer<object>(false);
			static AssertEqualityComparer<object> innerComparerStrict = new AssertEqualityComparer<object>(true);

			readonly bool Strict;

			public AssertEqualityComparer(bool strict)
			{
				this.Strict = strict;
			}

			public bool Equals(T x, T y)
            {
                Type type = typeof(T);

                // Null?
                if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
                {
                    if (Object.Equals(x, default(T)))
                        return Object.Equals(y, default(T));

                    if (Object.Equals(y, default(T)))
                        return false;
                }

                // Same type?
                if (this.Strict && x.GetType() != y.GetType())
                    return false;

                // Implements IEquatable<T>?
                IEquatable<T> equatable = x as IEquatable<T>;
                if (equatable != null)
                    return equatable.Equals(y);

                // Implements IComparable<T>?
                IComparable<T> comparable1 = x as IComparable<T>;
                if (comparable1 != null)
                    return comparable1.CompareTo(y) == 0;

                // Implements IComparable?
                IComparable comparable2 = x as IComparable;
                if (comparable2 != null)
                    return comparable2.CompareTo(y) == 0;

				#region IDictionary<string, object> hack

				// equivalent ExpandoObjects

				IDictionary<string, object> dictionaryX = x as IDictionary<string, object>;
				IDictionary<string, object> dictionaryY = y as IDictionary<string, object>;

				if (dictionaryX != null && dictionaryY != null)
				{
					if (dictionaryX.Count != dictionaryY.Count)
						return false;

					foreach (string key in dictionaryX.Keys)
					{
						if (!dictionaryY.ContainsKey(key) ||
							!(this.Strict ? innerComparerStrict : innerComparer).Equals(dictionaryX[key], dictionaryY[key]))
                            return false;
					}

					return true;
				}

				#endregion IDictionary<string, object> hack

				// Enumerable?
                IEnumerable enumerableX = x as IEnumerable;
                IEnumerable enumerableY = y as IEnumerable;

                if (enumerableX != null && enumerableY != null)
                {
                    IEnumerator enumeratorX = enumerableX.GetEnumerator();
                    IEnumerator enumeratorY = enumerableY.GetEnumerator();

                    while (true)
                    {
                        bool hasNextX = enumeratorX.MoveNext();
                        bool hasNextY = enumeratorY.MoveNext();

                        if (!hasNextX || !hasNextY)
                            return (hasNextX == hasNextY);

                        if (!(this.Strict ? innerComparerStrict : innerComparer).Equals(enumeratorX.Current, enumeratorY.Current))
                            return false;
                    }
                }

                // Last case, rely on Object.Equals
                return Object.Equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                throw new NotImplementedException();
            }

			public int Compare(T x, T y)
			{
				return this.Equals(x, y) ? 0 : 1;
			}
		}

		#endregion AssertEqualityComparer<T>
	}
}
