using MonoDevelop.Xml.Completion;
using NUnit.Framework;
using System;

namespace MonoDevelop.Xml.Tests.Schema.Uri
{
	/// <summary>
	/// Tests the <see cref="XmlSchemaCompletionProvider.GetUri"/> method.
	/// </summary>
	[TestFixture]
	public class GetUriTestFixture
	{
		[Test]
		public void SimpleFileName()
		{
			string fileName = @"C:\temp\foo.xml";
			string expectedUri = "file:///C:/temp/foo.xml";

			Assert.AreEqual(expectedUri, XmlSchemaCompletionProvider.GetUri(fileName));
		}
		
		[Test]
		public void NullFileName()
		{
			Assert.AreEqual(String.Empty, XmlSchemaCompletionProvider.GetUri(null));
		}
		
		[Test]
		public void EmptyString()
		{
			Assert.AreEqual(String.Empty, XmlSchemaCompletionProvider.GetUri(String.Empty));
		}
	}
}
