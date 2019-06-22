﻿// Copyright (c) 2016 Xamarin Inc.
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Ide.TypeSystem;
using MonoDevelop.MSBuildEditor.Language;
using MonoDevelop.Xml.Editor;

namespace MonoDevelop.MSBuildEditor
{
	class MSBuildParsedDocument : XmlParsedDocument
	{
		public MSBuildRootDocument Document { get; internal set; }

		public MSBuildParsedDocument (string filename) : base (filename)
		{
		}

		internal static ParsedDocument ParseInternal (ParseOptions options, CancellationToken token)
		{
			var oldDoc = (options.OldParsedDocument as MSBuildParsedDocument)?.Document;

			var runtimeInformation =
				oldDoc
				?.RuntimeInformation
				 ?? new MSBuildRuntimeInformation (Runtime.SystemAssemblyService.CurrentRuntime, MSBuildToolsVersion.Unknown);
			
			var schemaProvider = new MonoDevelopMSBuildSchemaProvider ();

			var doc = new MSBuildParsedDocument (options.FileName);

			doc.Flags |= ParsedDocumentFlags.NonSerializable;
			doc.Document = MSBuildRootDocument.Parse (options.FileName, options.Content, oldDoc, schemaProvider, runtimeInformation, token);
			doc.XDocument = doc.Document.XDocument;

			return doc;
		}

		public override Task<IReadOnlyList<Error>> GetErrorsAsync (CancellationToken cancellationToken = default (CancellationToken))
		{
			return Task.FromResult<IReadOnlyList<Error>> (Document.Errors);
		}
	}
}
