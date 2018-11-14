using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.IO;
using Reusable.IO.Extensions;

namespace Reusable.Tests.IO
{
    [TestClass]
    public class PhysicalFileProviderTest
    {
        private static readonly IFileProvider FileProvider = new PhysicalFileProvider();

        [TestMethod]
        public void CanWriteReadDelteFile()
        {
            var tempDir = Path.GetTempPath();
            var tempFile = Path.GetRandomFileName();

            var fullName = Path.Combine(tempDir, tempFile);

            FileProvider.CreateFileAsync(fullName, "Jake").GetAwaiter().GetResult();
            var file = FileProvider.GetFileInfoAsync(fullName).Result;

            Assert.IsTrue(file.Exists);
            Assert.AreEqual(fullName, file.Path);
            Assert.AreEqual("Jake", file.ReadAllTextAsync().GetAwaiter().GetResult());

            FileProvider.DeleteFileAsync(fullName).GetAwaiter().GetResult();

            file = FileProvider.GetFileInfoAsync(fullName).Result;

            Assert.IsFalse(file.Exists);
        }

       
        [TestMethod]
        public void CanCreateDeleteDirectory()
        {
            var tempDir = Path.GetTempPath();
            var randomDir = Path.GetRandomFileName();

            var fullName = Path.Combine(tempDir, randomDir);

            var newDirectoryInfo = FileProvider.CreateDirectoryAsync(fullName).Result;
            Assert.AreEqual(fullName, newDirectoryInfo.Path);

            var newDirectoryInfo2 = FileProvider.CreateDirectoryAsync(fullName).Result;
            Assert.AreEqual(fullName, newDirectoryInfo2.Path);
        }
    }
}
