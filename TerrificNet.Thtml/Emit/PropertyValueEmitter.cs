using System;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class ScopeEmitter : NodeVisitorBase<IDataBinder>
    {
        private IDataBinder _dataBinder;

        private ScopeEmitter(IDataBinder dataBinder)
        {
            _dataBinder = dataBinder;
        }

        public override IDataBinder Visit(MemberExpression memberExpression)
        {
            _dataBinder = _dataBinder.Property(memberExpression.Name);
            if (memberExpression.SubExpression != null)
                return memberExpression.SubExpression.Accept(this);

            return _dataBinder;
        }

        public static IDataBinder Bind(IDataBinder binder, MustacheExpression expression)
        {
            var visitor = new ScopeEmitter(binder);
            return expression.Accept(visitor);
        }
    }

    internal class PropertyValueEmitter : NodeVisitorBase<IEmitterRunnable<VPropertyValue>>
    {
        private readonly IDataBinder _dataBinder;

        public PropertyValueEmitter(IDataBinder dataBinder)
        {
            _dataBinder = dataBinder;
        }

        public override IEmitterRunnable<VPropertyValue> Visit(AttributeContentStatement constantAttributeContent)
        {
            return constantAttributeContent.Expression.Accept(this);
        }

        public override IEmitterRunnable<VPropertyValue> Visit(MemberExpression memberExpression)
        {
            var scope = ScopeEmitter.Bind(_dataBinder, memberExpression);

            IEvaluator<string> evaluator;
            if (!scope.TryCreateEvaluation(out evaluator))
                throw new Exception();

            return EmitterNode.Lambda((d, r) => new StringVPropertyValue(evaluator.Evaluate(d)));
        }

        public override IEmitterRunnable<VPropertyValue> Visit(ConstantAttributeContent attributeContent)
        {
            return EmitterNode.Lambda((d, r) => new StringVPropertyValue(attributeContent.Text));
        }

        private static VPropertyValue GetPropertyValue(IListEmitter<VPropertyValue> emitter, IDataContext dataContext, IRenderingContext renderingContext)
        {
            var stringBuilder = new StringBuilder();
            foreach (var emit in emitter.Execute(dataContext, renderingContext))
            {
                var stringValue = emit as StringVPropertyValue;
                if (stringValue == null)
                    throw new Exception($"Unsupported property value {emit.GetType()}.");

                stringBuilder.Append(stringValue.Value);
            }

            return new StringVPropertyValue(stringBuilder.ToString());
        }
    }
}