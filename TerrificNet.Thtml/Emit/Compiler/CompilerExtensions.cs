namespace TerrificNet.Thtml.Emit.Compiler
{
	public class CompilerExtensions
	{
		private readonly AggregatedHelperBinder _helperBinder = new AggregatedHelperBinder();

		private readonly AggregatedTagHelper _tagHelper = new AggregatedTagHelper();

		public IHelperBinder HelperBinder => _helperBinder;

		public ITagHelper TagHelper => _tagHelper;

		internal IOutputExpressionEmitter OutputEmitter { get; }

		public IEmitter Emitter { get; }

		public static readonly CompilerExtensions Default = new CompilerExtensions();

		private CompilerExtensions()
		{
		}

		private CompilerExtensions(AggregatedHelperBinder helperBinder, AggregatedTagHelper tagHelper, IEmitter emitter)
		{
			_helperBinder = helperBinder;
			Emitter = emitter;
			_tagHelper = tagHelper;
			OutputEmitter = emitter?.OutputExpressionEmitter;
		}

		public CompilerExtensions AddHelperBinder(IHelperBinder helperBinder)
		{
			return new CompilerExtensions(_helperBinder.AddBinder(helperBinder), _tagHelper, Emitter);
		}

		public CompilerExtensions WithEmitter(IEmitter emitter)
		{
			return new CompilerExtensions(_helperBinder, _tagHelper, emitter);
		}

		public CompilerExtensions AddTagHelper(ITagHelper tagHelper)
		{
			return new CompilerExtensions(_helperBinder, _tagHelper.AddHelper(tagHelper), Emitter);
		}
	}
}
