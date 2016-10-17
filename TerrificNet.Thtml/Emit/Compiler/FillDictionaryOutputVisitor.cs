using System.Collections.Generic;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class FillDictionaryOutputVisitor : NodeVisitorBase<IDataScopeContract>
	{
		private readonly IDataScopeContract _dataContract;

		private readonly Dictionary<string, IDataScopeContract> _attributeContracts = new Dictionary<string, IDataScopeContract>();

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

			return new AttributeDataBinder(_attributeContracts);
		}

		public override IDataScopeContract Visit(AttributeNode attributeNode)
		{
			var value = attributeNode.Value.Accept(this);
			_attributeContracts.Add(attributeNode.Name, value);

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
	}
}