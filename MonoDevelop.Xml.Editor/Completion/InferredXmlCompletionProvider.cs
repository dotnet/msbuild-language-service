// 
// InferredXmlCompletionProvider.cs
//  
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
// 
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using MonoDevelop.Xml.Dom;

namespace MonoDevelop.Xml.Completion
{
	class InferredXmlCompletionProvider : XmlCompletionProvider
	{
		Dictionary<string,HashSet<string>> elementCompletions = new Dictionary<string,HashSet<string>> ();
		Dictionary<string,HashSet<string>> attributeCompletions = new Dictionary<string,HashSet<string>> ();
		
		public DateTime TimeStampUtc { get; set; }
		public int ErrorCount { get; set; }

		//TODO: respect namespaces
		public void Populate (XDocument doc)
		{
			foreach (XNode node in doc.AllDescendentNodes) {
				XElement el = node as XElement;
				if (el == null)
					continue;
				string parentName = "";
				XElement parentEl = el.Parent as XElement;
				if (parentEl != null)
					parentName = parentEl.Name.Name;
				
				HashSet<string> map;
				if (!elementCompletions.TryGetValue (parentName, out map)) {
					map = new HashSet<string> ();
					elementCompletions.Add (parentName, map);
				}
				map.Add (el.Name.Name);
				
				if (!attributeCompletions.TryGetValue (el.Name.Name, out map)) {
					map = new HashSet<string> ();
					attributeCompletions.Add (el.Name.Name, map);
				}
				foreach (XAttribute att in el.Attributes)
					map.Add (att.Name.Name);
			}
		}
		
		public override Task GetElementCompletionData (CompletionContext context, XmlElementPath path, CancellationToken token)
		{
			return GetCompletions (context, elementCompletions, path, XmlCompletionData.DataType.XmlElement);
		}
		
		public override Task GetAttributeCompletionData (CompletionContext context, XmlElementPath path, CancellationToken token)
		{
			return GetCompletions (context, attributeCompletions, path, XmlCompletionData.DataType.XmlAttribute);
		}
		
		public override Task GetAttributeValueCompletionData (CompletionContext context, XmlElementPath path, string name, CancellationToken token)
		{
			return Task.CompletedTask;
		}
		
		static Task GetCompletions (CompletionContext context, Dictionary<string,HashSet<string>> map, XmlElementPath path, XmlCompletionData.DataType type)
		{
			if (path == null || path.Elements.Count == 0) {
				return Task.CompletedTask;
			}

			var tagName = path.Elements [path.Elements.Count - 1].Name;

			HashSet<string> values;
			if (map.TryGetValue (tagName, out values)) {
				foreach (string s in values) {
					context.AddItem (XmlCompletionDataHelper.Create (s, type));
				}
			}
			return Task.CompletedTask;
		}
	}

	public static class XmlCompletionDataHelper
	{
		public static CompletionItem Create (string name, XmlCompletionData.DataType type)
		{
			//TODO: icon
			return CompletionItem.Create (name);
		}
	}
}
