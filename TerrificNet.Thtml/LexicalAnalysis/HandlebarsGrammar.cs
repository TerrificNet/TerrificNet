using System.Linq;

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
                else if (_lexerState.Can(Attribute, TokenCategory.HandlebarsAttribute))
                {
                    AttributeList();
                }
                return TokenCategory.HandlebarsExpression;
            });
        }

        internal void Handlebar()
        {
            _lexerState.Composite(() =>
            {
                if (_lexerState.Can('!', TokenCategory.CommentStart))
                {
                    _lexerState.Must('-', TokenCategory.Dash);
                    _lexerState.Must('-', TokenCategory.Dash);

                    _lexerState.MoveUntil((c1, c2) => (c1 == '-' && c2 != '-') || c1 != '-',
                        TokenCategory.CommentContent);

                    _lexerState.Must('-', TokenCategory.Dash);
                    _lexerState.Must('-', TokenCategory.Dash);

                    return TokenCategory.Comment;
                }

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
                if (token.Lexem == "if" || token.Lexem == "each")
                {
                    _lexerState.Must(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);
                    _lexerState.Must(Expression, TokenCategory.HandlebarsExpression);
                }
                else
                {
                    AttributeList();
                }
                return TokenCategory.HandlebarsBlockStart;
            }
            if (_lexerState.Can('/', TokenCategory.Slash))
            {
                _lexerState.Must(() => _commonGrammar.Name(), TokenCategory.Name);
                return TokenCategory.HandlebarsBlockEnd;
            }
            if (_lexerState.Can('{', TokenCategory.HandlebarsStart))
            {
                _lexerState.Must(Expression, TokenCategory.HandlebarsExpression);
                _lexerState.Must('}', TokenCategory.HandlebarsEnd);
                return TokenCategory.HandlebarsEvaluateInHtml;
            }

            _lexerState.Must(Expression, TokenCategory.HandlebarsExpression);
            return TokenCategory.HandlebarsEvaluate;
        }

        private void AttributeList()
        {
            while (true)
            {
                if (_lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace))
                {
                    if (_lexerState.Can(Attribute, TokenCategory.HandlebarsAttribute))
                        continue;
                }
                break;
            }
        }

        private void Attribute()
        {
            if (_lexerState.Can(() => _commonGrammar.Name(), TokenCategory.Name))
            {
                _lexerState.Composite(() =>
                {
                    _lexerState.Can(() => _commonGrammar.Whitespace());
                    _lexerState.Must('=', TokenCategory.Equality);
                    _lexerState.Can(() => _commonGrammar.Whitespace());

                    if (_lexerState.Can('"', TokenCategory.Quote))
                    {
                        _lexerState.MoveUntil(c => c != '"', TokenCategory.AttributeContent);
                        _lexerState.Must('"', TokenCategory.Quote);
                    }
                    else
                    {
                        _lexerState.MoveUntil(
                            (c1, c2) =>
                                !CharacterClasses.WhitespaceCharacters.Contains(c1) &&
                                (c1 != '}' || c2 != '}'), TokenCategory.AttributeContent);
                    }

                    return TokenCategory.HandlebarsAttribute;
                });
            }
        }
    }
}
