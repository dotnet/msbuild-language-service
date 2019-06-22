using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using MonoDevelop.Xml.Editor.Completion;
using NUnit.Framework;

namespace MonoDevelop.Xml.Tests.Schema
{
	/// <summary>
	/// Tests complex content extension elements.
	/// </summary>
	[TestFixture]
	public class ComplexContentExtensionTestFixture : SchemaTestFixtureBase
	{
		CompletionContext bodyChildElements;
		CompletionContext bodyAttributes;
		
		async Task Init ()
		{
			if (bodyChildElements != null)
				return;
			XmlElementPath path = new XmlElementPath();
			path.Elements.Add(new QualifiedName("body", "http://www.w3schools.com")); 
			
			bodyChildElements = await SchemaCompletionData.GetChildElementCompletionDataAsync (DummyCompletionSource.Instance, path, CancellationToken.None);
			bodyAttributes = await SchemaCompletionData.GetAttributeCompletionDataAsync (DummyCompletionSource.Instance, path, CancellationToken.None);
		}	
		
		[Test]
		public async Task TitleHasNoChildElements()
		{
			await Init ();
			XmlElementPath path = new XmlElementPath();
			path.Elements.Add(new QualifiedName("body", "http://www.w3schools.com")); 
			path.Elements.Add(new QualifiedName("title", "http://www.w3schools.com")); 

			Assert.AreEqual(0, (await SchemaCompletionData.GetChildElementCompletionDataAsync (DummyCompletionSource.Instance, path, CancellationToken.None)).Items.Length,
			                "Should be no child elements.");
		}
		
		[Test]
		public async Task TextHasNoChildElements()
		{
			await Init ();
			XmlElementPath path = new XmlElementPath();
			path.Elements.Add(new QualifiedName("body", "http://www.w3schools.com")); 
			path.Elements.Add(new QualifiedName("text", "http://www.w3schools.com")); 

			Assert.AreEqual(0, (await SchemaCompletionData.GetChildElementCompletionDataAsync (DummyCompletionSource.Instance, path, CancellationToken.None)).Items.Length,
			                "Should be no child elements.");
		}		
		
		[Test]
		public async Task BodyHasTwoChildElements()
		{
			await Init ();
			Assert.AreEqual(2, bodyChildElements.Items.Length, 
			                "Should be two child elements.");
		}
		
		[Test]
		public async Task BodyChildElementIsText()
		{
			await Init ();
			Assert.IsTrue(SchemaTestFixtureBase.Contains(bodyChildElements, "text"), 
			              "Should have a child element called text.");
		}
		
		[Test]
		public async Task BodyChildElementIsTitle()
		{
			await Init ();
			Assert.IsTrue(SchemaTestFixtureBase.Contains(bodyChildElements, "title"), 
			              "Should have a child element called title.");
		}		
		
		[Test]
		public async Task BodyAttributeCount()
		{
			await Init ();
			Assert.AreEqual(1, bodyAttributes.Items.Length, 
			                "Should be one attribute.");
		}
		
		[Test]
		public async Task BodyAttributeName()
		{
			await Init ();
			Assert.IsTrue(SchemaTestFixtureBase.Contains(bodyAttributes, "id"), "Attribute id not found.");
		}
		
		protected override string GetSchema()
		{
			return "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace=\"http://www.w3schools.com\"  xmlns=\"http://www.w3schools.com\" elementFormDefault=\"qualified\">\r\n" +
				"\t<xs:complexType name=\"Block\">\r\n" +
				"\t\t<xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n" +
				"\t\t\t<xs:element name=\"title\" type=\"xs:string\"/>\r\n" +
				"\t\t\t<xs:element name=\"text\" type=\"xs:string\"/>\r\n" +
				"\t\t</xs:choice>\r\n" +
				"\t</xs:complexType>\r\n" +
				"\r\n" +
				"\t<xs:element name=\"body\">\r\n" +
				"\t\t<xs:complexType>\r\n" +
				"\t\t\t<xs:complexContent>\r\n" +
				"\t\t\t\t<xs:extension base=\"Block\">\r\n" +
				"\t\t\t\t\t<xs:attribute name=\"id\" type=\"xs:string\"/>\r\n" +
				"\t\t\t\t</xs:extension>\r\n" +
				"\t\t\t</xs:complexContent>\r\n" +
				"\t\t</xs:complexType>\r\n" +
				"\t</xs:element>\r\n" +
				"</xs:schema>";
		}
	}
}
