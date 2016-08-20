using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TerrificNet.Environment.Building
{
    public class Builder
    {
        private readonly Project _project;
        private readonly List<IBuildTask> _tasks = new List<IBuildTask>();
        private readonly ConcurrentDictionary<int, Task> _runningTasks = new ConcurrentDictionary<int, Task>();

        public Builder(Project project)
        {
            _project = project;
            _project.AddObserver(new BuildObserver(this));
            if (project == null)
                throw new ArgumentNullException(nameof(project));
        }

        public void AddTask(IBuildTask task)
        {
            _tasks.Add(task);

            //foreach (var item in _project.GetItems().ToList())
            {
                PushTask(Proceed(task, _project.GetItems().ToList()));
            }
        }

        private async Task Proceed(IBuildTask task, IList<ProjectItem> items)
        {
            if (!task.DependsOn.IsMatch(items))
                return;

            var querySets = task.DependsOn.Select(_project.GetItems(), items).ToList();
            foreach (var querySet in querySets)
            {
                var sources = task.Proceed(querySet.GetItems());
                if (sources == null)
                    throw new InvalidOperationException("The proceed method has to return a value.");

                foreach (var source in sources)
                {
                    ProjectItem existing;
                    if (_project.TryGetItemById(source.Identifier, out existing))
                    {
                        var defferred = existing as DefferedProjectItem;
                        defferred?.SetDirty(source.Content);

                        var inMemory = existing as InMemoryProjectItem;
                        inMemory?.SetContent((await CopyToStream(source)).ToArray());

                        _project.Touch(existing);
                    }
                    else
                    {
                        ProjectItem item2;
                        if (task.Options == BuildOptions.BuildInBackground)
                        {
                            item2 = await BuildInMemoryItem(source);
                        }
                        else
                            item2 = new DefferedProjectItem(source.Identifier, source.Content);

                        _project.AddItem(item2);

                        foreach (var linkedItem in querySet.GetItems())
                            _project.AddLink(linkedItem, new ProjectItemLinkDescriptionFromBuilder(task.Name, this, linkedItem), item2);
                    }
                }
            }
        }

        private static async Task<ProjectItem> BuildInMemoryItem(BuildTaskResult source)
        {
            var memoryStream = await CopyToStream(source);
            return new InMemoryProjectItem(source.Identifier, memoryStream.ToArray());
        }

        private static async Task<MemoryStream> CopyToStream(BuildTaskResult source)
        {
            var memoryStream = new MemoryStream();
            var streamTask = source.Content.ReadAsync();
            if (streamTask != null)
            {
                using (var stream = await streamTask.ConfigureAwait(false))
                {
                    await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                }
            }
            return memoryStream;
        }

        private void Proceed(ProjectItem item)
        {
            var tasks = _tasks.Select(target => Proceed(target, new [] { item }));
            var task = Task.WhenAll(tasks);
            PushTask(task);
        }

        private void PushTask(Task task)
        {
            task.ContinueWith(t1 =>
            {
                Task t;
                _runningTasks.TryRemove(task.Id, out t);
            });
            _runningTasks.AddOrUpdate(task.Id, task, (o, n) => n);
        }

        private class ProjectItemLinkDescriptionFromBuilder : ProjectItemLinkDescription
        {
            public Builder Builder { get; }
            public ProjectItem ParentItem { get; }

            public ProjectItemLinkDescriptionFromBuilder(string name, Builder builder, ProjectItem parentItem) : base(name)
            {
                Builder = builder;
                ParentItem = parentItem;
            }
        }

        private class BuildObserver : IProjectObserver
        {
            private readonly Builder _builder;

            public BuildObserver(Builder builder)
            {
                _builder = builder;
            }

            public void NotifyItemAdded(Project underTest, ProjectItem item)
            {
                _builder.Proceed(item);
            }

            public void NotifyItemChanged(Project underTest, ProjectItem item)
            {
                _builder.Proceed(item);
            }

            public void NotifyItemRemoved(Project underTest, ProjectItem item)
            {
                _builder.ProceedRemove(item);
            }
        }

        private void ProceedRemove(ProjectItem item)
        {
            foreach (var linkedItemDescription in item.GetLinkedItems())
            {
                var builderDesc = linkedItemDescription.Description as ProjectItemLinkDescriptionFromBuilder;
                if (builderDesc != null && builderDesc.Builder == this && builderDesc.ParentItem == item)
                {
                    if (linkedItemDescription.ProjectItem.GetLinkedItems().Count() == 1)
                        _project.RemoveItem(linkedItemDescription.ProjectItem);
                    else
                        _project.Touch(item);
                }
            }
        }
    }
}
