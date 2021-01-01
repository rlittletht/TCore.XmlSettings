using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using TCore.XmlSettings;

namespace RefApp
{
	class Program
	{
		static void Main(string[] args)
		{
		}

		class MySettings
		{
			public int NumFoo { get; set; }
			public string StringBar { get; set; }
			public List<string> StringsBar { get; set; }

			public static string GetNumFooValue(MySettings settings) => settings.NumFoo.ToString();
			public static string GetStringBarValue(MySettings settings) => settings.StringBar;
		}
		
		[Test]
		public static void TestSimpleSettingsFileCreate()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			settings.NumFoo = 1;
			settings.StringBar = "bar";
			
			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddChildElement("numFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); })
					.AddElement("StringBar", MySettings.GetStringBarValue, (_settings, _value) => { _settings.StringBar = _value; });

			StringBuilder sb = new StringBuilder();
			
			using (StringWriter stringWriter = new StringWriter(sb))
			{
				using (WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(stringWriter))
				{
					file.SerializeSettings(description, settings);
					stringWriter.Flush();
				}
			}

			Assert.AreEqual($"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><numFoo>1</numFoo><StringBar>bar</StringBar></refSettings>", sb.ToString());
		}

		[Test]
		public static void TestSimpleSettingsFileCreate_NestedElements()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			settings.NumFoo = 1;
			settings.StringBar = "bar";

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddChildElement("numFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); })
					.AddElement("Parent", null, null)
					.AddChildElement("StringBar", MySettings.GetStringBarValue, (_settings, _value) => { _settings.StringBar = _value; });

			StringBuilder sb = new StringBuilder();

			using (StringWriter stringWriter = new StringWriter(sb))
			{
				using (WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(stringWriter))
				{
					file.SerializeSettings(description, settings);
					stringWriter.Flush();
				}
			}

			Assert.AreEqual($"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><numFoo>1</numFoo><Parent><StringBar>bar</StringBar></Parent></refSettings>", sb.ToString());
		}

		// we have a parent element and a child element, but the child has no value, so we never write out the subtree
		[Test]
		public static void TestSimpleSettingsFileCreate_NestedElements_NoLatentExpression()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			settings.NumFoo = 1;
			settings.StringBar = null;

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddChildElement("numFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); })
					.AddElement("Parent", null, null)
					.AddChildElement("StringBar", MySettings.GetStringBarValue, (_settings, _value) => { _settings.StringBar = _value; });

			StringBuilder sb = new StringBuilder();

			using (StringWriter stringWriter = new StringWriter(sb))
			{
				using (WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(stringWriter))
				{
					file.SerializeSettings(description, settings);
					stringWriter.Flush();
				}
			}

			Assert.AreEqual($"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><numFoo>1</numFoo></refSettings>", sb.ToString());
		}

		[Test]
		public static void TestSimpleSettingsFileCreate_SimpleAttribute()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			settings.NumFoo = 1;
			settings.StringBar = null;

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddChildElement("numFoo", null, null)
					.AddAttribute("attrNumFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); });

			StringBuilder sb = new StringBuilder();

			using (StringWriter stringWriter = new StringWriter(sb))
			{
				using (WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(stringWriter))
				{
					file.SerializeSettings(description, settings);
					stringWriter.Flush();
				}
			}

			Assert.AreEqual($"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><numFoo attrNumFoo=\"1\" /></refSettings>", sb.ToString());
		}

		[Test]
		public static void TestSimpleSettingsFileCreate_OptionalAttributeOmitted()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			settings.NumFoo = 1;
			settings.StringBar = null;

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddChildElement("numFoo", null, null)
					.AddAttribute("attrNumFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); })
					.AddAttribute("attrStringBar", MySettings.GetStringBarValue, (_settings, _value) => { _settings.StringBar = _value; });

			StringBuilder sb = new StringBuilder();

			using (StringWriter stringWriter = new StringWriter(sb))
			{
				using (WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(stringWriter))
				{
					file.SerializeSettings(description, settings);
					stringWriter.Flush();
				}
			}

			Assert.AreEqual($"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><numFoo attrNumFoo=\"1\" /></refSettings>", sb.ToString());
		}
		
