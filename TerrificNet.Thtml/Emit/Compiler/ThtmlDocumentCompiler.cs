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

		public IViewTemplate Compile(IDataBinder dataBinder, IOutputExpressionBuilderFactory outputFactory)
		{
			var dataScopeContract = CreateDataScope(dataBinder);
			var renderingContextExpression = Expression.Parameter(typeof(IRenderingContext));
			var outputExpression = Expression.Property(renderingContextExpression, nameof(IRenderingContext.OutputBuilder));
			var output = outputFactory.CreateExpressionBuilder(outputExpression);
			return CreateTemplate(CreateExpression(dataScopeContract, _extensions.WithOutput(output), renderingContextExpression));
		}

		private static DataScope CreateDataScope(IDataBinder dataBinder)
		{
			var dataContextParameter = Expression.Variable(dataBinder.ResultType, "item");
			return new DataScope(new DataScopeContract(BindingPathTemplate.Global), dataBinder, dataContextParameter);
		}

		private CompilerResult CreateExpression(IDataScopeContract dataScopeContract, CompilerExtensions compilerExtensions, ParameterExpression renderingContextExpression)
		{
			var visitor = new EmitExpressionVisitor(dataScopeContract, compilerExtensions, renderingContextExpression);
			var expression = visitor.Visit(_input);

			var inputExpression = Expression.Parameter(typeof(object), "input");

			var convertExpression = Expression.Assign(dataScopeContract.Expression, Expression.ConvertChecked(inputExpression, dataScopeContract.Expression.Type));
			var bodyExpression = Expression.Block(new[] { (ParameterExpression)dataScopeContract.Expression }, convertExpression, expression);
			return new CompilerResult(bodyExpression, inputExpression, renderingContextExpression);
		}

		private static IViewTemplate CreateTemplate(CompilerResult result)
		{
			var expression = Expression.Lambda(result.BodyExpression, result.InputExpression, result.RenderingContextExpression);

			var creationExpression = Expression.Lambda<Func<IViewTemplate>>(Expression.New(typeof(Template).GetTypeInfo().GetConstructors().First(), expression));
			var action = creationExpression.Compile();

			return action();
		}
	}
}