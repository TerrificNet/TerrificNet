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
        private readonly List<IBuildTask> _targets = new List<IBuildTask>();
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
            _targets.Add(task);
            foreach (var item in _project.GetItems().ToList())
            {
                PushTask(Proceed(task, item));
            }
        }

        private async Task Proceed(IBuildTask task, ProjectItem item)
        {
            if (!task.DependsOn.IsMatch(item))
                return;

            var projectItems = task.DependsOn.Select(_project.GetItems(), item).ToList();
            var source = task.Proceed(projectItems);
            if (source == null)
                throw new InvalidOperationException("The proceed method has to return a value.");

            ProjectItem existing;
            if (_project.TryGetItemById(source.Identifier, out existing))
            {
                var defferred = existing as DefferedProjectItem;
                defferred?.SetDirty(source.Content);

                var inMemory = existing as InMemoryProjectItem;
                inMemory?.SetContent(await CopyToStream(source));

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

                foreach (var linkedItem in projectItems)
                    _project.AddLink(linkedItem, new ProjectItemLinkDescription(task.Name), item2);
            }
        }

        private static async Task<ProjectItem> BuildInMemoryItem(ProjectItemSource source)
        {
            var memoryStream = await CopyToStream(source);
            return new InMemoryProjectItem(source.Identifier, memoryStream);
        }

        private static async Task<MemoryStream> CopyToStream(ProjectItemSource source)
        {
            var memoryStream = new MemoryStream();
            var streamTask = source.Content.GetContent();
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
            var tasks = _targets.Select(target => Proceed(target, item));
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
            }
        }
    }

    internal class DefferedProjectItem : ProjectItem
    {
        private IProjectItemContent _content;
        private MemoryStream _contentStream;

        public DefferedProjectItem(ProjectItemIdentifier identifier, IProjectItemContent content) : base(identifier)
        {
            _content = content;
        }

        public override Stream OpenRead()
        {
            if (_contentStream == null)
            {
                // TODO: Verify async
                using (var stream = _content.GetContent().Result)
                {
                    _contentStream = new MemoryStream();
                    stream.CopyTo(_contentStream);
                }

                _content = null;
            }

            _contentStream.Seek(0, SeekOrigin.Begin);
            return _contentStream;
        }

        public void SetDirty(IProjectItemContent content)
        {
            _content = content;

            _contentStream?.Dispose();
            _contentStream = null;
        }
    }

    internal class InMemoryProjectItem : ProjectItem
    {
        private MemoryStream _content;

        public InMemoryProjectItem(ProjectItemIdentifier identifier, MemoryStream content) : base(identifier)
        {
            _content = content;
        }

        public override Stream OpenRead()
        {
            _content.Seek(0, SeekOrigin.Begin);
            return _content;
        }

        public void SetContent(MemoryStream memoryStream)
        {
            _content = memoryStream;
        }
    }
}
