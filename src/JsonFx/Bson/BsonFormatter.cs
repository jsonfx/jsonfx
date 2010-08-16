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
using System.Text.RegularExpressions;

using JsonFx.IO;
using JsonFx.Model;
using JsonFx.Serialization;

namespace JsonFx.Bson
{
	public partial class BsonWriter
	{
		#region Constants

		// error messages
		internal const string ErrorUnterminated = "Unterminated document";
		private const string ErrorUnexpectedToken = "Unexpected token ({0})";
		private const string ErrorExpectedObjectValueDelim = "Expected value delimiter or end of document ({0})";

		internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		internal const int SizeOfByte = 1;
		internal const int SizeOfInt32 = 4;
		internal const int SizeOfInt64 = 8;
		internal const int SizeOfDouble = 8;
		internal const int SizeOfObjectID = 12;

		internal const byte NullByte = 0;
		internal const byte FalseByte = 0;
		internal const byte TrueByte = 1;

		#endregion Constants

		/// <summary>
		/// Outputs BSON bytes from an input stream of tokens
		/// </summary>
		public class BsonFormatter : IBinaryFormatter<ModelTokenType>
		{
			#region Format Methods

			/// <summary>
			/// Formats the token sequence as a string
			/// </summary>
			/// <param name="tokens"></param>
			public byte[] Format(IEnumerable<Token<ModelTokenType>> tokens)
			{
				if (tokens == null)
				{
					throw new ArgumentNullException("tokens");
				}

				using (MemoryStream stream = new MemoryStream())
				{
					this.Format(tokens, stream);

					return stream.ToArray();
				}
			}

			/// <summary>
			/// Formats the token sequence to the output writer
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			public void Format(IEnumerable<Token<ModelTokenType>> tokens, Stream stream)
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
					IStream<Token<ModelTokenType>> tokenStream = Stream<Token<ModelTokenType>>.Create(tokens);
					if (!tokenStream.IsCompleted)
					{
						this.WriteDocument(writer, tokenStream);
					}
				}
			}

			#endregion Format Methods

			#region Write Methods

			/// <summary>
			/// Emits a document (or array) to the binary stream
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			/// <returns>number of bytes written</returns>
			private int WriteDocument(BinaryWriter writer, IStream<Token<ModelTokenType>> tokens)
			{
				Token<ModelTokenType> token = tokens.Peek();
				if (tokens.IsCompleted || token == null)
				{
					throw new TokenException<ModelTokenType>(token, BsonWriter.ErrorUnterminated);
				}

				bool isArray;
				if (token.TokenType == ModelTokenType.ArrayBegin)
				{
					isArray = true;
				}
				else if (token.TokenType == ModelTokenType.ObjectBegin)
				{
					isArray = false;
				}
				else
				{
					// root must be a document
					throw new TokenException<ModelTokenType>(token,
						String.Format(BsonWriter.ErrorUnexpectedToken, token.TokenType));
				}
				tokens.Pop();

				long start = writer.BaseStream.Position;
				int total = BsonWriter.SizeOfInt32 + BsonWriter.SizeOfByte;// length + terminal

				// leave room for length
				writer.Seek(BsonWriter.SizeOfInt32, SeekOrigin.Current);

				token = tokens.Peek();

				int count = 0;
				while (!tokens.IsCompleted && (token != null))
				{
					if (isArray)
					{
						if (token.TokenType == ModelTokenType.ArrayEnd)
						{
							// consume closing
							tokens.Pop();
							break;
						}
					}
					else
					{
						if (token.TokenType == ModelTokenType.ObjectEnd)
						{
							// consume closing
							tokens.Pop();
							break;
						}
					}

					string ename;
					if (isArray)
					{
						ename = count.ToString(CultureInfo.InvariantCulture);
					}
					else
					{
						if (token.TokenType != ModelTokenType.Property)
						{
							throw new TokenException<ModelTokenType>(token,
								String.Format(BsonWriter.ErrorUnexpectedToken, token.TokenType));
						}

						ename = token.Name.LocalName;

						// consume property name
						tokens.Pop();
					}

					total += this.WriteElement(writer, tokens, ename);
					token = tokens.Peek();
					count++;
				}

				// write terminal
				writer.Write(BsonWriter.NullByte);

				// seek back to write out length
				long end = writer.BaseStream.Position;
				writer.Seek((int)(start-end), SeekOrigin.Current);
				writer.Write(total);

				// seek back to end
				writer.Seek((int)(end-start-BsonWriter.SizeOfInt32), SeekOrigin.Current);

				return total;
			}

