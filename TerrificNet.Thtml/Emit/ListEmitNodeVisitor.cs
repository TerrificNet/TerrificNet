using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit
{
	internal abstract class ListEmitNodeVisitor<TEmit> : EmitNodeVisitorBase<IListEmitter<TEmit>, object>
	{
		protected ListEmitNodeVisitor(IDataScopeContract dataScopeContract, IHelperBinder<IListEmitter<TEmit>, object> helperBinder)
			: base(dataScopeContract, helperBinder)
		{
			Emitter = new ListEmitterFactory<TEmit>();
		}

		protected ListEmitterFactory<TEmit> Emitter { get; }

		public override IListEmitter<TEmit> Visit(Statement statement)
		{
			var expression = statement.Expression;
			return HandleStatement(expression, statement.ChildNodes);
		}

		protected IListEmitter<TEmit> HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(DataScopeContract, iterationExpression.Expression);

				IDataScopeContract childScopeContract;
				var evaluator = scope.RequiresEnumerable(out childScopeContract);

				var child = CreateVisitor(childScopeContract);
				var children = childNodes.Select(c => c.Accept(child)).ToList();

				return Emitter.Iterator(d => evaluator.Evaluate(d), Emitter.Many(children));
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				var scope = ScopeEmitter.Bind(DataScopeContract, conditionalExpression.Expression);
				var evaluator = scope.RequiresBoolean();

				var children = childNodes.Select(c => c.Accept(this)).ToList();
				return Emitter.Condition(d => evaluator.Evaluate(d), Emitter.Many(children));
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				var result = HelperBinder.FindByName(callHelperExpression.Name,
					CreateDictionaryFromArguments(callHelperExpression.Attributes));
				if (result == null)
					throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

				var children = childNodes.Select(c => c.Accept(this)).ToList();
				var evaluation = result.CreateEmitter(this, Emitter.Many(children), HelperBinder, DataScopeContract);
				return evaluation;
			}

			var contentEmitter = expression.Accept(this);
			if (contentEmitter != null)
				return contentEmitter;

			var elements = childNodes.Select(childNode => childNode.Accept(this)).ToList();
			return Emitter.Many(elements);
		}

		protected abstract INodeVisitor<IListEmitter<TEmit>> CreateVisitor(IDataScopeContract childScopeContract);

		private static IDictionary<string, string> CreateDictionaryFromArguments(HelperAttribute[] attributes)
		{
			return attributes.ToDictionary(d => d.Name, d => d.Value);
		}
	}
}