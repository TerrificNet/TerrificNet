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
			var visitor = new EmitExpressionVisitor(dataScopeContract, helperBinder ?? new NullHelperBinder<Expression, ExpressionHelperConfig>());
			visitor.Visit(input);
			var action = visitor.DocumentFunc;

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
