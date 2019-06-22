// Copyright (c) 2014 Xamarin Inc.
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Highlighting;
using MonoDevelop.MSBuildEditor.Language;
using MonoDevelop.MSBuildEditor.PackageSearch;
using MonoDevelop.MSBuildEditor.Schema;
using MonoDevelop.Xml.Editor.Completion;
using MonoDevelop.Xml.Dom;
using MonoDevelop.Xml.Editor;
using MonoDevelop.Xml.Parser;
using ProjectFileTools.NuGetSearch.Contracts;
using ProjectFileTools.NuGetSearch.Feeds;
using ProjectFileTools.NuGetSearch.Feeds.Disk;
using ProjectFileTools.NuGetSearch.Feeds.Web;
using ProjectFileTools.NuGetSearch.IO;
using ProjectFileTools.NuGetSearch.Search;

namespace MonoDevelop.MSBuildEditor
{
	class MSBuildTextEditorExtension : BaseXmlEditorExtension
	{
		public static readonly string MSBuildMimeType = "application/x-msbuild";

		public IPackageSearchManager PackageSearchManager { get; set; }

		protected override void Initialize ()
		{
			base.Initialize ();

			//we don't have a MEF composition here, set it up manually
			PackageSearchManager = new PackageSearchManager (
				new MonoDevelopPackageFeedRegistry (),
				new PackageFeedFactorySelector (new IPackageFeedFactory [] {
					new NuGetDiskFeedFactory (new FileSystem()),
					new NuGetV3ServiceFeedFactory (new WebRequestFactory()),
				})
			);

			CheckHighlighting ();
		}

		//HACK: work around https://github.com/mono/monodevelop/issues/3438
		void CheckHighlighting ()
		{
			const System.Reflection.BindingFlags privateStatic
				= System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;

			Type shs = typeof (SyntaxHighlightingService);
			if (shs.GetField ("extensionBundle", privateStatic) != null) {
				return;
			}

			var meth = shs.GetMethod ("GetSyntaxHighlightingDefinitionByName", privateStatic);
			if (meth == null) {
				return;
			}

			//HACK: the props extension is the only MSBuild extension that is not handled by the built-in highlighting
			var highlighting = (SyntaxHighlightingDefinition) meth.Invoke (null, new object [] { new FilePath ("a.props") });
			if (highlighting == null) {
				return;
			}

			var old = Editor.SyntaxHighlighting;
			Editor.SyntaxHighlighting = new SyntaxHighlighting (highlighting, Editor);
			old.Dispose ();
		}

		public MSBuildRootDocument GetDocument ()
		{
			return ((MSBuildParsedDocument)DocumentContext.ParsedDocument)?.Document;
		}

		public override Task<ICompletionDataList> HandleCodeCompletionAsync (CodeCompletionContext completionContext, CompletionTriggerInfo triggerInfo, CancellationToken token = default (CancellationToken))
		{
			var doc = GetDocument ();
			if (doc != null) {
				var rr = ResolveCurrentLocation ();
				if (rr?.LanguageElement != null) {
					var expressionCompletion = HandleExpressionCompletion (rr, token);
					if (expressionCompletion != null) {
						return expressionCompletion;
					}
				}
			}

			return base.HandleCodeCompletionAsync (completionContext, triggerInfo, token);
		}

		internal MSBuildResolveResult ResolveAt (int offset)
		{
			var doc = GetDocument ();
			if (Tracker == null || doc == null) {
				return null;
			}
			Tracker.UpdateEngine (offset);
			return MSBuildResolver.Resolve (Tracker.Engine, Editor.CreateDocumentSnapshot (), doc);
		}

		internal MSBuildResolveResult ResolveCurrentLocation ()
		{
			var doc = GetDocument ();
			if (Tracker == null || doc == null) {
				return null;
			}
			Tracker.UpdateEngine ();
			return MSBuildResolver.Resolve (Tracker.Engine, Editor.CreateDocumentSnapshot (), doc);
		}

		protected override Task<CompletionDataList> GetElementCompletions (CancellationToken token)
		{
			var list = new CompletionDataList ();
			AddMiscBeginTags (list);

			var rr = ResolveCurrentLocation ();
			if (rr != null) {
				var doc = GetDocument ();
				foreach (var el in rr.GetElementCompletions (doc)) {
					list.Add (new MSBuildCompletionData (el, doc, rr, XmlCompletionData.DataType.XmlElement));
				}
			}

			return Task.FromResult (list);
		}

