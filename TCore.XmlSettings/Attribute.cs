using System;
using System.Collections.Generic;
using System.Text;

namespace TCore.XmlSettings
{
	public class Attribute<T>
	{
		public delegate void SetValueDelegate(T t, string value, RepeatContext<T>.RepeatItem repeatItem);
		public delegate string GetValueDelegate(T t, RepeatContext<T>.RepeatItem repeatItem);

		public string Namespace { get; set; }
		public string AttributeName { get; set; }

		public bool Required { get; set; }

		private readonly GetValueDelegate m_delegateGetValue;
		private readonly SetValueDelegate m_delegateSetValue;
		
		/*----------------------------------------------------------------------------
			%%Function:FGetValue
			%%Qualified:TCore.XmlSettings.Attribute<T>.FGetValue

			Get the value using the delegate we were given. If we have no delegate,
			or if the delegate returns null, then return false.
		----------------------------------------------------------------------------*/
		public bool FGetValue(T t, out string value, RepeatContext<T>.RepeatItem repeatItem = null)
		{
			value = null;

			if (m_delegateGetValue != null)
			{
				return (value = m_delegateGetValue(t, repeatItem)) != null;
			}
			
			return false;
		}

		/*----------------------------------------------------------------------------
			%%Function:SetValue
			%%Qualified:TCore.XmlSettings.Attribute<T>.SetValue

			Set the value using the delegate we were given
		----------------------------------------------------------------------------*/
		public void SetValue(T t, string value, bool discardIfNoSetter, RepeatContext<T>.RepeatItem repeatItem = null)
		{
			if (m_delegateSetValue == null)
			{
				if (!discardIfNoSetter)
					throw new Exception("trying to set value on attribute with no delegate");
				
				return;
			}

			m_delegateSetValue(t, value, repeatItem);
		}
		
		/*----------------------------------------------------------------------------
			%%Function:Attribute
			%%Qualified:TCore.XmlSettings.Attribute<T>.Attribute
		----------------------------------------------------------------------------*/
		public Attribute(string ns, string attributeName, GetValueDelegate getValueDelegate, SetValueDelegate setValueDelegate)
		{
			Namespace = ns;
			AttributeName = attributeName;
			
			m_delegateGetValue = getValueDelegate;
			m_delegateSetValue = setValueDelegate;
		}
	}
}
