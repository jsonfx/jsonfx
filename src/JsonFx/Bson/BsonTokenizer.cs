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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using JsonFx.Common;
using JsonFx.Serialization;

namespace JsonFx.Bson
{
	public partial class BsonReader
	{
		/// <summary>
		/// Generates a SAX-like sequence of tokens from BSON bytes
		/// </summary>
		public class BsonTokenizer : IBinaryTokenizer<CommonTokenType>
		{
			#region Constants

			// error messages
			private const string ErrorUnexpectedElementType = "Unexpected element type ({0})";

			private static readonly BinaryReader NullBinaryReader = new BinaryReader(Stream.Null, Encoding.UTF8);

			#endregion Constants

			#region Fields

			private BinaryReader Reader = BsonTokenizer.NullBinaryReader;

			#endregion Fields

			#region Properties

			/// <summary>
			/// Gets the current position of the underlying stream
			/// </summary>
			public long Index
			{
				get { return this.Reader.BaseStream.Position; }
			}

			#endregion Properties

			#region Scanning Methods

			private static void ReadDocument(List<Token<CommonTokenType>> queue, BinaryReader reader, bool isArray)
			{
				if (reader.PeekChar() < 0)
				{
					return;
				}

				long start = reader.BaseStream.Position;
				int size = reader.ReadInt32();

				queue.Add(isArray ? CommonGrammar.TokenArrayBegin() : CommonGrammar.TokenObjectBegin());

				bool needsValueDelim = false;
				while (reader.BaseStream.Position-start < size-1)
				{
					if (needsValueDelim)
					{
						queue.Add(CommonGrammar.TokenValueDelim);
					}
					else
					{
						needsValueDelim = true;
					}

					BsonTokenizer.ReadElement(queue, reader, isArray);
				}

				if (reader.ReadByte() != 0x00)
				{
					throw new DeserializationException(BsonWriter.ErrorUnterminated, start);
				}

				queue.Add(isArray ? CommonGrammar.TokenArrayEnd : CommonGrammar.TokenObjectEnd);
			}

			private static void ReadElement(List<Token<CommonTokenType>> queue, BinaryReader reader, bool isArrayItem)
			{
				BsonElementType elemType = (BsonElementType)reader.ReadByte();

				string ename = BsonTokenizer.ReadCString(reader);
				if (!isArrayItem)
				{
					queue.Add(CommonGrammar.TokenProperty(ename));
				}

				switch (elemType)
				{
					case BsonElementType.Double:
					{
						double value = reader.ReadDouble();
						queue.Add(CommonGrammar.TokenValue(value));
						break;
					}
					case BsonElementType.String:
					{
						string value = BsonTokenizer.ReadString(reader);
						queue.Add(CommonGrammar.TokenValue(value));
						break;
					}
					case BsonElementType.JavaScriptCode:
					{
						BsonJavaScriptCode value = (BsonJavaScriptCode)BsonTokenizer.ReadString(reader);
						queue.Add(CommonGrammar.TokenValue(value));
						break;
					}
					case BsonElementType.Symbol:
					{
						BsonSymbol value = (BsonSymbol)BsonTokenizer.ReadString(reader);
						queue.Add(CommonGrammar.TokenValue(value));
						break;
					}
					case BsonElementType.Document:
					{
						BsonTokenizer.ReadDocument(queue, reader, false);
						break;
					}
					case BsonElementType.Array:
					{
						BsonTokenizer.ReadDocument(queue, reader, true);
						break;
					}
					case BsonElementType.Binary:
					{
						BsonTokenizer.ReadBinary(queue, reader);
						break;
					}
					case BsonElementType.ObjectID:
					{
						byte[] value = reader.ReadBytes(BsonWriter.SizeOfObjectID);
						queue.Add(CommonGrammar.TokenValue(new BsonObjectID(value)));
						break;
					}
					case BsonElementType.Boolean:
					{
						bool value = reader.ReadByte() != BsonWriter.FalseByte;
						queue.Add(value ? CommonGrammar.TokenTrue : CommonGrammar.TokenFalse);
						break;
					}
					case BsonElementType.DateTimeUtc:
					{
						DateTime value = BsonWriter.UnixEpoch.AddMilliseconds(reader.ReadInt64());
						queue.Add(CommonGrammar.TokenValue(value));
						break;
					}
					case BsonElementType.RegExp:
					{
						string pattern = BsonTokenizer.ReadCString(reader);

						string optionsStr = BsonTokenizer.ReadCString(reader);

						RegexOptions options = RegexOptions.ECMAScript;

						for (int i=optionsStr.Length-1; i>=0; i--)
						{
							char ch = optionsStr[i];
							switch (ch)
							{
								case 'g':
								{
									// TODO: ensure correct encoding of ^$
									//options |= RegexOptions.Multiline;
									break;
								}
								case 'i':
								{
									options |= RegexOptions.IgnoreCase;
									break;
								}
								case 'm':
								{
									options |= RegexOptions.Multiline;
									break;
								}
							}
						}

						Regex regex = new Regex(pattern, options);

						queue.Add(CommonGrammar.TokenValue(regex));
						break;
					}
					case BsonElementType.DBPointer:
					{
						string value1 = BsonTokenizer.ReadString(reader);
						byte[] value2 = reader.ReadBytes(BsonWriter.SizeOfObjectID);

						BsonDBPointer pointer = new BsonDBPointer { Namespace=value1, ObjectID=new BsonObjectID(value2) };
						queue.Add(CommonGrammar.TokenValue(pointer));
						break;
					}
					case BsonElementType.CodeWithScope:
					{
						int size = reader.ReadInt32();
						string value = BsonTokenizer.ReadString(reader);

						queue.Add(CommonGrammar.TokenValue(value));

						BsonTokenizer.ReadDocument(queue, reader, false);
						break;
					}
					case BsonElementType.Int32:
					{
						int value = reader.ReadInt32();
						queue.Add(CommonGrammar.TokenValue(value));
						break;
					}
					case BsonElementType.TimeStamp:
					{
						long value = reader.ReadInt64();
						// TODO: convert to TimeSpan?
						queue.Add(CommonGrammar.TokenValue(value));
						break;
					}
					case BsonElementType.Int64:
					{
						long value = reader.ReadInt64();
						queue.Add(CommonGrammar.TokenValue(value));
						break;
					}
					case BsonElementType.Undefined:
					case BsonElementType.Null:
					case BsonElementType.MinKey:
					case BsonElementType.MaxKey:
					{
						// no data value
						break;
					}
					default:
					{
						throw new DeserializationException(
							String.Format(BsonTokenizer.ErrorUnexpectedElementType, elemType),
							reader.BaseStream.Position);
					}
				}
			}

