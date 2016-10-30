using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TerrificNet.Thtml.Emit;
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

		public void ElementOpenStart(IExpressionBuilder expressionBuilder, string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties != null && staticProperties.Count > 0)
			{
				var stringBuilder = new StringBuilder();
				AddStaticProperties(stringBuilder, tagName, staticProperties);

				expressionBuilder.Add(ExpressionHelper.Write(InstanceExpression, stringBuilder.ToString()));
				return;
			}

			expressionBuilder.Add(ExpressionHelper.Write(InstanceExpression, $"<{tagName}"));
		}

		public void ElementOpenEnd(IExpressionBuilder expressionBuilder)
		{
			expressionBuilder.Add(ExpressionHelper.Write(InstanceExpression, ">"));
		}

		public void ElementOpen(IExpressionBuilder expressionBuilder, string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties != null && staticProperties.Count > 0)
			{
				var stringBuilder = new StringBuilder();
				AddStaticProperties(stringBuilder, tagName, staticProperties);
				stringBuilder.Append(">");

				expressionBuilder.Add(ExpressionHelper.Write(InstanceExpression, stringBuilder.ToString()));
				return;
			}
			expressionBuilder.Add(ExpressionHelper.Write(InstanceExpression, $"<{tagName}>"));
		}

		private static void AddStaticProperties(StringBuilder builder, string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			builder.Append($"<{tagName}");
			staticProperties.Aggregate(builder, (b, a) => b.Append(" ").Append(a.Key).Append("=\"").Append(a.Value).Append("\""));
		}

		public void ElementClose(IExpressionBuilder expressionBuilder, string tagName)
		{
			expressionBuilder.Add(ExpressionHelper.Write(InstanceExpression, $"</{tagName}>"));
		}

		public void PropertyStart(IExpressionBuilder expressionBuilder, string propertyName)
		{
			expressionBuilder.Add(ExpressionHelper.Write(InstanceExpression, $" {propertyName}=\""));
		}

		public void PropertyEnd(IExpressionBuilder expressionBuilder)
		{
			expressionBuilder.Add(ExpressionHelper.Write(InstanceExpression, "\""));
		}

		public void Value(IExpressionBuilder expressionBuilder, IBinding valueBinding)
		{
			Expression expression;
			if (!valueBinding.TryGetExpression(out expression))
				return;

			Value(expressionBuilder, expression);
		}

		private void Value(IExpressionBuilder expressionBuilder, Expression value)
		{
			expressionBuilder.Add(ExpressionHelper.Write(InstanceExpression, value));
		}

		public void Text(IExpressionBuilder expressionBuilder, string text)
		{
			Value(expressionBuilder, Expression.Constant(text));
		}
	}
}