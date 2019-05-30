using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace MonoDevelop.Xml.Tests.EditorTestHelpers
{
	public sealed class XmlContentTypeHelpers
	{
		[Export]
		[Name (XmlContentTypeName)]
		[BaseDefinition (StandardContentTypeNames.Code)]
		public static readonly ContentTypeDefinition XmlContentTypeDefinition = null;

		public const string XmlContentTypeName = "xml";
	}
}
