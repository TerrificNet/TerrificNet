using System.IO;
using System.Threading.Tasks;

namespace TerrificNet.Environment.Building
{
    public interface IProjectItemContent
    {
        Task<Stream> GetContent();
    }
}