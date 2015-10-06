using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public class VText : VTree
    {
        public VText(string text)
        {
            Text = text;
        }

        public string Text { get; }

        public override void Accept(IVTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class VNode : VTree
    {
        public VNode(params VTree[] children) : this((IEnumerable<VTree>)children)
        {
        }

        public VNode(IEnumerable<VTree> children)
        {
            Children = children.ToList();
        }

        public IReadOnlyList<VTree> Children { get; }

        public override void Accept(IVTreeVisitor visitor)
        {
            visitor.BeginVisit(this);

            foreach (var child in Children)
                child.Accept(visitor);

            visitor.EndVisit(this);
        }
    }

    public class VElement : VNode
    {
        public VElement(string tagName, params VTree[] children) : this(tagName, (IEnumerable<VTree>)children)
        {
        }

        public VElement(string tagName, IEnumerable<VTree> children) : base(children)
        {
            TagName = tagName;
        }

        public string TagName { get; }

        public override void Accept(IVTreeVisitor visitor)
        {
            visitor.BeginVisit(this);

            foreach (var child in Children)
                child.Accept(visitor);

            visitor.EndVisit(this);
        }
    }

    public class VProperties
    {
    }

    public class VProperty
    {

    }

    public abstract class VPropertyValue
    {
    }

    public class StringVPropertyValue : VPropertyValue
    {

        public StringVPropertyValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

    }

    public class BooleanVPropertyValue : VPropertyValue
    {
        public BooleanVPropertyValue(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

    }

    public class NumberVPropertyValue : VPropertyValue
    {
        public NumberVPropertyValue(int value)
        {
            Value = value;
        }

        public int Value { get; }

    }

}
