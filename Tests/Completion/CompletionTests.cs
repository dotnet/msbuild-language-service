using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using MonoDevelop.Xml.Editor;
using MonoDevelop.Xml.Editor.IntelliSense;
using MonoDevelop.Xml.Tests.EditorTestHelpers;
using NUnit.Framework;

namespace MonoDevelop.Xml.Tests.Completion
{
	static class CompletionTestContentType
	{
		public const string Name = "XmlCompletionTest";

		[Export]
		[Name (Name)]
		[BaseDefinition (StandardContentTypeNames.Code)]
		public static readonly ContentTypeDefinition XmlCompletionTestContentTypeDefinition = null;
	}

	[Export (typeof (IAsyncCompletionSourceProvider))]
	[Name ("Xml Completion Test Source Provider")]
	[ContentType (CompletionTestContentType.Name)]
	class XmlCompletionTestSourceProvider : IAsyncCompletionSourceProvider
	{
		public IAsyncCompletionSource GetOrCreate (ITextView textView) => new XmlCompletionTestSource ();
	}

	class XmlCompletionTestSource : XmlCompletionSource<XmlBackgroundParser,XmlParseResult>
	{
	}

	[TestFixture]
	public class CompletionTests : CompletionTestBase
	{
		public override IContentType ContentType => Catalog.ContentTypeRegistryService.GetContentType (CompletionTestContentType.Name);

		[Test]
		public async Task TestElementStartCompletion ()
		{
			var result = await GetCompletionContext ("<$");
			Assert.Zero (result.Items.Length);
		}

		public async Task TestElementNameCompletionInvocation ()
		{
			var result = await GetCompletionContext ("<foo$");
			Assert.Zero (result.Items.Length);
		}
	}
}
