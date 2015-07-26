using System;

namespace TerrificNet.Environment.Build
{
    public interface IBuildTarget
    {
        BuildQuery DependsOn { get; set; }
   
        ProjectItemKind Target { get; set; }

        ProjectItem Proceed(ProjectItem item);
    }

    public enum BuildOptions
    {
        BuildOnRequest,
        BuildInBackground,
        RemoveWithSource
    }

    public abstract class BuildQuery
    {
        public static BuildQuery AllFromKind(ProjectItemKind kind)
        {
            return new BuildQueryPredicate(i => i.Kind == kind);
        }

        public static BuildQuery SingleFromKind(ProjectItemKind kind)
        {
            return new BuildQueryPredicate(i => i.Kind == kind);
        }

        private class BuildQueryPredicate : BuildQuery
        {
            private readonly Func<ProjectItem, bool> _predicate;

            public BuildQueryPredicate(Func<ProjectItem, bool> predicate)
            {
                _predicate = predicate;
            }

            public override bool IsMatch(ProjectItem item)
            {
                return _predicate(item);
            }
        }

        public abstract bool IsMatch(ProjectItem item);
    }
}