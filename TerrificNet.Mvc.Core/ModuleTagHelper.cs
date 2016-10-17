using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Mvc.Core
{
	public class ModuleTagHelper : ITagHelper
	{
		private readonly IControllerFactory _controllerFactory;
		private readonly IActionDescriptorCollectionProvider _actionCollection;
		private readonly IHttpContextAccessor _accessor;

		public ModuleTagHelper(IControllerFactory controllerFactory, IActionDescriptorCollectionProvider actionCollection, IHttpContextAccessor accessor)
		{
			_controllerFactory = controllerFactory;
			_actionCollection = actionCollection;
			_accessor = accessor;
		}

		public HelperBinderResult FindByName(Element element)
		{
			if (element.TagName.StartsWith("mod:"))
			{
				var name = element.TagName.Remove(0, "mod:".Length);
				var actionDescriptor = FindActionDescriptor(name);
				return new ModuleTagHelperBinderResult(actionDescriptor, _controllerFactory, _accessor);
			}

			return null;
		}

		internal class ModuleTagHelperBinderResult : HelperBinderResult
		{
			private readonly IControllerFactory _controllerFactory;
			private readonly IHttpContextAccessor _accessor;

			internal ControllerActionDescriptor ActionDescriptor { get; }

			public ModuleTagHelperBinderResult(ControllerActionDescriptor actionDescriptor, IControllerFactory controllerFactory, IHttpContextAccessor accessor)
			{
				_controllerFactory = controllerFactory;
				_accessor = accessor;
				ActionDescriptor = actionDescriptor;
			}

			public override Expression CreateExpression(HelperParameters helperParameters)
			{
				var viewResult = CreateViewResultExpression();
				return CreateExpressionFromViewResult(viewResult, helperParameters.CompilerExtensions.Emitter);
			}

			internal Expression CreateExpressionFromViewResult(Expression actionResultExpression, IEmitter emitter)
			{
				var actionContext = new ActionContext(_accessor.HttpContext, new RouteData(), ActionDescriptor);

				var viewResultExpression = Expression.ConvertChecked(actionResultExpression, typeof(ViewResult));
				var convertedExpression = Expression.Call(viewResultExpression, typeof(ViewResult).GetMethod("Execute"), Expression.Constant(emitter), emitter.RendererExpression, Expression.Constant(actionContext));

				return convertedExpression;
			}

			internal Expression CreateViewResultExpression()
			{
				var info = GetType().GetTypeInfo().GetMethod("Invoke");

				var createExpression = Expression.Call(Expression.Constant(this), info, Expression.Constant(ActionDescriptor));
				return Expression.Call(Expression.ConvertChecked(createExpression, ActionDescriptor.MethodInfo.DeclaringType), ActionDescriptor.MethodInfo);
			}

			public object Invoke(ControllerActionDescriptor actionDescriptor)
			{
				var controllerContext = new ControllerContext { ActionDescriptor = actionDescriptor, HttpContext = _accessor.HttpContext };
				var controller = _controllerFactory.CreateController(controllerContext);

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