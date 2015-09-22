using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class HandlebarsExpressionTest
    {
        [Fact]
        public void Test()
        {
            var expected = new EvaluateExpression(new MemberAccessExpression("test"));

            var underTest = new HandlebarsParser();
            var result = underTest.Parse("test");

            AssertEvaluateExpression(expected, result);
        }

        private static void AssertEvaluateExpression(EvaluateExpression expected, EvaluateExpression result)
        {
            AssertExpression(expected.Expression, result.Expression);
        }

        private static void AssertExpression(MemberAccessExpression expected, MemberAccessExpression result)
        {
            Assert.Equal(expected.Name, result.Name);
        }
    }
}
