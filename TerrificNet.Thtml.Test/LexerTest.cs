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
                yield return new object[] { string.Empty, new[] { startToken, EndToken(0) } };
                yield return new object[] { " ", new[] { startToken, Whitespace(" ", 0), EndToken(1) } };
                yield return new object[] { "   ", new[] { startToken, Whitespace("   ", 0), EndToken(3) } };
                yield return new object[] { "<html>", new[] { startToken, ElementStart("<html>", 0, 6), EndToken(6) } };
                yield return new object[] { "<html >", new[] { startToken, ElementStart("<html >", 0, 7), EndToken(7) } };
                yield return new object[] { "<html attribute>", new[] { startToken, ElementStart("<html attribute>", 0, 16), EndToken(16) } };
                yield return new object[] { "<html attribute=\"hallo\">", new[] { startToken, ElementStart("<html attribute=\"hallo\">", 0, 24), EndToken(24) } };
                yield return new object[] { "<html attribute=\"\">", new[] { startToken, ElementStart("<html attribute=\"\">", 0, 19), EndToken(19) } };
                yield return new object[] { "<html attribute=\"hallo\" att2=\"val2\">", new[] { startToken, ElementStart("<html attribute=\"hallo\" att2=\"val2\">", 0, 36), EndToken(36) } };
                yield return new object[] { "<html ><h1>", new[] { startToken, ElementStart("<html >", 0, 7), ElementStart("<h1>", 7, 11), EndToken(11) } };
                yield return new object[] { "</h1>", new[] { startToken, ElementEnd("</h1>", 0, 5), EndToken(5) } };
                yield return new object[] { "</h1  >", new[] { startToken, ElementEnd("</h1  >", 0, 7), EndToken(7) } };
                yield return new object[] { "<h1>content</h1>", new[] { startToken, ElementStart("<h1>", 0, 4), Content("content", 4), ElementEnd("</h1>", 11, 16), EndToken(16) } };
            }
        }

        private static Token Content(string lexem, int start)
        {
            return new Token(TokenCategory.Content, lexem, start, start + lexem.Length);
        }

        private static Token ElementEnd(string lexem, int position, int end)
        {
            return new CompositeToken(TokenCategory.ElementEnd, lexem, position, end);
        }

        private static Token EndToken(int position)
        {
            return new Token(TokenCategory.EndDocument, position, position);
        }

        private static Token Whitespace(string lexem, int position)
        {
            return new Token(TokenCategory.Whitespace, lexem, position, position + lexem.Length);
        }

        private static Token ElementStart(string lexem, int position, int end)
        {
            return new CompositeToken(TokenCategory.ElementStart, lexem, position, end);
        }
    }
}
