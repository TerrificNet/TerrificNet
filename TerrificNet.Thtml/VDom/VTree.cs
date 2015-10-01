using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.VDom
{
    public class VTree
    {
    }

    public class VText : VTree
    {
        public VText(string text)
        {
            Text = text;
        }

        public string Text { get; }

    }

    public class VNode : VTree
    {
        public VNode(IEnumerable<VTree> children)
        {
            Children = children.ToList();
        }

        public IReadOnlyList<VTree> Children { get; }
    }

    public class VElement : VNode
    {
        public VElement(string tagName, IEnumerable<VTree> children) : base(children)
        {
            TagName = tagName;
        }

        public string TagName { get; }

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
