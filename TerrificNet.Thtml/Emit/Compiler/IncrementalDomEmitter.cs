using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class IncrementalDomEmitter : IEmitter<IIncrementalDomTemplate>
	{
		private readonly ParameterExpression _parameterExpression;

		public IncrementalDomEmitter()
		{
			_parameterExpression = Expression.Parameter(typeof(IIncrementalDomRenderer));
			ExpressionBuilder = new IncrementalDomRendererExpressionBuilder(_parameterExpression);
		}

		public IOutputExpressionBuilder ExpressionBuilder { get; }

		public IIncrementalDomTemplate WrapResult(CompilerResult result)
		{
			var action = CreateLambda(result).Compile();
			return new IncrementDomTemplate(action);
		}

		public LambdaExpression CreateExpression(CompilerResult result)
		{
			return CreateLambda(result);
		}

		private Expression<Action<IIncrementalDomRenderer, object>> CreateLambda(CompilerResult result)
		{
			return Expression.Lambda<Action<IIncrementalDomRenderer, object>>(result.BodyExpression, _parameterExpression, result.InputExpression);
		}

		public Type ExpressionType => typeof(void);

		private class IncrementDomTemplate : IIncrementalDomTemplate
		{
			private readonly Action<IIncrementalDomRenderer, object> _action;

			public IncrementDomTemplate(Action<IIncrementalDomRenderer, object> action)
			{
				_action = action;
			}

			public void Execute(IIncrementalDomRenderer renderer, object data, IRenderingContext renderingContext)
			{
				_action(renderer, data);
			}
		}
	}
}