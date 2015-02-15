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

using JsonFx.IO;
using JsonFx.Serialization;

namespace JsonFx.Model.Filters
{
	/// <summary>
	/// Defines a filter for JSON-style serialization of DateTime into ISO-8601 string
	/// </summary>
	/// <remarks>
	/// This is the format used by EcmaScript 5th edition Date.prototype.toJSON(...):
	///		http://www.ecma-international.org/publications/files/ECMA-ST/ECMA-262.pdf
	///		http://www.w3.org/TR/NOTE-datetime
	///		http://en.wikipedia.org/wiki/ISO_8601
	///	
	/// NOTE: This format limits expressing DateTime as either UTC or Unspecified. Local (i.e. Server Local) is converted to UTC.
	/// </remarks>
	public class Iso8601DateFilter : ModelFilter<DateTime>
	{
		#region Precision

		/// <summary>
		/// Defines the precision of fractional seconds in ISO-8601 dates
		/// </summary>
		public enum Precision
		{
			Seconds,
			Milliseconds,
			Ticks
		}

		#endregion Precision

		#region Constants

		private const string ShortFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";
		private const string LongFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK";
		private const string FullFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFK";

		#endregion Constants

		#region Fields

		private string iso8601Format = Iso8601DateFilter.LongFormat;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Determines the precision of fractional seconds.
		/// Defaults to EcmaScript precision of milliseconds.
		/// </summary>
		public Precision Format
		{
			get
			{
				switch (this.iso8601Format)
				{
					case ShortFormat:
					{
						return Precision.Seconds;
					}
					case LongFormat:
					{
						return Precision.Milliseconds;
					}
					default:
					case FullFormat:
					{
						return Precision.Ticks;
					}
				}
			}
			set
			{
				switch (value)
				{
					case Precision.Seconds:
					{
						this.iso8601Format = Iso8601DateFilter.ShortFormat;
						break;
					}
					case Precision.Milliseconds:
					{
						this.iso8601Format = Iso8601DateFilter.LongFormat;
						break;
					}
					default:
					case Precision.Ticks:
					{
						this.iso8601Format = Iso8601DateFilter.FullFormat;
						break;
					}
				}
			}
		}

		#endregion Properties

		#region IDataFilter<DataTokenType,DateTime> Members

		public override bool TryRead(DataReaderSettings settings, IStream<Token<ModelTokenType>> tokens, out DateTime value)
		{
			Token<ModelTokenType> token = tokens.Peek();
			if (token == null ||
				token.TokenType != ModelTokenType.Primitive ||
				!(token.Value is string))
			{
				value = default(DateTime);
				return false;
			}

			if (!Iso8601DateFilter.TryParseIso8601(
				token.ValueAsString(),
				out value))
			{
				value = default(DateTime);
				return false;
			}

			tokens.Pop();
			return true;
		}

		public override bool TryWrite(DataWriterSettings settings, DateTime value, out IEnumerable<Token<ModelTokenType>> tokens)
		{
			tokens = new Token<ModelTokenType>[]
				{
					ModelGrammar.TokenPrimitive(this.FormatIso8601(value))
				};

			return true;
		}

		#endregion IDataFilter<DataTokenType,DateTime> Members

		#region Utility Methods

		/// <summary>
		/// Converts a ISO-8601 string to the corresponding DateTime representation
		/// </summary>
		/// <param name="date">ISO-8601 conformant date</param>
		/// <param name="value">UTC or Unspecified DateTime</param>
		/// <returns>true if parsing was successful</returns>
		private static bool TryParseIso8601(string date, out DateTime value)
		{
			if (!DateTime.TryParseExact(date,
				Iso8601DateFilter.FullFormat,
				CultureInfo.InvariantCulture,
				DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault,
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
			if (value.Kind == DateTimeKind.Local)
			{
				value = value.ToUniversalTime();
			}

			// DateTime in ISO-8601
			return value.ToString(this.iso8601Format);
		}

		#endregion Utility Methods
	}
}
