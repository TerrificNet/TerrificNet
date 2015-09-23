using System;
using System.Linq;
using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class HandlebarsParser
    {
        public EvaluateExpression Parse(string test)
        {
            var state = new LexerState(test);
            var lexer = new HandlebarsGrammar(state);
            lexer.HandlebarContent();

            var tokens = state.Tokens;
            var enumerator = tokens.GetEnumerator();
            enumerator.MoveNext();

            var current = enumerator.Current;
            if (!(current.Category == TokenCategory.HandlebarsExpression))
                throw new Exception(
                    $"Unexpected token {current.Category} at postion {current.Start}.");

            return new EvaluateExpression(Expression(current));
        }

        private static MemberAccessExpression Expression(Token current)
        {
            var compToken = current as CompositeToken;
            if (compToken == null)
                throw new Exception($"The token with category {current.Category} expected to be a CompositeToken");

            var nameToken = compToken.Tokens.SingleOrDefault(t => t.Category == TokenCategory.Name);
            if (nameToken == null)
                throw new Exception($"The token with category {current.Category} expected to have exactly one Name token.");

            var subExpression = compToken.Tokens.FirstOrDefault(t => t.Category == TokenCategory.HandlebarsExpression);
            if (subExpression != null)
                return new MemberAccessExpression(nameToken.Lexem, Expression(subExpression));

            return new MemberAccessExpression(nameToken.Lexem);
        }
    }
}
