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

		private CompilerExtensions()
		{
		}

		private CompilerExtensions(AggregatedHelperBinder helperBinder, AggregatedTagHelper tagHelper, IOutputExpressionBuilder output)
		{
			_helperBinder = helperBinder;
			_tagHelper = tagHelper;
			ExpressionBuilder = output;
		}

		public CompilerExtensions AddHelperBinder(IHelperBinder helperBinder)
		{
			return new CompilerExtensions(_helperBinder.AddBinder(helperBinder), _tagHelper, ExpressionBuilder);
		}

		public CompilerExtensions WithOutput(IOutputExpressionBuilder output)
		{
			return new CompilerExtensions(_helperBinder, _tagHelper, output);
		}

		public CompilerExtensions AddTagHelper(ITagHelper tagHelper)
		{
			return new CompilerExtensions(_helperBinder, _tagHelper.AddHelper(tagHelper), ExpressionBuilder);
		}
	}
}
