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
using System.Text.RegularExpressions;

using JsonFx.Serialization;

namespace JsonFx.Json.Filters
{
	/// <summary>
	/// Defines a filter for JSON serialization of DateTime into an ASP.NET Ajax Date string.
	/// </summary>
	/// <remarks>
	/// This is the format used by Microsoft ASP.NET Ajax:
	/// http://weblogs.asp.net/bleroy/archive/2008/01/18/dates-and-json.aspx
	/// </remarks>
	public class DateAspNetAjaxFilter : JsonFilter<DateTime>
	{
		#region Constant

		private static readonly DateTime EcmaScriptEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		private const string DateCtorPattern = @"^\\/Date\((\d+?)\)\\/$";
		private static readonly Regex DateCtorRegex = new Regex(DateCtorPattern, RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.ECMAScript);

		private const string DateCtorPrefix = @"\/Date(";
		private const string DateCtorSuffix = @")\/";

		#endregion Constant

		#region IDataFilter<JsonTokenType,DateTime> Members

		public override bool TryRead(DataReaderSettings settings, IEnumerable<Token<JsonTokenType>> tokens, out DateTime value)
		{
			// TODO: determine MoveNext or not?
			Token<JsonTokenType> token = tokens.GetEnumerator().Current;

			string date = Convert.ToString(token.Value, CultureInfo.InvariantCulture);
			return this.TryParseAspNetAjaxDate(date, out value);
		}

		public override bool TryWrite(DataWriterSettings settings, DateTime value, out IEnumerable<Token<JsonTokenType>> tokens)
		{
			tokens = new Token<JsonTokenType>[]
				{
					JsonGrammar.TokenString(this.FormatAspNetAjaxDate(value))
				};

			return true;
		}

		#endregion IDataFilter<JsonTokenType,DateTime> Members

		#region Utility Methods

		/// <summary>
		/// Converts an ASP.NET Ajax date string to the corresponding DateTime representation
		/// </summary>
		/// <param name="date">Date constructor string</param>
		/// <param name="value"></param>
		/// <returns>true if parsing was successful</returns>
		private bool TryParseAspNetAjaxDate(string date, out DateTime value)
		{
			if (String.IsNullOrEmpty(date))
			{
				value = default(DateTime);
				return false;
			}

			Match match = DateAspNetAjaxFilter.DateCtorRegex.Match(date);

			long ticks;
			if (!match.Success ||
				!Int64.TryParse(
					match.Value,
					NumberStyles.Integer,
					CultureInfo.InvariantCulture,
					out ticks))
			{
				value = default(DateTime);
				return false;
			}

			value = DateAspNetAjaxFilter.EcmaScriptEpoch.AddMilliseconds(ticks);
			return true;
		}

		/// <summary>
		/// Converts a DateTime to the corresponding ASP.NET Ajax date string representation
		/// </summary>
		/// <param name="value"></param>
		/// <returns>ASP.NET Ajax date string</returns>
		private string FormatAspNetAjaxDate(DateTime value)
		{
			if (value.Kind == DateTimeKind.Local)
			{
				// convert server-local to UTC
				value = value.ToUniversalTime();
			}

			// find the time since Jan 1, 1970
			TimeSpan duration = value.Subtract(DateAspNetAjaxFilter.EcmaScriptEpoch);

			// get the total milliseconds
			long ticks = (long)duration.TotalMilliseconds;

			// write out as a pseudo Date constructor
			return String.Concat(
				DateAspNetAjaxFilter.DateCtorPrefix,
				ticks,
				DateAspNetAjaxFilter.DateCtorSuffix);
		}

		#endregion Utility Methods
	}
}
