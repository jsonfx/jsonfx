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
using System.Reflection;

namespace JsonFx
{
	/// <summary>
	/// JsonFx metadata
	/// </summary>
	public sealed class About
	{
		#region Fields

		public static readonly About Fx = new About(typeof(About).Assembly);

		public readonly Version Version;
		public readonly string FullName;
		public readonly string Name;
		public readonly string Configuration;
		public readonly string Copyright;
		public readonly string Title;
		public readonly string Description;
		public readonly string Company;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="assembly"></param>
		public About(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}

			AssemblyName name = assembly.GetName();

			this.FullName = assembly.FullName;
			this.Version = name.Version;
			this.Name = name.Name;

			AssemblyCopyrightAttribute copyright = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
			this.Copyright = (copyright != null) ? copyright.Copyright : String.Empty;

			AssemblyDescriptionAttribute description = Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
			this.Description = (description != null) ? description.Description : String.Empty;

			AssemblyTitleAttribute title = Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
			this.Title = (title != null) ? title.Title : String.Empty;

			AssemblyCompanyAttribute company = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
			this.Company = (company != null) ? company.Company : String.Empty;

			AssemblyConfigurationAttribute config = Attribute.GetCustomAttribute(assembly, typeof(AssemblyConfigurationAttribute)) as AssemblyConfigurationAttribute;
			this.Configuration = (config != null) ? config.Configuration : String.Empty;
		}

		#endregion Init
	}
}
