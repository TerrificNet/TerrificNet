using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class AstTest
	{
		[Theory]
		[MemberData(nameof(GetTestCasesArrayFixed))]
		public void IsFixed(SyntaxNode node)
		{
			Assert.True(node.IsFixed);
		}

		[Theory]
		[MemberData(nameof(GetTestCasesArrayNotFixed))]
		public void IsNotFixed(SyntaxNode node)
		{
			Assert.False(node.IsFixed);
		}

		private static IEnumerable<object[]> GetTestCasesArrayFixed()
		{
			return GetTestCasesFixed().Select(o => new[] {o});
		}

		private static IEnumerable<object> GetTestCasesFixed()
		{
			var fixedElement = new Element("test");
			var fixedAttributeValue = new ConstantAttributeContent("test");
			var fixedAttribute = new AttributeNode("test", fixedAttributeValue);

			yield return new TextNode("test");
			yield return fixedElement;
			yield return fixedAttribute;
			yield return new Element("test", new [] { fixedAttribute });
			yield return new Element("test", fixedElement);
			yield return new Element("test", new[] {new AttributeNode("o", new CompositeAttributeContent(fixedAttributeValue, fixedAttributeValue))});
		}

		private static IEnumerable<object[]> GetTestCasesArrayNotFixed()
		{
			return GetTestCasesNotFixed().Select(o => new[] { o });
		}

		private static IEnumerable<object> GetTestCasesNotFixed()
		{
			var notFixedExpression = new MemberExpression("name");
			var notFixedStatement = new Statement(notFixedExpression);
			var notFixedAttributeValue = new AttributeContentStatement(notFixedExpression);
			var compAttribute = new CompositeAttributeContent(notFixedAttributeValue);

			yield return notFixedExpression;
			yield return notFixedStatement;
			yield return new Element("a", notFixedStatement);
			yield return new Element("a", new [] { new AttributeNode("a", notFixedAttributeValue) });
			yield return compAttribute;
		}
	}
}
