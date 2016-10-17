using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class IncrementalDomRendererExpressionBuilder : IOutputExpressionBuilder
	{
		private readonly Expression _parameterExpression;

		private readonly List<Expression> _propertyValueExpressions = new List<Expression>();
		private string _propertyName;

		public IncrementalDomRendererExpressionBuilder(Expression parameterExpression)
		{
			_parameterExpression = parameterExpression;
		}

		public Type ParameterType => typeof(IIncrementalDomRenderer);

		public Expression ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			var methodInfo = ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementOpenStart(null, null, null, null));
			return Expression.Call(_parameterExpression, methodInfo, Expression.Constant(tagName), Expression.Constant(null, typeof(string)), Expression.Constant(staticProperties), Expression.Constant(null, typeof(Dictionary<string, string>)));
		}

		public Expression ElementOpenEnd()
		{
			var methodInfo = ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementOpenEnd());
			return Expression.Call(_parameterExpression, methodInfo);
		}

		public Expression ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			var methodInfo = ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementOpen(null, null, null, null));
			return Expression.Call(_parameterExpression, methodInfo, Expression.Constant(tagName), Expression.Constant(null, typeof(string)), Expression.Constant(staticProperties), Expression.Constant(null, typeof(Dictionary<string, string>)));
		}

		public Expression PropertyStart(string propertyName)
		{
			_propertyName = propertyName;

			return Expression.Empty();
		}

		public Expression PropertyEnd()
		{
			var propertyNameExpression = Expression.Constant(_propertyName);

			var methodInfo = ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.Attr(null, null));
			Expression valueExpression;
			if (_propertyValueExpressions.Count == 1)
				valueExpression = _propertyValueExpressions[0];
			else if (_propertyValueExpressions.Count == 0)
				valueExpression = Expression.Constant(null, typeof(string));
			else
			{
				var addMethod = ExpressionHelper.GetMethodInfo<StringBuilder>(b => b.Append(""));
				var toString = ExpressionHelper.GetMethodInfo<StringBuilder, string>(b => b.ToString());
				var variable = Expression.Variable(typeof(StringBuilder));
				var block = new List<Expression>();
				block.Add(Expression.Assign(variable, Expression.New(typeof(StringBuilder))));
				block.AddRange(_propertyValueExpressions.Select(e => Expression.Call(variable, addMethod, e)));

				block.Add(Expression.Call(variable, toString));

				valueExpression = Expression.Block(new[] {variable}, block);
			}

			_propertyName = null;
			_propertyValueExpressions.Clear();

			return Expression.Call(_parameterExpression, methodInfo, propertyNameExpression, valueExpression);
		}

		public Expression Value(Expression value)
		{
			if (_propertyName != null)
			{
				_propertyValueExpressions.Add(value);
				return Expression.Empty();
			}

			var methodInfo = ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.Text(null));
			return Expression.Call(_parameterExpression, methodInfo, value);
		}

		public Expression ElementClose(string tagName)
		{
			return Expression.Call(_parameterExpression, ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementClose(null)), Expression.Constant(tagName));
		}

		public MethodCallExpression ElementOpen(string tagName, Expression staticAttributeList, Expression attributeList)
		{
			return Expression.Call(_parameterExpression, ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementOpen(null, null, null, null)), Expression.Constant(tagName), Expression.Constant(null, typeof(string)), staticAttributeList, attributeList);
		}
	}
}