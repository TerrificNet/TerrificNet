﻿using System;
using System.IO;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

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

		public IRunnable<Action<TextWriter>> CompileForTextWriter(IDataBinder dataBinder)
		{
			var writerParameter = Expression.Parameter(typeof(TextWriter));
			var handler = new StreamOutputExpressionEmitter(writerParameter);

			var result = CreateExpression(dataBinder, handler);
			var action = Expression.Lambda<Action<TextWriter, object>>(result.BodyExpression, writerParameter, result.InputExpression).Compile();
			return new TextWriterRunnable(action);
		}

		public IRunnable<VTree> CompileForVTree(IDataBinder dataBinder)
		{
			var handler = new VTreeOutputExpressionEmitter();

			var result = CreateExpression(dataBinder, handler);
			var action = Expression.Lambda<Func<object, VTree>>(result.BodyExpression, result.InputExpression).Compile();
			return new VTreeRunnable(action);
		}

		private ExpressionResult CreateExpression(IDataBinder dataBinder, IOutputExpressionEmitter handler)
		{
			var dataScopeContract = new DataScopeContractLegacyWrapper(new DataScopeContract(BindingPathTemplate.Global), dataBinder);
			var dataContextParameter = Expression.Variable(dataBinder.DataContextType, "item");

			var visitor = new EmitExpressionVisitor(dataScopeContract, _helperBinder, dataBinder, dataContextParameter, handler);
			var expression = visitor.Visit(_input);

			var inputExpression = Expression.Parameter(typeof (object), "input");
			var convertExpression = Expression.Assign(dataContextParameter,
				Expression.ConvertChecked(inputExpression, dataBinder.DataContextType));
			var bodyExpression = Expression.Block(new[] {dataContextParameter}, convertExpression, expression);
			return new ExpressionResult(bodyExpression, inputExpression);
		}

		private class ExpressionResult
		{
			public Expression BodyExpression { get; }
			public ParameterExpression InputExpression { get; }

			public ExpressionResult(Expression bodyExpression, ParameterExpression inputExpression)
			{
				BodyExpression = bodyExpression;
				InputExpression = inputExpression;
			}
		}

		private class TextWriterRunnable : IRunnable<Action<TextWriter>>
		{
			private readonly Action<TextWriter, object> _action;

			public TextWriterRunnable(Action<TextWriter, object> action)
			{
				_action = action;
			}

			public Action<TextWriter> Execute(object data, IRenderingContext renderingContext)
			{
				return writer => _action(writer, data);
			}
		}

		private class VTreeRunnable : IRunnable<VTree>
		{
			private readonly Func<object, VTree> _action;

			public VTreeRunnable(Func<object, VTree> action)
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
