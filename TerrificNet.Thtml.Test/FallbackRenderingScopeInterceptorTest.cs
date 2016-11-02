using System.Linq.Expressions;
using Moq;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
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

			var bindingSupportMock = new Mock<IBindingSupport>();
			bindingSupportMock.Setup(b => b.SupportsBinding(_supportedBinding)).Returns(true);
			bindingSupportMock.Setup(b => b.SupportsBinding(_unsupportedBinding)).Returns(false);

			_bindingSupport = bindingSupportMock.Object;
		}

		[Fact]
		public void FallbackRenderingScopeInterceptor_NoUnsupportedBinding_UsesDefaultOutput()
		{
			var scope = new RenderingScope(null);
			scope.UseBinding(_supportedBinding);
			scope.AddExpression(_expression1);

			var expressionBuilder = new Mock<IExpressionBuilder>();
			expressionBuilder.Setup(b => b.Add(_expression1));

			var underTest = new FallbackRenderingScopeInterceptor(_bindingSupport, new Mock<IExpressionBuilder>().Object);

			scope.Process(new ScopeParameters(expressionBuilder.Object, underTest));

			expressionBuilder.VerifyAll();
		}

		[Fact]
		public void FallbackRenderingScopeInterceptor_UnsupportedBinding_UseFallbackOutput()
		{
			var scope = new RenderingScope(null);
			scope.UseBinding(_unsupportedBinding);
			scope.AddExpression(_expression1);

			var expressionBuilder = new Mock<IExpressionBuilder>();
			var fallbackExpressionBuilder = new Mock<IExpressionBuilder>();
			fallbackExpressionBuilder.Setup(b => b.Add(_expression1));

			var underTest = new FallbackRenderingScopeInterceptor(_bindingSupport, fallbackExpressionBuilder.Object);

			scope.Process(new ScopeParameters(expressionBuilder.Object, underTest));

			expressionBuilder.VerifyAll();
			fallbackExpressionBuilder.VerifyAll();
		}
	}
}
