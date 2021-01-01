
namespace TCore.XmlSettings
{
	// Full describes an xml settings file, to be used to Serialize or DeSerialize settings
	public class XmlDescription<T>
	{
		public string Namespace { get; set; }
		public Element<T> RootElement { get; set; }
		public bool DiscardAttributesWithNoSetter { get; set; }
		public bool DiscardUnknownAttributes { get; set; }

		public XmlDescription(string ns, string rootElement)
		{
			Namespace = ns;
			RootElement = new Element<T>(this, ns, rootElement, null, null, null);
		}
	}
}