			/// <summary>
			/// Emits a single element to the binary stream
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			/// <param name="ename"></param>
			/// <returns>number of bytes written</returns>
			private int WriteElement(BinaryWriter writer, IStream<Token<ModelTokenType>> tokens, string ename)
			{
				Token<ModelTokenType> token = tokens.Peek();
				if (tokens.IsCompleted || token == null)
				{
					throw new TokenException<ModelTokenType>(token, BsonWriter.ErrorUnterminated);
				}

				BsonElementType elemType;
				switch (token.TokenType)
				{
					case ModelTokenType.ArrayBegin:
					{
						elemType = BsonElementType.Array;
						break;
					}
					case ModelTokenType.ObjectBegin:
					{
						elemType = BsonElementType.Document;
						break;
					}
					case ModelTokenType.Primitive:
					{
						elemType = BsonFormatter.GetElementType(token.Value);
						break;
					}
					default:
					{
						// the rest are invalid states
						throw new TokenException<ModelTokenType>(token,
							String.Format(BsonWriter.ErrorUnexpectedToken, token.TokenType));
					}
				}

				// write element type
				writer.Write((byte)elemType);
				int total = BsonWriter.SizeOfByte; // for element type

				// write EName
				total += BsonFormatter.WriteString(writer, ename, true);

				IBsonFormattable formattable = token.Value as IBsonFormattable;
				if (formattable != null)
				{
					total += formattable.Format(this, writer);
				}
				else
				{
					switch (elemType)
					{
						case BsonElementType.Double:
						{
							// consume token value
							tokens.Pop();

							// write double data
							writer.Write((double)token.Value);
							total += BsonWriter.SizeOfDouble;
							break;
						}
						case BsonElementType.String:
						case BsonElementType.JavaScriptCode:
						case BsonElementType.Symbol:
						{
							// consume token value
							tokens.Pop();

							// write as string data
							total += BsonFormatter.WriteString(writer, token.ValueAsString(), false);
							break;
						}
						case BsonElementType.Document:
						case BsonElementType.Array:
						{
							// delegate property to sub-document
							total += this.WriteDocument(writer, tokens);
							break;
						}
						case BsonElementType.Binary:
						{
							// consume token value
							tokens.Pop();

							total += BsonFormatter.WriteBinary(writer, token);
							break;
						}
						case BsonElementType.ObjectID:
						{
							// consume token value
							tokens.Pop();

							// write ObjectID data
							writer.Write((byte[])token.Value);
							total += BsonWriter.SizeOfObjectID;
							break;
						}
						case BsonElementType.Boolean:
						{
							// consume token value
							tokens.Pop();

							// write bool data
							bool value = true.Equals(token.Value);
							writer.Write(value ? BsonWriter.TrueByte : BsonWriter.FalseByte);
							total += BsonWriter.SizeOfByte;
							break;
						}
						case BsonElementType.DateTimeUtc:
						{
							// consume token value
							tokens.Pop();

							DateTime value = (DateTime)token.Value;
							if (value.Kind == DateTimeKind.Local)
							{
								// convert server-local to UTC
								value = value.ToUniversalTime();
							}

							// find the duration since Jan 1, 1970
							TimeSpan duration = value.Subtract(BsonWriter.UnixEpoch);

							// get the total milliseconds
							long ticks = (long)duration.TotalMilliseconds;

							// write long data
							writer.Write((long)ticks);
							total += BsonWriter.SizeOfInt64;
							break;
						}
						case BsonElementType.RegExp:
						{
							// consume token value
							tokens.Pop();

							Regex regex = token.Value as Regex;
							if (regex == null)
							{
								goto default;
							}

							// default implementation is to simply return the pattern string
							string pattern = regex.ToString();

							// write cstring data
							total += BsonFormatter.WriteString(writer, pattern, true);

							bool isGlobal = false; // nothing to switch on

							string options = isGlobal ? "g" : "";
							switch (regex.Options & (RegexOptions.IgnoreCase|RegexOptions.Multiline))
							{
								case RegexOptions.IgnoreCase:
								{
									options += "i";
									break;
								}
								case RegexOptions.Multiline:
								{
									options += "m";
									break;
								}
								case RegexOptions.IgnoreCase|RegexOptions.Multiline:
								{
									options += "im";
									break;
								}
							}

							// write cstring data
							total += BsonFormatter.WriteString(writer, options, true);
							break;
						}
						case BsonElementType.DBPointer:
						{
							// consume token value
							tokens.Pop();

							BsonDBPointer pointer = token.Value as BsonDBPointer;
							if (pointer == null)
							{
								goto default;
							}

							// write string data
							total += BsonFormatter.WriteString(writer, pointer.Namespace, false);

							// write bytes
							writer.Write((byte[])pointer.ObjectID);
							total += BsonWriter.SizeOfObjectID;
							break;
						}
						case BsonElementType.CodeWithScope:
						{
							// consume token value
							tokens.Pop();

							BsonCodeWithScope codews = token.Value as BsonCodeWithScope;
							if (codews == null)
							{
								goto default;
							}

							total += this.WriteCodeWithScope(writer, codews);
							break;
						}
						case BsonElementType.Int32:
						{
							// consume token value
							tokens.Pop();

							// write int data
							writer.Write((int)token.Value);
							total += BsonWriter.SizeOfInt32;
							break;
						}
						case BsonElementType.TimeStamp:
						case BsonElementType.Int64:
						{
							// consume token value
							tokens.Pop();

							// TODO: determine how to convert TimeStamp

							// write long data
							writer.Write((long)token.Value);
							total += BsonWriter.SizeOfInt64;
							break;
						}
						case BsonElementType.Undefined:
						case BsonElementType.Null:
						case BsonElementType.MinKey:
						case BsonElementType.MaxKey:
						{
							// consume token value
							tokens.Pop();

							// no data emitted for these
							break;
						}
						default:
						{
							// the rest are invalid states
							throw new TokenException<ModelTokenType>(token,
								String.Format(BsonWriter.ErrorUnexpectedToken, token.TokenType));
						}
					}
				}

				return total;
			}

