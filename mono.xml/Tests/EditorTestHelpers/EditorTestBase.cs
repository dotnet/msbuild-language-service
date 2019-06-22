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
			(Environment, Catalog) = InitializeEnvironment ();
		}

		protected abstract (EditorEnvironment, EditorCatalog) InitializeEnvironment ();

		public virtual EditorEnvironment Environment { get; private set; }
		public virtual EditorCatalog Catalog { get; private set; }
	}
}
