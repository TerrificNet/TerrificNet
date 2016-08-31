using System;
using System.IO;
using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class StreamEmitter : IEmitter<IStreamRenderer>
	{
		private readonly ParameterExpression _writerParameter;

		public StreamEmitter()
		{
			_writerParameter = Expression.Parameter(typeof(TextWriter));
			OutputExpressionEmitter = new StreamOutputExpressionEmitter(_writerParameter);
		}

		public IOutputExpressionEmitter OutputExpressionEmitter { get; }

		public IStreamRenderer WrapResult(CompilerResult result)
		{
			var action = Expression.Lambda<Action<TextWriter, object>>(result.BodyExpression, _writerParameter, result.InputExpression).Compile();
			return new StreamRenderer(action);
		}

		private class StreamRenderer : IStreamRenderer
		{
			private readonly Action<TextWriter, object> _action;

			public StreamRenderer(Action<TextWriter, object> action)
			{
				_action = action;
			}

			public void Execute(TextWriter writer, object data, IRenderingContext renderingContext)
			{
				_action(writer, data);
			}
		}
	}
}