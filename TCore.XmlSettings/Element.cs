using System;
using System.Collections.Generic;

namespace TCore.XmlSettings
{
	public class Element<T>
	{
		public XmlDescription<T> ParentDescription { get; set; }
		
		// for repeating items, we need to get a key telling us which
		// repeating item we are working with
		public delegate void SetValueDelegate(T t, string value, RepeatContext<T>.RepeatItemContext repeatItemContext);
		public delegate string GetValueDelegate(T t, RepeatContext<T>.RepeatItemContext repeatItemContext);
		
		public string Namespace { get; set; }
		public string ElementName { get; set; }
		
		public List<Attribute<T>> Attributes { get; set; }
		public List<Element<T>> Children { get; set; }
		public bool Required { get; set; }
		public bool TerminateAfterReadingElement{ get; set; }
		public bool TerminateAfterReadingAttributes { get; set; }
		public bool IsRepeating => m_createRepeatItemContextDelegate != null;
		
		private readonly GetValueDelegate m_getValueDelegate;
		private readonly SetValueDelegate m_setValueDelegate;
		private RepeatContext<T>.CommitRepeatItem m_commitRepeatItemDelegate;
		private RepeatContext<T>.CreateRepeatItemContext m_createRepeatItemContextDelegate;
		private RepeatContext<T>.AreRemainingItems m_areRemainingItemsDelegate;
		
		/*----------------------------------------------------------------------------
			%%Function:SetRepeating
			%%Qualified:TCore.XmlSettings.Element<T>.SetRepeating

		----------------------------------------------------------------------------*/
		public void SetRepeating(
			RepeatContext<T>.CreateRepeatItemContext createRepeatItemContextDelegate, 
			RepeatContext<T>.AreRemainingItems areRemainingItemsDelegate, 
			RepeatContext<T>.CommitRepeatItem commitRepeatItemDelegate)
		{
			m_createRepeatItemContextDelegate = createRepeatItemContextDelegate;
			m_commitRepeatItemDelegate = commitRepeatItemDelegate;
			m_areRemainingItemsDelegate = areRemainingItemsDelegate;
		}

		/*----------------------------------------------------------------------------
			%%Function:CreateRepeatItem
			%%Qualified:TCore.XmlSettings.Element<T>.CreateRepeatItem

		----------------------------------------------------------------------------*/
		public RepeatContext<T>.RepeatItemContext CreateRepeatItem(T t, Element<T> element, RepeatContext<T>.RepeatItemContext parent)
		{
			return m_createRepeatItemContextDelegate(t, element, parent);
		}

		/*----------------------------------------------------------------------------
			%%Function:CommitRepeatItem
			%%Qualified:TCore.XmlSettings.Element<T>.CommitRepeatItem

		----------------------------------------------------------------------------*/
		public void CommitRepeatItem(T t, RepeatContext<T>.RepeatItemContext repeatItemContext)
		{
			m_commitRepeatItemDelegate(t, repeatItemContext);
		}

		/*----------------------------------------------------------------------------
			%%Function:AreRemainingRepeatingItems
			%%Qualified:TCore.XmlSettings.Element<T>.AreRemainingRepeatingItems

		----------------------------------------------------------------------------*/
		public bool AreRemainingRepeatingItems(T t, RepeatContext<T>.RepeatItemContext repeatItemContext)
		{
			return m_areRemainingItemsDelegate(t, repeatItemContext);
		}
		
		/*----------------------------------------------------------------------------
			%%Function:SetValue
			%%Qualified:TCore.XmlSettings.Element<T>.SetValue

			Set the value using the delegate we were given
		----------------------------------------------------------------------------*/
		public void SetValue(T t, string value, RepeatContext<T>.RepeatItemContext repeatItemContext = null)
		{
			if (m_setValueDelegate == null)
				throw new Exception("trying to set value on element with no delegate");
			
			m_setValueDelegate(t, value, repeatItemContext);
		}

		/*----------------------------------------------------------------------------
			%%Function:FGetValue
			%%Qualified:TCore.XmlSettings.Element<T>.FGetValue

			Get the value using the delegate we were given. If we have no delegate,
			or if the delegate returns null, then return false.
		----------------------------------------------------------------------------*/
		public bool FGetValue(T t, out string value, RepeatContext<T>.RepeatItemContext repeatItemContext = null)
		{
			value = null;

			if (m_getValueDelegate != null)
			{
				return (value = m_getValueDelegate(t, repeatItemContext)) != null;
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
			RepeatContext<T>.CreateRepeatItemContext createRepeatItemContextDelegate = null)
		{
			ParentDescription = parent;
			Namespace = ns;
			ElementName = elementName;
			Attributes = attributes;
			m_getValueDelegate = getValueDelegate;
			m_setValueDelegate = setValueDelegate;
			m_createRepeatItemContextDelegate = createRepeatItemContextDelegate;
			
			if (Attributes == null)
				Attributes = new List<Attribute<T>>();
			
			Children = new List<Element<T>>();
		}
	}
}
