using System.Linq.Expressions;
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
			var emitter = emitterFactory.Create();
			var result = CreateExpression(dataBinder, emitter.OutputExpressionEmitter);

			return emitter.WrapResult(result);
		}

		private CompilerResult CreateExpression(IDataBinder dataBinder, IOutputExpressionEmitter handler)
		{
			var dataScopeContract = new DataScopeContractLegacyWrapper(new DataScopeContract(BindingPathTemplate.Global), dataBinder);
			var dataContextParameter = Expression.Variable(dataBinder.DataContextType, "item");

			var visitor = new EmitExpressionVisitor(dataScopeContract, _helperBinder, dataBinder, dataContextParameter, handler);
			var expression = visitor.Visit(_input);

			var inputExpression = Expression.Parameter(typeof(object), "input");
			var convertExpression = Expression.Assign(dataContextParameter, Expression.ConvertChecked(inputExpression, dataBinder.DataContextType));
			var bodyExpression = Expression.Block(new[] {dataContextParameter}, convertExpression, expression);
			return new CompilerResult(bodyExpression, inputExpression);
		}
	}
}