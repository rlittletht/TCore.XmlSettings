using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using XMLIO;

namespace TCore.XmlSettings
{
	public class ReadFile<T> : IDisposable
	{
		private T m_t;
		private XmlDescription<T> m_xmlDescription;
		readonly XmlReader m_reader;

		public class ReadFileContext
		{
			public T Data { get; set; }
			public Element<T> CurrentElement { get; set; }
			
			public ReadFileContext(T t, Element<T> currentElement)
			{
				Data = t;
				CurrentElement = currentElement;
			}
		}
		
		/*----------------------------------------------------------------------------
			%%Function:WriteFile
			%%Qualified:TCore.XmlSettings.WriteFile<T>.WriteFile

			Create a WriteFile on this TextWriter
		----------------------------------------------------------------------------*/
		public ReadFile(TextReader textReader)
		{
			m_reader = new XmlTextReader(textReader);
		}

		/*----------------------------------------------------------------------------
			%%Function:CreateSettingsFile
			%%Qualified:TCore.XmlSettings.WriteFile<T>.CreateSettingsFile

			Create a WriteFile for the given path
		----------------------------------------------------------------------------*/
		public static ReadFile<T> CreateSettingsFile(string path)
		{
			TextReader textReader = new StreamReader(path);
			ReadFile<T> file = new ReadFile<T>(textReader);

			return file;
		}

		/*----------------------------------------------------------------------------
			%%Function:CreateSettingsFile
			%%Qualified:TCore.XmlSettings.ReadFile<T>.CreateSettingsFile
		----------------------------------------------------------------------------*/
		public static ReadFile<T> CreateSettingsFile(TextReader textReader)
		{
			ReadFile<T> file = new ReadFile<T>(textReader);

			return file;
		}


		/*----------------------------------------------------------------------------
			%%Function:DeSerialize
			%%Qualified:TCore.XmlSettings.ReadFile<T>.DeSerialize
		----------------------------------------------------------------------------*/
		public void DeSerialize(XmlDescription<T> description, T t)
		{
			m_t = t;
			m_xmlDescription = description;
			
			// start reading, from the mandatory root element
			Element<T> element = m_xmlDescription.RootElement;

			// if the config is empty, return null
			if (!XmlIO.Read(m_reader))
				return;

			XmlIO.SkipNonContent(m_reader);

			XmlIO.ContentCollector contentCollector = new XmlIO.ContentCollector();

			try
			{
				if (!XmlIO.FReadElement<ReadFileContext>(
					m_reader,
					new ReadFileContext(m_t, element),
					element.ElementName,
					ProcessAttributes,
					ParseElement,
					contentCollector))
					throw new Exception("deserialize failed");
			}
			catch (XMLIO.Exceptions.UserCancelledException)
			{
				// this is totally fine. it means we were asked to cancel early
			}

			if (contentCollector.ToString().Length > 0)
				element.SetValue(m_t, contentCollector.ToString());
		}
		
		/*----------------------------------------------------------------------------
			%%Function:ProcessAttributes
			%%Qualified:TCore.XmlSettings.ReadFile<T>.ProcessAttributes
		----------------------------------------------------------------------------*/
		static bool ProcessAttributes(string sAttribute, string value, ReadFileContext context)
		{
			foreach (Attribute<T> attribute in context.CurrentElement.Attributes)
			{
				if (attribute.AttributeName == sAttribute)
				{
					attribute.SetValue(context.Data, value, context.CurrentElement.ParentDescription.DiscardAttributesWithNoSetter);
					return true;
				}
			}

			if (context.CurrentElement.ParentDescription.DiscardUnknownAttributes)
				return true;
			
			return false;
		}

		/*----------------------------------------------------------------------------
			%%Function:ParseElement
			%%Qualified:TCore.XmlSettings.ReadFile<T>.ParseElement
		----------------------------------------------------------------------------*/
		static bool ParseElement(XmlReader reader, string sElement, ReadFileContext context)
		{
			if (context.CurrentElement.TerminateAfterReadingAttributes)
			{
				// before we try to parse anything, check to see if we're supposed
				// to cancel after attributes (we've read attributes by now because
				// we're being asked to parse an element...
				throw new XMLIO.Exceptions.UserCancelledException();
			}
			
			foreach (Element<T> element in context.CurrentElement.Children)
			{
				if (element.ElementName == sElement)
				{
					XmlIO.ContentCollector contentCollector = new XmlIO.ContentCollector();

					bool f = XmlIO.FReadElement(
						reader,
						new ReadFileContext(context.Data, element),
						element.ElementName,
						ProcessAttributes,
						ParseElement,
						contentCollector);
					
					if (contentCollector.ToString().Length > 0)
						element.SetValue(context.Data, contentCollector.ToString());

					if (element.TerminateAfterReadingElement)
						throw new XMLIO.Exceptions.UserCancelledException();
					
					return f;
				}
			}

			if (context.CurrentElement.TerminateAfterReadingElement)
				throw new XMLIO.Exceptions.UserCancelledException();

			return false;
		}

		public void Dispose()
		{
			((IDisposable)m_reader).Dispose();
		}
	}
}
