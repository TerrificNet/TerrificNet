using System.Linq;

namespace TerrificNet.Thtml.LexicalAnalysis
{
    internal class HtmlGrammar
    {
        private readonly LexerState _lexerState;

        public HtmlGrammar(LexerState lexerState)
        {
            _lexerState = lexerState;
        }

        public void Document()
        {
            _lexerState.Can(Whitespace, TokenCategory.Whitespace);
            ElementList();
            _lexerState.Can(Whitespace, TokenCategory.Whitespace);

            if (!_lexerState.Eof())
                _lexerState.Problem("Expected end of document");
        }

        private void ElementList()
        {
            while (true)
            {
                if (_lexerState.Can(Element, TokenCategory.ElementStart, TokenCategory.ElementEnd))
                {
                    continue;
                }

                _lexerState.MoveUntil(CharacterClasses.IsCharData, TokenCategory.Content);
                if (_lexerState.Can(Handlebars, TokenCategory.HandlebarsEvaluate))
                    continue;

                if (_lexerState.Can(Element, TokenCategory.ElementStart, TokenCategory.ElementEnd))
                {
                    continue;
                }

                break;
            }
        }

        private void Handlebars()
        {
            if (!_lexerState.Can('{', TokenCategory.HandlebarsStart))
                return;

            if (!_lexerState.Can('{', TokenCategory.HandlebarsStart))
                return;

            _lexerState.Composite(() =>
            {
                _lexerState.Can(Whitespace, TokenCategory.Whitespace);
                _lexerState.Must(Name, TokenCategory.Name);
                _lexerState.Can(Whitespace, TokenCategory.Whitespace);
                _lexerState.Must('}', TokenCategory.HandlebarsEnd);
                _lexerState.Must('}', TokenCategory.HandlebarsEnd);

                return TokenCategory.HandlebarsEvaluate;
            }, 2);
        }

        private void Whitespace()
        {
            _lexerState.MoveUntil(CharacterClasses.WhitespaceCharacters.Contains, TokenCategory.Whitespace);
        }

        private void Element()
        {
            if (!_lexerState.Can('<', TokenCategory.BracketOpen))
                return;

            _lexerState.Composite(() =>
            {
                var tokenCategory = TokenCategory.ElementStart;
                if (!ElementEnd(ref tokenCategory))
                {
                    tokenCategory = ElementStart();
                }

                _lexerState.Must('>', TokenCategory.BracketClose);

                return tokenCategory;
            });
        }

        private bool ElementEnd(ref TokenCategory tokenCategory)
        {
            if (!_lexerState.Can('/', TokenCategory.Slash))
                return false;

            _lexerState.Must(Name, TokenCategory.Name);
            _lexerState.Can(Whitespace, TokenCategory.Whitespace);

            tokenCategory = TokenCategory.ElementEnd;
            return true;
        }

        private TokenCategory ElementStart()
        {
            _lexerState.Must(Name, TokenCategory.Name);
            AttributeList();

            if (_lexerState.Can('/', TokenCategory.Slash))
                return TokenCategory.EmptyElement;

            return TokenCategory.ElementStart;
        }

        private void AttributeList()
        {
            while (true)
            {
                if (_lexerState.Can(Whitespace, TokenCategory.Whitespace))
                {
                    if (_lexerState.Can(Attribute, TokenCategory.Attribute))
                    {
                        continue;
                    }
                }
                break;
            }
        }

        private void Attribute()
        {
            if (!_lexerState.Can(Name, TokenCategory.Name))
                return;

            _lexerState.Composite(() =>
            {
                _lexerState.Can(Whitespace, TokenCategory.Whitespace);

                if (!_lexerState.Can('=', TokenCategory.Equality))
                    return TokenCategory.Attribute;

                _lexerState.Can(Whitespace, TokenCategory.Whitespace);
                _lexerState.Must('"', TokenCategory.Quote);
                _lexerState.MoveUntil(CharacterClasses.IsAttributeValue, TokenCategory.AttributeContent);
                _lexerState.Must('"', TokenCategory.Quote);

                return TokenCategory.Attribute;
            });
        }

        private void Name()
        {
            if (!CharacterClasses.IsNameStartChar(_lexerState.CurrentChar()))
                return;

            _lexerState.Move();
            _lexerState.MoveUntil(CharacterClasses.IsNameChar, TokenCategory.Name);
        }
    }
}