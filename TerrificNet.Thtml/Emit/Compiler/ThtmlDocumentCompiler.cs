using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

		public IViewTemplate<T> Compile<T>(IDataBinder dataBinder, EmitterFactory<T> emitterFactory)
		{
			var emitter = emitterFactory.Create();
			return (IViewTemplate<T>) Compile(dataBinder, emitter);
		}

		public IViewTemplate Compile(IDataBinder dataBinder, IEmitter emitter)
		{
			var dataScopeContract = CreateDataScope(dataBinder);
			return CreateTemplate(CreateExpression(dataScopeContract, _extensions.WithOutput(emitter.ExpressionBuilder)), emitter.ExpressionBuilder);
		}

		public IViewTemplate Compile(IDataBinder dataBinder, IOutputExpressionBuilder output)
		{
			var dataScopeContract = CreateDataScope(dataBinder);
			return CreateTemplate(CreateExpression(dataScopeContract, _extensions.WithOutput(output)), output);
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

		private static IViewTemplate CreateTemplate(CompilerResult result, IOutputExpressionBuilder output)
		{
			var expression = Expression.Lambda(result.BodyExpression, (ParameterExpression)output.InstanceExpression, result.InputExpression);

			var creationExpression = Expression.Lambda<Func<IViewTemplate>>(Expression.New(typeof(Template<>).MakeGenericType(output.InstanceExpression.Type).GetTypeInfo().GetConstructors().First(), expression));
			var action = creationExpression.Compile();

			return action();
		}
	}
}