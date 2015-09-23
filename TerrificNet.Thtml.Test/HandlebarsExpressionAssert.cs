using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;

static internal class HandlebarsExpressionAssert
{
    public static void AssertEvaluateExpression(EvaluateExpression expected, EvaluateExpression result)
    {
        AssertExpression(expected.Expression, result.Expression);
    }

    public static void AssertExpression(MemberAccessExpression expected, MemberAccessExpression result)
    {
        Assert.Equal(expected.Name, result.Name);
        if (expected.Expression == null)
        {
            Assert.Null(result.Expression);
            return;
        }

        AssertExpression(expected.Expression, result.Expression);
    }
}