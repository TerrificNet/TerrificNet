namespace TerrificNet.Environment
{
    public class ProjectItemLink
    {
        public ProjectItemLinkDescription Description { get; }
        public ProjectItem ProjectItem { get; }

        public ProjectItemLink(ProjectItemLinkDescription description, ProjectItem projectItem)
        {
            Description = description;
            ProjectItem = projectItem;
        }
    }
}