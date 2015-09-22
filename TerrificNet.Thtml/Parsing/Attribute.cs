namespace TerrificNet.Thtml.Parsing
{
    public class Attribute
    {
        public string Name { get; }
        public string Value { get; }

        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}