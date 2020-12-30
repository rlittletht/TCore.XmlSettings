
namespace TCore.XmlSettings
{
	// Full describes an xml settings file, to be used to Serialize or DeSerialize settings
	public class XmlDescription<T>
	{
		public string Namespace { get; set; }
		public Element<T> RootElement { get; set; }
		
		public XmlDescription(string ns, string rootElement)
		{
			Namespace = ns;
			RootElement = new Element<T>(ns, rootElement, null, null, null);
		}
	}
}
