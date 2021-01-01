using System;
using System.Collections.Generic;

namespace TCore.XmlSettings
{
	public class Element<T>
	{
		public XmlDescription<T> ParentDescription { get; set; }
		
		public delegate void SetValueDelegate(T t, string value);
		public delegate string GetValueDelegate(T t);
		
		public string Namespace { get; set; }
		public string ElementName { get; set; }
		
		public List<Attribute<T>> Attributes { get; set; }
		public List<Element<T>> Children { get; set; }
		public bool Required { get; set; }
		public bool TerminateAfterReadingElement{ get; set; }
		public bool TerminateAfterReadingAttributes { get; set; }
		
		private readonly GetValueDelegate m_getValueDelegate;
		private readonly SetValueDelegate m_setValueDelegate;

		/*----------------------------------------------------------------------------
			%%Function:SetValue
			%%Qualified:TCore.XmlSettings.Element<T>.SetValue

			Set the value using the delegate we were given
		----------------------------------------------------------------------------*/
		public void SetValue(T t, string value)
		{
			if (m_setValueDelegate == null)
				throw new Exception("trying to set value on element with no delegate");
			
			m_setValueDelegate(t, value);
		}

		/*----------------------------------------------------------------------------
			%%Function:FGetValue
			%%Qualified:TCore.XmlSettings.Element<T>.FGetValue

			Get the value using the delegate we were given. If we have no delegate,
			or if the delegate returns null, then return false.
		----------------------------------------------------------------------------*/
		public bool FGetValue(T t, out string value)
		{
			value = null;

			if (m_getValueDelegate != null)
			{
				return (value = m_getValueDelegate(t)) != null;
			}
			
			return false;
		}
		
		/*----------------------------------------------------------------------------
			%%Function:Element
			%%Qualified:TCore.XmlSettings.Element<T>.Element
		----------------------------------------------------------------------------*/
		public Element(
			XmlDescription<T> parent,
			string ns, 
			string elementName, 
			GetValueDelegate getValueDelegate, 
			SetValueDelegate setValueDelegate, 
			List<Attribute<T>> attributes)
		{
			ParentDescription = parent;
			Namespace = ns;
			ElementName = elementName;
			Attributes = attributes;
			m_getValueDelegate = getValueDelegate;
			m_setValueDelegate = setValueDelegate;

			if (Attributes == null)
				Attributes = new List<Attribute<T>>();
			
			Children = new List<Element<T>>();
		}
	}
}
