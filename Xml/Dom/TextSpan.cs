// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MonoDevelop.Xml.Dom
{
	public struct TextSpan
	{
		public TextSpan (int start, int length)
		{
			Start = start;
			Length = length;
		}

        public int Start { get; }
        public int Length { get; }
		public int End => Start + Length;

		public bool Contains (int offset) => offset >= Start && offset <= End;

		public static TextSpan FromBounds (int start, int end) => new TextSpan (start, end - start);
	}
}
