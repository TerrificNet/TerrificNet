using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class StreamBuilderExpression : IOutputExpressionBuilder
	{
		public StreamBuilderExpression(Expression instance)
		{
			InstanceExpression = instance;
		}

		public Expression InstanceExpression { get; }

		public Expression ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties.Count > 0)
			{
				var sb = CreateStaticProperties(tagName, staticProperties);
				ExpressionHelper.Write(InstanceExpression, sb.ToString());
			}
			return ExpressionHelper.Write(InstanceExpression, $"<{tagName}");
		}

		public Expression ElementOpenEnd()
		{
			return ExpressionHelper.Write(InstanceExpression, ">");
		}

		public Expression ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties.Count > 0)
			{
				var sb = CreateStaticProperties(tagName, staticProperties);
				sb.Append(">");

				return ExpressionHelper.Write(InstanceExpression, sb.ToString());
			}
			return ExpressionHelper.Write(InstanceExpression, $"<{tagName}>");
		}

		private static StringBuilder CreateStaticProperties(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			var sb = new StringBuilder($"<{tagName}");
			staticProperties.Aggregate(sb, (b, a) => b.Append(" ").Append(a.Key).Append("=\"").Append(a.Value).Append("\""));
			return sb;
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