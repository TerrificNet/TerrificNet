using System.IO;
using System.Threading.Tasks;

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

        public override async Task<Stream> OpenRead()
        {
            if (_contentBuffer == null)
            {
                // TODO: Verify async
                using (var stream = await _content.ReadAsync().ConfigureAwait(false))
                {
                    var contentStream = new MemoryStream();
                    await stream.CopyToAsync(contentStream).ConfigureAwait(false);

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