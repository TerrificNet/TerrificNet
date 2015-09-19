using System.Collections.Generic;
using TerrificNet.Thtml.LexicalAnalysis;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class LexerTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestLexerTokenization(string input, IEnumerable<Token> expectedResult)
        {
            var lexer = new Lexer();
            var result = lexer.Tokenize(input);

            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestTokenWithSameCategoryAndSameStringAreEqual()
        {
            var token1 = new Token(TokenCategory.Whitespace, "  ", 2, 3);
            var token2 = new Token(TokenCategory.Whitespace, "  ", 2, 3);

            Assert.True(Equals(token1, token2));
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                var startToken = new Token(TokenCategory.StartDocument, 0, 0);
                yield return new object[] { string.Empty, new[] { startToken, TokenFactory.EndToken(0) } };
                yield return new object[] { " ", new[] { startToken, TokenFactory.Whitespace(" ", 0), TokenFactory.EndToken(1) } };
                yield return new object[] { "   ", new[] { startToken, TokenFactory.Whitespace("   ", 0), TokenFactory.EndToken(3) } };
                yield return new object[] { "<html>", new[] { startToken, TokenFactory.ElementStart("<html>", 0, 6), TokenFactory.EndToken(6) } };
                yield return new object[] { "<html >", new[] { startToken, TokenFactory.ElementStart("<html >", 0, 7), TokenFactory.EndToken(7) } };
                yield return new object[] { "<html attribute>", new[] { startToken, TokenFactory.ElementStart("<html attribute>", 0, 16), TokenFactory.EndToken(16) } };
                yield return new object[] { "<html attribute=\"hallo\">", new[] { startToken, TokenFactory.ElementStart("<html attribute=\"hallo\">", 0, 24), TokenFactory.EndToken(24) } };
                yield return new object[] { "<html attribute=\"\">", new[] { startToken, TokenFactory.ElementStart("<html attribute=\"\">", 0, 19), TokenFactory.EndToken(19) } };
                yield return new object[] { "<html attribute=\"hallo\" att2=\"val2\">", new[] { startToken, TokenFactory.ElementStart("<html attribute=\"hallo\" att2=\"val2\">", 0, 36), TokenFactory.EndToken(36) } };
                yield return new object[] { "<html ><h1>", new[] { startToken, TokenFactory.ElementStart("<html >", 0, 7), TokenFactory.ElementStart("<h1>", 7, 11), TokenFactory.EndToken(11) } };
                yield return new object[] { "</h1>", new[] { startToken, TokenFactory.ElementEnd("</h1>", 0, 5), TokenFactory.EndToken(5) } };
                yield return new object[] { "</h1  >", new[] { startToken, TokenFactory.ElementEnd("</h1  >", 0, 7), TokenFactory.EndToken(7) } };
                yield return new object[] { "<h1>content</h1>", new[] { startToken, TokenFactory.ElementStart("<h1>", 0, 4), TokenFactory.Content("content", 4), TokenFactory.ElementEnd("</h1>", 11, 16), TokenFactory.EndToken(16) } };
                yield return new object[] { "<h1 attr />", new[] { startToken, TokenFactory.EmptyElement("<h1 attr />", 0, 11), TokenFactory.EndToken(11) } };
            }
        }
    }
}
