using System;
using System.Linq;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class PropertyValueEmitter : EmitNodeVisitorBase<VPropertyValue>
    {
        public PropertyValueEmitter(IDataBinder dataBinder, IHelperBinder helperBinder) : base(dataBinder, helperBinder)
        {
        }

        public override IListEmitter<VPropertyValue> Visit(AttributeContentStatement constantAttributeContent)
        {
            return HandleStatement(constantAttributeContent.Expression, constantAttributeContent.Children);
        }

        public override IListEmitter<VPropertyValue> Visit(MemberExpression memberExpression)
        {
            var scope = ScopeEmitter.Bind(DataBinder, memberExpression);

            IEvaluator<string> evaluator;
            if (!scope.TryCreateEvaluation(out evaluator))
                throw new Exception();

            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new StringVPropertyValue(evaluator.Evaluate(d))));
        }

        public override IListEmitter<VPropertyValue> Visit(CompositeAttributeContent compositeAttributeContent)
        {
            return EmitterNode.Many(compositeAttributeContent.ContentParts.Select(p => p.Accept(this)).ToList());
        }

        public override IListEmitter<VPropertyValue> Visit(ConstantAttributeContent attributeContent)
        {
            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new StringVPropertyValue(attributeContent.Text)));
        }

        protected override INodeVisitor<IListEmitter<VPropertyValue>> CreateVisitor(IDataBinder childScope)
        {
            return new PropertyValueEmitter(childScope, HelperBinder);
        }
    }
}