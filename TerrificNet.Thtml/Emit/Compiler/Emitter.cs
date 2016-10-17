using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class Emitter<TRenderer> : IEmitter<IViewTemplate<TRenderer>>
	{
		private readonly ParameterExpression _rendererExpression;

		public Emitter(Func<ParameterExpression, IOutputExpressionBuilder> builderFactory)
		{
			_rendererExpression = Expression.Parameter(typeof(TRenderer));
			ExpressionBuilder = builderFactory(_rendererExpression);
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
			return Expression.Lambda<Action<TRenderer, object>>(result.BodyExpression, _rendererExpression, result.InputExpression);
		}

		public Type ExpressionType => typeof(void);

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
		}
	}
}
