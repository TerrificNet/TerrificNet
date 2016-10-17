using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class AttributeDictionaryVisitor : NodeVisitorBase<string>
	{
		private readonly IDictionary<string, string> _dict;

		public AttributeDictionaryVisitor(IDictionary<string, string> dict)
		{
			_dict = dict;
		}

		public override string Visit(AttributeNode attributeNode)
		{
			_dict.Add(attributeNode.Name, attributeNode.Value.Accept(this));

			return null;
		}

		public override string Visit(ConstantAttributeContent attributeContent)
		{
			return attributeContent.Text;
		}

		public override string Visit(CompositeAttributeContent compositeAttributeContent)
		{
			return compositeAttributeContent.ContentParts.Aggregate(new StringBuilder(), (sb, a) => sb.Append(a.Accept(this))).ToString();
		}
	}
}