using System.Diagnostics.CodeAnalysis;
using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;

namespace TerrificNet.Thtml.Test.Asserts
{
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    internal static class HandlebarsExpressionAssert
    {
        public static void AssertExpression(MustacheExpression expected, MustacheExpression result)
        {
            if (expected == null)
                Assert.Null(result);

            var memberAccess = expected as MemberExpression;
            var conditional = expected as ConditionalExpression;
            var helper = expected as CallHelperExpression;
            var iteration = expected as IterationExpression;
            var html = expected as UnconvertedExpression;

            if (memberAccess != null)
            {
                Assert.IsType<MemberExpression>(result);
                AssertExpression(memberAccess, result as MemberExpression);
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
            else if (html != null)
            {
                Assert.IsType<UnconvertedExpression>(result);
                AssertExpression(html, result as UnconvertedExpression);
            }
            else
                Assert.False(true, "Unknown expression type.");
        }

        private static void AssertIterationExpression(IterationExpression expected, IterationExpression actual)
        {
            AssertExpression(expected.Expression, actual.Expression);
        }

        public static void AssertExpression(UnconvertedExpression expected, UnconvertedExpression result)
        {
            AssertExpression(expected.Expression, result.Expression);
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
            AssertExpression(expected.Expression, result.Expression);
        }

        public static void AssertExpression(MemberExpression expected, MemberExpression result)
        {
            Assert.Equal(expected.Name, result.Name);
            if (expected.SubExpression == null)
            {
                Assert.Null(result.SubExpression);
                return;
            }

            AssertExpression(expected.SubExpression, result.SubExpression);
        }
    }
}