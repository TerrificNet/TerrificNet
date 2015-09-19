using System.Collections.Generic;
using TerrificNet.Thtml.LexicalAnalysis;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class ParserTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestParser(IEnumerable<Token> tokens, HtmlNode expectedNode)
        {
            var parser = new Parser();
            var result = parser.Parse(tokens);

            Assert.NotNull(result);
            AssertNode(expectedNode, result);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                var startToken = new Token(TokenCategory.StartDocument, 0, 0);
                yield return new object[] { new[] { startToken, TokenFactory.EndToken(0) }, new HtmlDocument() };

                var content = TokenFactory.Content("test", 0);
                yield return new object[] { new[] { startToken, content, TokenFactory.EndToken(4) }, new HtmlDocument(new HtmlTextNode(content)) };
            }
        }

        private static void AssertNode(HtmlNode expected, HtmlNode actual)
        {
            if (expected == null)
                Assert.Null(actual);
            
            Assert.IsType(expected.GetType(), actual);

            var eDocument = expected as HtmlDocument;
            var aDocument = actual as HtmlDocument;

            var eContent = expected as HtmlTextNode;
            var aContent = actual as HtmlTextNode;

            if (eDocument != null)
            {
                AssertDocument(eDocument, aDocument);
            }
            else if (eContent != null)
            {
                AssertContent(eContent, aContent);
            }
            else
                Assert.True(false, "Unknown type");
        }

        private static void AssertContent(HtmlTextNode expected, HtmlTextNode actual)
        {
            Assert.Equal(expected.Text, actual.Text);
        }

        private static void AssertDocument(HtmlDocument expected, HtmlDocument actual)
        {
            var expectedList = expected.ChildNodes;

            Assert.Equal(expectedList.Count, actual.ChildNodes.Count);
            for (int i = 0; i < expectedList.Count; i++)
            {
                AssertNode(expectedList[i], actual.ChildNodes[i]);
            }
        }
    }
}
