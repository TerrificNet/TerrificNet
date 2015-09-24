namespace TerrificNet.Thtml.LexicalAnalysis
{
    internal class HandlebarsGrammar
    {
        private readonly LexerState _lexerState;
        private readonly CommonGrammar _commonGrammar;

        public HandlebarsGrammar(LexerState lexerState)
        {
            _lexerState = lexerState;
            _commonGrammar = new CommonGrammar(lexerState);
        }

        internal void Expression()
        {
            if (!_lexerState.Can(() => _commonGrammar.Name(), TokenCategory.Name))
                return;

            _lexerState.Composite(() =>
            {
                _lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);
                if (_lexerState.Can('.', TokenCategory.Dot))
                {
                    _lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);
                    _lexerState.Must(Expression, TokenCategory.HandlebarsExpression);
                }
                return TokenCategory.HandlebarsExpression;
            });
        }

        internal void Handlebar()
        {
            _lexerState.Composite(() =>
            {
                _lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);
                var category = HandlebarContent();
                _lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);

                return category;
            }, 0);
        }

        private TokenCategory HandlebarContent()
        {
            if (_lexerState.Can('#', TokenCategory.Hash))
            {
                var token = _lexerState.Must(() => _commonGrammar.Name(), TokenCategory.Name);
                if (token.Lexem == "if")
                {
                    _lexerState.Must(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);
                    _lexerState.Must(Expression, TokenCategory.HandlebarsExpression);
                }
                else
                {
                    if (_lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace))
                    {
                        _lexerState.Can(Expression, TokenCategory.HandlebarsExpression);
                    }
                }
                return TokenCategory.HandlebarsBlockStart;
            }
            if (_lexerState.Can('/', TokenCategory.Slash))
            {
                _lexerState.Must(() => _commonGrammar.Name(), TokenCategory.Name);
                return TokenCategory.HandlebarsBlockEnd;
            }

            _lexerState.Must(Expression, TokenCategory.HandlebarsExpression);
            return TokenCategory.HandlebarsEvaluate;
        }
    }
}
