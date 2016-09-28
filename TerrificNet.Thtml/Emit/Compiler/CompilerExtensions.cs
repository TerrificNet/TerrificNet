using TerrificNet.Thtml.Binding;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class CompilerExtensions
	{
		public IHelperBinder HelperBinder { get; }

		public ITagHelper TagHelper { get; }

		public IOutputExpressionEmitter OutputEmitter { get; }

		public CompilerExtensions()
		{
		}

		private CompilerExtensions(IHelperBinder helperBinder, ITagHelper tagHelper, IOutputExpressionEmitter emitter)
		{
			HelperBinder = helperBinder;
			TagHelper = tagHelper;
			OutputEmitter = emitter;
		}

		public CompilerExtensions AddHelperBinder(IHelperBinder helperBinder)
		{
			return new CompilerExtensions(helperBinder ?? new NullHelperBinder(), this.TagHelper, this.OutputEmitter);
		}

		public CompilerExtensions WithEmitter(IOutputExpressionEmitter emitter)
		{
			return new CompilerExtensions(this.HelperBinder, this.TagHelper, emitter);
		}
	}
}
