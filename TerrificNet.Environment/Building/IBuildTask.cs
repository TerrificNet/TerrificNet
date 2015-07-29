using System.Collections.Generic;

namespace TerrificNet.Environment.Building
{
    public interface IBuildTask
    {
        BuildQuery DependsOn { get; }
        BuildOptions Options { get; }
        string Name { get; }

        IEnumerable<BuildTaskResult> Proceed(IEnumerable<ProjectItem> items);
    }
}