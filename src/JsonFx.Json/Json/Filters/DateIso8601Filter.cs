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

namespace JsonFx.Json
{
	/// <summary>
	/// Defines a filter for JSON serialization of DateTime
	/// </summary>
	public class DateIso8601Filter : IJsonFilter<DateTime>
	{
		#region IDataFilter<JsonTokenType,DateTime> Members

		public bool TryRead(IEnumerable<Token<JsonTokenType>> tokens, out DateTime value)
		{
			// TODO: determine MoveNext or not?
			Token<JsonTokenType> token = tokens.GetEnumerator().Current;

			return DateTime.TryParse(
				(string)token.Value,
				DateTimeFormatInfo.InvariantInfo,
				DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault,
				out value);
		}

		public bool TryWrite(DateTime value, out IEnumerable<Token<JsonTokenType>> tokens)
		{
			tokens = new Token<JsonTokenType>[]
				{
					JsonGrammar.TokenString(this.FormatIso8601(value))
				};

			return true;
		}

		#endregion IDataFilter<JsonTokenType,DateTime> Members

		#region IDataFilter<JsonTokenType> Members

		public bool TryRead(IEnumerable<Token<JsonTokenType>> tokens, out object value)
		{
			DateTime dateTime;
			if (this.TryRead(tokens, out dateTime))
			{
				value = dateTime;
				return true;
			}
			else
			{
				value = dateTime;
				return false;
			}
		}

		public bool TryWrite(object value, out IEnumerable<Token<JsonTokenType>> tokens)
		{
			tokens = null;

			return (value is DateTime) && (this.TryWrite((DateTime)value, out tokens));
		}

		#endregion IDataFilter<JsonTokenType> Members

		#region Utility Methods

		/// <summary>
		/// Converts a DateTime to the ISO-8601 string representation
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
					return String.Format("{0:s}Z", value);
				}
				default:
				{
					// DateTime in ISO-8601
					return String.Format("{0:s}", value);
				}
			}
		}

		#endregion Utility Methods
	}
}
