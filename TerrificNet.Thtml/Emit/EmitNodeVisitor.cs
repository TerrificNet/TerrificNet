using System;
using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class EmitNodeVisitor : EmitNodeVisitorBase<VTree>
	{
        public EmitNodeVisitor(IDataScopeContract dataScopeContract, IHelperBinder helperBinder) : base(dataScopeContract, helperBinder)
        {
        }

        public IEmitterRunnable<VNode> DocumentFunc { get; private set; }

		public override IListEmitter<VTree> Visit(Element element)
		{
		    var attributeVisitor = new PropertyEmitter(DataScopeContract, HelperBinder);

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

        protected override INodeVisitor<IListEmitter<VTree>> CreateVisitor(IDataScopeContract childScopeContract)
        {
            return new EmitNodeVisitor(childScopeContract, HelperBinder);
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
		    var scope = ScopeEmitter.Bind(DataScopeContract, memberExpression);

            var evaluator = scope.RequiresString();
			var emitterNode = EmitterNode.Lambda((d, r) => new VText(evaluator.Evaluate(d)));
			return EmitterNode.AsList(emitterNode);
		}
	}
}