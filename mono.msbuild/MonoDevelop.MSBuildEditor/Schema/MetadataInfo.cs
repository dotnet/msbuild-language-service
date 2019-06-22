﻿// Copyright (c) 2016 Xamarin Inc.
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MonoDevelop.MSBuildEditor.Schema
{
	class MetadataInfo : ValueInfo
	{
		public bool Reserved { get; }
		public bool Required { get; }

		public MetadataInfo (
			string name, DisplayText description,
			bool reserved = false, bool required = false,
			MSBuildValueKind valueKind = MSBuildValueKind.Unknown,
			ItemInfo item = null,
			List<ConstantInfo> values = null, string defaultValue = null)
			: base (name, description, valueKind, values, defaultValue)
		{
			Item = item;
			Required = required;
			Reserved = reserved;
		}

		public ItemInfo Item { get; internal set; }
    }
}