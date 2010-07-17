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

namespace JsonFx.Json
{
	/// <summary>
	/// Specifies the naming to use for a property or field when serializing
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Enum|AttributeTargets.Field|AttributeTargets.Property|AttributeTargets.Struct, AllowMultiple=false)]
	public class JsonNameAttribute : Attribute
	{
		#region Fields

		private string name;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public JsonNameAttribute()
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="name"></param>
		public JsonNameAttribute(string name)
		{
			this.Name = name;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the name to be used in serialization
		/// </summary>
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		#endregion Properties
	}
}