		protected override Task<CompletionDataList> GetAttributeCompletions (IAttributedXObject attributedOb,
			Dictionary<string, string> existingAtts, CancellationToken token)
		{
			var rr = ResolveCurrentLocation ();
			if (rr?.LanguageElement == null)
				return null;

			var doc = GetDocument ();
			var list = new CompletionDataList ();
			foreach (var att in rr.GetAttributeCompletions (doc, doc.ToolsVersion)) {
				if (!existingAtts.ContainsKey (att.Name)) {
					list.Add (new MSBuildCompletionData (att, doc, rr, XmlCompletionData.DataType.XmlAttribute));
				}
			}

			return Task.FromResult (list);
		}

		Task<ICompletionDataList> GetPackageNameCompletions (MSBuildRootDocument doc, int startIdx, int triggerLength)
		{
			string name = ((IXmlParserContext)Tracker.Engine).KeywordBuilder.ToString ();
			if (string.IsNullOrWhiteSpace (name)) {
				return null;
			}

			return Task.FromResult<ICompletionDataList> (
				new PackageNameSearchCompletionDataList (name, PackageSearchManager, doc.GetTargetFrameworkNuGetSearchParameter ()) {
					TriggerWordStart = startIdx,
					TriggerWordLength = triggerLength
				}
			);
		}

		Task<ICompletionDataList> GetPackageVersionCompletions (MSBuildRootDocument doc, MSBuildResolveResult rr, int startIdx, int triggerLength)
		{
			var name = rr.XElement.Attributes.FirstOrDefault (a => a.Name.FullName == "Include")?.Value;
			if (string.IsNullOrEmpty (name)) {
				return null;
			}
			return Task.FromResult<ICompletionDataList> (
				new PackageVersionSearchCompletionDataList (PackageSearchManager, doc.GetTargetFrameworkNuGetSearchParameter (), name){
					TriggerWordStart = startIdx,
					TriggerWordLength = triggerLength
				}
			);
		}

		Task<ICompletionDataList> GetSdkCompletions (int triggerLength, CancellationToken token)
		{
			var list = new CompletionDataList { TriggerWordLength = triggerLength };
			var doc = GetDocument ();
			if (doc == null) {
				return null;
			}

			var sdks = new HashSet<string> ();

			foreach (var sdk in doc.RuntimeInformation.GetRegisteredSdks ()) {
				if (sdks.Add (sdk.Name)) {
					list.Add (Path.GetFileName (sdk.Name));
				}
			}

			//TODO: how can we find SDKs in the non-default locations?
			return Task.Run<ICompletionDataList> (() => {
				foreach (var d in Directory.GetDirectories (doc.RuntimeInformation.GetSdksPath ())) {
					string name = Path.GetFileName (d);
					if (sdks.Add (name)) {
						list.Add (name);
					}
				}
				return list;
			}, token);
		}

		Task<ICompletionDataList> HandleExpressionCompletion (MSBuildResolveResult rr, CancellationToken token)
		{
			var doc = GetDocument ();

			if (!ExpressionCompletion.IsPossibleExpressionCompletionContext (Tracker.Engine)) {
				return null;
			}

			string expression = GetAttributeOrElementTextToCaret ();

			var triggerState = ExpressionCompletion.GetTriggerState (
				expression,
				rr.IsCondition (),
				out int triggerLength,
				out ExpressionNode triggerExpression,
				out IReadOnlyList<ExpressionNode> comparandVariables
			);

			if (triggerState == ExpressionCompletion.TriggerState.None) {
				return null;
			}

			var info = rr.GetElementOrAttributeValueInfo (doc);
			if (info == null) {
				return null;
			}

			var kind = MSBuildCompletionExtensions.InferValueKindIfUnknown (info);

			if (!ExpressionCompletion.ValidateListPermitted (ref triggerState, kind)) {
				return null;
			}

			bool allowExpressions = kind.AllowExpressions ();

			kind = kind.GetScalarType ();

			if (kind == MSBuildValueKind.Data || kind == MSBuildValueKind.Nothing) {
				return null;
			}

			var list = new CompletionDataList { TriggerWordLength = triggerLength, AutoSelect = false };

			if (comparandVariables != null && triggerState == ExpressionCompletion.TriggerState.Value) {
				foreach (var ci in ExpressionCompletion.GetComparandCompletions (doc, comparandVariables)) {
					list.Add (new MSBuildCompletionData (ci, doc, rr, XmlCompletionData.DataType.XmlAttributeValue));
				}
			}

			if (triggerState == ExpressionCompletion.TriggerState.Value) {
				switch (kind) {
				case MSBuildValueKind.NuGetID:
					return GetPackageNameCompletions (doc, Editor.CaretOffset - triggerLength, triggerLength);
				case MSBuildValueKind.NuGetVersion:
					return GetPackageVersionCompletions (doc, rr, Editor.CaretOffset - triggerLength, triggerLength);
				case MSBuildValueKind.Sdk:
				case MSBuildValueKind.SdkWithVersion:
					return GetSdkCompletions (triggerLength, token);
				case MSBuildValueKind.Guid:
					list.Add (new GenerateGuidCompletionData ());
					break;
				case MSBuildValueKind.Lcid:
					foreach (var culture in System.Globalization.CultureInfo.GetCultures (System.Globalization.CultureTypes.AllCultures)) {
						string name = culture.Name;
						string id = culture.LCID.ToString ();
						string display = culture.DisplayName;
						//insert multiple versions for matching on both the name and the number
						list.Add (new CompletionData (id, null, display));
						list.Add (new CompletionData (display, null, id, id));
					}
					break;
				}
			}

			//TODO: better metadata support

			IEnumerable<BaseInfo> cinfos;
			if (info.Values != null && info.Values.Count > 0 && triggerState == ExpressionCompletion.TriggerState.Value) {
				cinfos = info.Values;
			} else {
				cinfos = ExpressionCompletion.GetCompletionInfos (rr, triggerState, kind, triggerExpression, triggerLength, doc);
			}

			if (cinfos != null) {
				foreach (var ci in cinfos) {
					list.Add (new MSBuildCompletionData (ci, doc, rr, XmlCompletionData.DataType.XmlAttributeValue));
				}
			}

			if (allowExpressions && triggerState == ExpressionCompletion.TriggerState.Value) {
				list.Add (new CompletionDataWithSkipCharAndRetrigger ("$(", "md-variable", "Property value reference", "$(|)", ')'));
				list.Add (new CompletionDataWithSkipCharAndRetrigger ("@(", "md-variable", "Item list reference", "@(|)", ')'));
			}

			if (list.Count > 0) {
				return Task.FromResult<ICompletionDataList> (list);
			}

			return null;
		}

