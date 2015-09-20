namespace TerrificNet.Thtml
{
    public class CreateAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public CreateAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}