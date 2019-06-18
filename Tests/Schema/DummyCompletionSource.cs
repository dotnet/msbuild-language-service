using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using MonoDevelop.Xml.Editor.IntelliSense;

namespace MonoDevelop.Xml.Tests.Schema
{
	class DummyCompletionSource : IAsyncCompletionSource
	{
		public static DummyCompletionSource Instance => new DummyCompletionSource ();

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
			throw new NotImplementedException ();
		}
	}
}
