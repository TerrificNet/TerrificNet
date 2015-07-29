using System.IO;
using TerrificNet.ViewEngine.IO;

namespace TerrificNet.Environment
{
    public class FileProjectItem : ProjectItem
    {
        private readonly IFileSystem _fileSystem;
        public IFileInfo FileInfo { get; }

        public FileProjectItem(string kind, IFileInfo fileInfo, IFileSystem fileSystem) 
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