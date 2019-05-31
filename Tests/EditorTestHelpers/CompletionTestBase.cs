using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MonoDevelop.Xml.Tests.EditorTestHelpers
{
	public abstract class CompletionTestBase : EditorTestBase
	{
		public abstract IContentType ContentType { get; }

		public virtual ITextView CreateTextView (string documentText)
		{
			var buffer = Catalog.BufferFactoryService.CreateTextBuffer (documentText, ContentType);
			return Catalog.TextViewFactory.CreateTextView (buffer);
		}

		public Task<CompletionContext> GetCompletionContext (string documentText, CompletionTriggerReason reason = CompletionTriggerReason.Invoke, char triggerChar = '\0', char caretMarker = '$')
		{
			var caretOffset = documentText.IndexOf (caretMarker);
			if (caretOffset < 0) {
				throw new ArgumentException ("Document does not contain a caret marker", nameof (documentText));
			}
			documentText = documentText.Substring (0, caretOffset) + documentText.Substring (caretOffset + 1);

			var textView = CreateTextView (documentText);
			return GetCompletionContext (textView, caretOffset, reason, triggerChar);
		}

		public async Task<CompletionContext> GetCompletionContext (ITextView textView, int caretPosition, CompletionTriggerReason reason, char triggerChar, CancellationToken cancellationToken = default)
		{
			var broker = Catalog.AsyncCompletionBroker;
			var snapshot = textView.TextBuffer.CurrentSnapshot;

			var trigger = new CompletionTrigger (reason, snapshot, triggerChar);
			if (triggerChar != '\0') {
				snapshot = textView.TextBuffer.Insert (caretPosition, triggerChar.ToString ());
				caretPosition++;
			}

			var context = await broker.GetAggregatedCompletionContextAsync (
				textView,
				trigger,
				new SnapshotPoint (snapshot, caretPosition),
				cancellationToken
			);

			return context.CompletionContext;
		}
	}
}
