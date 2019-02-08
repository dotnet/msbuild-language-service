// 
// Parser.cs
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
using System.Collections.Generic;
using System.Text;

using MonoDevelop.Xml.Dom;

namespace MonoDevelop.Xml.Parser
{
	public class XmlParser : IXmlParserContext, ICloneable
	{
		XmlParserState previousState;
		int stateTag;
		StringBuilder keywordBuilder;

		public XmlParser (XmlRootState rootState, bool buildTree)
		{
			RootState = rootState;
			CurrentState = rootState;
			previousState = rootState;
			BuildTree = buildTree;
			Reset ();
		}

		XmlParser (XmlParser copyFrom)
		{
			BuildTree = false;

			RootState = copyFrom.RootState;
			CurrentState = copyFrom.CurrentState;
			previousState = copyFrom.previousState;

			Offset = copyFrom.Offset;
			stateTag = copyFrom.stateTag;
			keywordBuilder = new StringBuilder (copyFrom.keywordBuilder.ToString ());
			CurrentStateLength = copyFrom.CurrentStateLength;

			//clone the node stack
			var l = new List<XObject> (CopyXObjects (copyFrom.Nodes));
			l.Reverse ();
			Nodes = new NodeStack (l);
		}

		IEnumerable<XObject> CopyXObjects (IEnumerable<XObject> src)
		{
			foreach (XObject o in src)
				yield return o.ShallowCopy ();
		}

		public XmlRootState RootState { get; }

		#region IDocumentStateEngine

		public int Offset { get; private set; }

		public void Reset ()
		{
			CurrentState = RootState;
			previousState = RootState;
			Offset = 0;
			stateTag = 0;
			keywordBuilder = new StringBuilder ();
			CurrentStateLength = 0;
			Nodes = new NodeStack ();
			Nodes.Push (RootState.CreateDocument ());
		}

		public void Parse (System.IO.TextReader reader)
		{
			int i = reader.Read ();
			while (i >= 0) {
				char c = (char)i;
				Push (c);
				i = reader.Read ();
			}
		}

		public void Push (char c)
		{
			try {
				//FIXME: position/location should be at current char, not after it
				Offset++;

				for (int loopLimit = 0; loopLimit < 10; loopLimit++) {
					CurrentStateLength++;
					string rollback = null;
					if (CurrentState == null)
						return;
					XmlParserState nextState = CurrentState.PushChar (c, this, ref rollback);

					// no state change
					if (nextState == CurrentState || nextState == null)
						return;

					// state changed; reset stuff
					previousState = CurrentState;
					CurrentState = nextState;
					stateTag = 0;
					CurrentStateLength = 0;
					if (keywordBuilder.Length < 50)
						keywordBuilder.Length = 0;
					else
						keywordBuilder = new StringBuilder ();


					// only loop if the same char should be run through the new state
					if (rollback == null)
						return;

					//simple rollback, just run same char through again
					if (rollback.Length == 0)
						continue;

					//"complex" rollbacks require actually skipping backwards.
					//Note the previous state is invalid for this operation.

					//rollback position so it's valid
					Offset -= (rollback.Length + 1);

					foreach (char rollChar in rollback)
						Push (rollChar);

					//restore position
					Offset++;
				}
				throw new InvalidOperationException ("Too many state changes for char '" + c + "'. Current state is " + CurrentState.ToString () + ".");
			} catch (Exception ex) {
				//attach parser state to exceptions
				throw new Exception (ToString (), ex);
			}
		}

		object ICloneable.Clone ()
		{
			if (BuildTree)
				throw new InvalidOperationException ("Parser can only be cloned when in stack mode");
			return new XmlParser (this);
		}

		#endregion

		public XmlParser GetTreeParser ()
		{
			if (BuildTree)
				throw new InvalidOperationException ("Parser can only be cloned when in stack mode");
			var parser = new XmlParser (this) { BuildTree = true };

			//reconnect the node tree
			((IXmlParserContext)parser).ConnectAll ();

			parser.Diagnostics.Clear ();

			return parser;
		}

		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendFormat ("[Parser Location={0} CurrentStateLength={1}", Offset, CurrentStateLength);
			builder.AppendLine ();

			builder.Append (' ', 2);
			builder.AppendLine ("Stack=");

			XObject rootOb = null;
			foreach (XObject ob in Nodes) {
				rootOb = ob;
				builder.Append (' ', 4);
				builder.Append (ob.ToString ());
				builder.AppendLine ();
			}

			builder.Append (' ', 2);
			builder.AppendLine ("States=");
			XmlParserState s = CurrentState;
			while (s != null) {
				builder.Append (' ', 4);
				builder.Append (s.ToString ());
				builder.AppendLine ();
				s = s.Parent;
			}

			if (BuildTree && rootOb != null) {
				builder.Append (' ', 2);
				builder.AppendLine ("Tree=");
				rootOb.BuildTreeString (builder, 3);
			}

			if (BuildTree && Diagnostics.Count > 0) {
				builder.Append (' ', 2);
				builder.AppendLine ("Errors=");
				foreach (XmlDiagnosticInfo err in Diagnostics) {
					builder.Append (' ', 4);
					builder.AppendLine ($"[{err.Severity}@{err.Span}: {err.Message}]");
				}
			}

			builder.AppendLine ("]");
			return builder.ToString ();
		}

		#region IParseContext

		int IXmlParserContext.StateTag {
			get { return stateTag; }
			set { stateTag = value; }
		}

		StringBuilder IXmlParserContext.KeywordBuilder {
			get { return keywordBuilder; }
		}

		XmlParserState IXmlParserContext.PreviousState {
			get { return previousState; }
		}

		public int CurrentStateLength { get; private set; }
		public NodeStack Nodes { get; private set; }

		public bool BuildTree { get; private set; }

		void IXmlParserContext.Log (XmlDiagnosticInfo diagnostic)
		{
			Diagnostics.Add (diagnostic);
		}

		void IXmlParserContext.ConnectAll ()
		{
			XNode prev = null;
			foreach (XObject o in Nodes) {
				XContainer container = o as XContainer;
				if (prev != null && container != null && prev.IsComplete)
					container.AddChildNode (prev);
				if (o.Parent != null)
					break;
				prev = o as XNode;
			}
		}

		void IXmlParserContext.EndAll (bool pop)
		{
			int popCount = 0;
			foreach (XObject ob in Nodes) {
				if (!ob.IsEnded && !(ob is XDocument)) {
					ob.End (Offset);
					popCount++;
				} else {
					break;
				}
			}
			if (pop)
				for (; popCount > 0; popCount--)
					Nodes.Pop ();
		}

		#endregion

		public XmlParserState CurrentState { get; private set; }

		public List<XmlDiagnosticInfo> Diagnostics { get; } = new List<XmlDiagnosticInfo> ();
	}
}
