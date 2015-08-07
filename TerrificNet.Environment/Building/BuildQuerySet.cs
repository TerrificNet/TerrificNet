using System.Collections.Generic;

namespace TerrificNet.Environment.Building
{
    public class BuildQuerySet
    {
        private readonly IList<ProjectItem> _items;

        public BuildQuerySet(IList<ProjectItem> items)
        {
            _items = items;
        }

        public IEnumerable<ProjectItem> GetItems()
        {
            return _items;
        }
    }
}