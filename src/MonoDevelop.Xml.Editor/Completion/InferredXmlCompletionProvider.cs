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
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using MonoDevelop.Xml.Dom;

namespace MonoDevelop.Xml.Editor.Completion
{
	class InferredXmlCompletionProvider : IXmlCompletionProvider
	{
		Dictionary<string, HashSet<string>> elementCompletions = new Dictionary<string, HashSet<string>> ();
		Dictionary<string, HashSet<string>> attributeCompletions = new Dictionary<string, HashSet<string>> ();

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

		public Task<CompletionContext> GetElementCompletionDataAsync (IAsyncCompletionSource source, CancellationToken token)
		{
			return GetChildElementCompletionDataAsync (source, "", token);
		}

		public Task<CompletionContext> GetElementCompletionDataAsync (IAsyncCompletionSource source, string namespacePrefix, CancellationToken token)
		{
			return Task.FromResult (CompletionContext.Empty);
		}

		public Task<CompletionContext> GetChildElementCompletionDataAsync (IAsyncCompletionSource source, XmlElementPath path, CancellationToken token)
		{
			return GetCompletions (source, elementCompletions, path);
		}

		public Task<CompletionContext> GetAttributeCompletionDataAsync (IAsyncCompletionSource source, XmlElementPath path, CancellationToken token)
		{
			return GetCompletions (source, attributeCompletions, path);
		}

		public Task<CompletionContext> GetAttributeValueCompletionDataAsync (IAsyncCompletionSource source, XmlElementPath path, string name, CancellationToken token)
		{
			return Task.FromResult (CompletionContext.Empty);
		}

		public Task<CompletionContext> GetChildElementCompletionDataAsync (IAsyncCompletionSource source, string tagName, CancellationToken token)
		{
			return GetCompletions (source, elementCompletions, tagName);
		}

		public Task<CompletionContext> GetAttributeCompletionDataAsync (IAsyncCompletionSource source, string tagName, CancellationToken token)
		{
			return GetCompletions (source, attributeCompletions, tagName);
		}

		public Task<CompletionContext> GetAttributeValueCompletionDataAsync (IAsyncCompletionSource source, string tagName, string name, CancellationToken token)
		{
			return Task.FromResult (CompletionContext.Empty);
		}

		static Task<CompletionContext> GetCompletions (IAsyncCompletionSource source, Dictionary<string, HashSet<string>> map, string tagName)
		{
			var items = ImmutableArray<CompletionItem>.Empty;
			HashSet<string> values;
			if (map.TryGetValue (tagName, out values))
				foreach (string s in values)
					items = items.Add (new CompletionItem (s, source));
			var context = new CompletionContext (items, null, InitialSelectionHint.SoftSelection);
			return Task.FromResult (context);
		}

		static Task<CompletionContext> GetCompletions (IAsyncCompletionSource source, Dictionary<string, HashSet<string>> map, XmlElementPath path)
		{
			if (path == null || path.Elements.Count == 0)
				return Task.FromResult (CompletionContext.Empty);
			return GetCompletions (source, map, path.Elements[path.Elements.Count - 1].Name);
		}
	}
}