			/// <summary>
			/// Emits a string value
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="token"></param>
			/// <returns>number of bytes written</returns>
			private static int WriteString(BinaryWriter writer, string value, bool asCString)
			{
				int total = BsonWriter.SizeOfByte; // for terminal
				byte[] bytes = Encoding.UTF8.GetBytes(value);

				if (!asCString)
				{
					// write length prefix
					writer.Write(bytes.Length+BsonWriter.SizeOfByte);
					total += BsonWriter.SizeOfInt32;
				}

				// write character data
				writer.Write(bytes);
				total += bytes.Length;

				// write terminal
				writer.Write(BsonWriter.NullByte);

				return total;
			}

			/// <summary>
			/// Emits a binary value
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			/// <returns>number of bytes written</returns>
			private static int WriteBinary(BinaryWriter writer, Token<ModelTokenType> token)
			{
				BsonBinary binary = BsonFormatter.GetBsonBinary(token.Value);

				// write length
				writer.Write(binary.Count);

				// write subtype
				writer.Write((byte)binary.Type);

				// write binary data
				writer.Write(binary.Data);

				// length + subtype + bytes
				return BsonWriter.SizeOfInt64 + BsonWriter.SizeOfByte + binary.Count;
			}

			/// <summary>
			/// Emits a code_w_s value
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="tokens"></param>
			/// <returns>number of bytes written</returns>
			private int WriteCodeWithScope(BinaryWriter writer, BsonCodeWithScope value)
			{
				long start = writer.BaseStream.Position;
				int total = BsonWriter.SizeOfInt32;// code_w_s length field

				// leave room for length
				writer.Seek(BsonWriter.SizeOfInt32, SeekOrigin.Current);

				// write code
				total += BsonFormatter.WriteString(writer, (string)value.Code, false);

				//TODO: this is currently broken.

				// write scope
				total += this.WriteDocument(writer, value.Scope);

				// seek back to write out code_w_s length
				long end = writer.BaseStream.Position;
				writer.Seek((int)(start-end), SeekOrigin.Current);
				writer.Write(total);

				// seek back to end
				writer.Seek((int)(end-start-BsonWriter.SizeOfInt32), SeekOrigin.Current);

				return total;
			}

