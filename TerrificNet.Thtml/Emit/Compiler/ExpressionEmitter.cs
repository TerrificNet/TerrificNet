using System;
using System.IO;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class ExpressionEmitter : IEmitter<Action<TextWriter>>
	{
		public IEmitterRunnable<Action<TextWriter>> Emit(Document input, IDataScopeContract dataScopeContract, IHelperBinder helperBinder)
		{
			var visitor = new EmitExpressionVisitor(dataScopeContract, helperBinder ?? new NullHelperBinder());
			visitor.Visit(input);
			var action = visitor.DocumentFunc;

			return new IlEmitterRunnable(action);
		}

		private class IlEmitterRunnable : IEmitterRunnable<Action<TextWriter>>
		{
			private readonly Action<TextWriter, IDataContext> _action;

			public IlEmitterRunnable(Action<TextWriter, IDataContext> action)
			{
				_action = action;
			}

			public Action<TextWriter> Execute(IDataContext context, IRenderingContext renderingContext)
			{
				return writer => _action(writer, context);
			}
		}
	}
}
