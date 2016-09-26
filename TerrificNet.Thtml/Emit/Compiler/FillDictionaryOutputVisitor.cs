using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class FillDictionaryOutputVisitor : NodeVisitorBase<IDataScopeContract>
	{
		private readonly IDataScopeContract _dataContract;
		private ParameterExpression _dictionary;
		private static readonly Type DictionaryType = typeof(Dictionary<string, IDataScopeContract>);

		private readonly Dictionary<string, IDataScopeContract> attributeContracts = new Dictionary<string, IDataScopeContract>();

		public FillDictionaryOutputVisitor(IDataScopeContract dataContract)
		{
			_dataContract = dataContract;
		}

		public override IDataScopeContract Visit(Element element)
		{
			foreach (var attribute in element.Attributes)
			{
				attribute.Accept(this);
			}

			return new AttributeDataBinder(attributeContracts);
		}

		public override IDataScopeContract Visit(AttributeNode attributeNode)
		{
			var value = attributeNode.Value.Accept(this);
			attributeContracts.Add(attributeNode.Name, value);

			return value;
		}

		public override IDataScopeContract Visit(ConstantAttributeContent attributeContent)
		{
			return new ConstantDataScopeContract(attributeContent.Text);
		}

		public override IDataScopeContract Visit(AttributeContentStatement constantAttributeContent)
		{
			return ScopeEmitter.Bind(_dataContract, constantAttributeContent.Expression);
		}

		public Expression HandleAttributeContent(ConstantAttributeContent attributeContent)
		{
			return Expression.New(typeof(ConstantDataScopeContract).GetTypeInfo().GetConstructor(new[] { typeof(object) }), Expression.Constant(attributeContent.Text));
		}

		public Expression HandleElement(Element element, INodeVisitor<Expression> visitor)
		{
			_dictionary = Expression.Variable(DictionaryType);
			var expressions = new List<Expression>();
			expressions.Add(Expression.Assign(_dictionary, Expression.New(DictionaryType)));

			expressions.AddRange(element.Attributes.Select(e => e.Accept(visitor)).ToList());
			expressions.Add(_dictionary);

			return Expression.Block(new [] { _dictionary }, expressions);
		}

		public IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, Expression valueEmitter)
		{
			string name = attributeNode.Name;
			yield return Expression.Call(_dictionary, DictionaryType.GetTypeInfo().GetMethod("Add", new[] {typeof(string), typeof(IDataScopeContract)}), Expression.Constant(name), valueEmitter);
		}

		public Expression HandleCall(Expression callExpression)
		{
			return callExpression;
		}

		public Expression HandleTextNode(TextNode textNode)
		{
			throw new NotSupportedException();
		}

		public Expression HandleDocument(List<Expression> expressions)
		{
			throw new NotSupportedException();
		}

		public Expression HandleCompositeAttribute(CompositeAttributeContent compositeAttributeContent, INodeVisitor<Expression> visitor)
		{
			var expressions = new List<Expression>();
			expressions.AddRange(compositeAttributeContent.ContentParts.Select(s => s.Accept(visitor)));

			var method = typeof(StringBuilder).GetTypeInfo().GetMethod("ToString", new Type[0]);
			expressions.Add(Expression.Call(_dictionary, method));

			return Expression.Block(new[] { _dictionary }, expressions);
		}
	}
}