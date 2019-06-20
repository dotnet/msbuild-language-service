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
using System.Text;
using System.Xml.Schema;
using System.Xml;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Language.StandardClassification;

namespace MonoDevelop.Xml.Editor.Completion
{
	/// <summary>
	/// Helpers for attaching documentation to <see cref="CompletionItem"/> instances.
	/// </summary>
	public static class CompletionItemDocumentationExtensions
	{
		static readonly Type DocsKey = typeof (ICompletionDocumentationProvider);

		static AnnotationDocumentationProvider annotationProvider = new AnnotationDocumentationProvider ();
		static StringXmlDocumentationProvider stringProvider = new StringXmlDocumentationProvider ();

		public static void AddDocumentationProvider (this CompletionItem item, ICompletionDocumentationProvider docsProvider)
		{
			item.Properties.AddProperty (DocsKey, docsProvider);
		}

		public static void AddDocumentation (this CompletionItem item, string documentation)
		{
			item.Properties.AddProperty (DocsKey, stringProvider);
			item.Properties.AddProperty (stringProvider, documentation);
		}

		public static void AddDocumentation (this CompletionItem item, XmlSchemaAnnotation annotation)
		{
			item.Properties.AddProperty (DocsKey, annotationProvider);
			item.Properties.AddProperty (annotationProvider, annotation);
		}

		public static Task<object> GetDocumentationAsync (this CompletionItem item)
		{
			if (item.Properties.TryGetProperty<ICompletionDocumentationProvider> (DocsKey, out var provider)) {
				return provider.GetDocumentationAsync (item);
			}
			return null;
		}

		class StringXmlDocumentationProvider : ICompletionDocumentationProvider
		{
			public Task<object> GetDocumentationAsync (CompletionItem item)
			{
				var desc = item.Properties.GetProperty<string> (this);
				var content = new ClassifiedTextElement (
					new ClassifiedTextRun (PredefinedClassificationTypeNames.NaturalLanguage, desc)
				);
				return Task.FromResult<object> (content);
			}
		}

		class AnnotationDocumentationProvider : ICompletionDocumentationProvider
		{
			public Task<object> GetDocumentationAsync (CompletionItem item)
			{
				var annotation = item.Properties.GetProperty<XmlSchemaAnnotation> (this);

				var documentationBuilder = new StringBuilder ();
				foreach (XmlSchemaObject schemaObject in annotation.Items) {
					var schemaDocumentation = schemaObject as XmlSchemaDocumentation;
					if (schemaDocumentation != null && schemaDocumentation.Markup != null) {
						foreach (XmlNode node in schemaDocumentation.Markup) {
							var textNode = node as XmlText;
							if (textNode != null && !string.IsNullOrEmpty (textNode.Data))
								documentationBuilder.Append (textNode.Data);
						}
					}
				}

				var desc = documentationBuilder.ToString ();
				var content = new ClassifiedTextElement (
					new ClassifiedTextRun (PredefinedClassificationTypeNames.NaturalLanguage, desc)
				);

				return Task.FromResult<object> (content);
			}
		}
	}

	/// <summary>
	/// If an instance of this is attached to a <see cref="CompletionItem"/> instance using <see cref="CompletionItemDocumentationExtensions.AddDocumentationProvider(CompletionItem, ICompletionDocumentationProvider)"/>,
	/// the completion source will use it to lazily look up documentation.
	/// </summary>
	public interface ICompletionDocumentationProvider
	{
		Task<object> GetDocumentationAsync (CompletionItem item);
	}
}
