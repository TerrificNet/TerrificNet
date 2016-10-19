using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Formatting.Text
{
	internal class TextWriterOutputExpressionBuilder : IOutputExpressionBuilder
	{
		public TextWriterOutputExpressionBuilder(Expression instance)
		{
			InstanceExpression = instance;
		}

		public Expression InstanceExpression { get; }

		public Expression ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties != null && staticProperties.Count > 0)
			{
				var stringBuilder = new StringBuilder();
				AddStaticProperties(stringBuilder, tagName, staticProperties);

				return ExpressionHelper.Write(InstanceExpression, stringBuilder.ToString());
			}
			return ExpressionHelper.Write(InstanceExpression, $"<{tagName}");
		}

		public Expression ElementOpenEnd()
		{
			return ExpressionHelper.Write(InstanceExpression, ">");
		}

		public Expression ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties != null && staticProperties.Count > 0)
			{
				var stringBuilder = new StringBuilder();
				AddStaticProperties(stringBuilder, tagName, staticProperties);
				stringBuilder.Append(">");

				return ExpressionHelper.Write(InstanceExpression, stringBuilder.ToString());
			}
			return ExpressionHelper.Write(InstanceExpression, $"<{tagName}>");
		}

		private static void AddStaticProperties(StringBuilder builder, string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			builder.Append($"<{tagName}");
			staticProperties.Aggregate(builder, (b, a) => b.Append(" ").Append(a.Key).Append("=\"").Append(a.Value).Append("\""));
		}

		public Expression ElementClose(string tagName)
		{
			return ExpressionHelper.Write(InstanceExpression, $"</{tagName}>");
		}

		public Expression PropertyStart(string propertyName)
		{
			return ExpressionHelper.Write(InstanceExpression, $" {propertyName}=\"");
		}

		public Expression PropertyEnd()
		{
			return ExpressionHelper.Write(InstanceExpression, $"\"");
		}

		public Expression Value(Expression value)
		{
			return ExpressionHelper.Write(InstanceExpression, value);
		}
	}
}