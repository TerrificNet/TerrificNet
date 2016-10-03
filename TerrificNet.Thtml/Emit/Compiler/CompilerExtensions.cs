using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class CompilerExtensions
	{
		public IHelperBinder HelperBinder { get; }

		public ITagHelper TagHelper { get; }

		public IOutputExpressionEmitter OutputEmitter { get; }

		public static readonly CompilerExtensions Default = new CompilerExtensions();

		private CompilerExtensions()
		{
			HelperBinder = new NullHelperBinder();
			TagHelper = new NullTagHelper();
		}

		private CompilerExtensions(IHelperBinder helperBinder, ITagHelper tagHelper, IOutputExpressionEmitter emitter)
		{
			HelperBinder = helperBinder;
			TagHelper = tagHelper;
			OutputEmitter = emitter;
		}

		public CompilerExtensions AddHelperBinder(IHelperBinder helperBinder)
		{
			if (helperBinder == null)
				return this;

			return new CompilerExtensions(helperBinder, TagHelper, OutputEmitter);
		}

		public CompilerExtensions WithEmitter(IOutputExpressionEmitter emitter)
		{
			return new CompilerExtensions(HelperBinder, TagHelper, emitter);
		}

		public CompilerExtensions AddTagHelper(ITagHelper tagHelper)
		{
			return new CompilerExtensions(HelperBinder, tagHelper, OutputEmitter);
		}
	}

	public class NullTagHelper : ITagHelper
	{
		public HelperBinderResult FindByName(Element element)
		{
			return null;
		}
	}
}
