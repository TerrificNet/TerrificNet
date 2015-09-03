using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Environment.Building
{
    public abstract class BuildQuery
    {
        public static BuildQuery AllFromKind(string kind)
        {
            return new BuildQueryPredicate(i => i.Identifier.Kind == kind, false);
        }

        public static BuildQuery SingleFromKind(string kind)
        {
            return new BuildQueryPredicate(i => i.Identifier.Kind == kind, true);
        }

        public static BuildQuery Exact(ProjectItemIdentifier inputItem)
        {
            return new BuildQueryPredicate(p => p.Identifier.Equals(inputItem), true);
        }

        public abstract bool IsMatch(ProjectItem item);

        public abstract bool IsMatch(IEnumerable<ProjectItem> items);

        public abstract IEnumerable<BuildQuerySet> Select(IEnumerable<ProjectItem> getItems, IEnumerable<ProjectItem> changedItems);

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

            public override bool IsMatch(IEnumerable<ProjectItem> items)
            {
                return items.Any(_predicate);
            }

            public override IEnumerable<BuildQuerySet> Select(IEnumerable<ProjectItem> getItems, IEnumerable<ProjectItem> changedItems)
            {
                if (_changedOnly)
                {
                    return changedItems.Where(IsMatch).Select(s => new BuildQuerySet(new [] { s }));
                }

                return new [] { new BuildQuerySet(getItems.Where(IsMatch).ToList()) };
            }
        }

    }
}