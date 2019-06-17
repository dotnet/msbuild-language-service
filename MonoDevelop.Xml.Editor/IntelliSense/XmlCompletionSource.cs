// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
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
					return await GetElementCompletions (nodePath, false, token);
				case XmlCompletionTrigger.ElementWithBracket:
					return await GetElementCompletions (nodePath, false, token);
				case XmlCompletionTrigger.Attribute:
					IAttributedXObject attributedOb = (spine.Nodes.Peek () as IAttributedXObject) ?? spine.Nodes.Peek (1) as IAttributedXObject;
					return await GetAttributeCompletions (nodePath, attributedOb, GetExistingAttributes (attributedOb), token);
				case XmlCompletionTrigger.AttributeValue:
					if (spine.Nodes.Peek () is XAttribute att && spine.Nodes.Peek (1) is IAttributedXObject attributedObject) {
						return await GetAttributeValueCompletions (nodePath, attributedObject, att, token);
					}
					break;
				case XmlCompletionTrigger.Entity:
					return await GetEntityCompletions (nodePath, token);
				case XmlCompletionTrigger.DocType:
					return await GetDocTypeCompletions (nodePath, token);
				}
			}

			return CompletionContext.Empty;
		}

		public Task<object> GetDescriptionAsync (IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
		{
			throw new NotImplementedException ();
		}

		public CompletionStartData InitializeCompletion (CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
		{
			var parser = BackgroundParser<TResult>.GetParser<TParser> ((ITextBuffer2)triggerLocation.Snapshot.TextBuffer);
			var spine = parser.GetSpineParser (triggerLocation);

			LoggingService.LogDebug (
				"Attempting completion for state '{0}'x{1}, character='{2}', trigger='{3}'",
				spine.CurrentState, spine.CurrentStateLength, trigger.Character, trigger
			);

			var triggerResult = XmlCompletionTriggering.GetTrigger (spine, trigger.Character);
			if (triggerResult.kind != XmlCompletionTrigger.None) {
				return new CompletionStartData (CompletionParticipation.ProvidesItems, new SnapshotSpan (triggerLocation.Snapshot, triggerLocation.Position - triggerResult.length, triggerResult.length));
			}

			return CompletionStartData.DoesNotParticipateInCompletion;
			/*
			var currentChar = trigger.Character;

			bool forced = false;
			switch (trigger.Reason) {
			case CompletionTriggerReason.InvokeAndCommitIfUnique:
				forced = true;
				break;
			case CompletionTriggerReason.Backspace:
				//currentChar = triggerLocation.Position;
				break;
			case CompletionTriggerReason.Insertion:
				break;
			default:
				return CompletionStartData.DoesNotParticipateInCompletion;
			}

			var buf = triggerLocation.Snapshot;
			var currentLocation = triggerLocation.Position;
			char previousChar = currentLocation < 1? ' ' : buf[currentLocation - 1];

			var parser = BackgroundParser<TResult>.GetParser<TParser> ((ITextBuffer2) triggerLocation.Snapshot.TextBuffer);
			var spine = parser.GetSpineParser (triggerLocation);

			//closing tag completion
			if (spine.CurrentState is XmlRootState && currentChar == '>') {
				return CompletionStartData.ParticipatesInCompletionIfAny;
			}

			//entity completion
			if (currentChar == '&' && (spine.CurrentState is XmlRootState ||  spine.CurrentState is XmlAttributeValueState)) {
				return CompletionStartData.ParticipatesInCompletionIfAny;
			}

			//doctype completion
			if (spine.CurrentState is XmlDocTypeState) {
				if (spine.CurrentStateLength == 1) {
					return CompletionStartData.ParticipatesInCompletionIfAny;
				}
				return CompletionStartData.DoesNotParticipateInCompletion;
			}

			//attribute value completion
			//determine whether to trigger completion within attribute values quotes
			if ((spine.CurrentState is XmlAttributeValueState)
				//trigger on the opening quote
				&& ((spine.CurrentStateLength == 1 && (currentChar == '\'' || currentChar == '"'))
				//or trigger on first letter of value, if unforced
				|| (forced || spine.CurrentStateLength == 2))) {
				var att = (XAttribute)spine.Nodes.Peek ();

				if (att.IsNamed) {
					var attributedOb = spine.Nodes.Peek (1) as IAttributedXObject;
					if (attributedOb == null)
						return CompletionStartData.DoesNotParticipateInCompletion;

					//if triggered by first letter of value or forced, grab those letters
					return CompletionStartData.ParticipatesInCompletionIfAny;
				}
			}

			//attribute name completion
			if ((forced && spine.Nodes.Peek () is IAttributedXObject && !spine.Nodes.Peek ().IsEnded)
				 || ((spine.CurrentState is XmlNameState
				&& spine.CurrentState.Parent is XmlAttributeState) ||
				spine.CurrentState is XmlTagState)) {
				IAttributedXObject attributedOb = (spine.Nodes.Peek () as IAttributedXObject) ??
					spine.Nodes.Peek (1) as IAttributedXObject;

				if (attributedOb == null || !attributedOb.Name.IsValid)
					return CompletionStartData.DoesNotParticipateInCompletion;

				var currentIsNameStart = XmlNameState.IsValidNameStart (currentChar);
				var currentIsWhiteSpace = char.IsWhiteSpace (currentChar);
				var previousIsWhiteSpace = char.IsWhiteSpace (previousChar);

				bool shouldTriggerAttributeCompletion = forced
					|| (currentIsNameStart && previousIsWhiteSpace)
					|| currentIsWhiteSpace;
				if (!shouldTriggerAttributeCompletion)
					return CompletionStartData.DoesNotParticipateInCompletion;

				return new CompletionStartData (CompletionParticipation.ProvidesItems, new SnapshotSpan (triggerLocation, 0));
			}

			//element completion
			if (currentChar == '<' && spine.CurrentState is XmlRootState ||
				(spine.CurrentState is XmlNameState && forced)) {
				var list = await GetElementCompletions (token);
				if (completionContext.TriggerLine == 1 && completionContext.TriggerOffset == 1) {
					var encoding = Editor.Encoding.WebName;
					list.Add (new BaseXmlCompletionData ($"?xml version=\"1.0\" encoding=\"{encoding}\" ?>"));
				}
				AddCloseTag (list, spine.Nodes);
				return list.Count > 0 ? list : null;
			}

			if (forced && spine.CurrentState is XmlRootState) {
				var list = new CompletionDataList ();
				MonoDevelop.Ide.CodeTemplates.CodeTemplateService.AddCompletionDataForFileName (DocumentContext.Name, list);
				return list.Count > 0 ? list : null;
			}

			return null;

			throw new NotImplementedException ();
			*/
		}

		protected virtual Task<CompletionContext> GetElementCompletions (List<XObject> nodePath, bool includeBracket, CancellationToken token) => Task.FromResult (CompletionContext.Empty);
		protected virtual Task<CompletionContext> GetAttributeCompletions (List<XObject> nodePath, IAttributedXObject attributedObject, Dictionary<string, string> existingAtts, CancellationToken token) => Task.FromResult (CompletionContext.Empty);
		protected virtual Task<CompletionContext> GetAttributeValueCompletions (List<XObject> nodePath, IAttributedXObject attributedObject, XAttribute attribute, CancellationToken token) => Task.FromResult (CompletionContext.Empty);
		protected virtual Task<CompletionContext> GetEntityCompletions (List<XObject> nodePath, CancellationToken token) => Task.FromResult (CompletionContext.Empty);
		protected virtual Task<CompletionContext> GetDocTypeCompletions (List<XObject> nodePath, CancellationToken token) => Task.FromResult (CompletionContext.Empty);

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

		//FIXME: include attributes ahead of the current position. 
		static Dictionary<string, string> GetExistingAttributes (IAttributedXObject attributedOb)
		{
			var existingAtts = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);
			foreach (XAttribute a in attributedOb.Attributes) {
				existingAtts[a.Name.FullName] = a.Value ?? string.Empty;
			}

			return existingAtts;
		}
	}
}