			#endregion Write Methods

			#region Utility Methods

			private static BsonElementType GetElementType(object value)
			{
				Type type = (value == null) ? null : value.GetType();

				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Boolean:
					{
						return BsonElementType.Boolean;
					}
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.SByte:
					case TypeCode.UInt16:
					{
						return BsonElementType.Int32;
					}
					case TypeCode.Char:
					case TypeCode.String:
					{
						return BsonElementType.String;
					}
					case TypeCode.DateTime:
					{
						return BsonElementType.DateTimeUtc;
					}
					case TypeCode.DBNull:
					case TypeCode.Empty:
					{
						return BsonElementType.Null;
					}
					case TypeCode.Decimal:
					case TypeCode.Double:
					case TypeCode.Single:
					{
						return BsonElementType.Double;
					}
					case TypeCode.Int64:
					case TypeCode.UInt32:
					case TypeCode.UInt64:
					{
						return BsonElementType.Int32;
					}
					default:
					{
						if (value is IBsonFormattable)
						{
							return ((IBsonFormattable)value).GetElementType();
						}
						if (value is TimeSpan)
						{
							return BsonElementType.TimeStamp;
						}
						if (value is Regex)
						{
							return BsonElementType.RegExp;
						}
						if (value is Guid ||
							value is byte[] ||
							value is BsonBinary ||
							value is BsonMD5)
						{
							return BsonElementType.Binary;
						}
						if (value is BsonObjectID)
						{
							return BsonElementType.ObjectID;
						}
						if (value is BsonSymbol)
						{
							return BsonElementType.Symbol;
						}
						if (value is BsonJavaScriptCode)
						{
							return BsonElementType.JavaScriptCode;
						}
						if (value is BsonCodeWithScope)
						{
							return BsonElementType.CodeWithScope;
						}
						if (value is BsonDBPointer)
						{
							return BsonElementType.DBPointer;
						}

						throw new NotSupportedException("Unknown BSON element data type");
					}
				}
			}

			private static BsonBinary GetBsonBinary(object value)
			{
				BsonBinary binary = value as BsonBinary;
				if (binary != null)
				{
					return binary;
				}

				if (value is BsonMD5)
				{
					return new BsonBinary(BsonBinarySubtype.MD5, ((Guid)value).ToByteArray());
				}

				if (value is Guid)
				{
					return new BsonBinary(BsonBinarySubtype.UUID, ((Guid)value).ToByteArray());
				}

				if (value is byte[])
				{
					return new BsonBinary(BsonBinarySubtype.Generic, (byte[])value);
				}

				throw new NotSupportedException("Unknown BSON binary data type");
			}

			#endregion Utility Methods
		}
	}
}
