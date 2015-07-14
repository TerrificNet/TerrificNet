using System;
using System.IO;
using System.Reflection;

namespace TerrificNet.Test.Common
{
    public class PathUtility
    {
        public static string GetDirectory()
        {
            return GetFullFilename(string.Empty);
        }

        public static string GetFullFilename(string filename)
        {
            string executable = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(executable), filename));
        }
    }
}
