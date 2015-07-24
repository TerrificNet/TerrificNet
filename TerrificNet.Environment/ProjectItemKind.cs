using System;

namespace TerrificNet.Environment
{
    public class ProjectItemKind
    {
        public static readonly ProjectItemKind Unknown = new ProjectItemKind();

        private ProjectItemKind()
        {
        }

        public ProjectItemKind(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));

            Identifier = identifier;
        }

        public string Identifier { get; set; }
    }
}