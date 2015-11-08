using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Emit.Vtree;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.TemplateHandler.UI;
using Veil;

namespace TerrificNet.UnityModules
{
	public class ThtmlViewEngine : IViewEngine
	{
		private readonly IModuleRepository _moduleRepository;
		private readonly ITemplateRepository _templateRepository;
		private readonly IModelProvider _modelProvider;

		public ThtmlViewEngine(IModuleRepository moduleRepository, ITemplateRepository templateRepository, IModelProvider modelProvider)
		{
			_moduleRepository = moduleRepository;
			_templateRepository = templateRepository;
			_modelProvider = modelProvider;
		}

		public Task<IView> CreateViewAsync(TemplateInfo templateInfo, Type modelType, IModelBinder modelBinder)
		{
			IDataBinder scope;
			if (modelType == typeof(object))
				scope = new DynamicDataScope();
			else
			{
				ParameterExpression dataContextParameter = Expression.Parameter(modelType);
				scope = TypeDataScope.BinderFromType(dataContextParameter.Type);
			}

			var emitter = CreateEmitter(templateInfo, new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), scope), new BasicHelperBinder(_moduleRepository, _templateRepository, _modelProvider));

			return Task.FromResult<IView>(new ThtmlView(emitter));
		}

		internal static IEmitterRunnable<VTree> CreateEmitter(TemplateInfo templateInfo, IDataScopeContract dataScope, IHelperBinder<IListEmitter<VTree>, object> helperBinder)
		{
			var ast = GetDocument(templateInfo);
			var compiler = new VTreeEmitter();

			var emitter = compiler.Emit(ast, dataScope, new NullHelperBinder<Expression, ExpressionHelperConfig>()); //helperBinder);
			return emitter;
		}

		internal static Document GetDocument(TemplateInfo templateInfo)
		{
			string template;
			using (var reader = new StreamReader(templateInfo.Open()))
			{
				template = reader.ReadToEnd();
			}

			var lexer = new Lexer();
			var tokens = lexer.Tokenize(template);
			var parser = new Parser(new HandlebarsParser());
			var ast = parser.Parse(tokens);
			return ast;
		}

		private class ThtmlView : IView
		{
			private readonly IEmitterRunnable<VTree> _method;

			public ThtmlView(IEmitterRunnable<VTree> method)
			{
				_method = method;
			}

			public void Render(object model, RenderingContext context)
			{
				var result = _method.Execute(model, new RenderingContextAdapter(context));
				context.Writer.Write(result);
			}
		}

		internal class RenderingContextAdapter : IRenderingContext
		{
			public RenderingContext Adaptee { get; }

			public RenderingContextAdapter(RenderingContext adaptee)
			{
				Adaptee = adaptee;
			}

			public bool TryGetData<T>(string key, out T obj)
			{
				return Adaptee.TryGetData(key, out obj);
			}
		}
	}

	public class BasicHelperBinder : IHelperBinder<IListEmitter<VTree>, object>
	{
		private readonly IModuleRepository _moduleRepository;
		private readonly ITemplateRepository _templateRepository;
		private readonly IModelProvider _modelProvider;

		public BasicHelperBinder(IModuleRepository moduleRepository, ITemplateRepository templateRepository, IModelProvider modelProvider)
		{
			_moduleRepository = moduleRepository;
			_templateRepository = templateRepository;
			_modelProvider = modelProvider;
		}

		public HelperBinderResult<IListEmitter<VTree>, object> FindByName(string helper, IDictionary<string, string> arguments)
		{
			//if ("partial".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
			//	return new PartialHelperBinderResult(_templateRepository, arguments["template"]);

			if ("module".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
				return new ModuleHelperBinderResult(_moduleRepository, _modelProvider, arguments["template"], arguments.ContainsKey("skin") ? arguments["skin"] : null);

			//if ("placeholder".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
			//	return new PlaceholderHelperBinderResult(arguments["key"], _templateRepository, _moduleRepository, _modelProvider);

			//if ("grid-cell".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
			//	return new GridHelperBinderResult(arguments.ContainsKey("ratio") ? arguments["ratio"] : null);

			return null;
		}

		public class GridHelperBinderResult : HelperBinderResult<Expression, ExpressionHelperConfig>
		{
			public GridHelperBinderResult(string ration)
			{
			}

			public override Expression CreateEmitter(HelperParameters helperParameters, Expression children)
			{
				return children;
			}
		}
	}

	public class PlaceholderHelperBinderResult : HelperBinderResult<Expression, ExpressionHelperConfig>
	{
		private readonly string _key;
		private readonly ITemplateRepository _templateRepository;
		private readonly IModuleRepository _moduleRepository;
		private readonly IModelProvider _modelProvider;

		public PlaceholderHelperBinderResult(string key, ITemplateRepository templateRepository, IModuleRepository moduleRepository, IModelProvider modelProvider)
		{
			_key = key;
			_templateRepository = templateRepository;
			_moduleRepository = moduleRepository;
			_modelProvider = modelProvider;
		}

		//public override IListEmitter<VTree> CreateEmitter(IOutputExpressionEmitter outputExpressionEmitter, IListEmitter<VTree> children, IHelperBinder<IListEmitter<VTree>, object> helperBinder, IDataScopeContract scopeContract)
		//{
		//	return new PlaceholderEmitter(_key, helperBinder, _templateRepository, _moduleRepository, _modelProvider);
		//}

		private class PlaceholderEmitter : IListEmitter<VTree>
		{
			private readonly string _name;
			private readonly IHelperBinder<IListEmitter<VTree>, object> _helperBinder;
			private readonly ITemplateRepository _templateRepository;
			private readonly IModuleRepository _moduleRepository;
			private readonly IModelProvider _modelProvider;

			public PlaceholderEmitter(string name, IHelperBinder<IListEmitter<VTree>, object> helperBinder, ITemplateRepository templateRepository, IModuleRepository moduleRepository, IModelProvider modelProvider)
			{
				_name = name;
				_helperBinder = helperBinder;
				_templateRepository = templateRepository;
				_moduleRepository = moduleRepository;
				_modelProvider = modelProvider;
			}

			public IEnumerable<VTree> Execute(object context, IRenderingContext renderingContext)
			{
				ViewDefinition definition;
				if (!renderingContext.TryGetData("siteDefinition", out definition))
					throw new InvalidOperationException("The context must contain a siteDefinition to use the placeholder helper.");

				if (definition.Placeholder == null)
					return Enumerable.Empty<VTree>();

				ViewDefinition[] definitions;
				if (!definition.Placeholder.TryGetValue(_name, out definitions))
					return Enumerable.Empty<VTree>();

				return EmitterNode<VTree>.AsList(GetEmitters(definitions, renderingContext)).Execute(context, renderingContext);
			}

			private IEnumerable<IEmitterRunnable<VTree>> GetEmitters(IEnumerable<ViewDefinition> definitions, IRenderingContext renderingContext)
			{
				foreach (var placeholderConfig in definitions)
				{
					var ctx = new RenderingContext(null, ((ThtmlViewEngine.RenderingContextAdapter)renderingContext).Adaptee);
					ctx.Data["siteDefinition"] = placeholderConfig;

					var newCtx = new ThtmlViewEngine.RenderingContextAdapter(ctx);

					var moduleConfig = placeholderConfig as ModuleViewDefinition;
					if (moduleConfig != null)
					{
						var moduleEmitter = ModuleHelperBinderResult.CreateModuleEmitter(_helperBinder,
							_moduleRepository, _modelProvider,
							moduleConfig.Module);

						yield return EmitterNode<VTree>.Lambda((c, r) => moduleEmitter.Execute(c, newCtx));
					}

					var partialConfig = placeholderConfig as PageViewDefinition<object>;
					if (partialConfig != null)
					{
						var res = new PartialHelperBinderResult(_templateRepository, partialConfig.Template);
						var emitters = res.CreateEmitter(_helperBinder, new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), new DynamicDataScope()));
						yield return EmitterNode<VTree>.Lambda((c, r) => emitters.Execute(c, newCtx));
					}
				}
			}


		}

		public override Expression CreateEmitter(HelperParameters helperParameters, Expression children)
		{
			throw new NotImplementedException();
		}
	}

	public class ModuleHelperBinderResult : HelperBinderResult<IListEmitter<VTree>, object>
	{
		private readonly IModuleRepository _templateRepository;
		private readonly IModelProvider _modelProvider;
		private readonly string _module;
		private readonly string _skin;

		public ModuleHelperBinderResult(IModuleRepository templateRepository, IModelProvider modelProvider, string module, string skin)
		{
			_templateRepository = templateRepository;
			_modelProvider = modelProvider;
			_module = module;
			_skin = skin;
		}

		public override IListEmitter<VTree> CreateEmitter(HelperParameters helperParameters, IListEmitter<VTree> children)
		{
			var moduleEmitter = CreateModuleEmitter(helperParameters.HelperBinder, _templateRepository, _modelProvider, _module);
			return EmitterNode<VTree>.AsList(moduleEmitter);
		}

		internal static IEmitterRunnable<VTree> CreateModuleEmitter(IHelperBinder<IListEmitter<VTree>, object> helperBinder, IModuleRepository templateRepository, IModelProvider modelProvider, string module)
		{
			var moduleDescription = templateRepository.GetModuleDefinitionByIdAsync(module).Result;
			var data = modelProvider.GetModelForModuleAsync(moduleDescription, null).Result;
			var template = moduleDescription.DefaultTemplate;

			var context = data;
			var binder = new DynamicDataScope();

			var emitter = ThtmlViewEngine.CreateEmitter(template, new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), binder), helperBinder);
			var moduleEmitter = new ModuleEmitter<VTree>(emitter, context);
			return moduleEmitter;
		}

		private class ModuleEmitter<T> : IEmitterRunnable<T>
		{
			private readonly IEmitterRunnable<T> _adaptee;
			private readonly object _dataContext;

			public ModuleEmitter(IEmitterRunnable<T> adaptee, object dataContext)
			{
				_adaptee = adaptee;
				_dataContext = dataContext;
			}

			public T Execute(object context, IRenderingContext renderingContext)
			{
				return _adaptee.Execute(_dataContext, renderingContext);
			}
		}
	}

	public class PartialHelperBinderResult : HelperBinderResult<Expression, ExpressionHelperConfig>
	{
		private readonly ITemplateRepository _templateRepository;
		private readonly string _templateName;

		public PartialHelperBinderResult(ITemplateRepository templateRepository, string templateName)
		{
			_templateRepository = templateRepository;
			_templateName = templateName;
		}

		internal IEmitterRunnable<VTree> CreateEmitter(IHelperBinder<IListEmitter<VTree>, object> helperBinder, IDataScopeContract scope)
		{
			var template = _templateRepository.GetTemplateAsync(_templateName).Result;
			var emitter = ThtmlViewEngine.CreateEmitter(template, scope, helperBinder);
			return emitter;
		}

		//public override IListEmitter<VTree> CreateEmitter(IOutputExpressionEmitter outputExpressionEmitter, IListEmitter<VTree> children, IHelperBinder<IListEmitter<VTree>, object> helperBinder, IDataScopeContract scopeContract)
		//{
		//	var emitter = CreateEmitter(helperBinder, scopeContract);
		//	return EmitterNode<VTree>.AsList(emitter);
		//}

		public override Expression CreateEmitter(HelperParameters helperParameters, Expression children)
		{
			var visitor = new EmitExpressionVisitor(helperParameters.ScopeContract, helperParameters.HelperBinder, helperParameters.DataContextParameter, helperParameters.OutputExpressionEmitter);

			return visitor.Visit(ThtmlViewEngine.GetDocument(_templateRepository.GetTemplateAsync(_templateName).Result));
		}
	}
}
