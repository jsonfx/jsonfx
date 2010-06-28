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
		#region Self Tests

		[Fact]
		public void Assert_EqualNestedArrays_ThrowsEqualException()
		{
			Assert.Throws<EqualException>(
				delegate()
				{
					Assert.Equal<IEnumerable>(
						new[] { new[] { "Foo" }, new[] { "Bar" } },
						new[] { new[] { "Foo" }, new[] { "Bar" } });
				});
		}

		[Fact]
		public void AssertPatched_EqualNestedArrays_Passes()
		{
			AssertPatched.Equal<IEnumerable>(
				new[] { new[] { "Foo" }, new[] { "Bar" } },
				new[] { new[] { "Foo" }, new[] { "Bar" } });
		}

		#endregion Self Tests
	}

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
			return new AssertEqualityComparer<T>();
		}

		static IComparer<T> GetComparer<T>()
		{
			return new AssertComparer<T>();
		}

		#endregion Factory Methods

		#region AssertEqualityComparer

		class AssertEqualityComparer<T> : IEqualityComparer<T>
		{
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
				if (x.GetType() != y.GetType())
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

				// Enumerable?
				IEnumerable enumerableX = x as IEnumerable;
				IEnumerable enumerableY = y as IEnumerable;

				if (enumerableX != null && enumerableY != null)
				{
					IDictionary<Type, object> comparerCache = new Dictionary<Type, object>();
					IDictionary<Type, MethodInfo> methodCache = new Dictionary<Type, MethodInfo>();

					IEnumerator enumeratorX = enumerableX.GetEnumerator();
					IEnumerator enumeratorY = enumerableY.GetEnumerator();

					while (true)
					{
						bool hasNextX = enumeratorX.MoveNext();
						bool hasNextY = enumeratorY.MoveNext();

						if (!hasNextX || !hasNextY)
							return (hasNextX == hasNextY);

						if (enumeratorX.Current == null)
						{
							if (enumeratorY.Current == null)
								continue;
							else
								return false;
						}

						if (enumeratorY.Current == null)
							return false;

						Type itemType = enumeratorX.Current.GetType();

						if (enumeratorY.Current.GetType() != itemType)
							return false;

						object comparer;
						MethodInfo equalsMethod;
						if (comparerCache.ContainsKey(itemType))
						{
							comparer = comparerCache[itemType];
							equalsMethod = methodCache[itemType];
						}
						else
						{
							Type comparerType = typeof(AssertEqualityComparer<>).MakeGenericType(itemType);
							ConstructorInfo ctor = comparerType.GetConstructor(Type.EmptyTypes);
							comparerCache[itemType] = comparer = ctor.Invoke(Type.EmptyTypes);
							methodCache[itemType] = equalsMethod = comparerType.GetMethod("Equals", new Type[] { itemType, itemType });
						}

						bool areEqual = (bool)equalsMethod.Invoke(comparer, new object[] { enumeratorX.Current, enumeratorY.Current });

						if (!areEqual)
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

		#endregion AssertEqualityComparer

		#region AssertComparer

		class AssertComparer<T> : IComparer<T>
		{
			public int Compare(T x, T y)
			{
				Type type = typeof(T);

				// Null?
				if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
				{
					if (Equals(x, default(T)))
					{
						if (Equals(y, default(T)))
							return 0;
						return -1;
					}

					if (Equals(y, default(T)))
						return -1;
				}

				// Same type?
				if (x.GetType() != y.GetType())
					return -1;

				// Implements IComparable<T>?
				IComparable<T> comparable1 = x as IComparable<T>;
				if (comparable1 != null)
					return comparable1.CompareTo(y);

				// Implements IComparable?
				IComparable comparable2 = x as IComparable;
				if (comparable2 != null)
					return comparable2.CompareTo(y);

				// Implements IEquatable<T>?
				IEquatable<T> equatable = x as IEquatable<T>;
				if (equatable != null)
					return equatable.Equals(y) ? 0 : -1;

				// Enumerable?
				IEnumerable enumerableX = x as IEnumerable;
				IEnumerable enumerableY = y as IEnumerable;

				if (enumerableX != null && enumerableY != null)
				{
					IDictionary<Type, object> comparerCache = new Dictionary<Type, object>();
					IDictionary<Type, MethodInfo> methodCache = new Dictionary<Type, MethodInfo>();

					IEnumerator enumeratorX = enumerableX.GetEnumerator();
					IEnumerator enumeratorY = enumerableY.GetEnumerator();

					while (true)
					{
						bool hasNextX = enumeratorX.MoveNext();
						bool hasNextY = enumeratorY.MoveNext();

						if (!hasNextX || !hasNextY)
							return (hasNextX == hasNextY) ? 0 : -1;

						if (enumeratorX.Current == null)
						{
							if (enumeratorY.Current == null)
								continue;
							else
								return -1;
						}

						if (enumeratorY.Current == null)
							return -1;

						Type itemType = enumeratorX.Current.GetType();

						if (enumeratorY.Current.GetType() != itemType)
							return -1;

						object comparer;
						MethodInfo equalsMethod;
						if (comparerCache.ContainsKey(itemType))
						{
							comparer = comparerCache[itemType];
							equalsMethod = methodCache[itemType];
						}
						else
						{
							Type comparerType = typeof(AssertEqualityComparer<>).MakeGenericType(itemType);
							ConstructorInfo ctor = comparerType.GetConstructor(Type.EmptyTypes);
							comparerCache[itemType] = comparer = ctor.Invoke(Type.EmptyTypes);
							methodCache[itemType] = equalsMethod = comparerType.GetMethod("Equals", new Type[] { itemType, itemType });
						}

						bool areEqual = (bool)equalsMethod.Invoke(comparer, new object[] { enumeratorX.Current, enumeratorY.Current });

						if (!areEqual)
							return -1;
					}
				}

				// Last case, rely on Object.Equals
				return Equals(x, y) ? 0 : -1;
			}
		}

		#endregion AssertComparer
	}
}
