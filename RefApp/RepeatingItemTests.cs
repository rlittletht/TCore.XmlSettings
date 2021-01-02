using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TCore.XmlSettings;

namespace RefApp
{
	public class RepeatingItemTests
	{
		public RepeatingItemTests() { }

		class RepeatSettings
		{
			public int NumFoo { get; set; }
			public string StringBar { get; set; }
			public List<string> StringsBar { get; set; }

			public class Nested
			{
				public string Name { get; set; }
				public int NestedNumFoo { get; set; }
				public List<string> NestedStrings { get; set; }
			}
			
			public Dictionary<string, Nested> MapNested { get; set; }

			public static void SetNumFooValueSmart(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItem repeatItem)
			{
				if (repeatItem != null)
				{
					Nested nested = (Nested)repeatItem.RepeatKey;
					nested.NestedNumFoo = Int32.Parse(value);
				}
				else
				{
					settings.NumFoo = Int32.Parse(value);
				}
			} 
			
			public static string GetNumFooValueSmart(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItem repeatItem)
			{
				if (repeatItem != null)
				{
					Nested nested = (Nested)repeatItem.RepeatKey;
					return nested.NestedNumFoo.ToString();
				}
				return settings.NumFoo.ToString();
			}

			public static void SetNumFooValue(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => settings.NumFoo = Int32.Parse(value);
			public static string GetNumFooValue(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => settings.NumFoo.ToString();
			public static void SetStringBarValue(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => settings.StringBar = value;
			public static string GetStringBarValue(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => settings.StringBar;

			public static void SetNumFooValueNested(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => ((Nested)repeatItem.RepeatKey).NestedNumFoo = Int32.Parse(value);
			public static string GetNumFooValueNested(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => ((Nested)repeatItem.RepeatKey).NestedNumFoo.ToString();

			public static void SetNestedName(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => ((Nested)repeatItem.RepeatKey).Name = value;
			public static string GetNestedName(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => ((Nested)repeatItem.RepeatKey).Name;

			public static void SetCollectionItem(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => ((string[])repeatItem.RepeatKey)[0] = value;
			public static string GetCollectionItem(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItem repeatItem) => ((string[])repeatItem.RepeatKey)[0];
			
			public static RepeatContext<RepeatSettings>.RepeatItem CreateCollectionRepeatItem(Element<RepeatSettings> element, RepeatContext<RepeatSettings>.RepeatItem parent) => new RepeatContext<RepeatSettings>.RepeatItem(element, parent, new string[1]);

			public static void CommitCollectionRepeatItem(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItem item)
			{
				if (settings.StringsBar == null)
					settings.StringsBar = new List<string>();

				settings.StringsBar.Add(((string[]) item.RepeatKey)[0]);
			}

			public static void CommitNestedCollectionRepeatItem(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItem item)
			{
				Nested nested = (Nested) item.Parent.RepeatKey; 
				
				if (nested.NestedStrings == null)
					nested.NestedStrings = new List<string>();

				nested.NestedStrings.Add(((string[])item.RepeatKey)[0]);
			}

			// now to build our
			public static RepeatContext<RepeatSettings>.RepeatItem CreateNestedRepeatItem(Element<RepeatSettings> element, RepeatContext<RepeatSettings>.RepeatItem parent) => new RepeatContext<RepeatSettings>.RepeatItem(element, parent, new Nested());

			public static void CommitRepeatItem(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItem item)
			{
				Nested nested = (Nested) item.RepeatKey;
				if (settings.MapNested == null)
					settings.MapNested = new Dictionary<string, Nested>();
				
				settings.MapNested.Add(nested.Name, nested);
			}
		}
		
		[Test]
		public static void TestRepeatingClass()
		{
			RepeatSettings settings = new RepeatSettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<RepeatSettings> description =
				XmlDescriptionBuilder<RepeatSettings>
					.Build(ns, "refSettings")
					.AddChildElement("NumFoo", RepeatSettings.GetNumFooValue, RepeatSettings.SetNumFooValue)
					.AddElement("StringBar", RepeatSettings.GetStringBarValue, RepeatSettings.SetStringBarValue)
					.AddElement("Nesteds")
					.AddChildElement("Nested")
					.SetRepeating(RepeatSettings.CreateNestedRepeatItem, RepeatSettings.CommitRepeatItem)
					.AddAttribute("Name", RepeatSettings.GetNestedName, RepeatSettings.SetNestedName)
					.AddChildElement("NumFoo", RepeatSettings.GetNumFooValueNested, RepeatSettings.SetNumFooValueNested);

			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><NumFoo>1</NumFoo><StringBar>foo</StringBar><Nesteds><Nested Name='test'><NumFoo>11</NumFoo></Nested></Nesteds></refSettings>";

			using (StringReader stringReader = new StringReader(sXml))
			{
				using (ReadFile<RepeatSettings> file = ReadFile<RepeatSettings>.CreateSettingsFile(stringReader))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual("foo", settings.StringBar);
			Assert.AreEqual(1, settings.MapNested.Count);
			Assert.AreEqual(11, settings.MapNested["test"].NestedNumFoo);
		}

		[Test]
		public static void TestRepeatingClass_TwoItems()
		{
			RepeatSettings settings = new RepeatSettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<RepeatSettings> description =
				XmlDescriptionBuilder<RepeatSettings>
					.Build(ns, "refSettings")
					.AddChildElement("NumFoo", RepeatSettings.GetNumFooValue, RepeatSettings.SetNumFooValue)
					.AddElement("StringBar", RepeatSettings.GetStringBarValue, RepeatSettings.SetStringBarValue)
					.AddElement("Nesteds")
					.AddChildElement("Nested")
					.SetRepeating(RepeatSettings.CreateNestedRepeatItem, RepeatSettings.CommitRepeatItem)
					.AddAttribute("Name", RepeatSettings.GetNestedName, RepeatSettings.SetNestedName)
					.AddChildElement("NumFoo", RepeatSettings.GetNumFooValueNested, RepeatSettings.SetNumFooValueNested);

			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><NumFoo>1</NumFoo><StringBar>foo</StringBar><Nesteds><Nested Name='test'><NumFoo>11</NumFoo></Nested><Nested Name='test2'><NumFoo>22</NumFoo></Nested></Nesteds></refSettings>";

			using (StringReader stringReader = new StringReader(sXml))
			{
				using (ReadFile<RepeatSettings> file = ReadFile<RepeatSettings>.CreateSettingsFile(stringReader))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual("foo", settings.StringBar);
			Assert.AreEqual(2, settings.MapNested.Count);
			Assert.AreEqual(11, settings.MapNested["test"].NestedNumFoo);
			Assert.AreEqual(22, settings.MapNested["test2"].NestedNumFoo);
		}

		[Test]
		public static void TestRepeatingClass_SimpleCollection()
		{
			RepeatSettings settings = new RepeatSettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<RepeatSettings> description =
				XmlDescriptionBuilder<RepeatSettings>
					.Build(ns, "refSettings")
					.AddChildElement("NumFoo", RepeatSettings.GetNumFooValue, RepeatSettings.SetNumFooValue)
					.AddElement("StringBar", RepeatSettings.GetStringBarValue, RepeatSettings.SetStringBarValue)
					.AddElement("StringBars")
					.AddChildElement("StringBar", RepeatSettings.GetCollectionItem, RepeatSettings.SetCollectionItem)
					.SetRepeating(RepeatSettings.CreateCollectionRepeatItem, RepeatSettings.CommitCollectionRepeatItem);

			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><NumFoo>1</NumFoo><StringBar>foo</StringBar><StringBars><StringBar>One</StringBar><StringBar>Two</StringBar></StringBars></refSettings>";

			using (StringReader stringReader = new StringReader(sXml))
			{
				using (ReadFile<RepeatSettings> file = ReadFile<RepeatSettings>.CreateSettingsFile(stringReader))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual("foo", settings.StringBar);
			Assert.AreEqual(2, settings.StringsBar.Count);
			Assert.AreEqual("One", settings.StringsBar[0]);
			Assert.AreEqual("Two", settings.StringsBar[1]);
		}

		static XmlDescription<RepeatSettings> CreateRepeatingNestedClassDescriptor(string ns)
		{
			return 
				XmlDescriptionBuilder<RepeatSettings>
					.Build(ns, "refSettings")
					.AddChildElement("NumFoo", RepeatSettings.GetNumFooValue, RepeatSettings.SetNumFooValue)
					.AddElement("StringBar", RepeatSettings.GetStringBarValue, RepeatSettings.SetStringBarValue)
					.AddElement("StringBars")
					.AddChildElement("StringBar", RepeatSettings.GetCollectionItem, RepeatSettings.SetCollectionItem)
					.SetRepeating(RepeatSettings.CreateCollectionRepeatItem, RepeatSettings.CommitCollectionRepeatItem)
					.Pop()
					.AddElement("Nesteds")
					.AddChildElement("Nested")
					.SetRepeating(RepeatSettings.CreateNestedRepeatItem, RepeatSettings.CommitRepeatItem)
					.AddAttribute("Name", RepeatSettings.GetNestedName, RepeatSettings.SetNestedName)
					.AddChildElement("NumFoo", RepeatSettings.GetNumFooValueNested, RepeatSettings.SetNumFooValueNested)
					.AddElement("StringBars")
					.AddChildElement("StringBar", RepeatSettings.GetCollectionItem, RepeatSettings.SetCollectionItem)
					.SetRepeating(RepeatSettings.CreateCollectionRepeatItem, RepeatSettings.CommitNestedCollectionRepeatItem);

		}
		
		[Test]
		public static void TestRepeatingNestedClass_WithSimpleCollection()
		{
			RepeatSettings settings = new RepeatSettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<RepeatSettings> description = CreateRepeatingNestedClassDescriptor(ns);

			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\">"
				+ "<NumFoo>1</NumFoo>"
				+ "<StringBar>foo</StringBar>"
				+ "<StringBars>"
					+ "<StringBar>One</StringBar>"
					+ "<StringBar>Two</StringBar>"
				+ "</StringBars>"
				+ "<Nesteds>"
					+ "<Nested Name='test'>"
						+ "<NumFoo>11</NumFoo>"
						+ "<StringBars>"
							+ "<StringBar>One</StringBar>"
							+ "<StringBar>Two</StringBar>"
						+ "</StringBars>"
					+ "</Nested>"
					+ "<Nested Name='test2'>"
						+ "<NumFoo>22</NumFoo>"
						+ "<StringBars>"
							+ "<StringBar>2One</StringBar>"
							+ "<StringBar>2Two</StringBar>"
						+ "</StringBars>"
					+ "</Nested>"
				+ "</Nesteds>"
				+ "</refSettings>";

			using (StringReader stringReader = new StringReader(sXml))
			{
				using (ReadFile<RepeatSettings> file = ReadFile<RepeatSettings>.CreateSettingsFile(stringReader))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual("foo", settings.StringBar);
			Assert.AreEqual(2, settings.StringsBar.Count);
			Assert.AreEqual("One", settings.StringsBar[0]);
			Assert.AreEqual("Two", settings.StringsBar[1]);
			Assert.AreEqual(2, settings.MapNested.Count);
			Assert.AreEqual(11, settings.MapNested["test"].NestedNumFoo);
			Assert.AreEqual(2, settings.MapNested["test"].NestedStrings.Count);
			Assert.AreEqual("One", settings.MapNested["test"].NestedStrings[0]);
			Assert.AreEqual("Two", settings.MapNested["test"].NestedStrings[1]);
			Assert.AreEqual(22, settings.MapNested["test2"].NestedNumFoo);
			Assert.AreEqual(2, settings.MapNested["test2"].NestedStrings.Count);
			Assert.AreEqual("2One", settings.MapNested["test2"].NestedStrings[0]);
			Assert.AreEqual("2Two", settings.MapNested["test2"].NestedStrings[1]);
		}

		[Test]
		public static void TestRepeatingNestedClass_WithSimpleCollection_SecondItemEmpty()
		{
			RepeatSettings settings = new RepeatSettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<RepeatSettings> description = CreateRepeatingNestedClassDescriptor(ns);

			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\">"
				+ "<NumFoo>1</NumFoo>"
				+ "<StringBar>foo</StringBar>"
				+ "<StringBars>"
					+ "<StringBar>One</StringBar>"
					+ "<StringBar>Two</StringBar>"
				+ "</StringBars>"
				+ "<Nesteds>"
					+ "<Nested Name='test'>"
						+ "<NumFoo>11</NumFoo>"
						+ "<StringBars>"
							+ "<StringBar>One</StringBar>"
							+ "<StringBar>Two</StringBar>"
						+ "</StringBars>"
					+ "</Nested>"
					+ "<Nested Name='test2'>"
						+ "<NumFoo>22</NumFoo>"
					+ "</Nested>"
				+ "</Nesteds>"
				+ "</refSettings>";

			using (StringReader stringReader = new StringReader(sXml))
			{
				using (ReadFile<RepeatSettings> file = ReadFile<RepeatSettings>.CreateSettingsFile(stringReader))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual("foo", settings.StringBar);
			Assert.AreEqual(2, settings.StringsBar.Count);
			Assert.AreEqual("One", settings.StringsBar[0]);
			Assert.AreEqual("Two", settings.StringsBar[1]);
			Assert.AreEqual(2, settings.MapNested.Count);
			Assert.AreEqual(11, settings.MapNested["test"].NestedNumFoo);
			Assert.AreEqual(2, settings.MapNested["test"].NestedStrings.Count);
			Assert.AreEqual("One", settings.MapNested["test"].NestedStrings[0]);
			Assert.AreEqual("Two", settings.MapNested["test"].NestedStrings[1]);
			Assert.AreEqual(22, settings.MapNested["test2"].NestedNumFoo);
			Assert.AreEqual(null, settings.MapNested["test2"].NestedStrings);
		}
	}
}
