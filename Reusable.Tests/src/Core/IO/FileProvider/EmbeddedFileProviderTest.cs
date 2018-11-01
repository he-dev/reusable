using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.IO;
using Reusable.IO.Extensions;

namespace Reusable.Tests.IO
{
    [TestClass]
    public class EmbeddedFileProviderTest
    {
        [TestMethod]
        public void GetFileInfo_Test_txt_Text()
        {
            var fileProvider = new EmbeddedFileProvider(typeof(EmbeddedFileProviderTest).Assembly);
            var file = fileProvider.GetFileInfo(@"res\textfile1.txt");

            Assert.IsTrue(file.Exists);
            Assert.AreEqual("Resource1", file.ReadAllTextAsync().GetAwaiter().GetResult());
        }
    }
}
