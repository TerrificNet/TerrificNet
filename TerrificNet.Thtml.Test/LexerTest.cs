﻿using System.Collections.Generic;
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
            Assert.Equal(expectedResult, result, new TokenEqualityComparer());
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                var startToken = new Token(TokenCategory.StartDocument, 0, 0);
                yield return new object[] 
                {
                    "",
                    TokenFactory.DocumentList()
                };
                yield return new object[]
                {
                    " ",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Whitespace(" ", i))
                };
                yield return new object[]
                {
                    "   ",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Whitespace("   ", i))
                };
                yield return new object[]
                {
                    "<html>",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementStart,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("html", a), 
                            TokenFactory.BracketClose))
                };
                yield return new object[]
                {
                    "<html >",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementStart,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("html", a),
                            TokenFactory.Whitespace,
                            TokenFactory.BracketClose))
                };
                yield return new object[]
                {
                    "<html attribute>",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementStart,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("html", a),
                            TokenFactory.Whitespace,
                            a => TokenFactory.Composite(a, 
                                TokenCategory.Attribute,
                                b => TokenFactory.Name("attribute", b)),
                            TokenFactory.BracketClose))
                };

                yield return new object[]
                {
                    "<html attribute=\"hallo\">",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementStart,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("html", a),
                            TokenFactory.Whitespace,
                            a => TokenFactory.Composite(a,
                                TokenCategory.Attribute,
                                b => TokenFactory.Name("attribute", b),
                                TokenFactory.Equal,
                                TokenFactory.Quote,
                                b => TokenFactory.AttributeContent("hallo", b),
                                TokenFactory.Quote),
                            TokenFactory.BracketClose))
                };
                yield return new object[]
                {
                    "<html attribute=\"\">",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementStart,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("html", a),
                            TokenFactory.Whitespace,
                            a => TokenFactory.Composite(a,
                                TokenCategory.Attribute,
                                b => TokenFactory.Name("attribute", b),
                                TokenFactory.Equal,
                                TokenFactory.Quote,
                                TokenFactory.Quote),
                            TokenFactory.BracketClose))
                };
                yield return new object[]
                {
                    "<html attribute=\"hallo\" att2=\"val2\">",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementStart,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("html", a),
                            TokenFactory.Whitespace,
                            a => TokenFactory.AttributeWithContent(a, "attribute", "hallo"),
                            TokenFactory.Whitespace,
                            a => TokenFactory.AttributeWithContent(a, "att2", "val2"),
                            TokenFactory.BracketClose))
                };
                yield return new object[]
                {
                    "<html ><h1>",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementStart,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("html", a),
                            TokenFactory.Whitespace,
                            TokenFactory.BracketClose),
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementStart,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("h1", a),
                            TokenFactory.BracketClose))
                };
                yield return new object[]
                {
                    "</h1>",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementEnd,
                            TokenFactory.BracketOpen,
                            TokenFactory.Slash,
                            a => TokenFactory.Name("h1", a),
                            TokenFactory.BracketClose))
                };
                yield return new object[]
                {
                    "</h1  >",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementEnd,
                            TokenFactory.BracketOpen,
                            TokenFactory.Slash,
                            a => TokenFactory.Name("h1", a),
                            a => TokenFactory.Whitespace("  ", a),
                            TokenFactory.BracketClose))
                };
                yield return new object[]
                {
                    "<h1>content</h1>",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementStart,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("h1", a),
                            TokenFactory.BracketClose),
                        i => TokenFactory.Content("content", i),
                        i => TokenFactory.Composite(i,
                            TokenCategory.ElementEnd,
                            TokenFactory.BracketOpen,
                            TokenFactory.Slash,
                            a => TokenFactory.Name("h1", a),
                            TokenFactory.BracketClose))
                };
                yield return new object[]
                {
                    "<h1 attr />",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.EmptyElement,
                            TokenFactory.BracketOpen,
                            a => TokenFactory.Name("h1", a),
                            TokenFactory.Whitespace,
                            a => TokenFactory.Composite(a,
                                TokenCategory.Attribute,
                                b => TokenFactory.Name("attr", b),
                                TokenFactory.Whitespace),
                            TokenFactory.Slash,
                            TokenFactory.BracketClose))
                };
            }
        }
    }
}
