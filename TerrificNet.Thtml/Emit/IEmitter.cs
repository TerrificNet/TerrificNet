using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    public interface IEmitter<out T>
    {
        T Execute(IDataContext context, IRenderingContext renderingContext);
    }
}

namespace TerrificNet.Thtml.Emit
{
	public interface IEmitter
	{
		IEmitter<VTree> Emit(Document input, IDataBinder dataBinder, IHelperBinder helperBinder);
	}
}