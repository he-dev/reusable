using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.SmartConfig.SettingConverters.Json.Tests
{
    [TestClass]
    public class JsonSettingConverterTest
    {
        private static readonly ISettingConverter Converter = new JsonSettingConverter();

        [TestMethod]
        public void Serialize_BasicTypes_Strings()
        {
            var testValues = new (object Value, string Expected)[]
            {

            };
        }
    }
}
