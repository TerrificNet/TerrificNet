using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrificNet.Thtml.LexicalAnalysis
{
    internal class LexerState
    {
        private readonly string _input;

        public List<Token> Tokens = new List<Token>();
        private Token _currentToken;
        private readonly StringBuilder _lexemBuffer = new StringBuilder();
        private int _position;

        public LexerState(string input)
        {
            _input = input;
        }

        public void MoveUntil(Func<char, bool> predicate, TokenCategory tokenCategory)
        {
            for (; _position < _input.Length && predicate(_input[_position]); _position++)
            {
                _lexemBuffer.Append(_input[_position]);
            }

            if (_lexemBuffer.Length > 0)
            {
                var start = _position - _lexemBuffer.Length;
                Put(new Token(tokenCategory, _lexemBuffer.ToString(), start, start + _lexemBuffer.Length));

                _lexemBuffer.Clear();
            }
        }

        public void MoveUntil(Func<char, char, bool> predicate, TokenCategory tokenCategory)
        {
            for (; _position < _input.Length && predicate(_input[_position], GetChar(_position + 1)); _position++)
            {
                _lexemBuffer.Append(_input[_position]);
            }

            if (_lexemBuffer.Length > 0)
            {
                var start = _position - _lexemBuffer.Length;
                Put(new Token(tokenCategory, _lexemBuffer.ToString(), start, start + _lexemBuffer.Length));

                _lexemBuffer.Clear();
            }
        }

        public bool Can(Action action, params TokenCategory[] tokens)
        {
            var oldToken = _currentToken;
            if (!Eof())
                action();

            return !ReferenceEquals(_currentToken, oldToken) && (tokens.Contains(_currentToken.Category));
        }

        public bool Eof()
        {
            return _position >= _input.Length;
        }

        public void Composite(Func<TokenCategory> compositeAction, int indicatorCount = 1)
        {
            var tmpToken1 = Tokens;
            
            var indicators = TakeIndicators(indicatorCount).ToList();
            indicators.Reverse();

            Tokens = new List<Token>();
            foreach (var indicatorToken in indicators)
            {
                Tokens.Add(indicatorToken);
            }

            var tokenCategory = compositeAction();

            var subTokens = Tokens;
            Tokens = tmpToken1;

            Put(new CompositeToken(tokenCategory, subTokens));
        }

        private IEnumerable<Token> TakeIndicators(int indicatorCount)
        {
            for (int i = 0; i < indicatorCount; i++)
            {
                int pos = Tokens.Count - 1;
                yield return Tokens[pos];

                Tokens.RemoveAt(pos);
            }
        }

        public bool Can(char c, TokenCategory category)
        {
            if (c == CurrentChar())
            {
                Put(new Token(category, c.ToString(), _position, _position + 1));
                _position++;

                return true;
            }
            return false;
        }

        public char CurrentChar()
        {
            return GetChar(_position);
        }

        private char GetChar(int position)
        {
            return position < _input.Length ? _input[position] : (char) 0;
        }

        public void Must(char c, TokenCategory token)
        {
            if (CurrentChar() != c)
                Problem($"Character {c} expected.");

            Put(new Token(token, c.ToString(), _position, _position + 1));
            _position++;
        }

        public void Must(Action name, TokenCategory tokenCategory)
        {
            if (Eof())
                Problem($"Unexpected end of file. Expected a {tokenCategory} token.");

            name();

            if (_currentToken.Category != tokenCategory)
                Problem($"Expected '{tokenCategory}'.");
        }

        public void Put(Token token)
        {
            _currentToken = token;
            Tokens.Add(token);
        }

        public void Move()
        {
            _lexemBuffer.Append(_input[_position]);
            _position++;
        }

        public void Problem(string problem)
        {
            throw new Exception($"{problem} at {_position}");
        }
    }
}