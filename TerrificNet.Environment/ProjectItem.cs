using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Environment
{
    public class ProjectItem
    {
        private readonly List<ProjectItemLink> _links = new List<ProjectItemLink>();

        public ProjectItem() : this(ProjectItemKind.Unknown)
        {
        }

        public ProjectItem(ProjectItemKind kind)
        {
            Kind = kind;
        }

        public ProjectItemKind Kind { get; }

        public IEnumerable<ProjectItemLink> GetLinkedItems()
        {
            return _links;
        }

        internal void AddLinkedItem(ProjectItemLinkDescription linkDescription, ProjectItem linkTo)
        {
            _links.Add(new ProjectItemLink(linkDescription, linkTo));
        }

        internal void RemoveLinkedItem(ProjectItemLinkDescription description, ProjectItem item2)
        {
            var link = _links.FirstOrDefault(l => l.Description == description && l.ProjectItem == item2);
            if (link != null)
                _links.Remove(link);
        }
    }

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