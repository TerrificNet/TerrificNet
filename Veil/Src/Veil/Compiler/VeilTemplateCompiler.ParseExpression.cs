﻿using System.Linq.Expressions;
using System.Reflection;
using TerrificNet.Thtml.Binding;
using Veil.Parser;
using Veil.Parser.Nodes;

namespace Veil.Compiler
{
	internal partial class VeilTemplateCompiler<T>
	{
		private static readonly MethodInfo RuntimeBindFunction = typeof(Helpers).GetMethod("RuntimeBind");

		private Expression ParseExpression(ExpressionNode node)
		{
			bool escapeHtml;
			return ParseExpression(node, out escapeHtml);
		}

		private Expression ParseExpression(ExpressionNode node, out bool escapeHtml)
		{
			escapeHtml = true;
			if (node is PropertyExpressionNode) return EvaluateProperty((PropertyExpressionNode)node);
			if (node is FieldExpressionNode) return EvaluateField((FieldExpressionNode)node);
			if (node is SubModelExpressionNode) return EvaluateSubModel((SubModelExpressionNode)node);
			if (node is SelfExpressionNode) return EvaluateSelfExpressionNode((SelfExpressionNode)node);
			if (node is LateBoundExpressionNode) return EvaluateLateBoundExpression((LateBoundExpressionNode)node);
			if (node is CollectionHasItemsExpressionNode) return EvaluateHasItemsNode((CollectionHasItemsExpressionNode)node);
			if (node is FunctionCallExpressionNode) return EvaluateFunctionCall((FunctionCallExpressionNode)node);

			throw new VeilCompilerException("Unknown expression type '{0}'".FormatInvariant(node.GetType().Name), node);
		}

		private Expression EvaluateFunctionCall(FunctionCallExpressionNode node)
		{
			var modelExpression = EvaluateScope(node.Scope, node);
			return Expression.Call(modelExpression, node.MethodInfo);
		}

		private Expression EvaluateHasItemsNode(CollectionHasItemsExpressionNode node)
		{
			var collection = this.ParseExpression(node.CollectionExpression);
			var count = node.CollectionExpression.ResultType.GetCollectionInterface().GetProperty("Count");
			return Expression.NotEqual(Expression.Property(collection, count), Expression.Constant(0));
		}

		private Expression EvaluateLateBoundExpression(LateBoundExpressionNode node)
		{
			var modelExpression = EvaluateScope(node.Scope, node);
			return Expression.Call(null, RuntimeBindFunction, new[] {
                modelExpression,
                Expression.Constant(node)
            });
		}

		private Expression EvaluateSelfExpressionNode(SelfExpressionNode node)
		{
			return EvaluateScope(node.Scope, node);
		}

		private Expression EvaluateSubModel(SubModelExpressionNode node)
		{
			var modelExpression = ParseExpression(node.ModelExpression);
			PushScope(modelExpression);
			var subModel = ParseExpression(node.SubModelExpression);
			PopScope();
			return subModel;
		}

		private Expression EvaluateField(FieldExpressionNode node)
		{
			var modelExpression = EvaluateScope(node.Scope, node);
			return Expression.Field(modelExpression, node.FieldInfo);
		}

		private Expression EvaluateProperty(PropertyExpressionNode node)
		{
			var modelExpression = EvaluateScope(node.Scope, node);
			return Expression.Block(
                NullCheck(string.Format("Cannot access property '{0}' because value is null", node.PropertyInfo.Name), node, modelExpression),
                Expression.Property(modelExpression, node.PropertyInfo));
		}

		private Expression EvaluateScope(ExpressionScope scope, SyntaxTreeNode node)
		{
			switch (scope)
			{
				case ExpressionScope.CurrentModelOnStack: return this._modelStack.First.Value;
				case ExpressionScope.RootModel: return this._modelStack.Last.Value;
				case ExpressionScope.ModelOfParentScope: return this._modelStack.First.Next.Value;
				default:
					throw new VeilCompilerException("Unknown expression scope '{0}'".FormatInvariant(scope), node);
			}
		}
	}
}