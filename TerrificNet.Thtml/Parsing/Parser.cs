using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class Parser
    {
        private readonly HandlebarsParser _parser;

        public Parser(HandlebarsParser parser)
        {
            _parser = parser;
        }

        public Document Parse(IEnumerable<Token> tokens)
        {
            var enumerator = tokens.GetEnumerator();
            MoveNext(enumerator);

            Expect(enumerator, TokenCategory.StartDocument);

            var nodes = Content(enumerator).ToList();

            Expect(enumerator, TokenCategory.EndDocument);

            var document = new Document(nodes);
            return document;
        }

        private IEnumerable<Node> Content(IEnumerator<Token> enumerator)
        {
            while (true)
            {
                if (enumerator.Current.Category == TokenCategory.Content)
                {
                    Token token = enumerator.Current;
                    yield return new TextNode(token.Lexem);
                    MoveNext(enumerator);
                }
                else if (enumerator.Current.Category == TokenCategory.ElementStart)
                {
                    string tagName;
                    var attributes = GetElementParts(enumerator.Current, out tagName).ToList();

                    MoveNext(enumerator);

                    var nodes = Content(enumerator).ToList();

                    var tEnd = GetNamePart(Expect(enumerator, TokenCategory.ElementEnd), TokenCategory.Name);
                    if (tEnd != tagName)
                        throw new Exception($"Unexpected tag name '{tEnd}'. Expected closing tag for '{tagName}'.");

                    yield return new Element(tagName, nodes, attributes);
                }
                else if (enumerator.Current.Category == TokenCategory.EmptyElement)
                {
                    string tagName;
                    var attributes = GetElementParts(enumerator.Current, out tagName).ToList();

                    MoveNext(enumerator);

                    yield return new Element(tagName, attributes);

                }
                else if (enumerator.Current.Category == TokenCategory.External)
                {
                    var ft = GetExternalToken(enumerator.Current);
                    if (ft.Category == TokenCategory.HandlebarsEvaluate || ft.Category == TokenCategory.HandlebarsEvaluateInHtml)
                    {
                        yield return new Statement(_parser.ParseExpression(ft));
                        MoveNext(enumerator);
                    }
                    else if (ft.Category == TokenCategory.HandlebarsBlockStart)
                    {
                        var name = GetNamePart(ft, TokenCategory.Name);
                        var expression = _parser.ParseExpression(ft);

                        MoveNext(enumerator);

                        var nodes = Content(enumerator).ToList();

                        ExpectEndOfExternal(enumerator, name);

                        MoveNext(enumerator);

                        yield return new Statement(expression, nodes.ToArray());
                    }
                    else
                        break;
                }
                else
                    break;
            }
        }

        private static void MoveNext(IEnumerator<Token> enumerator)
        {
            while (enumerator.MoveNext() && (enumerator.Current.Category == TokenCategory.Comment ||
                   enumerator.Current.Category == TokenCategory.Whitespace))
            {
            }
        }

        private static void ExpectEndOfExternal(IEnumerator<Token> enumerator, string name)
        {
            var tEnd = GetNamePart(GetExternalToken(enumerator.Current), TokenCategory.Name);
            if (tEnd != name)
                throw new Exception(
                    $"Unexpected expression '{tEnd}'. Expected ending epxression for '{name}'.");
        }

        private static Token GetExternalToken(Token token)
        {
            var current = ExpectComposite(token);
            var ft = current.Tokens.OfType<CompositeToken>().FirstOrDefault();
            if (ft == null)
                throw new Exception("The external token requires to have one inner token.");

            return ft;
        }

        private IEnumerable<ElementPart> GetElementParts(Token token, out string name)
        {
            var compositeToken = ExpectComposite(token);
            var tokens = compositeToken.Tokens.GetEnumerator();
            tokens.MoveNext();

            Expect(tokens, TokenCategory.BracketOpen);
            var nameToken = Expect(tokens, TokenCategory.Name);
            name = nameToken.Lexem;

            return GetElementPartsAfterName(tokens).ToList();
        }

        private IEnumerable<ElementPart> GetElementPartsAfterName(IEnumerator<Token> tokens, string expectedEndPart = null)
        {
            while (true)
            {
                Token external;
                if ((external = Can(tokens, TokenCategory.External)) != null)
                {
                    var ft = GetExternalToken(external);
                    if (ft.Category == TokenCategory.HandlebarsEvaluate)
                        yield return new AttributeStatement(_parser.ParseExpression(ft));
                    else if (ft.Category == TokenCategory.HandlebarsBlockStart)
                    {
                        var name = GetNamePart(ft, TokenCategory.Name);
                        var parts = GetElementPartsAfterName(tokens, name).ToList();

                        yield return
                            new AttributeStatement(_parser.ParseExpression(ft),
                                parts.OfType<AttributeNode>().ToArray());
                    }
                    else if (ft.Category == TokenCategory.HandlebarsBlockEnd && string.IsNullOrEmpty(expectedEndPart))
                    {
                        ExpectEndOfExternal(tokens, expectedEndPart);
                        break;
                    }
                }

                Token attributeToken;
                if ((attributeToken = Can(tokens, TokenCategory.Attribute)) != null)
                {
                    yield return GetAttribute(attributeToken);
                    continue;
                }

                break;
            }
        }

        private AttributeNode GetAttribute(Token token)
        {
            var compositeToken = ExpectComposite(token);
            var name = GetNameToken(compositeToken, TokenCategory.Name);

            var values = compositeToken.Tokens
                .SkipWhile(f => f.Category != TokenCategory.Quote)
                .Skip(1)
                .TakeWhile(f => f.Category != TokenCategory.Quote)
                .Select(GetAttributeContent)
                .ToArray();

            AttributeContent content = null;
            if (values.Length > 1)
                content = new CompositeAttributeContent(values);
            else if (values.Length == 1)
                content = values[0];

            return new AttributeNode(name.Lexem, content);
        }

        private AttributeContent GetAttributeContent(Token token)
        {
            if (token.Category == TokenCategory.AttributeContent)
                return new ConstantAttributeContent(token.Lexem);

            if (token.Category == TokenCategory.External)
                return new AttributeContentStatement(_parser.ParseExpression(GetExternalToken(token)));

            throw new Exception($"Unexpected token {token.Category} at position {token.Start}");
        }

        private static string GetNamePart(Token token, TokenCategory tokenCategory)
        {
            var compositeToken = ExpectComposite(token);

            var tagNameToken = GetNameToken(compositeToken, tokenCategory);

            return tagNameToken.Lexem;
        }

        private static Token GetNameToken(CompositeToken compositeToken, TokenCategory tokenCategory)
        {
            var tagNameToken = compositeToken.Tokens?.FirstOrDefault(t => t.Category == tokenCategory);
            if (tagNameToken == null)
                throw new Exception($"Invalid composite token. Expected to have a {tokenCategory} token inside.");

            return tagNameToken;
        }

        private static CompositeToken ExpectComposite(Token token)
        {
            var compositeToken = token as CompositeToken;
            if (compositeToken == null)
                throw new Exception($"Expected token {token.Category} to be a composite token.");

            return compositeToken;
        }

        private static Token Can(IEnumerator<Token> tokens, TokenCategory category)
        {
            var current = tokens.Current;
            if (current.Category != category)
                return null;

            MoveNext(tokens);

            return current;
        }

        private static Token Expect(IEnumerator<Token> tokens, TokenCategory category)
        {
            var current = tokens.Current;
            if (current.Category != category)
                throw new Exception($"Expected token {category} at position {current.Start}");

            MoveNext(tokens);

            return current;
        }
    }
}
