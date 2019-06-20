// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace MonoDevelop.Xml.Editor.Completion
{
	public abstract partial class BackgroundParser<T>
	{
		class ParseOperation
		{
			CancellationTokenSource tokenSource;

			// if this ever reaches zero, the task gets cancelled
			int ownerCount;

			//if value is zero, the cancel method has not been called
			int primaryOwnerCanceled;

			public ParseOperation (Task<T> operation, ITextSnapshot2 snapshot, CancellationTokenSource tokenSource)
			{
				Task = operation;
				Snapshot = snapshot;
				this.tokenSource = tokenSource;
				ownerCount = 1;
				primaryOwnerCanceled = 0;
			}

			public Task<T> Task { get; }
			public ITextSnapshot2 Snapshot { get; }

			#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
			public T Result => Task.IsCompleted ? Task.Result : default (T);
			#pragma warning restore VSTHRD002

			public void Cancel ()
			{
				//ensure this can only run once, regardless of threading
				if (Interlocked.Exchange (ref primaryOwnerCanceled, 1) == 0) {
					CancelInternal ();
				}
			}

			//returns true if the operation has still not been cancelled
			public bool RegisterAdditionalCancellationOwner (CancellationToken token)
			{
				// if still not cancelled, register ownership and wire up cancellation callback
				if (tokenSource != null && InterlockedIncrementIfNonzero (ref ownerCount)) {
					token.Register (CancelInternal);
					return true;
				}

				return !Task.IsCanceled;
			}

			void CancelInternal ()
			{
				// if this ever gets to zero, do the cancellation for real
				if (Interlocked.Decrement (ref ownerCount) == 0) {
					tokenSource?.Cancel ();
					tokenSource = null;
				}
			}

			static bool InterlockedIncrementIfNonzero (ref int location)
			{
				while (true) {
					int value = location;
					if (value == 0) {
						return false;
					}
					if (Interlocked.CompareExchange (ref location, value + 1, value) == value) {
						return true;
					}
				}
			}
		}
	}
}
