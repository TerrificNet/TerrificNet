using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Formatting;
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
			var compilerResult = CreateCompilerResult(dataBinder, outputFactory, new ExpressionBuilder(), false);

			return CreateSyncTemplate(compilerResult);
		}

		public IAsyncViewTemplate CompileForAsync(IDataBinder dataBinder, IOutputExpressionBuilderFactory outputFactory)
		{
			var compilerResult = CreateCompilerResult(dataBinder, outputFactory, new AsyncExpressionBuilder(), true);

			return CreateAsyncTemplate(compilerResult);
		}

		private CompilerResult CreateCompilerResult(IDataBinder dataBinder, IOutputExpressionBuilderFactory outputFactory, IExpressionBuilder expressionBuilder, bool supportAsync)
		{
			var dataScopeContract = CreateDataScope(dataBinder);
			var renderingContextExpression = Expression.Parameter(typeof(IRenderingContext));
			var outputExpression = Expression.Property(renderingContextExpression, nameof(IRenderingContext.OutputBuilder));
			var output = outputFactory.CreateExpressionBuilder(outputExpression);
			var compilerExtensions = _extensions.WithOutput(output);
			if (supportAsync)
				compilerExtensions = compilerExtensions.WithAsyncSupport();

			var compilerResult = CreateExpression(dataScopeContract, compilerExtensions, renderingContextExpression, expressionBuilder);

			return compilerResult;
		}

		private static DataScope CreateDataScope(IDataBinder dataBinder)
		{
			var dataContextParameter = Expression.Variable(dataBinder.ResultType, "item");
			return new DataScope(new DataScopeContract(BindingPathTemplate.Global), dataBinder, dataContextParameter);
		}

		private CompilerResult CreateExpression(DataScope dataScopeContract, CompilerExtensions compilerExtensions, ParameterExpression renderingContextExpression, IExpressionBuilder expressionBuilder)
		{
			var clientScope = new DataScopeContract(BindingPathTemplate.Global.Property("$scope"));
			IScopedExpressionBuilder scopedExpressionBuilder;
			if (compilerExtensions.SkipUnsupportedBindings)
				scopedExpressionBuilder = new ScopedExpressionBuilder(expressionBuilder, new DefaultRenderingScopeInterceptor(compilerExtensions.ExpressionBuilder));
			else
				scopedExpressionBuilder = new EmptyScopedExpressionBuilder(expressionBuilder, compilerExtensions.ExpressionBuilder);

			var visitor = new EmitExpressionVisitor(new DataScopeWithClientContract(dataScopeContract, clientScope), compilerExtensions, renderingContextExpression, scopedExpressionBuilder);
			visitor.Visit(_input);
			var expression = scopedExpressionBuilder.BuildExpression();

			var inputExpression = Expression.Parameter(typeof(object), "input");

			var convertExpression = Expression.Assign(dataScopeContract.Expression, Expression.ConvertChecked(inputExpression, dataScopeContract.Expression.Type));
			var bodyExpression = Expression.Block(new[] { (ParameterExpression)dataScopeContract.Expression }, convertExpression, expression);
			return new CompilerResult(bodyExpression, inputExpression, renderingContextExpression);
		}

		private static IViewTemplate CreateSyncTemplate(CompilerResult result)
		{
			var expression = Expression.Lambda(result.BodyExpression, result.InputExpression, result.RenderingContextExpression);

			var creationExpression = Expression.Lambda<Func<IViewTemplate>>(Expression.New(typeof(CompiledSyncTemplate).GetTypeInfo().GetConstructors().First(), expression));
			var action = creationExpression.Compile();

			return action();
		}

		private static IAsyncViewTemplate CreateAsyncTemplate(CompilerResult result)
		{
			var expression = Expression.Lambda(result.BodyExpression, result.InputExpression, result.RenderingContextExpression);

			var creationExpression = Expression.Lambda<Func<IAsyncViewTemplate>>(Expression.New(typeof(CompiledAsyncTemplate).GetTypeInfo().GetConstructors().First(), expression));
			var action = creationExpression.Compile();

			return action();
		}
	}

	internal class EmptyScopedExpressionBuilder : IScopedExpressionBuilder
	{
		private readonly IExpressionBuilder _expressionBuilder;
		private readonly IBindingSupport _bindingSupport;

		public EmptyScopedExpressionBuilder(IExpressionBuilder expressionBuilder, IBindingSupport bindingSupport)
		{
			_expressionBuilder = expressionBuilder;
			_bindingSupport = bindingSupport;
		}

		public void Add(Expression expression)
		{
			_expressionBuilder.Add(expression);
		}

		public void DefineVariable(ParameterExpression expression)
		{
			_expressionBuilder.DefineVariable(expression);
		}

		public Expression BuildExpression()
		{
			return _expressionBuilder.BuildExpression();
		}

		public void UseBinding(IBinding binding)
		{
			if (!_bindingSupport.SupportsBinding(binding))
				throw new NotSupportedException($"The given binding with path '{binding.Path}' isn't supported.");
		}

		public void Enter()
		{
		}

		public IRenderingScope Leave()
		{
			return null;
		}
	}
}