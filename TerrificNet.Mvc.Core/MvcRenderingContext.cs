using System;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Mvc.Core
{
	public class MvcRenderingContext : IRenderingContext
	{
		private readonly ActionContext _context;

		public MvcRenderingContext(ActionContext context)
		{
			_context = context;
		}

		public IServiceProvider ServiceProvider => _context.HttpContext.RequestServices;

		public bool TryGetData<T>(string key, out T obj)
		{
			obj = default(T);
			return false;
		}
	}
}