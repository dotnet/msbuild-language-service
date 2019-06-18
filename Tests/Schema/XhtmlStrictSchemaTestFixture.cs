using MonoDevelop.Xml.Completion;
using NUnit.Framework;
using System.Xml;
using MonoDevelop.Xml.Tests.Utils;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;

namespace MonoDevelop.Xml.Tests.Schema
{
	/// <summary>
	/// Tests the xhtml1-strict schema.
	/// </summary>
	[TestFixture]
	public class XhtmlStrictSchemaTestFixture
	{
		XmlSchemaCompletionProvider schemaCompletionData;
		XmlElementPath h1Path;
		CompletionContext h1Attributes;
		string namespaceURI = "http://www.w3.org/1999/xhtml";
		
		async Task Init ()
		{
			if (schemaCompletionData != null)
				return;
			
			var reader = ResourceManager.GetXhtmlStrictSchema();
			schemaCompletionData = new XmlSchemaCompletionProvider(reader);
			
			// Set up h1 element's path.
			h1Path = new XmlElementPath();
			h1Path.Elements.Add(new QualifiedName("html", namespaceURI));
			h1Path.Elements.Add(new QualifiedName("body", namespaceURI));
			h1Path.Elements.Add(new QualifiedName("h1", namespaceURI));
			
			// Get h1 element info.
			h1Attributes = await schemaCompletionData.GetAttributeCompletionDataAsync (DummyCompletionSource.Instance, h1Path, CancellationToken.None);
		}
		
		[Test]
		public async Task H1HasAttributes()
		{
			await Init ();
			Assert.IsTrue(h1Attributes.Items.Length > 0, "Should have at least one attribute.");
		}
	}
}
