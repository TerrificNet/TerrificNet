using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit
{
	internal abstract class EmitNodeVisitorBase<T> : NodeVisitorBase<IListEmitter<T>>
	{
		protected IHelperBinder HelperBinder { get; }
		protected IDataScope DataScope { get; }

		protected EmitNodeVisitorBase(IDataScope dataScope, IHelperBinder helperBinder)
		{
			DataScope = dataScope;
			HelperBinder = helperBinder;
		}

		protected IListEmitter<T> HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(DataScope, iterationExpression.Expression);

				IDataScope childScope;
				var evaluator = scope.BindEnumerable(out childScope);

				var child = CreateVisitor(childScope);
				var children = childNodes.Select(c => c.Accept(child)).ToList();

				return EmitterNode.Iterator(d => evaluator.Evaluate(d), EmitterNode.Many(children));
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				var scope = ScopeEmitter.Bind(DataScope, conditionalExpression.Expression);
				var evaluator = scope.BindBoolean();

				var children = childNodes.Select(c => c.Accept(this)).ToList();
				return EmitterNode.Condition(d => evaluator.Evaluate(d), EmitterNode.Many(children));
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				var result = HelperBinder.FindByName(callHelperExpression.Name,
					CreateDictionaryFromArguments(callHelperExpression.Attributes));
				if (result == null)
					throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

				var children = childNodes.Select(c => c.Accept(this)).ToList();
				var evaluation = result.CreateEmitter(EmitterNode.Many(children), HelperBinder, DataScope);
				return evaluation;
			}

			var contentEmitter = expression.Accept(this);
			if (contentEmitter != null)
				return contentEmitter;

			var elements = childNodes.Select(childNode => childNode.Accept(this)).ToList();
			return EmitterNode.Many(elements);
		}

		protected abstract INodeVisitor<IListEmitter<T>> CreateVisitor(IDataScope childScope);

		private static IDictionary<string, string> CreateDictionaryFromArguments(HelperAttribute[] attributes)
		{
			return attributes.ToDictionary(d => d.Name, d => d.Value);
		}

		private static IEvaluator<T> ExceptionDecorator<T>(IEvaluator<T> createEvaluation, MustacheExpression expression)
		{
			return new ExceptionWrapperEvaluator<T>(createEvaluation, expression);
		}

		private class ExceptionWrapperEvaluator<T> : IEvaluator<T>
		{
			private readonly IEvaluator<T> _evaluator;
			private readonly MustacheExpression _expression;

			public ExceptionWrapperEvaluator(IEvaluator<T> evaluator, MustacheExpression expression)
			{
				_evaluator = evaluator;
				_expression = expression;
			}

			public T Evaluate(IDataContext context)
			{
				try
				{
					return _evaluator.Evaluate(context);
				}
				catch (Exception ex)
				{
					throw new Exception($"Exception on executing expression {_expression}", ex);
				}
			}
		}
	}
}