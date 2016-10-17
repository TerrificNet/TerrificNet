using System;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class EmitterFactory<T>
	{
		private readonly Func<Expression, IOutputExpressionBuilder> _createFunc;

		public EmitterFactory(Func<Expression, IOutputExpressionBuilder> createFunc)
		{
			_createFunc = createFunc;
		}

		public Emitter<T> Create()
		{
			return new Emitter<T>(p => _createFunc(p));
		}
	}
}