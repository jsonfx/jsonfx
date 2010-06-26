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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("JsonFx.Json")]
[assembly: AssemblyDescription("JsonFx JSON Serialization")]
[assembly: AssemblyProduct("JsonFx.NET")]
[assembly: AssemblyCopyright("Copyright © 2006-2010 Stephen M. McKamey. All rights reserved.")]
[assembly: AssemblyCompany("http://jsonfx.net")]

[assembly: AssemblyConfiguration(
#if NET40
".NET 4.0 "+
#elif NET35
".NET 3.5 "+
#elif NET20
".NET 2.0 "+
#endif

#if DEBUG
"Debug"
#elif STRONG
"Signed"
#else
"Release"
#endif
)]

[assembly: ComVisible(false)]
[assembly: Guid("D98A5EF8-4709-4FF8-B162-8EA04B281400")]
#if !STRONG
[assembly: InternalsVisibleTo("JsonFx.Json.Tests")]
#endif