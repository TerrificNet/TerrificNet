using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Environment.Building
{
    public abstract class BuildQuery
    {
        public static BuildQuery AllFromKind(ProjectItemKind kind)
        {
            return new BuildQueryPredicate(i => i.Kind == kind, false);
        }

        public static BuildQuery SingleFromKind(ProjectItemKind kind)
        {
            return new BuildQueryPredicate(i => i.Kind == kind, true);
        }

        private class BuildQueryPredicate : BuildQuery
        {
            private readonly Func<ProjectItem, bool> _predicate;
            private readonly bool _changedOnly;

            public BuildQueryPredicate(Func<ProjectItem, bool> predicate, bool changedOnly)
            {
                _predicate = predicate;
                _changedOnly = changedOnly;
            }

            public override bool IsMatch(ProjectItem item)
            {
                return _predicate(item);
            }

            public override IEnumerable<ProjectItem> Select(IEnumerable<ProjectItem> getItems, ProjectItem changedItem)
            {
                if (_changedOnly)
                {
                    if (IsMatch(changedItem))
                        return new[] {changedItem};

                    return Enumerable.Empty<ProjectItem>();
                }

                return getItems.Where(_predicate);
            }
        }

        public abstract bool IsMatch(ProjectItem item);

        public abstract IEnumerable<ProjectItem> Select(IEnumerable<ProjectItem> getItems, ProjectItem changedItem);
    }
}