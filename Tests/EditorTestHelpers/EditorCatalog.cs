using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.MiniEditor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace MonoDevelop.Xml.Tests.Completion
{
	public class EditorCatalog
	{
		public EditorCatalog (EditorEnvironment env) => Host = env.GetEditorHost ();

		EditorEnvironment.Host Host { get; }

		public ITextViewFactoryService TextViewFactory
			=> Host.GetService<ITextViewFactoryService> ();

		public ITextDocumentFactoryService TextDocumentFactoryService
			=> Host.GetService<ITextDocumentFactoryService> ();

		public IFileToContentTypeService FileToContentTypeService
			=> Host.GetService<IFileToContentTypeService> ();

		public ITextBufferFactoryService BufferFactoryService
			=> Host.GetService<ITextBufferFactoryService> ();

		public IContentTypeRegistryService ContentTypeRegistryService
			=> Host.GetService<IContentTypeRegistryService> ();

		public IAsyncCompletionBroker AsyncCompletionBroker
			=> Host.GetService<IAsyncCompletionBroker> ();

		public IClassifierAggregatorService ClassifierAggregatorService
			=> Host.GetService<IClassifierAggregatorService> ();

		public IClassificationTypeRegistryService ClassificationTypeRegistryService
			=> Host.GetService<IClassificationTypeRegistryService> ();

		public IBufferTagAggregatorFactoryService BufferTagAggregatorFactoryService
			=> Host.GetService<IBufferTagAggregatorFactoryService> ();
	}
}
