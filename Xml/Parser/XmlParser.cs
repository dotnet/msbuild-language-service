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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MonoDevelop.Xml.Parser
{
	public class XmlParser : IXmlParserContext, ICloneable
	{
		XmlRootState rootState;
		XmlParserState currentState;
		XmlParserState previousState;
		bool buildTree;

		int offset;
		int stateTag;
		StringBuilder keywordBuilder;
		int currentStateLength;
		NodeStack nodes;

		public XmlParser (XmlRootState rootState, bool buildTree)
		{
			this.rootState = rootState;
			this.currentState = rootState;
			this.previousState = rootState;
			this.buildTree = buildTree;
			Reset ();
		}

		XmlParser (XmlParser copyFrom)
		{
			buildTree = false;

			rootState = copyFrom.rootState;
			currentState = copyFrom.currentState;
			previousState = copyFrom.previousState;

			offset = copyFrom.offset;
			stateTag = copyFrom.stateTag;
			keywordBuilder = new StringBuilder (copyFrom.keywordBuilder.ToString ());
			currentStateLength = copyFrom.currentStateLength;

			//clone the node stack
			var l = new List<XObject> (CopyXObjects (copyFrom.nodes));
			l.Reverse ();
			nodes = new NodeStack (l);
		}

		IEnumerable<XObject> CopyXObjects (IEnumerable<XObject> src)
		{
			foreach (XObject o in src)
				yield return o.ShallowCopy ();
		}

		public XmlRootState RootState { get { return rootState; } }

		#region IDocumentStateEngine

		public int Offset { get { return offset; } }

		public void Reset ()
		{
			currentState = rootState;
			previousState = rootState;
			offset = 0;
			stateTag = 0;
			keywordBuilder = new StringBuilder ();
			currentStateLength = 0;
			nodes = new NodeStack ();
			nodes.Push (rootState.CreateDocument ());
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
				offset++;

				for (int loopLimit = 0; loopLimit < 10; loopLimit++) {
					currentStateLength++;
					string rollback = null;
					if (currentState == null)
						return;
					XmlParserState nextState = currentState.PushChar (c, this, ref rollback);

					// no state change
					if (nextState == currentState || nextState == null)
						return;

					// state changed; reset stuff
					previousState = currentState;
					currentState = nextState;
					stateTag = 0;
					currentStateLength = 0;
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
					offset -= (rollback.Length + 1);

					foreach (char rollChar in rollback)
						Push (rollChar);

					//restore position
					offset++;
				}
				throw new InvalidOperationException ("Too many state changes for char '" + c + "'. Current state is " + currentState.ToString () + ".");
			} catch (Exception ex) {
				//attach parser state to exceptions
				throw new Exception (ToString (), ex);
			}
		}

		object ICloneable.Clone ()
		{
			if (buildTree)
				throw new InvalidOperationException ("Parser can only be cloned when in stack mode");
			return new XmlParser (this);
		}

		#endregion

		public XmlParser GetTreeParser ()
		{
			if (buildTree)
				throw new InvalidOperationException ("Parser can only be cloned when in stack mode");
			var parser = new XmlParser (this) { buildTree = true };

			//reconnect the node tree
			((IXmlParserContext)parser).ConnectAll ();

			parser.Diagnostics.Clear ();

			return parser;
		}

		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendFormat ("[Parser Location={0} CurrentStateLength={1}", offset, currentStateLength);
			builder.AppendLine ();

			builder.Append (' ', 2);
			builder.AppendLine ("Stack=");

			XObject rootOb = null;
			foreach (XObject ob in nodes) {
				rootOb = ob;
				builder.Append (' ', 4);
				builder.Append (ob.ToString ());
				builder.AppendLine ();
			}

			builder.Append (' ', 2);
			builder.AppendLine ("States=");
			XmlParserState s = currentState;
			while (s != null) {
				builder.Append (' ', 4);
				builder.Append (s.ToString ());
				builder.AppendLine ();
				s = s.Parent;
			}

			if (buildTree && rootOb != null) {
				builder.Append (' ', 2);
				builder.AppendLine ("Tree=");
				rootOb.BuildTreeString (builder, 3);
			}

			if (buildTree && Diagnostics.Count > 0) {
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

		public int CurrentStateLength { get { return currentStateLength; } }
		public NodeStack Nodes { get { return nodes; } }

		public bool BuildTree { get { return buildTree; } }

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

		public XmlParserState CurrentState { get { return currentState; } }

		public List<XmlDiagnosticInfo> Diagnostics { get; } = new List<XmlDiagnosticInfo> ();
	}
}
