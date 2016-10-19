using System;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Mvc.Core
{
	public class MvcRenderingContext : IRenderingContext
	{
		private readonly ActionContext _context;

		public MvcRenderingContext(IOutputBuilder outputBuilder, ActionContext context)
		{
			_context = context;
			OutputBuilder = outputBuilder;
		}

		public IServiceProvider ServiceProvider => _context.HttpContext.RequestServices;

		public IOutputBuilder OutputBuilder { get; }

		public bool TryGetData<T>(string key, out T obj)
		{
			obj = default(T);
			return false;
		}
	}
}