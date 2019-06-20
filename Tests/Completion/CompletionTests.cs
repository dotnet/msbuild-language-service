using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.MiniEditor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using MonoDevelop.Xml.Dom;
using MonoDevelop.Xml.Editor;
using MonoDevelop.Xml.Editor.Completion;
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
		public IAsyncCompletionSource GetOrCreate (ITextView textView) => new XmlCompletionTestSource (textView);
	}

	class XmlCompletionTestSource : XmlCompletionSource<XmlBackgroundParser,XmlParseResult>
	{
		public XmlCompletionTestSource (ITextView textView) : base (textView)
		{
		}

		protected override Task<CompletionContext> GetElementCompletionsAsync (
			SnapshotPoint triggerLocation,
			List<XObject> nodePath,
			bool includeBracket,
			CancellationToken token)
		{
			var item = new CompletionItem ("Hello", this);
			var items = ImmutableArray<CompletionItem>.Empty;
			items = items.Add (item);
			return Task.FromResult (new CompletionContext (items));
		}
	}

	[TestFixture]
	public class CompletionTests : CompletionTestBase
	{
		public override IContentType ContentType => Catalog.ContentTypeRegistryService.GetContentType (CompletionTestContentType.Name);

		[Test]
		public async Task TestElementStartCompletion ()
		{
			var result = await GetCompletionContext ("<$");
			Assert.AreEqual (1, result.Items.Length);
			Assert.AreEqual ("Hello", result.Items[0].DisplayText);
		}

		[Test]
		public async Task TestElementNameCompletionInvocation ()
		{
			var result = await GetCompletionContext ("<foo$");
			Assert.AreEqual (1, result.Items.Length);
			Assert.AreEqual ("Hello", result.Items[0].DisplayText);
		}

		protected override (EditorEnvironment, EditorCatalog) InitializeEnvironment () => TestEnvironment.EnsureInitialized ();
	}
}
