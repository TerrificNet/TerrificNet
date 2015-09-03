using System.IO;

namespace TerrificNet.Environment.Building
{
    internal class DefferedProjectItem : ProjectItem
    {
        private IProjectItemContent _content;
        private byte[] _contentBuffer;

        public DefferedProjectItem(ProjectItemIdentifier identifier, IProjectItemContent content) : base(identifier)
        {
            _content = content;
        }

        public override Stream OpenRead()
        {
            if (_contentBuffer == null)
            {
                // TODO: Verify async
                using (var stream = _content.ReadAsync().Result)
                {
                    var contentStream = new MemoryStream();
                    stream.CopyTo(contentStream);

                    _contentBuffer = contentStream.ToArray();
                }

                _content = null;
            }

            return new MemoryStream(_contentBuffer);
        }

        public void SetDirty(IProjectItemContent content)
        {
            _content = content;
            _contentBuffer = null;
        }
    }
}