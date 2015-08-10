using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TerrificNet.Test.Common;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.ViewEngine.Test
{
	
    [Collection("File System")]
	public class FileSystemSubscriptionTest
	{
		protected PathInfo TestFileName;
		protected const string TestFilePattern = "*.*";

        public FileSystemSubscriptionTest()
	    {
	        TestFileName = PathInfo.Create(Path.GetRandomFileName());
	    }

		[Fact]
		public async Task TestSubscription()
		{
			var fileSystem = new FileSystem(PathUtility.GetDirectory());
			fileSystem.RemoveFile(TestFileName);

			Assert.Equal(false, fileSystem.FileExists(TestFileName));

			var c = new TaskCompletionSource<FileChangeEventArgs>();
			using (fileSystem.Subscribe(GlobPattern.All, s => c.TrySetResult(s)))
			{
				using (var writer = new StreamWriter(fileSystem.OpenWrite(TestFileName)))
				{
					writer.BaseStream.SetLength(0);
					writer.Write("123456789");
				}

				var result = await c.Task.ConfigureAwait(false);
				Assert.Equal(TestFileName.ToString(), Path.GetFileName(result.FileInfo.FilePath.ToString()));
			}
		}

		[Fact]
		public async Task TestDirectorySubscription()
		{
            IFileSystem fileSystem = new FileSystem(PathUtility.GetDirectory());
			fileSystem.RemoveFile(TestFileName);

			Assert.Equal(false, fileSystem.FileExists(TestFileName));

			var c = new TaskCompletionSource<FileChangeEventArgs>();
            using (fileSystem.Subscribe(GlobPattern.All, infos =>
            {
                c.SetResult(infos);
            }))
			{
				using (var writer = new StreamWriter(fileSystem.OpenWrite(TestFileName)))
				{
					writer.BaseStream.SetLength(0);
					writer.Write("123456789");
				}

				var result = await c.Task.ConfigureAwait(false);
				Assert.Equal(TestFileName, result.FileInfo.FilePath.Name);
			}
		}
	}
}