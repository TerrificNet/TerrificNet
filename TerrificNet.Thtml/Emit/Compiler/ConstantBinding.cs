using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class ConstantBinding : IBindingWithExpression
	{
		public ConstantBinding(object value) : this(Expression.Constant(value))
		{
		}

		private ConstantBinding(Expression expression)
		{
			Expression = expression;
		}

		public BindingPathTemplate Path { get; }

		public bool IsSupported(RenderingScope server)
		{
			return true;
		}

		public Expression Expression { get; }
	}
}