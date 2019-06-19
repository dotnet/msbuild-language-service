using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text.Adornments;
using MonoDevelop.Xml.Completion;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonoDevelop.Xml.Tests.Schema
{
	[TestFixture]
	public abstract class SchemaTestFixtureBase
	{
		XmlSchemaCompletionProvider schemaCompletionData;

		/// <summary>
		/// Gets the <see cref="XmlSchemaCompletionProvider"/> object generated
		/// by this class.
		/// </summary>
		/// <remarks>This object will be null until the <see cref="FixtureInitBase"/>
		/// has been run.</remarks>
		internal XmlSchemaCompletionProvider SchemaCompletionData {
			get {
				return schemaCompletionData;
			}
		}
		
		/// <summary>
		/// Creates the <see cref="XmlSchemaCompletionProvider"/> object from 
		/// the derived class's schema.
		/// </summary>
		/// <remarks>Calls <see cref="FixtureInit"/> at the end of the method.
		/// </remarks>
		[OneTimeSetUp]
		public void FixtureInitBase()
		{
			schemaCompletionData = CreateSchemaCompletionDataObject();
			FixtureInit();
		}
		
		/// <summary>
		/// Method overridden by derived class so it can execute its own
		/// fixture initialisation.
		/// </summary>
		public virtual void FixtureInit()
		{
		}

		/// <summary>
		/// Checks whether the specified name exists in the completion data.
		/// </summary>
		public static bool Contains(CompletionContext items, string name)
		{
			bool Contains = false;
			
			foreach (var data in items.Items) {
				if (data.DisplayText == name) {
					Contains = true;
					break;
				}
			}
				
			return Contains;
		}
		
		/// <summary>
		/// Checks whether the completion data specified by name has
		/// the correct description.
		/// </summary>
		public static async Task<bool> ContainsDescription(CompletionContext items, string name, string description)
		{
			bool Contains = false;
			
			foreach (var data in items.Items) {
				if (data.DisplayText == name) {
					var descEl = await data.GetDocumentationAsync () as ClassifiedTextElement;
					if (descEl != null && descEl.Runs.FirstOrDefault()?.Text == description) {
						Contains = true;
						break;						
					}
				}
			}
				
			return Contains;
		}		
		
		/// <summary>
		/// Gets a count of the number of occurrences of a particular name
		/// in the completion data.
		/// </summary>
		public static int GetItemCount(CompletionContext items, string name)
		{
			int count = 0;
			
			foreach (var data in items.Items) {
				if (data.DisplayText == name) {
					++count;
				}
			}
			
			return count;
		}
		
		/// <summary>
		/// Returns the schema that will be used in this test fixture.
		/// </summary>
		/// <returns></returns>
		protected virtual string GetSchema()
		{
			return String.Empty;
		}
		
		/// <summary>
		/// Creates an <see cref="XmlSchemaCompletionProvider"/> object that 
		/// will be used in the test fixture.
		/// </summary>
		internal virtual XmlSchemaCompletionProvider CreateSchemaCompletionDataObject()
		{
			StringReader reader = new StringReader(GetSchema());
			return new XmlSchemaCompletionProvider(reader);
		}

		protected async Task AssertDescription(string expected, CompletionItem item)
        {
			var description = (ClassifiedTextElement) await item.GetDocumentationAsync ();
			Assert.AreEqual (expected, description.Runs.First ().Text);
		}
	}
}
