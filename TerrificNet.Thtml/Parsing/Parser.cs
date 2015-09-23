﻿using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml.Parsing
{
    public class Parser
    {
        public Node Parse(IEnumerable<Token> tokens)
        {
            var enumerator = tokens.GetEnumerator();
            enumerator.MoveNext();

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
                    enumerator.MoveNext();
                }
                else if (enumerator.Current.Category == TokenCategory.ElementStart)
                {
                    var tagName = GetNamePart(enumerator.Current, TokenCategory.Name);
                    var attributes = GetAttributes(enumerator.Current).ToList();

                    enumerator.MoveNext();

                    var nodes = Content(enumerator).ToList();

                    var tEnd = GetNamePart(Expect(enumerator, TokenCategory.ElementEnd), TokenCategory.Name);
                    if (tEnd != tagName)
                        throw new Exception($"Unexpected tag name '{tEnd}'. Expected closing tag for '{tagName}'.");

                    yield return new Element(tagName, nodes, attributes);
                }
                else if (enumerator.Current.Category == TokenCategory.EmptyElement)
                {
                    var tagName = GetNamePart(enumerator.Current, TokenCategory.Name);
                    var attributes = GetAttributes(enumerator.Current).ToList();

                    enumerator.MoveNext();

                    yield return new Element(tagName, attributes);

                }
                else if (enumerator.Current.Category == TokenCategory.HandlebarsEvaluate)
                {
                    var name = GetNamePart(enumerator.Current, TokenCategory.HandlebarsExpression);

                    yield return new EvaluateExpressionNode(name);
                    enumerator.MoveNext();
                }
                else if (enumerator.Current.Category == TokenCategory.HandlebarsBlockStart)
                {
                    var name = GetNamePart(enumerator.Current, TokenCategory.Name);
                    enumerator.MoveNext();

                    var nodes = Content(enumerator).ToList();

                    var tEnd = GetNamePart(Expect(enumerator, TokenCategory.HandlebarsBlockEnd), TokenCategory.Name);
                    if (tEnd != name)
                        throw new Exception($"Unexpected expression '{tEnd}'. Expected ending epxression for '{name}'.");

                    yield return new EvaluateBlockNode(name, nodes.ToArray());
                }
                else
                    break;
            }
        }

        private static IEnumerable<AttributeNode> GetAttributes(Token token)
        {
            var compositeToken = ExpectComposite(token);
            return compositeToken.Tokens
                .Where(t => t.Category == TokenCategory.Attribute)
                .Select(GetAttribute);
        }

        private static AttributeNode GetAttribute(Token token)
        {
            var compositeToken = ExpectComposite(token);
            var name = GetNameToken(compositeToken, TokenCategory.Name);
            var value = compositeToken.Tokens.FirstOrDefault(t => t.Category == TokenCategory.AttributeContent);

            return new AttributeNode(name.Lexem, value?.Lexem);
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
                throw new Exception($"Expected token {token} to be a composite token.");

            return compositeToken;
        }

        private static Token Expect(IEnumerator<Token> tokens, TokenCategory category)
        {
            var current = tokens.Current;
            if (current.Category != category)
                throw new Exception($"Expected token {category} at position {current.Start}");

            tokens.MoveNext();

            return current;
        }
    }
}
