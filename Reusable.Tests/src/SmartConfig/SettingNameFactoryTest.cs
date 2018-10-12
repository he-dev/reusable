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
                Factory.DeriveSettingName(
                    SettingName.Parse("Assembly:Namespace+Type.Member"),
                    new SettingNameConvention(SettingNameComplexity.Medium, PrefixHandling.Disable),
                    null
                )
            );

            Assert.AreEqual
            (
                SettingName.Parse("Type.Member,Instance"),
                Factory.DeriveSettingName(SettingName.Parse("Assembly:Namespace+Type.Member,Instance"),
                    new SettingNameConvention(SettingNameComplexity.Medium, PrefixHandling.Disable),
                    null
                )
            );
        
            Assert.AreEqual
            (
                SettingName.Parse("Namespace+Type.Member"),
                Factory.DeriveSettingName(SettingName.Parse("Assembly:Namespace+Type.Member"),
                    new SettingNameConvention(SettingNameComplexity.High, PrefixHandling.Enable),
                    null
                )
            );

            Assert.AreEqual
            (
                SettingName.Parse("Assembly:Namespace+Type.Member,Instance"),
                Factory.DeriveSettingName(SettingName.Parse("Assembly:Namespace+Type.Member,Instance"),
                    new SettingNameConvention(SettingNameComplexity.High, PrefixHandling.Enable),
                    "Assembly"
                )
            );
        }
    }
}