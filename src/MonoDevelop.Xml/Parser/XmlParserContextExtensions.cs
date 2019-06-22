// 
// Parser.cs
// 
// Author:
//   Mikayla Hutchinson <m.j.hutchinson@gmail.com>
// 
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using MonoDevelop.Xml.Dom;

namespace MonoDevelop.Xml.Parser
{
	public static class XmlParserContextExtensions
	{
		public static void LogError (this IXmlParserContext ctx, string message)
		{
			ctx.Log (new XmlDiagnosticInfo (DiagnosticSeverity.Error, message, ctx.Position - 1));
		}

		public static void LogWarning (this IXmlParserContext ctx, string message)
		{
			ctx.Log (new XmlDiagnosticInfo (DiagnosticSeverity.Warning, message, ctx.Position - 1));
		}

		public static void LogError (this IXmlParserContext ctx, string message, int offset)
		{
			ctx.Log (new XmlDiagnosticInfo (DiagnosticSeverity.Error, message, offset));
		}

		public static void LogWarning (this IXmlParserContext ctx, string message, int offset)
		{
			ctx.Log (new XmlDiagnosticInfo (DiagnosticSeverity.Warning, message, offset));
		}

		public static void LogError (this IXmlParserContext ctx, string message, TextSpan span)
		{
			ctx.Log (new XmlDiagnosticInfo (DiagnosticSeverity.Error, message, span));
		}

		public static void LogWarning (this IXmlParserContext ctx, string message, TextSpan span)
		{
			ctx.Log (new XmlDiagnosticInfo (DiagnosticSeverity.Warning, message, span));
		}
	}
}
