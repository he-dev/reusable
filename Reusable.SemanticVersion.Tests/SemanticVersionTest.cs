using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable;

namespace Reusable.Tests
{
    [TestClass]
    public class SemanticVersionTest
    {
        [TestMethod]
        public void Parse_VariousVersions()
        {
            var semVer = SemanticVersion.Parse("1.2.3");
            Assert.AreEqual(1, semVer.Major);
            Assert.AreEqual(2, semVer.Minor);
            Assert.AreEqual(3, semVer.Patch);

            semVer = SemanticVersion.Parse("v1.2.3-beta");
            Assert.AreEqual(1, semVer.Major);
            Assert.AreEqual(2, semVer.Minor);
            Assert.AreEqual(3, semVer.Patch);
        }

#pragma warning disable 1718
        [TestMethod]
        public void Equals_VariousVersions()
        {
            Assert.IsTrue(SemanticVersion.Parse("1.0.0") == SemanticVersion.Parse("1.0.0"));
            Assert.IsFalse(SemanticVersion.Parse("1.1.0") == null);
            var semVer = SemanticVersion.Parse("1.1.1");
            Assert.IsTrue(semVer == semVer);
        }
#pragma warning restore 1718

        [TestMethod]
        public void CompareTo_LessThen()
        {
            Assert.IsTrue(SemanticVersion.Parse("1.0.0") < SemanticVersion.Parse("2.0.0"));
            Assert.IsTrue(SemanticVersion.Parse("1.1.0") < SemanticVersion.Parse("1.2.0"));
            Assert.IsTrue(SemanticVersion.Parse("1.1.1") < SemanticVersion.Parse("1.1.2"));
        }

        [TestMethod]
        public void CompareTo_Equal()
        {
            Assert.IsTrue(SemanticVersion.Parse("1.0.0") == SemanticVersion.Parse("1.0.0"));
            Assert.IsTrue(SemanticVersion.Parse("1.1.0") == SemanticVersion.Parse("1.1.0"));
            Assert.IsTrue(SemanticVersion.Parse("1.1.1") == SemanticVersion.Parse("1.1.1"));
        }

        [TestMethod]
        public void CompareTo_GreaterThen()
        {
            Assert.IsTrue(SemanticVersion.Parse("2.0.0") > SemanticVersion.Parse("1.0.0"));
            Assert.IsTrue(SemanticVersion.Parse("2.2.0") > SemanticVersion.Parse("2.1.0"));
            Assert.IsTrue(SemanticVersion.Parse("2.2.2") > SemanticVersion.Parse("2.2.1"));
        }

        [TestMethod]
        public void CompareTo_LessThenOrEqual()
        {
            Assert.IsTrue(SemanticVersion.Parse("1.0.0") <= SemanticVersion.Parse("2.0.0"));
            Assert.IsTrue(SemanticVersion.Parse("1.12.123") <= SemanticVersion.Parse("1.12.123"));
        }

        [TestMethod]
        public void CompareTo_GreaterThenOrEqual()
        {
            Assert.IsTrue(SemanticVersion.Parse("2.0.1") >= SemanticVersion.Parse("2.0.0"));
            Assert.IsTrue(SemanticVersion.Parse("2.23.234") >= SemanticVersion.Parse("2.23.234"));
        }

        [TestMethod]
        public void Sort_WithoutLabels()
        {
            var semVers = new[]
            {
                "2.1.0",
                "2.0.0",
                "2.1.1",
                "1.0.0",
            }.Select(SemanticVersion.Parse);

            var sortedSemVers = semVers.OrderBy(sv => sv).Select(x => x.ToString()).ToList();
            // 1.0.0 < 2.0.0 < 2.1.0 < 2.1.1.
            CollectionAssert.AreEqual(new[]
            {
                "1.0.0",
                "2.0.0",
                "2.1.0",
                "2.1.1",
            }, sortedSemVers);

            // Example: 1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-alpha.beta < 1.0.0-beta < 1.0.0-beta.2 < 1.0.0-beta.11 < 1.0.0-rc.1 < 1.0.0.
        }

        [TestMethod]
        public void Sort_WithLabels()
        {
            var random = new[]
            {
                "1.0.0-beta.11",
                "1.0.0-alpha.beta",
                "1.0.0-alpha.1",
                "1.0.0-rc.1",
                "1.0.0-alpha",
                "1.0.0-beta.2",
                "1.0.0-beta",
                "1.0.0",
            }.Select(SemanticVersion.Parse);

            var exptected = new[]
            {
                "1.0.0-alpha",
                "1.0.0-alpha.1",
                "1.0.0-alpha.beta",
                "1.0.0-beta",
                "1.0.0-beta.2",
                "1.0.0-beta.11",
                "1.0.0-rc.1",
                "1.0.0",
            }.Select(SemanticVersion.Parse).ToList();

            var sorted = random.OrderBy(sv => sv).ToList();
            CollectionAssert.AreEqual(exptected, sorted);

        }
    }
}
