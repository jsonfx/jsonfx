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
using System.IO;
using System.Text;

using JsonFx.IO;
using JsonFx.Serialization;

namespace JsonFx.Bson
{
	public partial class BsonWriter
	{
		/// <summary>
		/// Outputs BSON bytes from a SAX-like input stream of BSON tokens
		/// </summary>
		public class BsonFormatter : IBinaryFormatter<BsonTokenType>
		{
			#region Format Methods

			/// <summary>
			/// Formats the token sequence as a string
			/// </summary>
			/// <param name="tokens"></param>
			public byte[] Format(IEnumerable<Token<BsonTokenType>> tokens)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				using (MemoryStream stream = new MemoryStream())
				{
					this.Format(stream, tokens);

					return stream.ToArray();
				}
			}

			/// <summary>
			/// Formats the token sequence to the output writer
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			public void Format(Stream stream, IEnumerable<Token<BsonTokenType>> tokens)
			{
				if (stream == null)
				{
					throw new ArgumentNullException("stream");
				}
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
				{
					IStream<Token<BsonTokenType>> tokenStream = new Stream<Token<BsonTokenType>>(tokens);
					if (!tokenStream.IsCompleted)
					{
						BsonFormatter.WriteDocument(writer, tokenStream);
					}
				}
			}

			#endregion Format Methods

			#region Write Methods

			/// <summary>
			/// 
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			/// <returns>number of bytes written</returns>
			private static int WriteDocument(BinaryWriter writer, IStream<Token<BsonTokenType>> tokens)
			{
				Token<BsonTokenType> token = tokens.Peek();
				if (tokens.IsCompleted || token == null ||
					token.TokenType != BsonTokenType.DocumentBegin)
				{
					throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.DocumentBegin+" token");
				}
				tokens.Pop();

				long start = writer.BaseStream.Position;
				int total = BsonReader.SizeOfInt32 + BsonReader.SizeOfByte;// length + terminal

				// leave room for length
				writer.Seek(BsonReader.SizeOfInt32, SeekOrigin.Current);

				token = tokens.Peek();
				while (!tokens.IsCompleted && token != null && token.TokenType != BsonTokenType.DocumentEnd)
				{
					total += BsonFormatter.WriteElement(writer, tokens);
					token = tokens.Peek();
				}

				// write terminal
				writer.Write(BsonReader.NullByte);

				// seek back to write out length
				long end = writer.BaseStream.Position;
				writer.Seek((int)(start-end), SeekOrigin.Current);
				writer.Write(total);

				// seek back to end
				writer.Seek((int)(end-start-BsonReader.SizeOfInt32), SeekOrigin.Current);

				return total;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			/// <returns>number of bytes written</returns>
			private static int WriteElement(BinaryWriter writer, IStream<Token<BsonTokenType>> tokens)
			{
				Token<BsonTokenType> token = tokens.Peek();
				if (tokens.IsCompleted || token == null ||
					token.TokenType != BsonTokenType.ElementType)
				{
					throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.ElementType+" token");
				}
				tokens.Pop();

				BsonElementType elemType = (BsonElementType)token.Value;

				// write element type
				writer.Write(Convert.ToByte(token.Value, CultureInfo.InvariantCulture));
				int total = BsonReader.SizeOfByte; // for element type

				token = tokens.Peek();
				if (tokens.IsCompleted || token == null ||
					token.TokenType != BsonTokenType.CString)
				{
					throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.CString+" token");
				}
				tokens.Pop();

				// write EName
				total += BsonFormatter.WriteString(writer, token);

