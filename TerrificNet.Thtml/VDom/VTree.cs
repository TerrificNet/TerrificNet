using System.IO;
using System.Text;

namespace TerrificNet.Thtml.VDom
{
    public class VTree
    {
        public override string ToString()
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                var buildStringVisitor = new ToStringVisitor(writer);
                this.Accept(buildStringVisitor);
            }

            return builder.ToString();
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

            public void EndVisit(VNode vNode)
            {
            }

            public void BeginVisit(VElement vElement)
            {
                _textWriter.Write("<");
                _textWriter.Write(vElement.TagName);

                if (vElement.Properties.Count > 0)
                    _textWriter.Write(" ");

                foreach (var property in vElement.Properties)
                {
                    _textWriter.Write(property.Name);
                    _textWriter.Write("=");
                    _textWriter.Write("\"");
                    var stringValue = property.Value as StringVPropertyValue;
                    if (stringValue != null)
                        _textWriter.Write(stringValue.Value);

                    _textWriter.Write("\"");
                }
                _textWriter.Write(">");
            }

            public void EndVisit(VElement vElement)
            {
                _textWriter.Write("</");
                _textWriter.Write(vElement.TagName);
                _textWriter.Write(">");
            }

            public void Visit(VTree vTree)
            {
            }

            public void Visit(VText vText)
            {
                _textWriter.Write(vText.Text);
            }

            public void BeginVisit(VNode vNode)
            {
            }
        }
    }
}
