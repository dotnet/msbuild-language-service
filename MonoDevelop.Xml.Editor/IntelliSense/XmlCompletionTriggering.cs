// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.Text;
using MonoDevelop.Xml.Dom;
using MonoDevelop.Xml.Parser;

namespace MonoDevelop.Xml.Editor.IntelliSense
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
		DocType
	}
}
