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
using System.Text;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Represents a single immutable token in an input sequence
	/// </summary>
	public sealed class Token<T>
	{
		#region Constants

		private const string FullDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFK";

		#endregion Constants

		#region Fields

		/// <summary>
		/// The type of the token
		/// </summary>
		public readonly T TokenType;

		/// <summary>
		/// The name of the token
		/// </summary>
		public readonly DataName Name;

		/// <summary>
		/// The value of the token
		/// </summary>
		public readonly object Value;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenType"></param>
		public Token(T tokenType)
		{
			this.TokenType = tokenType;
			this.Name = DataName.Empty;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenType"></param>
		/// <param name="value"></param>
		public Token(T tokenType, object value)
		{
			this.TokenType = tokenType;
			this.Value = value;
			this.Name = DataName.Empty;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenType"></param>
		/// <param name="name"></param>
		public Token(T tokenType, DataName name)
		{
			this.TokenType = tokenType;
			this.Name = name;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenType"></param>
		/// <param name="name"></param>
		public Token(T tokenType, DataName name, object value)
		{
			this.TokenType = tokenType;
			this.Name = name;
			this.Value = value;
		}

		#endregion Init

		#region Object Overrides

		/// <summary>
		/// Returns a string that represents the current token.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("{ ");
			builder.Append(this.TokenType);
			if (!this.Name.IsEmpty)
			{
				builder.Append("=");
				builder.Append(this.Name);
			}
			if (this.Value != null)
			{
				builder.Append("=");
				builder.Append(this.ValueAsString());
			}
			builder.Append(" }");

			return builder.ToString();
		}

		public override bool Equals(object obj)
		{
			Token<T> that = obj as Token<T>;
			if (that == null)
			{
				return false;
			}

			return this.Equals(that);
		}

		public bool Equals(Token<T> that)
		{
			if (that == null)
			{
				return false;
			}

			return
				EqualityComparer<T>.Default.Equals(this.TokenType, that.TokenType) &&
				EqualityComparer<DataName>.Default.Equals(this.Name, that.Name) &&
				EqualityComparer<object>.Default.Equals(this.Value, that.Value);
		}

		public override int GetHashCode()
		{
			// equivalent to new { this.TokenType, this.Name, this.Value }.GetHashCode()

			const int ShiftValue = -1521134295;

			int hashcode = 0x43F0F47E;
			hashcode = (ShiftValue * hashcode) + EqualityComparer<T>.Default.GetHashCode(this.TokenType);
			hashcode = (ShiftValue * hashcode) + EqualityComparer<DataName>.Default.GetHashCode(this.Name);
			hashcode = (ShiftValue * hashcode) + EqualityComparer<object>.Default.GetHashCode(this.Value);
			return hashcode;
		}

		#endregion Object Overrides

		#region Operators

		public static bool operator ==(Token<T> a, Token<T> b)
		{
			if (Object.ReferenceEquals(a, null))
			{
				return Object.ReferenceEquals(b, null);
			}

			return a.Equals(b);
		}

		public static bool operator !=(Token<T> a, Token<T> b)
		{
			return !(a == b);
		}

		#endregion Operators

		#region Utility Methods

		/// <summary>
		/// Gets the value of the token as a string
		/// </summary>
		/// <returns></returns>
		public string ValueAsString()
		{
			return Token<T>.ToString(this.Value);
		}

		/// <summary>
		/// Converts a value to a string giving opportunity for IConvertible, IFormattable
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToString(object value)
		{
			Type type = (value == null) ? null : value.GetType();

			if (type != null && type.IsEnum)
			{
				return ((Enum)value).ToString("F");
			}

			// explicitly control BCL primitives
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean:
				{
					return true.Equals(value) ? "true" : "false";
				}
				case TypeCode.Byte:
				{
					return ((byte)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.Char:
				{
					return Char.ToString((char)value);
				}
				case TypeCode.DateTime:
				{
					// default unhandled dates to ISO-8601 with full precision
					return ((DateTime)value).ToString(Token<T>.FullDateTimeFormat);
				}
				case TypeCode.DBNull:
				case TypeCode.Empty:
				{
					return String.Empty;
				}
				case TypeCode.Decimal:
				{
					return ((decimal)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.Double:
				{
					return ((double)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.Int16:
				{
					return ((short)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.Int32:
				{
					return ((int)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.Int64:
				{
					return ((long)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.SByte:
				{
					return ((sbyte)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.Single:
				{
					return ((float)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.String:
				{
					return (string)value;
				}
				case TypeCode.UInt16:
				{
					return ((ushort)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.UInt32:
				{
					return ((uint)value).ToString("g", CultureInfo.InvariantCulture);
				}
				case TypeCode.UInt64:
				{
					return ((ulong)value).ToString("g", CultureInfo.InvariantCulture);
				}
			}

			// allow IConvertable and IFormattable next chance
			IConvertible convertible = value as IConvertible;
			if (convertible != null)
			{
				return convertible.ToString(CultureInfo.InvariantCulture);
			}

			IFormattable formattable = value as IFormattable;
			if (formattable != null)
			{
				return formattable.ToString(null, CultureInfo.InvariantCulture);
			}

			// try to use any explicit cast operators
			return (value as string) ?? value.ToString();
		}

		/// <summary>
		/// Converts token to a token of a different type
		/// </summary>
		/// <typeparam name="TOther"></typeparam>
		/// <returns>token with same values and different type</returns>
		public Token<TOther> ChangeType<TOther>(TOther tokenType)
		{
			return new Token<TOther>(tokenType, this.Name, this.Value);
		}

		#endregion Utility Methods
	}
}
