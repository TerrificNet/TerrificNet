using System;
using System.IO;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class StreamEmitter : IEmitter<Action<TextWriter>>
	{
		public IEmitterRunnable<Action<TextWriter>> Emit(Document input, IDataScopeContract dataScopeContract, IHelperBinder helperBinder)
		{
			var visitor = new EmitStreamVisitor(dataScopeContract, helperBinder ?? new NullHelperBinder());
			visitor.Visit(input);
			var action = visitor.DocumentFunc;

			return new IlEmitterRunnable(action);
		}

		private class IlEmitterRunnable : IEmitterRunnable<Action<TextWriter>>
		{
			private readonly IEmitterRunnable<StreamWriterHandler> _action;

			public IlEmitterRunnable(IEmitterRunnable<StreamWriterHandler> action)
			{
				_action = action;
			}

			public Action<TextWriter> Execute(object context, IRenderingContext renderingContext)
			{
				return writer => _action.Execute(context, renderingContext)(writer);
			}
		}
	}
}