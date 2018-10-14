using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.Tests.SmartConfig
{
    [TestClass]
    public class SettingNameFactoryTest
    {
        private static readonly ISettingNameFactory Factory = new SettingNameFactory();

        [TestMethod]
        public void DeriveSettingName_CanUseDifferentConventions()
        {
            Assert.AreEqual
            (
                SettingName.Parse("Type.Member"),
                Factory.CreateProviderSettingName(
                    SettingName.Parse("Assembly:Namespace+Type.Member"),
                    new SettingProviderNaming
                    {
                        Strength = SettingNameStrength.Medium,
                        PrefixHandling = PrefixHandling.Disable
                    }
                )
            );

            Assert.AreEqual
            (
                SettingName.Parse("Type.Member,Instance"),
                Factory.CreateProviderSettingName(SettingName.Parse("Assembly:Namespace+Type.Member,Instance"),
                    new SettingProviderNaming
                    {
                        Strength = SettingNameStrength.Medium,
                        PrefixHandling = PrefixHandling.Disable
                    }
                )
            );

            Assert.AreEqual
            (
                SettingName.Parse("Namespace+Type.Member"),
                Factory.CreateProviderSettingName(SettingName.Parse("Assembly:Namespace+Type.Member"),
                    new SettingProviderNaming
                    {
                        Strength = SettingNameStrength.High,
                        PrefixHandling = PrefixHandling.Enable
                    }
                )
            );

            Assert.AreEqual
            (
                SettingName.Parse("Assembly:Namespace+Type.Member,Instance"),
                Factory.CreateProviderSettingName(SettingName.Parse("Assembly:Namespace+Type.Member,Instance"),
                    new SettingProviderNaming
                    {
                        Strength = SettingNameStrength.High,
                        Prefix = "Assembly",
                        PrefixHandling = PrefixHandling.Enable
                    }
                )
            );
        }
    }
}