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

        private EmitNodeVisitor(EmitExpressionVisitor expressionVisitor)
        {
            _expressionVisitor = expressionVisitor;
        }

        public EmitNodeVisitor(IDataBinder dataBinder, IHelperBinder helperBinder) : this(new EmitExpressionVisitor(dataBinder, helperBinder))
        {
        }

		public void Visit(SyntaxNode node)
		{
			var element = node as Element;
			if (element != null)
			{
				VisitElement(element);
				return;
			}

			var document = node as Document;
			if (document != null)
			{
				VisitDocument(document);
				return;
			}

			var textNode = node as TextNode;
			if (textNode != null)
			{
				VisitTextNode(textNode);
                return;
			}

			var statement = node as Statement;
			if (statement != null)
			{
				VisitStatement(statement);
				return;
			}

			var attributeNode = node as AttributeNode;
			if (attributeNode != null)
			{
				VisitAttributeNode(attributeNode);
				return;
			}

			var attributeContentStatement = node as AttributeContentStatement;
			if (attributeContentStatement != null)
			{
				VisitAttributeContentStatement(attributeContentStatement);
				return;
			}

			var constantAttributeContent = node as ConstantAttributeContent;
			if (constantAttributeContent != null)
			{
				VisitConstantAttributeContent(constantAttributeContent);
				return;
			}

			throw new NotImplementedException();
		}

		public IEmitterRunnable<VNode> DocumentFunc { get; private set; }

	    private void VisitElement(Element element)
	    {
			EnterScope();
			_properties.Push(new List<IListEmitter<VProperty>>());

		    foreach (var attribute in element.Attributes)
		    {
			    Visit(attribute);
		    }
		    foreach (var node in element.ChildNodes)
		    {
			    Visit(node);
		    }

			var emitter = EmitterNode.Many(LeaveScope());
			var attributeEmitter = EmitterNode.Many(_properties.Pop());
			Scope.Add(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VElement(element.TagName, attributeEmitter.Execute(d, r), emitter.Execute(d, r)))));
		}
		
        private void VisitTextNode(TextNode textNode)
        {
            Scope.Add(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(textNode.Text))));
        }

	    private void VisitStatement(Statement statement)
	    {
			_expressionVisitor.EnterScope(statement.Expression);
			EnterScope();

		    foreach (var childNode in statement.ChildNodes)
		    {
			    Visit(childNode);
		    }
			
			var childEmitter = EmitterNode.Many(LeaveScope());
			var listEmitter = _expressionVisitor.LeaveTreeScope(statement.Expression, childEmitter);

			Scope.Add(listEmitter);
		}

	    private void VisitAttributeNode(AttributeNode attributeNode)
	    {
			if (_propertyValueEmitter == null)
				_propertyValueEmitter = EmitterNode.Lambda<VPropertyValue>((d, r) => null);

			var valueEmitter = _propertyValueEmitter;
			_properties.Peek().Add(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VProperty(attributeNode.Name, valueEmitter.Execute(d, r)))));

			_propertyValueEmitter = null;
		}

        private void VisitAttributeContentStatement(AttributeContentStatement constantAttributeContent)
        {
            _expressionVisitor.EnterScope(constantAttributeContent.Expression);
            var emitter = _expressionVisitor.LeavePropertyValueScope(constantAttributeContent.Expression);

            _propertyValueEmitter = EmitterNode.Lambda((d, r) => GetPropertyValue(emitter, d, r));
        }

        private void VisitConstantAttributeContent(ConstantAttributeContent attributeContent)
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

	    private void VisitDocument(Document document)
	    {
			EnterScope();

		    foreach (var node in document.ChildNodes)
		    {
			    Visit(node);
		    }

			var emitter = EmitterNode.Many(LeaveScope());
			DocumentFunc = EmitterNode.Lambda((d, r) => new VNode(emitter.Execute(d, r)));
	    }
    }
}