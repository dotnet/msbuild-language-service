using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoDevelop.Xml.Editor.IntelliSense;
using MonoDevelop.Xml.Parser;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace MonoDevelop.Xml.Tests.Completion
{
	[TestFixture]
	public class CompletionTriggerTests
	{
		[Test]
		[TestCase("<", XmlCompletionTrigger.Element)]
		[TestCase("", '<', XmlCompletionTrigger.Element)]
		[TestCase("<foo", XmlCompletionTrigger.Element, 3)]
		public void TriggerTests (object[] args)
		{
			string doc = (string)args[0];
			char triggerChar = (args[1] as char?) ?? '\0';
			var kind = (XmlCompletionTrigger)(args[1] is char ? args[2] : args[1]);
			int length = args[args.Length - 1] as int? ?? 0;

			var spine = new XmlParser (new XmlRootState (), false);
			spine.Parse (new StringReader (doc));

			var result = XmlCompletionTriggering.GetTrigger (spine, triggerChar);
			Assert.AreEqual (result.kind, kind);
			Assert.AreEqual (result.length, length);
		}
	}
}
