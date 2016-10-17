using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class Emitter<TRenderer> : IEmitter<TRenderer>
	{
		public ParameterExpression RendererExpression { get; }

		public IViewTemplate CreateTemplate(CompilerResult result)
		{
			return WrapResult(result);
		}

		public Emitter(Func<ParameterExpression, IOutputExpressionBuilder> builderFactory)
		{
			RendererExpression = Expression.Parameter(typeof(TRenderer));
			ExpressionBuilder = builderFactory(RendererExpression);
		}

		public IViewTemplate<TRenderer> WrapResult(CompilerResult result)
		{
			var action = CreateLambda(result).Compile();
			return new Template((r, d, c) => action(r, d));
		}

		public IOutputExpressionBuilder ExpressionBuilder { get; }

		public LambdaExpression CreateExpression(CompilerResult result)
		{
			return CreateLambda(result);
		}

		private Expression<Action<TRenderer, object>> CreateLambda(CompilerResult result)
		{
			return Expression.Lambda<Action<TRenderer, object>>(result.BodyExpression, RendererExpression, result.InputExpression);
		}

		private class Template : IViewTemplate<TRenderer>
		{
			private readonly Action<TRenderer, object, IRenderingContext> _action;

			public Template(Action<TRenderer, object, IRenderingContext> action)
			{
				_action = action;
			}

			public void Execute(TRenderer renderer, object data, IRenderingContext renderingContext)
			{
				_action(renderer, data, renderingContext);
			}

			public void Execute(object renderer, object data, IRenderingContext renderingContext)
			{
				_action((TRenderer) renderer, data, renderingContext);
			}
		}
	}
}
