using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml
{
    public class Parser
    {
        public CreateNode Parse(IEnumerable<Token> tokens)
        {
            var enumerator = tokens.GetEnumerator();
            enumerator.MoveNext();

            Expect(enumerator, TokenCategory.StartDocument);

            var nodes = Content(enumerator).ToList();

            Expect(enumerator, TokenCategory.EndDocument);

            var document = new CreateDocument(nodes);
            return document;
        }

        private IEnumerable<CreateNode> Content(IEnumerator<Token> enumerator)
        {
            while (true)
            {
                if (enumerator.Current.Category == TokenCategory.Content)
                {
                    yield return new CreateTextNode(enumerator.Current);
                    enumerator.MoveNext();
                }
                else if (enumerator.Current.Category == TokenCategory.ElementStart)
                {
                    var tagName = GetNamePart(enumerator.Current);
                    var attributes = GetAttributes(enumerator.Current).ToList();

                    enumerator.MoveNext();

                    var nodes = Content(enumerator).ToList();

                    var tEnd = GetNamePart(Expect(enumerator, TokenCategory.ElementEnd));
                    if (tEnd != tagName)
                        throw new Exception($"Unexpected tag name '{tEnd}'. Expected closing tag for '{tagName}'.");

                    yield return new CreateElement(tagName, nodes) { Attributes = attributes };
                }
                else if (enumerator.Current.Category == TokenCategory.HandlebarsEvaluate)
                {
                    var name = GetNamePart(enumerator.Current);

                    yield return new DynamicCreateNode(name);
                    enumerator.MoveNext();
                }
                else
                    break;
            }
        }

        private IEnumerable<CreateAttribute> GetAttributes(Token token)
        {
            var compositeToken = ExpectComposite(token);
            return compositeToken.Tokens
                .Where(t => t.Category == TokenCategory.Attribute)
                .Select(GetAttribute);
        }

        private CreateAttribute GetAttribute(Token token)
        {
            var compositeToken = ExpectComposite(token);
            var name = GetNameToken(compositeToken);
            var value = compositeToken.Tokens.FirstOrDefault(t => t.Category == TokenCategory.AttributeContent);

            return new CreateAttribute(name.Lexem, value?.Lexem);
        }

        private static string GetNamePart(Token token)
        {
            var compositeToken = ExpectComposite(token);

            var tagNameToken = GetNameToken(compositeToken);

            return tagNameToken.Lexem;
        }

        private static Token GetNameToken(CompositeToken compositeToken)
        {
            var tagNameToken = compositeToken.Tokens.FirstOrDefault(t => t.Category == TokenCategory.Name);
            if (tagNameToken == null)
                throw new Exception("Invalid composite token. Expected to have a name token inside.");

            return tagNameToken;
        }

        private static CompositeToken ExpectComposite(Token token)
        {
            var compositeToken = token as CompositeToken;
            if (compositeToken == null)
                throw new Exception($"Expected token {token} to be a composite token.");

            return compositeToken;
        }

        private Token Expect(IEnumerator<Token> tokens, TokenCategory category)
        {
            var current = tokens.Current;
            if (current.Category != category)
                throw new Exception($"Expected token {category} at position {current.Start}");

            tokens.MoveNext();

            return current;
        }
    }
}
