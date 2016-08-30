using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.LexicalAnalysis
{
	public class CompositeToken : Token
	{
		public IList<Token> Tokens { get; private set; }

		public CompositeToken(TokenCategory category, IList<Token> tokens)
			: base(category, tokens.Min(t => t.Start), tokens.Max(t => t.End))
		{
			Tokens = tokens;
			Lexem = tokens.OrderBy(t => t.Start).Aggregate(string.Empty, (val, token) => val + token.Lexem);
		}

		public CompositeToken(TokenCategory category, string lexem, int start, int end) : base(category, lexem, start, end)
		{
		}
	}
}