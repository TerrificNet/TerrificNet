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
			var expectedExpression = Expression.Empty();

			var expressionBuilder = new ExpressionBuilder();
			var scopeCondition = new ScopedExpressionBuilder(expressionBuilder);
			scopeCondition.Enter();
			scopeCondition.Add(expectedExpression);
			scopeCondition.Leave();

			var ex = scopeCondition.BuildExpression();
			Assert.NotNull(ex);
			var blockExpression = Assert.IsAssignableFrom<BlockExpression>(ex);
			Assert.Equal(1, blockExpression.Expressions.Count);
			Assert.Equal(expectedExpression, blockExpression.Expressions[0]);
		}

		[Fact]
		public void ScopedExpressionBuilder_ExpressionWithBinding_IncludeCondition()
		{
			var expectedExpression = Expression.Empty();
			var bindingMock = new Mock<IBinding>();

			var expressionBuilder = new ExpressionBuilder();
			var scopeCondition = new ScopedExpressionBuilder(expressionBuilder);
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
		public void ScopedExpressionBuilder_Begin_StartsNewScope()
		{
			var bindingMock = new Mock<IBinding>();
			var bindingMock2 = new Mock<IBinding>();

			var expressionBuilder = new ExpressionBuilder();
			var underTest = new ScopedExpressionBuilder(expressionBuilder);
			underTest.Enter();
			underTest.UseBinding(bindingMock.Object);
			underTest.Enter();
			underTest.UseBinding(bindingMock2.Object);
			var resultInner = underTest.Leave();
			var resultOuter = underTest.Leave();

			Assert.NotEqual(resultInner, resultOuter);
			Assert.Equal(resultInner.Parent, resultOuter);
			Assert.Equal(resultInner, resultOuter.Children.First());
			Assert.Equal(1, resultInner.GetBindings().Count());
			Assert.Equal(1, resultOuter.GetBindings().Count());
		}
	}
}
