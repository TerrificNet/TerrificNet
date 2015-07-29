using System;

namespace TerrificNet.Environment.Building
{
    public class BuildTaskResult
    {
        public ProjectItemIdentifier Identifier { get; }
        public IProjectItemContent Content { get; }

        public BuildTaskResult(ProjectItemIdentifier identifier, IProjectItemContent content)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            Identifier = identifier;
            Content = content;
        }
    }
}