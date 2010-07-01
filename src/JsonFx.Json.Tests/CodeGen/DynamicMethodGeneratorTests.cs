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

namespace JsonFx.CodeGen
{
	public class DynamicMethodGeneratorTests
	{
		#region Test Types

		public class Example
		{
			public Example(
				string a, string b, string c, string d, string e,
				int one, int two, int three, int four, int five)
			{
				this.A = a;
				this.B = b;
				this.C = c;
				this.D = d;
				this.E = e;
				this.One = one;
				this.Two = two;
				this.Three = three;
				this.Four = four;
				this.Five = five;
			}

			public string A { get; set; }
			public virtual string B { get; set; }
			protected string C { get; set; }
			protected virtual string D { get; set; }
			private string E { get; set; }

			public int One { get; set; }
			public virtual int Two { get; set; }
			protected int Three { get; set; }
			protected virtual int Four { get; set; }
			private int Five { get; set; }
		}

		#endregion Test Types

		#region GetPropertyGetter Tests

		[Fact]
		public void GetPropertyGetter_AnonymousObjectWithPublicValueTypeProperty_ReturnsValueType()
		{
			var input = new
			{
				A = "aye",
				B = "bee",
				C = "sea",
				One = 1,
				Two = 2,
				Three = 3
			};

			var propertyInfo = input.GetType().GetProperty("Two");

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);

			Assert.Equal(2, getter(input));
		}

		[Fact]
		public void GetPropertyGetter_AnonymousObjectWithPublicReferenceTypeProperty_ReturnsReferenceType()
		{
			var input = new
			{
				A = "aye",
				B = "bee",
				C = "sea",
				One = 1,
				Two = 2,
				Three = 3
			};

			var propertyInfo = input.GetType().GetProperty("C");

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);

			Assert.Equal("sea", getter(input));
		}

		[Fact]
		public void GetPropertyGetter_ClassWithPublicValueTypeProperty_ReturnsValueType()
		{
			var input = new Example("aye", "bee", "sea", "dee", "yi", 1, 2, 3, 4, 5);

			var propertyInfo = input.GetType().GetProperty("One");

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);

			Assert.Equal(1, getter(input));
		}

		[Fact]
		public void GetPropertyGetter_ClassWithPublicReferenceTypeProperty_ReturnsReferenceType()
		{
			var input = new Example("aye", "bee", "sea", "dee", "yi", 1, 2, 3, 4, 5);

			var propertyInfo = input.GetType().GetProperty("A");

			GetterDelegate getter = DynamicMethodGenerator.GetPropertyGetter(propertyInfo);

			Assert.Equal("aye", getter(input));
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
