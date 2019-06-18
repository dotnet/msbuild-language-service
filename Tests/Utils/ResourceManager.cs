using System.IO;
using System.Reflection;
using System.Xml;

namespace MonoDevelop.Xml.Tests.Utils
{
	/// <summary>
	/// Returns strings from the embedded test resources.
	/// </summary>
	public class ResourceManager
	{
		static ResourceManager manager;
		
		static ResourceManager()
		{
			manager = new ResourceManager();
		}
		
		/// <summary>
		/// Returns the xhtml strict schema xml.
		/// </summary>
		public static TextReader GetXhtmlStrictSchema ()
		{
			return manager.GetText("xhtml1-strict.xsd");
		}
		
		/// <summary>
		/// Returns the xsd schema.
		/// </summary>
		public static TextReader GetXsdSchema ()
		{
			return manager.GetText("XMLSchema.xsd");
		}
		
		/// <summary>
		/// Returns the xml read from the specified file which is embedded
		/// in this assembly as a resource.
		/// </summary>
		public TextReader GetText (string fileName)
		{
			TextReader reader = null;
			
			Assembly assembly = Assembly.GetAssembly(this.GetType());
			
			Stream resourceStream = assembly.GetManifestResourceStream(fileName);
			if (resourceStream != null) {
				reader = new StreamReader(resourceStream);
			}
			
			return reader;
		}
		
	}
}
