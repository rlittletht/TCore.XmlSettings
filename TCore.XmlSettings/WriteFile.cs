﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace TCore.XmlSettings
{
    public class WriteFile<T> : IDisposable where T : class
    {
        private T? m_t;
        private XmlDescription<T> m_xmlDescription;
        readonly XmlWriter m_writer;

        /*----------------------------------------------------------------------------
            %%Function:WriteFile
            %%Qualified:TCore.XmlSettings.WriteFile<T>.WriteFile

            Create a WriteFile on this TextWriter
        ----------------------------------------------------------------------------*/
        public WriteFile(TextWriter textWriter)
        {
            m_writer = new XmlTextWriter(textWriter);
            m_xmlDescription = new XmlDescription<T>(string.Empty, string.Empty);
        }

        /*----------------------------------------------------------------------------
            %%Function:CreateSettingsFile
            %%Qualified:TCore.XmlSettings.WriteFile<T>.CreateSettingsFile

            Create a WriteFile for the given path
        ----------------------------------------------------------------------------*/
        public static WriteFile<T> CreateSettingsFile(XmlDescription<T> description, string path, T t)
        {
            TextWriter textWriter = new StreamWriter(path);
            WriteFile<T> file = new WriteFile<T>(textWriter);

            file.m_t = t;
            file.m_xmlDescription = description;

            return file;
        }

        /*----------------------------------------------------------------------------
            %%Function:CreateSettingsFile
            %%Qualified:TCore.XmlSettings.WriteFile<T>.CreateSettingsFile

            Create a WriteFile on the TextWriter
        ----------------------------------------------------------------------------*/
        public static WriteFile<T> CreateSettingsFile(TextWriter textWriter)
        {
            WriteFile<T> file = new WriteFile<T>(textWriter);

            return file;
        }

        /*----------------------------------------------------------------------------
            %%Function:SerializeSettings
            %%Qualified:TCore.XmlSettings.WriteFile<T>.SerializeSettings

            Serialize the object we were given that is described by the descriptor
            we were given.
        
            Perhaps take both of those here instead? No reason they should be part
            of construction...
        ----------------------------------------------------------------------------*/
        public bool SerializeSettings(XmlDescription<T> description, T t)
        {
            m_xmlDescription = description;
            m_t = t;

            m_writer.WriteStartDocument();
            Element<T> element = m_xmlDescription.RootElement;

            List<Element<T>> latentElements = new List<Element<T>>();
            if (!FWriteElement(element, latentElements, null))
                throw new Exception("root element not required??");

            m_writer.WriteEndDocument();
            return true;
        }

        /*----------------------------------------------------------------------------
            %%Function:WriteElementStart
            %%Qualified:TCore.XmlSettings.WriteFile<T>.WriteElementStart
        ----------------------------------------------------------------------------*/
        void WriteElementStart(Element<T> element)
        {
            m_writer.WriteStartElement(element.ElementName, element.Namespace);
        }

        /*----------------------------------------------------------------------------
            %%Function:WriteLatentElementStarts
            %%Qualified:TCore.XmlSettings.WriteFile<T>.WriteLatentElementStarts
        ----------------------------------------------------------------------------*/
        void WriteLatentElementStarts(List<Element<T>> latentElements)
        {
            while (latentElements.Count > 0)
            {
                Element<T> element = latentElements[0];
                latentElements.RemoveAt(0);

                WriteElementStart(element);
            }
        }

        /*----------------------------------------------------------------------------
            %%Function:FAddAttribute
            %%Qualified:TCore.XmlSettings.WriteFile<T>.FAddAttribute
        ----------------------------------------------------------------------------*/
        bool FAddAttribute(Attribute<T> attribute, List<Element<T>> latentElements, RepeatContext<T>.RepeatItemContext? repeatItemContext)
        {
            if (m_t == null)
                throw new InvalidOperationException("m_t is null in add attribute");

            bool fHasValue = attribute.FGetValue(m_t, out string? value, repeatItemContext);

            if (attribute.Required || fHasValue)
            {
                if (attribute.Required && fHasValue == false)
                    throw new Exception("missing required value for attribute");

                WriteLatentElementStarts(latentElements);
                m_writer.WriteAttributeString(attribute.AttributeName, attribute.Namespace, value);
                return true;
            }

            return false;
        }

        /*----------------------------------------------------------------------------
            %%Function:FAddElementAttributes
            %%Qualified:TCore.XmlSettings.WriteFile<T>.FAddElementAttributes
        ----------------------------------------------------------------------------*/
        bool FAddElementAttributes(Element<T> element, List<Element<T>> latentElements, RepeatContext<T>.RepeatItemContext? repeatItemContext)
        {
            if (element.Attributes == null)
                return false;

            bool fAddedAttribute = false;

            foreach (Attribute<T> attribute in element.Attributes)
            {
                fAddedAttribute |= FAddAttribute(attribute, latentElements, repeatItemContext);
            }

            return fAddedAttribute;
        }

        /*----------------------------------------------------------------------------
            %%Function:FWriteElement
            %%Qualified:TCore.XmlSettings.WriteFile<T>.FWriteElement

            return true if we wrote out our children (and hence we should close
            our element

            (since we don't want to open elements unless they are necessary, we will
            pass down a stack of latent	elements that will be opened on demand
        ----------------------------------------------------------------------------*/
        bool FWriteElement(Element<T> element, List<Element<T>> latentElements, RepeatContext<T>.RepeatItemContext? repeatItemContextParent)
        {
            if (m_t == null)
                throw new InvalidOperationException("m_t is null in WriteElement");

            // as we write things, keep track of whether or not we wrote
            // anything (which would mean that our element is no longer latent
            // and thus needs to be closed)
            bool fWroteChildren = false;
            bool fRepeat = false;

            do
            {
                RepeatContext<T>.RepeatItemContext? repeatItemContext = repeatItemContextParent;

                if (element.IsRepeating)
                {
                    if (m_t == null)
                        throw new InvalidOperationException("m_t is null in FWriteElement");

                    // first see if there are any remaining items to write for this
                    // repeating item.
                    if (!element.AreRemainingRepeatingItems(m_t, repeatItemContextParent))
                        return fWroteChildren; // we didn't write anything

                    repeatItemContext = element.CreateRepeatItem(m_t, element, repeatItemContextParent);
                    fRepeat = true; // we will repeat until we return above
                }

                bool fHasValue = element.FGetValue(m_t, out string? value, repeatItemContext);

                latentElements.Add(element); // this is latent until we need it

                // write any attributes
                fWroteChildren |= FAddElementAttributes(element, latentElements, repeatItemContext);

                if (element.Required && fHasValue == false)
                    throw new Exception("missing required value");

                if (element.Required || fHasValue)
                {
                    WriteLatentElementStarts(latentElements);
                    fWroteChildren = true;
                }

                // if we have a value, we can't have children too
                if (fHasValue)
                {
                    if (element.Children.Count > 0)
                        throw new Exception("data inconsistent -- mixed content not allowed");

                    m_writer.WriteString(value);
                    m_writer.WriteEndElement();

                    continue; // repeat if we are going to...
                }

                foreach (Element<T> child in element.Children)
                {
                    fWroteChildren |= FWriteElement(child, latentElements, repeatItemContext);
                }

                if (fWroteChildren)
                    m_writer.WriteEndElement();
                else
                    latentElements.RemoveAt(0);
            } while (fRepeat);

            return fWroteChildren;
        }

        public void Dispose()
        {
            ((IDisposable)m_writer).Dispose();
        }
    }
}
