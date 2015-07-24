using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerrificNet.ViewEngine.IO;

namespace TerrificNet.Environment
{
    public class Project
    {
        private readonly HashSet<ProjectItem> _items = new HashSet<ProjectItem>();
        private readonly List<IProjectItemProcessor> _processors = new List<IProjectItemProcessor>();

        public void AddItem(ProjectItem projectItem)
        {
            if (_items.Contains(projectItem))
                throw new ArgumentException("Project already contains item.", nameof(projectItem));

            _items.Add(projectItem);

            foreach (var processor in _processors)
            {
                processor.NotifyItemAdded(this, projectItem);
            }
        }

        public void RemoveItem(ProjectItem projectItem)
        {
            _items.Remove(projectItem);

            foreach (var processor in _processors)
            {
                processor.NotifyItemRemoved(this, projectItem);
            }
        }

        public IEnumerable<ProjectItem> GetItems()
        {
            return _items;
        }

        public void AddProcessor(IProjectItemProcessor processor)
        {
            _processors.Add(processor);
        }

        public void Touch(ProjectItem item)
        {
            foreach (var processor in _processors)
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
                var kindObj = new ProjectItemKind(entry.Key);
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
                            project.AddItem(new FileProjectItem(kindObj, file));
                        }

                        fileSystem.Subscribe(globPattern, a =>
                        {
                            HandleChange(a, project, kindObj);
                        });
                    }
                    else
                    {
                        project.AddItem(new FileProjectItem(kindObj, info));

                        fileSystem.Subscribe(GlobPattern.Exact(item), a =>
                        {
                            HandleChange(a, project, kindObj);
                        });
                    }
                }
            }

            return project;
        }

        private static void HandleChange(FileChangeEventArgs a, Project project, ProjectItemKind kindObj)
        {
            if (a.ChangeType == FileChangeType.Created)
                project.AddItem(new FileProjectItem(kindObj, a.FileInfo));
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
    }
}
