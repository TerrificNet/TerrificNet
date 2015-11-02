using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
	internal class EmitNodeVisitor : ListEmitNodeVisitor<VTree>
	{
		private readonly ListEmitterFactory<VProperty> _propertyEmitterFactory;

		public EmitNodeVisitor(IDataScopeContract dataScopeContract, IHelperBinder<IListEmitter<VTree>, object> helperBinder)
			: base(dataScopeContract, helperBinder)
		{
			_propertyEmitterFactory = new ListEmitterFactory<VProperty>();
		}

		public override IListEmitter<VTree> Visit(Document document)
		{
			var elements = document.ChildNodes.Select(node => node.Accept(this)).ToList();

			var emitter = Emitter.Many(elements);
			DocumentFunc = Emitter.Lambda((d, r) => new VNode(emitter.Execute(d, r)));

			return emitter;
		}

		public IEmitterRunnable<VTree> DocumentFunc { get; set; }

		public override IListEmitter<VTree> Visit(Element element)
		{
			var attributeVisitor = new PropertyEmitter(DataScopeContract, null);

			var properties = element.Attributes.Select(attribute => attribute.Accept(attributeVisitor)).ToList();
			var elements = element.ChildNodes.Select(node => node.Accept(this)).ToList();

			var emitter = Emitter.Many(elements);
			var attributeEmitter = _propertyEmitterFactory.Many(properties);

			return Emitter.AsList(
				Emitter.Lambda((d, r) => new VElement(element.TagName, attributeEmitter.Execute(d, r), emitter.Execute(d, r))));
		}

		public override IListEmitter<VTree> Visit(TextNode textNode)
		{
			return Emitter.AsList(Emitter.Lambda((d, r) => new VText(textNode.Text)));
		}

		public override IListEmitter<VTree> Visit(UnconvertedExpression unconvertedExpression)
		{
			return unconvertedExpression.Expression.Accept(this);
		}

		public override IListEmitter<VTree> Visit(MemberExpression memberExpression)
		{
			var scope = ScopeEmitter.Bind(DataScopeContract, memberExpression);

			var evaluator = scope.RequiresString();
			return Emitter.AsList(Emitter.Lambda((d, r) => new VText(evaluator.Evaluate(d))));
		}

		protected override INodeVisitor<IListEmitter<VTree>> CreateVisitor(IDataScopeContract childScopeContract)
		{
			return new EmitNodeVisitor(childScopeContract, HelperBinder);
		}
	}
}