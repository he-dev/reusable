using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.IO;
using Reusable.IO.Extensions;

namespace Reusable.Tests.IO
{
    [TestClass]
    public class InMemoryFileProviderTests
    {
        [TestMethod]
        public void FullSimulation()
        {
            //var temp = Path.GetTempPath();

            //var fileNames = new[]
            //{
            //    Path.GetRandomFileName(),
            //    Path.GetRandomFileName(),
            //    Path.GetRandomFileName(),
            //};

            var fileProvider = new InMemoryFileProvider
            {
                { @"C:\temp" },
                { @"C:\temp\foo.txt", "foo" },
                { @"C:\temp\bar.txt", "bar" },
                { @"C:\temp\sub" },
                { @"C:\temp\sub\baz.txt", "baz" },
            };

            //Assert.AreEqual(5, fileProvider.GetFileInfo(@"C:\temp").Count());
            //Assert.AreEqual(2, fileProvider.GetFileInfo(@"C:\temp\sub").Count());

            var foo = fileProvider.GetFileInfo(@"C:\temp\foo.txt");

            Assert.IsTrue(foo.Exists);
            Assert.IsFalse(foo.IsDirectory);
            Assert.AreEqual("foo", foo.ReadAllTextAsync().GetAwaiter().GetResult());

            var sub = fileProvider.GetFileInfo(@"C:\temp\sub");

            Assert.IsTrue(sub.Exists);
            Assert.IsTrue(sub.IsDirectory);
            //Assert.AreEqual(2, sub.Count());
        }
    }
}
