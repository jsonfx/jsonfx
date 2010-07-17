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

namespace JsonFx.Bson
{
	#region Element Types

	public enum BsonElementType : byte
	{
		None = 0x00,

		Double = 0x01,
		String = 0x02,
		Document = 0x03,
		Array = 0x04,
		Binary = 0x05,
		[Obsolete]
		Undefined = 0x06,
		ObjectID = 0x07,
		Boolean = 0x08,
		DateTimeUtc = 0x09,
		Null = 0x0A,
		RegExp = 0x0B,
		[Obsolete]
		DBPointer = 0x0C,
		JavaScriptCode = 0x0D,
		Symbol = 0x0E,
		CodeWithScope = 0x0F,
		Int32 = 0x10,
		TimeStamp = 0x11,
		Int64 = 0x12,

		MinKey = 0xFF,
		MaxKey = 0x7F
	}

	#endregion Element Types

	#region Binary Subtypes

	public enum BsonBinarySubtype : byte
	{
		Generic = 0x00,
		Function = 0x01,
		[Obsolete]
		BinaryOld = 0x02,
		UUID = 0x03,
		MD5 = 0x05,
		UserDefined = 0x80
	}

	#endregion Binary Subtypes
}