		//FIXME: move this down to XML layer
		string GetAttributeOrElementTextToCaret ()
		{
			int currentPosition = Editor.CaretOffset;
			int lineStart = Editor.GetLine (Editor.CaretLine).Offset;
			int expressionStart = currentPosition - Tracker.Engine.CurrentStateLength;
			if (Tracker.Engine.CurrentState is XmlAttributeValueState && Tracker.Engine.GetAttributeValueDelimiter () != 0) {
				expressionStart += 1;
			}
			int start = Math.Max (expressionStart, lineStart);
			var expression = Editor.GetTextAt (start, currentPosition - start);
			return expression;
		}

		[CommandHandler (Refactoring.RefactoryCommands.GotoDeclaration)]
		void GotoDefinition ()
		{
			var rr = ResolveCurrentLocation ();
			var doc = GetDocument ();
			var result = MSBuildNavigation.GetNavigation (doc, Editor.CaretLocation, rr);
			MSBuildNavigationExtension.Navigate (result, doc);
		}

		[CommandUpdateHandler (Refactoring.RefactoryCommands.GotoDeclaration)]
		void UpdateGotoDefinition (CommandInfo info)
		{
			var rr = ResolveCurrentLocation ();
			var doc = GetDocument ();
			info.Enabled = rr != null && MSBuildNavigation.CanNavigate (doc, Editor.CaretLocation, rr);
		}

		[CommandHandler (Refactoring.RefactoryCommands.FindReferences)]
		void FindReferences ()
		{
			var rr = ResolveCurrentLocation ();
			var doc = GetDocument ();
			MSBuildNavigationExtension.FindReferences (() => MSBuildReferenceCollector.Create (rr), doc);
		}

		[CommandUpdateHandler (Refactoring.RefactoryCommands.FindReferences)]
		void UpdateFindReferences (CommandInfo info)
		{
			var rr = ResolveCurrentLocation ();
			info.Enabled = MSBuildReferenceCollector.CanCreate (rr);
		}

		static string GetCounterpartFile (string name)
		{
			switch (Path.GetExtension (name.ToLower ())) {
			case ".targets":
				name = Path.ChangeExtension (name, ".props");
				break;
			case ".props":
				name = Path.ChangeExtension (name, ".targets");
				break;
			default:
				return null;
			}
			return File.Exists (name) ? name : null;
		}

		[CommandHandler (DesignerSupport.Commands.SwitchBetweenRelatedFiles)]
		protected void Run ()
		{
			var counterpart = GetCounterpartFile (FileName);
			IdeApp.Workbench.OpenDocument (counterpart, DocumentContext.Project, true);
		}

		[CommandUpdateHandler (DesignerSupport.Commands.SwitchBetweenRelatedFiles)]
		protected void Update (CommandInfo info)
		{
			info.Enabled = GetCounterpartFile (FileName) != null;
		}
	}
}