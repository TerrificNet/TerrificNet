using System.Linq.Expressions;
using TerrificNet.Thtml.Formatting.IncrementalDom;
using TerrificNet.Thtml.Formatting.Text;
using TerrificNet.Thtml.Formatting.VDom;

namespace TerrificNet.Thtml.Formatting
{
	public static class OutputFactories
	{
		public static OutputExpressionBuilderFactory<TextWriterOutputBuilder> Text { get; } = new OutputExpressionBuilderFactory<TextWriterOutputBuilder>(p => new TextWriterOutputExpressionBuilder(AccessInnerProperty(p)));
		public static OutputExpressionBuilderFactory<VDomOutputBuilder> VTree { get; } = new OutputExpressionBuilderFactory<VDomOutputBuilder>(p => new VDomOutputExpressionBuilder(AccessInnerProperty(p)));
		public static OutputExpressionBuilderFactory<IncrementalDomOutputBuilder> IncrementalDomScript { get; } = new OutputExpressionBuilderFactory<IncrementalDomOutputBuilder>(p => new IncrementalDomOutputExpressionBuilder(AccessInnerProperty(p)));

		private static MemberExpression AccessInnerProperty(Expression p)
		{
			return Expression.Property(p, "Inner");
		}
	}
}