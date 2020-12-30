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
				WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(description, stringWriter, settings);

				file.SerializeSettings();
				stringWriter.Flush();
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
				WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(description, stringWriter, settings);

				file.SerializeSettings();
				stringWriter.Flush();
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
				WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(description, stringWriter, settings);

				file.SerializeSettings();
				stringWriter.Flush();
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
				WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(description, stringWriter, settings);

				file.SerializeSettings();
				stringWriter.Flush();
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
				WriteFile<MySettings> file = WriteFile<MySettings>.CreateSettingsFile(description, stringWriter, settings);

				file.SerializeSettings();
				stringWriter.Flush();
			}

			Assert.AreEqual($"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><numFoo attrNumFoo=\"1\" /></refSettings>", sb.ToString());
		}
	}
}
