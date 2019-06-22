//
// XObject.cs
//
// Author:
//   Mikayla Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;
using System.Text;

namespace MonoDevelop.Xml.Dom
{
	public abstract class XObject
	{
		protected XObject (int startOffset)
		{
			Span = new TextSpan (startOffset, 0);
		}

		protected XObject (TextSpan span)
		{
			this.Span = span;
		}

		public XObject Parent { get; internal protected set; }

		public IEnumerable<XNode> Parents {
			get {
				XNode next = Parent as XNode;
				while (next != null) {
					yield return next;
					next = next.Parent as XNode;
				}
			}
		}

		public TextSpan Span { get; private set; }

		public void End (int end)
		{
			Span = TextSpan.FromBounds (Span.Start, end);
		}

		/// <summary>
		/// Whether this node is fully parsed i.e. has an end position.
		/// </summary>
		public bool IsEnded {
			get { return Span.Length > 0; }
		}

		/// <summary>
		/// Whether this node is complete i.e. has a many end node, if applicable.
		/// </summary>
		public virtual bool IsComplete {
			get { return IsEnded; }
		}

		public virtual void BuildTreeString (StringBuilder builder, int indentLevel)
		{
			builder.Append (' ', indentLevel * 2);
			builder.AppendFormat (ToString ());
			builder.AppendLine ();
		}

		public override string ToString ()
		{
			return string.Format ("[{0} Location='{1}']", GetType (), Span);
		}

		//creates a parallel tree -- should NOT retain references into old tree
		public XObject ShallowCopy ()
		{
			XObject copy = NewInstance ();
			copy.ShallowCopyFrom (this);
			return copy;
		}

		protected abstract XObject NewInstance ();

		protected virtual void ShallowCopyFrom (XObject copyFrom)
		{
			Span = copyFrom.Span;
		}

		protected XObject () {}

		public virtual string FriendlyPathRepresentation {
			get { return GetType ().ToString (); }
		}
	}
}
