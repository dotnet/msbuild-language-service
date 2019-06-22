﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Ide.Editor.Extension;
using MonoDevelop.Ide.FindInFiles;
using MonoDevelop.MSBuildEditor.Language;

namespace MonoDevelop.MSBuildEditor
{
	class MSBuildHighlightUsagesExtension : AbstractUsagesExtension<MSBuildResolveResult>
	{
		//FIXME docs say this is called on background thread but not true
		protected override Task<IEnumerable<MemberReference>> GetReferencesAsync (MSBuildResolveResult resolveResult, CancellationToken token)
		{
			if (resolveResult == null) {
				return Task.FromResult (Enumerable.Empty<MemberReference> ());
			}

			var ext = Editor.GetContent<MSBuildTextEditorExtension> ();
			var doc = ext.GetDocument ();
			var collector = MSBuildReferenceCollector.Create (resolveResult);

			//FIXME: it should be possible to run this async, the doc is immutable
			collector.Run (doc);

			return Task.FromResult (
				collector.Results.Select (r => {
					var usage = ReferenceUsageType.Unknown;
					switch (r.Usage) {
					case ReferenceUsage.Write:
						usage = ReferenceUsageType.Write;
						break;
					case ReferenceUsage.Declaration:
						usage = ReferenceUsageType.Declaration;
						break;
					case ReferenceUsage.Read:
						usage = ReferenceUsageType.Read;
						break;
					}
					return new MemberReference (r, doc.Filename, r.Offset, r.Length) {
						ReferenceUsageType = usage
					};
				})
			);
		}

		protected override Task<MSBuildResolveResult> ResolveAsync (CancellationToken token)
		{
			var ext = Editor.GetContent<MSBuildTextEditorExtension> ();

			//FIXME can we cache this? maybe make it async?
			var rr = ext.ResolveCurrentLocation ();
			if (rr == null) {
				//FIXME: AbstractUsagesExtension doesn't like us returning null directly
				return Task.FromResult<MSBuildResolveResult> (null);
			}

			switch (rr.ReferenceKind) {
			case MSBuildReferenceKind.Metadata:
				if (rr.ReferenceAsMetadata.itemName != null) {
					return Task.FromResult (rr);
				}
				break;
			case MSBuildReferenceKind.Item:
			case MSBuildReferenceKind.Property:
			case MSBuildReferenceKind.Target:
			case MSBuildReferenceKind.ItemFunction:
			case MSBuildReferenceKind.PropertyFunction:
			case MSBuildReferenceKind.StaticPropertyFunction:
			case MSBuildReferenceKind.ClassName:
			case MSBuildReferenceKind.Enum:
			case MSBuildReferenceKind.Task:
				return Task.FromResult (rr);
			}

			return Task.FromResult<MSBuildResolveResult> (null);
		}
	}
}
