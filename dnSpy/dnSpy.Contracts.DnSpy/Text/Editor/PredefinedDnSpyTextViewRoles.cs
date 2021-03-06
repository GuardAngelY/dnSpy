﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using dnSpy.Contracts.Files.Tabs.DocViewer;
using dnSpy.Contracts.Output;
using Microsoft.VisualStudio.Text.Editor;

namespace dnSpy.Contracts.Text.Editor {
	/// <summary>
	/// Predefined dnSpy textview roles
	/// </summary>
	public static class PredefinedDnSpyTextViewRoles {
		/// <summary>
		/// <see cref="IDocumentViewer"/> text view role
		/// </summary>
		public const string DocumentViewer = "dnSpy-IDocumentViewer";

		/// <summary>
		/// <see cref="ILogEditor"/> text view role
		/// </summary>
		public const string LogEditor = "dnSpy-ILogEditor";

		/// <summary>
		/// <see cref="IOutputTextPane"/> text view role
		/// </summary>
		public const string OutputTextPane = "dnSpy-IOutputTextPane";

		/// <summary>
		/// <see cref="IReplEditor"/> text view role
		/// </summary>
		public const string ReplEditor = "dnSpy-IReplEditor";

		/// <summary>
		/// Roslyn REPL (any supported language, eg. C# and Visual Basic)
		/// </summary>
		public const string RoslynRepl = "dnSpy-RoslynRepl";

		/// <summary>
		/// C# REPL
		/// </summary>
		public const string CSharpRepl = "dnSpy-CSharpRepl";

		/// <summary>
		/// Visual Basic REPL
		/// </summary>
		public const string VisualBasicRepl = "dnSpy-VisualBasicRepl";

		/// <summary>
		/// <see cref="ICodeEditor"/> text view role
		/// </summary>
		public const string CodeEditor = "dnSpy-ICodeEditor";

		/// <summary>
		/// Roslyn code editor (any supported language, eg. C# and Visual Basic)
		/// </summary>
		public const string RoslynCodeEditor = "dnSpy-RoslynCodeEditor";

		/// <summary>
		/// Roslyn code editor (C#)
		/// </summary>
		public const string RoslynCSharpCodeEditor = "dnSpy-RoslynCSharpCodeEditor";

		/// <summary>
		/// Roslyn code editor (Visual Basic)
		/// </summary>
		public const string RoslynVisualBasicCodeEditor = "dnSpy-RoslynVisualBasicCodeEditor";

		/// <summary>
		/// Enables the custom line number margin, see <see cref="Editor.CustomLineNumberMargin"/>
		/// documentation for more info.
		/// </summary>
		public const string CustomLineNumberMargin = "dnSpy-CustomLineNumberMargin";

		/// <summary>
		/// <see cref="IGlyphTextMarkerService"/> services can be used. Not needed if
		/// <see cref="PredefinedTextViewRoles.Interactive"/> is already used.
		/// </summary>
		public const string CanHaveGlyphTextMarkerService = "dnSpy-CanHaveGlyphTextMarkerService";

		/// <summary>
		/// Allows the current line highlighter to be used. Not needed if
		/// <see cref="PredefinedTextViewRoles.Document"/> is already used.
		/// </summary>
		public const string CanHaveCurrentLineHighlighter = "dnSpy-CanHaveCurrentLineHighlighter";

		/// <summary>
		/// Allows the line number margin to be used. Not needed if
		/// <see cref="PredefinedTextViewRoles.Document"/> is already used.
		/// </summary>
		public const string CanHaveLineNumberMargin = "dnSpy-CanHaveLineNumberMargin";

		/// <summary>
		/// Allows line separators to be used. Not needed if
		/// <see cref="PredefinedTextViewRoles.Document"/> is already used.
		/// </summary>
		public const string CanHaveLineSeparator = "dnSpy-CanHaveLineSeparator";

		/// <summary>
		/// Allows background images to be used
		/// </summary>
		public const string CanHaveBackgroundImage = "dnSpy-CanHaveBackgroundImage";

		/// <summary>
		/// Allows line compressor
		/// </summary>
		public const string CanHaveLineCompressor = "dnSpy-CanHaveLineCompressor";

		/// <summary>
		/// Allows intellisense controllers
		/// </summary>
		public const string CanHaveIntellisenseControllers = "dnSpy-CanHaveIntellisenseControllers";
	}
}
