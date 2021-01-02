using System;
using System.Collections.Generic;

namespace TCore.XmlSettings
{
	public class Element<T>
	{
		public XmlDescription<T> ParentDescription { get; set; }
		
		// for repeating items, we need to get a key telling us which
		// repeating item we are working with
		public delegate void SetValueDelegate(T t, string value, RepeatContext<T>.RepeatItem repeatItem);
		public delegate string GetValueDelegate(T t, RepeatContext<T>.RepeatItem repeatItem);
		
		public string Namespace { get; set; }
		public string ElementName { get; set; }
		
		public List<Attribute<T>> Attributes { get; set; }
		public List<Element<T>> Children { get; set; }
		public bool Required { get; set; }
		public bool TerminateAfterReadingElement{ get; set; }
		public bool TerminateAfterReadingAttributes { get; set; }
		public bool IsRepeating => m_createRepeatItemDelegate != null;
		
		private readonly GetValueDelegate m_getValueDelegate;
		private readonly SetValueDelegate m_setValueDelegate;
		private RepeatContext<T>.CommitRepeatItem m_commitRepeatItemDelegate;
		private RepeatContext<T>.CreateRepeatItem m_createRepeatItemDelegate;

		/*----------------------------------------------------------------------------
			%%Function:SetRepeating
			%%Qualified:TCore.XmlSettings.Element<T>.SetRepeating

		----------------------------------------------------------------------------*/
		public void SetRepeating(RepeatContext<T>.CreateRepeatItem createRepeatItemDelegate, RepeatContext<T>.CommitRepeatItem commitRepeatItemDelegate)
		{
			m_createRepeatItemDelegate = createRepeatItemDelegate;
			m_commitRepeatItemDelegate = commitRepeatItemDelegate;
		}

		/*----------------------------------------------------------------------------
			%%Function:CreateRepeatItem
			%%Qualified:TCore.XmlSettings.Element<T>.CreateRepeatItem

		----------------------------------------------------------------------------*/
		public RepeatContext<T>.RepeatItem CreateRepeatItem(Element<T> element, RepeatContext<T>.RepeatItem parent)
		{
			return m_createRepeatItemDelegate(element, parent);
		}

		public void CommitRepeatItem(T t, RepeatContext<T>.RepeatItem repeatItem)
		{
			m_commitRepeatItemDelegate(t, repeatItem);
		}
		
		/*----------------------------------------------------------------------------
			%%Function:SetValue
			%%Qualified:TCore.XmlSettings.Element<T>.SetValue

			Set the value using the delegate we were given
		----------------------------------------------------------------------------*/
		public void SetValue(T t, string value, RepeatContext<T>.RepeatItem repeatItem = null)
		{
			if (m_setValueDelegate == null)
				throw new Exception("trying to set value on element with no delegate");
			
			m_setValueDelegate(t, value, repeatItem);
		}

		/*----------------------------------------------------------------------------
			%%Function:FGetValue
			%%Qualified:TCore.XmlSettings.Element<T>.FGetValue

			Get the value using the delegate we were given. If we have no delegate,
			or if the delegate returns null, then return false.
		----------------------------------------------------------------------------*/
		public bool FGetValue(T t, out string value, RepeatContext<T>.RepeatItem repeatItem = null)
		{
			value = null;

			if (m_getValueDelegate != null)
			{
				return (value = m_getValueDelegate(t, repeatItem)) != null;
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
			List<Attribute<T>> attributes,
			RepeatContext<T>.CreateRepeatItem createRepeatItemDelegate = null)
		{
			ParentDescription = parent;
			Namespace = ns;
			ElementName = elementName;
			Attributes = attributes;
			m_getValueDelegate = getValueDelegate;
			m_setValueDelegate = setValueDelegate;
			m_createRepeatItemDelegate = createRepeatItemDelegate;
			
			if (Attributes == null)
				Attributes = new List<Attribute<T>>();
			
			Children = new List<Element<T>>();
		}
	}
}
