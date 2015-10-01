using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class HandlebarsParser
    {
        public Expression Parse(string test)
        {
            var state = new LexerState(test);
            var lexer = new HandlebarsGrammar(state);
            lexer.Handlebar();

            var current = state.Tokens.FirstOrDefault();
            if (current == null)
                return null;

            return ParseExpression(current);
        }

        public Expression ParseExpression(Token token)
        {
            if (token.Category == TokenCategory.HandlebarsEvaluate)
            {
                return GetExpression(token);
            }
            if (token.Category == TokenCategory.HandlebarsEvaluateInHtml)
            {
                return new UnconvertedExpression(GetExpression(token));
            }
            if (token.Category == TokenCategory.HandlebarsBlockStart)
            {
                var compToken = ExpectComposite(token);
                var nameToken = ExpectTokenCategory(compToken, TokenCategory.Name);

                if (nameToken.Lexem == "if")
                    return new ConditionalExpression(GetExpression(token));

                if (nameToken.Lexem == "each")
                    return new IterationExpression(GetExpression(token));

                var attributes = GetHelperAttributes(token).ToArray();
                return new CallHelperExpression(nameToken.Lexem, attributes);
            }
            throw new ArgumentException("Unknown Token type.");
        }

        private static IEnumerable<HelperAttribute> GetHelperAttributes(Token token)
        {
            var compToken = ExpectComposite(token);

            return compToken.Tokens
                .Where(t => t.Category == TokenCategory.HandlebarsAttribute)
                .Select(GetAttribute);
        }

        private static HelperAttribute GetAttribute(Token token)
        {
            var compToken = ExpectComposite(token);

            var name = ExpectTokenCategory(compToken, TokenCategory.Name);
            var value = ExpectTokenCategory(compToken, TokenCategory.AttributeContent);

            return new HelperAttribute(name.Lexem, value.Lexem);
        }

        private static Expression GetExpression(Token token)
        {
            var compToken = ExpectComposite(token);
            var expressionToken = ExpectTokenCategory(compToken, TokenCategory.HandlebarsExpression);

            var memberAccess = Expression(expressionToken);
            return memberAccess;
        }

        private static Expression Expression(Token current)
        {
            var compToken = ExpectComposite(current);
            var nameToken = ExpectTokenCategory(compToken, TokenCategory.Name);

            var subExpression = compToken.Tokens.FirstOrDefault(t => t.Category == TokenCategory.HandlebarsExpression);
            if (subExpression != null)
                return new MemberExpression(nameToken.Lexem, (MemberExpression)Expression(subExpression));

            var attributes = GetHelperAttributes(compToken).ToArray();
            if (attributes.Length > 0)
                return new CallHelperExpression(nameToken.Lexem, attributes);

            return new MemberExpression(nameToken.Lexem);
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
