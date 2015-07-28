using System.Collections.Generic;

namespace TerrificNet.Environment.Building
{
    public interface IBuildTask
    {
        BuildQuery DependsOn { get; }
        BuildOptions Options { get; }
        string Name { get; }

        ProjectItemSource Proceed(IEnumerable<ProjectItem> items);
    }
}