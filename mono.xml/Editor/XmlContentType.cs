// 
// Copyright (C) Microsoft Corp. All rights reserved.
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

using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Utilities;

namespace MonoDevelop.Xml.Editor
{
	static class XmlContentType
	{
		[Export]
		[Name (XmlContentTypeNames.Xml)]
		[BaseDefinition (StandardContentTypeNames.Code)]
		public static readonly ContentTypeDefinition XmlContentTypeDefinition = null;

		[Export]
		[Name (XmlContentTypeNames.Xslt)]
		[BaseDefinition (XmlContentTypeNames.Xml)]
		public static readonly ContentTypeDefinition XsltContentTypeDefinition = null;

		[Export]
		[Name (XmlContentTypeNames.Xsd)]
		[BaseDefinition (XmlContentTypeNames.Xml)]
		public static readonly ContentTypeDefinition XsdContentTypeDefinition = null;

		[Export]
		[FileExtension (".xml")]
		[ContentType (XmlContentTypeNames.Xml)]
		internal static FileExtensionToContentTypeDefinition XmlFileExtensionDefinition = null;

		[Export]
		[FileExtension (".xsl")]
		[ContentType (XmlContentTypeNames.Xslt)]
		internal static FileExtensionToContentTypeDefinition XslFileExtensionDefinition = null;

		[Export]
		[FileExtension (".xslt")]
		[ContentType (XmlContentTypeNames.Xslt)]
		internal static FileExtensionToContentTypeDefinition XsltFileExtensionDefinition = null;

		[Export]
		[FileExtension (".xsd")]
		[ContentType (XmlContentTypeNames.Xsd)]
		internal static FileExtensionToContentTypeDefinition XsdFileExtensionDefinition = null;

	}
}
