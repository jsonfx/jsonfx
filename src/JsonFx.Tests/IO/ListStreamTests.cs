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
using System.Text;

using Xunit;

namespace JsonFx.IO
{
	public class ListStreamTests
	{
		#region Constants

		private const string TraitName = "IStream<T>";
		private const string TraitValue = "ListStream";

		#endregion Constants

		#region Start State Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Peek_NullString_ReturnsNullChar()
		{
			using (var scanner = new ListStream<char>(null))
			{
				Assert.Equal('\0', scanner.Peek());
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Pop_NullString_ReturnsNullChar()
		{
			using (var scanner = new ListStream<char>(null))
			{
				Assert.Equal('\0', scanner.Pop());
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void IsCompleted_NullString_ReturnsFalse()
		{
			using (var scanner = new ListStream<char>(null))
			{
				Assert.Equal(true, scanner.IsCompleted);
			}
		}

		#endregion Start State Tests

		#region Pop Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Pop_NullString_ReturnsEmptySequence()
		{
			using (var scanner = new ListStream<char>(null))
			{
				var buffer = new StringBuilder();
				while (!scanner.IsCompleted)
				{
					buffer.Append(scanner.Pop());
				}

				Assert.Equal(String.Empty, buffer.ToString());
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Pop_EmptyString_ReturnsEmptySequence()
		{
			const string input = "";

			using (var scanner = new ListStream<char>(input.ToCharArray()))
			{
				var buffer = new StringBuilder();
				while (!scanner.IsCompleted)
				{
					buffer.Append(scanner.Pop());
				}

				Assert.Equal(input, buffer.ToString());
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Pop_OneCharString_ReturnsSameSequence()
		{
			const string input = "_";

			using (var scanner = new ListStream<char>(input.ToCharArray()))
			{
				var buffer = new StringBuilder();
				while (!scanner.IsCompleted)
				{
					buffer.Append(scanner.Pop());
				}

				Assert.Equal(input, buffer.ToString());
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Pop_LongString_ReturnsSameSequence()
		{
			const string input = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

			using (var scanner = new ListStream<char>(input.ToCharArray()))
			{
				var buffer = new StringBuilder();
				while (!scanner.IsCompleted)
				{
					buffer.Append(scanner.Pop());
				}

				Assert.Equal(input, buffer.ToString());
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Pop_EscapedSequence_ReturnsSameSequence()
		{
			const string input = @"""\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\""""";

			using (var scanner = new ListStream<char>(input.ToCharArray()))
			{
				var buffer = new StringBuilder();
				while (!scanner.IsCompleted)
				{
					buffer.Append(scanner.Pop());
					scanner.Peek();
				}

				Assert.Equal(input, buffer.ToString());
			}
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Pop_UnicodeString_ReturnsSameSequence()
		{
			const string input = "私が日本語を話すことはありません。";

			using (var scanner = new ListStream<char>(input.ToCharArray()))
			{
				var buffer = new StringBuilder();
				while (!scanner.IsCompleted)
				{
					buffer.Append(scanner.Pop());
				}

				Assert.Equal(input, buffer.ToString());
			}
		}

		#endregion Pop Tests

		#region Peek Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void Peek_LongString_ReturnsSameAsPop()
		{
			const string input = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

			using (var scanner = new ListStream<char>(input.ToCharArray()))
			{
				while (!scanner.IsCompleted)
				{
					char ch = scanner.Peek();
					Assert.Equal(scanner.Pop(), ch);
				}

				Assert.Equal(true, scanner.IsCompleted);
			}
		}

		#endregion Peek Tests
	}
}
