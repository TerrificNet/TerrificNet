namespace TerrificNet.ViewEngine.IO
{
    public interface IFileInfo
    {
        PathInfo FilePath { get; }
        string Etag { get; }
    }
}