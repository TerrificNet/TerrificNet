using System;
using System.Collections.Generic;
using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml
{
    public class Parser
    {
        public HtmlNode Parse(IEnumerable<Token> tokens)
        {
            var enumerator = tokens.GetEnumerator();
            enumerator.MoveNext();

            var nodes = new List<HtmlNode>();

            Expect(enumerator, TokenCategory.StartDocument);

            if (enumerator.Current.Category == TokenCategory.Content)
            {
                nodes.Add(new HtmlTextNode(enumerator.Current));
                enumerator.MoveNext();
            }

            Expect(enumerator, TokenCategory.EndDocument);

            var document = new HtmlDocument(nodes);
            return document;
        }

        private void Expect(IEnumerator<Token> tokens, TokenCategory category)
        {
            if (tokens.Current.Category != category)
                throw new Exception($"Expected token {category} at position {tokens.Current.Start}");

            tokens.MoveNext();
        }
    }
}
