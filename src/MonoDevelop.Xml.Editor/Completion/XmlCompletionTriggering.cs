// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.Text;
using MonoDevelop.Xml.Dom;
using MonoDevelop.Xml.Parser;

namespace MonoDevelop.Xml.Editor.Completion
{
	class XmlCompletionTriggering
	{
		//FIXME: the length should do a readahead to capture the whole token
		public static (XmlCompletionTrigger kind, int length) GetTrigger (XmlParser spine, char typedCharacter)
		{
			int stateTag = ((IXmlParserContext)spine).StateTag;
			bool isExplicit = typedCharacter == '\0';

			// explicit invocation in element name
			if (isExplicit && spine.CurrentState is XmlNameState && spine.Nodes.Peek () is XElement el && !el.IsNamed) {
				int length = spine.CurrentStateLength;
				return (XmlCompletionTrigger.Element, length);
			}

			//typed angle bracket in free space
			if (typedCharacter == '<' && spine.CurrentState is XmlRootState && stateTag == XmlRootState.BRACKET) {
				return (XmlCompletionTrigger.ElementWithBracket, 0);
			}

			//explicit invocation in free space
			if (isExplicit && spine.CurrentState is XmlRootState && stateTag == XmlRootState.FREE) {
				return (XmlCompletionTrigger.ElementWithBracket, 0);
			}

			// trigger on typing <
			if (typedCharacter == '<' && spine.CurrentState is XmlRootState) {
				return (XmlCompletionTrigger.Element, 0);
			}

			// trigger on explicit invocation after <
			if (isExplicit && spine.CurrentState is XmlRootState && stateTag == XmlRootState.BRACKET) {
				return (XmlCompletionTrigger.Element, 0);
			}

			//entity completion
			if (typedCharacter == '&' && (spine.CurrentState is XmlRootState || spine.CurrentState is XmlAttributeValueState)) {
				return (XmlCompletionTrigger.Entity, 0);
			}

			//doctype/cdata completion, explicit trigger after <! or type ! after <
			if ((isExplicit || typedCharacter == '!') && spine.CurrentState is XmlRootState && stateTag == XmlRootState.BRACKET_EXCLAM) {
				return (XmlCompletionTrigger.DocTypeOrCData, 2);
			}

			//explicit trigger in existing doctype
			if (isExplicit && ((spine.CurrentState is XmlRootState && stateTag == XmlRootState.DOCTYPE) || spine.Nodes.Peek () is XDocType)) {
				int length = spine.CurrentState is XmlRootState ? spine.CurrentStateLength : spine.Position - ((XDocType)spine.Nodes.Peek ()).Span.Start;
				return (XmlCompletionTrigger.DocType, length);
			}

			//explicit trigger in attribute name
			if (isExplicit && spine.CurrentState is XmlNameState && spine.CurrentState.Parent is XmlAttributeState) {
				return (XmlCompletionTrigger.Attribute, spine.CurrentStateLength);
			}

			//typed space or explicit trigger in tag
			if ((isExplicit || typedCharacter == ' ') && spine.CurrentState is XmlTagState && stateTag == XmlTagState.FREE) {
				return (XmlCompletionTrigger.Attribute, 0);
			}

			//attribute value completion on quote
			if (spine.CurrentState is XmlAttributeValueState) {
				if (isExplicit) {
					var kind = (stateTag & XmlAttributeValueState.TagMask);
					if (kind == XmlAttributeValueState.DOUBLEQUOTE || kind == XmlAttributeValueState.SINGLEQUOTE) {
						return (XmlCompletionTrigger.AttributeValue, spine.CurrentStateLength - 1);
					}
				}
				//trigger on typing opening quote
				else if (spine.CurrentStateLength == 1 && (typedCharacter == '\'' || typedCharacter =='"')) {
					return (XmlCompletionTrigger.AttributeValue, 0);
				}
			}

			return (XmlCompletionTrigger.None, 0);
		}
	}

	enum XmlCompletionTrigger
	{
		None,
		Element,
		ElementWithBracket,
		Attribute,
		AttributeValue,
		Entity,
		DocType,
		DocTypeOrCData
	}
}
