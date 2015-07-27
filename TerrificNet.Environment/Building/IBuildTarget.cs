using System;
using System.IO;
using System.Threading.Tasks;

namespace TerrificNet.Environment.Building
{
    public interface IBuildTarget
    {
        BuildQuery DependsOn { get; }
        BuildOptions Options { get; }
        string Name { get; }

        ProjectItemSource Proceed(ProjectItem item);
    }

    public class ProjectItemSource
    {
        private readonly IProjectItemContent _content;
        public ProjectItemIdentifier Identifier { get; }
        public IProjectItemContent Content { get; }

        public ProjectItemSource(ProjectItemIdentifier identifier, IProjectItemContent content)
        {
            _content = content;
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            Identifier = identifier;
            Content = content;
        }
    }

    public class NullProjectItemContent : IProjectItemContent
    {
        public static readonly IProjectItemContent Instance = new NullProjectItemContent();

        private NullProjectItemContent()
        {
        }

        public Task<Stream> Transform(ProjectItem projectItem)
        {
            return null;
        }
    }

    public class ProjectItemContentFromAction : IProjectItemContent
    {
        private readonly Func<ProjectItem, Task<Stream>> _proceedingAction;

        public ProjectItemContentFromAction(Func<ProjectItem, Task<Stream>> proceedingAction)
        {
            _proceedingAction = proceedingAction;
        }

        public Task<Stream> Transform(ProjectItem projectItem)
        {
            return _proceedingAction(projectItem);
        }
    }

    public interface IProjectItemContent
    {
        Task<Stream> Transform(ProjectItem projectItem);
    }
}