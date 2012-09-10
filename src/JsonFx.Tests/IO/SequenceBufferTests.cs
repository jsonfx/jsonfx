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
using System.Linq;
using System.Text;

using Xunit;

namespace JsonFx.IO
{

#if !NET40
	public static class SequenceBufferHelpers
	{
		public static void Clear(this StringBuilder sb)
		{
		    sb.Length = 0;
		}
	}
#endif

	public class SequenceBufferTests
	{
		#region Constants

		private const string TraitName = "IStream<T>";
		private const string TraitValue = "SequenceBuffer";

		#endregion Constants

		#region Enumerable Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void WrapInput_NullString_ReturnsEmptySequence()
		{
			var buffer = new SequenceBuffer<char>(null);

			Assert.Equal(new char[0], buffer.ToArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void WrapInput_EmptyString_ReturnsEmptySequence()
		{
			var input = "".ToCharArray();

			var buffer = EnsureEnumerable(new SequenceBuffer<char>(input));

			Assert.Equal(input, buffer.ToArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void WrapInput_OneCharString_ReturnsSameSequence()
		{
			char[] input = "_".ToCharArray();

			var buffer = EnsureEnumerable(new SequenceBuffer<char>(input));

			Assert.Equal(input, buffer.ToArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void WrapInput_LongString_ReturnsSameSequence()
		{
			char[] input = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

			var buffer = EnsureEnumerable(new SequenceBuffer<char>(input));

			Assert.Equal(input, buffer.ToArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void WrapInput_EscapedSequence_ReturnsSameSequence()
		{
			char[] input = @"""\\\b\f\n\r\t\u0123\u4567\u89AB\uCDEF\uabcd\uef4A\""""".ToCharArray();

			var buffer = EnsureEnumerable(new SequenceBuffer<char>(input));

			Assert.Equal(input, buffer.ToArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void WrapInput_UnicodeString_ReturnsSameSequence()
		{
			char[] input = "私が日本語を話すことはありません。".ToCharArray();

			var buffer = EnsureEnumerable(new SequenceBuffer<char>(input));

			Assert.Equal(input, buffer.ToArray());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void WrapInput_LongStringTwice_ReturnsSameSequence()
		{
			char[] input = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

			List<char> expected = new List<char>(input);
			expected.AddRange(input);

			var buffer = EnsureEnumerable(new SequenceBuffer<char>(input));

			List<char> actual = new List<char>(buffer);
			actual.AddRange(buffer);

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void NonWrapedResult_LongStringTwice_Fails()
		{
			const string input = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var sequence = this.GetSequence(new StringStream(input));

			var buffer = new StringBuilder();
			foreach (char ch in sequence)
			{
				buffer.Append(ch);
			}

			Assert.Equal(input, buffer.ToString());

			buffer.Clear();
			foreach (char ch in sequence)
			{
				buffer.Append(ch);
			}

			// Fails to iterate a second time
			Assert.Equal("", buffer.ToString());
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void WrapResult_LongStringTwice_ReturnsSameSequence()
		{
			const string input = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var sequence = new SequenceBuffer<char>(this.GetSequence(new StringStream(input)));

			var buffer = new StringBuilder();
			foreach (char ch in sequence)
			{
				buffer.Append(ch);
			}

			Assert.Equal(input, buffer.ToString());

			buffer.Clear();
			foreach (char ch in sequence)
			{
				buffer.Append(ch);
			}

			// wrapped version iterates multiple times
			Assert.Equal(input, buffer.ToString());
		}

		#endregion Enumerable Tests

		#region Methods

		private IEnumerable<T> EnsureEnumerable<T>(IEnumerable<T> sequence)
		{
			if (sequence != null)
			{
				foreach (T item in sequence)
				{
					yield return item;
				}
			}
		}

		public IEnumerable<char> GetSequence(ITextStream stream)
		{
			using (stream)
			{
				while (!stream.IsCompleted)
				{
					yield return stream.Pop();
				}
			}
		}

		#endregion Methods
	}
}
