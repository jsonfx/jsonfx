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

using JsonFx.Model;
using JsonFx.Serialization;

namespace JsonFx.Bson
{
	public partial class BsonReader
	{
		/// <summary>
		/// Generates a sequence of tokens from BSON bytes
		/// </summary>
		public class BsonTokenizer : IBinaryTokenizer<ModelTokenType>
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

			private static void ReadDocument(List<Token<ModelTokenType>> tokens, BinaryReader reader, bool isArray)
			{
				if (reader.PeekChar() < 0)
				{
					return;
				}

				long start = reader.BaseStream.Position;
				int size = reader.ReadInt32();

				tokens.Add(isArray ? ModelGrammar.TokenArrayBeginUnnamed : ModelGrammar.TokenObjectBeginUnnamed);

				while (reader.BaseStream.Position-start < size-1)
				{
					BsonTokenizer.ReadElement(tokens, reader, isArray);
				}

				if (reader.ReadByte() != 0x00)
				{
					throw new DeserializationException(BsonWriter.ErrorUnterminated, start);
				}

				tokens.Add(isArray ? ModelGrammar.TokenArrayEnd : ModelGrammar.TokenObjectEnd);
			}

			private static void ReadElement(List<Token<ModelTokenType>> tokens, BinaryReader reader, bool isArrayItem)
			{
				BsonElementType elemType = (BsonElementType)reader.ReadByte();

				string ename = BsonTokenizer.ReadCString(reader);
				if (!isArrayItem)
				{
					tokens.Add(ModelGrammar.TokenProperty(ename));
				}

				switch (elemType)
				{
					case BsonElementType.Double:
					{
						double value = reader.ReadDouble();
						tokens.Add(ModelGrammar.TokenPrimitive(value));
						break;
					}
					case BsonElementType.String:
					{
						string value = BsonTokenizer.ReadString(reader);
						tokens.Add(ModelGrammar.TokenPrimitive(value));
						break;
					}
					case BsonElementType.JavaScriptCode:
					{
						BsonJavaScriptCode value = (BsonJavaScriptCode)BsonTokenizer.ReadString(reader);
						tokens.Add(ModelGrammar.TokenPrimitive(value));
						break;
					}
					case BsonElementType.Symbol:
					{
						BsonSymbol value = (BsonSymbol)BsonTokenizer.ReadString(reader);
						tokens.Add(ModelGrammar.TokenPrimitive(value));
						break;
					}
					case BsonElementType.Document:
					{
						BsonTokenizer.ReadDocument(tokens, reader, false);
						break;
					}
					case BsonElementType.Array:
					{
						BsonTokenizer.ReadDocument(tokens, reader, true);
						break;
					}
					case BsonElementType.Binary:
					{
						BsonTokenizer.ReadBinary(tokens, reader);
						break;
					}
					case BsonElementType.ObjectID:
					{
						byte[] value = reader.ReadBytes(BsonWriter.SizeOfObjectID);
						tokens.Add(ModelGrammar.TokenPrimitive(new BsonObjectID(value)));
						break;
					}
					case BsonElementType.Boolean:
					{
						bool value = reader.ReadByte() != BsonWriter.FalseByte;
						tokens.Add(value ? ModelGrammar.TokenTrue : ModelGrammar.TokenFalse);
						break;
					}
					case BsonElementType.DateTimeUtc:
					{
						DateTime value = BsonWriter.UnixEpoch.AddMilliseconds(reader.ReadInt64());
						tokens.Add(ModelGrammar.TokenPrimitive(value));
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

						tokens.Add(ModelGrammar.TokenPrimitive(regex));
						break;
					}
					case BsonElementType.DBPointer:
					{
						string value1 = BsonTokenizer.ReadString(reader);
						byte[] value2 = reader.ReadBytes(BsonWriter.SizeOfObjectID);

						BsonDBPointer pointer = new BsonDBPointer { Namespace=value1, ObjectID=new BsonObjectID(value2) };
						tokens.Add(ModelGrammar.TokenPrimitive(pointer));
						break;
					}
					case BsonElementType.CodeWithScope:
					{
						int size = reader.ReadInt32();
						string value = BsonTokenizer.ReadString(reader);

						tokens.Add(ModelGrammar.TokenPrimitive(value));

						BsonTokenizer.ReadDocument(tokens, reader, false);
						break;
					}
					case BsonElementType.Int32:
					{
						int value = reader.ReadInt32();
						tokens.Add(ModelGrammar.TokenPrimitive(value));
						break;
					}
					case BsonElementType.TimeStamp:
					{
						long value = reader.ReadInt64();
						// TODO: convert to TimeSpan?
						tokens.Add(ModelGrammar.TokenPrimitive(value));
						break;
					}
					case BsonElementType.Int64:
					{
						long value = reader.ReadInt64();
						tokens.Add(ModelGrammar.TokenPrimitive(value));
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

			private static void ReadBinary(List<Token<ModelTokenType>> tokens, BinaryReader reader)
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

				tokens.Add(ModelGrammar.TokenPrimitive(value));
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

			#region IBinaryTokenizer<ModelTokenType> Members

			/// <summary>
			/// Gets a token sequence from the TextReader
			/// </summary>
			/// <param name="reader"></param>
			/// <returns></returns>
			public IEnumerable<Token<ModelTokenType>> GetTokens(Stream stream)
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
			public IEnumerable<Token<ModelTokenType>> GetTokens(byte[] bytes)
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
			protected IEnumerable<Token<ModelTokenType>> GetTokens(BinaryReader reader)
			{
				if (reader == null)
				{
					throw new ArgumentNullException("reader");
				}

				this.Reader = reader;
				using (reader)
				{
					List<Token<ModelTokenType>> tokens = new List<Token<ModelTokenType>>();

					BsonTokenizer.ReadDocument(tokens, reader, false);

					this.Reader = BsonTokenizer.NullBinaryReader;
					return tokens;
				};
			}

			#endregion IBinaryTokenizer<ModelTokenType> Members

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
