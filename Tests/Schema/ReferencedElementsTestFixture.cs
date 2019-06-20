using System.Threading;
using MonoDevelop.Xml.Editor.Completion;
using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;

namespace MonoDevelop.Xml.Tests.Schema
{
	[TestFixture]
	public class ReferencedElementsTestFixture : SchemaTestFixtureBase
	{
		CompletionContext shipOrderAttributes;
		CompletionContext shipToAttributes;
		XmlElementPath shipToPath;
		XmlElementPath shipOrderPath;
		
		async Task Init ()
		{
			if (shipOrderAttributes != null)
				return;
			
			// Get shipto attributes.
			shipToPath = new XmlElementPath();
			QualifiedName shipOrderName = new QualifiedName("shiporder", "http://www.w3schools.com");
			shipToPath.Elements.Add(shipOrderName);
			shipToPath.Elements.Add(new QualifiedName("shipto", "http://www.w3schools.com"));
			IAsyncCompletionSource source = DummyCompletionSource.Instance;

			shipToAttributes = await SchemaCompletionData.GetAttributeCompletionDataAsync (source, shipToPath, CancellationToken.None);
			
			// Get shiporder attributes.
			shipOrderPath = new XmlElementPath();
			shipOrderPath.Elements.Add(shipOrderName);
			
			shipOrderAttributes = await SchemaCompletionData.GetAttributeCompletionDataAsync (source, shipOrderPath, CancellationToken.None);
			
		}
		
		[Test]
		public async Task OneShipOrderAttribute()
		{
			await Init ();
			Assert.AreEqual(1, shipOrderAttributes.Items.Length, "Should only have one shiporder attribute.");
		}		
		
		[Test]
		public async Task ShipOrderAttributeName()
		{
			await Init ();
			Assert.IsTrue(SchemaTestFixtureBase.Contains(shipOrderAttributes,"id"),
			                "Incorrect shiporder attribute name.");
		}

		[Test]
		public async Task OneShipToAttribute()
		{
			await Init ();
			Assert.AreEqual(1, shipToAttributes.Items.Length, "Should only have one shipto attribute.");
		}
		
		[Test]
		public async Task ShipToAttributeName()
		{
			await Init ();
			Assert.IsTrue(SchemaTestFixtureBase.Contains(shipToAttributes, "address"),
			                "Incorrect shipto attribute name.");
		}					
		
		[Test]
		public async Task ShipOrderChildElementsCount()
		{
			await Init ();
			Assert.AreEqual(1, (await SchemaCompletionData.GetChildElementCompletionDataAsync (DummyCompletionSource.Instance, shipOrderPath, CancellationToken.None)).Items.Length, 
			                "Should be one child element.");
		}
		
		[Test]
		public async Task ShipOrderHasShipToChildElement()
		{
			await Init ();
			var data = await SchemaCompletionData.GetChildElementCompletionDataAsync (DummyCompletionSource.Instance, shipOrderPath, CancellationToken.None);
			Assert.IsTrue(SchemaTestFixtureBase.Contains(data, "shipto"), 
			                "Incorrect child element name.");
		}
		
		[Test]
		public async Task ShipToChildElementsCount()
		{
			await Init ();
			Assert.AreEqual(2, (await SchemaCompletionData.GetChildElementCompletionDataAsync (DummyCompletionSource.Instance, shipToPath, CancellationToken.None)).Items.Length, 
			                "Should be 2 child elements.");
		}		
		
		[Test]
		public async Task ShipToHasNameChildElement()
		{
			await Init ();
			var data = await SchemaCompletionData.GetChildElementCompletionDataAsync (DummyCompletionSource.Instance, shipToPath, CancellationToken.None);
			Assert.IsTrue(SchemaTestFixtureBase.Contains(data, "name"),  "Incorrect child element name.");
		}		
		
		[Test]
		public async Task ShipToHasAddressChildElement()
		{
			await Init ();
			var data = await SchemaCompletionData.GetChildElementCompletionDataAsync (DummyCompletionSource.Instance, shipToPath, CancellationToken.None);
			Assert.IsTrue(SchemaTestFixtureBase.Contains(data, "address"), "Incorrect child element name.");
		}		
		
		protected override string GetSchema()
		{
			return "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace=\"http://www.w3schools.com\"  xmlns=\"http://www.w3schools.com\">\r\n" +
				"\r\n" +
				"<!-- definition of simple elements -->\r\n" +
				"<xs:element name=\"name\" type=\"xs:string\"/>\r\n" +
				"<xs:element name=\"address\" type=\"xs:string\"/>\r\n" +
				"\r\n" +
				"<!-- definition of complex elements -->\r\n" +
				"<xs:element name=\"shipto\">\r\n" +
				" <xs:complexType>\r\n" +
				"  <xs:sequence>\r\n" +
				"   <xs:element ref=\"name\"/>\r\n" +
				"   <xs:element ref=\"address\"/>\r\n" +
				"  </xs:sequence>\r\n" +
				"  <xs:attribute name=\"address\"/>\r\n" +
				" </xs:complexType>\r\n" +
				"</xs:element>\r\n" +
				"\r\n" +
				"<xs:element name=\"shiporder\">\r\n" +
				" <xs:complexType>\r\n" +
				"  <xs:sequence>\r\n" +
				"   <xs:element ref=\"shipto\"/>\r\n" +
				"  </xs:sequence>\r\n" +
				"  <xs:attribute name=\"id\"/>\r\n" +
				" </xs:complexType>\r\n" +
				"</xs:element>\r\n" +
				"\r\n" +
				"</xs:schema>";
		}
	}
}
