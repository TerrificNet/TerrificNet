﻿using System.Linq.Expressions;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class ThtmlDocumentCompiler
	{
		private readonly Document _input;
		private readonly IHelperBinder _helperBinder;

		public ThtmlDocumentCompiler(Document input, IHelperBinder helperBinder)
		{
			_input = input;
			_helperBinder = helperBinder ?? new NullHelperBinder();
		}

		public T Compile<T>(IDataBinder dataBinder, IEmitterFactory<T> emitterFactory)
		{
			var dataContextParameter = Expression.Variable(dataBinder.ResultType, "item");
			var dataScopeContract = new DataScope(new DataScopeContract(BindingPathTemplate.Global), dataBinder, dataContextParameter);

			return Compile(dataScopeContract, emitterFactory);
		}

		private T Compile<T>(IDataScopeContract dataScopeContract, IEmitterFactory<T> emitterFactory)
		{
			var emitter = emitterFactory.Create();
			var result = CreateExpression(emitter.OutputExpressionEmitter, dataScopeContract);

			return emitter.WrapResult(result);
		}

		private CompilerResult CreateExpression(IOutputExpressionEmitter handler, IDataScopeContract dataScopeContract)
		{
			var visitor = new EmitExpressionVisitor(dataScopeContract, _helperBinder, handler);
			var expression = visitor.Visit(_input);

			var inputExpression = Expression.Parameter(typeof(object), "input");
			var convertExpression = Expression.Assign(dataScopeContract.Expression, Expression.ConvertChecked(inputExpression, dataScopeContract.Expression.Type));
			var bodyExpression = Expression.Block(new[] {(ParameterExpression)dataScopeContract.Expression}, convertExpression, expression);
			return new CompilerResult(bodyExpression, inputExpression);
		}
	}
}