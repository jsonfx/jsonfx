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
    /// Specifies the name of the property which specifies if member should be serialized.
    /// </summary>
    /// <remarks>
    /// These properties can be marked private/protected/internal and it will still be recognized
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class JsonSpecifiedPropertyAttribute : Attribute
    {
        #region Fields

        private string specifiedProperty;

        #endregion Fields

        #region Init

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="propertyName">the name of the property which controls serialization for this member</param>
        public JsonSpecifiedPropertyAttribute()
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="propertyName">the name of the property which controls serialization for this member</param>
        public JsonSpecifiedPropertyAttribute(string propertyName)
        {
            this.specifiedProperty = propertyName;
        }

        #endregion Init

        #region Properties

        /// <summary>
        /// Gets and sets the name of the property which
        /// specifies if member should be serialized
        /// </summary>
        public string SpecifiedProperty
        {
            get { return this.specifiedProperty; }
            set { this.specifiedProperty = value; }
        }

        #endregion Properties
    }
}