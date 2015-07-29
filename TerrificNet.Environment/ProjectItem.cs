using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TerrificNet.Environment
{
    public class ProjectItem
    {
        private readonly List<ProjectItemLink> _links = new List<ProjectItemLink>();

        public ProjectItem(string identifier) : this(identifier, string.Empty)
        {
        }

        public ProjectItem(string identifier, string kind)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));

            if (kind == null)
                throw new ArgumentNullException(nameof(kind));

            Identifier = new ProjectItemIdentifier(identifier, kind);
        }

        public ProjectItem(ProjectItemIdentifier id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            Identifier = id;
        }

        public ProjectItemIdentifier Identifier { get; }

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

        public virtual Stream OpenRead()
        {
            throw new NotSupportedException();
        }
    }
}