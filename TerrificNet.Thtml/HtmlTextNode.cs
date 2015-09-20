using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml
{
    public class HtmlTextNode : HtmlNode
    {
        private readonly Token _token;
        public string Text { get; set; }

        public HtmlTextNode(string text)
        {
            this.Text = text;
        }

        public HtmlTextNode(Token token)
        {
            _token = token;
            Text = token.Lexem;
        }
    }
}