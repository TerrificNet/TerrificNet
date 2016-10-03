using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class CompilerExtensions
	{
		private readonly AggregatedHelperBinder _helperBinder = new AggregatedHelperBinder();

		public IHelperBinder HelperBinder => _helperBinder;

		public ITagHelper TagHelper { get; }

		internal IOutputExpressionEmitter OutputEmitter { get; }

		public static readonly CompilerExtensions Default = new CompilerExtensions();

		private CompilerExtensions()
		{
			TagHelper = new NullTagHelper();
		}

		private CompilerExtensions(AggregatedHelperBinder helperBinder, ITagHelper tagHelper, IOutputExpressionEmitter emitter)
		{
			_helperBinder = helperBinder;
			TagHelper = tagHelper;
			OutputEmitter = emitter;
		}

		public CompilerExtensions AddHelperBinder(IHelperBinder helperBinder)
		{
			return new CompilerExtensions(_helperBinder.AddBinder(helperBinder), TagHelper, OutputEmitter);
		}

		public CompilerExtensions WithEmitter(IOutputExpressionEmitter emitter)
		{
			return new CompilerExtensions(_helperBinder, TagHelper, emitter);
		}

		public CompilerExtensions AddTagHelper(ITagHelper tagHelper)
		{
			return new CompilerExtensions(_helperBinder, tagHelper, OutputEmitter);
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
