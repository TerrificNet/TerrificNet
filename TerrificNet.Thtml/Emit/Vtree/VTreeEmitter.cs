using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Vtree
{
	public class VTreeEmitter : IEmitter<VTree, Expression, ExpressionHelperConfig>
	{
		public IEmitterRunnable<VTree> Emit(Document input, IDataScopeContract dataScopeContract, IHelperBinder helperBinder)
		{
			var dataContextParameter = Expression.Variable(dataScopeContract.ResultType, "item");
			var handler = new VTreeOutputExpressionEmitter();

			var visitor = new EmitExpressionVisitor(dataScopeContract, helperBinder, dataContextParameter, handler);
			var expression = visitor.Visit(input);

			var inputExpression = Expression.Parameter(typeof(object), "input");
			var convertExpression = Expression.Assign(dataContextParameter, Expression.ConvertChecked(inputExpression, dataScopeContract.ResultType));
			var bodyExpression = Expression.Block(new[] { dataContextParameter }, convertExpression, expression);
			var action = Expression.Lambda<Func<object, VTree>>(bodyExpression, inputExpression).Compile();

			return new IlEmitterRunnable(action);
		}

		private class IlEmitterRunnable : IEmitterRunnable<VTree>
		{
			private readonly Func<object, VTree> _action;

			public IlEmitterRunnable(Func<object, VTree> action)
			{
				_action = action;
			}

			public VTree Execute(object data, IRenderingContext renderingContext)
			{
				return _action(data);
			}
		}
	}
}