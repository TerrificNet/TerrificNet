using System;
using System.IO;
using System.Threading.Tasks;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.ViewEngine.Test
{
    [Collection("File System")]
    public class FileSystemTest
	{
		protected const string TestFilePattern = "*.*";
		protected IFileSystem FileSystem;

		public FileSystemTest()
		{
			FileSystem = new FileSystem();
		}

		[Fact]
		public async Task TestWrite()
		{
		    var testFileName = PathInfo.Create(Path.GetRandomFileName());
		    Assert.Equal(false, FileSystem.FileExists(testFileName));

			var completion = new TaskCompletionSource<FileChangeEventArgs>();
		    using (FileSystem.Subscribe(GlobPattern.All, info => completion.SetResult(info)))
		    {

		        using (var stream = new StreamWriter(FileSystem.OpenWrite(testFileName)))
		        {
		            stream.Write("123456");
		        }

		        await completion.Task.ConfigureAwait(false);
		        Assert.Equal(true, FileSystem.FileExists(testFileName));

		        FileSystem.RemoveFile(testFileName);
		    }
		}

		[Fact]
		public async Task TestReWrite()
		{
			var completion = new TaskCompletionSource<FileChangeEventArgs>();
		    using (FileSystem.Subscribe(GlobPattern.All, info => completion.SetResult(info)))
		    {

		        var testFileName = PathInfo.Create(Path.GetRandomFileName());
		        Assert.Equal(false, FileSystem.FileExists(testFileName));

		        using (var stream = new StreamWriter(FileSystem.OpenWrite(testFileName)))
		        {
		            stream.Write("123456");
		        }

		        await completion.Task.ConfigureAwait(false);
		        Assert.Equal(true, FileSystem.FileExists(testFileName));
		        completion = new TaskCompletionSource<FileChangeEventArgs>();

		        using (var stream = new StreamReader(FileSystem.OpenRead(testFileName)))
		        {
		            Assert.Equal("123456", stream.ReadToEnd());
		        }

		        using (var stream = new StreamWriter(FileSystem.OpenWrite(testFileName)))
		        {
		            stream.Write("654321");
		        }

		        await completion.Task.ConfigureAwait(false);
		        Assert.Equal(true, FileSystem.FileExists(testFileName));
		        completion = new TaskCompletionSource<FileChangeEventArgs>();

		        using (var stream = new StreamReader(FileSystem.OpenRead(testFileName)))
		        {
		            Assert.Equal("654321", stream.ReadToEnd());
		        }

		        FileSystem.RemoveFile(testFileName);
		        await completion.Task.ConfigureAwait(false);

		        try
		        {
		            using (var stream = new StreamReader(FileSystem.OpenRead(testFileName)))
		            {
		            }
		            Assert.True(false);
		        }
		        catch (Exception)
		        {
		        }
		    }
		}
	}
}