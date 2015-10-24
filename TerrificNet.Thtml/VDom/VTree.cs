using System.IO;
using System.Text;

namespace TerrificNet.Thtml.VDom
{
    public abstract class VTree
    {
        public override string ToString()
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                WriteTo(writer);
            }

            return builder.ToString();
        }

        public void WriteTo(TextWriter writer)
        {
            var buildStringVisitor = new ToStringVisitor(writer);
            Accept(buildStringVisitor);
        }

        public virtual void Accept(IVTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        private class ToStringVisitor : IVTreeVisitor
        {
            private readonly TextWriter _textWriter;

            public ToStringVisitor(TextWriter textWriter)
            {
                _textWriter = textWriter;
            }

            public void Visit(VElement vElement)
            {
                _textWriter.Write("<");
                _textWriter.Write(vElement.TagName);

                if (vElement.PropertyList.Count > 0)
                    _textWriter.Write(" ");

                foreach (var property in vElement.PropertyList)
                {
                    var stringValue = property.Value as StringVPropertyValue;
                    var value = stringValue?.Value;

                    if (string.IsNullOrEmpty(value))
                        continue;

                    _textWriter.Write(property.Name);
                    _textWriter.Write("=");
                    _textWriter.Write("\"");
                    _textWriter.Write(value);
                    _textWriter.Write("\"");
                    _textWriter.Write(" ");
                }
                _textWriter.Write(">");

                foreach (var child in vElement.Children)
                {
                    child.Accept(this);
                }

                _textWriter.Write("</");
                _textWriter.Write(vElement.TagName);
                _textWriter.Write(">");
            }

            public void Visit(BooleanVPropertyValue booleanValue)
            {
            }

            public void Visit(NumberVPropertyValue numberValue)
            {
            }

            public void Visit(StringVPropertyValue stringValue)
            {
            }

            public void Visit(VTree vTree)
            {
            }

            public void Visit(VText vText)
            {
                _textWriter.Write(vText.Text);
            }

            public void Visit(VNode vNode)
            {
                foreach (var child in vNode.Children)
                {
                    child.Accept(this);
                }
            }
        }

        public abstract string Type { get; }

        public string Version => "2";
    }
}
