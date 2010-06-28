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

namespace JsonFx.Serialization
{
	/// <summary>
	/// Controls deserialization settings for IDataReader
	/// </summary>
	public class DataReaderSettings
		: IMemberCacheContainer
	{
		#region Fields

		private bool allowNullValueTypes = true;
		private IDataNameResolver resolver;
		private MemberCache memberCache;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets and sets if ValueTypes can accept values of null
		/// </summary>
		/// <remarks>
		/// If this is true and a ValueType T is assigned the value of null,
		/// it will receive the value of default(T).
		/// Setting this to false, throws an exception if null is
		/// specified for a ValueType member.
		/// </remarks>
		public bool AllowNullValueTypes
		{
			get { return this.allowNullValueTypes; }
			set { this.allowNullValueTypes = value; }
		}

		/// <summary>
		/// Gets and sets extensibility point to control name resolution for IDataReader
		/// </summary>
		public IDataNameResolver Resolver
		{
			get
			{
				if (this.resolver == null)
				{
					this.resolver = new DataNameResolver();
				}
				return this.resolver;
			}
			set
			{
				this.resolver = value;
				this.memberCache = null;
			}
		}

		/// <summary>
		/// Gets and sets extensibility point to control name resolution for IDataReader
		/// </summary>
		MemberCache IMemberCacheContainer.MemberCache
		{
			get
			{
				if (this.memberCache == null)
				{
					this.memberCache = new MemberCache(this.Resolver);
				}
				return this.memberCache;
			}
		}

		#endregion Properties

		#region IMemberCacheContainer Members

		public void CopyCacheFrom(IMemberCacheContainer container)
		{
			this.Resolver = container.MemberCache.Resolver;
			this.memberCache = container.MemberCache;
		}

		#endregion IMemberCacheContainer Members
	}
}
