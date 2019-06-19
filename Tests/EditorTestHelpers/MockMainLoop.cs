using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace MonoDevelop.Xml.Tests.EditorTestHelpers
{
	public class MockMainLoop : SynchronizationContext
	{
		AsyncQueue<Payload> pending = new AsyncQueue<Payload> ();

		void Run (object obj)
		{
			JoinableTaskContext = new JoinableTaskContext (MainThread, this);

			((TaskCompletionSource<bool>)obj).SetResult (true);
			obj = null;

			while (!pending.IsCompleted) {
				var payload = pending.DequeueAsync ().Result;
				if (payload.Waiter != null) {
					try {
						payload.Callback (payload.State);
						payload.Waiter.TrySetResult (null);
					} catch (Exception ex) {
						payload.Waiter.TrySetException (ex);
					}
				} else {
					payload.Callback (payload.State);
				}
			}
		}

		public Thread MainThread { get; private set; }
		public JoinableTaskContext JoinableTaskContext { get; private set; }

		public Task Start ()
		{
			var readyTask = new TaskCompletionSource<bool> ();
			MainThread = new Thread (Run) { IsBackground = true };
			MainThread.Start (readyTask);
			return readyTask.Task;
		}

		public void Stop () => pending.Complete ();

		public override void Send (SendOrPostCallback d, object state)
		{
			var waiter = new TaskCompletionSource<object> ();
			pending.Enqueue (new Payload (d, state, waiter));
			waiter.Task.Wait ();
		}

		public override void Post (SendOrPostCallback d, object state)
		{
			pending.Enqueue (new Payload (d, state, null));
		}

		class Payload
		{
			public SendOrPostCallback Callback { get; private set; }
            public object State { get; private set; }
			public TaskCompletionSource<object> Waiter { get; private set; }

            public Payload (SendOrPostCallback d, object state, TaskCompletionSource<object> waiter)
			{
				Callback = d;
				State = state;
				Waiter = waiter;
			}
		}
	}
}
