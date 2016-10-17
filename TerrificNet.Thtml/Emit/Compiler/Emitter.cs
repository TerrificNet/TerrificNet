using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class Emitter<TRenderer> : IEmitter
	{
		public Emitter(Func<ParameterExpression, IOutputExpressionBuilder> builderFactory)
		{
			ExpressionBuilder = builderFactory(Expression.Parameter(typeof(TRenderer)));
		}

		public IOutputExpressionBuilder ExpressionBuilder { get; }
	}

	public class Template<TRenderer> : IViewTemplate<TRenderer>
	{
		private readonly Action<TRenderer, object> _action;

		public Template(Action<TRenderer, object> action)
		{
			_action = action;
		}

		public void Execute(TRenderer renderer, object data, IRenderingContext renderingContext)
		{
			_action(renderer, data);
		}

		public void Execute(object renderer, object data, IRenderingContext renderingContext)
		{
			_action((TRenderer) renderer, data);
		}
	}
}
