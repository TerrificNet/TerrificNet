namespace TerrificNet.Thtml.VDom
{
	public interface IVTreeVisitor
	{
		void Visit(VTree vTree);
		void Visit(VText vText);
		void Visit(VNode vNode);
		void Visit(VElement vElement);
		void Visit(BooleanVPropertyValue booleanValue);
		void Visit(NumberVPropertyValue numberValue);
		void Visit(StringVPropertyValue stringValue);
	}
}