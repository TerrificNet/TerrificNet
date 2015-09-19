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

        public TokenCategory Category { get; set; }
        public int Start { get; private set; }
        public int End { get; internal set; }
        public string Lexem { get; protected set; }

        public override bool Equals(object obj)
        {
            return obj is Token && this.Equals((Token)obj);
        }

        protected bool Equals(Token other)
        {
            return Category == other.Category && string.Equals(Lexem, other.Lexem) && Start == other.Start && End == other.End;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Category * 397) ^ (Lexem?.GetHashCode() ?? 0) ^ Start ^ End;
            }
        }

        public void PutChar(char c)
        {
            this.Lexem += c;
            End++;
        }
    }
}