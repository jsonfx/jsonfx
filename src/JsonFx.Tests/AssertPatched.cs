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
#if NET4
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
#endif

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
		public static void Equal<T>(T expected, T actual, bool checkType)
		{
			Equal(expected, actual, GetEqualityComparer<T>(checkType));
		}

		/// <summary>
		/// Verifies that two dictionaries are equal, using a default comparer. Allows less strict comparison.
		/// </summary>
		/// <typeparam name="TKey">The type of the keys to be compared</typeparam>
		/// <typeparam name="TVal">The type of the values to be compared</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <exception cref="EqualException">Thrown when the objects are not equal</exception>
		public static void Equal<TKey, TVal>(IDictionary<TKey, TVal> expected, IDictionary<TKey, TVal> actual, bool checkType)
		{
			// equivalent dictionaries or ExpandoObjects
			Equal(expected, actual, new AssertDictionaryComparer<TKey, TVal>(checkType));
		}

		/// <summary>
		/// Verifies that two objects are equal, using a default comparer. Allows less strict comparison.
		/// </summary>
		/// <typeparam name="TKey">The type of the keys to be compared</typeparam>
		/// <typeparam name="TVal">The type of the values to be compared</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <exception cref="EqualException">Thrown when the objects are not equal</exception>
		public static void Equal<TKey, TVal>(IEnumerable<KeyValuePair<TKey, TVal>> expected, IEnumerable<KeyValuePair<TKey, TVal>> actual, bool checkType)
		{
			// equivalent dictionaries or ExpandoObjects
			Equal(expected, actual, new AssertKeyValuePairEnumerableComparer<TKey, TVal>(checkType));
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
		public static void NotEqual<T>(T expected, T actual, bool checkType)
		{
			NotEqual(expected, actual, GetEqualityComparer<T>(checkType));
		}

		#endregion AssertEqualityComparer<T> Entry Points

		#region Factory Methods

		static IEqualityComparer<T> GetEqualityComparer<T>()
		{
			return GetEqualityComparer<T>(true);
		}

		static IEqualityComparer<T> GetEqualityComparer<T>(bool checkType)
		{
			return new AssertEqualityComparer<T>(checkType);
		}

		#endregion Factory Methods

		#region Equality Comparers

        class AssertEqualityComparer<T> : IEqualityComparer<T>
        {
			static AssertEqualityComparer<object> innerComparer = new AssertEqualityComparer<object>(false);
			static AssertEqualityComparer<object> innerComparerStrict = new AssertEqualityComparer<object>(true);

			readonly bool CheckType;

			public AssertEqualityComparer(bool checkType)
			{
				this.CheckType = checkType;
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
                if (this.CheckType && x.GetType() != y.GetType())
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

				// hack for very common Dictionary instance
				IDictionary<string, object> dictionaryX = x as IDictionary<string, object>;
				IDictionary<string, object> dictionaryY = y as IDictionary<string, object>;
				if (dictionaryX != null &&
					dictionaryY != null)
				{
					return new AssertDictionaryComparer<string, object>(this.CheckType).Equals(dictionaryX, dictionaryY);
				}

				// Enumerable?
                IEnumerable enumerableX = x as IEnumerable;
                IEnumerable enumerableY = y as IEnumerable;

                if (enumerableX != null && enumerableY != null)
                {
                    IEnumerator enumeratorX = enumerableX.GetEnumerator();
                    IEnumerator enumeratorY = enumerableY.GetEnumerator();
					IEqualityComparer<object> comparer = this.CheckType ? innerComparerStrict : innerComparer;

                    while (true)
                    {
                        bool hasNextX = enumeratorX.MoveNext();
                        bool hasNextY = enumeratorY.MoveNext();

                        if (!hasNextX || !hasNextY)
                            return (hasNextX == hasNextY);

                        if (!comparer.Equals(enumeratorX.Current, enumeratorY.Current))
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
		}

		class AssertDictionaryComparer<TKey, TVal> : IEqualityComparer<IDictionary<TKey, TVal>>
		{
			static AssertEqualityComparer<object> innerComparer = new AssertEqualityComparer<object>(false);
			static AssertEqualityComparer<object> innerComparerStrict = new AssertEqualityComparer<object>(true);
			private readonly bool CheckType;

			public AssertDictionaryComparer(bool checkType)
			{
				this.CheckType = checkType;
			}

			#region IEqualityComparer<IDictionary<TKey,TVal>> Members

			public bool Equals(IDictionary<TKey, TVal> x, IDictionary<TKey, TVal> y)
			{
				Type type = typeof(IDictionary<TKey, TVal>);

				// Null?
				if (x == null)
					return y == null;

				if (y == null)
					return false;

				// Same type?
				if (this.CheckType && x.GetType() != y.GetType())
					return false;

				if (x.Count != y.Count)
					return false;

				IEqualityComparer<object> comparer = this.CheckType ? innerComparerStrict : innerComparer;
				foreach (TKey key in x.Keys)
				{
					if (!y.ContainsKey(key) ||
						!comparer.Equals(x[key], y[key]))
					{
						return false;
					}
				}

				return true;
			}

			public int GetHashCode(IDictionary<TKey, TVal> obj)
			{
				throw new NotImplementedException();
			}

			#endregion IEqualityComparer<IDictionary<TKey,TVal>> Members
		}

		class AssertKeyValuePairEnumerableComparer<TKey, TVal> : IEqualityComparer<IEnumerable<KeyValuePair<TKey, TVal>>>
		{
			static AssertEqualityComparer<object> innerComparer = new AssertEqualityComparer<object>(false);
			static AssertEqualityComparer<object> innerComparerStrict = new AssertEqualityComparer<object>(true);
			private readonly bool CheckType;

			public AssertKeyValuePairEnumerableComparer(bool checkType)
			{
				this.CheckType = checkType;
			}

			#region IEqualityComparer<IEnumerable<KeyValuePair<TKey, TVal>>> Members

			public bool Equals(IEnumerable<KeyValuePair<TKey, TVal>> x, IEnumerable<KeyValuePair<TKey, TVal>> y)
			{
				// Null?
				if (x == null)
				{
					return (y == null);
				}

				if (y == null)
				{
					return false;
				}

				// Same type?
				if (this.CheckType && x.GetType() != y.GetType())
				{
					return false;
				}

				IEnumerator<KeyValuePair<TKey, TVal>> enumeratorX = x.GetEnumerator();
				IEnumerator<KeyValuePair<TKey, TVal>> enumeratorY = y.GetEnumerator();
				IEqualityComparer<object> comparer = this.CheckType ? innerComparerStrict : innerComparer;

				while (true)
				{
					bool hasNextX = enumeratorX.MoveNext();
					bool hasNextY = enumeratorY.MoveNext();

					if (!hasNextX || !hasNextY)
						return (hasNextX == hasNextY);

					if (!comparer.Equals(enumeratorX.Current.Key, enumeratorY.Current.Key) ||
						!comparer.Equals(enumeratorX.Current.Value, enumeratorY.Current.Value))
					{
						return false;
					}
				}
			}

			public int GetHashCode(IEnumerable<KeyValuePair<TKey, TVal>> obj)
			{
				throw new NotImplementedException();
			}

			#endregion IEqualityComparer<IEnumerable<KeyValuePair<TKey, TVal>>> Members
		}

		#endregion Equality Comparers
	}
}
