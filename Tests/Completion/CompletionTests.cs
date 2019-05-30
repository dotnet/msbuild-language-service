using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Utilities;
using MonoDevelop.Xml.Tests.EditorTestHelpers;
using NUnit.Framework;

namespace MonoDevelop.Xml.Tests.Completion
{
	[TestFixture]
	public class CompletionTests : CompletionTestBase
	{
		public override IContentType ContentType => Catalog.ContentTypeRegistryService.GetContentType (XmlContentTypeHelpers.XmlContentTypeName);

		[Test]
		public async Task TestCompletion ()
		{
			var results = await GetCompletions ("$");
			Assert.Zero (results.Length);
		}
	}
}
