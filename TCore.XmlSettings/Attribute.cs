using System;
using System.Collections.Generic;
using System.Text;

namespace TCore.XmlSettings
{
	public class Attribute<T>
	{
		public delegate void SetValueDelegate(T t, string value);
		public delegate string GetValueDelegate(T t);

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
		public bool FGetValue(T t, out string value)
		{
			value = null;

			if (m_delegateGetValue != null)
			{
				return (value = m_delegateGetValue(t)) != null;
			}
			
			return false;
		}

		/*----------------------------------------------------------------------------
			%%Function:SetValue
			%%Qualified:TCore.XmlSettings.Attribute<T>.SetValue

			Set the value using the delegate we were given
		----------------------------------------------------------------------------*/
		public void SetValue(T t, string value)
		{
			if (m_delegateSetValue == null)
				throw new Exception("trying to set value on attribute with no delegate");

			m_delegateSetValue(t, value);
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
