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
using System.IO;

namespace JsonFx.Json
{
	/// <summary>
	/// Provides base implementation for standard deserializers
	/// </summary>
	public abstract class DataReader<T> : IDataReader
	{
		#region Fields

		private readonly DataReaderSettings settings;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public DataReader(DataReaderSettings settings)
		{
			this.settings = settings;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the supported content type of the serialized data
		/// </summary>
		public abstract string ContentType
		{
			get;
		}

		/// <summary>
		/// Gets the settings used for deserialization
		/// </summary>
		public DataReaderSettings Settings
		{
			get { return this.settings; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <typeparam name="TVal">the expected type of the serialized data</typeparam>
		public virtual TVal Deserialize<TVal>(TextReader input)
		{
			object value = this.Deserialize(input, typeof(TVal));

			return (value is TVal) ? (TVal)value : default(TVal);
		}

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="input">the input reader</param>
		public virtual object Deserialize(TextReader input)
		{
			return this.Deserialize(input, null);
		}

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		public virtual object Deserialize(TextReader input, Type targetType)
		{
			ITokenizer<T> tokenizer = this.GetTokenizer(this.Settings);
			if (tokenizer == null)
			{
				throw new InvalidOperationException("Tokenizer is invalid");
			}

			IParser<T> parser = this.GetParser(this.Settings);
			if (parser == null)
			{
				throw new InvalidOperationException("Parser is invalid");
			}

			try
			{
				return parser.Parse(tokenizer.Tokenize(input), targetType);
			}
			catch (DeserializationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new DeserializationException(ex.Message, tokenizer.Position, ex);
			}
		}

		protected abstract ITokenizer<T> GetTokenizer(DataReaderSettings settings);

		protected abstract IParser<T> GetParser(DataReaderSettings settings);

		#endregion Methods
	}
}
