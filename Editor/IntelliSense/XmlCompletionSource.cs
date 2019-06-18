// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using MonoDevelop.Xml.Completion;
using MonoDevelop.Xml.Dom;
using MonoDevelop.Xml.Parser;

namespace MonoDevelop.Xml.Editor.IntelliSense
{
	public abstract class XmlCompletionSource<TParser,TResult> : IAsyncCompletionSource where TResult : XmlParseResult where TParser : XmlBackgroundParser<TResult>, new ()
	{
		public async Task<CompletionContext> GetCompletionContextAsync (IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
		{
			var parser = BackgroundParser<TResult>.GetParser<TParser> ((ITextBuffer2)triggerLocation.Snapshot.TextBuffer);
			var spine = parser.GetSpineParser (triggerLocation);

			var triggerResult = await Task.Run (() => XmlCompletionTriggering.GetTrigger (spine, trigger.Character), token).ConfigureAwait (false);

			if (triggerResult.kind != XmlCompletionTrigger.None) {
				List<XObject> nodePath = GetNodePath (spine, triggerLocation.Snapshot);

				switch (triggerResult.kind) {
				case XmlCompletionTrigger.Element:
				case XmlCompletionTrigger.ElementWithBracket:
					//TODO: if it's on the first line and there's no XML declaration, add <"?xml version=\"1.0\" encoding=\"{encoding}\" ?>";
					//TODO: if it's on the first or second line and there's no DTD declaration, add the DTDs, or at least <!DOCTYPE
					//TODO: add closing tags // AddCloseTag (list, spine.Nodes);
					//TODO: add snippets // MonoDevelop.Ide.CodeTemplates.CodeTemplateService.AddCompletionDataForFileName (DocumentContext.Name, list);
					return await GetElementCompletionsAsync (nodePath, triggerResult.kind == XmlCompletionTrigger.ElementWithBracket, token);

				case XmlCompletionTrigger.Attribute:
					IAttributedXObject attributedOb = (spine.Nodes.Peek () as IAttributedXObject) ?? spine.Nodes.Peek (1) as IAttributedXObject;
					return await GetAttributeCompletionsAsync (nodePath, attributedOb, GetExistingAttributes (spine, triggerLocation.Snapshot, attributedOb), token);

				case XmlCompletionTrigger.AttributeValue:
					if (spine.Nodes.Peek () is XAttribute att && spine.Nodes.Peek (1) is IAttributedXObject attributedObject) {
						return await GetAttributeValueCompletionsAsync (nodePath, attributedObject, att, token);
					}
					break;

				case XmlCompletionTrigger.Entity:
					return await GetEntityCompletionsAsync (nodePath, token);

				case XmlCompletionTrigger.DocType:
				case XmlCompletionTrigger.DocTypeOrCData:
					// we delegate adding the CDATA completion to the subclass as only it knows whether character data is valid in that position
					return await GetDocTypeCompletionsAsync (nodePath, triggerResult.kind == XmlCompletionTrigger.DocTypeOrCData, token);
				}
			}

			return CompletionContext.Empty;
		}

		public Task<object> GetDescriptionAsync (IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
		{
			return item.GetDocumentationAsync ();
		}

		public CompletionStartData InitializeCompletion (CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
		{
			var parser = BackgroundParser<TResult>.GetParser<TParser> ((ITextBuffer2)triggerLocation.Snapshot.TextBuffer);
			var spine = parser.GetSpineParser (triggerLocation);

			LoggingService.LogDebug (
				"Attempting completion for state '{0}'x{1}, character='{2}', trigger='{3}'",
				spine.CurrentState, spine.CurrentStateLength, trigger.Character, trigger
			);

			var (kind, length) = XmlCompletionTriggering.GetTrigger (spine, trigger.Character);
			if (kind != XmlCompletionTrigger.None) {
				return new CompletionStartData (CompletionParticipation.ProvidesItems, new SnapshotSpan (triggerLocation.Snapshot, triggerLocation.Position - length, length));
			}

			//TODO: closing tag completion after typing >

			return CompletionStartData.DoesNotParticipateInCompletion;
		}

		protected virtual Task<CompletionContext> GetElementCompletionsAsync (List<XObject> nodePath, bool includeBracket, CancellationToken token) => Task.FromResult (CompletionContext.Empty);
		protected virtual Task<CompletionContext> GetAttributeCompletionsAsync (List<XObject> nodePath, IAttributedXObject attributedObject, Dictionary<string, string> existingAtts, CancellationToken token) => Task.FromResult (CompletionContext.Empty);
		protected virtual Task<CompletionContext> GetAttributeValueCompletionsAsync (List<XObject> nodePath, IAttributedXObject attributedObject, XAttribute attribute, CancellationToken token) => Task.FromResult (CompletionContext.Empty);
		protected virtual Task<CompletionContext> GetEntityCompletionsAsync (List<XObject> nodePath, CancellationToken token) => Task.FromResult (CompletionContext.Empty);
		protected virtual Task<CompletionContext> GetDocTypeCompletionsAsync (List<XObject> nodePath, bool includeCData, CancellationToken token) => Task.FromResult (CompletionContext.Empty);

		protected List<XObject> GetNodePath (XmlParser spine, ITextSnapshot snapshot)
		{
			var path = new List<XObject> (spine.Nodes);

			//remove the root XDocument
			path.RemoveAt (path.Count - 1);

			//complete incomplete XName if present
			if (spine.CurrentState is XmlNameState && path[0] is INamedXObject) {
				path[0] = path[0].ShallowCopy ();
				XName completeName = GetCompleteName (spine, snapshot);
				((INamedXObject)path[0]).Name = completeName;
			}
			path.Reverse ();
			return path;
		}

		protected XName GetCompleteName (XmlParser spine, ITextSnapshot snapshot)
		{
			Debug.Assert (spine.CurrentState is XmlNameState);

			int end = spine.Position;
			int start = end - spine.CurrentStateLength;
			int mid = -1;

			int limit = Math.Min (snapshot.Length, end + 35);

			//try to find the end of the name, but don't go too far
			for (; end < limit; end++) {
				char c = snapshot[end];

				if (c == ':') {
					if (mid == -1)
						mid = end;
					else
						break;
				} else if (!XmlChar.IsNameChar (c))
					break;
			}

			if (mid > 0 && end > mid + 1) {
				return new XName (snapshot.GetText(start, mid - start), snapshot.GetText (mid + 1, end - mid -1));
			}
			return new XName (snapshot.GetText (start, end - start));
		}

		static Dictionary<string, string> GetExistingAttributes (XmlParser spineParser, ITextSnapshot snapshot, IAttributedXObject attributedOb)
		{
			// clone parser to avoid modifying state
			spineParser = (XmlParser) ((ICloneable)spineParser).Clone ();

			// parse rest of element to get all attributes
			for (int i = spineParser.Position; i < snapshot.Length; i++) {
				spineParser.Push (snapshot[i]);

				var currentState = spineParser.CurrentState;
				switch (spineParser.CurrentState) {
				case XmlAttributeState _:
				case XmlAttributeValueState _:
				case XmlTagState _:
						continue;
				case XmlNameState _:
					if (currentState.Parent is XmlAttributeState) {
						continue;
					}
					break;
				}
				break;
			}

			var existingAtts = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);
			foreach (XAttribute a in attributedOb.Attributes) {
				existingAtts[a.Name.FullName] = a.Value ?? string.Empty;
			}

			return existingAtts;
		}
	}
}
