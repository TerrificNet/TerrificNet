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
		private readonly IViewDiscovery _viewDiscovery;

		public MixinTagHelper(CompilerService compilerService, IViewDiscovery viewDiscovery)
		{
			_compilerService = compilerService;
			_viewDiscovery = viewDiscovery;
		}

		public HelperBinderResult FindByName(Element element)
		{
			if (element.TagName.StartsWith("mixin:"))
			{
				var partialName = element.TagName.Remove(0, "mixin:".Length);
				var document = _compilerService.Parse(_viewDiscovery.FindPartial(partialName)).Result;

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

			public override void Visit(HelperParameters helperParameters)
			{
				var subContract = _element.Accept(new FillDictionaryOutputVisitor(helperParameters.ScopeContract));

				var helperBinderExtension = new BodyHelperBinder(helperParameters.Visitor, _element.ChildNodes);
				var compilerExtensions = helperParameters.CompilerExtensions
					.AddHelperBinder(helperBinderExtension);

				var visitor = helperParameters.Visitor
					.ChangeContract(subContract)
					.ChangeExtensions(compilerExtensions);

				visitor.Visit(_document);
			}
		}

		private class BodyHelperBinder : IHelperBinder
		{
			private readonly INodeCompilerVisitor _visitor;
			private readonly IReadOnlyList<Node> _childNodes;

			public BodyHelperBinder(INodeCompilerVisitor visitor, IReadOnlyList<Node> childNodes)
			{
				_visitor = visitor;
				_childNodes = childNodes;
			}

			public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
			{
				if ("body".Equals(helper, StringComparison.OrdinalIgnoreCase))
				{
					return HelperBinderResult.Create(param => _visitor.Visit(_childNodes));
				}

				return null;
			}
		}
	}
}