		[Test]
		public static void TestSimpleSettingsFileRead_SingleContentElement()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddChildElement("numFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); });

			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><numFoo>1</numFoo></refSettings>";
			
			using (StringReader stringWriter = new StringReader(sXml))
			{
				using (ReadFile<MySettings> file = ReadFile<MySettings>.CreateSettingsFile(stringWriter))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
		}

		[Test]
		public static void TestSimpleSettingsFileRead_Attributes()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddChildElement("numFoo", null, null)
					.AddAttribute("attrNumFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); })
					.AddAttribute("attrStringBar", MySettings.GetStringBarValue, (_settings, _value) => { _settings.StringBar = _value; });


			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><numFoo attrNumFoo='1' attrStringBar='foo'/></refSettings>";

			using (StringReader stringWriter = new StringReader(sXml))
			{
				using (ReadFile<MySettings> file = ReadFile<MySettings>.CreateSettingsFile(stringWriter))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual("foo", settings.StringBar);
		}

		[Test]
		public static void TestSimpleSettingsFileRead_RootElementWithAttributes()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddAttribute("attrNumFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); })
					.AddAttribute("attrStringBar", MySettings.GetStringBarValue, (_settings, _value) => { _settings.StringBar = _value; });


			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\" attrNumFoo='1' attrStringBar='foo'/>";

			using (StringReader stringWriter = new StringReader(sXml))
			{
				using (ReadFile<MySettings> file = ReadFile<MySettings>.CreateSettingsFile(stringWriter))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual("foo", settings.StringBar);
		}

		// This test emulates sniffing a file -- we only define enough of the file to get what we need, then complete gracefully
		[Test]
		public static void TestSimpleSettingsFileRead_NonEmptyRootWithAttribute_IncompleteDefinitionByDesign()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddAttribute("attrNumFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); })
					.TerminateAfterReadingAttributes();

			StringBuilder sb = new StringBuilder();

			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\" attrNumFoo='1' ><StringBar>foo</StringBar><Child><GrandChild/></Child></refSettings>";

			using (StringReader stringWriter = new StringReader(sXml))
			{
				using (ReadFile<MySettings> file = ReadFile<MySettings>.CreateSettingsFile(stringWriter))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual(null, settings.StringBar);
		}

		[Test]
		public static void TestSimpleSettingsFileRead_NonEmptyRootWithAttribute_IncompleteDefinitionByDesign_CancelBeforeValue()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddAttribute("attrNumFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); })
					.TerminateAfterReadingAttributes()
					.AddChildElement("StringBar", null, (_settings, _value) => { _settings.StringBar = _value; });

			StringBuilder sb = new StringBuilder();

			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\" attrNumFoo='1' ><StringBar>foo</StringBar><Child><GrandChild/></Child></refSettings>";

			using (StringReader stringWriter = new StringReader(sXml))
			{
				using (ReadFile<MySettings> file = ReadFile<MySettings>.CreateSettingsFile(stringWriter))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual(null, settings.StringBar);
		}
		
		[Test]
		public static void TestSimpleSettingsFileRead_NonEmptyRootWithAttribute_IncompleteDefinitionByDesign_CancelAfterElement()
		{
			MySettings settings = new MySettings();
			string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

			XmlDescription<MySettings> description =
				XmlDescriptionBuilder<MySettings>
					.Build(ns, "refSettings")
					.AddAttribute("attrNumFoo", MySettings.GetNumFooValue, (_settings, _value) => { _settings.NumFoo = Int32.Parse(_value); })
					.AddChildElement("StringBar", null, (_settings, _value) => { _settings.StringBar = _value; })
					.TerminateAfterReadingElement();

			StringBuilder sb = new StringBuilder();

			string sXml =
				$"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\" attrNumFoo='1' ><StringBar>foo</StringBar><Child><GrandChild/></Child></refSettings>";

			using (StringReader stringWriter = new StringReader(sXml))
			{
				using (ReadFile<MySettings> file = ReadFile<MySettings>.CreateSettingsFile(stringWriter))
					file.DeSerialize(description, settings);
			}

			Assert.AreEqual(1, settings.NumFoo);
			Assert.AreEqual("foo", settings.StringBar);
		}
	}
}
