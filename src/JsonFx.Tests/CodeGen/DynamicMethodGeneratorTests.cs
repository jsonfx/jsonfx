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
using System.Reflection;

using Xunit;

namespace JsonFx.CodeGen
{
	public class DynamicMethodGeneratorTests
	{
		#region Constants

		private const string TraitName = "Utilities";
		private const string TraitValue = "DynamicMethodGenerator";

		#endregion Constants

		#region Test Types

		private class Example
		{
			#region Fields

			public string a;
			private string b;
			protected string c;
			public int one = 1;
			private int two = 2;
			private int three = 3;

			public static int MyStatic = 42;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			static Example()
			{
				Example.Solo = "Single";
			}

			/// <summary>
			/// Ctor
			/// </summary>
			public Example()
				: this("aye", "bee", "sea", 1, 2, 3, "doe", "ray", "me")
			{
			}

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <param name="c"></param>
			/// <param name="one"></param>
			/// <param name="two"></param>
			/// <param name="three"></param>
			/// <param name="do"></param>
			/// <param name="re"></param>
			/// <param name="mi"></param>
			public Example(string a, string b, string c, int one, int two, int three, string @do, string re, string mi)
			{
				this.a = a;
				this.b = b;
				this.c = c;
				this.one = one;
				this.two = two;
				this.three = three;
				this.Do = @do;
				this.Re = re;
				this.Mi = mi;
			}

			#endregion Init

			#region Properties

			public string A
			{
				get { return this.a; }
				set { this.a = value; }
			}

			public virtual string B
			{
				get { return this.b; }
				set { this.b = value; }
			}

			protected string C
			{
				get { return this.c; }
				set { this.c = value; }
			}

			public int One
			{
				get { return this.one; }
				set { this.one = value; }
			}

			private int Two
			{
				get { return this.two; }
				set { this.two = value; }
			}

			public int Three
			{
				get { return this.three; }
				set { this.three = value; }
			}

			public int Three_Getter
			{
				get { return this.three; }
			}

			public int Three_Setter
			{
				set { this.three = value; }
			}

			public string Do
			{
				get;
				set;
			}

			public string Re
			{
				get;
				private set;
			}

			public string Mi
			{
				private get;
				set;
			}

			public static string Solo
			{
				get;
				set;
			}

			#endregion Properties

			#region Methods

			public void Reset(string a, string b, string c, int one, int two, int three, string @do, string re, string mi)
			{
				this.a = a;
				this.b = b;
				this.c = c;
				this.one = one;
				this.two = two;
				this.three = three;
				this.Do = @do;
				this.Re = re;
				this.Mi = mi;
			}

			public string GetMi()
			{
				return this.Mi;
			}

			public string Swap(string a)
			{
				return System.Threading.Interlocked.Exchange(ref this.a, a);
			}

			#endregion Methods
		}

		#endregion Test Types

		#region Constants

		private const long MaxCount = 1000000;

		#endregion Constants

		#region GetPropertyGetter Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_PublicReferenceTypeProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("A");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			Assert.Equal("aye", getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_VirtualProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("B");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			Assert.Equal("bee", getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_ProtectedProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("C", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			Assert.Equal("sea", getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_ValueTypeProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("One");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			Assert.Equal(1, getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_PrivateProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("Two", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			Assert.Equal(2, getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_GetterOnlyProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("Three_Getter");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			Assert.Equal(3, getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_SetterOnlyProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("Three_Setter");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.Null(getter);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_AutoImplementedProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("Do");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			Assert.Equal("doe", getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_PrivateSetterProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("Re");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			Assert.Equal("ray", getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_PrivateGetterProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("Mi");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);

