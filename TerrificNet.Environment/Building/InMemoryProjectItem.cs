using System.IO;

namespace TerrificNet.Environment.Building
{
    internal class InMemoryProjectItem : ProjectItem
    {
        private byte[] _content;

        public InMemoryProjectItem(ProjectItemIdentifier identifier, byte[] content) : base(identifier)
        {
            _content = content;
        }

        public override Stream OpenRead()
        {
            return new MemoryStream(_content);
        }

        public void SetContent(byte[] buffer)
        {
            _content = buffer;
        }
    }
}