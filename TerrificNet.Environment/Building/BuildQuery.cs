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

        public BuildQuery Or(BuildQuery query)
        {
            return new OrBuildQuery(this, query);
        }

        private class OrBuildQuery : BuildQuery
        {
            private readonly BuildQuery _query1;
            private readonly BuildQuery _query2;

            public OrBuildQuery(BuildQuery query1, BuildQuery query2)
            {
                _query1 = query1;
                _query2 = query2;
            }

            public override bool IsMatch(ProjectItem item)
            {
                return _query1.IsMatch(item) || _query2.IsMatch(item);
            }

            public override bool IsMatch(IEnumerable<ProjectItem> items)
            {
                return _query1.IsMatch(items) || _query2.IsMatch(items);
            }

            public override IEnumerable<BuildQuerySet> Select(IEnumerable<ProjectItem> getItems, IEnumerable<ProjectItem> changedItems)
            {
                return _query1.Select(getItems, changedItems).Union(_query2.Select(getItems, changedItems));
            }
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