using System;
using System.Collections.Generic;
using System.IO;

namespace TerrificNet.ViewEngine.IO
{
	public interface IFileSystem
	{
		PathInfo BasePath { get; }

		bool DirectoryExists(PathInfo directory);

        [Obsolete("Use GetFiles instead.")]
		IEnumerable<PathInfo> DirectoryGetFiles(PathInfo directory, string fileExtension);
		Stream OpenRead(PathInfo filePath);
		Stream OpenWrite(PathInfo filePath);
		bool FileExists(PathInfo filePath);
		void RemoveFile(PathInfo filePath);
		void CreateDirectory(PathInfo directory);
		Stream OpenReadOrCreate(PathInfo filePath);
		IPathHelper Path { get; }
		bool SupportsSubscribe { get; }
	    IDisposable Subscribe(GlobPattern pattern, Action<FileChangeEventArgs> handler);
		IFileInfo GetFileInfo(PathInfo filePath);

	    IEnumerable<IFileInfo> GetFiles(GlobPattern pattern);
	}

    public class FileChangeEventArgs
    {
        public FileChangeEventArgs(IFileInfo fileInfo, FileChangeType changeType)
        {
            FileInfo = fileInfo;
            ChangeType = changeType;
        }

        public IFileInfo FileInfo { get; }
        public FileChangeType ChangeType { get; }
    }

    public enum FileChangeType
    {
        Created = 1,
        Deleted = 2,
        Changed = 4
    }
}