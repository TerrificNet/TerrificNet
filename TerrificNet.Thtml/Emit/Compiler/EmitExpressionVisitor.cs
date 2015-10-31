using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;
using ConditionalExpression = TerrificNet.Thtml.Parsing.Handlebars.ConditionalExpression;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class EmitExpressionVisitor : NodeVisitorBase<Expression>
	{
		private readonly IDataScopeContract _dataScopeContract;
		private readonly IHelperBinder _helperBinder;
		private readonly ParameterExpression _writerParameter = Expression.Parameter(typeof(TextWriter));
		private readonly ParameterExpression _dataContextParameter = Expression.Parameter(typeof(IDataContext));

		public Action<TextWriter, IDataContext> DocumentFunc { get; private set; }

		public EmitExpressionVisitor(IDataScopeContract dataScopeContract, IHelperBinder helperBinder, ParameterExpression writerParameter = null)
		{
			_dataScopeContract = dataScopeContract;
			_helperBinder = helperBinder;

			if (writerParameter != null)
				_writerParameter = writerParameter;
		}

		public override Expression Visit(Document document)
		{
			var elements = document.ChildNodes.Select(node => node.Accept(this)).ToList();

			var expression = elements.Count == 0 ? (Expression)Expression.Empty() : Expression.Block(elements);
			DocumentFunc = Expression.Lambda<Action<TextWriter, IDataContext>>(expression, _writerParameter, _dataContextParameter).Compile();

			return expression;
		}

		public override Expression Visit(Element element)
		{
			var expressions = new List<Expression>();
			expressions.Add(Write($"<{element.TagName}"));
			expressions.AddRange(element.Attributes.Select(attribute => attribute.Accept(this)));
			expressions.Add(Write(">"));
			expressions.AddRange(element.ChildNodes.Select(i => i.Accept(this)));
			expressions.Add(Write($"</{element.TagName}>"));

			return Expression.Block(expressions);
		}

		public override Expression Visit(AttributeNode attributeNode)
		{
			var valueEmitter = attributeNode.Value.Accept(this);

			return Expression.Block(Write(" " + attributeNode.Name + "=\""),
				valueEmitter,
				Write("\""));

			//return EmitterNode.AsList(EmitterNode.Lambda<StreamWriterHandler>((d, r) => (writer =>
			//{
			//	writer.Write(" {0}=\"", attributeNode.Name);
			//	GetPropertyValue(writer, valueEmitter, d, r);
			//	writer.Write("\"");
			//})));
		}

		public override Expression Visit(ConstantAttributeContent attributeContent)
		{
			return Write(attributeContent.Text);
		}

		public override Expression Visit(AttributeContentStatement constantAttributeContent)
		{
			return HandleStatement(constantAttributeContent.Expression, constantAttributeContent.Children);
		}

		public override Expression Visit(Statement statement)
		{
			var expression = statement.Expression;
			return HandleStatement(expression, statement.ChildNodes);
		}

		public override Expression Visit(UnconvertedExpression unconvertedExpression)
		{
			return unconvertedExpression.Expression.Accept(this);
		}

		public override Expression Visit(MemberExpression memberExpression)
		{
			var scope = ScopeEmitter.Bind(_dataScopeContract, memberExpression);

			var evaluator = scope.RequiresString();
			var evaluateMethod = GetMethodInfo<IEvaluator<string>>(i => i.Evaluate(null));
			var callExpression = Expression.Call(Expression.Constant(evaluator), evaluateMethod, _dataContextParameter);
			return Write(callExpression);
		}

		public override Expression Visit(TextNode textNode)
		{
			return Write(textNode.Text);
		}

		protected Expression HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, iterationExpression.Expression);

				IDataScopeContract childScopeContract;
				var evaluator = scope.RequiresEnumerable(out childScopeContract);

				//var tmp = childScopeContract.ResultType;

				var child = CreateVisitor(childScopeContract);
				var children = childNodes.Select(c => c.Accept(child)).ToList();
				var bodyExpression = children.Any() ? (Expression)Expression.Block(children) : Expression.Empty();

				var evaluateMethod = GetMethodInfo<IEvaluator<IEnumerable>>(i => i.Evaluate(null));
				var collection = Expression.Call(Expression.Constant(evaluator), evaluateMethod, _dataContextParameter);
				return ForEach(collection, child._dataContextParameter, bodyExpression);

				//return EmitterNode.Iterator(d => evaluator.Evaluate(d), EmitterNode.Many(children));

				return Expression.Empty();
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				throw new NotImplementedException();

				//var scope = ScopeEmitter.Bind(_dataScopeContract, conditionalExpression.Expression);
				//	var evaluator = scope.RequiresBoolean();

				//	var children = childNodes.Select(c => c.Accept(this)).ToList();


				//	return EmitterNode.Condition(d => evaluator.Evaluate(d), EmitterNode.Many(children));
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				throw new NotImplementedException();
			}

			var contentEmitter = expression.Accept(this);
			if (contentEmitter != null)
				return contentEmitter;

			var elements = childNodes.Select(childNode => childNode.Accept(this)).ToList();
			return elements.Any() ? (Expression)Expression.Block(elements) : Expression.Empty();
		}

		private EmitExpressionVisitor CreateVisitor(IDataScopeContract childScopeContract)
		{
			return new EmitExpressionVisitor(childScopeContract, _helperBinder, _writerParameter);
		}

		private static void GetPropertyValue(TextWriter writer, IListEmitter<VPropertyValue> emitter, IDataContext dataContext, IRenderingContext renderingContext)
		{
			foreach (var emit in emitter.Execute(dataContext, renderingContext))
			{
				var stringValue = emit as StringVPropertyValue;
				if (stringValue == null)
					throw new Exception($"Unsupported property value {emit.GetType()}.");

				writer.Write(stringValue.Value);
			}
		}

		public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
		{
			var elementType = loopVar.Type;
			var enumerableType = typeof(IEnumerable);
			var enumeratorType = typeof(IEnumerator);

			var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
			var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
			var enumeratorAssign = Expression.Assign(enumeratorVar, Expression.Convert(getEnumeratorCall, enumeratorType));

			// The MoveNext method's actually on IEnumerator, not IEnumerator<T>
			var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

			var breakLabel = Expression.Label("LoopBreak");

			var loop = Expression.Block(new[] { enumeratorVar },
				enumeratorAssign,
				Expression.Loop(
					Expression.IfThenElse(
						Expression.Equal(moveNextCall, Expression.Constant(true)),
						Expression.Block(new[] { loopVar },
							Expression.Assign(loopVar, Expression.Convert(Expression.Property(enumeratorVar, "Current"), elementType)),
							loopContent
						),
						Expression.Break(breakLabel)
					),
				breakLabel)
			);

			return loop;
		}

		private Expression Write(Expression inputExpression)
		{
			return Expression.Call(_writerParameter, GetMethodInfo<TextWriter>(i => i.Write("")), inputExpression);
		}

		private Expression Write(string value)
		{
			var param = Expression.Constant(value);
			return Expression.Call(_writerParameter, GetMethodInfo<TextWriter>(i => i.Write("")), param);
		}

		private static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
		{
			var member = expression.Body as MethodCallExpression;

			if (member != null)
				return member.Method;

			throw new ArgumentException("Expression is not a method", "expression");
		}
	}
}