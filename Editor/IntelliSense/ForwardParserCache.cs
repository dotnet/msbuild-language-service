// Copyright (c) Microsoft. All rights reserved.
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

//#define DEBUG_DOCUMENT_STATE_ENGINE_TRACKER

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using MonoDevelop.Xml.Parser;

// taken from MonoDevelop.Ide.Gui.Content and ported to VS editor
namespace MonoDevelop.Xml.Editor
{
	class ForwardParserCache<T> : IDisposable where T : IForwardParser
	{
		readonly Stack<T> cachedEngines = new Stack<T> ();
		readonly ITextBuffer2 buffer;

		public ForwardParserCache (T engine, ITextBuffer2 buffer)
		{
			this.Parser = engine;
			this.buffer = buffer;
			buffer.Changed += BufferChanged;
		}

		public void Dispose ()
		{
			buffer.Changed -= BufferChanged;
		}

		public T Parser { get; private set; }

		void BufferChanged (object sender, TextContentChangedEventArgs e)
		{
			int lowestPos = Parser.Position;
			foreach (var change in e.Changes) {
				lowestPos = Math.Min (change.OldPosition, lowestPos);
			}
			if (lowestPos < Parser.Position) {
				ResetEngineToPosition (lowestPos);
			}
		}

		public void ResetEngineToPosition (int position)
		{
			bool gotCachedEngine = false;
			while (cachedEngines.Count > 0) {
				T topEngine = cachedEngines.Peek ();
				if (topEngine.Position <= position) {
					Parser = (T)topEngine.Clone ();
					gotCachedEngine = true;
					ConsoleWrite ("Recovered state engine #{0}", cachedEngines.Count);
					break;
				} else {
					cachedEngines.Pop ();
				}
			}
			if (!gotCachedEngine) {
				ConsoleWrite ("Did not recover a state engine", cachedEngines.Count);
				Parser.Reset ();
			}
		}

		//Makes sure that the smart indent engine's cursor has caught up with the 
		//text editor's cursor.
		//The engine can take some time to parse the file, and we need it to be snappy
		//so we keep a stack of old engines (they're fairly lightweight) that we can clone
		//in order to quickly catch up.
		public void UpdatePosition (int position)
		{
			//bigger buffer means fewer saved stacks needed
			const int BUFFER_SIZE = 2000;


			ConsoleWrite ("moving backwards if currentEngine.Position {0} > position {1}", Parser.Position, position);

			if (Parser.Position == position) {
				//positions match, nothing to be done
				return;
			} else if (Parser.Position > position) {
				//moving backwards, so reset from previous saved location
				ResetEngineToPosition (position);
			}

			// get the engine caught up
			var snapshot = buffer.CurrentSnapshot;
			int nextSave = (cachedEngines.Count == 0) ? BUFFER_SIZE : cachedEngines.Peek ().Position + BUFFER_SIZE;
			while (Parser.Position < position) {
				char ch = snapshot[Parser.Position];
				Parser.Push (ch);
				ConsoleWrite ("pushing character '{0}'", ch);
				if (Parser.Position == nextSave)
					cachedEngines.Push ((T)Parser.Clone ());
			}
			ConsoleWrite ("Now state engine is at {0}, doc is at {1}", Parser.Position, position);
		}

		[System.Diagnostics.Conditional ("DEBUG_DOCUMENT_STATE_ENGINE_TRACKER")]
		void ConsoleWrite (string message, params object[] args)
		{
			Console.Write ("DocumentStateTracker: ");
			try {
				Console.WriteLine (message, args);
			} catch (Exception e) {
				Console.WriteLine ("\nError: Exception while formatting '{0}' for an array with {1} elements", message, args == null ? 0 : args.Length);
				Console.WriteLine (e);
			}
		}
	}
}