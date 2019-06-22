using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Core.Imaging;

namespace MonoDevelop.Xml.Editor.Completion
{
	public static class XmlImages
	{
		public static readonly Microsoft.VisualStudio.Text.Adornments.ImageElement ElementImage = new Microsoft.VisualStudio.Text.Adornments.ImageElement (new ImageId ());
		public static readonly Microsoft.VisualStudio.Text.Adornments.ImageElement AttributeImage = new Microsoft.VisualStudio.Text.Adornments.ImageElement (new ImageId ());
		public static readonly Microsoft.VisualStudio.Text.Adornments.ImageElement AttributeValueImage = new Microsoft.VisualStudio.Text.Adornments.ImageElement (new ImageId ());
		public static readonly Microsoft.VisualStudio.Text.Adornments.ImageElement NamespaceImage = new Microsoft.VisualStudio.Text.Adornments.ImageElement (new ImageId ());
	}
}
