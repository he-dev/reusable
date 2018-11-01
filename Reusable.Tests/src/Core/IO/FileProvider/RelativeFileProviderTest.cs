using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.IO;
using Reusable.IO.Extensions;

namespace Reusable.Tests.IO
{
    [TestClass]
    public class RelativeFileProviderTest
    {
        [TestMethod]
        public void GetFileInfo_Test_txt_Text()
        {
            var fileProvider =
                new RelativeFileProvider(
                    new EmbeddedFileProvider(typeof(RelativeFileProviderTest).Assembly),
                    @"reusable\tests\res");

            var file = fileProvider.GetFileInfo(@"textfile1.txt");

            Assert.IsTrue(file.Exists);
            Assert.AreEqual("Resource1", file.ReadAllTextAsync().GetAwaiter().GetResult());
        }
    }
}
