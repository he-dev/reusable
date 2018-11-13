using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.IO;
using Reusable.IO.Extensions;

namespace Reusable.Tests.IO
{
    [TestClass]
    public class RelativeFileProviderTest
    {
        [TestMethod]
        public void GetFileInfo_CanGetEmbeddedFile()
        {
            var fileProvider =
                new RelativeFileProvider(
                    new EmbeddedFileProvider(typeof(RelativeFileProviderTest).Assembly),
                    @"relative\path");

            var file = fileProvider.GetFileInfo(@"file.ext");

            Assert.IsFalse(file.Exists);
            Assert.IsTrue(SoftString.Comparer.Equals(@"Reusable\Tests\relative\path\file.ext", file.Path));
            //Assert.AreEqual("Resource1", file.ReadAllTextAsync().GetAwaiter().GetResult());
        }
    }
}
