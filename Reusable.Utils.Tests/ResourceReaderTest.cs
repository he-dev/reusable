using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Tests
{
    [TestClass]
    public class ResourceReaderTest
    {
        [TestMethod]
        public void GetEmbededResourceNames_Void_AllResourceNames()
        {
            var names = ResourceReader.GetEmbededResourceNames<ResourceReaderTest>();
            names.Count().Verify().IsEqual(2);
        }
        [TestMethod]
        public void ReadEmbededResource_ResourceName_ResourceText()
        {
            var text = ResourceReader.ReadEmbeddedResource<ResourceReaderTest, ResourceReaderTest>("TextFile1.txt");
            Assert.AreEqual("Resource1", text);
        }
    }
}
