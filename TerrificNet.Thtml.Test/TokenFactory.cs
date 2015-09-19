using TerrificNet.Thtml.LexicalAnalysis;

static internal class TokenFactory
{
    public static Token EmptyElement(string lexem, int position, int end)
    {
        return new CompositeToken(TokenCategory.EmptyElement, lexem, position, end);
    }

    public static Token Content(string lexem, int start)
    {
        return new Token(TokenCategory.Content, lexem, start, start + lexem.Length);
    }

    public static Token ElementEnd(string lexem, int position, int end)
    {
        return new CompositeToken(TokenCategory.ElementEnd, lexem, position, end);
    }

    public static Token EndToken(int position)
    {
        return new Token(TokenCategory.EndDocument, position, position);
    }

    public static Token Whitespace(string lexem, int position)
    {
        return new Token(TokenCategory.Whitespace, lexem, position, position + lexem.Length);
    }

    public static Token ElementStart(string lexem, int position, int end)
    {
        return new CompositeToken(TokenCategory.ElementStart, lexem, position, end);
    }
}