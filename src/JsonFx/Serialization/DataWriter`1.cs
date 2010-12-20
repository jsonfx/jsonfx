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

using JsonFx.Serialization.Filters;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Provides base implementation of standard serializers
	/// </summary>
	public abstract class DataWriter<T> : IDataWriter
	{
		#region Fields

		private readonly DataWriterSettings settings;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		protected DataWriter(DataWriterSettings settings)
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
		/// Gets the content encoding for the serialized data
		/// </summary>
		public abstract Encoding ContentEncoding
		{
			get;
		}

		/// <summary>
		/// Gets the supported content types for the serialized data
		/// </summary>
		public abstract IEnumerable<string> ContentType
		{
			get;
		}

		/// <summary>
		/// Gets the supported file extensions for the serialized data
		/// </summary>
		public abstract IEnumerable<string> FileExtension
		{
			get;
		}

		/// <summary>
		/// Gets the settings used for serialization
		/// </summary>
		public DataWriterSettings Settings
		{
			get { return this.settings; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="data">the data to be serialized</param>
		/// <param name="output">the output writer</param>
		public virtual void Write(object data, TextWriter output)
		{
			IObjectWalker<T> walker = this.GetWalker();
			if (walker == null)
			{
				throw new ArgumentNullException("walker");
			}

			ITextFormatter<T> formatter = this.GetFormatter();
			if (formatter == null)
			{
				throw new ArgumentNullException("formatter");
			}

			try
			{
				// objects => tokens => characters
				formatter.Format(walker.GetTokens(data), output);
			}
			catch (SerializationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new SerializationException(ex.Message, ex);
			}
		}

		/// <summary>
		/// Serializes the data to the given output
		/// </summary>
		/// <param name="data">the data to be serialized</param>
		/// <returns>the serialized data</returns>
		public virtual string Write(object data)
		{
			IObjectWalker<T> walker = this.GetWalker();
			if (walker == null)
			{
				throw new ArgumentNullException("walker");
			}

			ITextFormatter<T> formatter = this.GetFormatter();
			if (formatter == null)
			{
				throw new ArgumentNullException("formatter");
			}

			try
			{
				// objects => tokens => characters
				return formatter.Format(walker.GetTokens(data));
			}
			catch (SerializationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new SerializationException(ex.Message, ex);
			}
		}

		/// <summary>
		/// Gets the walker for this DataWriter
		/// </summary>
		/// <param name="dataWriterSettings"></param>
		/// <returns></returns>
		protected abstract IObjectWalker<T> GetWalker();

		/// <summary>
		/// Gets the formatter for this DataWriter
		/// </summary>
		/// <param name="dataWriterSettings"></param>
		/// <returns></returns>
		protected abstract ITextFormatter<T> GetFormatter();

		#endregion Methods
	}
}
