using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;

static internal class HandlebarsExpressionAssert
{
    public static void AssertEvaluateExpression(EvaluateExpression expected, EvaluateExpression result)
    {
        if (expected is EvaluateInHtmlExpression)
            Assert.IsType<EvaluateInHtmlExpression>(result);

        AssertExpression(expected.Expression, result.Expression);
    }

    public static void AssertExpression(AccessExpression expected, AccessExpression result)
    {
        var memberAccess = expected as MemberAccessExpression;
        var conditional = expected as ConditionalExpression;

        if (memberAccess != null)
        {
            Assert.IsType<MemberAccessExpression>(result);
            AssertExpression(memberAccess, result as MemberAccessExpression);
        }
        else if (conditional != null)
        {
            Assert.IsType<ConditionalExpression>(result);
            AssertExpression(conditional, result as ConditionalExpression);
        }
        else
            Assert.False(true, "Unknown expression type.");
    }

    public static void AssertExpression(ConditionalExpression expected, ConditionalExpression result)
    {
        AssertExpression(expected.MemberAccessExpression, result.MemberAccessExpression);
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