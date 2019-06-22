// 
// XmlCompletionDataList.cs
//  
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
// 
// Copyright (c) 2011 Novell, Inc.
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
using System.Text;
using System.Xml.Schema;
using System.Xml;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using MonoDevelop.Xml.Editor.Completion;
using System.Collections.Immutable;

namespace MonoDevelop.Xml.Editor.Completion
{
	class XmlSchemaCompletionBuilder
	{
		List<CompletionItem> items = new List<CompletionItem> ();

		HashSet<string> names = new HashSet<string> ();
		readonly XmlNamespacePrefixMap nsMap;
		readonly IAsyncCompletionSource source;

		public XmlSchemaCompletionBuilder (IAsyncCompletionSource source, XmlNamespacePrefixMap nsMap)
		{
			this.nsMap = nsMap;
			this.source = source;
		}

		public XmlSchemaCompletionBuilder (IAsyncCompletionSource source) : this (source, new XmlNamespacePrefixMap ())
		{
		}

		internal void AddNamespace (string namespaceUri)
		{
			var item = new CompletionItem (namespaceUri, source, XmlImages.NamespaceImage);
			items.Add (item);
		}

		public void AddAttribute (XmlSchemaAttribute attribute)
		{
			string name = attribute.Name;
			if (name == null) {
				var ns = attribute.RefName.Namespace;
				if (string.IsNullOrEmpty (ns))
					return;
				var prefix = nsMap.GetPrefix (ns);
				if (prefix == null) {
					if (ns == "http://www.w3.org/XML/1998/namespace")
						prefix = "xml";
					else
						return;
				}
				name = attribute.RefName.Name;
				if (prefix.Length > 0)
					name = prefix + ":" + name;
			}
			if (!names.Add (name))
				return;
			var item = new CompletionItem (name, source, XmlImages.AttributeImage);
			item.AddDocumentation (attribute.Annotation);
			items.Add (item);
		}
		
		public void AddAttributeValue (string valueText)
		{
			var item = new CompletionItem (valueText, source, XmlImages.AttributeValueImage);
			items.Add (item);
		}
		
		public void AddAttributeValue (string valueText, XmlSchemaAnnotation annotation)
		{
			var item = new CompletionItem (valueText, source, XmlImages.AttributeValueImage);
			item.AddDocumentation (annotation);
			items.Add (item);
		}		
		
		/// <summary>
		/// Adds an element completion data to the collection if it does not 
		/// already exist.
		/// </summary>
		public void AddElement (string name, string prefix, string documentation)
		{
			if (!names.Add (name))
				return;
			//FIXME: don't accept a prefix, accept a namespace and resolve it to a prefix
			if (prefix.Length > 0)
				name = string.Concat (prefix, ":", name);

			var item = new CompletionItem (name, source, XmlImages.ElementImage);
			item.AddDocumentation (documentation);
			items.Add (item);
		}
		
		/// <summary>
		/// Adds an element completion data to the collection if it does not 
		/// already exist.
		/// </summary>
		public void AddElement (string name, string prefix, XmlSchemaAnnotation annotation)
		{
			if (!names.Add (name))
				return;
			//FIXME: don't accept a prefix, accept a namespace and resolve it to a prefix
			if (prefix.Length > 0)
				name = string.Concat (prefix, ":", name);

			var item = new CompletionItem (name, source, XmlImages.ElementImage);
			item.AddDocumentation (annotation);
			items.Add (item);
		}

		public ImmutableArray<CompletionItem> GetItems ()
		{
			return ImmutableArray<CompletionItem>.Empty.AddRange (items);
		}
	}
}