				switch (elemType)
				{
					case BsonElementType.Double:
					{
						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.Double)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.Double+" token");
						}
						tokens.Pop();

						// write double data
						writer.Write(Convert.ToDouble(token.Value, CultureInfo.InvariantCulture));
						total += BsonReader.SizeOfDouble;
						break;
					}
					case BsonElementType.String:
					case BsonElementType.JavaScriptCode:
					case BsonElementType.Symbol:
					{
						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.String)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.String+" token");
						}
						tokens.Pop();

						// write string data
						total += BsonFormatter.WriteString(writer, token);
						break;
					}
					case BsonElementType.Document:
					case BsonElementType.Array:
					{
						total += BsonFormatter.WriteDocument(writer, tokens);
						break;
					}
					case BsonElementType.Binary:
					{
						total += BsonFormatter.WriteBinary(writer, tokens);
						break;
					}
					case BsonElementType.ObjectID:
					{
						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.ByteArray)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.ByteArray+" token");
						}
						tokens.Pop();

						// write ObjectID data
						writer.Write((byte[])token.Value);
						total += BsonReader.SizeOfObjectID;
						break;
					}
					case BsonElementType.Boolean:
					{
						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.Boolean)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.Boolean+" token");
						}
						tokens.Pop();

						// write bool data
						bool value = !BsonReader.FalseByte.Equals(Convert.ToByte(token.Value));
						writer.Write(value ? BsonReader.TrueByte : BsonReader.FalseByte);
						total += BsonReader.SizeOfByte;
						break;
					}
					case BsonElementType.DateTimeUtc:
					{
						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.UtcDateTime ||
							!(token.Value is DateTime))
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.UtcDateTime+" token");
						}
						tokens.Pop();

						DateTime value = (DateTime)token.Value;
						if (value.Kind == DateTimeKind.Local)
						{
							// convert server-local to UTC
							value = value.ToUniversalTime();
						}

						// find the duration since Jan 1, 1970
						TimeSpan duration = value.Subtract(BsonReader.UnixEpoch);

						// get the total milliseconds
						long ticks = (long)duration.TotalMilliseconds;

						// write long data
						writer.Write(Convert.ToInt64(token.Value, CultureInfo.InvariantCulture));
						total += BsonReader.SizeOfInt64;
						break;
					}
					case BsonElementType.RegExp:
					{
						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.CString)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.CString+" token");
						}
						tokens.Pop();

						// write cstring data
						total += BsonFormatter.WriteString(writer, token);

						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.CString)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.CString+" token");
						}
						tokens.Pop();

						// write cstring data
						total += BsonFormatter.WriteString(writer, token);
						break;
					}
					case BsonElementType.DBPointer:
					{
						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.String)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.String+" token");
						}
						tokens.Pop();

						// write string data
						total += BsonFormatter.WriteString(writer, token);

						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.ByteArray)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.ByteArray+" token");
						}
						tokens.Pop();

						// write bytes
						writer.Write((byte[])token.Value);
						total += BsonReader.SizeOfObjectID;
						break;
					}
					case BsonElementType.CodeWithScope:
					{
						total += BsonFormatter.WriteCodeWithScope(writer, tokens);
						break;
					}
					case BsonElementType.Int32:
					{
						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.Int32)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.Int32+" token");
						}
						tokens.Pop();

						// write int data
						writer.Write(Convert.ToInt32(token.Value, CultureInfo.InvariantCulture));
						total += BsonReader.SizeOfInt32;
						break;
					}
					case BsonElementType.TimeStamp:
					case BsonElementType.Int64:
					{
						token = tokens.Peek();
						if (tokens.IsCompleted || token == null ||
							token.TokenType != BsonTokenType.Int64)
						{
							throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.Int64+" token");
						}
						tokens.Pop();

						// write long data
						writer.Write(Convert.ToInt64(token.Value, CultureInfo.InvariantCulture));
						total += BsonReader.SizeOfInt64;
						break;
					}
					case BsonElementType.Undefined:
					case BsonElementType.Null:
					case BsonElementType.MinKey:
					case BsonElementType.MaxKey:
					{
						// no data emitted for these
						break;
					}
					default:
					{
						throw new AnalyzerException<BsonTokenType>(BsonGrammar.TokenElementType(elemType), BsonReader.ErrorUnrecognizedToken);
					}
				}

				return total;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="token"></param>
			/// <returns>number of bytes written</returns>
			private static int WriteString(BinaryWriter writer, Token<BsonTokenType> token)
			{
				int total = BsonReader.SizeOfByte; // for terminal

				string value = Convert.ToString(token.Value, CultureInfo.InvariantCulture);
				byte[] bytes = Encoding.UTF8.GetBytes(value);

				if (token.TokenType != BsonTokenType.CString)
				{
					// write length prefix
					writer.Write(bytes.Length+BsonReader.SizeOfByte);
					total += BsonReader.SizeOfInt32;
				}

				// write character data
				writer.Write(bytes);
				total += bytes.Length;

				// write terminal
				writer.Write(BsonReader.NullByte);

				return total;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			/// <returns>number of bytes written</returns>
			private static int WriteBinary(BinaryWriter writer, IStream<Token<BsonTokenType>> tokens)
			{
				Token<BsonTokenType> token = tokens.Peek();
				if (tokens.IsCompleted || token == null ||
					token.TokenType != BsonTokenType.BinarySubtype)
				{
					throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.BinarySubtype+" token");
				}
				tokens.Pop();

				BsonBinarySubtype subtype = (BsonBinarySubtype)token.Value;

				token = tokens.Peek();
				if (tokens.IsCompleted || token == null ||
					token.TokenType != BsonTokenType.ByteArray ||
					!(token.Value is byte[]))
				{
					throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.ByteArray+" token");
				}
				tokens.Pop();

				byte[] bytes = (byte[])token.Value;

				// write length
				writer.Write(bytes.Length);

				// write subtype
				writer.Write((byte)subtype);

				// write binary data
				writer.Write(bytes);

				// length + subtype + bytes
				return BsonReader.SizeOfInt64 + BsonReader.SizeOfByte + bytes.Length;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			/// <returns>number of bytes written</returns>
			private static int WriteCodeWithScope(BinaryWriter writer, IStream<Token<BsonTokenType>> tokens)
			{
				long start = writer.BaseStream.Position;
				int total = BsonReader.SizeOfInt32;// code_w_s length

				// leave room for length
				writer.Seek(BsonReader.SizeOfInt32, SeekOrigin.Current);

				Token<BsonTokenType>  token = tokens.Peek();
				if (tokens.IsCompleted || token == null ||
					token.TokenType != BsonTokenType.String)
				{
					throw new AnalyzerException<BsonTokenType>(token, "Expected "+BsonTokenType.String+" token");
				}
				tokens.Pop();

				// write code
				total += BsonFormatter.WriteString(writer, token);

				// write scope
				total += BsonFormatter.WriteDocument(writer, tokens);

				// seek back to write out code_w_s length
				long end = writer.BaseStream.Position;
				writer.Seek((int)(start-end), SeekOrigin.Current);
				writer.Write(total);

				// seek back to end
				writer.Seek((int)(end-start-BsonReader.SizeOfInt32), SeekOrigin.Current);

				return total;
			}

			#endregion Write Methods
		}
	}
}
