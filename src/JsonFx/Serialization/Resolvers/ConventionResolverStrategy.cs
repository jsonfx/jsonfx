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
using System.Reflection;

namespace JsonFx.Serialization.Resolvers
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter using convention-based name mapping
	/// </summary>
	/// <remarks>
	/// Converts standard .NET PascalCase naming convention into the specified naming convention.
	/// http://msdn.microsoft.com/en-us/library/x2dbyw72.aspx
	/// http://msdn.microsoft.com/en-us/library/141e06ef.aspx
	/// http://msdn.microsoft.com/en-us/library/xzf533w0.aspx
	/// </remarks>
	public class ConventionResolverStrategy : PocoResolverStrategy
	{
		#region Enums

		public enum WordCasing
		{
			NoChange,
			PascalCase,
			CamelCase,
			Lowercase,
			Uppercase,
		}

		#endregion Enums

		#region Fields

		public readonly string WordSeparator;
		public readonly WordCasing Casing;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="casing"></param>
		public ConventionResolverStrategy(WordCasing casing)
			: this(casing, null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="casing"></param>
		/// <param name="wordSeparator"></param>
		public ConventionResolverStrategy(WordCasing casing, string wordSeparator)
		{
			this.WordSeparator = wordSeparator ?? String.Empty;
			this.Casing = casing;
		}

		#endregion Init

		#region Name Resolution Methods

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public override IEnumerable<DataName> GetName(MemberInfo member)
		{
			string[] words = this.SplitWords(member.Name);

			if (this.Casing != WordCasing.NoChange)
			{
				for (int i=0, length=words.Length; i<length; i++)
				{
					switch (this.Casing)
					{
						case WordCasing.PascalCase:
						{
							string word = words[i];
							if (word.Length <= 1)
							{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
								words[i] = word.ToUpper();
#else
								words[i] = word.ToUpperInvariant();
#endif
							}
							else
							{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
								words[i] = Char.ToUpper(word[0])+word.Substring(1).ToLower();
#else
								words[i] = Char.ToUpperInvariant(word[0])+word.Substring(1).ToLowerInvariant();
#endif
							}
							break;
						}
						case WordCasing.CamelCase:
						{
							string word = words[i];
							if (i == 0)
							{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
								words[i] = word.ToLower();
#else
								words[i] = word.ToLowerInvariant();
#endif
							}
							else if (word.Length <= 1)
							{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
								words[i] = word.ToUpper();
#else
								words[i] = word.ToUpperInvariant();
#endif
							}
							else
							{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
								words[i] = Char.ToUpper(word[0])+word.Substring(1).ToLower();
#else
								words[i] = Char.ToUpperInvariant(word[0])+word.Substring(1).ToLowerInvariant();
#endif
							}
							break;
						}
						case WordCasing.Lowercase:
						{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
							words[i] = words[i].ToLower();
#else
							words[i] = words[i].ToLowerInvariant();
#endif
							break;
						}
						case WordCasing.Uppercase:
						{
#if SILVERLIGHT && (NET20 || NET30 || NET35)
							words[i] = words[i].ToUpper();
#else
							words[i] = words[i].ToUpperInvariant();
#endif
							break;
						}
					}
				}
			}

			yield return new DataName(String.Join(this.WordSeparator, words));
		}

		#endregion Name Resolution Methods

		#region Methods

		/// <summary>
		/// Splits a multi-word name assuming standard .NET PascalCase conventions.
		/// </summary>
		/// <param name="multiword"></param>
		/// <returns></returns>
		/// <remarks>
		/// http://msdn.microsoft.com/en-us/library/x2dbyw72.aspx
		/// http://msdn.microsoft.com/en-us/library/141e06ef.aspx
		/// http://msdn.microsoft.com/en-us/library/xzf533w0.aspx
		/// </remarks>
		private string[] SplitWords(string multiword)
		{
			if (String.IsNullOrEmpty(multiword))
			{
				return new string[0];
			}

			List<string> words = new List<string>(5);

			// treat as capitalized (even if not)
			bool prevWasUpper = true;

			int start = 0,
				length = multiword.Length;

			for (int i=0; i<length; i++)
			{
				if (!Char.IsLetterOrDigit(multiword, i))
				{
					// found split on symbol char
					if (i > start)
					{
						words.Add(multiword.Substring(start, i-start));
					}
					start = i+1;
					prevWasUpper = true;
					continue;
				}

				bool isLower = Char.IsLower(multiword, i);

				if (prevWasUpper)
				{
					if (isLower && (start < i-1))
					{
						// found split
						words.Add(multiword.Substring(start, i-start-1));
						start = i-1;
					}
				}
				else if (!isLower)
				{
					// found split
					words.Add(multiword.Substring(start, i-start));
					start = i;
				}

				prevWasUpper = !isLower;
			}

			int remaining = length-start-1;
			if (((remaining == 1) && Char.IsLetterOrDigit(multiword[start])) || (remaining > 1))
			{
				words.Add(multiword.Substring(start));
			}

			return words.ToArray();
		}

		#endregion Methods
	}
}
