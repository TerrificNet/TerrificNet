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

                    yield return new Element(tagName, attributes, nodes);
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

                        ExpectEndOfExternal(GetExternalToken(enumerator.Current), name);

                        MoveNext(enumerator);

                        yield return new Statement(expression, nodes.ToArray());
                    }
                    else if (ft.Category == TokenCategory.Comment)
                    {
                        MoveNext(enumerator);
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

        private static void ExpectEndOfExternal(Token token, string name)
        {
            var tEnd = GetNamePart(token, TokenCategory.Name);
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
            return Move<ElementPart>(tokens, expectedEndPart, 
                (expression, children) => new AttributeStatement(expression, children.OfType<AttributeNode>().ToArray()), 
                GetAttribute, 
                TokenCategory.Attribute);
        }

        private IEnumerable<T> Move<T>(IEnumerator<Token> tokens, string expectedEndPart, Func<MustacheExpression, IEnumerable<T>, T> factory, Func<Token, T> childFunction, TokenCategory childTokenCategory)
        {
            while (true)
            {
                Token external;
                if ((external = Can(tokens, TokenCategory.External)) != null)
                {
                    var ft = GetExternalToken(external);
                    if (ft.Category == TokenCategory.HandlebarsEvaluate)
                        yield return factory(_parser.ParseExpression(ft), null);
                    else if (ft.Category == TokenCategory.HandlebarsBlockStart)
                    {
                        var name = GetNamePart(ft, TokenCategory.Name);
                        var parts = Move(tokens, name, factory, childFunction, childTokenCategory).ToList();

                        yield return factory(_parser.ParseExpression(ft), parts);
                    }
                    else if (ft.Category == TokenCategory.HandlebarsBlockEnd && !string.IsNullOrEmpty(expectedEndPart))
                    {
                        ExpectEndOfExternal(ft, expectedEndPart);
                        break;
                    }
                }

                Token attributeToken;
                if ((attributeToken = Can(tokens, childTokenCategory)) != null)
                {
                    yield return childFunction(attributeToken);
                    continue;
                }

                break;
            }
        }

        private AttributeNode GetAttribute(Token token)
        {
            var compositeToken = ExpectComposite(token);
            var enumerator = compositeToken.Tokens.GetEnumerator();
            enumerator.MoveNext();

            var name = Expect(enumerator, TokenCategory.Name);
            
            MoveNext(enumerator);

            AttributeContent content = null;
            if (Can(enumerator, TokenCategory.Quote) != null)
            {
                var values = Move(enumerator, null,
                    (expression, children) => new AttributeContentStatement(expression, children?.ToArray()),
                    GetAttributeContent,
                    TokenCategory.AttributeContent).ToArray();

                Expect(enumerator, TokenCategory.Quote);

                if (values.Length > 1)
                    content = new CompositeAttributeContent(values);
                else if (values.Length == 1)
                    content = values[0];
            }

            return new AttributeNode(name.Lexem, content);
        }

        private AttributeContent GetAttributeContent(Token token)
        {
            if (token.Category == TokenCategory.AttributeContent)
                return new ConstantAttributeContent(token.Lexem);

            if (token.Category == TokenCategory.External)
            {
                return new AttributeContentStatement(_parser.ParseExpression(GetExternalToken(token)));
            }

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
            if (current == null || current.Category != category)
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
