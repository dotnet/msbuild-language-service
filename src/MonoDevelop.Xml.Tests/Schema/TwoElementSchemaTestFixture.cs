using MonoDevelop.Xml.Editor.Completion;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Threading;

namespace MonoDevelop.Xml.Tests.Schema
{
	/// <summary>
	/// Two elements defined in a schema, one uses the 'type' attribute to
	/// link to the complex type definition.
	/// </summary>
	[TestFixture]
	public class TwoElementSchemaTestFixture : SchemaTestFixtureBase
	{
		XmlElementPath noteElementPath;
		XmlElementPath textElementPath;
		
		public override void FixtureInit()
		{
			// Note element path.
			noteElementPath = new XmlElementPath();
			QualifiedName noteQualifiedName = new QualifiedName("note", "http://www.w3schools.com");
			noteElementPath.Elements.Add(noteQualifiedName);
		
			// Text element path.
			textElementPath = new XmlElementPath();
			textElementPath.Elements.Add(noteQualifiedName);
			textElementPath.Elements.Add(new QualifiedName("text", "http://www.w3schools.com"));
		}	
		
		[Test]
		public async Task TextElementHasOneAttribute()
		{
			var attributesCompletionData = await SchemaCompletionData.GetAttributeCompletionDataAsync (DummyCompletionSource.Instance, textElementPath, CancellationToken.None);
			
			Assert.AreEqual (1, attributesCompletionData.Items.Length, "Should have 1 text attribute.");
		}
		
		[Test]
		public async Task TextElementAttributeName()
		{
			var attributesCompletionData = await SchemaCompletionData.GetAttributeCompletionDataAsync (DummyCompletionSource.Instance, textElementPath, CancellationToken.None);
			Assert.IsTrue (SchemaTestFixtureBase.Contains(attributesCompletionData, "foo"), "Unexpected text attribute name.");
		}

		[Test]
		public async Task NoteElementHasChildElement()
		{
			var childElementCompletionData = await SchemaCompletionData.GetChildElementCompletionDataAsync (DummyCompletionSource.Instance, noteElementPath, CancellationToken.None);
			
			Assert.AreEqual(1, childElementCompletionData.Items.Length, "Should be one child.");
		}
		
		[Test]
		public async Task NoteElementHasNoAttributes()
		{	
			var attributeCompletionData = await SchemaCompletionData.GetAttributeCompletionDataAsync (DummyCompletionSource.Instance, noteElementPath, CancellationToken.None);
			
			Assert.AreEqual(0, attributeCompletionData.Items.Length, "Should no attributes.");
		}

		[Test]
		public async Task OneRootElement()
		{
			var elementCompletionData = await SchemaCompletionData.GetElementCompletionDataAsync (DummyCompletionSource.Instance, CancellationToken.None);
			
			Assert.AreEqual(1, elementCompletionData.Items.Length, "Should be 1 root element.");
		}
		
		[Test]
		public async Task RootElementIsNote()
		{
			var elementCompletionData = await SchemaCompletionData.GetElementCompletionDataAsync (DummyCompletionSource.Instance, CancellationToken.None);
			
			Assert.IsTrue(Contains(elementCompletionData, "note"), "Should be called note.");
		}
		
		protected override string GetSchema()
		{
			return "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace=\"http://www.w3schools.com\" xmlns=\"http://www.w3schools.com\" elementFormDefault=\"qualified\">\r\n" +
				"\t<xs:element name=\"note\">\r\n" +
				"\t\t<xs:complexType> \r\n" +
				"\t\t\t<xs:sequence>\r\n" +
				"\t\t\t\t<xs:element name=\"text\" type=\"text-type\"/>\r\n" +
				"\t\t\t</xs:sequence>\r\n" +
				"\t\t</xs:complexType>\r\n" +
				"\t</xs:element>\r\n" +
				"\t<xs:complexType name=\"text-type\">\r\n" +
				"\t\t<xs:attribute name=\"foo\"/>\r\n" +
				"\t</xs:complexType>\r\n" +
				"</xs:schema>";
		}
	}
}
