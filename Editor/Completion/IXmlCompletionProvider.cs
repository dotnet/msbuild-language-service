// 
// IXmlCompletionProvider.cs
// 
// Author:
//   Mikayla Hutchinson <m.j.hutchinson@gmail.com>
// 
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;

namespace MonoDevelop.Xml.Editor.Completion
{
	interface IXmlCompletionProvider
	{
		Task<CompletionContext> GetElementCompletionDataAsync (IAsyncCompletionSource source, CancellationToken token);
		Task<CompletionContext> GetElementCompletionDataAsync (IAsyncCompletionSource source, string namespacePrefix, CancellationToken token);

		Task<CompletionContext> GetChildElementCompletionDataAsync (IAsyncCompletionSource source, XmlElementPath path, CancellationToken token);
		Task<CompletionContext> GetAttributeCompletionDataAsync (IAsyncCompletionSource source, XmlElementPath path, CancellationToken token);
		Task<CompletionContext> GetAttributeValueCompletionDataAsync (IAsyncCompletionSource source, XmlElementPath path, string name, CancellationToken token);

		Task<CompletionContext> GetChildElementCompletionDataAsync (IAsyncCompletionSource source, string tagName, CancellationToken token);
		Task<CompletionContext> GetAttributeCompletionDataAsync (IAsyncCompletionSource source, string tagName, CancellationToken token);
		Task<CompletionContext> GetAttributeValueCompletionDataAsync (IAsyncCompletionSource source, string tagName, string name, CancellationToken token);
	}
}