using System;
using System.IO;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class ExpressionEmitter : IEmitter<Action<TextWriter>, Expression, ExpressionHelperConfig>
	{
		public IEmitterRunnable<Action<TextWriter>> Emit(Document input, IDataScopeContract dataScopeContract, IHelperBinder<Expression, ExpressionHelperConfig> helperBinder)
		{
			var dataContextParameter = Expression.Variable(dataScopeContract.ResultType, "item");
			var writerParameter = Expression.Parameter(typeof (TextWriter));
			var handler = new StreamOutputExpressionEmitter(writerParameter);

			var visitor = new EmitExpressionVisitor(dataScopeContract, helperBinder ?? new NullHelperBinder<Expression, ExpressionHelperConfig>(), dataContextParameter, handler);
			var expression = visitor.Visit(input);

			var inputExpression = Expression.Parameter(typeof(object), "input");
			var convertExpression = Expression.Assign(dataContextParameter, Expression.ConvertChecked(inputExpression, dataScopeContract.ResultType));
			var bodyExpression = Expression.Block(new[] { dataContextParameter }, convertExpression, expression);
			var action = Expression.Lambda<Action<TextWriter, object>>(bodyExpression, writerParameter, inputExpression).Compile();

			return new IlEmitterRunnable(action);
		}

		private class IlEmitterRunnable : IEmitterRunnable<Action<TextWriter>>
		{
			private readonly Action<TextWriter, object> _action;

			public IlEmitterRunnable(Action<TextWriter, object> action)
			{
				_action = action;
			}

			public Action<TextWriter> Execute(object data, IRenderingContext renderingContext)
			{
				return writer => _action(writer, data);
			}
		}
	}
}
