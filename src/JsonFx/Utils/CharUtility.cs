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

namespace JsonFx.Utils
{
	/// <summary>
	/// Character Utility
	/// </summary>
	/// <remarks>
	/// These are either simpler definitions of character classes (e.g. letter is [a-zA-Z]),
	/// or they implement platform-agnositic checks (read: "Silverlight workarounds").
	/// </remarks>
	public static class CharUtility
	{
		#region Char Methods

		/// <summary>
		/// Checks if string is null, empty or entirely made up of whitespace
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks>
		/// Essentially the same as String.IsNullOrWhiteSpace from .NET 4.0
		/// with a simplfied view of whitespace.
		/// </remarks>
		public static bool IsNullOrWhiteSpace(string value)
		{
			if (value != null)
			{
				for (int i=0, length=value.Length; i<length; i++)
				{
					if (!CharUtility.IsWhiteSpace(value[i]))
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Checks if character is line ending, tab or space
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		public static bool IsWhiteSpace(char ch)
		{
			return
				(ch == ' ') |
				(ch == '\n') ||
				(ch == '\r') ||
				(ch == '\t');
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		public static bool IsControl(char ch)
		{
			return (ch <= '\u001F');
		}

		/// <summary>
		/// Checks if character matches [A-Za-z]
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		public static bool IsLetter(char ch)
		{
			return
				((ch >= 'a') && (ch <= 'z')) ||
				((ch >= 'A') && (ch <= 'Z'));
		}

		/// <summary>
		/// Checks if character matches [0-9]
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		public static bool IsDigit(char ch)
		{
			return (ch >= '0') && (ch <= '9');
		}

		/// <summary>
		/// Checks if character matches [0-9A-Fa-f]
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		public static bool IsHexDigit(char ch)
		{
			return
				(ch >= '0' && ch <= '9') ||
				(ch >= 'A' && ch <= 'F') ||
				(ch >= 'a' && ch <= 'f');
		}

		/// <summary>
		/// Gets a 4-bit number as a hex digit
		/// </summary>
		/// <param name="i">0-15</param>
		/// <returns></returns>
		public static char GetHexDigit(int i)
		{
			if (i < 10)
			{
				return (char)(i + '0');
			}

			return (char)((i - 10) + 'a');
		}

		/// <summary>
		/// Formats a number as a hex digit
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public static string GetHexString(ulong i)
		{
			string hex = "";
			while (i > 0)
			{
				hex = String.Concat(CharUtility.GetHexDigit((int)(i % 0x10)), hex);
				i >>= 4;
			}
			return hex;
		}

		/// <summary>
		/// Converts the value of a UTF-16 encoded character or surrogate pair at a specified
		/// position in a string into a Unicode code point.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="i"></param>
		/// <returns></returns>
		public static int ConvertToUtf32(string value, int index)
		{
#if SILVERLIGHT
			return (int)value[index];
#else
			if (char.IsSurrogate(value, index))
			{
				return ((int)value[index]);
			}
			else
			{
				return Char.ConvertToUtf32(value, index);
			}
#endif
		}

		/// <summary>
		/// Converts the specified Unicode code point into a UTF-16 encoded string.
		/// </summary>
		/// <param name="utf32"></param>
		/// <returns></returns>
		public static string ConvertFromUtf32(int utf32)
		{
#if SILVERLIGHT
			if (utf32 <= 0xFFFF)
			{
				return new string((char)utf32, 1);
			}

			utf32 -= 0x10000;

			return new string(
				new char[]
				{
					(char)((utf32 / 0x400) + 0xD800),
					(char)((utf32 % 0x400) + 0xDC00)
				});
#else
			return Char.ConvertFromUtf32(utf32);
#endif
		}

		#endregion Char Methods
	}
}
