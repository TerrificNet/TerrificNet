using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerrificNet.ViewEngine.IO;

namespace TerrificNet.Environment
{
    public class Project : IDisposable
    {
        private readonly Dictionary<ProjectItemIdentifier, ProjectItem> _items = new Dictionary<ProjectItemIdentifier, ProjectItem>();
        private readonly List<IProjectObserver> _observers = new List<IProjectObserver>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        public void AddItem(ProjectItem projectItem)
        {
            if (_items.ContainsKey(projectItem.Identifier))
                throw new ArgumentException("Project already contains item.", nameof(projectItem));

            _items.Add(projectItem.Identifier, projectItem);

            foreach (var processor in _observers)
            {
                processor.NotifyItemAdded(this, projectItem);
            }
        }

        public void RemoveItem(ProjectItem projectItem)
        {
            if (!_items.Remove(projectItem.Identifier))
                return;

            foreach (var processor in _observers)
            {
                processor.NotifyItemRemoved(this, projectItem);
            }
        }

        public ProjectItem GetItemById(ProjectItemIdentifier identifier)
        {
            return _items[identifier];
        }

        public bool TryGetItemById(ProjectItemIdentifier identifier, out ProjectItem item)
        {
            return _items.TryGetValue(identifier, out item);
        }

        public IEnumerable<ProjectItem> GetItems()
        {
            return _items.Values;
        }

        public void AddObserver(IProjectObserver observer)
        {
            _observers.Add(observer);
        }

        public void Touch(ProjectItem item)
        {
            foreach (var processor in _observers)
            {
                processor.NotifyItemChanged(this, item);
            }
        }

        public static Project FromFile(string input, IFileSystem fileSystem)
        {
            var jObject = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(input);
            var project = new Project();

            foreach (var entry in jObject)
            {
                var kindObj = entry.Key;
                string[] list;
                if (entry.Value is JArray)
                    list = entry.Value.ToObject<string[]>();
                else
                    list = new[] {entry.Value.ToObject<string>()};

                foreach (var item in list)
                {
                    var info = fileSystem.GetFileInfo(PathInfo.Create(item));
                    if (info == null)
                    {
                        var globPattern = GlobPattern.Create(item);
                        if (!globPattern.IsWildcard)
                            throw new InvalidProjectFileException($"Could not find file {item}.");

                        var items = fileSystem.GetFiles(globPattern);
                        foreach (var file in items)
                        {
                            var fileItem = new FileProjectItem(kindObj, file, fileSystem);
                            ProjectItem existing;
                            if (!project.TryGetItemById(fileItem.Identifier, out existing))
                                project.AddItem(fileItem);
                        }

                        project.AddSubscription(fileSystem.Subscribe(globPattern, a =>
                        {
                            HandleChange(a, project, kindObj, fileSystem);
                        }));
                    }
                    else
                    {
                        AddFile(fileSystem, project, kindObj, info, item);
                    }
                }
            }

            return project;
        }

        private static void AddFile(IFileSystem fileSystem, Project project, string kindObj, IFileInfo info, string item)
        {
            project.AddItem(new FileProjectItem(kindObj, info, fileSystem));

            project.AddSubscription(fileSystem.Subscribe(GlobPattern.Exact(item),
                a => { HandleChange(a, project, kindObj, fileSystem); }));
        }

        private static void HandleChange(FileChangeEventArgs a, Project project, string kindObj, IFileSystem fileSystem)
        {
            if (a.ChangeType == FileChangeType.Created)
            {
                var item = new FileProjectItem(kindObj, a.FileInfo, fileSystem);
                if (!project.Contains(item.Identifier))
                    project.AddItem(item);
            }
            else
            {
                var changedItem = project.GetItems()
                    .OfType<FileProjectItem>()
                    .FirstOrDefault(f => f.FileInfo.FilePath.Equals(a.FileInfo.FilePath));

                if (changedItem != null)
                {
                    if (a.ChangeType == FileChangeType.Changed)
                        project.Touch(changedItem);
                    else if (a.ChangeType == FileChangeType.Deleted)
                        project.RemoveItem(changedItem);
                }
            }
        }

        private bool Contains(ProjectItemIdentifier identifier)
        {
            return _items.ContainsKey(identifier);
        }

        public void AddLink(ProjectItem item1, ProjectItemLinkDescription linkDescription, ProjectItem item2)
        {
            item1.AddLinkedItem(linkDescription, item2);
            item2.AddLinkedItem(linkDescription, item1);
        }

        public void RemoveLink(ProjectItem item1, ProjectItemLinkDescription link, ProjectItem item2)
        {
            item1.RemoveLinkedItem(link, item2);
            item2.RemoveLinkedItem(link, item1);
        }

        private void AddSubscription(IDisposable disposable)
        {
            _subscriptions.Add(disposable);
        }

        ~Project()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var disposable in _subscriptions)
                    disposable.Dispose();
            }
        }
    }
}
