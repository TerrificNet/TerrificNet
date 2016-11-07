using System.Linq.Expressions;
using Moq;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Formatting;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class FallbackRenderingScopeInterceptorTest
	{
		private readonly DefaultExpression _expression1;
		private readonly IBinding _supportedBinding;
		private readonly IBinding _unsupportedBinding;
		private readonly IBindingSupport _bindingSupport;

		public FallbackRenderingScopeInterceptorTest()
		{
			_expression1 = Expression.Empty();
			_supportedBinding = new Mock<IBinding>().Object;
			_unsupportedBinding = new Mock<IBinding>().Object;

			var outputMock = new Mock<IIncrementalOutputExpressionBuilder>();
			outputMock.Setup(b => b.SupportsBinding(_supportedBinding)).Returns(true);
			outputMock.Setup(b => b.SupportsBinding(_unsupportedBinding)).Returns(false);

			_bindingSupport = outputMock.Object;
		}

		[Fact]
		public void FallbackRenderingScopeInterceptor_NoUnsupportedBinding_UsesDefaultOutput()
		{
			var scope = new RenderingScope(null, null);
			scope.UseBinding(_supportedBinding);
			scope.AddExpression(_expression1);

			var expressionBuilder = new Mock<IExpressionBuilder>();
			expressionBuilder.Setup(b => b.Add(_expression1));

			var underTest = new FallbackRenderingScopeInterceptor(_bindingSupport, new Mock<IExpressionBuilder>().Object, new Mock<IIncrementalOutputExpressionBuilder>().Object);

			scope.Process(new ScopeParameters(expressionBuilder.Object, underTest));

			expressionBuilder.VerifyAll();
		}

		[Fact]
		public void FallbackRenderingScopeInterceptor_UnsupportedBinding_UseFallbackOutput()
		{
			var idBinding = new ConstantBinding("id");
			var scope = new RenderingScope(null, idBinding);
			scope.UseBinding(_unsupportedBinding);
			scope.AddExpression(_expression1);

			var expressionBuilder = new Mock<IExpressionBuilder>();
			var fallbackExpressionBuilder = new Mock<IExpressionBuilder>();
			fallbackExpressionBuilder.Setup(b => b.Add(_expression1));

			var outputMock = new Mock<IIncrementalOutputExpressionBuilder>();
			outputMock.Setup(o => o.EnterScope(fallbackExpressionBuilder.Object, idBinding));
			outputMock.Setup(o => o.LeaveScope(fallbackExpressionBuilder.Object));

			var underTest = new FallbackRenderingScopeInterceptor(_bindingSupport, fallbackExpressionBuilder.Object, outputMock.Object);

			scope.Process(new ScopeParameters(expressionBuilder.Object, underTest));

			expressionBuilder.VerifyAll();
			fallbackExpressionBuilder.VerifyAll();
			outputMock.VerifyAll();
		}
	}
}
