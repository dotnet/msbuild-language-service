// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MonoDevelop.Xml.Editor.IntelliSense
{
	//FIXME: replace this MD shim with something better
	internal class LoggingService
	{
		internal static void LogWarning (string formatString, params object[] args) => Console.WriteLine (formatString, args);
		internal static void LogDebug (string formatString, params object[] args) => Console.WriteLine (formatString, args);
	}
}
