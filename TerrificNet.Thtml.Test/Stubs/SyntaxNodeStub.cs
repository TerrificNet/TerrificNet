using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Test.Stubs
{
	public class SyntaxNodeStub
	{
		internal static readonly SyntaxNode Node1 = new DummySyntaxNode();
		internal static readonly SyntaxNode Node2 = new DummySyntaxNode();
		internal static readonly SyntaxNode Node3 = new DummySyntaxNode();

		private class DummySyntaxNode : SyntaxNode
		{
			protected override bool CheckIfIsFixed()
			{
				return true;
			}
		}
	}
}