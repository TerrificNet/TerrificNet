using System;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Mvc.Core
{
	public class MvcRenderingContext : IRenderingContext
	{
		public MvcRenderingContext(IOutputBuilder outputBuilder, ActionContext actionContext)
		{
			ActionContext = actionContext;
			OutputBuilder = outputBuilder;
		}

		public ActionContext ActionContext { get; }

		public IServiceProvider ServiceProvider => ActionContext.HttpContext.RequestServices;

		public IOutputBuilder OutputBuilder { get; }

		public bool TryGetData<T>(string key, out T obj)
		{
			obj = default(T);
			return false;
		}
	}
}