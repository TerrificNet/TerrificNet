using System;
using System.IO;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class StreamPropertyEmitter : EmitNodeVisitorBase<StreamWriterHandler>
	{
		public StreamPropertyEmitter(IDataScope dataScope, IHelperBinder helperBinder) : base(dataScope, helperBinder)
		{
		}

		public override IListEmitter<StreamWriterHandler> Visit(AttributeNode attributeNode)
		{
			var valueVisitor = new PropertyValueEmitter(DataScope, HelperBinder);
			var valueEmitter = attributeNode.Value.Accept(valueVisitor);

			if (valueEmitter == null)
				valueEmitter = EmitterNode.AsList(EmitterNode.Lambda<VPropertyValue>((d, r) => null));

			return EmitterNode.AsList(EmitterNode.Lambda<StreamWriterHandler>((d, r) => (writer =>
			{
				writer.Write(" {0}=\"", attributeNode.Name);
				GetPropertyValue(writer, valueEmitter, d, r);
				writer.Write("\"");
			})));
		}

		public override IListEmitter<StreamWriterHandler> Visit(AttributeStatement attributeStatement)
		{
			return HandleStatement(attributeStatement.Expression, attributeStatement.ChildNodes);
		}

		private static void GetPropertyValue(TextWriter writer, IListEmitter<VPropertyValue> emitter, IDataContext dataContext, IRenderingContext renderingContext)
		{
			foreach (var emit in emitter.Execute(dataContext, renderingContext))
			{
				var stringValue = emit as StringVPropertyValue;
				if (stringValue == null)
					throw new Exception($"Unsupported property value {emit.GetType()}.");

				writer.Write(stringValue.Value);
			}
		}

		protected override INodeVisitor<IListEmitter<StreamWriterHandler>> CreateVisitor(IDataScope childScope)
		{
			return new StreamPropertyEmitter(childScope, HelperBinder);
		}
	}
}