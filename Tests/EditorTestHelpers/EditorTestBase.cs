using Microsoft.VisualStudio.MiniEditor;
using MonoDevelop.Xml.Tests.Completion;
using NUnit.Framework;

namespace MonoDevelop.Xml.Tests.EditorTestHelpers
{
	public abstract class EditorTestBase
	{
		[OneTimeSetUp]
		public void InitializeEditorEnvironment ()
		{
			TestEnvironment.EnsureInitialized ();
		}

		public EditorEnvironment Environment => TestEnvironment.EditorEnvironment;
		public EditorCatalog Catalog => TestEnvironment.EditorCatalog;
	}
}
