using System;
using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class EmitNodeVisitor : EmitNodeVisitorBase<VTree>
	{
        public EmitNodeVisitor(IDataScope dataScope, IHelperBinder helperBinder) : base(dataScope, helperBinder)
        {
        }

        public IEmitterRunnable<VNode> DocumentFunc { get; private set; }

		public override IListEmitter<VTree> Visit(Element element)
		{
		    var attributeVisitor = new PropertyEmitter(DataScope, HelperBinder);

		    var properties = element.Attributes.Select(attribute => attribute.Accept(attributeVisitor)).ToList();
		    var elements = element.ChildNodes.Select(node => node.Accept(this)).ToList();

		    var emitter = EmitterNode.Many(elements);
			var attributeEmitter = EmitterNode.Many(properties);

            return EmitterNode.AsList(
                EmitterNode.Lambda((d, r) => new VElement(element.TagName, attributeEmitter.Execute(d, r), emitter.Execute(d, r))));
		}

        public override IListEmitter<VTree> Visit(TextNode textNode)
		{
			return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(textNode.Text)));
		}

        public override IListEmitter<VTree> Visit(Statement statement)
        {
            var expression = statement.Expression;
            return HandleStatement(expression, statement.ChildNodes);
        }

        protected override INodeVisitor<IListEmitter<VTree>> CreateVisitor(IDataScope childScope)
        {
            return new EmitNodeVisitor(childScope, HelperBinder);
        }

        public override IListEmitter<VTree> Visit(Document document)
		{
		    var elements = document.ChildNodes.Select(node => node.Accept(this)).ToList();

		    var emitter = EmitterNode.Many(elements);
			DocumentFunc = EmitterNode.Lambda((d, r) => new VNode(emitter.Execute(d, r)));

            return emitter;
        }

        public override IListEmitter<VTree> Visit(UnconvertedExpression unconvertedExpression)
		{
			return unconvertedExpression.Expression.Accept(this);
		}

        public override IListEmitter<VTree> Visit(MemberExpression memberExpression)
		{
		    var scope = ScopeEmitter.Bind(DataScope, memberExpression);

            var evaluator = scope.BindString();
			return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(evaluator.Evaluate(d))));
		}
	}
}