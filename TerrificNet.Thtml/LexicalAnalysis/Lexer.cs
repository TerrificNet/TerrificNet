using System.Collections.Generic;

namespace TerrificNet.Thtml.LexicalAnalysis
{
    public class Lexer
    {
        public IEnumerable<Token> Tokenize(string input)
        {
            var state = new LexerState(input);
            var grammar = new HtmlGrammar(state);
            state.Put(new Token(TokenCategory.StartDocument, 0, 0));
            grammar.Document();

            state.Put(new Token(TokenCategory.EndDocument, input.Length, input.Length));

            return state.Tokens;
        }
    }
}