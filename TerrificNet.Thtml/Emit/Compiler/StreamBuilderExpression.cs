using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class StreamBuilderExpression : IOutputExpressionBuilder
	{
		private readonly Expression _instance;

		public StreamBuilderExpression(Expression instance)
		{
			_instance = instance;
		}

		public Expression ElementOpenStart(string tagName)
		{
			return ExpressionHelper.Write(_instance, $"<{tagName}");
		}

		public Expression ElementOpenEnd()
		{
			return ExpressionHelper.Write(_instance, ">");
		}

		public Expression ElementOpen(string tagName)
		{
			return ExpressionHelper.Write(_instance, $"<{tagName}>");
		}

		public Expression ElementClose(string tagName)
		{
			return ExpressionHelper.Write(_instance, $"</{tagName}>");
		}

		public Expression PropertyStart(string propertyName)
		{
			return ExpressionHelper.Write(_instance, $" {propertyName}=\"");
		}

		public Expression PropertyEnd()
		{
			return ExpressionHelper.Write(_instance, $"\"");
		}

		public Expression Value(Expression value)
		{
			return ExpressionHelper.Write(_instance, value);
		}
	}
}