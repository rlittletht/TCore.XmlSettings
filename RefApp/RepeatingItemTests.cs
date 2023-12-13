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
        public RepeatingItemTests()
        {
        }

        // Generic RepeatSettings class used by all tests. This often has more than we need for any given test
        // (Also note the generic applicability of the Set/Get value functions. Interchangeable for attributes
        // and elements)
        class RepeatSettings
        {
            public int NumFoo { get; set; }
            public string StringBar { get; set; }
            public List<string> StringBars { get; set; }
            public int CurrentStringBar { get; set; }

            public class Nested
            {
                public string Name { get; set; }
                public string NestedStringVal { get; set; }
                public int NestedNumFoo { get; set; }
                public List<string> NestedStrings { get; set; }
                public int CurrentNestedString { get; set; }
            }

            public Dictionary<string, Nested> MapNested { get; set; }
            public IEnumerator<string> NestedEnumerator { get; set; }

            public static void SetNumFooValueSmart(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext)
            {
                if (repeatItemContext != null)
                {
                    Nested nested = (Nested)repeatItemContext.RepeatKey;
                    nested.NestedNumFoo = Int32.Parse(value);
                }
                else
                {
                    settings.NumFoo = Int32.Parse(value);
                }
            }

            public static string GetNumFooValueSmart(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext)
            {
                if (repeatItemContext != null)
                {
                    Nested nested = (Nested)repeatItemContext.RepeatKey;
                    return nested.NestedNumFoo.ToString();
                }

                return settings.NumFoo.ToString();
            }

            // Parser Accessors for Top Level items
            public static void SetNumFooValue(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                settings.NumFoo = Int32.Parse(value);

            public static string GetNumFooValue(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                settings.NumFoo.ToString();

            public static void SetStringBarValue(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                settings.StringBar = value;

            public static string GetStringBarValue(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                settings.StringBar;

            public static void SetNumFooValueNested(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                ((Nested)repeatItemContext.RepeatKey).NestedNumFoo = Int32.Parse(value);

            public static string GetNumFooValueNested(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                ((Nested)repeatItemContext.RepeatKey).NestedNumFoo.ToString();

            public static void SetNestedStringValueNested(
                RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                ((Nested)repeatItemContext.RepeatKey).NestedStringVal = value;

            public static string GetNestedStringValueNested(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                ((Nested)repeatItemContext.RepeatKey).NestedStringVal;

            public static void SetNestedName(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                ((Nested)repeatItemContext.RepeatKey).Name = value;

            public static string GetNestedName(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                ((Nested)repeatItemContext.RepeatKey).Name;

            // Top level simple collection

#region Top Level Collection

            public static void SetCollectionItem(RepeatSettings settings, string value, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                ((string[])repeatItemContext.RepeatKey)[0] = value;

            public static string GetCollectionItem(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext repeatItemContext) =>
                ((string[])repeatItemContext.RepeatKey)[0];

            public static RepeatContext<RepeatSettings>.RepeatItemContext CreateCollectionRepeatItem(
                RepeatSettings settings,
                Element<RepeatSettings> element,
                RepeatContext<RepeatSettings>.RepeatItemContext parent)
            {
                if (settings.StringBars != null && settings.CurrentStringBar != -1 && settings.CurrentStringBar < settings.StringBars.Count)
                {
                    return new RepeatContext<RepeatSettings>.RepeatItemContext(
                        element,
                        parent,
                        new string[1] { settings.StringBars[settings.CurrentStringBar++] });
                }

                return new RepeatContext<RepeatSettings>.RepeatItemContext(element, parent, new string[1]);
            }

            public static bool AreRemainingItemsInCollection(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext itemContext)
            {
                if (settings.StringBars == null || settings.CurrentStringBar == -1)
                    return false;

                if (settings.CurrentStringBar >= settings.StringBars.Count)
                {
                    settings.CurrentStringBar = -1;
                    return false;
                }

                return true;
            }

            public static void CommitCollectionRepeatItem(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext itemContext)
            {
                if (settings.StringBars == null)
                    settings.StringBars = new List<string>();

                settings.StringBars.Add(((string[])itemContext.RepeatKey)[0]);
                settings.CurrentStringBar = settings.StringBars.Count; // by definition we're done with the previous one.
            }

#endregion

#region Nested Collection

            public static void CommitNestedCollectionRepeatItem(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext itemContext)
            {
                Nested nested = (Nested)itemContext.Parent.RepeatKey;

                if (nested.NestedStrings == null)
                    nested.NestedStrings = new List<string>();

                nested.NestedStrings.Add(((string[])itemContext.RepeatKey)[0]);
                nested.CurrentNestedString = nested.NestedStrings.Count;
            }

            public static bool AreRemainingItemsInNestedCollection(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext itemContext)
            {
                Nested nested = (Nested)itemContext.RepeatKey;

                if (nested.NestedStrings == null || nested.CurrentNestedString == -1)
                    return false;

                if (nested.CurrentNestedString >= nested.NestedStrings.Count)
                {
                    nested.CurrentNestedString = -1;
                    return false;
                }

                return true;
            }

            public static RepeatContext<RepeatSettings>.RepeatItemContext CreateNestedCollectionRepeatItem(
                RepeatSettings settings,
                Element<RepeatSettings> element,
                RepeatContext<RepeatSettings>.RepeatItemContext parent)
            {
                Nested nested = (Nested)parent?.RepeatKey;

                if (nested?.NestedStrings != null && nested.CurrentNestedString != -1 && nested.CurrentNestedString < nested.NestedStrings.Count)
                    return new RepeatContext<RepeatSettings>.RepeatItemContext(
                        element,
                        parent,
                        new string[1] { nested.NestedStrings[nested.CurrentNestedString++] });

                return new RepeatContext<RepeatSettings>.RepeatItemContext(element, parent, new string[1]);
            }

#endregion

#region Repeating Top Level Nested Item

            // now to build our

            public static RepeatContext<RepeatSettings>.RepeatItemContext CreateNestedRepeatItem(
                RepeatSettings settings,
                Element<RepeatSettings> element,
                RepeatContext<RepeatSettings>.RepeatItemContext parent)
            {
                if (settings.MapNested != null && settings.NestedEnumerator != null)
                {
                    // also propagate the name (we will only have access to the item as the
                    // repeatItemContext.ContextKey, so we won't have access to the key in the
                    // dictionary that brought us here. propagate that here since our item
                    // doesn't have the key in it. If your item already has the key, then you
                    // can just use that (though asserting they are the same is not a bad thing)
                    settings.MapNested[settings.NestedEnumerator.Current].Name = settings.NestedEnumerator.Current;
                    return new RepeatContext<RepeatSettings>.RepeatItemContext(
                        element,
                        parent,
                        settings.MapNested[settings.NestedEnumerator.Current]);
                }

                return new RepeatContext<RepeatSettings>.RepeatItemContext(element, parent, new Nested());
            }

            public static bool AreRemainingNestedItems(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext itemContext)
            {
                if (settings.MapNested == null)
                    return false;

                if (settings.NestedEnumerator == null)
                    settings.NestedEnumerator = settings.MapNested.Keys.GetEnumerator();

                return settings.NestedEnumerator.MoveNext();
            }

            public static void CommitRepeatItem(RepeatSettings settings, RepeatContext<RepeatSettings>.RepeatItemContext itemContext)
            {
                Nested nested = (Nested)itemContext.RepeatKey;
                if (settings.MapNested == null)
                    settings.MapNested = new Dictionary<string, Nested>();

                settings.MapNested.Add(nested.Name, nested);
            }

#endregion
        }


#region Test Simple Top Level Repeating Class

        private static XmlDescription<RepeatSettings> CreateRepeatingClassXmlDescriptor(string ns)
        {
            XmlDescription<RepeatSettings> description =
                XmlDescriptionBuilder<RepeatSettings>
                   .Build(ns, "refSettings")
                   .AddChildElement("NumFoo", RepeatSettings.GetNumFooValue, RepeatSettings.SetNumFooValue)
                   .AddElement("StringBar", RepeatSettings.GetStringBarValue, RepeatSettings.SetStringBarValue)
                   .AddElement("Nesteds")
                   .AddChildElement("Nested")
                   .SetRepeating(
                        RepeatSettings.CreateNestedRepeatItem,
                        RepeatSettings.AreRemainingNestedItems,
                        RepeatSettings.CommitRepeatItem)
                   .AddAttribute("Name", RepeatSettings.GetNestedName, RepeatSettings.SetNestedName)
                   .AddChildElement("NumFoo", RepeatSettings.GetNumFooValueNested, RepeatSettings.SetNumFooValueNested);
            return description;
        }

        private static XmlDescription<RepeatSettings> CreateRepeatingClassStringValueXmlDescriptor(string ns)
        {
            XmlDescription<RepeatSettings> description =
                XmlDescriptionBuilder<RepeatSettings>
                   .Build(ns, "refSettings")
                   .AddChildElement("NumFoo", RepeatSettings.GetNumFooValue, RepeatSettings.SetNumFooValue)
                   .AddElement("StringBar", RepeatSettings.GetStringBarValue, RepeatSettings.SetStringBarValue)
                   .AddElement("Nesteds")
                   .AddChildElement("Nested")
                   .SetRepeating(
                        RepeatSettings.CreateNestedRepeatItem,
                        RepeatSettings.AreRemainingNestedItems,
                        RepeatSettings.CommitRepeatItem)
                   .AddAttribute("Name", RepeatSettings.GetNestedName, RepeatSettings.SetNestedName)
                   .AddChildElement("StringVal", RepeatSettings.GetNestedStringValueNested, RepeatSettings.SetNestedStringValueNested);
            return description;
        }

        [Test]
        public static void TestWriteRepeatingClass_TwoItemsWithNullValues()
        {
            RepeatSettings settings = new RepeatSettings();

            settings.NumFoo = 1;
            settings.StringBar = "foo";
            settings.MapNested = new Dictionary<string, RepeatSettings.Nested>()
                                 {
                                     { "test", new RepeatSettings.Nested() },
                                     { "test2", new RepeatSettings.Nested() },
                                 };

            // assert preconditions
            Assert.AreEqual(1, settings.NumFoo);
            Assert.AreEqual("foo", settings.StringBar);
            Assert.AreEqual(2, settings.MapNested.Count);
            Assert.AreEqual(null, settings.MapNested["test"].NestedStringVal);
            Assert.AreEqual(null, settings.MapNested["test2"].NestedStringVal);

            string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

            XmlDescription<RepeatSettings> description = CreateRepeatingClassStringValueXmlDescriptor(ns);

            StringBuilder sb = new StringBuilder();

            using (StringWriter stringWriter = new StringWriter(sb))
            {
                using (WriteFile<RepeatSettings> file = WriteFile<RepeatSettings>.CreateSettingsFile(stringWriter))
                    file.SerializeSettings(description, settings);
            }

            string sXml =
                $"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><NumFoo>1</NumFoo><StringBar>foo</StringBar><Nesteds><Nested Name=\"test\" /><Nested Name=\"test2\" /></Nesteds></refSettings>";

            Assert.AreEqual(sXml, sb.ToString());
        }

        [Test]
        public static void TestWriteRepeatingClass_TwoItems()
        {
            RepeatSettings settings = new RepeatSettings();

            settings.NumFoo = 1;
            settings.StringBar = "foo";
            settings.MapNested = new Dictionary<string, RepeatSettings.Nested>()
                                 {
                                     { "test", new RepeatSettings.Nested() { NestedNumFoo = 11 } },
                                     { "test2", new RepeatSettings.Nested() { NestedNumFoo = 22 } },
                                 };

            // assert preconditions
            Assert.AreEqual(1, settings.NumFoo);
            Assert.AreEqual("foo", settings.StringBar);
            Assert.AreEqual(2, settings.MapNested.Count);
            Assert.AreEqual(11, settings.MapNested["test"].NestedNumFoo);
            Assert.AreEqual(22, settings.MapNested["test2"].NestedNumFoo);

            string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

            XmlDescription<RepeatSettings> description = CreateRepeatingClassXmlDescriptor(ns);

            StringBuilder sb = new StringBuilder();

            using (StringWriter stringWriter = new StringWriter(sb))
            {
                using (WriteFile<RepeatSettings> file = WriteFile<RepeatSettings>.CreateSettingsFile(stringWriter))
                    file.SerializeSettings(description, settings);
            }

            string sXml =
                $"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><NumFoo>1</NumFoo><StringBar>foo</StringBar><Nesteds><Nested Name=\"test\"><NumFoo>11</NumFoo></Nested><Nested Name=\"test2\"><NumFoo>22</NumFoo></Nested></Nesteds></refSettings>";

            Assert.AreEqual(sXml, sb.ToString());
        }

        [Test]
        public static void TestRepeatingClass()
        {
            RepeatSettings settings = new RepeatSettings();
            string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

            XmlDescription<RepeatSettings> description = CreateRepeatingClassXmlDescriptor(ns);

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

            XmlDescription<RepeatSettings> description = CreateRepeatingClassXmlDescriptor(ns);

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

#endregion

#region Test Nested Repeating Item With Collection

        static XmlDescription<RepeatSettings> CreateRepeatingNestedClassDescriptor(string ns)
        {
            return
                XmlDescriptionBuilder<RepeatSettings>
                   .Build(ns, "refSettings")
                   .AddChildElement("NumFoo", RepeatSettings.GetNumFooValue, RepeatSettings.SetNumFooValue)
                   .AddElement("StringBar", RepeatSettings.GetStringBarValue, RepeatSettings.SetStringBarValue)
                   .AddElement("StringBars")
                   .AddChildElement("StringBar", RepeatSettings.GetCollectionItem, RepeatSettings.SetCollectionItem)
                   .SetRepeating(
                        RepeatSettings.CreateCollectionRepeatItem,
                        RepeatSettings.AreRemainingItemsInCollection,
                        RepeatSettings.CommitCollectionRepeatItem)
                   .Pop()
                   .AddElement("Nesteds")
                   .AddChildElement("Nested")
                   .SetRepeating(
                        RepeatSettings.CreateNestedRepeatItem,
                        RepeatSettings.AreRemainingNestedItems,
                        RepeatSettings.CommitRepeatItem)
                   .AddAttribute("Name", RepeatSettings.GetNestedName, RepeatSettings.SetNestedName)
                   .AddChildElement("NumFoo", RepeatSettings.GetNumFooValueNested, RepeatSettings.SetNumFooValueNested)
                   .AddElement("StringBars")
                   .AddChildElement("StringBar", RepeatSettings.GetCollectionItem, RepeatSettings.SetCollectionItem)
                   .SetRepeating(
                        RepeatSettings.CreateNestedCollectionRepeatItem,
                        RepeatSettings.AreRemainingItemsInNestedCollection,
                        RepeatSettings.CommitNestedCollectionRepeatItem);
        }

        [Test]
        public static void TestRepeatingClass_SimpleCollection()
        {
            RepeatSettings settings = new RepeatSettings();
            string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

            XmlDescription<RepeatSettings> description = CreateRepeatingNestedClassDescriptor(ns);

            string sXml =
                $"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><NumFoo>1</NumFoo><StringBar>foo</StringBar><StringBars><StringBar>One</StringBar><StringBar>Two</StringBar></StringBars></refSettings>";

            using (StringReader stringReader = new StringReader(sXml))
            {
                using (ReadFile<RepeatSettings> file = ReadFile<RepeatSettings>.CreateSettingsFile(stringReader))
                    file.DeSerialize(description, settings);
            }

            Assert.AreEqual(1, settings.NumFoo);
            Assert.AreEqual("foo", settings.StringBar);
            Assert.AreEqual(2, settings.StringBars.Count);
            Assert.AreEqual("One", settings.StringBars[0]);
            Assert.AreEqual("Two", settings.StringBars[1]);
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
            Assert.AreEqual(2, settings.StringBars.Count);
            Assert.AreEqual("One", settings.StringBars[0]);
            Assert.AreEqual("Two", settings.StringBars[1]);
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
            Assert.AreEqual(2, settings.StringBars.Count);
            Assert.AreEqual("One", settings.StringBars[0]);
            Assert.AreEqual("Two", settings.StringBars[1]);
            Assert.AreEqual(2, settings.MapNested.Count);
            Assert.AreEqual(11, settings.MapNested["test"].NestedNumFoo);
            Assert.AreEqual(2, settings.MapNested["test"].NestedStrings.Count);
            Assert.AreEqual("One", settings.MapNested["test"].NestedStrings[0]);
            Assert.AreEqual("Two", settings.MapNested["test"].NestedStrings[1]);
            Assert.AreEqual(22, settings.MapNested["test2"].NestedNumFoo);
            Assert.AreEqual(null, settings.MapNested["test2"].NestedStrings);
        }

        [Test]
        public static void TestWriteRepeatingNestedClass_WithSimpleCollection()
        {
            RepeatSettings settings = new RepeatSettings();
            string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

            settings.StringBar = "foo";
            settings.NumFoo = 1;
            settings.StringBars = new List<string>(new[] { "One", "Two" });
            settings.MapNested = new Dictionary<string, RepeatSettings.Nested>();
            settings.MapNested.Add("test", new RepeatSettings.Nested());
            settings.MapNested["test"].NestedNumFoo = 11;
            settings.MapNested["test"].NestedStrings = new List<string>(new[] { "One", "Two" });
            settings.MapNested.Add("test2", new RepeatSettings.Nested());
            settings.MapNested["test2"].NestedNumFoo = 22;
            settings.MapNested["test2"].NestedStrings = new List<string>(new[] { "2One", "2Two" });

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
                + "<Nested Name=\"test\">"
                + "<NumFoo>11</NumFoo>"
                + "<StringBars>"
                + "<StringBar>One</StringBar>"
                + "<StringBar>Two</StringBar>"
                + "</StringBars>"
                + "</Nested>"
                + "<Nested Name=\"test2\">"
                + "<NumFoo>22</NumFoo>"
                + "<StringBars>"
                + "<StringBar>2One</StringBar>"
                + "<StringBar>2Two</StringBar>"
                + "</StringBars>"
                + "</Nested>"
                + "</Nesteds>"
                + "</refSettings>";

            StringBuilder sb = new StringBuilder();

            using (StringWriter stringWriter = new StringWriter(sb))
            {
                using (WriteFile<RepeatSettings> file = WriteFile<RepeatSettings>.CreateSettingsFile(stringWriter))
                    file.SerializeSettings(description, settings);
            }

            Assert.AreEqual(sXml, sb.ToString());
        }


        [Test]
        public static void TestWriteRepeatingNestedClass_EmptyNestedClass()
        {
            RepeatSettings settings = new RepeatSettings();
            string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

            settings.StringBar = "foo";
            settings.NumFoo = 1;
            settings.StringBars = new List<string>(new[] { "One", "Two" });

            XmlDescription<RepeatSettings> description = CreateRepeatingNestedClassDescriptor(ns);

            string sXml =
                $"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\">"
                + "<NumFoo>1</NumFoo>"
                + "<StringBar>foo</StringBar>"
                + "<StringBars>"
                + "<StringBar>One</StringBar>"
                + "<StringBar>Two</StringBar>"
                + "</StringBars>"
                + "</refSettings>";

            StringBuilder sb = new StringBuilder();

            using (StringWriter stringWriter = new StringWriter(sb))
            {
                using (WriteFile<RepeatSettings> file = WriteFile<RepeatSettings>.CreateSettingsFile(stringWriter))
                    file.SerializeSettings(description, settings);
            }

            Assert.AreEqual(sXml, sb.ToString());
        }

#endregion

#region Test Simple Top Level Collection

        private static XmlDescription<RepeatSettings> CreateSimpleCollectionDescriptor(string ns)
        {
            XmlDescription<RepeatSettings> description =
                XmlDescriptionBuilder<RepeatSettings>
                   .Build(ns, "refSettings")
                   .AddChildElement("NumFoo", RepeatSettings.GetNumFooValue, RepeatSettings.SetNumFooValue)
                   .AddElement("StringBar", RepeatSettings.GetStringBarValue, RepeatSettings.SetStringBarValue)
                   .AddElement("StringBars")
                   .AddChildElement("StringBar", RepeatSettings.GetCollectionItem, RepeatSettings.SetCollectionItem)
                   .SetRepeating(
                        RepeatSettings.CreateCollectionRepeatItem,
                        RepeatSettings.AreRemainingItemsInCollection,
                        RepeatSettings.CommitCollectionRepeatItem);
            return description;
        }

        // write tests
        [Test]
        public static void TestWriteRepeatingClass_SimpleCollection()
        {
            RepeatSettings settings = new RepeatSettings();

            settings.NumFoo = 10;
            settings.StringBar = "foo";
            settings.StringBars = new List<string>(new string[] { "One", "Two" });

            string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

            XmlDescription<RepeatSettings> description = CreateSimpleCollectionDescriptor(ns);

            StringBuilder sb = new StringBuilder();

            using (StringWriter stringWriter = new StringWriter(sb))
            {
                using (WriteFile<RepeatSettings> file = WriteFile<RepeatSettings>.CreateSettingsFile(stringWriter))
                    file.SerializeSettings(description, settings);
            }

            string sXml =
                $"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><NumFoo>10</NumFoo><StringBar>foo</StringBar><StringBars><StringBar>One</StringBar><StringBar>Two</StringBar></StringBars></refSettings>";

            Assert.AreEqual(sXml, sb.ToString());
        }

        [Test]
        public static void TestWriteRepeatingClass_SimpleCollectionEmpty()
        {
            RepeatSettings settings = new RepeatSettings();

            settings.NumFoo = 10;
            settings.StringBar = "foo";

            string ns = "http://schemas.thetasoft.com/TCore.XmlSettings/reftest/2020";

            XmlDescription<RepeatSettings> description = CreateSimpleCollectionDescriptor(ns);

            StringBuilder sb = new StringBuilder();

            using (StringWriter stringWriter = new StringWriter(sb))
            {
                using (WriteFile<RepeatSettings> file = WriteFile<RepeatSettings>.CreateSettingsFile(stringWriter))
                    file.SerializeSettings(description, settings);
            }

            string sXml =
                $"<?xml version=\"1.0\" encoding=\"utf-16\"?><refSettings xmlns=\"{ns}\"><NumFoo>10</NumFoo><StringBar>foo</StringBar></refSettings>";

            Assert.AreEqual(sXml, sb.ToString());
        }

#endregion
    }
}
