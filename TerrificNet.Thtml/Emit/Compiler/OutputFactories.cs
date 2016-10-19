using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public static class OutputFactories
	{
		public static OutputExpressionBuilderFactory<TextWriterOutput> Stream { get; } = new OutputExpressionBuilderFactory<TextWriterOutput>(p => new StreamBuilderExpression(AccessInnerProperty(p)));
		public static OutputExpressionBuilderFactory<VDomOutput> VTree { get; } = new OutputExpressionBuilderFactory<VDomOutput>(p => new VDomOutputExpressionBuilder(AccessInnerProperty(p)));
		public static OutputExpressionBuilderFactory<IncrementalDomOutput> IncrementalDomScript { get; } = new OutputExpressionBuilderFactory<IncrementalDomOutput>(p => new IncrementalDomRendererExpressionBuilder(AccessInnerProperty(p)));

		private static MemberExpression AccessInnerProperty(Expression p)
		{
			return Expression.Property(p, "Inner");
		}
	}
}