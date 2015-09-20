namespace TerrificNet.Thtml.LexicalAnalysis
{
    public enum TokenCategory
    {
        Whitespace,
        StartDocument,
        ElementStartName,
        BracketOpen,
        ElementStartWithAttribute,
        ElementStart,
        EndDocument,
        Name,
        BracketClose,
        Attribute,
        Equality,
        Quote,
        AttributeContent,
        ElementEnd,
        Slash,
        Content,
        EmptyElement,
        
        HandlebarsStart,
        HandlebarsEnd,
        HandlebarsEvaluate
    }
}