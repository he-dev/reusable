using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Data;

namespace Reusable.Tests.SmartConfig
{
    [TestClass]
    public class SettingNameFactoryTest
    {
        private static readonly ISettingNameFactory Factory = new SettingNameFactory(SettingNameConvention.Default);

        [TestMethod]
        public void CreateSettingNames_CanUseDefaultConvention()
        {
            Assert.AreEqual
            (
                SettingName.Parse("Type.Member"),
                Factory.CreateSettingName(SettingName.Parse("Assembly:Namespace+Type.Member"))
            );

            Assert.AreEqual
            (
                SettingName.Parse("Type.Member,Instance"),
                Factory.CreateSettingName(SettingName.Parse("Assembly:Namespace+Type.Member,Instance"))
            );
        }

        [TestMethod]
        public void CreateSettingNames_CanOverrideDefaultConvention()
        {
            Assert.AreEqual
            (
                SettingName.Parse("Namespace+Type.Member"),
                Factory.CreateSettingName(SettingName.Parse("Assembly:Namespace+Type.Member"), new SettingNameConvention(SettingNameComplexity.High))
            );

            Assert.AreEqual
            (
                SettingName.Parse("Assembly:Member,Instance"),
                Factory.CreateSettingName(SettingName.Parse("Assembly:Namespace+Type.Member,Instance"), new SettingNameConvention(SettingNameComplexity.Low, true))
            );
        }        
    }
}