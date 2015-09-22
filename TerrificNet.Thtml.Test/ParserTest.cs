﻿using System.Collections.Generic;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class ParserTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestParser(IEnumerable<Token> tokens, Node expectedNode)
        {
            var parser = new Parser();
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
                        new Element("h1", new List<Attribute>
                        {
                            new Attribute("test", "val")
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
                        new Element("h1", new List<Attribute>
                        {
                            new Attribute("test", "val"),
                            new Attribute("test2", null)
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
                        new Element("h1", new EvaluateExpressionNode("name")))
                };

                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.HandlebarsBlockStart(i, "if"),
                        i => TokenFactory.EmptyElement("br", i),
                        i => TokenFactory.HandlebarsSimple(i, "name"),
                        i => TokenFactory.HandlebarsBlockEnd(i, "if")),

                    new Document(
                        new EvaluateBlockNode(
                            "if",
                            new Element("br"), 
                            new EvaluateExpressionNode("name")))
                };

            }
        }
    }
}