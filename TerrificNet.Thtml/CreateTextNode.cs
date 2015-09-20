using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml
{
    public class CreateTextNode : CreateNode
    {
        private readonly Token _token;
        public string Text { get; set; }

        public CreateTextNode(string text)
        {
            this.Text = text;
        }

        public CreateTextNode(Token token)
        {
            _token = token;
            Text = token.Lexem;
        }
    }
}