using System;
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
			_helperBinder = helperBinder;
		}

		public IRunnable<Action<TextWriter>> CompileForTextWriter(IDataBinder dataBinder)
		{
			var dataScopeContract = new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), dataBinder);
            var dataContextParameter = Expression.Variable(dataScopeContract.ResultType, "item");
			var writerParameter = Expression.Parameter(typeof (TextWriter));
			var handler = new StreamOutputExpressionEmitter(writerParameter);

			var visitor = new EmitExpressionVisitor(dataScopeContract, _helperBinder ?? new NullHelperBinder(), dataBinder, dataContextParameter, handler);
			var expression = visitor.Visit(_input);

			var inputExpression = Expression.Parameter(typeof(object), "input");
			var convertExpression = Expression.Assign(dataContextParameter, Expression.ConvertChecked(inputExpression, dataScopeContract.ResultType));
			var bodyExpression = Expression.Block(new[] { dataContextParameter }, convertExpression, expression);
			var action = Expression.Lambda<Action<TextWriter, object>>(bodyExpression, writerParameter, inputExpression).Compile();

			return new TextWriterRunnable(action);
		}

		public IRunnable<VTree> CompileForVTree(IDataBinder dataBinder)
		{
			var dataScopeContract = new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), dataBinder);
            var dataContextParameter = Expression.Variable(dataScopeContract.ResultType, "item");
			var handler = new VTreeOutputExpressionEmitter();

			var visitor = new EmitExpressionVisitor(dataScopeContract, _helperBinder, dataBinder, dataContextParameter, handler);
			var expression = visitor.Visit(_input);

			var inputExpression = Expression.Parameter(typeof(object), "input");
			var convertExpression = Expression.Assign(dataContextParameter, Expression.ConvertChecked(inputExpression, dataScopeContract.ResultType));
			var bodyExpression = Expression.Block(new[] { dataContextParameter }, convertExpression, expression);
			var action = Expression.Lambda<Func<object, VTree>>(bodyExpression, inputExpression).Compile();

			return new VTreeRunnable(action);
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
