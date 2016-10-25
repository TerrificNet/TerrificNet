using System;
using System.Threading.Tasks;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class CompiledAsyncTemplate : IAsyncViewTemplate
	{
		private readonly Func<object, IRenderingContext, Task> _action;

		public CompiledAsyncTemplate(Func<object, IRenderingContext, Task> action)
		{
			_action = action;
		}

		public Task Execute(object data, IRenderingContext renderingContext)
		{
			return _action(data, renderingContext);
		}
	}
}