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
using System.IO;

namespace JsonFx.Serialization
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
		protected DataReader(DataReaderSettings settings)
		{
			if (settings == null)
			{
				throw new NullReferenceException("settings");
			}
			this.settings = settings;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the supported content type of the serialized data
		/// </summary>
		public abstract IEnumerable<string> ContentType
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

		#region Read Methods

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <param name="ignored">a value used to trigger Type inference for <typeparamref name="TResult"/> (e.g. for deserializing anonymous objects)</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		public virtual TResult Read<TResult>(TextReader input, TResult ignored)
		{
			return this.Read<TResult>(input);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		public virtual TResult Read<TResult>(TextReader input)
		{
			object value = this.Read(input, typeof(TResult));

			return (value is TResult) ? (TResult)value : default(TResult);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		public virtual object Read(TextReader input)
		{
			return this.Read(input, null);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		public virtual object Read(TextReader input, Type targetType)
		{
			ITextTokenizer<T> tokenizer = this.GetTokenizer();
			if (tokenizer == null)
			{
				throw new InvalidOperationException("Tokenizer is invalid");
			}

			return this.ReadSingle(tokenizer, tokenizer.GetTokens(input), targetType);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <param name="ignored">a value used to trigger Type inference for <typeparamref name="TResult"/> (e.g. for deserializing anonymous objects)</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		public virtual TResult Read<TResult>(string input, TResult ignored)
		{
			return this.Read<TResult>(input);
		}
	
		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		public virtual TResult Read<TResult>(string input)
		{
			object value = this.Read(input, typeof(TResult));

			return (value is TResult) ? (TResult)value : default(TResult);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		public virtual object Read(string input)
		{
			return this.Read(input, null);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		public virtual object Read(string input, Type targetType)
		{
			ITextTokenizer<T> tokenizer = this.GetTokenizer();
			if (tokenizer == null)
			{
				throw new ArgumentNullException("tokenizer");
			}

			return this.ReadSingle(tokenizer, tokenizer.GetTokens(input), targetType);
		}

		/// <summary>
		/// Deserializes a potentially endless sequence of objects from a stream source
		/// </summary>
		/// <param name="input">a streamed source of objects</param>
		/// <returns>a sequence of objects</returns>
		/// <remarks>
		/// character stream => token stream => object stream
		/// </remarks>
		public IEnumerable ReadMany(TextReader input)
		{
			ITextTokenizer<T> tokenizer = this.GetTokenizer();
			if (tokenizer == null)
			{
				throw new ArgumentNullException("tokenizer");
			}

			ITokenAnalyzer<T> analyzer = this.GetAnalyzer();
			if (analyzer == null)
			{
				throw new ArgumentNullException("analyzer");
			}

			try
			{
				// chars stream => token stream => object stream
				return analyzer.Analyze(tokenizer.GetTokens(input));
			}
			catch (DeserializationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new DeserializationException(ex.Message, tokenizer.Index, tokenizer.Line, tokenizer.Column, ex);
			}
		}

		private object ReadSingle(ITextTokenizer<T> tokenizer, IEnumerable<Token<T>> tokens, Type targetType)
		{
			ITokenAnalyzer<T> analyzer = this.GetAnalyzer();
			if (analyzer == null)
			{
				throw new ArgumentNullException("analyzer");
			}

			try
			{
				IEnumerator enumerator = analyzer.Analyze(tokens, targetType).GetEnumerator();
				if (!enumerator.MoveNext())
				{
					return null;
				}

				// character stream => token stream => object stream
				object value = enumerator.Current;

				// enforce only one object in stream
				if (!this.Settings.AllowTrailingContent && enumerator.MoveNext())
				{
					throw new DeserializationException("Invalid trailing content", tokenizer.Index, tokenizer.Line, tokenizer.Column);
				}

				return value;
			}
			catch (DeserializationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new DeserializationException(ex.Message, tokenizer.Index, tokenizer.Line, tokenizer.Column, ex);
			}
		}

		#endregion Read Methods

		#region Methods

		protected abstract ITextTokenizer<T> GetTokenizer();

		protected abstract ITokenAnalyzer<T> GetAnalyzer();

		#endregion Methods
	}
}
