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
using System.Globalization;

using JsonFx.Serialization;

namespace JsonFx.Json.Filters
{
	/// <summary>
	/// Defines a filter for JSON serialization of DateTime into ISO-8601
	/// </summary>
	/// <remarks>
	/// This is the format used by EcmaScript JSON.stringify(...):
	///		http://json.org/json.js
	///		http://www.w3.org/TR/NOTE-datetime
	///		http://en.wikipedia.org/wiki/ISO_8601
	///	
	/// NOTE: This format is limited to expressing DateTime at the millisecond level as either UTC or Unspecified.
	/// </remarks>
	public class Iso8601DateFilter : JsonFilter<DateTime>
	{
		#region Constants

		private const string Iso8601Format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";
		private const string UtcIso8601Format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

		#endregion Constants

		#region IDataFilter<JsonTokenType,DateTime> Members

		public override bool TryRead(DataReaderSettings settings, IEnumerable<Token<JsonTokenType>> tokens, out DateTime value)
		{
			IEnumerator<Token<JsonTokenType>> enumerator = tokens.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				value = default(DateTime);
				return false;
			}

			Token<JsonTokenType> token = enumerator.Current;
			if (enumerator.Current == null ||
				enumerator.Current.TokenType != JsonTokenType.String)
			{
				value = default(DateTime);
				return false;
			}

			return this.TryParseIso8601(
				Convert.ToString(token.Value, CultureInfo.InvariantCulture),
				out value);
		}

		public override bool TryWrite(DataWriterSettings settings, DateTime value, out IEnumerable<Token<JsonTokenType>> tokens)
		{
			tokens = new Token<JsonTokenType>[]
				{
					JsonGrammar.TokenString(this.FormatIso8601(value))
				};

			return true;
		}

		#endregion IDataFilter<JsonTokenType,DateTime> Members

		#region Utility Methods

		/// <summary>
		/// Converts a ISO-8601 string to the corresponding DateTime representation
		/// </summary>
		/// <param name="date">ISO-8601 conformant date</param>
		/// <param name="value">UTC or Unspecified DateTime</param>
		/// <returns>true if parsing was successful</returns>
		private bool TryParseIso8601(string date, out DateTime value)
		{
			if (!DateTime.TryParse(
				date,
				CultureInfo.InvariantCulture,
				DateTimeStyles.RoundtripKind|DateTimeStyles.AllowWhiteSpaces|DateTimeStyles.NoCurrentDateDefault,
				out value))
			{
				value = default(DateTime);
				return false;
			}

			if (value.Kind == DateTimeKind.Local)
			{
				value = value.ToUniversalTime();
			}

			return true;
		}

		/// <summary>
		/// Converts a DateTime to the corresponding ISO-8601 string representation
		/// </summary>
		/// <param name="value"></param>
		/// <returns>ISO-8601 conformant date</returns>
		private string FormatIso8601(DateTime value)
		{
			switch (value.Kind)
			{
				case DateTimeKind.Local:
				{
					value = value.ToUniversalTime();
					goto case DateTimeKind.Utc;
				}
				case DateTimeKind.Utc:
				{
					// UTC DateTime in ISO-8601
					return value.ToString(Iso8601DateFilter.UtcIso8601Format);
				}
				default:
				{
					// DateTime in ISO-8601
					return value.ToString(Iso8601DateFilter.Iso8601Format);
				}
			}
		}

		#endregion Utility Methods
	}
}
