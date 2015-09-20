using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml
{
    public class Parser
    {
        public HtmlNode Parse(IEnumerable<Token> tokens)
        {
            var enumerator = tokens.GetEnumerator();
            enumerator.MoveNext();

            Expect(enumerator, TokenCategory.StartDocument);

            var nodes = Content(enumerator).ToList();

            Expect(enumerator, TokenCategory.EndDocument);

            var document = new HtmlDocument(nodes);
            return document;
        }

        private IEnumerable<HtmlNode> Content(IEnumerator<Token> enumerator)
        {
            if (enumerator.Current.Category == TokenCategory.Content)
            {
                yield return new HtmlTextNode(enumerator.Current);
                enumerator.MoveNext();
            }
            else if (enumerator.Current.Category == TokenCategory.ElementStart)
            {
                var tagName = GetTagName(enumerator.Current);
                var attributes = GetAttributes(enumerator.Current);

                enumerator.MoveNext();

                var nodes = Content(enumerator).ToList();

                var tEnd = GetTagName(Expect(enumerator, TokenCategory.ElementEnd));
                if (tEnd != tagName)
                    throw new Exception($"Unexpected tag name '{tEnd}'. Expected closing tag for '{tagName}'.");

                yield return new HtmlElement(tagName, nodes);
            }
        }

        private IEnumerable<HtmlAttribute> GetAttributes(Token current)
        {

            yield return new HtmlAttribute();
        }

        private static string GetTagName(Token token)
        {
            var compositeToken = token as CompositeToken;
            if (compositeToken == null)
                throw new Exception("Only possible to get the tagname from a composition token.");

            var tagNameToken = compositeToken.Tokens.FirstOrDefault(t => t.Category == TokenCategory.Name);
            if (tagNameToken == null)
                throw new Exception("Invalid composite token. Expected to have a name token inside.");

            return tagNameToken.Lexem;
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

    internal class HtmlAttribute
    {
    }
}
