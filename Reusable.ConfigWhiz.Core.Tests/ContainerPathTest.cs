using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Core.Tests.Data;

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class ContainerPathTest
    {
        [TestMethod]
        public void IEquatable_SameProperties_Equal()
        {
            var path1 = ContainerPath.Create<Foo, Bar>(null);
            var path2 = ContainerPath.Create<Foo, Bar>(null);
            Assert.AreEqual(path1.GetHashCode(), path2.GetHashCode());
            Assert.IsTrue(path1.Equals(path2));   
        }

        [TestMethod]
        public void IEquatable_DifferentProperties_NonEqual()
        {
        }
    }
}
