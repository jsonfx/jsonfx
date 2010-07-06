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
		private readonly IEnumerable<IDataFilter<T>> filters;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public DataReader(DataReaderSettings settings, params IDataFilter<T>[] filters)
			: this(settings, (IEnumerable<IDataFilter<T>>)filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public DataReader(DataReaderSettings settings, IEnumerable<IDataFilter<T>> filters)
		{
			this.settings = settings;
			this.filters = filters;
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

		/// <summary>
		/// Gets the filters used for deserialization
		/// </summary>
		public IEnumerable<IDataFilter<T>> Filters
		{
			get { return this.filters; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		public virtual TResult Deserialize<TResult>(TextReader input)
		{
			object value = this.Deserialize(input, typeof(TResult));

			return (value is TResult) ? (TResult)value : default(TResult);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		public virtual object Deserialize(TextReader input)
		{
			return this.Deserialize(input, null);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input reader</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		public virtual object Deserialize(TextReader input, Type targetType)
		{
			IDataTokenizer<T> tokenizer = this.GetTokenizer();
			if (tokenizer == null)
			{
				throw new InvalidOperationException("Tokenizer is invalid");
			}

			return this.DeserializeInternal(tokenizer, tokenizer.GetTokens(input), targetType);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <typeparam name="TResult">the expected type of the serialized data</typeparam>
		public virtual TResult Deserialize<TResult>(string input)
		{
			object value = this.Deserialize(input, typeof(TResult));

			return (value is TResult) ? (TResult)value : default(TResult);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		public virtual object Deserialize(string input)
		{
			return this.Deserialize(input, null);
		}

		/// <summary>
		/// Deserializes the data from the given input
		/// </summary>
		/// <param name="input">the input text</param>
		/// <param name="targetType">the expected type of the serialized data</param>
		public virtual object Deserialize(string input, Type targetType)
		{
			IDataTokenizer<T> tokenizer = this.GetTokenizer();
			if (tokenizer == null)
			{
				throw new ArgumentNullException("tokenizer");
			}
			return DeserializeInternal(tokenizer, tokenizer.GetTokens(input), targetType);
		}

		/// <summary>
		/// Deserializes a potentially endless sequence of objects from a stream source
		/// </summary>
		/// <param name="input">a streamed source of objects</param>
		/// <returns>a sequence of objects</returns>
		/// <remarks>
		/// character stream => token stream => object stream
		/// </remarks>
		public IEnumerable StreamedDeserialize(TextReader input)
		{
			IDataTokenizer<T> tokenizer = this.GetTokenizer();
			if (tokenizer == null)
			{
				throw new ArgumentNullException("tokenizer");
			}

			IDataAnalyzer<T> analyzer = this.GetAnalyzer();
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

		private object DeserializeInternal(IDataTokenizer<T> tokenizer, IEnumerable<Token<T>> tokens, Type targetType)
		{
			IDataAnalyzer<T> analyzer = this.GetAnalyzer();
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

		protected abstract IDataTokenizer<T> GetTokenizer();

		protected abstract IDataAnalyzer<T> GetAnalyzer();

		#endregion Methods
	}
}
