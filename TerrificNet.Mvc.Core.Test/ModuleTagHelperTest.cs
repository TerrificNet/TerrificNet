using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;
using Xunit;
using System.Reflection;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Mvc.Core.Test
{
	public class ModuleTagHelperTest
	{
		private readonly IControllerFactory _controllerFactory;
		private IActionDescriptorCollectionProvider _actionCollection;
		private IHttpContextAccessor _accessor;
		private ControllerActionDescriptor _controllerActionDescriptor;

		public ModuleTagHelperTest()
		{
			_controllerActionDescriptor = new ControllerActionDescriptor
			{
				ActionName = "Index",
				ControllerName = "Test",
				MethodInfo = typeof(TestController).GetTypeInfo().GetMethod("Index")
			};
			_controllerFactory = new Mock<IControllerFactory>().Object;

			var mock = new Mock<IActionDescriptorCollectionProvider>();
			mock.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor> {_controllerActionDescriptor}, 1));

			_actionCollection = mock.Object;
			var accessorMock = new Mock<IHttpContextAccessor>();
			accessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
			_accessor = accessorMock.Object;
		}

		[Fact]
		public void ModuleTagHelper_FindByName_ReturnsResult()
		{
			var element = new Element("mod:test");

			var underTest = new ModuleTagHelper(_controllerFactory, _actionCollection, _accessor);
			var result = underTest.FindByName(element);

			Assert.NotNull(result);
			var typedResult = Assert.IsType<ModuleTagHelper.ModuleTagHelperBinderResult>(result);
			Assert.Equal(_controllerActionDescriptor, typedResult.ActionDescriptor);
		}

		[Fact]
		public void ModuleTagHelperBinderResult_CreateExpression_ReturnsResult()
		{
			var underTest = new ModuleTagHelper.ModuleTagHelperBinderResult(_controllerActionDescriptor, _controllerFactory, _accessor);
			var result = underTest.CreateViewResultExpression();

			Assert.NotNull(result);
			Assert.Equal(typeof(IActionResult), result.Type);
		}

		[Fact]
		public void ModuleTagHelperBinderResult_CreateViewExpression_ReturnsCompilerResult()
		{
			var viewResult = new ViewResult("test", new object());

			var inputExpression = Expression.Constant(viewResult, typeof(IActionResult));
			var underTest = new ModuleTagHelper.ModuleTagHelperBinderResult(_controllerActionDescriptor, _controllerFactory, _accessor);

			var emitter = EmitterFactories.VTree.Create();

			var result = underTest.CreateExpressionFromViewResult(inputExpression, emitter.ExpressionBuilder);
			Assert.NotNull(result);
			Assert.Equal(typeof(void), result.Type);
		}

		private class TestController
		{
			public IActionResult Index()
			{
				return null;
			}
		}

	}
}
