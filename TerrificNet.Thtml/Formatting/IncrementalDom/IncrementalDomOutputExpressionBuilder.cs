using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Formatting.IncrementalDom
{
	internal class IncrementalDomOutputExpressionBuilder : IOutputExpressionBuilder
	{
		private readonly List<Expression> _propertyValueExpressions = new List<Expression>();
		private string _propertyName;

		public IncrementalDomOutputExpressionBuilder(Expression parameterExpression)
		{
			InstanceExpression = parameterExpression;
		}

		public Expression InstanceExpression { get; }

		public void ElementOpenStart(IExpressionBuilder expressionBuilder, string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			var methodInfo = ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementOpenStart(null, null, null, null));
			var ex = Expression.Call(InstanceExpression, methodInfo, Expression.Constant(tagName), Expression.Constant(null, typeof(string)), Expression.Constant(staticProperties), Expression.Constant(null, typeof(Dictionary<string, string>)));
			expressionBuilder.Add(ex);
		}

		public void ElementOpenEnd(IExpressionBuilder expressionBuilder)
		{
			var methodInfo = ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementOpenEnd());
			expressionBuilder.Add(Expression.Call(InstanceExpression, methodInfo));
		}

		public void ElementOpen(IExpressionBuilder expressionBuilder, string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			var methodInfo = ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementOpen(null, null, null, null));
			var ex = Expression.Call(InstanceExpression, methodInfo, Expression.Constant(tagName), Expression.Constant(null, typeof(string)), Expression.Constant(staticProperties), Expression.Constant(null, typeof(Dictionary<string, string>)));
			expressionBuilder.Add(ex);
		}

		public void PropertyStart(IExpressionBuilder expressionBuilder, string propertyName)
		{
			_propertyName = propertyName;
		}

		public void PropertyEnd(IExpressionBuilder expressionBuilder)
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

				valueExpression = Expression.Block(new[] { variable }, block);
			}

			_propertyName = null;
			_propertyValueExpressions.Clear();

			expressionBuilder.Add(Expression.Call(InstanceExpression, methodInfo, propertyNameExpression, valueExpression));
		}

		public void Value(IExpressionBuilder expressionBuilder, IBinding valueBinding)
		{
			Value(expressionBuilder, valueBinding.EnsureBinding().Expression);
		}

		private void Value(IExpressionBuilder expressionBuilder, Expression value)
		{
			if (_propertyName != null)
			{
				_propertyValueExpressions.Add(value);
				return;
			}

			var methodInfo = ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.Text(null));
			expressionBuilder.Add(Expression.Call(InstanceExpression, methodInfo, value));
		}

		public void Text(IExpressionBuilder expressionBuilder, string text)
		{
			Value(expressionBuilder, Expression.Constant(text));
		}

		public bool SupportsBinding(IBinding binding)
		{
			return binding is IBindingWithExpression;
		}

		public void ElementClose(IExpressionBuilder expressionBuilder, string tagName)
		{
			var ex = Expression.Call(InstanceExpression, ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementClose(null)), Expression.Constant(tagName));
			expressionBuilder.Add(ex);
		}
	}
}