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
using System.Globalization;

namespace JsonFx.Bson
{
	/// <summary>
	/// BSON MD5 Datatype
	/// </summary>
	public struct MD5
	{
		#region Fields

		public readonly Guid Hash;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="hash"></param>
		public MD5(string hash)
		{
			this.Hash = new Guid(hash);
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="hash"></param>
		public MD5(byte[] hash)
		{
			this.Hash = new Guid(hash);
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="hash"></param>
		public MD5(Guid hash)
		{
			this.Hash = hash;
		}

		#endregion Init

		#region Conversions

		/// <summary>
		/// Converts MD5 to Guid
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator Guid(MD5 value)
		{
			return value.Hash;
		}

		/// <summary>
		/// Converts Guid to MD5
		/// </summary>
		/// <param name="value"></param>
		/// <returns></implicit>
		public static explicit operator MD5(Guid value)
		{
			return new MD5(value);
		}

		/// <summary>
		/// Converts MD5 to Guid
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Hash.ToString();
		}

		/// <summary>
		/// Gets the hashcode of the underlying Guid
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return EqualityComparer<Guid>.Default.GetHashCode(this.Hash);
		}

		#endregion Conversions
	}

	/// <summary>
	/// BSON JavaScriptCode Datatype
	/// </summary>
	public struct JavaScriptCode
	{
		#region Fields

		public readonly string Code;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="code"></param>
		public JavaScriptCode(string code)
		{
			this.Code = code;
		}

		#endregion Init

		#region Conversions

		/// <summary>
		/// Converts JavaScriptCode to string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator string(JavaScriptCode value)
		{
			return value.Code;
		}

		/// <summary>
		/// Converts string to JavaScriptCode
		/// </summary>
		/// <param name="value"></param>
		/// <returns></implicit>
		public static explicit operator JavaScriptCode(string value)
		{
			return new JavaScriptCode(value);
		}

		/// <summary>
		/// Converts JavaScriptCode to string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Code;
		}

