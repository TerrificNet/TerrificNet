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
            lexer.Handlebar();

            var tokens = state.Tokens;
            var enumerator = tokens.GetEnumerator();
            if (!enumerator.MoveNext() || enumerator.Current == null)
                return null;

            return Parse(enumerator.Current);
        }

        public EvaluateExpression Parse(Token token)
        {
            if (token.Category != TokenCategory.HandlebarsBlockStart && token.Category != TokenCategory.HandlebarsBlockEnd && token.Category != TokenCategory.HandlebarsEvaluate)
                throw new Exception(
                    $"Unexpected token {token.Category} at postion {token.Start}.");

            return Access(token);
        }

        private static EvaluateExpression Access(Token token)
        {
            if (token.Category == TokenCategory.HandlebarsEvaluate)
            {
                return new EvaluateExpression(GetMemberAccess(token));
            }
            if (token.Category == TokenCategory.HandlebarsBlockStart)
            {
                var compToken = ExpectComposite(token);
                var nameToken = ExpectTokenCategory(compToken, TokenCategory.Name);

                return new EvaluateExpression(new ConditionalExpression(GetMemberAccess(token)));
            }
            throw new ArgumentException("Unknown Token type.");
        }

        private static MemberAccessExpression GetMemberAccess(Token token)
        {
            var compToken = ExpectComposite(token);
            var expressionToken = ExpectTokenCategory(compToken, TokenCategory.HandlebarsExpression);

            var memberAccess = Expression(expressionToken);
            return memberAccess;
        }

        private static MemberAccessExpression Expression(Token current)
        {
            var compToken = ExpectComposite(current);
            var nameToken = ExpectTokenCategory(compToken, TokenCategory.Name);

            var subExpression = compToken.Tokens.FirstOrDefault(t => t.Category == TokenCategory.HandlebarsExpression);
            if (subExpression != null)
                return new MemberAccessExpression(nameToken.Lexem, Expression(subExpression));

            return new MemberAccessExpression(nameToken.Lexem);
        }

        private static Token ExpectTokenCategory(CompositeToken compToken, TokenCategory tokenCategory)
        {
            var nameToken = compToken.Tokens.SingleOrDefault(t => t.Category == tokenCategory);
            if (nameToken == null)
                throw new Exception($"The token with category {compToken.Category} expected to have exactly one {tokenCategory} token.");

            return nameToken;
        }

        private static CompositeToken ExpectComposite(Token current)
        {
            var compToken = current as CompositeToken;
            if (compToken == null)
                throw new Exception($"The token with category {current.Category} expected to be a CompositeToken");
            return compToken;
        }
    }
}
