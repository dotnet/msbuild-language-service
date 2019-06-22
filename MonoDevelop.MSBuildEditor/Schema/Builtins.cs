﻿// Copyright (c) 2015 Xamarin Inc.
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MonoDevelop.MSBuildEditor.Schema
{

	static class Builtins
	{
		public static Dictionary<string, MetadataInfo> Metadata { get; } = new Dictionary<string, MetadataInfo> ();
		public static Dictionary<string, PropertyInfo> Properties { get; } = new Dictionary<string, PropertyInfo> ();

		static void AddMetadata (string name, string description, MSBuildValueKind kind = MSBuildValueKind.Unknown, bool notReserved = false)
		{
			Metadata.Add (name, new MetadataInfo (name, description, !notReserved, false, kind));
		}

		static void AddProperty (string name, string description, MSBuildValueKind kind = MSBuildValueKind.Unknown, bool notReserved = false)
		{
			Properties.Add (name, new PropertyInfo (name, description, !notReserved, kind));
		}

		static Builtins ()
		{
			AddMetadata ("FullPath", "The full path of the item", MSBuildValueKind.File);
			AddMetadata ("RootDir", "The root directory of the item", MSBuildValueKind.FolderWithSlash);
			AddMetadata ("Filename", "The filename of the item", MSBuildValueKind.Filename);
			AddMetadata ("Extension", "The file extension of the item", MSBuildValueKind.Extension);
			AddMetadata ("RelativeDir", "The path specified in the Include attribute", MSBuildValueKind.FolderWithSlash);
			AddMetadata ("Directory", "The directory of the item, excluding the root directory", MSBuildValueKind.FolderWithSlash);
			AddMetadata ("RecursiveDir", "If the item contained a ** wildstar, the value to which it was expanded", MSBuildValueKind.FolderWithSlash);
			AddMetadata ("Identity", "The value specified in the Include attribute", MSBuildValueKind.MatchItem);
			AddMetadata ("ModifiedTime", "The time the the item was last modified", MSBuildValueKind.DateTime);
			AddMetadata ("CreatedTime", "The time the the item was created", MSBuildValueKind.DateTime);
			AddMetadata ("AccessedTime", "The time the the item was last accessed", MSBuildValueKind.DateTime);
			AddMetadata ("DefiningProjectFullPath", "The full path of the project in which this item was defined", MSBuildValueKind.File);
			AddMetadata ("DefiningProjectDirectory", "The directory of the project in which this item was defined", MSBuildValueKind.Folder);
			AddMetadata ("DefiningProjectName", "The name of the project in which this item was defined", MSBuildValueKind.Filename);
			AddMetadata ("DefiningProjectExtension", "The extension of the project in which this item was defined", MSBuildValueKind.Extension);

			AddProperty ("MSBuildBinPath", "Absolute path of the bin directory where MSBuild is located. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildExtensionsPath", "Absolute path of the MSBuild extensions directory for the current architecture. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildExtensionsPath32", "Absolute path of the 32-bit MSBuild extensions directory. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildExtensionsPath64", "Absolute path of the 64-bit MSBuild extensions directory. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildLastTaskResult", "True if the last task completed without errors.", MSBuildValueKind.Bool);
			AddProperty ("MSBuildNodeCount", "The number of concurrent build nodes.", MSBuildValueKind.Int);
			AddProperty ("MSBuildProgramFiles32", "Absolute path of the 32-bit Program Files folder. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildProjectDefaultTargets", "The value of the DefaultTargets attribute in the Project element.", MSBuildValueKind.TargetName.List ());
			AddProperty ("MSBuildProjectDirectory", "Directory where the project file is located. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildProjectDirectoryNoRoot", "Directory where the project file is located, excluding drive root. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildProjectFile", "Name of the project file, including extension.", MSBuildValueKind.File);
			AddProperty ("MSBuildProjectExtension", "Extension of the project file.", MSBuildValueKind.Extension);
			AddProperty ("MSBuildProjectFullPath", "Full path of the project file.", MSBuildValueKind.File);
			AddProperty ("MSBuildProjectName", "Name of the project file, excluding extension.", MSBuildValueKind.Filename);
			AddProperty ("MSBuildStartupDirectory", "Absolute path of the directory where MSBuild is invoked. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildThisFile", "Name of the current MSBuild file, including extension.", MSBuildValueKind.Filename);
			AddProperty ("MSBuildThisFileDirectory", "Directory where the current MSBuild file is located.", MSBuildValueKind.FolderWithSlash);
			AddProperty ("MSBuildThisFileDirectoryNoRoot", "Directory where the current MSBuild file is located, excluding drive root.", MSBuildValueKind.FolderWithSlash);
			AddProperty ("MSBuildThisFileExtension", "Extension of the current MSBuild file.", MSBuildValueKind.Extension);
			AddProperty ("MSBuildThisFileFullPath", "Absolute path of the current MSBuild file.", MSBuildValueKind.File);
			AddProperty ("MSBuildThisFileName", "Name of the current MSBuild file, excluding extension.", MSBuildValueKind.File);
			AddProperty ("MSBuildToolsPath", "Path to the current toolset, specfied by the MSBuildToolsVersion. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildToolsVersion", "Version of the current toolset", MSBuildValueKind.ToolsVersion);
			AddProperty ("MSBuildUserExtensionsPath", "Directory from which user extensions are imported. Does not include final backslash.", MSBuildValueKind.Folder);
			AddProperty ("MSBuildRuntimeType", "The runtime on which MSBuild is running", MSBuildValueKind.HostRuntime);
			AddProperty ("MSBuildOverrideTasksPath", "Path to files that override built-in tasks", MSBuildValueKind.Folder);
			AddProperty ("DefaultOverrideToolsVersion", "Tools version to override the built-in tools version", MSBuildValueKind.ToolsVersion);
			AddProperty ("MSBuildAssemblyVersion", "The version of the MSBuild assemblies", MSBuildValueKind.Version);
			AddProperty ("OS", "The OS on which MSBuild is running", MSBuildValueKind.HostOS);
			AddProperty ("MSBuildFrameworkToolsRoot", "The root directory of the .NET framework tools", MSBuildValueKind.FolderWithSlash);

			AddProperty ("MSBuildTreatWarningsAsErrors", "Name of the property that indicates that all warnings should be treated as errors", MSBuildValueKind.PropertyName, true);
			AddProperty ("MSBuildWarningsAsErrors", "Name of the property that indicates a list of warnings to treat as errors", MSBuildValueKind.PropertyName, true);
			AddProperty ("MSBuildWarningsAsMessages", "Name of the property that indicates the list of warnings to treat as messages", MSBuildValueKind.PropertyName, true);
		}
	}
}