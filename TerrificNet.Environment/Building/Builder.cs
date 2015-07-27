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
        private readonly List<IBuildTarget> _targets = new List<IBuildTarget>();
        private readonly ConcurrentDictionary<int, Task> _runningTasks = new ConcurrentDictionary<int, Task>();

        public Builder(Project project)
        {
            _project = project;
            _project.AddObserver(new BuildObserver(this));
            if (project == null)
                throw new ArgumentNullException(nameof(project));
        }

        public void AddTarget(IBuildTarget target)
        {
            _targets.Add(target);
            foreach (var item in _project.GetItems().ToList())
            {
                PushTask(Proceed(target, item));
            }
        }

        private async Task Proceed(IBuildTarget target, ProjectItem item)
        {
            if (!target.DependsOn.IsMatch(item))
                return;

            var source = target.Proceed(item);
            if (source == null)
                throw new InvalidOperationException("The proceed method has to return a value.");

            ProjectItem existing;
            if (_project.TryGetItemById(source.Identifier, out existing))
            {
                var defferred = existing as DefferedProjectItem;
                defferred?.SetDirty(source.Content, item);

                var inMemory = existing as InMemoryProjectItem;
                inMemory?.SetContent(await CopyToStream(item, source));

                _project.Touch(existing);
            }
            else
            {
                ProjectItem item2;
                if (target.Options == BuildOptions.BuildInBackground)
                {
                    item2 = await BuildInMemoryItem(item, source);
                }
                else
                    item2 = new DefferedProjectItem(source.Identifier, source.Content, item);

                _project.AddItem(item2);
                _project.AddLink(item, new ProjectItemLinkDescription(target.Name), item2);
            }
        }

        private static async Task<ProjectItem> BuildInMemoryItem(ProjectItem item, ProjectItemSource source)
        {
            var memoryStream = await CopyToStream(item, source);
            return new InMemoryProjectItem(source.Identifier, memoryStream);
        }

        private static async Task<MemoryStream> CopyToStream(ProjectItem item, ProjectItemSource source)
        {
            var memoryStream = new MemoryStream();
            var streamTask = source.Content.Transform(item);
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
        private ProjectItem _projectItem;
        private MemoryStream _contentStream;

        public DefferedProjectItem(ProjectItemIdentifier identifier, IProjectItemContent content, ProjectItem projectItem) : base(identifier)
        {
            _content = content;
            _projectItem = projectItem;
        }

        public override Stream OpenRead()
        {
            if (_contentStream == null)
            {
                // TODO: Verify async
                using (var stream = _content.Transform(_projectItem).Result)
                {
                    _contentStream = new MemoryStream();
                    stream.CopyTo(_contentStream);
                }

                _content = null;
                _projectItem = null;
            }

            _contentStream.Seek(0, SeekOrigin.Begin);
            return _contentStream;
        }

        public void SetDirty(IProjectItemContent content, ProjectItem projectItem)
        {
            _content = content;
            _projectItem = projectItem;

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
