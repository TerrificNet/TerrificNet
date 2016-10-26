using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit
{
	public interface IBindingWithExpression : IBinding
	{
		Expression Expression { get; }
	}
}