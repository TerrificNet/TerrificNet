﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TerrificNet.Thtml.Binding;
using Veil.Parser;

namespace Veil.Handlebars
{
    internal static class HandlebarsExpressionParser
    {
        public static ExpressionNode Parse(HandlebarsParserState state, HandlebarsBlockStack blockStack, SourceLocation location, string expression, IMemberLocator memberLocator = null)
        {
	        memberLocator = memberLocator ?? MemberLocator.Default;

            expression = expression.Trim();

            if (expression == "this")
            {
                return SyntaxTreeExpression.Self(blockStack.GetCurrentModelType(), location, ExpressionScope.CurrentModelOnStack);
            }
            if (expression.StartsWith("../"))
            {
                return ParseAgainstModel(blockStack.GetParentModelType(), expression.Substring(3), ExpressionScope.ModelOfParentScope, memberLocator, location.MoveIndex(3));
            }

            return ParseAgainstModel(blockStack.GetCurrentModelType(), expression, ExpressionScope.CurrentModelOnStack, memberLocator, location);
        }

		private static ExpressionNode ParseAgainstModel(Type modelType, string expression, ExpressionScope expressionScope, IMemberLocator memberLocator, SourceLocation location)
        {
            var dotIndex = expression.IndexOf('.');
            if (dotIndex >= 0)
            {
				var subModel = ParseAgainstModel(modelType, expression.Substring(0, dotIndex), expressionScope, memberLocator, location.SetLength(dotIndex));
                return SyntaxTreeExpression.SubModel(
                    subModel,
                    ParseAgainstModel(subModel.ResultType, expression.Substring(dotIndex + 1), ExpressionScope.CurrentModelOnStack, memberLocator, location.MoveIndex(dotIndex + 1)),
					location
                );
            }

            if (expression.EndsWith("()"))
            {
                var func = memberLocator.FindMethod(modelType, expression.Substring(0, expression.Length - 2));
                if (func != null) return SyntaxTreeExpression.Function(modelType, func.Name, location, expressionScope);
            }

            var prop = memberLocator.FindProperty(modelType, expression);
		    if (prop != null)
		        return SyntaxTreeExpression.Property(modelType, prop.Name, location, expressionScope);

            var field = memberLocator.FindField(modelType, expression);
		    if (field != null)
		        return SyntaxTreeExpression.Field(modelType, field.Name, location, expressionScope);

            if (IsLateBoundAcceptingType(modelType)) 
				return SyntaxTreeExpression.LateBound(expression, location, memberLocator, false, expressionScope);

            throw new VeilParserException(
                $"Unable to parse model expression '{expression}' againt model '{modelType.Name}'", location);
        }

        private static bool IsLateBoundAcceptingType(Type type)
        {
            return type == typeof(object) 
                || type.IsDictionary()
                || type.GetInterfaces().Any(IsDictionary)
                || type.GetProperties().Any(p => p.GetIndexParameters().Any());
        }

        private static bool IsDictionary(this Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }
    }
}