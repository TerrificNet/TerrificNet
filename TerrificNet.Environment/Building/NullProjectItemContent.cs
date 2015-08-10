using System.IO;
using System.Threading.Tasks;

namespace TerrificNet.Environment.Building
{
    public class NullProjectItemContent : IProjectItemContent
    {
        public static readonly IProjectItemContent Instance = new NullProjectItemContent();

        private NullProjectItemContent()
        {
        }

        public Task<Stream> GetContent()
        {
            return Task.FromResult<Stream>(null);
        }
    }
}