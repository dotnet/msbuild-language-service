// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;

namespace MonoDevelop.Xml.Editor.IntelliSense
{
	public abstract class XmlCompletionSource<TParser,TResult> : IAsyncCompletionSource where TResult : XmlParseResult where TParser : XmlBackgroundParser<TResult>, new ()
	{
		public Task<CompletionContext> GetCompletionContextAsync (IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
		{
			throw new NotImplementedException ();
		}

		public Task<object> GetDescriptionAsync (IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
		{
			throw new NotImplementedException ();
		}

		public CompletionStartData InitializeCompletion (CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
		{
			var parser = BackgroundParser<TResult>.GetParser<TParser> ((ITextBuffer2)triggerLocation.Snapshot.TextBuffer);
			var spine = parser.GetSpineParser (triggerLocation);

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

			LoggingService.LogDebug ("Attempting completion for state '{0}'x{1}, previousChar='{2}',"
				+ " currentChar='{3}', forced='{4}'", spine.CurrentState,
				spine.CurrentStateLength, previousChar, currentChar, forced);

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
	}

	internal class LoggingService
	{
		internal static void LogDebug (string v, object currentState, object currentStateLength, char previousChar, char currentChar, object forced)
		{
			throw new NotImplementedException ();
		}
	}
}
