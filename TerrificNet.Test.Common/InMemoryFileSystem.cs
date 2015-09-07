using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerrificNet.ViewEngine.IO;

namespace TerrificNet.Test.Common
{
    public class InMemoryFileSystem : IFileSystem
    {
        private readonly Dictionary<string, DummyFileInfo> _files;
        private readonly List<Subscription<FileChangeEventArgs>> _directorySubscription = new List<Subscription<FileChangeEventArgs>>();

        public InMemoryFileSystem(IEnumerable<string> files)
        {
            _files = files.ToDictionary(f => f, f => new DummyFileInfo(PathInfo.Create(f)));
        }

        public PathInfo BasePath { get; }
        public bool DirectoryExists(PathInfo directory)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PathInfo> DirectoryGetFiles(PathInfo directory, string fileExtension)
        {
            throw new NotImplementedException();
        }

        public Stream OpenRead(PathInfo filePath)
        {
            throw new NotImplementedException();
        }

        public Stream OpenWrite(PathInfo filePath)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(PathInfo filePath)
        {
            throw new NotImplementedException();
        }

        public void RemoveFile(PathInfo filePath)
        {
            var key = filePath.ToString();
            var item = _files[key];
            _files.Remove(key);

            foreach (var dirSubscription in _directorySubscription)
            {
                if (dirSubscription.Pattern.IsMatch(filePath))
                    dirSubscription.Handler(new FileChangeEventArgs(item, FileChangeType.Deleted));
            }
        }

        public void CreateDirectory(PathInfo directory)
        {
            throw new NotImplementedException();
        }

        public Stream OpenReadOrCreate(PathInfo filePath)
        {
            throw new NotImplementedException();
        }

        public IPathHelper Path { get; }
        public bool SupportsSubscribe => true;

        private class Subscription<T> : IDisposable
        {
            private readonly Action<Subscription<T>> _cleanup;
            public GlobPattern Pattern { get; set; }
            public Action<T> Handler { get; }

            public Subscription(GlobPattern pattern, Action<T> handler, Action<Subscription<T>> cleanup)
            {
                _cleanup = cleanup;
                Pattern = pattern;
                Handler = handler;
            }

            public void Dispose()
            {
                _cleanup(this);
            }
        }

        public IDisposable Subscribe(GlobPattern pattern, Action<FileChangeEventArgs> handler)
        {
            var subscription = new Subscription<FileChangeEventArgs>(pattern, handler,
                s => _directorySubscription.Remove(s));

            _directorySubscription.Add(subscription);
            return subscription;
        }

        public IFileInfo GetFileInfo(PathInfo filePath)
        {
            DummyFileInfo info;
            if (_files.TryGetValue(filePath.ToString(), out info))
                return info;

            return null;
        }

        public IEnumerable<IFileInfo> GetFiles(GlobPattern pattern)
        {
            return _files.Where(d => pattern.IsMatch(d.Value.FilePath)).Select(d => d.Value);
        }

        private class DummyFileInfo : IFileInfo
        {
            public DummyFileInfo(PathInfo filePath)
            {
                this.FilePath = filePath;
                this.Etag = filePath.ToString();
            }

            public PathInfo FilePath { get; }

            public string Etag { get; }
        }

        public void Touch(string filePath)
        {
            if (_directorySubscription.Count == 0)
                return;

            DummyFileInfo info;
            if (_files.TryGetValue(filePath, out info))
            {
                foreach (var dirSubscription in _directorySubscription)
                {
                    if (dirSubscription.Pattern.IsMatch(info.FilePath))
                        dirSubscription.Handler(new FileChangeEventArgs(info, FileChangeType.Changed));
                }
            }
        }

        public void Add(string filePath)
        {
            var pathInfo = PathInfo.Create(filePath);
            var fileInfo = new DummyFileInfo(pathInfo);
            _files.Add(filePath, fileInfo);

            foreach (var dirSubscription in _directorySubscription)
            {
                if (dirSubscription.Pattern.IsMatch(pathInfo))
                    dirSubscription.Handler(new FileChangeEventArgs(fileInfo, FileChangeType.Created));
            }
        }
    }
}