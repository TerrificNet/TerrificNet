using System;
using System.Collections.Generic;

namespace TerrificNet.Environment.Build
{
    public class Builder
    {
        private readonly Project _project;
        private readonly List<IBuildTarget> _targets = new List<IBuildTarget>();

        public Builder(Project project)
        {
            _project = project;
            _project.AddProcessor(new BuildProcessor(this));
            if (project == null)
                throw new ArgumentNullException(nameof(project));
        }

        public void AddTarget(IBuildTarget target)
        {
            _targets.Add(target);
            foreach (var item in _project.GetItems())
            {
                Proceed(target, item);
            }
        }

        private static void Proceed(IBuildTarget target, ProjectItem item)
        {
            if (target.DependsOn.IsMatch(item))
                target.Proceed(item);
        }

        private void Proceed(ProjectItem item)
        {
            foreach (var target in _targets)
            {
                Proceed(target, item);
            }
        }

        private class BuildProcessor : IProjectItemProcessor
        {
            private readonly Builder _builder;

            public BuildProcessor(Builder builder)
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
}
