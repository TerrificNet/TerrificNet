using System;
using System.Collections.Generic;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class EmitNodeVisitor : INodeVisitor
    {
        private readonly EmitExpressionVisitor _expressionVisitor;
        private readonly Stack<List<IListEmitter<VTree>>> _elements = new Stack<List<IListEmitter<VTree>>>();
        private readonly Stack<List<IListEmitter<VProperty>>> _properties = new Stack<List<IListEmitter<VProperty>>>();
        private IEmitterRunnable<VPropertyValue> _propertyValueEmitter;

        private List<IListEmitter<VTree>> Scope => _elements.Peek();

	    public EmitNodeVisitor(IDataBinder dataBinder, IHelperBinder helperBinder) 
        {
			_expressionVisitor = new EmitExpressionVisitor(dataBinder, helperBinder);
		}

		public IEmitterRunnable<VNode> DocumentFunc { get; private set; }

		public void Visit(Element element)
	    {
			EnterScope();
			_properties.Push(new List<IListEmitter<VProperty>>());

		    foreach (var attribute in element.Attributes)
		    {
				attribute.Accept(this);
		    }
		    foreach (var node in element.ChildNodes)
		    {
				node.Accept(this);
		    }

			var emitter = EmitterNode.Many(LeaveScope());
			var attributeEmitter = EmitterNode.Many(_properties.Pop());
			Scope.Add(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VElement(element.TagName, attributeEmitter.Execute(d, r), emitter.Execute(d, r)))));
		}

		public void Visit(TextNode textNode)
        {
            Scope.Add(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(textNode.Text))));
        }

		public void Visit(Statement statement)
	    {
			_expressionVisitor.EnterScope(statement.Expression);
			EnterScope();

		    foreach (var childNode in statement.ChildNodes)
		    {
				childNode.Accept(this);
		    }
			
			var childEmitter = EmitterNode.Many(LeaveScope());
			var listEmitter = _expressionVisitor.LeaveTreeScope(statement.Expression, childEmitter);

			Scope.Add(listEmitter);
		}

		public void Visit(AttributeNode attributeNode)
	    {
			attributeNode.Value.Accept(this);

			if (_propertyValueEmitter == null)
				_propertyValueEmitter = EmitterNode.Lambda<VPropertyValue>((d, r) => null);

			var valueEmitter = _propertyValueEmitter;
			_properties.Peek().Add(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VProperty(attributeNode.Name, valueEmitter.Execute(d, r)))));

			_propertyValueEmitter = null;
		}

		public void Visit(AttributeContentStatement constantAttributeContent)
        {
            _expressionVisitor.EnterScope(constantAttributeContent.Expression);
            var emitter = _expressionVisitor.LeavePropertyValueScope(constantAttributeContent.Expression);

            _propertyValueEmitter = EmitterNode.Lambda((d, r) => GetPropertyValue(emitter, d, r));
        }

		public void Visit(ConstantAttributeContent attributeContent)
        {
            _propertyValueEmitter = EmitterNode.Lambda((d, r) => new StringVPropertyValue(attributeContent.Text));
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

        private void EnterScope()
        {
            _elements.Push(new List<IListEmitter<VTree>>());
        }

        private List<IListEmitter<VTree>> LeaveScope()
        {
            return _elements.Pop();
        }

	    public void Visit(Document document)
	    {
			EnterScope();

		    foreach (var node in document.ChildNodes)
		    {
			    node.Accept(this);
		    }

			var emitter = EmitterNode.Many(LeaveScope());
			DocumentFunc = EmitterNode.Lambda((d, r) => new VNode(emitter.Execute(d, r)));
	    }
    }
}