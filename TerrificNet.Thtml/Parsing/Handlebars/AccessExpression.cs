namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public abstract class AccessExpression : MustacheExpression
	{
		protected AccessExpression(AccessExpression subExpression)
		{
			SubExpression = subExpression;
		}

		public AccessExpression SubExpression { get; }
	}
}