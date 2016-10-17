using System;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IEmitter
	{
		VTreeOutputExpressionEmitter OutputExpressionEmitter { get; }

		LambdaExpression CreateExpression(CompilerResult result);

		Type ExpressionType { get; }
	}

	public interface IEmitter<out TResult> : IEmitter
	{
		TResult WrapResult(CompilerResult result);
	}
}