using System.Collections.Generic;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Test
{
    public class ParserTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestParser(IEnumerable<Token> tokens, Node expectedNode)
        {
            var parser = new Parser(new HandlebarsParser());
            var result = parser.Parse(tokens);

            Assert.NotNull(result);
            NodeAsserts.AssertNode(expectedNode, result);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[]
                {
                    TokenFactory.DocumentList(),
                    new Document()
                };

                yield return new object[]
                {
                    TokenFactory.DocumentList(i => TokenFactory.Content("test", i)),
                    new Document(new TextNode("test"))
                };

                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.ElementEnd("h1", i)),
                    new Document(new Element("h1"))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.Content("test", i),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new Document(new Element("h1", new TextNode("test")))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.ElementStart("h2", i),
                        i => TokenFactory.Content("test", i),
                        i => TokenFactory.ElementEnd("h2", i),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new Document(
                        new Element("h1",
                            new Element("h2",
                                new TextNode("test"))))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.Content("inner", i),
                        i => TokenFactory.ElementStart("h2", i),
                        i => TokenFactory.Content("test", i),
                        i => TokenFactory.ElementEnd("h2", i),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new Document(
                        new Element("h1",
                            new TextNode("inner"),
                            new Element("h2",
                                new TextNode("test"))))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i,
                            TokenFactory.Whitespace,
                            a => TokenFactory.AttributeWithContent(a, "test", "val")),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new Document(
                        new Element("h1", new List<AttributeNode>
                        {
                            new AttributeNode("test", "val")
                        }))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i,
                            TokenFactory.Whitespace,
                            a => TokenFactory.AttributeWithContent(a, "test", "val"),
                            TokenFactory.Whitespace,
                            a => TokenFactory.AttributeWithoutContent(a, "test2")),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new Document(
                        new Element("h1", new List<AttributeNode>
                        {
                            new AttributeNode("test", "val"),
                            new AttributeNode("test2", (ConstantAttributeContent)null)
                        }))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.EmptyElement("h1", i)),

                    new Document(
                        new Element("h1"))
                };

                // Handlebars

                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.HandlebarsSimple(i, "name"),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new Document(
                        new Element("h1", new Statement(new MemberExpression("name"))))
                };

                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.HandlebarsBlockStart(i, "if", a => TokenFactory.Expression(a, "test")),
                        i => TokenFactory.EmptyElement("br", i),
                        i => TokenFactory.HandlebarsSimple(i, "name"),
                        i => TokenFactory.HandlebarsBlockEnd(i, "if")),

                    new Document(
                        new Statement(
                            new ConditionalExpression(new MemberExpression("test")),
                            new Element("br"),
                            new Statement(new MemberExpression("name"))))
                };

                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i,
                            a => TokenFactory.AttributeWithContentExtended(a, "attr",
                                b => TokenFactory.AttributeContent("before", b),
                                b => TokenFactory.HandlebarsSimple(b, "test"),
                                b => TokenFactory.AttributeContent("after", b))),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new Document(
                        new Element("h1", new []
                        {
                           new AttributeNode("attr",
                           new CompositeAttributeContent(
                               new ConstantAttributeContent("before"),
                               new AttributeContentStatement(new MemberExpression("test")),
                               new ConstantAttributeContent("after")))
                        }))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i,
                            a => TokenFactory.IfStartExpression("true", a),
                            a => TokenFactory.AttributeWithContent(a, "attr", "val"),
                            TokenFactory.IfEndExpression),
                        i => TokenFactory.ElementEnd("h1", i)),
                    new Document(
                        new Element("h1", new []
                        {
                            new AttributeStatement(new ConditionalExpression(new MemberExpression("true")),
                                new AttributeNode("attr", new ConstantAttributeContent("val")))
                        }))
                };

            }
        }
    }
}
