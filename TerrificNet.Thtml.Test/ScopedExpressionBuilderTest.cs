using System.Linq;
using System.Linq.Expressions;
using Moq;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class ScopedExpressionBuilderTest
	{
		[Fact]
		public void ScopedExpressionBuilder_EmptyExpression_IgnoreCondition()
		{
			var inputExpression = Expression.Empty();

			var expressionBuilder = new ExpressionBuilder();
			var scopeCondition = new ScopedExpressionBuilder(expressionBuilder, new RequiredRenderingInterceptor());
			scopeCondition.Enter();
			scopeCondition.Add(inputExpression);
			scopeCondition.Leave();

			var ex = scopeCondition.BuildExpression();
			Assert.NotNull(ex);
			var blockExpression = Assert.IsAssignableFrom<BlockExpression>(ex);
			Assert.Equal(1, blockExpression.Expressions.Count);
			Assert.Equal(inputExpression, blockExpression.Expressions[0]);
		}

		[Fact]
		public void ScopedExpressionBuilder_ExpressionWithBinding_IncludeCondition()
		{
			var expectedExpression = Expression.Empty();
			var bindingMock = new Mock<IBinding>();

			var expressionBuilder = new ExpressionBuilder();
			var scopeCondition = new ScopedExpressionBuilder(expressionBuilder, new RequiredRenderingInterceptor());
			scopeCondition.Enter();
			scopeCondition.UseBinding(bindingMock.Object);
			scopeCondition.Add(expectedExpression);
			scopeCondition.Leave();

			var ex = scopeCondition.BuildExpression();
			Assert.NotNull(ex);
			var blockExpression = Assert.IsAssignableFrom<BlockExpression>(ex);
			Assert.Equal(3, blockExpression.Expressions.Count);
			Assert.Equal(expectedExpression, blockExpression.Expressions[1]);
		}

		[Fact]
		public void ScopedExpressionBuilder_Enter_StartsNewScope()
		{
			var bindingMock = new Mock<IBinding>();
			var bindingMock2 = new Mock<IBinding>();

			var expressionBuilder = new ExpressionBuilder();
			var underTest = new ScopedExpressionBuilder(expressionBuilder, new RequiredRenderingInterceptor());
			underTest.Enter();
			underTest.UseBinding(bindingMock.Object);
			underTest.Enter();
			underTest.UseBinding(bindingMock2.Object);
			var resultInner = underTest.Leave();
			var resultOuter = underTest.Leave();

			Assert.NotEqual(resultInner, resultOuter);
			Assert.Equal(resultInner.Parent, resultOuter);
			Assert.Equal(1, resultOuter.Children.Count);
			Assert.Equal(resultInner, resultOuter.Children[0]);
			Assert.Equal(1, resultInner.GetBindings().Count());
			Assert.Equal(1, resultOuter.GetBindings().Count());
		}

		[Fact]
		public void BindingScope_UnsupportedBinding_ExcludesScope()
		{
			var unsupportedBinding = new Mock<IBinding>().Object;

			var expression1 = Expression.Empty();
			var expression2 = Expression.Empty();
			var expressionInUnsupportedScope = Expression.Empty();

			var mock = new Mock<IBindingSupport>();
			mock.Setup(s => s.SupportsBinding(unsupportedBinding)).Returns(false);

			var expressionBuilder = new ExpressionBuilder();

			var scope = new RenderingScope(null, null);
			scope.AddExpression(expression1);
			var childScope = scope.CreateChildScope(null);
			
			childScope.AddExpression(expressionInUnsupportedScope);
			childScope.UseBinding(unsupportedBinding);

			scope.AddExpression(expression2);

			var scopeParameters = new ScopeParameters(expressionBuilder, new OnlySupportedBindingScopeInterceptor(mock.Object));
			scope.Process(scopeParameters);

			var result = expressionBuilder.BuildExpression();
			Assert.NotNull(result);
			var blockExpression = Assert.IsAssignableFrom<BlockExpression>(result);
			Assert.Equal(expression1, blockExpression.Expressions[0]);
			Assert.Equal(expression2, blockExpression.Expressions[1]);
		}
	}
}
