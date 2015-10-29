using System;
using System.Linq;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class EmitStreamVisitor : EmitNodeVisitorBase<StreamWriterHandler>
	{
		public IEmitterRunnable<StreamWriterHandler> DocumentFunc { get; private set; }

		public EmitStreamVisitor(IDataScope dataScope, IHelperBinder helperBinder) : base(dataScope, helperBinder)
		{

		}

		protected override INodeVisitor<IListEmitter<StreamWriterHandler>> CreateVisitor(IDataScope childScope)
		{
			return new EmitStreamVisitor(childScope, HelperBinder);
		}

		public override IListEmitter<StreamWriterHandler> Visit(Document document)
		{
			var elements = document.ChildNodes.Select(node => node.Accept(this)).ToList();

			var emitter = EmitterNode.Many(elements);
			DocumentFunc = EmitterNode.Lambda<StreamWriterHandler>((d, r) => (writer =>
			{
				foreach (var handler in emitter.Execute(d, r))
				{
					handler(writer);
				}
			}));

			return emitter;
		}

		public override IListEmitter<StreamWriterHandler> Visit(Element element)
		{
			var attributeVisitor = new StreamPropertyEmitter(DataScope, HelperBinder);

			var properties = element.Attributes.Select(attribute => attribute.Accept(attributeVisitor)).ToList();
			var elements = element.ChildNodes.Select(node => node.Accept(this)).ToList();

			var emitter = EmitterNode.Many(elements);
			var attributeEmitter = EmitterNode.Many(properties);

			// new VElement(element.TagName, attributeEmitter.Execute(d, r), emitter.Execute(d, r))
			return EmitterNode.AsList(
				EmitterNode.Lambda<StreamWriterHandler>((d, r) => (writer =>
				{
					writer.Write("<{0}", element.TagName);
					foreach (var handler in attributeEmitter.Execute(d, r))
					{
						handler(writer);
					}
					writer.Write(">");
					foreach (var handler in emitter.Execute(d, r))
					{
						handler(writer);
					}
					writer.Write("</{0}>", element.TagName);
				})));
		}

		public override IListEmitter<StreamWriterHandler> Visit(Statement statement)
		{
			var expression = statement.Expression;
			return HandleStatement(expression, statement.ChildNodes);
		}

		public override IListEmitter<StreamWriterHandler> Visit(UnconvertedExpression unconvertedExpression)
		{
			return unconvertedExpression.Expression.Accept(this);
		}

		public override IListEmitter<StreamWriterHandler> Visit(MemberExpression memberExpression)
		{
			var scope = ScopeEmitter.Bind(DataScope, memberExpression);

			var evaluator = scope.RequiresString();
            return EmitterNode.AsList(EmitterNode.Lambda<StreamWriterHandler>((d, r) => (writer => writer.Write(evaluator.Evaluate(d)))));
		}

		public override IListEmitter<StreamWriterHandler> Visit(TextNode textNode)
		{
			return EmitterNode.AsList(EmitterNode.Lambda<StreamWriterHandler>((d, r) => (writer => writer.Write(textNode.Text))));
		}
	}
}