			private static void ReadBinary(List<Token<CommonTokenType>> queue, BinaryReader reader)
			{
				int size = reader.ReadInt32();

				BsonBinarySubtype subtype = (BsonBinarySubtype)reader.ReadByte();

				byte[] buffer = reader.ReadBytes(size);

				object value;
				switch (subtype)
				{
					case BsonBinarySubtype.MD5:
					{
						if (size != 16)
						{
							goto default;
						}
						value = new BsonMD5(buffer);
						break;
					}
					case BsonBinarySubtype.UUID:
					{
						if (size != 16)
						{
							goto default;
						}
						value = new Guid(buffer);
						break;
					}
					case BsonBinarySubtype.BinaryOld:
					{
						// Binary (Old):
						// "The structure of the binary data (this byte* array in the binary non-terminal) must be an int32 followed by a (byte*)."
						// http://bsonspec.org/#/specification
						size = BitConverter.ToInt32(buffer, 0);

						// trim Int32 size off front of array
						byte[] temp = new byte[size];
						Buffer.BlockCopy(buffer, 4, temp, 0, size);

						// since obsolete, convert to generic
						value = new BsonBinary(BsonBinarySubtype.Generic, temp);
						break;
					}
					case BsonBinarySubtype.Function:
					case BsonBinarySubtype.Generic:
					case BsonBinarySubtype.UserDefined:
					default:
					{
						// TODO: convert Function accordingly
						value = new BsonBinary(subtype, buffer);
						break;
					}
				}

				queue.Add(CommonGrammar.TokenValue(value));
			}

			private static string ReadString(BinaryReader reader)
			{
				int size = reader.ReadInt32();

				char[] buffer = reader.ReadChars(size);

				// trim null char
				return new String(buffer, 0, size-BsonWriter.SizeOfByte);
			}

			private static string ReadCString(BinaryReader reader)
			{
				// TODO: rebuild this using byte[] buffered reads
				StringBuilder buffer = new StringBuilder();

				char ch;
				while ('\0' != (ch = reader.ReadChar()))
				{
					buffer.Append(ch);
				}

				return buffer.ToString();
			}

			#endregion Scanning Methods

			#region IBinaryTokenizer<CommonTokenType> Members

			/// <summary>
			/// Gets a token sequence from the TextReader
			/// </summary>
			/// <param name="reader"></param>
			/// <returns></returns>
			public IEnumerable<Token<CommonTokenType>> GetTokens(Stream stream)
			{
				if (stream == null)
				{
					throw new ArgumentNullException("stream");
				}

				return this.GetTokens(new BinaryReader(stream, Encoding.UTF8));
			}

			/// <summary>
			/// Gets a token sequence from the string
			/// </summary>
			/// <param name="text"></param>
			/// <returns></returns>
			public IEnumerable<Token<CommonTokenType>> GetTokens(byte[] bytes)
			{
				if (bytes == null)
				{
					throw new ArgumentNullException("bytes");
				}

				return this.GetTokens(new MemoryStream(bytes, false));
			}

			/// <summary>
			/// Gets a token sequence from the reader
			/// </summary>
			/// <param name="reader"></param>
			/// <returns></returns>
			protected IEnumerable<Token<CommonTokenType>> GetTokens(BinaryReader reader)
			{
				if (reader == null)
				{
					throw new ArgumentNullException("reader");
				}

				this.Reader = reader;
				using (reader)
				{
					List<Token<CommonTokenType>> queue = new List<Token<CommonTokenType>>();

					BsonTokenizer.ReadDocument(queue, reader, false);

					this.Reader = BsonTokenizer.NullBinaryReader;
					return queue;
				};
			}

			#endregion IBinaryTokenizer<CommonTokenType> Members

			#region IDisposable Members

			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
					((IDisposable)this.Reader).Dispose();
				}
			}

			#endregion IDisposable Members
		}
	}
}
