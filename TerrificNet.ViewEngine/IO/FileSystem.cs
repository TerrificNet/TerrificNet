using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TerrificNet.ViewEngine.IO
{
	public class FileSystem : IFileSystem
	{
		private static readonly IPathHelper PathHelper = new FilePathHelper();

		private readonly PathInfo _basePath;
		private readonly HashSet<LookupDirectoryFileSystemSubscription> _directorySubscriptions = new HashSet<LookupDirectoryFileSystemSubscription>();

		private HashSet<PathInfo> _fileInfo;
		private HashSet<PathInfo> _directoryInfo;
		private Dictionary<PathInfo, FileInfo> _fileInfoCache = new Dictionary<PathInfo, FileInfo>();
		private FileSystemWatcher _watcher;
		private readonly string _basePathConverted;

	    private readonly object _lock = new object();

		public FileSystem()
			: this(string.Empty)
		{
		}

		public FileSystem(string basePath)
		{
			if (string.IsNullOrEmpty(basePath))
				basePath = Environment.CurrentDirectory;

			_basePath = PathInfo.Create(basePath);
			_basePathConverted = basePath;

			Initialize();
			InitializeWatcher();
		}

		private void Initialize()
		{
		    lock (_lock)
		    {
		        _fileInfo = new HashSet<PathInfo>(
		            Directory.EnumerateFiles(_basePathConverted, "*", SearchOption.AllDirectories)
		                .Select(fileName => PathInfo.GetSubPath(_basePath, fileName)));

		        _fileInfoCache = _fileInfo.ToDictionary(GetRootPath, i => FileInfo.Create(GetRootPath(i)));

		        _directoryInfo = new HashSet<PathInfo>(
		            Directory.EnumerateDirectories(_basePathConverted, "*", SearchOption.AllDirectories)
		                .Select(fileName => PathInfo.GetSubPath(_basePath, fileName)));
		    }
		}

		private void InitializeWatcher()
		{
			_watcher = new FileSystemWatcher(_basePathConverted)
			{
				Path = _basePathConverted,
				EnableRaisingEvents = true,
				IncludeSubdirectories = true
			};
			_watcher.Changed += (sender, a) => HandleFileSystemEvent(a);
			_watcher.Created += (sender, a) => HandleFileSystemEvent(a);
			_watcher.Deleted += (sender, a) => HandleFileSystemEvent(a);
			_watcher.Renamed += (sender, a) => HandleRenameFileSystemEvent(a);
		}

	    private void HandleRenameFileSystemEvent(RenamedEventArgs a)
	    {
	        Initialize();

            var fileInfoOld = FileInfo.Create(GetRootPath(PathInfo.GetSubPath(_basePath, a.OldFullPath)));
            var fileInfo = FileInfo.Create(GetRootPath(PathInfo.GetSubPath(_basePath, a.FullPath)));

            NotifySubscriptions(new FileChangeEventArgs(fileInfoOld, FileChangeType.Deleted));
            NotifySubscriptions(new FileChangeEventArgs(fileInfo, FileChangeType.Created));
        }

		private void HandleFileSystemEvent(FileSystemEventArgs a)
		{
			Initialize();

			var fileInfo = FileInfo.Create(GetRootPath(PathInfo.GetSubPath(_basePath, a.FullPath)));
			if (fileInfo != null || a.ChangeType == WatcherChangeTypes.Deleted)
				NotifySubscriptions(new FileChangeEventArgs(fileInfo, GetChangeType(a.ChangeType)));
		}

	    private FileChangeType GetChangeType(WatcherChangeTypes changeType)
	    {
	        if (changeType == WatcherChangeTypes.Changed)
	            return FileChangeType.Changed;

            if (changeType == WatcherChangeTypes.Created)
                return FileChangeType.Created;

            if (changeType == WatcherChangeTypes.Deleted)
                return FileChangeType.Deleted;

	        throw new ArgumentException("Unknown changeType.", nameof(changeType));
	    }

        private void NotifySubscriptions(FileChangeEventArgs args)
		{
            var filePath = PathInfo.GetSubPath(_basePath, args.FileInfo.FilePath.ToString());

            foreach (var subscription in _directorySubscriptions.ToList())
			{
			    if (!subscription.IsMatch(filePath))
					continue;

				subscription.Notify(args);
			}
		}

		public PathInfo BasePath { get { return _basePath; } }

		public bool DirectoryExists(PathInfo directory)
		{
			return _directoryInfo.Contains(directory);
		}

		public IEnumerable<PathInfo> DirectoryGetFiles(PathInfo directory, string fileExtension)
		{
			var checkDirectory = directory == null;
			var checkExtension = fileExtension == null;
			if (!checkExtension)
				fileExtension = string.Concat(".", fileExtension);

			return
				_fileInfo.Where(
					f => (checkDirectory || f.StartsWith(directory)) &&
						 (checkExtension || f.HasExtension(fileExtension)));
		}

		public Stream OpenRead(PathInfo filePath)
		{
			return new FileStream(GetRootPath(filePath).ToString(), FileMode.Open, FileAccess.Read);
		}

        public Stream OpenReadOrCreate(PathInfo filePath)
		{
			return new FileStream(GetRootPath(filePath).ToString(), FileMode.OpenOrCreate, FileAccess.Read);
		}

		public IPathHelper Path
		{
			get { return PathHelper; }
		}

		public bool SupportsSubscribe => true;

		private void Unsubscribe(LookupDirectoryFileSystemSubscription subscription)
		{
			_directorySubscriptions.Remove(subscription);
		}

	    public IDisposable Subscribe(GlobPattern pattern, Action<FileChangeEventArgs> handler)
		{
			var subscription = new LookupDirectoryFileSystemSubscription(this, pattern, handler);

			_directorySubscriptions.Add(subscription);

			return subscription;
		}

		public IFileInfo GetFileInfo(PathInfo filePath)
		{
			FileInfo result;
			if (_fileInfoCache.TryGetValue(GetRootPath(filePath), out result))
				return result;

			return null;
		}

	    public IEnumerable<IFileInfo> GetFiles(GlobPattern pattern)
	    {
	        return _fileInfo.Where(pattern.IsMatch).Select(GetFileInfo);
	    }

	    public Stream OpenWrite(PathInfo filePath)
		{
			var stream = new FileStream(GetRootPath(filePath).ToString(), FileMode.OpenOrCreate, FileAccess.Write);
			stream.SetLength(0);
			return stream;
		}

		public bool FileExists(PathInfo filePath)
		{
			return _fileInfo.Contains(filePath);
		}

		public void RemoveFile(PathInfo filePath)
		{
			File.Delete(GetRootPath(filePath).ToString());
		    _fileInfo.Remove(filePath);
		}

		public void CreateDirectory(PathInfo directory)
		{
			Directory.CreateDirectory(GetRootPath(directory).ToString());
		}

		private PathInfo GetRootPath(PathInfo part)
		{
			if (part == null)
				return _basePath;

			return Path.Combine(_basePath, part.RemoveStartSlash());
		}


		internal class FilePathHelper : IPathHelper
		{
			public PathInfo Combine(params PathInfo[] parts)
			{
				return PathInfo.Combine(parts);
			}

			public PathInfo GetDirectoryName(PathInfo filePath)
			{
				return filePath.DirectoryName;
			}

			public PathInfo ChangeExtension(PathInfo fileName, string extension)
			{
				return PathInfo.Create(System.IO.Path.ChangeExtension(fileName.ToString(), extension));
			}

			public PathInfo GetFileNameWithoutExtension(PathInfo path)
			{
				return PathInfo.Create(System.IO.Path.GetFileNameWithoutExtension(path.ToString()));
			}

			public string GetExtension(PathInfo path)
			{
				return System.IO.Path.GetExtension(path.ToString());
			}
		}

		private class LookupDirectoryFileSystemSubscription : IDisposable
		{
			private FileSystem _parent;
		    private readonly GlobPattern _pattern;
		    private Action<FileChangeEventArgs> _handler;

		    public LookupDirectoryFileSystemSubscription(FileSystem parent, GlobPattern pattern, Action<FileChangeEventArgs> handler)
			{
				_parent = parent;
			    _pattern = pattern;
			    _handler = handler;
			}

			internal void Notify(FileChangeEventArgs files)
			{
				_handler(files);
			}

			public void Dispose()
			{
				_parent.Unsubscribe(this);
				_handler = null;
				_parent = null;
			}

			public bool IsMatch(PathInfo path)
			{
				return _pattern.IsMatch(path);
			}
		}

		private class FileInfo : IFileInfo
		{
			public PathInfo FilePath { get; }

			public string Etag { get; }

			private FileInfo(PathInfo filePath, System.IO.FileInfo fileInfo)
			{
				FilePath = filePath;
				Etag = fileInfo.LastWriteTimeUtc.Ticks.ToString("X8");
			}

			public static FileInfo Create(PathInfo filePath)
			{
				var fileInfo = new System.IO.FileInfo(filePath.ToString());
				return new FileInfo(filePath, fileInfo);
			}
		}
	}
}