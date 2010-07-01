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
using System.Reflection;

using Xunit;

namespace JsonFx.CodeGen
{
	public class DynamicMethodGeneratorTests
	{
		#region Test Types

		private class Example
		{
			#region Fields

			private string a = "aye";
			private string b = "bee";
			private string c = "sea";
			private int one = 1;
			private int two = 2;
			private int three = 3;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			public Example()
			{
				this.Do = "doe";
				this.Re = "ray";
				this.Mi = "me";
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

			#endregion Properties
		}

		#endregion Test Types

		#region GetPropertyGetter Tests

		[Fact]
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
		public void GetPropertyGetter_ProtectedProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("C", BindingFlags.NonPublic|BindingFlags.Instance);
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);

			Assert.NotNull(getter);
			Assert.Equal("sea", getter(input));
		}

		[Fact]
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
		public void GetPropertyGetter_PrivateProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("Two", BindingFlags.NonPublic|BindingFlags.Instance);
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);

			Assert.NotNull(getter);
			Assert.Equal(2, getter(input));
		}

		[Fact]
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
		public void GetPropertyGetter_SetterOnlyProperty_ReturnsPropertyValue()
		{
			var input = new Example();

			var propertyInfo = input.GetType().GetProperty("Three_Setter");
			Assert.NotNull(propertyInfo);

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);

			Assert.Null(getter);
		}

		[Fact]
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
		public void GetPropertyGetter_NullInput_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(
				delegate()
				{
					GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(null);
				});
		}

		#endregion GetPropertyGetter Tests
	}
}
