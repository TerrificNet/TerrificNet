using System.IO;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Hyperscript
{
    public class VHyperscriptRenderer
    {
        public void Render(VTree tree, TextWriter writer)
        {
            tree.Accept(new VTreeVisitor(writer));
        }

        private class VTreeVisitor : IVTreeVisitor
        {
            private const char Apostroph = '\'';
            private readonly TextWriter _writer;

            public VTreeVisitor(TextWriter writer)
            {
                _writer = writer;
            }

            public void Visit(VTree vTree)
            {
            }

            public void Visit(VText vText)
            {
            }

            public void Visit(VNode vNode)
            {
                foreach (var child in vNode.Children)
                    child.Accept(this);
            }

            public void Visit(VElement vElement)
            {
                _writer.Write("h('");
                _writer.Write(vElement.TagName);
                _writer.Write(Apostroph);

                RenderProperties(vElement);
                RenderChildren(vElement);

                _writer.Write(")");
            }

            public void Visit(BooleanVPropertyValue booleanValue)
            {
                _writer.Write(booleanValue.Value.ToString().ToLower());
            }

            public void Visit(NumberVPropertyValue numberValue)
            {
                _writer.Write(numberValue.Value.ToString());
            }

            public void Visit(StringVPropertyValue stringValue)
            {
                _writer.Write(Apostroph);
                _writer.Write(stringValue.Value);
                _writer.Write(Apostroph);
            }

            private void RenderChildren(VElement vElement)
            {
                if (vElement.Children.Count > 0)
                {
                    var last = vElement.Children[vElement.Children.Count - 1];

                    _writer.Write(",[");
                    foreach (var child in vElement.Children)
                    {
                        child.Accept(this);
                        if (child != last)
                            _writer.Write(",");
                    }
                    _writer.Write("]");
                }
            }

            private void RenderProperties(VElement vElement)
            {
                if (vElement.Properties.Count > 0)
                {
                    var last = vElement.Properties[vElement.Properties.Count - 1];

                    _writer.Write(",{");
                    foreach (var property in vElement.Properties)
                    {
                        _writer.Write(property.Name);
                        _writer.Write(":");
                        property.Value.Accept(this);

                        if (property != last)
                            _writer.Write(",");
                    }
                    _writer.Write("}");
                }
            }
        }
    }
}