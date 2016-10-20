namespace TerrificNet.Thtml.Parsing
{
	public class TextNode : Node
	{
		public string Text { get; }

		public TextNode(string text)
		{
			Text = text;
		}

		public override T Accept<T>(INodeVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}

		protected override bool CheckIfIsFixed()
		{
			return true;
		}
	}
}