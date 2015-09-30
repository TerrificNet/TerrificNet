using System.Diagnostics.CodeAnalysis;
using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    internal static class HandlebarsExpressionAssert
    {
        public static void AssertEvaluateExpression(EvaluateExpression expected, EvaluateExpression result)
        {
            if (expected is EvaluateInHtmlExpression)
                Assert.IsType<EvaluateInHtmlExpression>(result);

            AssertExpression(expected.Expression, result.Expression);
        }

        public static void AssertExpression(AccessExpression expected, AccessExpression result)
        {
            if (expected == null)
                Assert.Null(result);

            var memberAccess = expected as MemberAccessExpression;
            var conditional = expected as ConditionalExpression;
            var helper = expected as CallHelperExpression;
            var helperBound = expected as CallHelperBoundExpression;
            var iteration = expected as IterationExpression;

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
            else if (iteration != null)
            {
                Assert.IsType<IterationExpression>(result);
                AssertIterationExpression(iteration, (IterationExpression) result);
            }
            else if (helper != null)
            {
                Assert.IsType<CallHelperExpression>(result);
                AssertExpression(helper, result as CallHelperExpression);
            }
            else if (helperBound != null)
            {
                Assert.IsType<CallHelperBoundExpression>(result);
                AssertExpression(helperBound, result as CallHelperBoundExpression);
            }
            else
                Assert.False(true, "Unknown expression type.");
        }

        private static void AssertIterationExpression(IterationExpression expected, IterationExpression actual)
        {
            AssertExpression(expected.Expression, actual.Expression);
        }

        public static void AssertExpression(CallHelperBoundExpression expected, CallHelperBoundExpression result)
        {
            Assert.Equal(expected.Name, result.Name);
            AssertExpression(expected.AccessExpression, result.AccessExpression);
        }

        public static void AssertExpression(CallHelperExpression expected, CallHelperExpression result)
        {
            Assert.Equal(expected.Name, result.Name);
            if (expected.Attributes == null)
                Assert.Null(result.Attributes);
            else
            {
                Assert.Equal(expected.Attributes.Length, result.Attributes.Length);
                for (int i = 0; i < expected.Attributes.Length; i++)
                {
                    AssertHelperAttribute(expected.Attributes[i], result.Attributes[i]);
                }
            }
        }

        private static void AssertHelperAttribute(HelperAttribute expected, HelperAttribute result)
        {
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.Value, result.Value);
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
}