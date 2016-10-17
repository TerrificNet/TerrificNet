using System.Linq.Expressions;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class ThtmlDocumentCompiler
	{
		private readonly Document _input;
		private readonly CompilerExtensions _extensions;

		public ThtmlDocumentCompiler(Document input, CompilerExtensions extensions)
		{
			_input = input;
			_extensions = extensions;
		}

		public IViewTemplate<T> Compile<T>(IDataBinder dataBinder, IEmitterFactory<T> emitterFactory)
		{
			var dataScopeContract = CreateDataScope(dataBinder);
			return Compile(dataScopeContract, emitterFactory.Create());
		}

		public IViewTemplate Compile(IDataBinder dataBinder, IEmitter emitter)
		{
			var dataScopeContract = CreateDataScope(dataBinder);
			return emitter.CreateTemplate(CreateExpression(dataScopeContract, _extensions.WithEmitter(emitter)));
		}

		private IViewTemplate<T> Compile<T>(IDataScopeContract dataScopeContract, IEmitter<T> emitter)
		{
			var result = CreateExpression(dataScopeContract, _extensions.WithEmitter(emitter));
			return emitter.WrapResult(result);
		}

		private static DataScope CreateDataScope(IDataBinder dataBinder)
		{
			var dataContextParameter = Expression.Variable(dataBinder.ResultType, "item");
			return new DataScope(new DataScopeContract(BindingPathTemplate.Global), dataBinder, dataContextParameter);
		}

		private CompilerResult CreateExpression(IDataScopeContract dataScopeContract, CompilerExtensions compilerExtensions)
		{
			var visitor = new EmitExpressionVisitor(dataScopeContract, compilerExtensions);
			var expression = visitor.Visit(_input);

			var inputExpression = Expression.Parameter(typeof(object), "input");

			var convertExpression = Expression.Assign(dataScopeContract.Expression, Expression.ConvertChecked(inputExpression, dataScopeContract.Expression.Type));
			var bodyExpression = Expression.Block(new[] {(ParameterExpression)dataScopeContract.Expression}, convertExpression, expression);
			return new CompilerResult(bodyExpression, inputExpression);
		}
	}
}