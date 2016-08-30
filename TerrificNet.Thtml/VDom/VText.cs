namespace TerrificNet.Thtml.VDom
{
	public class VText : VTree
	{
		public VText(string text)
		{
			Text = text;
		}

		public string Text { get; }

		public override void Accept(IVTreeVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override string Type => "VirtualText";
	}
}