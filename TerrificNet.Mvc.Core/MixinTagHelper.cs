using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Mvc.Core
{
	public class MixinTagHelper : ITagHelper
	{
		private readonly CompilerService _compilerService;

		public MixinTagHelper(CompilerService compilerService)
		{
			_compilerService = compilerService;
		}

		public HelperBinderResult FindByName(Element element)
		{
			if (element.TagName.StartsWith("mixin:"))
			{
				var partialName = element.TagName.Remove(0, "mixin:".Length).Replace("-", "");
				var document = _compilerService.Parse($@"D:\projects\TerrificNet\TerrificNet.Sample\components\modules\{partialName}\{partialName}.html").Result;

				return new MixinHelperBinderResult(document, element);
			}

			return null;
		}

		private class MixinHelperBinderResult : HelperBinderResult
		{
			private readonly Element _element;
			private readonly Document _document;

			public MixinHelperBinderResult(Document document, Element element)
			{
				_document = document;
				_element = element;
			}

			public override Expression CreateExpression(HelperParameters helperParameters)
			{
				var children = helperParameters.Visitor.Visit(_element.ChildNodes);

				var subContract = _element.Accept(new FillDictionaryOutputVisitor(helperParameters.ScopeContract));

				var helperBinderExtension = new BodyHelperBinder(children);
				var compilerExtensions = helperParameters.CompilerExtensions
					.AddHelperBinder(helperBinderExtension);

				var visitor = helperParameters.Visitor
					.ChangeContract(subContract)
					.ChangeExtensions(compilerExtensions);

				return visitor.Visit(_document);
			}
		}

		private class BodyHelperBinder : IHelperBinder
		{
			private readonly Expression _expression;

			public BodyHelperBinder(Expression expression)
			{
				_expression = expression;
			}

			public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
			{
				if ("body".Equals(helper, StringComparison.OrdinalIgnoreCase))
				{
					return HelperBinderResult.Create(param => _expression);
				}

				return null;
			}
		}
	}
}