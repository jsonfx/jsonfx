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
using System.Text;

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
		public DataWriter(DataWriterSettings settings)
		{
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
		/// Gets the supported content type for the serialized data
		/// </summary>
		public abstract string ContentType
		{
			get;
		}

		/// <summary>
		/// Gets the supported file extension for the serialized data
		/// </summary>
		public abstract string FileExtension
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
		/// <param name="output">the output writer</param>
		/// <param name="data">the data to be serialized</param>
		public virtual void Serialize(TextWriter output, object data)
		{
			IDataGenerator<T> generator = this.GetGenerator(this.Settings);
			if (generator == null)
			{
				throw new InvalidOperationException("Generator is invalid");
			}

			IDataFormatter<T> formatter = this.GetFormatter(this.Settings);
			if (formatter == null)
			{
				throw new InvalidOperationException("Formatter is invalid");
			}

			try
			{
				formatter.Write(output, generator.GetTokens(data));
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

		protected abstract IDataGenerator<T> GetGenerator(DataWriterSettings dataWriterSettings);

		protected abstract IDataFormatter<T> GetFormatter(DataWriterSettings dataWriterSettings);

		#endregion Methods
	}
}
