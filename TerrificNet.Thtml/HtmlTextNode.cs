using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml
{
    public class HtmlTextNode : HtmlNode
    {
        private readonly Token _token;
        public string Text { get; set; }

        public HtmlTextNode(Token token)
        {
            _token = token;
            Text = token.Lexem;
        }
    }
}