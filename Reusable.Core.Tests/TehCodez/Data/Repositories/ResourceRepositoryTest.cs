using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data.Repositories;
using Reusable.Reflection;

namespace Reusable.Tests.Data.Repositories
{
    [TestClass]
    public class ResourceRepositoryTest
    {
        [TestMethod]
        public void GetResourceNames_Void_AllResourceNames()
        {
            var names = ResourceReader.Default.GetResourceNames();
            Assert.AreEqual(3, names.Count());
        }

        [TestMethod]
        public void FindString_ByFileName_ResourceText()
        {
            var text = ResourceReader.Default.FindString(name => name.Contains("TestFile1"));
            Assert.AreEqual("Hallo!", text);
        }
    }
}
