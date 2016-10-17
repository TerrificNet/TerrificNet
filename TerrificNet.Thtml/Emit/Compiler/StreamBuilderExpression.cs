using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class StreamBuilderExpression : IOutputExpressionBuilder
	{
		private readonly Expression _instance;

		public StreamBuilderExpression(Expression instance)
		{
			_instance = instance;
		}

		public Type ParameterType => typeof(TextWriter);

		public Expression ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties.Count > 0)
			{
				var sb = CreateStaticProperties(tagName, staticProperties);
				ExpressionHelper.Write(_instance, sb.ToString());
			}
			return ExpressionHelper.Write(_instance, $"<{tagName}");
		}

		public Expression ElementOpenEnd()
		{
			return ExpressionHelper.Write(_instance, ">");
		}

		public Expression ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties.Count > 0)
			{
				var sb = CreateStaticProperties(tagName, staticProperties);
				sb.Append(">");

				return ExpressionHelper.Write(_instance, sb.ToString());
			}
			return ExpressionHelper.Write(_instance, $"<{tagName}>");
		}

		private static StringBuilder CreateStaticProperties(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			var sb = new StringBuilder($"<{tagName}");
			staticProperties.Aggregate(sb, (b, a) => b.Append(" ").Append(a.Key).Append("=\"").Append(a.Value).Append("\""));
			return sb;
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