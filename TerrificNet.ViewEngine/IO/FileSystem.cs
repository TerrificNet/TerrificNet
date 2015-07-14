using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TerrificNet.ViewEngine.IO
{
	public class FileSystem : IFileSystem
	{
		private static readonly IPathHelper PathHelper = new FilePathHelper();

		private readonly PathInfo _basePath;
		private readonly List<LookupFileSystemSubscription> _subscriptions = new List<LookupFileSystemSubscription>();
		private readonly HashSet<LookupDirectoryFileSystemSubscription> _directorySubscriptions = new HashSet<LookupDirectoryFileSystemSubscription>();

		private HashSet<PathInfo> _fileInfo;
		private HashSet<PathInfo> _directoryInfo;
		private Dictionary<PathInfo, FileInfo> _fileInfoCache = new Dictionary<PathInfo, FileInfo>();
		private FileSystemWatcher _watcher;
		private readonly string _basePathConverted;

	    private object _lock = new object();

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
			_watcher.Renamed += (sender, a) => HandleFileSystemEvent(a);
		}

		private void HandleFileSystemEvent(FileSystemEventArgs a)
		{
			Initialize();

			var fileInfo = FileInfo.Create(GetRootPath(PathInfo.GetSubPath(_basePath, a.FullPath)));
			if (fileInfo != null || a.ChangeType == WatcherChangeTypes.Deleted)
				NotifySubscriptions(fileInfo);
		}

		private void NotifySubscriptions(IFileInfo file)
		{
			foreach (var subscription in _subscriptions.ToList())
			{
				subscription.Notify(file);
			}

			foreach (var subscription in _directorySubscriptions.ToList())
			{
				if (!subscription.IsMatch(file.FilePath))
					continue;

				subscription.Notify(new[] { file });
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

		public bool SupportsSubscribe
		{
			get { return true; }
		}

		private void Unsubscribe(LookupFileSystemSubscription subscription)
		{
			_subscriptions.Remove(subscription);
		}

		private void Unsubscribe(LookupDirectoryFileSystemSubscription subscription)
		{
			_directorySubscriptions.Remove(subscription);
		}

		public Task<IDisposable> SubscribeAsync(Action<IFileInfo> handler)
		{
			var subscription = new LookupFileSystemSubscription(this, handler);
			_subscriptions.Add(subscription);

			return Task.FromResult<IDisposable>(subscription);
		}

		public Task<IDisposable> SubscribeDirectoryGetFilesAsync(PathInfo prefix, string extension, Action<IEnumerable<IFileInfo>> handler)
		{
			var subscription = new LookupDirectoryFileSystemSubscription(this, prefix, extension, handler);

			_directorySubscriptions.Add(subscription);

			return Task.FromResult<IDisposable>(subscription);
		}

		public IFileInfo GetFileInfo(PathInfo filePath)
		{
			FileInfo result;
			if (_fileInfoCache.TryGetValue(GetRootPath(filePath), out result))
				return result;

			return null;
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
				//return PathInfo.Create(PathUtility.Combine(parts.Select(s => s == null ? null : s.ToString()).ToArray()));
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

		private class LookupFileSystemSubscription : IDisposable
		{
			private FileSystem _parent;
			private Action<IFileInfo> _handler;

			public LookupFileSystemSubscription(FileSystem parent, Action<IFileInfo> handler)
			{
				_parent = parent;
				_handler = handler;
			}

			internal void Notify(IFileInfo file)
			{
				_handler(file);
			}

			public void Dispose()
			{
				_parent.Unsubscribe(this);
				_handler = null;
				_parent = null;
			}
		}

		private class LookupDirectoryFileSystemSubscription : IDisposable
		{
			private FileSystem _parent;
			private Action<IEnumerable<IFileInfo>> _handler;
			private readonly PathInfo _prefix;
			private readonly string _extension;

			public LookupDirectoryFileSystemSubscription(FileSystem parent, PathInfo prefix, string extension, Action<IEnumerable<IFileInfo>> handler)
			{
				_parent = parent;
				_handler = handler;
				_prefix = prefix;
				_extension = extension;
			}

			internal void Notify(IEnumerable<IFileInfo> files)
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
				return path.StartsWith(_prefix) && (string.IsNullOrEmpty(_extension) || path.HasExtension(_extension));
			}
		}

		private class FileInfo : IFileInfo
		{
			public PathInfo FilePath { get; private set; }

			public string Etag { get; private set; }

			private FileInfo(PathInfo filePath, System.IO.FileInfo fileInfo)
			{
				FilePath = filePath;
				Etag = fileInfo.LastWriteTimeUtc.Ticks.ToString("X8");
			}

			public static FileInfo Create(PathInfo filePath)
			{
				var fileInfo = new System.IO.FileInfo(filePath.ToString());
				if (!fileInfo.Exists)
					return null;

				return new FileInfo(filePath, fileInfo);
			}
		}
	}
}