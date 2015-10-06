namespace TerrificNet.Thtml.VDom
{
    public interface IVTreeVisitor
    {
        void Visit(VTree vTree);
        void Visit(VText vText);
        void BeginVisit(VNode vNode);
        void EndVisit(VNode vNode);
        void BeginVisit(VElement vElement);
        void EndVisit(VElement vElement);
    }
}