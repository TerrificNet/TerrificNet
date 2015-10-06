namespace TerrificNet.Thtml.LexicalAnalysis
{
    public class Token
    {
        public Token(TokenCategory category, int start, int end)
        {
            Category = category;
            Start = start;
            End = end;
        }

        public Token(TokenCategory category, string lexem, int start, int end)
        {
            Lexem = lexem;
            Start = start;
            End = end;
            Category = category;
        }

        public TokenCategory Category { get; }
        public int Start { get; }
        public int End { get; }
        public string Lexem { get; protected set; }
    }
}