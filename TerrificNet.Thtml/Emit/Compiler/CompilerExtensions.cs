using TerrificNet.Thtml.Formatting;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class CompilerExtensions
	{
		private readonly AggregatedHelperBinder _helperBinder = new AggregatedHelperBinder();

		private readonly AggregatedTagHelper _tagHelper = new AggregatedTagHelper();

		public IHelperBinder HelperBinder => _helperBinder;

		public ITagHelper TagHelper => _tagHelper;

		public IOutputExpressionBuilder ExpressionBuilder { get; }

		public static readonly CompilerExtensions Default = new CompilerExtensions();

		public bool SupportAsync { get; private set; }

		public bool SkipUnsupportedBindings { get; private set; }

		private CompilerExtensions()
		{
			SupportAsync = false;
		}

		private CompilerExtensions(AggregatedHelperBinder helperBinder, AggregatedTagHelper tagHelper, IOutputExpressionBuilder output)
		{
			_helperBinder = helperBinder;
			_tagHelper = tagHelper;
			ExpressionBuilder = output;
		}

		internal CompilerExtensions WithAsyncSupport()
		{
			return new CompilerExtensions(_helperBinder, _tagHelper, ExpressionBuilder) { SupportAsync = true, SkipUnsupportedBindings = SkipUnsupportedBindings };
		}

		public CompilerExtensions WithBindingOptions(bool skipUnsupportedBindings)
		{
			return new CompilerExtensions(_helperBinder, _tagHelper, ExpressionBuilder) { SupportAsync = SupportAsync, SkipUnsupportedBindings = skipUnsupportedBindings };
		}

		public CompilerExtensions AddHelperBinder(IHelperBinder helperBinder)
		{
			return new CompilerExtensions(_helperBinder.AddBinder(helperBinder), _tagHelper, ExpressionBuilder) { SupportAsync = SupportAsync, SkipUnsupportedBindings = SkipUnsupportedBindings };
		}

		public CompilerExtensions WithOutput(IOutputExpressionBuilder output)
		{
			return new CompilerExtensions(_helperBinder, _tagHelper, output) { SupportAsync = SupportAsync, SkipUnsupportedBindings = SkipUnsupportedBindings };
		}

		public CompilerExtensions AddTagHelper(ITagHelper tagHelper)
		{
			return new CompilerExtensions(_helperBinder, _tagHelper.AddHelper(tagHelper), ExpressionBuilder) { SupportAsync = SupportAsync, SkipUnsupportedBindings = SkipUnsupportedBindings };
		}
	}
}
