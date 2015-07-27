using System.IO;
using System.Threading.Tasks;
using TerrificNet.ViewEngine.IO;

namespace TerrificNet.Environment
{
    public class FileProjectItem : ProjectItem
    {
        private readonly IFileSystem _fileSystem;
        public IFileInfo FileInfo { get; }

        public FileProjectItem(ProjectItemKind kind, IFileInfo fileInfo, IFileSystem fileSystem) 
            : base(fileInfo.FilePath.ToString(), kind)
        {
            _fileSystem = fileSystem;
            FileInfo = fileInfo;
        }

        public override Stream OpenRead()
        {
            return _fileSystem.OpenRead(FileInfo.FilePath);
        }
    }
}