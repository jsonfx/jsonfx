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

namespace JsonFx.CodeGen
{
	/// <summary>
	/// Generalized delegate for invoking a constructor
	/// </summary>
	/// <param name="args"></param>
	/// <returns></returns>
	public delegate object FactoryDelegate(params object[] args);

	/// <summary>
	/// Generalized delegate for invoking a method
	/// </summary>
	/// <param name="target">the instance object</param>
	/// <param name="args">the method parameters</param>
	/// <returns></returns>
	public delegate object ProxyDelegate(object target, params object[] args);

	/// <summary>
	/// Generalized delegate for getting a field or property value
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public delegate object GetterDelegate(object target);

	/// <summary>
	/// Generalized delegate for setting a field or property value
	/// </summary>
	/// <param name="target"></param>
	/// <param name="value"></param>
	public delegate void SetterDelegate(object target, object value);
}
