// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using MonoDevelop.Xml.Dom;
using MonoDevelop.Xml.Parser;

namespace MonoDevelop.Xml.Editor.IntelliSense
{
	public abstract class XmlBackgroundParser<TResult> : BackgroundParser<TResult> where TResult : XmlParseResult
	{
		protected override void Initialize ()
		{
			StateMachine = CreateParserStateMachine ();
			spineCache = new ForwardParserCache<XmlParser> (new XmlParser (StateMachine, false), Buffer);
		}

		protected virtual XmlRootState CreateParserStateMachine () => new XmlRootState ();

		// the state machine does not store any state itself, so we can re-use it
		protected XmlRootState StateMachine { get; private set; }

		// this is intended to be called from the UI thread
		public XmlParser GetSpineParser (SnapshotPoint point)
		{
			spineCache.UpdatePosition (point.Position);
			return spineCache.Parser;
		}

		ForwardParserCache<XmlParser> spineCache;
	}

	public sealed class XmlBackgroundParser : XmlBackgroundParser<XmlParseResult>
	{
		protected override Task<XmlParseResult> StartParseAsync (ITextSnapshot2 snapshot, XmlParseResult previousParse, ITextSnapshot2 previousSnapshot, CancellationToken token)
		{
			var parser = new XmlParser (StateMachine, true);
			return Task.Run (() => {
				var length = snapshot.Length;
				for (int i = 0; i < length; i++) {
					parser.Push (snapshot[i]);
				}
				return new XmlParseResult (parser.Nodes.GetRoot (), parser.Diagnostics);
			});
		}
	}

	public class XmlParseResult
	{
		public XmlParseResult (XDocument xDocument, List<XmlDiagnosticInfo> diagnostics)
		{
			XDocument = xDocument;
			Diagnostics = diagnostics;
		}

        public List<XmlDiagnosticInfo> Diagnostics { get; }
        public XDocument XDocument { get; }
    }
}
