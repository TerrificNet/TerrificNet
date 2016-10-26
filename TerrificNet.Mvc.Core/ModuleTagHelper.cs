using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace TerrificNet.Mvc.Core
{
	public class ModuleTagHelper : ITagHelper
	{
		private readonly IActionDescriptorCollectionProvider _actionCollection;

		public ModuleTagHelper(IActionDescriptorCollectionProvider actionCollection)
		{
			_actionCollection = actionCollection;
		}

		public HelperBinderResult FindByName(Element element)
		{
			if (element.TagName.StartsWith("mod:"))
			{
				var name = element.TagName.Remove(0, "mod:".Length);
				var actionDescriptor = FindActionDescriptor(name);
				return new ModuleTagHelperBinderResult(actionDescriptor);
			}

			return null;
		}

		internal class ModuleTagHelperBinderResult : HelperBinderResult
		{
			internal ControllerActionDescriptor ActionDescriptor { get; }

			public ModuleTagHelperBinderResult(ControllerActionDescriptor actionDescriptor)
			{
				ActionDescriptor = actionDescriptor;
			}

			public override void Visit(HelperParameters helperParameters)
			{
				var viewResult = CreateViewResultExpression(helperParameters.RenderingContextExpression);
				var executionMethod = nameof(ExecuteResult);
				if (helperParameters.CompilerExtensions.SupportAsync)
					executionMethod = nameof(ExecuteResultAsync);

				var ex = CreateExpressionFromViewResult(viewResult, helperParameters.RenderingContextExpression, executionMethod);

				helperParameters.ExpressionBuilder.Add(ex);
			}

			internal Expression CreateExpressionFromViewResult(Expression actionResultExpression, Expression renderingContextExpression, string executionMethod)
			{
				var viewResultExpression = Expression.ConvertChecked(actionResultExpression, typeof(ViewResult));
				var methodInfo = typeof(ModuleTagHelperBinderResult).GetTypeInfo().GetMethod(executionMethod);
				var callExpression = Expression.Call(methodInfo, viewResultExpression, renderingContextExpression, Expression.Constant(ActionDescriptor));

				return callExpression;
			}

			public static void ExecuteResult(ViewResult viewResult, IRenderingContext renderingContext, ActionDescriptor controllerActionDescriptor)
			{
				var childRenderingContext = CreateRenderingContext(renderingContext, controllerActionDescriptor);
				viewResult.ExecuteChildResultAsync(childRenderingContext).Wait();
			}

			public static Task ExecuteResultAsync(ViewResult viewResult, IRenderingContext renderingContext, ActionDescriptor controllerActionDescriptor)
			{
				var childRenderingContext = CreateRenderingContext(renderingContext, controllerActionDescriptor);
				return viewResult.ExecuteChildResultAsync(childRenderingContext);
			}

			private static MvcRenderingContext CreateRenderingContext(IRenderingContext renderingContext, ActionDescriptor controllerActionDescriptor)
			{
				var mvcContext = (MvcRenderingContext) renderingContext;
				var httpContext = mvcContext.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
				var actionContext = new ActionContext(httpContext, new RouteData(), controllerActionDescriptor);
				var childRenderingContext = new MvcRenderingContext(mvcContext.OutputBuilder, actionContext);
				return childRenderingContext;
			}

			internal Expression CreateViewResultExpression(Expression renderingContextExpression)
			{
				var info = GetType().GetTypeInfo().GetMethod("Invoke");

				var createExpression = Expression.Call(info, Expression.Constant(ActionDescriptor), renderingContextExpression);
				return Expression.Call(Expression.ConvertChecked(createExpression, ActionDescriptor.MethodInfo.DeclaringType), ActionDescriptor.MethodInfo);
			}

			public static object Invoke(ControllerActionDescriptor actionDescriptor, IRenderingContext renderingContext)
			{
				var mvcRenderingContext = (MvcRenderingContext) renderingContext;
				var httpContext = mvcRenderingContext.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
				var controllerContext = new ControllerContext { ActionDescriptor = actionDescriptor, HttpContext = httpContext };
				var controllerFactory = mvcRenderingContext.ServiceProvider.GetRequiredService<IControllerFactory>();
				var controller = controllerFactory.CreateController(controllerContext);

				return controller;
			}
		}

		private ControllerActionDescriptor FindActionDescriptor(string name)
		{
			var actionName = "Index";
			var actionDescriptor = _actionCollection.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
				.FirstOrDefault(a => a.ControllerName.Equals(name, StringComparison.OrdinalIgnoreCase) && a.ActionName == actionName);

			if (actionDescriptor == null)
				throw new Exception($"No controller found with name {name} and action {actionName}");

			return actionDescriptor;
		}

	}
}