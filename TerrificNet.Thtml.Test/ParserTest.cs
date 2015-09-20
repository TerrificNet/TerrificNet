using System.Collections.Generic;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class ParserTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestParser(IEnumerable<Token> tokens, CreateNode expectedNode)
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
                    new CreateDocument()
                };

                yield return new object[]
                {
                    TokenFactory.DocumentList(i => TokenFactory.Content("test", i)),
                    new CreateDocument(new CreateTextNode("test"))
                };

                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.ElementEnd("h1", i)),
                    new CreateDocument(new CreateElement("h1"))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.Content("test", i),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new CreateDocument(new CreateElement("h1", new CreateTextNode("test")))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.ElementStart("h2", i),
                        i => TokenFactory.Content("test", i),
                        i => TokenFactory.ElementEnd("h2", i),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new CreateDocument(
                        new CreateElement("h1",
                            new CreateElement("h2",
                                new CreateTextNode("test"))))
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

                    new CreateDocument(
                        new CreateElement("h1",
                            new CreateTextNode("inner"),
                            new CreateElement("h2",
                                new CreateTextNode("test"))))
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i,
                            TokenFactory.Whitespace,
                            a => TokenFactory.AttributeWithContent(a, "test", "val")),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new CreateDocument(
                        new CreateElement("h1") { Attributes = new List<CreateAttribute>
                        {
                            new CreateAttribute("test", "val")
                        }})
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

                    new CreateDocument(
                        new CreateElement("h1") { Attributes = new List<CreateAttribute>
                        {
                            new CreateAttribute("test", "val"),
                            new CreateAttribute("test2", null)
                        }})
                };
                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.EmptyElement("h1", i)),

                    new CreateDocument(
                        new CreateElement("h1"))
                };

                // Handlebars

                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.ElementStart("h1", i),
                        i => TokenFactory.HandlebarsSimple(i, "name"),
                        i => TokenFactory.ElementEnd("h1", i)),

                    new CreateDocument(
                        new CreateElement("h1", new DynamicCreateSingleNode("name")))
                };

                yield return new object[]
                {
                    TokenFactory.DocumentList(
                        i => TokenFactory.HandlebarsBlockStart(i, "if"),
                        i => TokenFactory.EmptyElement("br", i),
                        i => TokenFactory.HandlebarsSimple(i, "name"),
                        i => TokenFactory.HandlebarsBlockEnd(i, "if")),

                    new CreateDocument(
                        new DynamicCreateBlockNode(
                            "if",
                            new CreateElement("br"), 
                            new DynamicCreateSingleNode("name")))
                };

            }
        }
    }
}
