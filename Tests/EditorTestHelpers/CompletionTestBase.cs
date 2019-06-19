// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
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

		public Task<QuickInfoItemsCollection> GetQuickInfoItems (
			string documentText,
			char caretMarker = '$')
		{
			var caretOffset = documentText.IndexOf (caretMarker);
			if (caretOffset < 0) {
				throw new ArgumentException ("Document does not contain a caret marker", nameof (documentText));
			}
			documentText = documentText.Substring (0, caretOffset) + documentText.Substring (caretOffset + 1);

			var textView = CreateTextView (documentText);
			return GetQuickInfoItems (textView, caretOffset);

		}

		public async Task<QuickInfoItemsCollection> GetQuickInfoItems (
			ITextView textView,
			int caretPosition,
			CancellationToken cancellationToken = default)
		{
			var broker = Catalog.AsyncQuickInfoBroker;
			var snapshot = textView.TextBuffer.CurrentSnapshot;

			await Catalog.JoinableTaskContext.Factory.SwitchToMainThreadAsync();

			var items = await broker.GetQuickInfoItemsAsync (
				textView,
				snapshot.CreateTrackingPoint (caretPosition, PointTrackingMode.Positive),
				cancellationToken
			);

			return items;
		}
	}
}
