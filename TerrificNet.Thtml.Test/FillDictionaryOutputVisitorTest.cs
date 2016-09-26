using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Test.Stubs;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class FillDictionaryOutputVisitorTest
	{
		[Fact]
		public void FillDictionaryTest_ConstantAttribute_IncludedInDictionary()
		{
			IEnumerable<ElementPart> attributes = new List<ElementPart>
			{
				new AttributeNode("entry1", new ConstantAttributeContent("val1"))
			};
			var element = new Element("test", attributes);

			var dataContract = new DataScopeContract(BindingPathTemplate.Global);
			var underTest = new FillDictionaryOutputVisitor(dataContract);
			var result = element.Accept(underTest);

			Assert.NotNull(result);

			var propertyContract = result.Property("entry1", SyntaxNodeStub.Node1);
			Assert.NotNull(propertyContract);

			var binding = propertyContract.RequiresString();
			Assert.NotNull(binding);

			var constExpression = Assert.IsType<ConstantExpression>(binding.Expression);
			Assert.Equal("val1", constExpression.Value);
		}

		[Fact]
		public void FillDictionaryTest_FixedBinding_IncludedInDictionary()
		{
			var data = new
			{
				value = "value1"
			};

			IEnumerable<ElementPart> attributes = new List<ElementPart>
			{
				new AttributeNode("entry1", new AttributeContentStatement(new Parsing.Handlebars.MemberExpression("value")))
			};
			var element = new Element("test", attributes);

			var parameter = Expression.Parameter(data.GetType());
			var dataContract = new DataScope(new DataScopeContract(BindingPathTemplate.Global), TypeDataBinder.BinderFromObject(data), parameter);
			var underTest = new FillDictionaryOutputVisitor(dataContract);
			var result = element.Accept(underTest);

			Assert.NotNull(result);
			var contract = result.Property("entry1", SyntaxNodeStub.Node1);
			Assert.NotNull(contract);
			var binding = contract.RequiresString();
			var memberExpression = Assert.IsAssignableFrom<MemberExpression>(binding.Expression);
			Assert.Equal(parameter, memberExpression.Expression);
			Assert.Equal("value", memberExpression.Member.Name);
		}
	}
}
