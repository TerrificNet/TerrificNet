using System;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class CompiledSyncTemplate : IViewTemplate
	{
		private readonly Action<object, IRenderingContext> _action;

		public CompiledSyncTemplate(Action<object, IRenderingContext> action)
		{
			_action = action;
		}

		public void Execute(object data, IRenderingContext renderingContext)
		{
			if (renderingContext == null)
				throw new ArgumentNullException(nameof(renderingContext));

			_action(data, renderingContext);
		}
	}
}