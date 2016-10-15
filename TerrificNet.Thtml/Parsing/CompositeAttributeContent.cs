using System.Linq;

namespace TerrificNet.Thtml.Parsing
{
	public class CompositeAttributeContent : AttributeContent
	{
		public AttributeContent[] ContentParts { get; private set; }

		public CompositeAttributeContent(params AttributeContent[] contentParts)
		{
			ContentParts = contentParts;
		}

		public override T Accept<T>(INodeVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}

		protected override bool CheckIfIsFixed()
		{
			return ContentParts.All(p => p.IsFixed);
		}
	}
}