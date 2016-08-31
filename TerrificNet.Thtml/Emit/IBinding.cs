using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;

namespace TerrificNet.Thtml.Emit
{
	public interface IBinding
	{
		BindingPathTemplate Path { get; }

		Expression CreateExpression(Expression dataContext);
	}

	public interface IBinding<T> : IBinding
	{
	}
}