//using System;
//using System.IO;
//using TerrificNet.Thtml.Parsing;

//namespace TerrificNet.Thtml.Emit.Compiler
//{
//	public class StreamEmitter : IEmitter<Action<TextWriter>>
//	{
//		public IEmitterRunnable<Action<TextWriter>> Emit(Document input, IDataBinder dataBinder, IHelperBinder helperBinder)
//		{
//			var visitor = new IlExpressionEmitNodeVisitor(dataBinder, helperBinder ?? new NullHelperBinder());
//			visitor.Visit(input);
//			var action = visitor.Generate();

//			return new IlEmitterRunnable(action);
//		}

//		private class IlEmitterRunnable : IEmitterRunnable<Action<TextWriter>>
//		{
//			private readonly Action<TextWriter, IDataContext, IRenderingContext> _action;

//			public IlEmitterRunnable(Action<TextWriter, IDataContext, IRenderingContext> action)
//			{
//				_action = action;
//			}

//			public Action<TextWriter> Execute(IDataContext context, IRenderingContext renderingContext)
//			{
//				return writer => _action(writer, context, renderingContext);
//			}
//		}
//	}
//}
