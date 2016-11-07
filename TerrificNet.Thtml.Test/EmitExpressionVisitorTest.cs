using System;
using System.Linq.Expressions;
using Moq;
using Moq.Language;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.Rendering;
using Xunit;
using TerrificNet.Thtml.Test.Extensions;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Test
{
	public class EmitExpressionVisitorTest
	{
		private readonly IRenderingScope _renderingScope;

		public EmitExpressionVisitorTest()
		{
			_renderingScope = new Mock<IRenderingScope>().Object;
		}

		[Fact]
		public void EmitExpressionVisior_Element_NewScope()
		{
			TestSequence(new Document(new Element("div")), 
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Enter()), 
				a => a.Setup(s => s.Leave()).Returns(_renderingScope), 
				a => a.Setup(s => s.Leave()).Returns(_renderingScope));
		}

		[Fact]
		public void EmitExpressionVisior_ElementWithStaticId_NewScopeWithId()
		{
			TestSequence(new Element("div", new [] { new AttributeNode("id", new ConstantAttributeContent("testId"))}),
				a => a.Setup(s => s.Enter(It.Is<IBinding>(b => AssertBinding(b, "testId")))),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope));
		}

		private static bool AssertBinding(IBinding binding, string value)
		{
			Assert.NotNull(binding);
			var exBinding = Assert.IsAssignableFrom<IBindingWithExpression>(binding);
			Assert.NotNull(exBinding.Expression);
			var constEx = Assert.IsAssignableFrom<ConstantExpression>(exBinding.Expression);
			Assert.Equal(value, constEx.Value);

			return true;
		}

		[Fact]
		public void EmitExpressionVisior_ElementWithDynamicId_NewScopeWithId()
		{
			TestSequence(new Element("div", new[] { new AttributeNode("id", new AttributeContentStatement(new MemberExpression("id"))) }),
				a => a.Setup(s => s.Enter(It.Is<IBinding>(e => AssertBinding(e)))),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.UseBinding(It.IsAny<IBinding>())),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope));
		}

		private static bool AssertBinding(IBinding binding)
		{
			Assert.NotNull(binding);
			Assert.Equal(BindingPathTemplate.Global.Property("id"), binding.Path);

			return true;
		}

		[Fact]
		public void EmitExpressionVisitor_Attribute_NewScope()
		{
			TestSequence(new Document(new Element("div", new [] { new AttributeNode("attr", new AttributeContentStatement(new MemberExpression("value")))})),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.UseBinding(It.IsAny<IBinding>())),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope));
		}

		[Fact]
		public void EmitExpressionVisitor_ChildElement_NewScope()
		{
			TestSequence(new Document(new Element("div", new Statement(new MemberExpression("value")), new Element("div"))),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.UseBinding(It.IsAny<IBinding>())),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope),
				a => a.Setup(s => s.Leave()).Returns(_renderingScope));
		}

		private static void TestSequence(Node input, params Action<ISetupConditionResult<IScopedExpressionBuilder>>[] sequence)
		{
			var formatter = new Mock<IOutputExpressionBuilder>();

			var scopedExpressionBuilderMock = new Mock<IScopedExpressionBuilder>(MockBehavior.Strict);

			scopedExpressionBuilderMock.InSequence(sequence);

			var dataScopeContract = new DataScopeContract(BindingPathTemplate.Global);
			var underTest = new EmitExpressionVisitor(dataScopeContract, CompilerExtensions.Default.WithOutput(formatter.Object),
				Expression.Parameter(typeof(IRenderingContext)), scopedExpressionBuilderMock.Object);

			input.Accept(underTest);

			scopedExpressionBuilderMock.VerifyAll();
		}
	}
}
