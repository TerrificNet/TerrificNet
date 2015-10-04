using System.Collections.Generic;
using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class HandlebarsParserTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestParser(string input, MustacheExpression expected)
        {
            var underTest = new HandlebarsParser();
            var result = underTest.Parse(input);

            HandlebarsExpressionAssert.AssertExpression(expected, result);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[]
                {
                    "test",
                    new MemberExpression("test")
                };
                yield return new object[]
                {
                    "test.property1",
                    new MemberExpression("test", new MemberExpression("property1"))
                };
                yield return new object[]
                {
                    "test . property1.property2 ",
                    new MemberExpression("test", new MemberExpression("property1", new MemberExpression("property2")))
                };
                yield return new object[]
                {
                    "#if test",
                    new ConditionalExpression(new MemberExpression("test"))
                };
                yield return new object[]
                {
                    "#each test",
                    new IterationExpression(new MemberExpression("test"))
                };
                yield return new object[]
                {
                    "{test}",
                    new UnconvertedExpression(new MemberExpression("test"))
                };
                yield return new object[]
                {
                    "#test val=\"1\" val2=\"2\"",
                    new CallHelperExpression("test", new HelperAttribute("val", "1"), new HelperAttribute("val2", "2"))
                };
                yield return new object[]
                {
                    "test val=\"1\" val2=\"2\"",
                    new CallHelperExpression("test", new HelperAttribute("val", "1"), new HelperAttribute("val2", "2"))
                };
            }
        }
    }
}
