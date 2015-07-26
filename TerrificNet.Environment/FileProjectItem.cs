using TerrificNet.ViewEngine.IO;

namespace TerrificNet.Environment
{
    public class FileProjectItem : ProjectItem
    {
        public IFileInfo FileInfo { get; set; }

        public FileProjectItem(ProjectItemKind kind, IFileInfo fileInfo) 
            : base(fileInfo.FilePath.Name.ToString(), kind)
        {
            FileInfo = fileInfo;
        }
    }
}