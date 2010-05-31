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

#if NET40 && DEBUG
[assembly: AssemblyConfiguration("Debug (NET40)")]
#elif NET40 && STRONG
[assembly: AssemblyConfiguration("Signed (NET40)")]
#elif NET40
[assembly: AssemblyConfiguration("Release (NET40)")]
#elif NET35 && DEBUG
[assembly: AssemblyConfiguration("Debug (NET35)")]
#elif NET35 && STRONG
[assembly: AssemblyConfiguration("Signed (NET35)")]
#elif NET35
[assembly: AssemblyConfiguration("Release (NET35)")]
#elif NET20 && DEBUG
[assembly: AssemblyConfiguration("Debug (NET20)")]
#elif NET20 && STRONG
[assembly: AssemblyConfiguration("Signed (NET20)")]
#elif NET20
[assembly: AssemblyConfiguration("Release (NET20)")]
#elif DEBUG
[assembly: AssemblyConfiguration("Debug")]
#elif STRONG
[assembly: AssemblyConfiguration("Signed")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: ComVisible(false)]
[assembly: Guid("D98A5EF8-4709-4FF8-B162-8EA04B281400")]
[assembly: InternalsVisibleTo("JsonFx.Json.Tests")]
