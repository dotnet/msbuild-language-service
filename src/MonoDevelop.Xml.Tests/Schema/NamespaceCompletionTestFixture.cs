using MonoDevelop.Xml.Editor.Completion;
using NUnit.Framework;
using System.IO;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;

namespace MonoDevelop.Xml.Tests.Schema
{
	/// <summary>
	/// The collection of schemas should provide completion data for the
	/// namespaces it holds.
	/// </summary>
	[TestFixture]
	public class NamespaceCompletionTestFixture
	{
		CompletionContext namespaceCompletionData;
		string firstNamespace = "http://foo.com/foo.xsd";
		string secondNamespace = "http://bar.com/bar.xsd";
		
		[OneTimeSetUp]
		public void FixtureInit()
		{
			XmlSchemaCompletionDataCollection items = new XmlSchemaCompletionDataCollection();
			
			StringReader reader = new StringReader(GetSchema(firstNamespace));
			XmlSchemaCompletionProvider schema = new XmlSchemaCompletionProvider(reader);
			items.Add(schema);
			
			reader = new StringReader(GetSchema(secondNamespace));
			schema = new XmlSchemaCompletionProvider(reader);
			items.Add(schema);
			var builder = new XmlSchemaCompletionBuilder (DummyCompletionSource.Instance);
			items.GetNamespaceCompletionData (builder);
			namespaceCompletionData = new CompletionContext (builder.GetItems ());
		}
		
		[Test]
		public void NamespaceCount()
		{
			Assert.AreEqual(2, namespaceCompletionData.Items.Length, "Should be 2 namespaces.");
		}
		
		[Test]
		public void ContainsFirstNamespace()
		{
			Assert.IsTrue(SchemaTestFixtureBase.Contains(namespaceCompletionData, firstNamespace));
		}
		
		[Test]
		public void ContainsSecondNamespace()
		{
			Assert.IsTrue(SchemaTestFixtureBase.Contains(namespaceCompletionData, secondNamespace));
		}		
		
		string GetSchema(string namespaceURI)
		{
			return "<?xml version=\"1.0\"?>\r\n" +
				"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\r\n" +
				"targetNamespace=\"" + namespaceURI + "\"\r\n" +
				"xmlns=\"" + namespaceURI + "\"\r\n" +
				"elementFormDefault=\"qualified\">\r\n" +
				"<xs:element name=\"note\">\r\n" +
				"</xs:element>\r\n" +
				"</xs:schema>";
		}
	}
}
