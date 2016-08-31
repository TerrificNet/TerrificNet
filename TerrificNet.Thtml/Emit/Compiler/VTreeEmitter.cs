using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class VTreeEmitter : IEmitter<IVTreeRenderer>
	{
		public VTreeEmitter()
		{
			OutputExpressionEmitter = new VTreeOutputExpressionEmitter();
		}

		public IOutputExpressionEmitter OutputExpressionEmitter { get; }

		public IVTreeRenderer WrapResult(CompilerResult result)
		{
			var action = Expression.Lambda<Func<object, VTree>>(result.BodyExpression, result.InputExpression).Compile();
			return new VTreeRenderer(action);
		}

		private class VTreeRenderer : IVTreeRenderer
		{
			private readonly Func<object, VTree> _action;

			public VTreeRenderer(Func<object, VTree> action)
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