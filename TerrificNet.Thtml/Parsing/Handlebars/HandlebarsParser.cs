using System;
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

            if (!(enumerator.Current.Category == TokenCategory.HandlebarsExpression))
                throw new Exception(
                    $"Unexpected token {enumerator.Current.Category} at postion {enumerator.Current.Start}.");

            return new EvaluateExpression(new MemberAccessExpression(enumerator.Current.Lexem));
        }
    }
}
