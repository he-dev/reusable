using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Paths;
using Reusable.ConfigWhiz.Tests.Common;
using CData =Reusable.ConfigWhiz.Tests.Common.Data;

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class ContainerPathTest
    {
        [TestMethod]
        public void IEquatable_SameProperties_Equal()
        {
            var path1 = Identifier.Create<TestConsumer, CData.Bar>(null, NameType.Simple);
            var path2 = Identifier.Create<TestConsumer, CData.Bar>(null, NameType.Simple);
            Assert.AreEqual(path1.GetHashCode(), path2.GetHashCode());
            Assert.IsTrue(path1.Equals(path2));   
        }

        [TestMethod]
        public void IEquatable_DifferentProperties_NonEqual()
        {
        }
    }
}
