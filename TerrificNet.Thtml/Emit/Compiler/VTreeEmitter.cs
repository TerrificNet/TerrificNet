using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class VTreeEmitter : IEmitter<IVTreeRenderer>
	{
		private readonly ParameterExpression _builderParameter;

		public VTreeEmitter()
		{
			_builderParameter = Expression.Parameter(typeof(IVDomBuilder));
			ExpressionBuilder = new VDomOutputExpressionBuilder(_builderParameter);
		}

		public IOutputExpressionBuilder ExpressionBuilder { get; }

		public IVTreeRenderer WrapResult(CompilerResult result)
		{
			var action = CreateLambda(result).Compile();
			return new VTreeRenderer(action);
		}

		public LambdaExpression CreateExpression(CompilerResult result)
		{
			return Expression.Lambda(Expression.Invoke(CreateLambda(result), _builderParameter, result.InputExpression), result.InputExpression);
		}

		private Expression<Action<IVDomBuilder, object>> CreateLambda(CompilerResult result)
		{
			return Expression.Lambda<Action<IVDomBuilder, object>>(result.BodyExpression, _builderParameter, result.InputExpression);
		}

		public Type ExpressionType => typeof(VTree);

		private class VTreeRenderer : IVTreeRenderer
		{
			private readonly Action<IVDomBuilder, object> _action;

			public VTreeRenderer(Action<IVDomBuilder, object> action)
			{
				_action = action;
			}

			public void Execute(IVDomBuilder domBuilder, object data, IRenderingContext renderingContext)
			{
				_action(domBuilder, data);
			}
		}
	}
}
