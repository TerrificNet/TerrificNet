namespace TerrificNet.Environment
{
    public class ProjectItem
    {
        public ProjectItem() : this(ProjectItemKind.Unknown)
        {
        }

        public ProjectItem(ProjectItemKind kind)
        {
            Kind = kind;
        }

        public ProjectItemKind Kind { get; }
    }
}