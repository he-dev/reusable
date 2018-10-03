using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Data;

namespace Reusable.Utilities.MSTest
{
    [TestClass]
    public class SettingNameFactoryTest
    {
        private static readonly ISettingNameFactory Factory = new SettingNameFactory();

        [TestMethod]
        public void CreateSettingNames_CanCreateBasicNamesWithoutInstance()
        {
            var names = Factory.CreateSettingNames(SettingName.Parse("Assembly:Namespace+Type.Member")).Select(x => x.ToString()).ToList();

            Assert.That.Collection().AreEqual(
                new[]
                {
                    "Member",
                    "Type.Member",
                    "Namespace+Type.Member"
                },
                names
            );
        }

        [TestMethod]
        public void CreateSettingNames_CanCreateBasicNamesWithInstance()
        {
            var names = Factory.CreateSettingNames(SettingName.Parse("Assembly:Namespace+Type.Member,Instance")).Select(x => x.ToString()).ToList();

            Assert.That.Collection().AreEqual(
                new[]
                {
                    "Member,Instance",
                    "Type.Member,Instance",
                    "Namespace+Type.Member,Instance"
                },
                names
            );
        }

        [TestMethod]
        public void CreateSettingNames_CanCreateNamesByConvention()
        {
            var names = Factory.CreateSettingNames(SettingName.Parse("Assembly:Namespace+Type.Member,Instance"), new SettingNameOption(SettingNameConvention.Normal, false)).Select(x => x.ToString()).ToList();

            Assert.That.Collection().AreEqual(
                new[]
                {
                    "Type.Member,Instance",
                },
                names
            );
        }

        [TestMethod]
        public void CreateSettingNames_CanCreateRestrictedNamesWithInstance()
        {
            var names = Factory.CreateSettingNames(SettingName.Parse("Assembly:Namespace+Type.Member,Instance"), new SettingNameOption(null, true)).Select(x => x.ToString()).ToList();

            Assert.That.Collection().AreEqual(
                new[]
                {
                    "Assembly:Member,Instance",
                    "Assembly:Type.Member,Instance",
                    "Assembly:Namespace+Type.Member,Instance"
                },
                names
            );
        }
    }
}