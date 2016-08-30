namespace TerrificNet.Thtml.LexicalAnalysis
{
	internal class HtmlGrammar
	{
		private readonly LexerState _lexerState;
		private readonly HandlebarsGrammar _handlebarsGrammar;
		private readonly CommonGrammar _commonGrammar;

		public HtmlGrammar(LexerState lexerState)
		{
			_lexerState = lexerState;
			_handlebarsGrammar = new HandlebarsGrammar(lexerState);
			_commonGrammar = new CommonGrammar(lexerState);
		}

		public void Document()
		{
			_lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);

			ElementList();

			_lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);

			if (!_lexerState.Eof())
				_lexerState.Problem("Expected end of document");
		}

		private void ElementList()
		{
			while (true)
			{
				_lexerState.MoveUntil(CharacterClasses.IsCharData, TokenCategory.Content);

				if (_lexerState.Can(ElementOrComment, TokenCategory.ElementStart, TokenCategory.ElementEnd, TokenCategory.EmptyElement, TokenCategory.Comment)
				    || _lexerState.Can(Handlebars, TokenCategory.External))
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
				_handlebarsGrammar.Handlebar();

				_lexerState.Must('}', TokenCategory.HandlebarsEnd);
				_lexerState.Must('}', TokenCategory.HandlebarsEnd);

				return TokenCategory.External;
			}, 2);
		}

		private void ElementOrComment()
		{
			if (!_lexerState.Can('<', TokenCategory.BracketOpen))
				return;

			if (_lexerState.Can('!', TokenCategory.CommentStart))
			{
				_lexerState.Composite(() =>
				{
					_lexerState.Must('-', TokenCategory.Dash);
					_lexerState.Must('-', TokenCategory.Dash);

					_lexerState.MoveUntil((c1, c2) => (c1 == '-' && c2 != '-') || c1 != '-',
						TokenCategory.CommentContent);

					_lexerState.Must('-', TokenCategory.Dash);
					_lexerState.Must('-', TokenCategory.Dash);
					_lexerState.Must('>', TokenCategory.BracketClose);

					return TokenCategory.Comment;
				}, 2);
			}
			else
			{
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
		}

		private bool ElementEnd(ref TokenCategory tokenCategory)
		{
			if (!_lexerState.Can('/', TokenCategory.Slash))
				return false;

			_lexerState.Must(() => _commonGrammar.Name(), TokenCategory.Name);
			_lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);

			tokenCategory = TokenCategory.ElementEnd;
			return true;
		}

		private TokenCategory ElementStart()
		{
			_lexerState.Must(() => _commonGrammar.Name(), TokenCategory.Name);
			AttributeList();

			if (_lexerState.Can('/', TokenCategory.Slash))
				return TokenCategory.EmptyElement;

			return TokenCategory.ElementStart;
		}

		private void AttributeList()
		{
			while (true)
			{
				if (_lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace)
				    || _lexerState.Can(Handlebars, TokenCategory.External))
				{
					_lexerState.Can(Attribute, TokenCategory.Attribute);
					continue;
				}
				break;
			}
		}

		private void Attribute()
		{
			if (!_lexerState.Can(() => _commonGrammar.Name(), TokenCategory.Name))
				return;

			_lexerState.Composite(() =>
			{
				_lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);

				if (!_lexerState.Can('=', TokenCategory.Equality))
					return TokenCategory.Attribute;

				_lexerState.Can(() => _commonGrammar.Whitespace(), TokenCategory.Whitespace);
				_lexerState.Must('"', TokenCategory.Quote);
				_lexerState.MoveUntil(CharacterClasses.IsAttributeValue, TokenCategory.AttributeContent);
				while (_lexerState.Can(Handlebars, TokenCategory.External))
				{
					_lexerState.MoveUntil(CharacterClasses.IsAttributeValue, TokenCategory.AttributeContent);
				}
				_lexerState.Must('"', TokenCategory.Quote);

				return TokenCategory.Attribute;
			});
		}
	}
}