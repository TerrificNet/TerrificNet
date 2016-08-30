using System.Linq;

namespace TerrificNet.Thtml.LexicalAnalysis
{
	public class CommonGrammar
	{
		private readonly LexerState _lexerState;

		internal CommonGrammar(LexerState lexerState)
		{
			_lexerState = lexerState;
		}

		internal void Whitespace()
		{
			_lexerState.MoveUntil(CharacterClasses.WhitespaceCharacters.Contains, TokenCategory.Whitespace);
		}

		internal void Name()
		{
			if (!CharacterClasses.IsNameStartChar(_lexerState.CurrentChar()))
				return;

			_lexerState.Move();
			_lexerState.MoveUntil(CharacterClasses.IsNameChar, TokenCategory.Name);
		}
	}
}