			Assert.NotNull(getter);
			Assert.Equal("me", getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_AnonymousObjectProperty_ReturnsPropertyValue()
		{
			var input = new
			{
				A = "aye",
				B = "bee",
				C = "sea",
				One = 1,
				Two = 2,
				Three = 3,
				Do = "doe",
				Re = "ray",
				Mi = "me"
			};

			var propertyInfo = input.GetType().GetProperty("Two");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			Assert.Equal(2, getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_NullInput_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(null);
				});

			Assert.Equal("propertyInfo", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertyGetter_StaticProperty_ChangesFieldValue()
		{
			var propertyInfo = typeof(Example).GetProperty("Solo");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);
			Assert.NotNull(getter);

			var actual = getter(null);
			Assert.Equal("Single", Example.Solo);
		}

		[Fact(Timeout=1000)]
		public void GetPropertyGetter_1MillionPropertyGets_PerformsInAround10ms()
		{
			Example instance = new Example();
			PropertyInfo propertyInfo = typeof(Example).GetProperty("A", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);

			string value = null;
			for (long i=0; i<MaxCount; i++)
			{
				value = (string)getter(instance);
			}
		}

		#endregion GetPropertyGetter Tests

		#region GetPropertySetter Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_PublicReferenceTypeProperty_ReturnsPropertyValue()
		{
			var input = new Example();
			var expected = "alpha";

			var propertyInfo = input.GetType().GetProperty("A");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);

			Assert.NotNull(setter);
			setter(input, expected);

			Assert.Equal(expected, input.A);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_VirtualProperty_ReturnsPropertyValue()
		{
			var input = new Example();
			var expected = "bravo";

			var propertyInfo = input.GetType().GetProperty("B");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.NotNull(setter);

			setter(input, expected);
			Assert.Equal(expected, input.B);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_ProtectedProperty_ReturnsPropertyValue()
		{
			var input = new Example();
			var expected = "charlie";

			var propertyInfo = input.GetType().GetProperty("C", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			MethodInfo getter = propertyInfo.GetGetMethod(true);
			Assert.NotNull(getter);

			Assert.Equal(expected, getter.Invoke(input, null));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_ValueTypeProperty_ReturnsPropertyValue()
		{
			var input = new Example();
			var expected = -1;

			var propertyInfo = input.GetType().GetProperty("One");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			Assert.Equal(expected, input.One);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_PrivateProperty_ReturnsPropertyValue()
		{
			var input = new Example();
			var expected = -2;

			var propertyInfo = input.GetType().GetProperty("Two", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			MethodInfo getter = propertyInfo.GetGetMethod(true);
			Assert.NotNull(getter);

			Assert.Equal(expected, getter.Invoke(input, null));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_GetterOnlyProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("Three_Getter");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.Null(setter);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_SetterOnlyProperty_ReturnsPropertyValue()
		{
			var input = new Example();
			var expected = -3;

			var propertyInfo = input.GetType().GetProperty("Three_Setter");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			Assert.Equal(expected, input.Three);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_AutoImplementedProperty_ReturnsPropertyValue()
		{
			var input = new Example();
			var expected = "deer";

			var propertyInfo = input.GetType().GetProperty("Do");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			Assert.Equal(expected, input.Do);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_PrivateSetterProperty_ReturnsPropertyValue()
		{
			var input = new Example();
			var expected = "sun";

			var propertyInfo = input.GetType().GetProperty("Re");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			Assert.Equal(expected, input.Re);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_PrivateGetterProperty_ReturnsPropertyValue()
		{
			var input = new Example();
			var expected = "myself";

			var propertyInfo = input.GetType().GetProperty("Mi");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			MethodInfo getter = propertyInfo.GetGetMethod(true);
			Assert.NotNull(getter);

			Assert.Equal(expected, getter.Invoke(input, null));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_AnonymousObjectProperty_ReturnsPropertyValue()
		{
			var input = new
			{
				A = "aye",
				B = "bee",
				C = "sea",
				One = 1,
				Two = 2,
				Three = 3,
				Do = "doe",
				Re = "ray",
				Mi = "me"
			};

			var propertyInfo = input.GetType().GetProperty("Two");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.Null(setter);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_NullInput_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(null);
				});

			Assert.Equal("propertyInfo", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetPropertySetter_StaticProperty_ChangesFieldValue()
		{
			var propertyInfo = typeof(Example).GetProperty("Solo");
			Assert.NotNull(propertyInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);
			Assert.NotNull(setter);

			var prev = Example.Solo;
			try
			{
				setter(null, "Duet");
				Assert.Equal("Duet", Example.Solo);
			}
			finally
			{
				Example.Solo = prev;
			}
		}

		[Fact(Timeout=1000)]
		public void GetPropertySetter_1MillionPropertySets_PerformsInAround10ms()
		{
			Example instance = new Example();
			PropertyInfo propertyInfo = typeof(Example).GetProperty("A", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);

			SetterDelegate setter = DynamicMethodGenerator.GetPropertySetter(propertyInfo);

			for (long i=0; i<MaxCount; i++)
			{
				setter(instance, "alpha");
			}
		}

		#endregion GetPropertySetter Tests

		#region GetFieldGetter Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldGetter_PublicReferenceTypeField_ReturnsFieldValue()
		{
			var input = new Example();

			var fieldInfo = input.GetType().GetField("a");
			Assert.NotNull(fieldInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);
			Assert.NotNull(getter);

			Assert.Equal("aye", getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldGetter_ProtectedField_ReturnsFieldValue()
		{
			var input = new Example();

			var fieldInfo = input.GetType().GetField("c", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			Assert.NotNull(fieldInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);
			Assert.NotNull(getter);

			Assert.Equal("sea", getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldGetter_ValueTypeField_ReturnsFieldValue()
		{
			var input = new Example();

			var fieldInfo = input.GetType().GetField("one");
			Assert.NotNull(fieldInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);
			Assert.NotNull(getter);

			Assert.Equal(1, getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldGetter_PrivateField_ReturnsFieldValue()
		{
			var input = new Example();

			var fieldInfo = input.GetType().GetField("two", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			Assert.NotNull(fieldInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);
			Assert.NotNull(getter);

			Assert.Equal(2, getter(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldGetter_NullInput_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					GetterDelegate getter = DynamicMethodGenerator.GetFieldGetter(null);
				});

			Assert.Equal("fieldInfo", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldGetter_StaticField_ReturnsFieldValue()
		{
			var fieldInfo = typeof(Example).GetField("MyStatic");
			Assert.NotNull(fieldInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);
			Assert.NotNull(getter);

			Assert.Equal(42, getter(null));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldGetter_StaticReadonlyField_ReturnsFieldValue()
		{
			var fieldInfo = typeof(Guid).GetField("Empty");
			Assert.NotNull(fieldInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);
			Assert.NotNull(getter);

			Assert.Equal(Guid.Empty, getter(null));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldGetter_ConstField_ReturnsFieldValue()
		{
			var fieldInfo = typeof(Int32).GetField("MaxValue");
			Assert.NotNull(fieldInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);
			Assert.NotNull(getter);

			Assert.Equal(Int32.MaxValue, getter(null));
		}

		[Fact(Timeout=1000)]
		public void GetFieldGetter_1MillionFieldGets_PerformsInAround10ms()
		{
			Example instance = new Example();
			FieldInfo fieldInfo = typeof(Example).GetField("a", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);

			GetterDelegate getter = DynamicMethodGenerator.GetFieldGetter(fieldInfo);

			string value = null;
			for (long i=0; i<MaxCount; i++)
			{
				value = (string)getter(instance);
			}
		}

		#endregion GetFieldGetter Tests

		#region GetFieldSetter Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldSetter_PublicReferenceTypeField_ReturnsFieldValue()
		{
			var input = new Example();
			var expected = "alpha";

			var fieldInfo = input.GetType().GetField("a");
			Assert.NotNull(fieldInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);

			Assert.NotNull(setter);
			setter(input, expected);

			Assert.Equal(expected, input.A);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldSetter_ProtectedField_ReturnsFieldValue()
		{
			var input = new Example();
			var expected = "charlie";

			var fieldInfo = input.GetType().GetField("c", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			Assert.NotNull(fieldInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			Assert.Equal(expected, fieldInfo.GetValue(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldSetter_ValueTypeField_ReturnsFieldValue()
		{
			var input = new Example();
			var expected = -1;

			var fieldInfo = input.GetType().GetField("one");
			Assert.NotNull(fieldInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			Assert.Equal(expected, input.One);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldSetter_PrivateField_ReturnsFieldValue()
		{
			var input = new Example();
			var expected = -2;

			var fieldInfo = input.GetType().GetField("two", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			Assert.NotNull(fieldInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);
			Assert.NotNull(setter);
			setter(input, expected);

			Assert.Equal(expected, fieldInfo.GetValue(input));
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldSetter_NullInput_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					SetterDelegate setter = DynamicMethodGenerator.GetFieldSetter(null);
				});

			Assert.Equal("fieldInfo", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldSetter_StaticField_ChangesFieldValue()
		{
			var fieldInfo = typeof(Example).GetField("MyStatic");
			Assert.NotNull(fieldInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);
			Assert.NotNull(setter);

			var prev = Example.MyStatic;
			try
			{
				setter(null, 2010);
				Assert.Equal(2010, Example.MyStatic);
			}
			finally
			{
				Example.MyStatic = prev;
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldSetter_StaticReadonlyField_ReturnsNull()
		{
			var fieldInfo = typeof(Guid).GetField("Empty");
			Assert.NotNull(fieldInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);
			Assert.Null(setter);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetFieldSetter_ConstField_ReturnsNull()
		{
			var fieldInfo = typeof(Int32).GetField("MaxValue");
			Assert.NotNull(fieldInfo);

			SetterDelegate setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);
			Assert.Null(setter);
		}

		[Fact(Timeout=1000)]
		public void GetFieldSetter_1MillionFieldSets_PerformsInAround10ms()
		{
			Example instance = new Example();
			FieldInfo fieldInfo = typeof(Example).GetField("a", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);

			SetterDelegate setter = DynamicMethodGenerator.GetFieldSetter(fieldInfo);

			for (long i=0; i<MaxCount; i++)
			{
				setter(instance, "alpha");
			}
		}

		#endregion GetFieldSetter Tests

		#region GetMethodProxy Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetMethodProxy_MethodNoArgsOneReturn_BuildsProxyAndInvokes()
		{
			var input = new Example();

			ProxyDelegate proxy = DynamicMethodGenerator.GetMethodProxy(typeof(Example).GetMethod("GetMi"));
			Assert.NotNull(proxy);
			var actual = (string)proxy(input);

			Assert.Equal("me", actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetMethodProxy_MethodOneArgOneReturn_BuildsProxyAndInvokes()
		{
			var input = new Example();

			ProxyDelegate proxy = DynamicMethodGenerator.GetMethodProxy(typeof(Example).GetMethod("Swap"));
			Assert.NotNull(proxy);
			var actual = (string)proxy(input, "foo");

			Assert.Equal("aye", actual);
			Assert.Equal("foo", input.A);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetMethodProxy_MethodAssortedArgsVoidReturn_BuildsProxyAndInvokes()
		{
			var input = new Example();
			var expected = new Example("alpha", "bravo", "charlie", -1, -2, -3, "deer", "sun", "myself");

			ProxyDelegate proxy = DynamicMethodGenerator.GetMethodProxy(typeof(Example).GetMethod("Reset"));
			Assert.NotNull(proxy);
			proxy(input, "alpha", "bravo", "charlie", -1, -2, -3, "deer", "sun", "myself");

			var getters =
				from m in typeof(Example).GetMembers(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy)
				let g = DynamicMethodGenerator.GetGetter(m)
				where (g != null)
				select g;

			foreach (GetterDelegate getter in getters)
			{
				// assert all of the fields and properties are equal
				Assert.Equal(getter(expected), getter(input));
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetMethodProxy_MethodExtraArgs_IgnoresExtraBuildsProxyAndInvokes()
		{
			var input = new Example();
			var expected = new Example("alpha", "bravo", "charlie", -1, -2, -3, "deer", "sun", "myself");

			ProxyDelegate proxy = DynamicMethodGenerator.GetMethodProxy(typeof(Example).GetMethod("Reset"));
			Assert.NotNull(proxy);
			proxy(input, "alpha", "bravo", "charlie", -1, -2, -3, "deer", "sun", "myself", 4, 5, 6, "extra", false);

			var getters =
				from m in typeof(Example).GetMembers(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy)
				let g = DynamicMethodGenerator.GetGetter(m)
				where (g != null)
				select g;

			foreach (GetterDelegate getter in getters)
			{
				// assert all of the fields and properties are equal
				Assert.Equal(getter(expected), getter(input));
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetMethodProxy_ArgsMissingWhenCalling_ThrowsArgumentNullException()
		{
			ProxyDelegate proxy = DynamicMethodGenerator.GetMethodProxy(typeof(Example).GetMethod("Reset"));
			Assert.NotNull(proxy);

			ArgumentException ex = Assert.Throws<ArgumentException>(
				delegate()
				{
					var actual = proxy(new Example(), "alpha", "bravo", "charlie", -1, -2, -3);
				});
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetMethodProxy_ArgsTypeMismatchWhenCalling_ThrowsArgumentNullException()
		{
			ProxyDelegate proxy = DynamicMethodGenerator.GetMethodProxy(typeof(Example).GetMethod("Reset"));
			Assert.NotNull(proxy);

			InvalidCastException ex = Assert.Throws<InvalidCastException>(
				delegate()
				{
					proxy(new Example(), 1, 2, 3, 4, 5, 6, 7, 8, 9);
				});
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetMethodProxy_NullInput_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					ProxyDelegate setter = DynamicMethodGenerator.GetMethodProxy(null);
				});

			Assert.Equal("methodInfo", ex.ParamName);
		}

		[Fact(Timeout=1000)]
		public void GetMethodProxy_1MillionMethodCalls_PerformsInAround50ms()
		{
			Example instance = new Example();
			MethodInfo methodInfo = typeof(Example).GetMethod("GetMi", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);

			ProxyDelegate proxy = DynamicMethodGenerator.GetMethodProxy(methodInfo);

			string value = null;
			for (long i=0; i<MaxCount; i++)
			{
				value = (string)proxy(instance);
			}
		}

		#endregion GetMethodProxy Tests

		#region GetTypeFactory Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_CtorNoArgs_ReturnsCorrectlyInstantiatedObject()
		{
			var expected = new Example();

			FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory(typeof(Example));
			Assert.NotNull(factory);
			var actual = (Example)factory();

			var getters =
				from m in typeof(Example).GetMembers(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy)
				let g = DynamicMethodGenerator.GetGetter(m)
				where (g != null)
				select g;

			foreach (GetterDelegate getter in getters)
			{
				// assert all of the fields and properties are equal
				Assert.Equal(getter(expected), getter(actual));
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_CtorNoArgsAlt_ReturnsCorrectlyInstantiatedObject()
		{
			var expected = new Example();

			FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory(typeof(Example), Type.EmptyTypes);
			Assert.NotNull(factory);
			var actual = (Example)factory();

			var getters =
				from m in typeof(Example).GetMembers(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy)
				let g = DynamicMethodGenerator.GetGetter(m)
				where (g != null)
				select g;

			foreach (GetterDelegate getter in getters)
			{
				// assert all of the fields and properties are equal
				Assert.Equal(getter(expected), getter(actual));
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_CtorAssortedArgs_ReturnsCorrectlyInstantiatedObject()
		{
			var expected = new Example("alpha", "bravo", "charlie", -1, -2, -3, "deer", "sun", "myself");

			FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory(
				typeof(Example),
				typeof(string), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(string), typeof(string), typeof(string));
			Assert.NotNull(factory);
			var actual = (Example)factory("alpha", "bravo", "charlie", -1, -2, -3, "deer", "sun", "myself");

			var getters =
				from m in typeof(Example).GetMembers(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy)
				let g = DynamicMethodGenerator.GetGetter(m)
				where (g != null)
				select g;

			foreach (GetterDelegate getter in getters)
			{
				// assert all of the fields and properties are equal
				Assert.Equal(getter(expected), getter(actual));
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_CtorExtraArgs_IgnoresAndReturnsCorrectlyInstantiatedObject()
		{
			var expected = new Example("alpha", "bravo", "charlie", -1, -2, -3, "deer", "sun", "myself");

			FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory(
				typeof(Example),
				typeof(string), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(string), typeof(string), typeof(string));
			Assert.NotNull(factory);
			var actual = (Example)factory("alpha", "bravo", "charlie", -1, -2, -3, "deer", "sun", "myself", 4, 5, 6, "extra", false);

			var getters =
				from m in typeof(Example).GetMembers(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy)
				let g = DynamicMethodGenerator.GetGetter(m)
				where (g != null)
				select g;

			foreach (GetterDelegate getter in getters)
			{
				// assert all of the fields and properties are equal
				Assert.Equal(getter(expected), getter(actual));
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_ArgsMismatchInBuilding_ReturnsNull()
		{
			FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory(
				typeof(Example),
				typeof(string), typeof(string), typeof(string));
			Assert.Null(factory);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_ArgsMissingWhenCalling_ThrowsArgumentNullException()
		{
			FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory(
				typeof(Example),
				typeof(string), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(string), typeof(string), typeof(string));
			Assert.NotNull(factory);

			TypeLoadException ex = Assert.Throws<TypeLoadException>(
				delegate()
				{
					var actual = (Example)factory("alpha", "bravo", "charlie", -1, -2, -3);
				});
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_ArgsTypeMismatchWhenCalling_ThrowsArgumentNullException()
		{
			FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory(
				typeof(Example),
				typeof(string), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(string), typeof(string), typeof(string));
			Assert.NotNull(factory);

			InvalidCastException ex = Assert.Throws<InvalidCastException>(
				delegate()
				{
					var actual = (Example)factory(1, 2, 3, 4, 5, 6, 7, 8, 9);
				});
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_NullTypeInput_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory((Type)null);
				});

			Assert.Equal("type", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_NullTypeInputAlt_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory((Type)null, Type.EmptyTypes);
				});

			Assert.Equal("type", ex.ParamName);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTypeFactory_NullCtorInput_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate()
				{
					FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory((ConstructorInfo)null);
				});

			Assert.Equal("ctor", ex.ParamName);
		}

		[Fact(Timeout=1000)]
		public void GetTypeFactory_1MillionInstantiations_PerformsInAround50ms()
		{
			FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory(typeof(Example));

			for (long i=0; i<MaxCount; i++)
			{
				Example instance = (Example)factory();
			}
		}

		[Fact(Timeout=1000)]
		public void GetTypeFactory_1MillionInstantiationsAlt_PerformsInAround50ms()
		{
			FactoryDelegate factory = DynamicMethodGenerator.GetTypeFactory(typeof(Example), Type.EmptyTypes);

			for (long i=0; i<MaxCount; i++)
			{
				Example instance = (Example)factory();
			}
		}

		#endregion GetTypeFactory Tests
	}
}
