namespace TerrificNet.Thtml.VDom
{
	public interface IVDomBuilder
	{
		void ElementOpen(string tagName);
		void ElementClose();
		void PropertyStart(string propertyName);
		void PropertyEnd();
		void Value(object value);
		void ElementOpenStart(string tagName);
		void ElementOpenEnd();
	}
}