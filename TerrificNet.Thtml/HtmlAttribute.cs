namespace TerrificNet.Thtml
{
    public class HtmlAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public HtmlAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}