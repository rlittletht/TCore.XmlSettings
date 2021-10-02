using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RefApp
{
	public class Collection
	{
		public Collection() { }

		[TestCase(new string[] {"\\test"}, new string[]{"\\test"})]
		[TestCase(new string[] { "\\test", "\\test2" }, new string[] { "\\test", "\\test2" })]
		[TestCase(new string[] { "c:test" }, new string[] { "c:test" })]
		[TestCase(new string[] { "TestSubDir" }, new string[] { "C:\\Users\\$$USERNAME$$\\Documents\\Settings\\TestSubDir" })]
		[Test]
		public static void TestSearchPathInitialization(string[] rgsPaths, string[] rgsExpected)
		{
			TCore.XmlSettings.Collection collection = new TCore.XmlSettings.Collection(null, rgsPaths);

			Assert.AreEqual(rgsExpected.Length, collection.SearchDirs.Count);
			for (int i = 0; i < collection.SearchDirs.Count; i++)
			{
				string expected = rgsExpected[i];

				expected = expected.Replace("$$USERNAME$$", Environment.GetEnvironmentVariable("USERNAME"));
				Assert.AreEqual(expected, collection.SearchDirs[i]);
			}
		}

		[TestCase("foo.xml", "foo", ".xml")]
		[TestCase("foo.xml.xml", "foo", ".xml.xml")]
		[TestCase("foo..xml", "foo", "..xml")]
		[TestCase("\\foo.x.xml", "foo", ".x.xml")]
		[TestCase("s.\\x.xml", "x", ".xml")]
		[TestCase("s.s\\x.xml", "x", ".xml")]
		[TestCase("s.s\\.a.xml", ".a", ".xml")]
		[Test]
		public static void TestFileDescriptionConstruction(string sFullPath, string sNameExpected, string sExtensionExpected)
		{
			TCore.XmlSettings.Collection.FileDescription file = new TCore.XmlSettings.Collection.FileDescription(sFullPath);

			Assert.AreEqual(sNameExpected, file.Name);
			Assert.AreEqual(sExtensionExpected, file.Extension);
		}
	}
}
