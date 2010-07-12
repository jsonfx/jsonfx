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

using JsonFx.Serialization;

namespace JsonFx.Bson
{
	public partial class BsonReader
	{
		/// <summary>
		/// Generates a SAX-like sequence of BSON tokens from text
		/// </summary>
		public class BsonTokenizer : IBinaryTokenizer<BsonTokenType>
		{
			#region Constants

			// tokenizing errors
			private const string ErrorUnrecognizedToken = "Illegal BSON sequence";
			private const string ErrorUnterminatedDocument = "Unterminated BSON document";

			private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
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

			private static void ReadDocument(Queue<Token<BsonTokenType>> queue, BinaryReader reader)
			{
				if (reader.PeekChar() < 0)
				{
					queue.Enqueue(BsonGrammar.TokenNone);
					return;
				}

				long start = reader.BaseStream.Position;
				int size = reader.ReadInt32();

				queue.Enqueue(BsonGrammar.TokenDocumentBegin);
				while (reader.BaseStream.Position-start < size-1)
				{
					BsonTokenizer.ReadElement(queue, reader);
				}

				if (reader.ReadByte() != 0x00)
				{
					throw new DeserializationException(BsonTokenizer.ErrorUnterminatedDocument, start);
				}
				queue.Enqueue(BsonGrammar.TokenDocumentEnd);
			}

			private static void ReadElement(Queue<Token<BsonTokenType>> queue, BinaryReader reader)
			{
				BsonElementType elemType = (BsonElementType)reader.ReadByte();
				queue.Enqueue(BsonGrammar.TokenElementType(elemType));

				string ename = BsonTokenizer.ReadCString(reader);
				queue.Enqueue(BsonGrammar.TokenCString(ename));

				switch (elemType)
				{
					case BsonElementType.Double:
					{
						double value = reader.ReadDouble();
						queue.Enqueue(BsonGrammar.TokenDouble(value));
						break;
					}
					case BsonElementType.String:
					case BsonElementType.JavaScriptCode:
					case BsonElementType.Symbol:
					{
						string value = BsonTokenizer.ReadString(reader);
						queue.Enqueue(BsonGrammar.TokenString(value));
						break;
					}
					case BsonElementType.Document:
					case BsonElementType.Array:
					{
						BsonTokenizer.ReadDocument(queue, reader);
						break;
					}
					case BsonElementType.Binary:
					{
						BsonTokenizer.ReadBinary(queue, reader);
						break;
					}
					case BsonElementType.ObjectID:
					{
						byte[] value = reader.ReadBytes(12);
						queue.Enqueue(BsonGrammar.TokenBinary(value));
						break;
					}
					case BsonElementType.Boolean:
					{
						bool value = reader.ReadByte() != 0x00;
						queue.Enqueue(value ? BsonGrammar.TokenTrue : BsonGrammar.TokenFalse);
						break;
					}
					case BsonElementType.DateTimeUtc:
					{
						DateTime value = BsonTokenizer.UnixEpoch.AddMilliseconds(reader.ReadInt64());
						queue.Enqueue(BsonGrammar.TokenUtcDateTime(value));
						break;
					}
					case BsonElementType.RegExp:
					{
						string pattern = BsonTokenizer.ReadCString(reader);
						queue.Enqueue(BsonGrammar.TokenCString(pattern));

						string options = BsonTokenizer.ReadCString(reader);
						queue.Enqueue(BsonGrammar.TokenCString(options));
						break;
					}
					case BsonElementType.DBPointer:
					{
						string value1 = BsonTokenizer.ReadString(reader);
						queue.Enqueue(BsonGrammar.TokenString(value1));

						byte[] value2 = reader.ReadBytes(12);
						queue.Enqueue(BsonGrammar.TokenBinary(value2));
						break;
					}
					case BsonElementType.CodeWithScope:
					{
						int size = reader.ReadInt32();
						string value = BsonTokenizer.ReadString(reader);

						queue.Enqueue(BsonGrammar.TokenString(value));

						BsonTokenizer.ReadDocument(queue, reader);
						break;
					}
					case BsonElementType.Int32:
					{
						int value = reader.ReadInt32();
						queue.Enqueue(BsonGrammar.TokenInt32(value));
						break;
					}
					case BsonElementType.TimeStamp:
					case BsonElementType.Int64:
					{
						long value = reader.ReadInt64();
						queue.Enqueue(BsonGrammar.TokenInt64(value));
						break;
					}
					case BsonElementType.Undefined:
					case BsonElementType.Null:
					case BsonElementType.MinKey:
					case BsonElementType.MaxKey:
					{
						break;
					}
					default:
					{
						throw new DeserializationException(ErrorUnrecognizedToken, reader.BaseStream.Position);
					}
				}
			}

			private static void ReadBinary(Queue<Token<BsonTokenType>> queue, BinaryReader reader)
			{
				int size = reader.ReadInt32();

				BsonBinarySubtype subtype = (BsonBinarySubtype)reader.ReadByte();
				queue.Enqueue(BsonGrammar.TokenBinarySubtype(subtype));

				byte[] buffer = reader.ReadBytes(size);
				queue.Enqueue(BsonGrammar.TokenBinary(buffer));
			}

			private static string ReadString(BinaryReader reader)
			{
				int size = reader.ReadInt32();

				char[] buffer = reader.ReadChars(size);

				return new String(buffer, 0, size-1);
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

			#region IBinaryTokenizer<BsonTokenType> Members

			/// <summary>
			/// Gets a token sequence from the TextReader
			/// </summary>
			/// <param name="reader"></param>
			/// <returns></returns>
			public IEnumerable<Token<BsonTokenType>> GetTokens(Stream stream)
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
			public IEnumerable<Token<BsonTokenType>> GetTokens(byte[] bytes)
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
			protected IEnumerable<Token<BsonTokenType>> GetTokens(BinaryReader reader)
			{
				if (reader == null)
				{
					throw new ArgumentNullException("reader");
				}

				this.Reader = reader;

				while (true)
				{
					Queue<Token<BsonTokenType>> queue = new Queue<Token<BsonTokenType>>();

					BsonTokenizer.ReadDocument(queue, reader);

					foreach (Token<BsonTokenType> token in queue)
					{
						if (token.TokenType == BsonTokenType.None)
						{
							reader.Dispose();
							this.Reader = BsonTokenizer.NullBinaryReader;
							yield break;
						}
						yield return token;
					}
				};
			}

			#endregion ITokenizer<BsonTokenType> Members

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
					this.Reader.Dispose();
				}
			}

			#endregion IDisposable Members
		}
	}
}
