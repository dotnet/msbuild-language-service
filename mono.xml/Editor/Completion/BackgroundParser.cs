// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace MonoDevelop.Xml.Editor.Completion
{
	public abstract partial class BackgroundParser<T> : IDisposable where T : class
	{
		public static TParser GetParser<TParser> (ITextBuffer2 buffer) where TParser : BackgroundParser<T>, new()
		{
			var parser = buffer.Properties.GetOrCreateSingletonProperty (nameof (TParser), () => new TParser ());
			//avoid capturing by calling this afterwards
			if (parser.Buffer == null) {
				parser.Initialize (buffer);
			}
			return parser;
		}

		void Initialize (ITextBuffer2 buffer)
		{
			Buffer = buffer;

			// it's not super-important to unsubscribe this, as it has the same lifetime as the buffer.
			buffer.ChangedOnBackground += BufferChangedOnBackground;

			// if the content type changes, discard the parser. it will be recreated if needed anyway.
			buffer.ContentTypeChanged += BufferContentTypeChanged;

			Initialize ();
		}

		protected ITextBuffer2 Buffer { get; private set; }

		protected virtual void Initialize ()
		{
		}

		void BufferChangedOnBackground (object sender, TextContentChangedEventArgs e)
		{
			currentOperation?.Cancel ();
			currentOperation = CreateParseOperation ((ITextSnapshot2)e.After);
		}

		ParseOperation CreateParseOperation (ITextSnapshot2 snapshot)
		{
			var tokenSource = new CancellationTokenSource ();
			var task = StartParseAsync (snapshot, tokenSource.Token);
			var operation = new ParseOperation (task, snapshot, tokenSource);

			#pragma warning disable VSTHRD110, VSTHRD105 // Observe result of async calls, Avoid method overloads that assume TaskScheduler.Current

			//capture successful parses
			task.ContinueWith ((t, parent) => {
				((BackgroundParser<T>)parent).lastSuccessfulOperation = operation;
			}, this, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);

			//handle errors
			task.ContinueWith ((t,parent) => {
				try {
					((BackgroundParser <T>)parent).OnUnhandledParseError (t.Exception);
				} catch {}
			}, this, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);

			#pragma warning restore VSTHRD110, VSTHRD105

			return operation;
		}

		protected virtual void OnUnhandledParseError (Exception ex)
		{
			if (System.Diagnostics.Debugger.IsAttached) {
				System.Diagnostics.Debugger.Break ();
			} else {
				Console.WriteLine (ex);
			}
		}

		ParseOperation currentOperation;
		ParseOperation lastSuccessfulOperation;

		protected abstract Task<T> StartParseAsync (ITextSnapshot2 snapshot, T previousParse, ITextSnapshot2 previousSnapshot, CancellationToken token);

		Task<T> StartParseAsync (ITextSnapshot2 snapshot, CancellationToken token)
		{
			var lastSuccessful = lastSuccessfulOperation;
			if (lastSuccessful != null && lastSuccessful.Snapshot.Version.VersionNumber < snapshot.Version.VersionNumber) {
				return StartParseAsync (snapshot, lastSuccessful.Result, lastSuccessful.Snapshot, token);
			}
			return StartParseAsync (snapshot, default (T), null, token);
		}

		public T LastParseResult => lastSuccessfulOperation?.Result;

		/// <summary>
		/// Get an existing completed or running parse task for the provided snapshot if available, or creates a new parse task.
		/// </summary>
		public Task<T> GetOrParseAsync (ITextSnapshot2 snapshot, CancellationToken token)
		{
			var current = currentOperation;
			if (current != null && current.Snapshot == snapshot && current.RegisterAdditionalCancellationOwner (token)) {
				return current.Task;
			}

			var lastSuccessful = lastSuccessfulOperation;
			if (lastSuccessful != null && lastSuccessful.Snapshot.Version.VersionNumber < snapshot.Version.VersionNumber) {
				return StartParseAsync (snapshot, lastSuccessful.Result, lastSuccessful.Snapshot, token);
			}

			return StartParseAsync (snapshot, default (T), null, token);
		}

		void BufferContentTypeChanged (object sender, ContentTypeChangedEventArgs e)
		{
			Dispose ();
		}

		bool disposed = false;

		public void Dispose ()
		{
			if (disposed) {
				return;
			}
			disposed = true;

			Buffer.ChangedOnBackground -= BufferChangedOnBackground;
			Buffer.ContentTypeChanged -= BufferContentTypeChanged;
			Buffer.Properties.RemoveProperty (GetType ().Name);
			Buffer = null;
		}
	}
}
