using System.IO;
using TerrificNet.ViewEngine.IO;

namespace TerrificNet.Environment
{
    public class FileProjectItem : ProjectItem
    {
        private readonly IFileSystem _fileSystem;
        private readonly PathInfo _subPath;
        public IFileInfo FileInfo { get; }

        public FileProjectItem(string kind, IFileInfo fileInfo, IFileSystem fileSystem) 
            : this(kind, fileInfo, fileSystem, GetSubPath(fileInfo, fileSystem))
        {
            
        }

        public FileProjectItem(string kind, IFileInfo fileInfo, IFileSystem fileSystem, PathInfo subPath) 
            : base(subPath.ToString(), kind)
        {
            _subPath = subPath;
            _fileSystem = fileSystem;
            FileInfo = fileInfo;
        }

        private static PathInfo GetSubPath(IFileInfo fileInfo, IFileSystem fileSystem)
        {
            return PathInfo.GetSubPath(fileSystem.BasePath, fileInfo.FilePath.ToString());
        }

        public override Stream OpenRead()
        {
            return _fileSystem.OpenRead(_subPath);
        }
    }
}