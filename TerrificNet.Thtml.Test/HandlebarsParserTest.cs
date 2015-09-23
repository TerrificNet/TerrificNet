using System.Collections.Generic;
using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class HandlebarsParserTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestParser(string input, EvaluateExpression expected)
        {
            var underTest = new HandlebarsParser();
            var result = underTest.Parse(input);

            HandlebarsExpressionAssert.AssertEvaluateExpression(expected, result);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[]
                {
                    "test",
                    new EvaluateExpression(new MemberAccessExpression("test"))
                };
                yield return new object[]
                {
                    "test.property1",
                    new EvaluateExpression(new MemberAccessExpression("test", new MemberAccessExpression("property1")))
                };
                yield return new object[]
                {
                    "test . property1.property2 ",
                    new EvaluateExpression(new MemberAccessExpression("test", new MemberAccessExpression("property1", new MemberAccessExpression("property2"))))
                };
            }
        }
    }
}