		/// <summary>
		/// Gets the hashcode of the underlying string
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.Code);
		}

		#endregion Conversions
	}

	/// <summary>
	/// BSON Symbol Datatype
	/// </summary>
	public struct Symbol
	{
		#region Fields

		public readonly string Code;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="code"></param>
		public Symbol(string code)
		{
			this.Code = code;
		}

		#endregion Init

		#region Conversions

		/// <summary>
		/// Converts Symbol to string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator string(Symbol value)
		{
			return value.Code;
		}

		/// <summary>
		/// Converts string to Symbol
		/// </summary>
		/// <param name="value"></param>
		/// <returns></implicit>
		public static explicit operator Symbol(string value)
		{
			return new Symbol(value);
		}

		/// <summary>
		/// Converts Symbol to string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Code;
		}

		/// <summary>
		/// Gets the hashcode of the underlying string
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.Code);
		}

		#endregion Conversions
	}

	/// <summary>
	/// BSON DBPointer Datatype (Deprecated)
	/// </summary>
	[Obsolete]
	public class DBPointer
	{
		#region Properties

		public string Namespace { get; set; }

		public ObjectID ObjectID { get; set; }

		#endregion Properties
	}

	/// <summary>
	/// Immutable BSON ObjectID Datatype
	/// </summary>
	/// <remarks>
	/// http://www.mongodb.org/display/DOCS/Object+IDs#ObjectIDs-TheBSONObjectIdDatatype
	/// </remarks>
	public struct ObjectID
	{
		#region Constants

		public static readonly ObjectID Empty = new ObjectID(new byte[0]);

		#endregion Constants

		#region Fields

		private readonly byte[] Bytes;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="bytes">12-byte object ID</param>
		public ObjectID(byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			if (bytes.Length != 12)
			{
				throw new ArgumentException("ObjectID is exact 12 bytes", "bytes");
			}

			this.Bytes = bytes;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="time">4-byte seconds since Unit epoch</param>
		/// <param name="machine">3-byte machine ID</param>
		/// <param name="pid">2-byte process ID</param>
		/// <param name="inc">3-byte counter</param>
		public ObjectID(DateTime time, int machine, int pid, int inc)
		{
			this.Bytes = new byte[12];

			// NOTE: time is stored as a BigEndian UInt16
			// http://www.mongodb.org/display/DOCS/Object+IDs#ObjectIDs-TheBSONObjectIdDatatype

			int seconds = (int)time.Subtract(BsonReader.UnixEpoch).TotalSeconds;
			if ((seconds < UInt16.MinValue) || (seconds > UInt16.MaxValue))
			{
				throw new ArgumentOutOfRangeException("ObjectID only supports a limited range of dates ("+UInt16.MaxValue+" seconds since Unix epoch).");
			}

			this.Bytes[0] = (byte)((seconds >> 0x18) % 0xFF);
			this.Bytes[1] = (byte)((seconds >> 0x10) % 0xFF);
			this.Bytes[2] = (byte)((seconds >> 0x08) % 0xFF);
			this.Bytes[3] = (byte)(seconds % 0xFF);

			// NOTE: time is stored as a BigEndian UInt16
			// http://www.mongodb.org/display/DOCS/Object+IDs#ObjectIDs-TheBSONObjectIdDatatype
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the time associated with this ObjectID
		/// </summary>
		public DateTime Time
		{
			get
			{
				// NOTE: time is stored as a 4-byte BigEndian integer
				// http://www.mongodb.org/display/DOCS/Object+IDs#ObjectIDs-TheBSONObjectIdDatatype

				int seconds = ((((this.Bytes[0] << 0x18) | (this.Bytes[1] << 0x10)) | (this.Bytes[2] << 0x08)) | this.Bytes[3]);
				return BsonReader.UnixEpoch.AddSeconds(seconds);
			}
		}

		/// <summary>
		/// Gets the machine id associated with this ObjectID
		/// </summary>
		public int Machine
		{
			get
			{
				// NOTE: machine is stored as a 3-byte LittleEndian integer
				// http://www.mongodb.org/display/DOCS/Object+IDs#ObjectIDs-TheBSONObjectIdDatatype

				return ((this.Bytes[4] | (this.Bytes[5] << 0x08)) | (this.Bytes[6] << 0x10));
			}
		}

		/// <summary>
		/// Gets the process id associated with this ObjectID
		/// </summary>
		public int Pid
		{
			get
			{
				// NOTE: machine is stored as a 2-byte LittleEndian integer
				// http://www.mongodb.org/display/DOCS/Object+IDs#ObjectIDs-TheBSONObjectIdDatatype

				return (this.Bytes[7] | (this.Bytes[8] << 8));
			}
		}

		/// <summary>
		/// Gets the counter associated with this ObjectID
		/// </summary>
		public int Inc
		{
			get
			{
				// NOTE: inc is stored as a 3-byte BigEndian integer
				// http://www.mongodb.org/display/DOCS/Object+IDs#ObjectIDs-TheBSONObjectIdDatatype

				return (((this.Bytes[9] << 0x10) | (this.Bytes[10] << 0x08)) | this.Bytes[11]);
			}
		}

		#endregion Properties

		#region Conversions

		/// <summary>
		/// Converts an ObjectID to a hex string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator string(ObjectID value)
		{
			// simply use ToString implementation
			return value.ToString();
		}

		/// <summary>
		/// Converts a hex string to an ObjectID
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator ObjectID(string value)
		{
			// simply use Parse implementation
			return ObjectID.Parse(value);
		}

		/// <summary>
		/// Converts an ObjectID to a byte array
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator byte[](ObjectID value)
		{
			return value.Bytes ?? ObjectID.Empty.Bytes;
		}

		/// <summary>
		/// Converts a byte array to an ObjectID
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator ObjectID(byte[] value)
		{
			return new ObjectID(value);
		}

		public override string ToString()
		{
			byte[] bytes = this.Bytes;

			if (bytes == null)
			{
				bytes = ObjectID.Empty.Bytes;
			}

			char[] hex = new char[24];

			for (int i=0, j=0; i<24; i+=2, j++)
			{
				hex[i] = ObjectID.GetHexDigit(bytes[j] / 0x10);
				hex[i] = ObjectID.GetHexDigit(bytes[j] % 0x10);
			}

			return new String(hex);
		}

		public byte[] ToByteArray()
		{
			byte[] bytes = new byte[12];

			Buffer.BlockCopy(this.Bytes, 0, bytes, 0, 12);

			return bytes;
		}

		public static ObjectID Parse(string value)
		{
			ObjectID result;

			if (ObjectID.TryParse(value, out result))
			{
				return result;
			}

			throw new InvalidCastException("String must be exactly 24 hex digits");
		}

		public static bool TryParse(string value, out ObjectID result)
		{
			if (String.IsNullOrEmpty(value) ||
				value.Length != 24)
			{
				result = ObjectID.Empty;
				return false;
			}

			byte[] bytes = new byte[12];

			for (int i=0; i<24; i+=2)
			{
				byte digit;
				if (!Byte.TryParse(
					value.Substring(i, 2),
					NumberStyles.AllowHexSpecifier,
					NumberFormatInfo.InvariantInfo,
					out digit))
				{
					result = ObjectID.Empty;
					return false;
				}

				bytes[i] = digit;
			}

			result = new ObjectID(bytes);
			return true;
		}

		/// <summary>
		/// Gets the hashcode of the underlying string
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return EqualityComparer<byte[]>.Default.GetHashCode(this.Bytes);
		}

		#endregion Conversions

		#region Utility Methods

		/// <summary>
		/// Gets a 4-bit number as a hex digit
		/// </summary>
		/// <param name="i">0-15</param>
		/// <returns></returns>
		private static char GetHexDigit(int i)
		{
			if (i < 10)
			{
				return (char)(i + '0');
			}

			return (char)((i - 10) + 'a');
		}

		#endregion Utility Methods
	}

	/// <summary>
	/// Generic binary holder
	/// </summary>
	/// <remarks>
	/// http://api.mongodb.org/java/2.0/org/bson/types/Binary.html
	/// </remarks>
	public class Binary : IEnumerable<byte>
	{
		#region Constants

		public static readonly Binary Empty = new Binary(BsonBinarySubtype.Generic, new byte[0]);

		#endregion Constants

		#region Fields

		private readonly BsonBinarySubtype Subtype;
		private readonly byte[] Bytes;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="subtype">binary type code</param>
		/// <param name="bytes">byte date</param>
		public Binary(BsonBinarySubtype subtype, byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}

			this.Subtype = subtype;
			this.Bytes = bytes;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the binary type code
		/// </summary>
		public BsonBinarySubtype Type
		{
			get { return this.Subtype; }
		}

		/// <summary>
		/// Gets the byte at the given index
		/// </summary>
		public byte this[int index]
		{
			get { return this.Bytes[index]; }
		}

		/// <summary>
		/// Gets the length of the binary data
		/// </summary>
		public int Count
		{
			get { return this.Bytes.Length; }
		}

		#endregion Properties

		#region Conversions

		/// <summary>
		/// Converts an ObjectID to a hex string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator string(Binary value)
		{
			// simply use ToString implementation
			return value.ToString();
		}

		/// <summary>
		/// Converts a hex string to an ObjectID
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator Binary(string value)
		{
			// simply use Parse implementation
			return Binary.Parse(value);
		}

		/// <summary>
		/// Converts an ObjectID to a byte array
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator byte[](Binary value)
		{
			return value.Bytes;
		}

		/// <summary>
		/// Converts a byte array to an ObjectID
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator Binary(byte[] value)
		{
			return new Binary(BsonBinarySubtype.Generic, value);
		}

		public override string ToString()
		{
			byte[] bytes = this.Bytes;

			char[] hex = new char[24];

			for (int i=0, j=0; i<24; i+=2, j++)
			{
				hex[i] = Binary.GetHexDigit(bytes[j] / 0x10);
				hex[i] = Binary.GetHexDigit(bytes[j] % 0x10);
			}

			return new String(hex);
		}

		public byte[] ToByteArray()
		{
			byte[] bytes = new byte[12];

			Buffer.BlockCopy(this.Bytes, 0, bytes, 0, 12);

			return bytes;
		}

		public static Binary Parse(string value)
		{
			Binary result;

			if (Binary.TryParse(value, out result))
			{
				return result;
			}

			throw new InvalidCastException("String must be only hex digits");
		}

		public static bool TryParse(string value, out Binary result)
		{
			if (String.IsNullOrEmpty(value))
			{
				result = Binary.Empty;
				return true;
			}

			byte[] bytes = new byte[value.Length / 2];

			for (int i=0; i<value.Length; i+=2)
			{
				byte digit;
				if (!Byte.TryParse(
					value.Substring(i, 2),
					NumberStyles.AllowHexSpecifier,
					NumberFormatInfo.InvariantInfo,
					out digit))
				{
					result = Binary.Empty;
					return false;
				}

				bytes[i] = digit;
			}

			result = new Binary(BsonBinarySubtype.Generic, bytes);
			return true;
		}

		/// <summary>
		/// Gets the hashcode of the underlying string
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return EqualityComparer<byte[]>.Default.GetHashCode(this.Bytes);
		}

		#endregion Conversions

		#region Utility Methods

		/// <summary>
		/// Gets a 4-bit number as a hex digit
		/// </summary>
		/// <param name="i">0-15</param>
		/// <returns></returns>
		private static char GetHexDigit(int i)
		{
			if (i < 10)
			{
				return (char)(i + '0');
			}

			return (char)((i - 10) + 'a');
		}

		#endregion Utility Methods

		#region IEnumerable<byte> Members

		public IEnumerator<byte> GetEnumerator()
		{
			return ((IEnumerable<byte>)this.Bytes).GetEnumerator();
		}

		#endregion IEnumerable<byte> Members

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Bytes.GetEnumerator();
		}

		#endregion IEnumerable Members
	}
}
