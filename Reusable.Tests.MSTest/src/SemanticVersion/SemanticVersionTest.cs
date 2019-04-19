using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Exceptionize;
using Reusable.Utilities.MSTest;

// ReSharper disable EqualExpressionComparison

namespace Reusable.Tests
{
    [TestClass]
    public class SemanticVersionTest
    {
        [TestMethod]
        public void ctor_MajorVersionLessThanZero_VersionOutOfRangeException()
        {
            Assert.That.Throws<DynamicException>(() => new SemanticVersion(-1, 1, 1));
        }

        [TestMethod]
        public void ctor_MinorVersionLessThanZero_VersionOutOfRangeException()
        {
            Assert.That.Throws<DynamicException>(() => new SemanticVersion(1, -1, 1));
        }

        [TestMethod]
        public void ctor_PatchVersionLessThanZero_VersionOutOfRangeException()
        {
            Assert.That.Throws<DynamicException>(() => new SemanticVersion(1, 1, -1));
        }

        [TestMethod]
        public void Parse_ValidVersions_SemVer()
        {
            Assert.IsNotNull(SemanticVersion.Parse("0.0.0"));
            Assert.IsNotNull(SemanticVersion.Parse("1.0.0"));
            Assert.IsNotNull(SemanticVersion.Parse("0.1.0"));
            Assert.IsNotNull(SemanticVersion.Parse("0.0.1"));
            Assert.IsNotNull(SemanticVersion.Parse("1.0.0-label1"));
            Assert.IsNotNull(SemanticVersion.Parse("1.0.0-label1.label2"));
        }

        [TestMethod]
        public void Parse_InvalidMajorVersion_InvalidVersionException()
        {
            Assert.That.Throws<DynamicException>(() => SemanticVersion.Parse("01.0.0"), filter => filter.When(name: "VersionFormatException"));
        }

        [TestMethod]
        public void Parse_InvalidMinorVersion_InvalidVersionException()
        {
            Assert.That.Throws<DynamicException>(() => SemanticVersion.Parse("0.01.0"), filter => filter.When(name: "VersionFormatException"));
        }

        [TestMethod]
        public void Parse_InvalidPatchVersion_InvalidVersionException()
        {
            Assert.That.Throws<DynamicException>(() => SemanticVersion.Parse("0.0.01"), filter => filter.When(name: "VersionFormatException"));
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
        public void operator_Equals_Null()
        {
            Assert.IsTrue(default(SemanticVersion) == default(SemanticVersion));
        }

        [TestMethod]
        public void CompareTo_LessThan()
        {
            Assert.IsTrue(SemanticVersion.Parse("1.0.0") < SemanticVersion.Parse("2.0.0"));
            Assert.IsTrue(SemanticVersion.Parse("1.1.0") < SemanticVersion.Parse("1.22.0"));
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
        public void CompareTo_GreaterThan()
        {
            Assert.IsTrue(SemanticVersion.Parse("2.0.0") > SemanticVersion.Parse("1.0.0"));
            Assert.IsTrue(SemanticVersion.Parse("2.2.0") > SemanticVersion.Parse("2.1.0"));
            Assert.IsTrue(SemanticVersion.Parse("2.2.2") > SemanticVersion.Parse("2.2.1"));
        }

        [TestMethod]
        public void CompareTo_LessThanOrEqual()
        {
            Assert.IsTrue(SemanticVersion.Parse("1.0.0") <= SemanticVersion.Parse("2.0.0"));
            Assert.IsTrue(SemanticVersion.Parse("1.12.123") <= SemanticVersion.Parse("1.12.123"));
        }

        [TestMethod]
        public void CompareTo_GreaterThanOrEqual()
        {
            Assert.IsTrue(SemanticVersion.Parse("2.0.1") >= SemanticVersion.Parse("2.0.0"));
            Assert.IsTrue(SemanticVersion.Parse("2.23.234") >= SemanticVersion.Parse("2.23.234"));
        }

        [TestMethod]
        public void Sort_WithoutLabels()
        {
            // 1.0.0 < 2.0.0 < 2.1.0 < 2.1.1.

            var actual = new[]
            {
                "2.1.0",
                "2.0.0",
                "2.1.1",
                "1.0.0",
            }
            .Select(SemanticVersion.Parse)
            .OrderBy(x => x)
            .Select(x => x.ToString())
            .ToList();

            var expected = new[]
            {
                "1.0.0",
                "2.0.0",
                "2.1.0",
                "2.1.1",
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Sort_WithLabels()
        {
            // Example: 1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-alpha.beta < 1.0.0-beta < 1.0.0-beta.2 < 1.0.0-beta.11 < 1.0.0-rc.1 < 1.0.0.

            var actual = new[]
            {
                "1.0.0-beta.11",
                "1.0.0-alpha.beta",
                "1.0.0-alpha.1",
                "1.0.0-rc.1",
                "1.0.0-alpha",
                "1.0.0-beta.2",
                "1.0.0-beta",
                "1.0.0",
            }
            .Select(SemanticVersion.Parse)
            .OrderBy(x => x)
            .Select(x => x.ToString())
            .ToList();

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
            };

            CollectionAssert.AreEqual(exptected, actual);
        }
    }
}
