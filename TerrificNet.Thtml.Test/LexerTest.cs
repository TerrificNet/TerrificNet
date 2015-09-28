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
            Assert.Equal(expectedResult, result, new TokenEqualityComparer());
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
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

                yield return new object[]
                {
                    "<div> <h1 attr /> </div>",
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("div", i),
                        i => TokenFactory.Content(" ", i),
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
                            TokenFactory.BracketClose),
                        i => TokenFactory.Content(" ", i),
                        i => TokenFactory.ElementEnd("div", i))
                };

                // Handlebars
                yield return new object[]
                {
                    "{{name}}",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.External,
                            TokenFactory.HandlebarsStart,
                            TokenFactory.HandlebarsStart,
                            a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate, b => TokenFactory.Expression(b, "name")),
                            TokenFactory.HandlebarsEnd,
                            TokenFactory.HandlebarsEnd))
                };
                yield return new object[]
                {
                    "{{  name }}",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.External,
                            TokenFactory.HandlebarsStart,
                            TokenFactory.HandlebarsStart,
                            a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
                                b => TokenFactory.Whitespace("  ", b),
                                b => TokenFactory.Expression(b, 
                                    c => TokenFactory.Name("name", c),
                                    TokenFactory.Whitespace)),
                            TokenFactory.HandlebarsEnd,
                            TokenFactory.HandlebarsEnd))
                };
                yield return new object[]
                {
                    "{ {  name }}",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Content("{ {  name }}", i))
                };
                yield return new object[]
                {
                    "{{name. value}}",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.External,
                            TokenFactory.HandlebarsStart,
                            TokenFactory.HandlebarsStart,
                            a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
                                b => TokenFactory.Expression(b, 
                                    c => TokenFactory.Name("name", c),
                                TokenFactory.Dot,
                                TokenFactory.Whitespace,
                                c => TokenFactory.Expression(c, "value")
                            )),
                            TokenFactory.HandlebarsEnd,
                            TokenFactory.HandlebarsEnd))
                };
                yield return new object[]
                {
                    "<h1>Ich bin {{name}}.</h1>{{end}}",
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.Content("Ich bin ", i),
                        i => TokenFactory.HandlebarsSimple(i, "name"),
                        i => TokenFactory.Content(".", i),
                        i => TokenFactory.ElementEnd("h1", i),
                        i => TokenFactory.HandlebarsSimple(i, "end"))
                };
                yield return new object[]
                {
                    "{{#group}}",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.External,
                            TokenFactory.HandlebarsStart,
                            TokenFactory.HandlebarsStart,
                            a => TokenFactory.Composite(a, TokenCategory.HandlebarsBlockStart,
                                TokenFactory.Hash,
                                b => TokenFactory.Name("group", b)),
                            TokenFactory.HandlebarsEnd,
                            TokenFactory.HandlebarsEnd))
                };
                yield return new object[]
                {
                    "{{e1}}{{e2}}",
                    TokenFactory.DocumentList(
                        i => TokenFactory.HandlebarsSimple(i, "e1"),
                        i => TokenFactory.HandlebarsSimple(i, "e2"))
                };
                yield return new object[]
                {
                    "{{/if}}",
                    TokenFactory.DocumentList(
                        TokenFactory.IfEndExpression)
                };

                yield return new object[]
                {
                    "{{#if expression}}",
                    TokenFactory.DocumentList(
                        i => TokenFactory.IfStartExpression("expression", i))
                };
                yield return new object[]
                {
                    "<h1 attr=\"{{test}}\">",
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i,
                            a => TokenFactory.AttributeWithContentExtended(a, "attr", 
                                b => TokenFactory.HandlebarsSimple(b, "test"))))
                };
                yield return new object[]
                {
                    "<h1 attr=\"before{{test}}after\">",
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i,
                            a => TokenFactory.AttributeWithContentExtended(a, "attr",
                                b => TokenFactory.AttributeContent("before", b),
                                b => TokenFactory.HandlebarsSimple(b, "test"),
                                b => TokenFactory.AttributeContent("after", b))))
                };
                yield return new object[]
                {
                    "<h1 {{#if true}}attr=\"val\"{{/if}}>",
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i,
                            a => TokenFactory.IfStartExpression("true", a),
                            a => TokenFactory.AttributeWithContent(a, "attr", "val"),
                            TokenFactory.IfEndExpression))
                };
                yield return new object[]
                {
                    "{{{test}}}",
                    TokenFactory.DocumentList(
                        i => TokenFactory.Composite(i,
                            TokenCategory.External,
                            TokenFactory.HandlebarsStart,
                            TokenFactory.HandlebarsStart,
                            a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluateInHtml, 
                                TokenFactory.HandlebarsStart,
                                b => TokenFactory.Expression(b, "test"),
                                TokenFactory.HandlebarsEnd),
                            TokenFactory.HandlebarsEnd,
                            TokenFactory.HandlebarsEnd))
                };
            }
        }
    }
}
